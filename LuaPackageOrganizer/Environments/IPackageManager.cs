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

        public void Install(Package package, IRepository repository, bool explicitly = false);

        public void Uninstall(Package package, bool explicitly = false);

        public void ApplyChanges();

        public void RollbackChanges();

        public string InstallPath(Package package);
    }
}