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
        
        private FileSystemEnvironment(string root)
        {
            LupoJsonFile = Path.Join(root, "lupo.json");
            LupoLockFile = Path.Join(root, "lupo.lock");
            VendorDirectory = Path.Join(root, "vendor");

            if (File.Exists(LupoJsonFile) != true)
                throw new Exception("lupo.json does not exist");

            if (Directory.Exists(VendorDirectory) != true)
                throw new Exception("vendor directory does not exist");

            LupoJson = LuaPackageOrganizer.LupoJsonFile.ParseFile(LupoJsonFile);
            LupoLock = LuaPackageOrganizer.LupoLockFile.ParseFile(LupoLockFile);
        }

        public static FileSystemEnvironment Local()
        {
            // todo: Change to Directory.GetCurrentDirectory()
            return new FileSystemEnvironment(@"C:\Users\markh\IdeaProjects\LupoTestPackage");
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
            Console.WriteLine($"{package} is being uninstalled");
            Directory.Delete(GetInstallationDirectoryFor(package), true);
        }

        private string GetInstallationDirectoryFor(Package package)
        {
            return Path.Join(VendorDirectory, package.Vendor, package.PackageName);
        }
    }
}