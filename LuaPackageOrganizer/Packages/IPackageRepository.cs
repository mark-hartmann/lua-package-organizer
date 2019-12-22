using System.Collections.Generic;

namespace LuaPackageOrganizer.Packages
{
    public interface IPackageRepository
    {
        public bool PackageExists(IPackage package);
        
        public List<Release> GetAvailableReleases(IPackage package);
    }
}