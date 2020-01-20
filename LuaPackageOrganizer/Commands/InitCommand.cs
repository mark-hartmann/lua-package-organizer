using System;
using LuaPackageOrganizer.Commands.Options;

namespace LuaPackageOrganizer.Commands
{
    public class InitCommand
    {
        public void Execute(InitOptions options)
        {
            Console.WriteLine("Initializing project...");
        }
    }
}