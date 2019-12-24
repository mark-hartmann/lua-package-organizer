namespace LuaPackageOrganizer.Packages
{
    public interface IPackage
    {
        string Vendor { get; }
        string PackageName { get; }
        Release Release { get; }
    }
}