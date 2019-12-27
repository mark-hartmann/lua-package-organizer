﻿using System;
using System.Collections.Generic;
using LuaPackageOrganizer.Environments;
using LuaPackageOrganizer.Packages;
using LuaPackageOrganizer.Packages.Repositories;

namespace LuaPackageOrganizer.Commands.Processes
{
    public class InstallProcess
    {
        private readonly GithubRepository _repository;
        private readonly FileSystemEnvironment _environment;

        public InstallProcess()
        {
            _repository = new GithubRepository();
            
            // Creates a local environment (current project). Later there should be something like a "global"
            // environment to be able to access installed packages from different projects
            _environment = FileSystemEnvironment.Local();
        }

        public void Execute(InstallOptions options)
        {
            var package = Package.FromInstallOptions(options);

            try
            {
                _environment.InstallPackage(package, _repository);
            }
            catch (ReleaseNotFoundException e)
            {
                var availableReleases = _repository.GetAvailableReleases(e.FailedPackage);
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

            // If the environment was modified, the content of the lupo.json file gets overwritten with the newly
            // installed package
            if (_environment.LupoJson.IsModified)
                _environment.LupoJson.WriteChanges();

            // If the environment was modified (added folders) but failed during the installation process, the mess must
            // be cleaned and set back to the initial state
            if (_environment.LupoJson.IsModified)
            {
                // todo: Cleanup all the mess that was added during the process
                // todo: Iterate through all added packages and cleanup all created directories or so 
            }
        }
    }
}