using System;
using LuaPackageOrganizer.Commands.Options;
using LuaPackageOrganizer.Commands.Output;
using LuaPackageOrganizer.Environments;

namespace LuaPackageOrganizer.Commands
{
    public class RemoveCommand
    {
        public void Execute(RemoveOptions options, IOutput output)
        {
            var environment = new FileSystemEnvironment(options.ProjectDirectory);

            try
            {
                // If the package was deliberately installed (using the "lupo install"-command), the package gets
                // resolved. If not, the package was never installed and the resolved package's attributes are null
                var package = environment.PackageManager.ResolveInstalled(options.Vendor, options.PackageName);

                if (package.PackageName == null)
                {
                    throw new Exception(
                        $"<package>{options.Package}</package> is not installed and therefore not removable");
                }

                environment.PackageManager.Uninstall(package);
                environment.PackageManager.ApplyChanges();

                output.WriteSuccess("Done");
            }
            catch (Exception e)
            {
                output.WriteError(e.Message);
            }
        }
    }
}