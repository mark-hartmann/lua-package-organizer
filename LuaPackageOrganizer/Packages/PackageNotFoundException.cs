﻿using System;

namespace LuaPackageOrganizer.Packages
{
    public class PackageNotFoundException : Exception
    {
        public IPackage FailedPackage { get; }

        public PackageNotFoundException(IPackage package) : base(GenerateErrorMessage(package))
        {
            FailedPackage = package;
        }

        private static string GenerateErrorMessage(IPackage package)
        {
            return $"Package {package.Vendor}/{package.PackageName} does not exist, was it a typo?";
        }
    }
}