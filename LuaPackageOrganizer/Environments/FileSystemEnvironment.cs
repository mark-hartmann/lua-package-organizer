using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LuaPackageOrganizer.Packages;

namespace LuaPackageOrganizer.Environments
{
    public class FileSystemEnvironment
    {
        public LupoJsonFile LupoJson { get; }
        private string LupoJsonFile { get; }
        private string VendorDirectory { get; }

        private Dictionary<Package, LupoJsonFile> _installedPackages;

        private FileSystemEnvironment(string root)
        {
            LupoJsonFile = Path.Join(root, "lupo.json");
            VendorDirectory = Path.Join(root, "vendor");

            if (File.Exists(LupoJsonFile) != true)
                throw new Exception("lupo.json does not exist");

            if (Directory.Exists(VendorDirectory) != true)
                throw new Exception("vendor directory does not exist");

            LupoJson = LuaPackageOrganizer.LupoJsonFile.ParseFile(LupoJsonFile);
            _installedPackages = new Dictionary<Package, LupoJsonFile>();

            ReadInstalledPackages();
        }

        public static FileSystemEnvironment Local()
        {
            // todo: Change to Directory.GetCurrentDirectory()
            return new FileSystemEnvironment(@"C:\Users\markh\IdeaProjects\LupoTestPackage");
        }

        public bool PackageAlreadyInstalled(Package package)
        {
            var foundPackages = LupoJson.Packages.Where(p => p.FullName == package.FullName).ToList();

            if (foundPackages.Count == 0)
                return false;

            // Checks if there the passed package was previously installed with a different version. This only works if
            // the releases name is not null
            if (package.Release.Name != null && !foundPackages.Any(p => p.Release.Equals(package.Release)))
                throw new Exception($"{package.FullName} installed with a different version ({package.Release.Name})");

            return true;
        }

        public void InstallPackage(Package package, IRepository repository)
        {
            if (repository.PackageExists(package) == false)
                throw new PackageNotFoundException(package);

            if (repository.IsReleaseAvailable(package, package.Release) == false)
                throw new ReleaseNotFoundException(package);

            repository.DownloadFiles(package, GetInstallationDirectoryFor(package));

            var lupoFile = Path.Join(GetInstallationDirectoryFor(package), "lupo.json");

            _installedPackages[package] = File.Exists(lupoFile)
                ? LuaPackageOrganizer.LupoJsonFile.ParseFile(lupoFile)
                : null;
        }

        public void UninstallPackage(Package package)
        {
            Console.WriteLine($"{package} is being uninstalled");

            var installationDirectory = GetInstallationDirectoryFor(package);
            Directory.Delete(installationDirectory, true);
        }

        private string GetInstallationDirectoryFor(Package package)
        {
            return Path.Join(VendorDirectory, package.Vendor, package.PackageName);
        }

        private void ReadInstalledPackages()
        {
            foreach (var vendorDirectory in Directory.EnumerateDirectories(VendorDirectory))
            {
                foreach (var packageDirectory in Directory.EnumerateDirectories(vendorDirectory))
                {
                    // todo: This is not as fancy as i wish it was...
                    var dirInfo = new DirectoryInfo(packageDirectory);
                    var package = new Package(dirInfo.Parent?.Name, dirInfo.Name, new Release());
                    var lupoFile = Path.Join(packageDirectory, "lupo.json");

                    _installedPackages[package] = File.Exists(lupoFile)
                        ? LuaPackageOrganizer.LupoJsonFile.ParseFile(lupoFile)
                        : null;
                }
            }
        }

        public List<Package> GetDependencies(Package package)
        {
            if (_installedPackages.Count(p => p.Key.FullName.Equals(package.FullName)) == 0)
                throw new Exception($"{package} is not installed!");

            foreach (var (installedPackage, lupoJsonFile) in _installedPackages)
            {
                if (!installedPackage.FullName.Equals(package.FullName) || lupoJsonFile == null)
                    continue;

                return lupoJsonFile.Packages;
            }

            return new List<Package>();
        }

        public List<Package> GetDependents(Package package)
        {
            // Queries all installed packages requirements and returns those where the passed package is a requirement
            // Linq is awesome!
            var dependents =
                from p in _installedPackages
                where p.Value != null && (from d in p.Value.Packages select d.FullName).Contains(package.FullName)
                select p.Key;

            return dependents.ToList();
        }

        public List<Package> GetRemovableDependencies(Package package)
        {
            var dependencies = GetDependencies(package);

            return dependencies.Count == 0
                ? new List<Package>()
                : dependencies.Where(dependency => GetDependents(dependency).Count == 1).ToList();
        }
    }
}