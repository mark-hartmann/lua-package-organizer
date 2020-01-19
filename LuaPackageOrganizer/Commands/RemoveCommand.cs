using System;
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
                var package = environment.PackageManager.ResolveInstalled(options.Vendor, options.PackageName);

                if (package.PackageName == null)
                {
                    throw new Exception($"{options.Package} is not installed and therefore not removable");
                }
                
                Console.Write($"Removing package {package.FullName}: ");

                environment.PackageManager.Uninstall(package);
                environment.PackageManager.ApplyChanges();

                Console.WriteLine("Done");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e);
            }
        }
    }
}