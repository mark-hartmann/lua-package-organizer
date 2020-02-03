using System;
using System.IO;
using LuaPackageOrganizer.Commands.Output;

namespace LuaPackageOrganizer.Environments
{
    public class FileSystemEnvironment
    {
        public readonly string LupoJsonFile;
        public readonly string LupoLockFile;
        public readonly string VendorDirectoryPath;
        public readonly IPackageManager PackageManager;

        public FileSystemEnvironment(string root, IOutput output)
        {
            // If root is null the current working directory is used instead
            root ??= Directory.GetCurrentDirectory();

            LupoJsonFile = Path.Join(root, "lupo.json");
            LupoLockFile = Path.Join(root, "lupo.lock");
            VendorDirectoryPath = Path.Join(root, "vendor");

            VerifyProjectDirectory();

            PackageManager = new PackageManager(this, output);
        }

        private void VerifyProjectDirectory()
        {
            if (!File.Exists(LupoJsonFile) || !File.Exists(LupoLockFile))
                throw new Exception(
                    "Project directory has not yet been initialized or is corrupted, maybe you forgot to run \"lupo init\"?");
        }

        public static void Init(string path)
        {
            var lupoJsonFile = Path.Join(path, "lupo.json");
            var lupoLockFile = Path.Join(path, "lupo.lock");
            var vendorDirectory = Path.Join(path, "vendor");

            if (File.Exists(lupoJsonFile))
            {
                throw new Exception("Project already initialized");
            }

            var lupoFileContent = @"{
  ""packages"": {}
}";

            File.WriteAllText(lupoJsonFile, lupoFileContent);
            File.WriteAllText(lupoLockFile, lupoFileContent);
            Directory.CreateDirectory(vendorDirectory);
        }
    }
}