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
                .WithParsed<InstallOptions>(opts => new InstallProcess().Execute(opts))
                .WithParsed<RemoveOptions>(opts => new RemoveProcess().Execute(opts))
                .WithNotParsed(Console.WriteLine);
        }
    }
}