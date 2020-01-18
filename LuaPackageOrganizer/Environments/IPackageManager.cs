using LuaPackageOrganizer.Packages;

namespace LuaPackageOrganizer.Environments
{
    public interface IPackageManager
    {
        public bool IsModified { get; }

        /// <summary>
        /// Checks if a given package is already installed
        /// </summary>
        /// <param name="package"></param>
        /// <param name="explicitly">If true, the search is limited to the lupo.json</param>
        /// <returns></returns>
        public bool IsInstalled(Package package, bool explicitly = false);

        public bool HasDependents(Package package);

        public void Install(Package package, IRepository repository, bool explicitly = false);

        /// <summary>
        /// Removes a package. One must be sure the package is not required by any another package as this will remove
        /// the lupo.lock (and if explicitly == true the lupo-json) entry as well as the packages local files  
        /// </summary>
        /// <param name="package"></param>
        /// <param name="explicitly">If true, the package also gets removed from the lupo.json</param>
        public void Uninstall(Package package, bool explicitly = false);

        public void ApplyChanges();

        public void RollbackChanges();

        public string InstallPath(Package package);
    }
}