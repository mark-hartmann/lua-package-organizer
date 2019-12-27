using System;
using System.Collections.Generic;
using System.IO;
using LuaPackageOrganizer.Packages;
using Newtonsoft.Json.Linq;

namespace LuaPackageOrganizer.Environments
{
    public class LocalEnvironment
    {
        private readonly string _root;


        public LupoJsonFile LupoJson { get; }
        public List<Exception> Errors { get; }

        public bool IsModified => LupoJson.IsModified;
        public bool IsCurrupted => Errors.Count != 0;

        private string LupoJsonFile => Path.Join(_root, "lupo.json");
        private string VendorDirectory => Path.Join(_root, "vendor");

        public LocalEnvironment()
        {
            Errors = new List<Exception>();
            _root = @"C:\Users\markh\IdeaProjects\LupoTestPackage"; // Directory.GetCurrentDirectory()
            ValidateProjectStructure();
            LupoJson = LuaPackageOrganizer.LupoJsonFile.ParseFile(LupoJsonFile);
        }

        public bool PackageAlreadyInstalled(Package package)
        {
            foreach (var installed in LupoJson.Packages)
            {
                if (!installed.Vendor.Equals(package.Vendor) || !installed.PackageName.Equals(package.PackageName))
                    continue;

                if (installed.Release.Name.Equals(package.Release.Name) == false)
                    throw new Exception("Package installed, but the installed version is different");

                return true;
            }

            return false;
        }

        public void MarkAsInstalled(Package package)
        {
            LupoJson.AddPackage(package);
        }

        public string GetInstallationDirectoryFor(Package package)
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
    }
}