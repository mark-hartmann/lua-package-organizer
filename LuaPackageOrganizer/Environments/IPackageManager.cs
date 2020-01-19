using LuaPackageOrganizer.Packages;

namespace LuaPackageOrganizer.Environments
{
    public interface IPackageManager
    {
        /// <summary>
        /// If the environments state changed (eg. by installing or removing a package)
        /// </summary>
        public bool IsModified { get; }

        /// <summary>
        /// Checks if a given package is already installed
        /// </summary>
        /// <param name="package"></param>
        /// <param name="explicitly">If true, the search is limited to the lupo.json</param>
        /// <returns></returns>
        public bool IsInstalled(Package package, bool explicitly = false);

        /// <summary>
        /// Returns if the given package has any dependents
        /// </summary>
        /// <param name="package"></param>
        /// <returns></returns>
        public bool HasDependents(Package package);

        /// <summary>
        /// Installs a given package to the project
        /// </summary>
        /// <param name="package"></param>
        /// <param name="repository">The repository to obtain the package from</param>
        /// <param name="explicitly">If true, the package is written to the lupo.json as well</param>
        public void Install(Package package, IRepository repository, bool explicitly = false);

        /// <summary>
        /// Removes a package with all dependencies it has unless they are required by another package
        /// </summary>
        /// <param name="package"></param>
        public void Uninstall(Package package);

        /// <summary>
        /// This finishes a change in the environment, meaning if a package was installed, this method persists the
        /// lupo.lock and lupo.json file
        /// </summary>
        public void ApplyChanges();

        /// <summary>
        /// If something went wrong, this cleans up the environment
        /// </summary>
        public void RollbackChanges();

        /// <summary>
        /// Returns the installation path for the given package
        /// </summary>
        /// <example>yonaba/30log @ 30log-1.3.0-1 results in C:/.../project/vendor/yonaba/30log</example>
        /// <param name="package"></param>
        /// <returns></returns>
        public string InstallPath(Package package);
    }
}