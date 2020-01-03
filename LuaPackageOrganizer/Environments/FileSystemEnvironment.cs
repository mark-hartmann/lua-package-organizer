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
            return _installedPackages.Count(p => p.Key.FullName == package.FullName) != 0;
        }

        public void InstallPackage(Package package, IRepository repository)
        {
            if (repository.PackageExists(package) == false)
                throw new PackageNotFoundException(package);

            // if (repository.IsReleaseAvailable(package, package.Release) == false)
            //     throw new ReleaseNotFoundException(package);

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
            var removablePackages = new List<Package>();
            var recursiveDependencies = GetDependenciesRecursive(package);

            foreach (var dependency in recursiveDependencies)
            {
                var dependents = GetDependents(dependency);

                foreach (var dependent in dependents)
                {
                    var isAlreadyInList = removablePackages.Contains(dependency);
                    var isRequiredByProject = LupoJson.Packages.Contains(dependency);
                    var isItselfRemovableDependency = recursiveDependencies.Contains(dependent);

                    if (!isAlreadyInList && !isRequiredByProject && !isItselfRemovableDependency)
                        removablePackages.Add(dependency);
                }
            }

            return removablePackages;
        }

        private List<Package> GetDependenciesRecursive(Package package)
        {
            var dependencies = GetDependencies(package);
            var allDependencies = new List<Package>();

            foreach (var dependency in dependencies)
            {
                allDependencies.Add(dependency);
                allDependencies.AddRange(GetDependenciesRecursive(dependency));
            }

            return allDependencies.Distinct().ToList();
        }
    }
}