using Newtonsoft.Json.Linq;

namespace LuaPackageOrganizer.Packages
{
    public class InstalledPackage : IPackage
    {
        public string Vendor { get; }
        public string PackageName { get; }
        public Release Release { get; }

        public InstalledPackage(JProperty property)
        {
            var splitted = property.Name.Split('/');

            Vendor = splitted[0];
            PackageName = splitted[1];
            Release = new Release {Name = property.Value.ToString()};
        }
    }
}