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
            var environment = FileSystemEnvironment.Local();

            try
            {
                var package = environment.LupoJson.Packages.FirstOrDefault(p => p.FullName == options.Package);

                if (package.PackageName == null)
                    throw new Exception($"{options.Package} is not installed and therefore not removable");

                var removablePackages = environment.GetRemovableDependencies(package);
                if (removablePackages.Count != 0)
                {
                    Console.WriteLine($"{removablePackages.Count} no longer needed package(s) can also be uninstalled");

                    foreach (var removablePackage in removablePackages)
                    {
                        environment.UninstallPackage(removablePackage);
                    }
                }

                environment.UninstallPackage(package);

                environment.LupoJson.RemovePackage(package);
                environment.LupoJson.WriteChanges();

                Console.WriteLine("Done.");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}