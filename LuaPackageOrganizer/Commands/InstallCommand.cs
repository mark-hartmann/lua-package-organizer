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
        public void Execute(InstallOptions options, IOutput output)
        {
            var environment = new FileSystemEnvironment(options.ProjectDirectory);
            var repository = new GithubRepository();

            // Create package using the given options
            var package = new Package(options.Vendor, options.PackageName, new Release {Name = options.Release});

            // If the user does not provide a release to install, it must be resolved
            if (package.Release.Name == null)
            {
                output.WriteNotice("No release given, trying to find a suitable one...");

                // This is a neat one, if the user tried to install a package without release it checks if the
                // --no-release flag was set. If so, the package will use the projects active branch, otherwise
                // the latest available release will be installed

                try
                {
                    var latestRelease = repository.GetLatestRelease(package, options.UseActiveBranch);
                    package = new Package(package.Vendor, package.PackageName, latestRelease);

                    if (options.UseActiveBranch)
                    {
                        output.WriteWarning($"Found branch: <release>{package.Release}</release>",
                            $"Package is going to be installed using <package>{package.Release}</package>, this may not be a good idea!");
                    }
                    else
                    {
                        output.WriteNotice($"Found release: <release>{package.Release}</release>",
                            $"Using latest release <release>{package.Release}</release> for <package>{package.FullName}</package>");
                    }
                }
                catch (Exception e)
                {
                    // This notices the user that he passed no release for a package not having any releases and the
                    // option --no-release was not set
                    output.WriteError(e.Message);
                    return;
                }
            }

            try
            {
                // Should only throw if the user does not use --no-release
                if (!repository.IsReleaseAvailable(package, package.Release) && !options.UseActiveBranch)
                {
                    throw new ReleaseNotFoundException(package);
                }

                // Early exit installation if the package is already installed
                if (environment.PackageManager.IsInstalled(package, true))
                {
                    output.WriteNotice($"Package <package>{package.FullName}</package> is already installed");
                    return;
                }

                var packages = new List<Package>(repository.GetRequiredPackages(package)) {package};

                var installationRequired =
                    packages.Where(p => environment.PackageManager.IsInstalled(p) == false).ToList();

                var installationNotRequired =
                    packages.Where(p => installationRequired.Contains(p) == false).ToList();

                output.WriteNotice($"{packages.Count} packages will now be installed");

                foreach (var satisfied in installationNotRequired)
                    output.WriteNotice($"Package <package>{satisfied.FullName}</package> is already installed");

                output.WriteLine(null);

                // If pkg is the same as the package the user wants to install, it gets installed explicitly and
                // gets added to the lupo.json
                foreach (var pkg in installationRequired)
                {
                    var installExplicitly = pkg.Equals(package);
                    environment.PackageManager.Install(pkg, repository, installExplicitly);
                }
            }
            catch (ReleaseNotFoundException e)
            {
                var availableReleases = repository.GetAvailableReleases(e.FailedPackage);

                if (availableReleases.Count > 0)
                {
                    var releases = string.Join(", ", availableReleases.Select(p => $"<release>{p.Name}</release>"));
                    output.WriteNotice($"{e.Message}, the following releases are available:", $"[{releases}]");
                }
                else
                {
                    output.WriteError($"{e.Message}, no releases available");

                    var issuesUri = $"https://github.com/{options.Package}/issues";

                    output.WriteLine($@"
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
                output.WriteError(e.Message);
                return;
            }

            environment.PackageManager.ApplyChanges();
            output.WriteSuccess("Done");

            // If the environment was modified (added folders) but failed during the installation process, the mess must
            // be cleaned and set back to the initial state
            if (environment.PackageManager.IsModified)
            {
                // todo: Cleanup all the mess that was added during the process
                // todo: Iterate through all added packages and cleanup all created directories or so 
            }
        }
    }
}