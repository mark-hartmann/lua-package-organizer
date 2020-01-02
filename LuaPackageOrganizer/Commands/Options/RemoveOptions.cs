using CommandLine;

namespace LuaPackageOrganizer.Commands.Options
{
    [Verb("remove", HelpText = "removes a package from your project")]
    public class RemoveOptions
    {
        public string Vendor { get; set; }
        public string PackageName { get; set; }

        [Value(0, MetaName = "package", HelpText = "The package you want to remove", Required = true)]
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
    }
}