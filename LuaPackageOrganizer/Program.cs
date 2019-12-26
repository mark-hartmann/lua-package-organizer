﻿using System;
using CommandLine;
using LuaPackageOrganizer.Commands;
using LuaPackageOrganizer.Commands.Processes;

namespace LuaPackageOrganizer
{
    class Program
    {
        static void Main(string[] args)
        {
            Parser.Default.ParseArguments<InstallOptions, RemoveOptions>(args)
                .WithParsed<InstallOptions>(opts => new InstallProcess().Execute(opts))
                .WithNotParsed(Console.WriteLine);
        }
    }
}