using CommandLine;

namespace LuaPackageOrganizer.Commands.Options
{
    public abstract class PackageOptions
    {
        public string Vendor;
        public string PackageName;

        [Value(0, MetaName = "package", HelpText = "The package", Required = true)]
        public string Package
        {
            get => Vendor + '/' + PackageName;
            set
            {
                var split = value.Split('/');
                Vendor = split[0];
                PackageName = split[1];
            }
        }

        [Option('p', "project",
            HelpText = "The path to the project directory. If not given the current directory is used ",
            Default = null)]
        public string ProjectDirectory { get; set; }
    }

    [Verb("install", HelpText = "Install a package to your project")]
    public class InstallOptions : PackageOptions
    {
        [Value(1, MetaName = "release",
            HelpText = "The release to install, the latest version is used if not specified otherwise",
            Required = false)]
        public string Release { get; set; }

        [Option("no-release", HelpText = "Uses the packages active branch (when applicable)", Default = false)]
        public bool UseActiveBranch { get; set; }
    }

    [Verb("remove", HelpText = "Remove package from your project")]
    public class RemoveOptions : PackageOptions
    {
    }
}