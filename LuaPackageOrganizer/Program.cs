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
                .WithParsed<InitOptions>(opts => new InitCommand().Execute(opts, output))
                .WithParsed<InstallOptions>(opts => new InstallCommand().Execute(opts))
                .WithParsed<RemoveOptions>(opts => new RemoveCommand().Execute(opts))
                .WithNotParsed(Console.WriteLine);

            Console.ResetColor();
        }
    }
}