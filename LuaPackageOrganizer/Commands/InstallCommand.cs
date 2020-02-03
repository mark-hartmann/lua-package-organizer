using System;
using System.Collections.Generic;
using System.Linq;
using LuaPackageOrganizer.Commands.Options;
using LuaPackageOrganizer.Commands.Output;
using LuaPackageOrganizer.Environments;
using LuaPackageOrganizer.Packages;
using LuaPackageOrganizer.Packages.Repositories;

namespace LuaPackageOrganizer.Commands
{
    public class InstallCommand
    {
        private readonly IOutput _output;
        private readonly InstallOptions _options;
        private readonly IRepository _repository;
        private readonly FileSystemEnvironment _environment;

        public InstallCommand(InstallOptions options, IOutput output)
        {
            _output = output;
            _options = options;
            _repository = new GithubRepository();
            _environment = new FileSystemEnvironment(options.ProjectDirectory, output);
        }

        public void Execute()
        {
            // Create package using the given options
            var package = new Package(_options.Vendor, _options.PackageName, new Release {Name = _options.Release});

            // If the user does not provide a release to install, it must be resolved
            if (package.Release.Name == null)
            {
                _output.WriteNotice("No release given, trying to find a suitable one...");

                // This is a neat one, if the user tried to install a package without release it checks if the
                // --no-release flag was set. If so, the package will use the projects active branch, otherwise
                // the latest available release will be installed

                try
                {
                    var latestRelease = _repository.GetLatestRelease(package, _options.UseActiveBranch);
                    package = new Package(package.Vendor, package.PackageName, latestRelease);

                    if (_options.UseActiveBranch)
                    {
                        _output.WriteWarning($"Found branch: <release>{package.Release}</release>",
                            $"Package is going to be installed using <package>{package.Release}</package>, this may not be a good idea!");
                    }
                    else
                    {
                        _output.WriteNotice($"Found release: <release>{package.Release}</release>",
                            $"Using latest release <release>{package.Release}</release> for <package>{package.FullName}</package>");
                    }
                }
                catch (Exception e)
                {
                    // This notices the user that he passed no release for a package not having any releases and the
                    // option --no-release was not set
                    _output.WriteError(e.Message);
                    return;
                }
            }

            try
            {
                // Should only throw if the user does not use --no-release
                if (!_repository.IsReleaseAvailable(package, package.Release) && !_options.UseActiveBranch)
                {
                    throw new ReleaseNotFoundException(package);
                }

                // Early exit installation if the package is already installed
                if (_environment.PackageManager.IsInstalled(package, true))
                {
                    _output.WriteNotice($"Package <package>{package.FullName}</package> is already installed");
                    return;
                }

                var packages = new List<Package>(_repository.GetRequiredPackages(package)) {package};

                var installationRequired =
                    packages.Where(p => _environment.PackageManager.IsInstalled(p) == false).ToList();

                var installationNotRequired =
                    packages.Where(p => installationRequired.Contains(p) == false).ToList();

                _output.WriteNotice($"{packages.Count} packages will now be installed");

                foreach (var satisfied in installationNotRequired)
                    _output.WriteNotice($"Package <package>{satisfied.FullName}</package> is already installed");

                _output.WriteLine(null);

                // If pkg is the same as the package the user wants to install, it gets installed explicitly and
                // gets added to the lupo.json
                foreach (var pkg in installationRequired)
                {
                    var installExplicitly = pkg.Equals(package);
                    _environment.PackageManager.Install(pkg, _repository, installExplicitly);
                }
            }
            catch (ReleaseNotFoundException e)
            {
                var availableReleases = _repository.GetAvailableReleases(e.FailedPackage).ToList();

                if (availableReleases.Count > 0)
                {
                    var releases = string.Join(", ", availableReleases.Select(p => $"<release>{p.Name}</release>"));
                    _output.WriteNotice($"{e.Message}, the following releases are available:", $"[{releases}]");
                }
                else
                {
                    _output.WriteError($"{e.Message}, no releases available");

                    var issuesUri = $"https://github.com/{_options.Package}/issues";

                    _output.WriteLine($@"
~ A note to <package>{package.Vendor}</package>: (._.) <- this is how some of us feel!
~
~ It is awesome when people invest their free time to develop open source projects, but especially for larger projects 
~ or libraries it's an advantage if some kind of versioning is used, this helps both the developers and the users.
~ 
~ You can create an issue and ask the content creator to provide a release by following this link:
~ <release>{issuesUri}</release>
");
                }

                return;
            }
            catch (Exception e)
            {
                _output.WriteError(e.Message);
                return;
            }

            _environment.PackageManager.ApplyChanges();
            _output.WriteSuccess("Done");

            // If the environment was modified (added folders) but failed during the installation process, the mess must
            // be cleaned and set back to the initial state
            if (_environment.PackageManager.IsModified)
            {
                // todo: Cleanup all the mess that was added during the process
                // todo: Iterate through all added packages and cleanup all created directories or so 
            }
        }
    }
}