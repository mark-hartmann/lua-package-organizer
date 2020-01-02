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
            Parser.Default.ParseArguments<InstallOptions, RemoveOptions>(args)
                .WithParsed<InstallOptions>(opts => new InstallCommand().Execute(opts))
                .WithParsed<RemoveOptions>(opts => new RemoveCommand().Execute(opts))
                .WithNotParsed(Console.WriteLine);
        }
    }
}