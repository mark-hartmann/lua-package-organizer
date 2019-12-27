using System;

namespace LuaPackageOrganizer.Packages
{
    public class PackageNotFoundException : Exception
    {
        public Package FailedPackage { get; }

        public PackageNotFoundException(Package package) : base(GenerateErrorMessage(package))
        {
            FailedPackage = package;
        }

        private static string GenerateErrorMessage(Package package)
        {
            return $"Package {package.Vendor}/{package.PackageName} does not exist, was it a typo?";
        }
    }
}