namespace LuaPackageOrganizer.Packages
{
    public interface IPackage
    {
        string Vendor { get; set; }
        string PackageName { get; set; }
    }
}