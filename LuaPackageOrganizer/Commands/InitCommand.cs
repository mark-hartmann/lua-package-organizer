using System;
using System.Drawing;
using System.IO;
using LuaPackageOrganizer.Commands.Options;
using LuaPackageOrganizer.Environments;
using Pastel;

namespace LuaPackageOrganizer.Commands
{
    public class InitCommand
    {
        public void Execute(InitOptions options)
        {
            try
            {
                if (!Directory.Exists(options.ProjectDirectory))
                {
                    throw new DirectoryNotFoundException(
                        $"Directory \"{options.ProjectDirectory.Pastel(Color.Coral)}\" does not exist.");
                }

                FileSystemEnvironment.Init(options.ProjectDirectory);
            }
            catch (Exception e)
            {
                Terminal.WriteError(e.Message, "Initialisation failed".Pastel(Color.Firebrick));
            }
        }
    }
}