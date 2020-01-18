using System.Collections.Generic;

namespace LuaPackageOrganizer.Packages
{
    public interface IRepository
    {
        public bool HasPackage(Package package);

        public bool IsReleaseAvailable(Package package, Release release);

        public List<Package> GetDependencies(Package package);

        public List<Release> GetAvailableReleases(Package package);

        public void DownloadFiles(Package package, string targetDirectory);
    }
}