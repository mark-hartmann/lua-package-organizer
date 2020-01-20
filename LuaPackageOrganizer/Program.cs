using System;
using CommandLine;
using LuaPackageOrganizer.Commands;
using LuaPackageOrganizer.Commands.Options;

namespace LuaPackageOrganizer
{
    class Program
    {
        static void Main(string[] args)
        {
            Parser.Default.ParseArguments<InitOptions, InstallOptions, RemoveOptions>(args)
                .WithParsed<InitOptions>(opts => new InitCommand().Execute(opts))
                .WithParsed<InstallOptions>(opts => new InstallCommand().Execute(opts))
                .WithParsed<RemoveOptions>(opts => new RemoveCommand().Execute(opts))
                .WithNotParsed(Console.WriteLine);
        }
    }
}