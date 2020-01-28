using System;
using System.Drawing;
using Pastel;

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
            return
                $"No such release ({package.Release.Name.Pastel(Color.CornflowerBlue)}) for {package.FullName.Pastel(Color.CornflowerBlue)}";
        }
    }
}