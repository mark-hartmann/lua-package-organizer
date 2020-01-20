using System;
using System.IO;
using LuaPackageOrganizer.Commands.Options;
using LuaPackageOrganizer.Environments;

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
                    throw new DirectoryNotFoundException($"Directory \"{options.ProjectDirectory}\" does not exist.");
                }

                Console.WriteLine("Initialize project...");
                FileSystemEnvironment.Init(options.ProjectDirectory);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);

                Console.WriteLine();
                Console.WriteLine("Initialisation failed");
            }
        }
    }
}