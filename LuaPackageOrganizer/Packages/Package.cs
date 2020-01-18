using Newtonsoft.Json.Linq;

namespace LuaPackageOrganizer.Packages
{
    public struct Package
    {
        public string Vendor { get; }
        public string PackageName { get; }
        public Release Release { get; }
        public string FullName => Vendor + '/' + PackageName;

        public Package(string vendor, string packageName, Release release)
        {
            Vendor = vendor;
            Release = release;
            PackageName = packageName;
        }

        public static Package FromJProperty(JProperty property)
        {
            var split = property.Name.Split('/');

            return new Package(split[0], split[1], new Release {Name = property.Value.ToString()});
        }

        public override string ToString()
        {
            return $"[Package: {Vendor}/{PackageName} @ Version: {Release}]";
        }
    }
}