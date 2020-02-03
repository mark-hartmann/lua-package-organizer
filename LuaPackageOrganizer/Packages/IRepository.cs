using System.Collections.Generic;

namespace LuaPackageOrganizer.Packages
{
    public interface IRepository
    {
        public bool HasPackage(Package package);

        public bool IsReleaseAvailable(Package package, Release release);

        public IEnumerable<Package> GetDependencies(Package package);

        public IEnumerable<Release> GetAvailableReleases(Package package);

        public Release GetLatestRelease(Package package, bool useDefaultBranch);

        public IEnumerable<Package> GetRequiredPackages(Package package);

        public void DownloadFiles(Package package, string targetDirectory);
    }
}