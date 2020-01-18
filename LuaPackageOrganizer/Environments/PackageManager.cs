using System;
using System.IO;
using System.Linq;
using LuaPackageOrganizer.Packages;

namespace LuaPackageOrganizer.Environments
{
    public class PackageManager : IPackageManager
    {
        private readonly LupoJsonFile _lupoJson;
        private readonly LupoLockFile _lupoLock;
        private readonly FileSystemEnvironment _env;

        public PackageManager(FileSystemEnvironment environment)
        {
            IsModified = false;

            _env = environment;
            _lupoJson = LupoJsonFile.ParseFile(environment.LupoJsonFile);
            _lupoLock = LupoLockFile.ParseFile(environment.LupoJsonFile);
        }

        public bool IsModified { get; private set; }

        public bool IsInstalled(Package package, bool explicitly = false)
        {
            // Should check if already installed but using a different release
            return explicitly
                ? _lupoJson.Packages.Contains(package)
                : _lupoLock.GetPackages().Any(p => p.FullName == package.FullName);
        }

        public void Install(Package package, IRepository repository, bool explicitly = false)
        {
            if (repository.HasPackage(package) == false)
            {
                throw new PackageNotFoundException(package);
            }

            if (explicitly)
            {
                _lupoJson.AddPackage(package);
            }

            IsModified = true;
            _lupoLock.LockPackage(package, repository);

            repository.DownloadFiles(package, InstallPath(package));
        }

        public void Uninstall(Package package, bool explicitly = false)
        {
            _lupoLock.UnlockPackage(package);
            // If the package is to be removed explicitly, the package gets removed from the lupo.json as well
            if (explicitly) _lupoJson.RemovePackage(package);

            Directory.Delete(InstallPath(package), true);
        }

        public void ApplyChanges()
        {
            if (!IsModified)
            {
                return;
            }

            _lupoJson.WriteChanges();
            _lupoLock.WriteChanges();
        }

        public void RollbackChanges()
        {
            throw new NotImplementedException();
        }

        public string InstallPath(Package package) =>
            Path.Join(_env.VendorDirectoryPath, package.Vendor, package.FullName);
    }
}