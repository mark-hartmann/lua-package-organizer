using System;
using CommandLine;
using LuaPackageOrganizer.Commands;
using LuaPackageOrganizer.Commands.Options;
using LuaPackageOrganizer.Commands.Output;

namespace LuaPackageOrganizer
{
    class Program
    {
        static void Main(string[] args)
        {
            var output = new BasicOutput();
            
            Parser.Default.ParseArguments<InitOptions, InstallOptions, RemoveOptions>(args)
                .WithParsed<InitOptions>(opts => new InitCommand(opts, output).Execute())
                .WithParsed<InstallOptions>(opts => new InstallCommand(opts, output).Execute())
                .WithParsed<RemoveOptions>(opts => new RemoveCommand(opts, output).Execute())
                .WithNotParsed(Console.WriteLine);

            Console.ResetColor();
        }
    }
}