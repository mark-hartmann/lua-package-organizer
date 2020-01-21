using System;
using System.IO;

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

            Console.WriteLine("Creating lupo.json");
            File.Create(lupoJsonFile);

            Console.WriteLine("Creating lupo.lock");
            File.Create(lupoLockFile);

            Console.WriteLine("Creating vendor directory");
            Directory.CreateDirectory(vendorDirectory);

            Console.WriteLine();
            Console.WriteLine("Done");
        }
    }
}