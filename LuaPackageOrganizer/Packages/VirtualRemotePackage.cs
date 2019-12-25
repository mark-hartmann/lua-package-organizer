using LuaPackageOrganizer.Commands;

namespace LuaPackageOrganizer.Packages
{
    public class VirtualRemotePackage : IPackage
    {
        public string Vendor { get; }
        public string PackageName { get; }
        public Release Release { get; }

        public VirtualRemotePackage(InstallOptions options)
        {
            Vendor = options.Vendor;
            PackageName = options.PackageName;
            Release = new Release() {Name = options.Release};
        }
    }
}