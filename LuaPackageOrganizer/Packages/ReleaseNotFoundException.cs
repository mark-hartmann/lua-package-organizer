using System;

namespace LuaPackageOrganizer.Packages
{
    public class ReleaseNotFoundException : Exception
    {
        public IPackage FailedPackage { get; }

        public ReleaseNotFoundException(IPackage package) : base(GenerateErrorMessage(package))
        {
            FailedPackage = package;
        }

        private static string GenerateErrorMessage(IPackage package)
        {
            return $"No such release ({package.Release}) for {package.Vendor}{package.PackageName}";
        }
    }
}