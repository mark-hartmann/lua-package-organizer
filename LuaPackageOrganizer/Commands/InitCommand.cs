using System;
using System.IO;
using LuaPackageOrganizer.Commands.Options;
using LuaPackageOrganizer.Commands.Output;
using LuaPackageOrganizer.Environments;

namespace LuaPackageOrganizer.Commands
{
    public class InitCommand
    {
        private readonly IOutput _output;
        private readonly InitOptions _options;

        public InitCommand(InitOptions options, IOutput output)
        {
            _output = output;
            _options = options;
        }

        public void Execute()
        {
            try
            {
                if (!Directory.Exists(_options.ProjectDirectory))
                    throw new DirectoryNotFoundException(
                        $"Directory <dir>\"{_options.ProjectDirectory}\"</dir> does not exist");

                FileSystemEnvironment.Init(_options.ProjectDirectory);
            }
            catch (Exception e)
            {
                _output.WriteError(e.Message, "Initialization failed");
            }
        }
    }
}