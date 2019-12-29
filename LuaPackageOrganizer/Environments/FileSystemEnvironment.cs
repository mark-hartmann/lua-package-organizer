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
            var installationQueue = new Queue<Package>();
            installationQueue.Enqueue(package);

            while (installationQueue.Count != 0)
            {
                var installingPackage = installationQueue.Dequeue();

                if (repository.PackageExists(installingPackage) == false)
                    throw new PackageNotFoundException(package);

                if (repository.IsReleaseAvailable(installingPackage, installingPackage.Release) == false)
                    throw new ReleaseNotFoundException(package);

                repository.DownloadFiles(installingPackage, GetInstallationDirectoryFor(installingPackage));

                var lupoFile = Path.Join(GetInstallationDirectoryFor(installingPackage), "lupo.json");

                if (!File.Exists(lupoFile))
                {
                    _installedPackages[installingPackage] = null;
                    continue;
                }

                _installedPackages[installingPackage] = LuaPackageOrganizer.LupoJsonFile.ParseFile(lupoFile);
                foreach (var requirement in _installedPackages[installingPackage].Packages)
                {
                    if (PackageAlreadyInstalled(requirement) == false)
                    {
                        installationQueue.Enqueue(requirement);
                        Console.WriteLine($"- {requirement} needs to be installed.");
                    }
                    else
                    {
                        Console.WriteLine($"- {requirement} already satisfied.");
                    }
                }
            }

            LupoJson.AddPackage(package);
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
            var c = _installedPackages.Count(p => p.Key.FullName == package.FullName);
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