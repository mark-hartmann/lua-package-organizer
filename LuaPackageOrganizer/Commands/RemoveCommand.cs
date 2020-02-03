using System;
using LuaPackageOrganizer.Commands.Options;
using LuaPackageOrganizer.Commands.Output;
using LuaPackageOrganizer.Environments;

namespace LuaPackageOrganizer.Commands
{
    public class RemoveCommand
    {
        private readonly IOutput _output;
        private readonly RemoveOptions _options;
        private readonly FileSystemEnvironment _environment;
        
        public RemoveCommand(RemoveOptions options, IOutput output)
        {
            _output = output;
            _options = options;
            _environment = new FileSystemEnvironment(options.ProjectDirectory, output);
        }
        
        public void Execute()
        {
            try
            {
                // If the package was deliberately installed (using the "lupo install"-command), the package gets
                // resolved. If not, the package was never installed and the resolved package's attributes are null
                var package = _environment.PackageManager.ResolveInstalled(_options.Vendor, _options.PackageName);

                if (package.PackageName == null)
                {
                    throw new Exception(
                        $"<package>{_options.Package}</package> is not installed and therefore not removable");
                }

                _environment.PackageManager.Uninstall(package);
                _environment.PackageManager.ApplyChanges();

                _output.WriteSuccess("Done");
            }
            catch (Exception e)
            {
                _output.WriteError(e.Message);
            }
        }
    }
}