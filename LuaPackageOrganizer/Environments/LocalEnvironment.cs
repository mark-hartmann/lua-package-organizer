using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LuaPackageOrganizer.Packages;
using Newtonsoft.Json.Linq;

namespace LuaPackageOrganizer.Environments
{
    public class LocalEnvironment
    {
        private readonly string _rootDirectory;
        private readonly JObject _parsedLupoJson;
        private readonly List<IPackage> _installedPackages;

        private string VendorDirectory => Path.Join(_rootDirectory, "vendor");
        private string LupoJsonFile => Path.Join(_rootDirectory, "lupo.json");

        public LocalEnvironment()
        {
            _rootDirectory = @"C:\Users\markh\IdeaProjects\LupoTestPackage"; // Directory.GetCurrentDirectory()
            _installedPackages = new List<IPackage>();

            ValidateProjectStructure();
            _parsedLupoJson = ParseLupoJson(LupoJsonFile);
            ReadInstalledPackages();
        }

        public bool PackageAlreadyInstalled(IPackage package)
        {
            foreach (var installed in _installedPackages)
            {
                if (!installed.Vendor.Equals(package.Vendor) || !installed.PackageName.Equals(package.PackageName))
                    continue;

                if (installed.Release.Name.Equals(package.Release.Name) == false)
                    throw new Exception("Package installed, but the installed version is different");

                return true;
            }

            return false;
        }

        public void MarkAsInstalled(VirtualRemotePackage package)
        {
            _parsedLupoJson["packages"][package.Vendor + '/' + package.PackageName] = package.Release.Name;
        }

        public void WriteLupoJson()
        {
            File.WriteAllText(LupoJsonFile, _parsedLupoJson.ToString());
        }

        public string GetInstallationDirectoryFor(VirtualRemotePackage package)
        {
            return Path.Join(VendorDirectory, package.Vendor, package.PackageName);
        }

        private void ValidateProjectStructure()
        {
            if (File.Exists(LupoJsonFile) != true)
                throw new Exception("lupo.json does not exist");

            if (Directory.Exists(VendorDirectory) != true)
                throw new Exception("vendor directory does not exist");
        }

        private void ReadInstalledPackages()
        {
            var packages = _parsedLupoJson["packages"].ToList();

            foreach (var jToken in packages)
            {
                var installedPackage = new InstalledPackage((JProperty) jToken);

                // todo: Check if the package's directory exists in this environment
                _installedPackages.Add(installedPackage);
            }
        }

        private static JObject ParseLupoJson(string file)
        {
            if (File.Exists(file) != true)
                throw new Exception("lupo.json not found");

            // todo: Validate lupo.json schema
            return JObject.Parse(File.ReadAllText(file));
        }
    }
}