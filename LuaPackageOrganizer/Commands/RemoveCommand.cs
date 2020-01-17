using System;
using System.Linq;
using LuaPackageOrganizer.Commands.Options;
using LuaPackageOrganizer.Environments;

namespace LuaPackageOrganizer.Commands
{
    public class RemoveCommand
    {
        public void Execute(RemoveOptions options)
        {
            var environment = new FileSystemEnvironment(options.ProjectDirectory);

            try
            {
                // If the package was deliberately installed (using the "lupo install"-command), the package gets
                // resolved. If not, the package was never installed and the resolved package's attributes are null
                var package = environment.LupoJson.Packages.FirstOrDefault(p => p.FullName == options.Package);

                if (package.PackageName == null)
                    throw new Exception($"{options.Package} is not installed and therefore not removable");

                // Resolves all packages related to the package that can be removed (packages that do not have a
                // dependent not related to the package). This includes dependencies, and each dependency's dependencies
                // and so on.
                var removablePackages = environment.LupoLock.GetRemovableDependencies(package);

                // If the package does not have any dependencies but is one itself, it gets removed from the lupo.json
                // and remains in the lupo.lock file 
                var hasDependents = environment.LupoLock.GetDependents(package).Any();
                var hasRemovablePackages = removablePackages.Count != 0;

                if (hasRemovablePackages)
                {
                    Console.WriteLine($"{removablePackages.Count} no longer needed package(s) can also be uninstalled");

                    foreach (var removablePackage in removablePackages)
                    {
                        // If the dependency is also a required by this project, skip
                        if (environment.LupoJson.Packages.Contains(removablePackage))
                            continue;

                        Console.Write($"Removing dependency {removablePackage.FullName}: ");

                        environment.UninstallPackage(removablePackage);
                        environment.LupoLock.UnlockPackage(removablePackage);

                        Console.WriteLine("Done");
                    }
                }

                if (hasDependents == false)
                {
                    environment.UninstallPackage(package);
                    environment.LupoLock.UnlockPackage(package);
                }

                Console.Write($"Removing package {package.FullName}: ");

                environment.LupoJson.RemovePackage(package);

                environment.LupoJson.WriteChanges();
                environment.LupoLock.WriteChanges();

                Console.WriteLine("Done");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}