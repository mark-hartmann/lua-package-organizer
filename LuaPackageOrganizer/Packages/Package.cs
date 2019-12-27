using LuaPackageOrganizer.Commands;
using Newtonsoft.Json.Linq;

namespace LuaPackageOrganizer.Packages
{
    public class Package
    {
        public string Vendor { get; private set; }
        public string PackageName { get; private set; }
        public Release Release { get; private set; }
        public string FullName => Vendor + '/' + PackageName;

        private Package()
        {
        }

        public static Package FromJProperty(JProperty property)
        {
            var splitted = property.Name.Split('/');
            var package = new Package
            {
                Vendor = splitted[0],
                PackageName = splitted[1],
                Release = new Release
                {
                    Name = property.Value.ToString()
                }
            };

            return package;
        }

        public static Package FromInstallOptions(InstallOptions options)
        {
            var package = new Package
            {
                Vendor = options.Vendor,
                PackageName = options.PackageName,
                Release = new Release
                {
                    Name = options.Release
                }
            };

            return package;
        }

        public override string ToString()
        {
            return $"[Package: {Vendor}/{PackageName} @ Version: {Release.Name}]";
        }
    }
}