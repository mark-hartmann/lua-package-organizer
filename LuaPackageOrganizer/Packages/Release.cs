namespace LuaPackageOrganizer.Packages
{
    public struct Release
    {
        public string Name { get; set; }

        public override string ToString() => Name;
    }
}