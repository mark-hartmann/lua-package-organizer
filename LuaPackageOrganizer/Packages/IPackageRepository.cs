namespace LuaPackageOrganizer.Packages
{
    public interface IPackageRepository
    {
        public bool PackageExists(IPackage package);
    }
}