using System;

namespace LuaPackageOrganizer.Packages
{
    public class ReleaseNotFoundException : Exception
    {
        public Package FailedPackage { get; }

        public ReleaseNotFoundException(Package package) : base(GenerateErrorMessage(package))
        {
            FailedPackage = package;
        }

        private static string GenerateErrorMessage(Package package)
        {
            return $"No such release ({package.Release}) for {package.Vendor}{package.PackageName}";
        }
    }
}