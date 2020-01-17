using System;
using System.IO;
using System.Linq;
using LuaPackageOrganizer.Packages;

namespace LuaPackageOrganizer.Environments
{
    public class FileSystemEnvironment
    {
        public LupoJsonFile LupoJson { get; }
        public LupoLockFile LupoLock { get; }
        private string LupoJsonFile { get; }
        private string LupoLockFile { get; }
        private string VendorDirectory { get; }

        public FileSystemEnvironment(string root)
        {
            // If root is null the current working directory is used instead
            if (root == null)
                root = Directory.GetCurrentDirectory();

            LupoJsonFile = Path.Join(root, "lupo.json");
            LupoLockFile = Path.Join(root, "lupo.lock");
            VendorDirectory = Path.Join(root, "vendor");

            VerifyProjectDirectory();

            LupoJson = LuaPackageOrganizer.LupoJsonFile.ParseFile(LupoJsonFile);

            if (File.Exists(LupoLockFile))
                LupoLock = LuaPackageOrganizer.LupoLockFile.ParseFile(LupoLockFile);
        }

        private void VerifyProjectDirectory()
        {
            if (!File.Exists(LupoJsonFile))
                throw new Exception("Project directory has not yet been initialized or is corrupted");
        }

        public bool PackageAlreadyInstalled(Package package)
        {
            return LupoLock.GetPackages().Count(p => p.FullName == package.FullName) != 0;
        }

        public void InstallPackage(Package package, IRepository repository)
        {
            if (repository.PackageExists(package) == false)
                throw new PackageNotFoundException(package);

            // if (repository.IsReleaseAvailable(package, package.Release) == false)
            //     throw new ReleaseNotFoundException(package);

            LupoLock.LockPackage(package, repository);
            repository.DownloadFiles(package, GetInstallationDirectoryFor(package));
        }

        public void UninstallPackage(Package package)
        {
            Directory.Delete(GetInstallationDirectoryFor(package), true);
        }

        private string GetInstallationDirectoryFor(Package package)
        {
            return Path.Join(VendorDirectory, package.Vendor, package.PackageName);
        }
    }
}