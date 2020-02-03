using System;
using System.IO;
using LuaPackageOrganizer.Commands.Options;
using LuaPackageOrganizer.Commands.Output;
using LuaPackageOrganizer.Environments;

namespace LuaPackageOrganizer.Commands
{
    public class InitCommand
    {
        public void Execute(InitOptions options, IOutput output)
        {
            try
            {
                if (!Directory.Exists(options.ProjectDirectory))
                {
                    throw new DirectoryNotFoundException(
                        $"Directory <dir>\"{options.ProjectDirectory}\"</dir> does not exist");
                }

                FileSystemEnvironment.Init(options.ProjectDirectory);
            }
            catch (Exception e)
            {
                output.WriteError(e.Message, "Initialization failed");
            }
        }
    }
}