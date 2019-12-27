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

        private FileSystemEnvironment(string root)
        {
            LupoJsonFile = Path.Join(root, "lupo.json");
            VendorDirectory = Path.Join(root, "vendor");

            if (File.Exists(LupoJsonFile) != true)
                throw new Exception("lupo.json does not exist");

            if (Directory.Exists(VendorDirectory) != true)
                throw new Exception("vendor directory does not exist");

            LupoJson = LuaPackageOrganizer.LupoJsonFile.ParseFile(LupoJsonFile);
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

            if (foundPackages.Any(p => p.Release.Equals(package.Release) == false))
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
                    continue;

                foreach (var requirement in LuaPackageOrganizer.LupoJsonFile.ParseFile(lupoFile).Packages)
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
    }
}