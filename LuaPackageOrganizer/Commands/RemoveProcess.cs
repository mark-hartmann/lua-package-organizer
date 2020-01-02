using System;
using System.Linq;
using LuaPackageOrganizer.Commands.Options;
using LuaPackageOrganizer.Environments;
using LuaPackageOrganizer.Packages;

namespace LuaPackageOrganizer.Commands
{
    public class RemoveProcess
    {
        public void Execute(RemoveOptions options)
        {
            var package = new Package(options.Vendor, options.PackageName, new Release());
            var environment = FileSystemEnvironment.Local();

            if (!environment.PackageAlreadyInstalled(package))
            {
                Console.WriteLine("Nothing can be removed that is not there");
                return;
            }

            var removablePackages = environment.GetRemovableDependencies(package);
            if (removablePackages.Count != 0)
            {
                Console.WriteLine($"{removablePackages.Count} no longer needed package(s) can also be uninstalled.");

                foreach (var removablePackage in removablePackages)
                {
                    environment.UninstallPackage(removablePackage);
                }
            }

            environment.LupoJson.RemovePackage(
                environment.LupoJson.Packages.First(p => p.FullName == package.FullName));
            
            environment.UninstallPackage(package);
            environment.LupoJson.WriteChanges();

            Console.WriteLine("Done.");
        }
    }
}