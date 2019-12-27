using System.Collections.Generic;

namespace LuaPackageOrganizer.Packages
{
    public interface IPackageRepository
    {
        public bool PackageExists(Package package);

        public bool IsReleaseAvailable(Package package, Release release);

        public List<Release> GetAvailableReleases(Package package);
    }
}