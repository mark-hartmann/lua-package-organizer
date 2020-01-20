using CommandLine;

namespace LuaPackageOrganizer.Commands.Options
{
    [Verb("init", HelpText = "Initialize a new project environment")]
    public class InitOptions
    {
        [Option('p', "project", HelpText = "The path where the project is going to be", Default = null)]
        public string ProjectDirectory { get; set; }
    }
}