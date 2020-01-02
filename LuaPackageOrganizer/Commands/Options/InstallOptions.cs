using CommandLine;

namespace LuaPackageOrganizer.Commands.Options
{
    [Verb("install", HelpText = "Install a package to your project")]
    public class InstallOptions
    {
        public string Vendor { get; set; }
        public string PackageName { get; set; }

        [Value(0, MetaName = "package", HelpText = "The package you want to install", Required = true)]
        public string Package
        {
            get => Vendor + '/' + PackageName;
            set
            {
                var splitted = value.Split('/');

                Vendor = splitted[0];
                PackageName = splitted[1];
            }
        }

        [Value(1, MetaName = "release", HelpText = "The release you want to install", Required = true)]
        public string Release { get; set; }
    }
}