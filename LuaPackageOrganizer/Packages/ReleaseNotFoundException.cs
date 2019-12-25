using System;

namespace LuaPackageOrganizer.Packages
{
    public class ReleaseNotFoundException : Exception
    {
        public ReleaseNotFoundException(string message) : base(message)
        {
        }
    }
}