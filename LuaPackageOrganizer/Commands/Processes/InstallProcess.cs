using System;
using LuaPackageOrganizer.Environments;
using LuaPackageOrganizer.Packages;
using LuaPackageOrganizer.Packages.Repositories;

namespace LuaPackageOrganizer.Commands.Processes
{
    public class InstallProcess
    {
        public void Execute(InstallOptions options)
        {
            var repository = new GithubRepository();
            var environment = new LocalEnvironment();
            var package = new VirtualRemotePackage(options);

            try
            {
                if (repository.PackageExists(package) == false) 
                    throw new PackageNotFoundException(package);

                if (repository.IsReleaseAvailable(package, package.Release) == false)
                    throw new ReleaseNotFoundException(package);

                PackageInstaller.Install(package, repository, environment);
                environment.WriteLupoJson();
            }
            catch (ReleaseNotFoundException e)
            {
                var availableReleases = repository.GetAvailableReleases(package);
                Console.WriteLine(e.Message + ", " + (availableReleases.Count == 0
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
This is unfortunately a more common phenomenon in the Lua/Love2D community. 
It's great when users provide open source libraries, but especially for larger projects or libraries it's an advantage 
if some kind of versioning is used, this helps both the developers and the users.

Perhaps today would be a good day to ask for a favor.
You can create an issue and ask the content creator to provide a release by following this link:
{issuesUri}
------------------------------------------------------------------------------------------------------------------------";
                    Console.WriteLine(lectures);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
}