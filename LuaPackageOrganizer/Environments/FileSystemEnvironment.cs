using System;
using System.IO;
using LuaPackageOrganizer.Packages;

namespace LuaPackageOrganizer.Environments
{
    public class FileSystemEnvironment
    {
        public readonly string LupoJsonFile;
        public readonly string LupoLockFile;
        public readonly string VendorDirectoryPath;
        public readonly IPackageManager PackageManager;

        public FileSystemEnvironment(string root)
        {
            // If root is null the current working directory is used instead
            root ??= Directory.GetCurrentDirectory();

            LupoJsonFile = Path.Join(root, "lupo.json");
            LupoLockFile = Path.Join(root, "lupo.lock");
            VendorDirectoryPath = Path.Join(root, "vendor");

            VerifyProjectDirectory();

            PackageManager = new PackageManager(this);
        }

        private void VerifyProjectDirectory()
        {
            if (!File.Exists(LupoJsonFile) || !File.Exists(LupoLockFile))
                throw new Exception("Project directory has not yet been initialized or is corrupted");
        }
    }
}