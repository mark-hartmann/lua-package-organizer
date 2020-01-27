using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using LuaPackageOrganizer.Commands.Options;
using LuaPackageOrganizer.Environments;
using LuaPackageOrganizer.Packages;
using LuaPackageOrganizer.Packages.Repositories;
using Pastel;

namespace LuaPackageOrganizer.Commands
{
    public class InstallCommand
    {
        private GithubRepository _repository;
        private FileSystemEnvironment _environment;

        public InstallCommand()
        {
            _repository = new GithubRepository();
        }

        public void Execute(InstallOptions options)
        {
            _environment = new FileSystemEnvironment(options.ProjectDirectory);

            var package = new Package(options.Vendor, options.PackageName, new Release {Name = options.Release});

            if (package.Release.Name == null)
            {
                // This is a neat one, if the user tried to install a package without release it checks if the
                // --no-release flag was set. If so, the package will use the projects active branch, otherwise
                // the latest available release will be installed 
                var latestRelease = _repository.GetLatestRelease(package, options.UseActiveBranch);
                package = new Package(package.Vendor, package.PackageName, latestRelease);

                var colorizedPackageName = package.FullName.Pastel(Color.CornflowerBlue);
                var colorizedReleaseName = package.Release.Name.Pastel(Color.CornflowerBlue);

                Terminal.WriteNotice(
                    options.UseActiveBranch
                        ? $"Warning: {colorizedPackageName} will use {colorizedReleaseName}, this may not be a good idea!"
                        : $"Using latest release ({colorizedReleaseName}) for {colorizedPackageName}");
            }

            try
            {
                if (_environment.PackageManager.IsInstalled(package, true) == false)
                {
                    // todo: Check if the dependencies are broken (Same package w/ different Releases) 
                    var packages = new List<Package>(_repository.GetRequiredPackages(package)) {package};

                    var installationRequired =
                        packages.Where(p => _environment.PackageManager.IsInstalled(p) == false).ToList();

                    var installationNotRequired =
                        packages.Where(p => installationRequired.Contains(p) == false).ToList();

                    Terminal.WriteNotice($"{packages.Count} packages will now be installed");
                    foreach (var satisfied in installationNotRequired)
                    {
                        Terminal.WriteNotice($"{satisfied.FullName.Pastel(Color.CornflowerBlue)} is already satisfied");
                    }

                    Console.WriteLine();
                    
                    // If pkg is the same as the package the user wants to install, it gets installed explicitly and
                    // gets added to the lupo.json
                    foreach (var pkg in installationRequired)
                    {
                        var installExplicitly = pkg.Equals(package);
                        _environment.PackageManager.Install(pkg, _repository, installExplicitly);
                    }
                }
                else
                {
                    Terminal.WriteNotice($"{package.FullName.Pastel(Color.CornflowerBlue)} is already installed");
                }
            }
            catch (ReleaseNotFoundException e)
            {
                var availableReleases = _repository.GetAvailableReleases(e.FailedPackage);
                Terminal.WriteNotice(e.Message + ", " + (availableReleases.Count == 0
                                      ? "no releases available."
                                      : "the following releases are available:"));

                if (availableReleases.Count > 0)
                {
                    Console.Write('[');
                    for (var i = 0; i < availableReleases.Count; i++)
                    {
                        Console.Write(availableReleases[i].Name);
                        if (i < availableReleases.Count - 1)
                        {
                            Console.Write(", ");
                        }
                    }

                    Console.Write(']');
                }
                else
                {
                    var issuesUri = $"https://github.com/{options.Package}/issues";
                    var lectures = $@"
[Comment on missing releases]
------------------------------------------------------------------------------------------------------------------------
1. A note to {package.Vendor}: (._.) <- this is how you should feel!

2. This is unfortunately a common phenomenon in the 'amateurish' Lua/Love2D community. 
   It is awesome when people invest their free time to develop open source projects, but especially for larger projects 
   or libraries it's an advantage if some kind of versioning is used, this helps both the developers and the users.

   Perhaps today would be a good day to ask for a favor. 
   You can create an issue and ask the content creator to provide a release by following this link:
   {issuesUri}
------------------------------------------------------------------------------------------------------------------------";
                    Console.WriteLine(lectures);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            _environment.PackageManager.ApplyChanges();

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