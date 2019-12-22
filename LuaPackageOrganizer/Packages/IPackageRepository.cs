using System.Collections.Generic;

namespace LuaPackageOrganizer.Packages
{
    public interface IPackageRepository
    {
        public bool PackageExists(IPackage package);

        public bool IsReleaseAvailable(IPackage package, Release release);

        public List<Release> GetAvailableReleases(IPackage package);
    }
}