namespace LuaPackageOrganizer
{
    public interface IPackageRepository
    {
        public bool PackageExists(IPackage package);
    }
}