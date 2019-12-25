using System;

namespace LuaPackageOrganizer.Packages
{
    public class PackageNotFoundException : Exception
    {
        public PackageNotFoundException(string message) : base(message)
        {
        }
    }
}