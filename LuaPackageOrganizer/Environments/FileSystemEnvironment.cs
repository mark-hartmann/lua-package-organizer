using System;
using System.Drawing;
using System.IO;
using Pastel;

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
                Terminal.WriteNotice("Project already initialized");
                return;
            }

            Terminal.WriteNotice("Create " + "/lupo.json".Pastel(Color.Coral) + " file");
            File.Create(lupoJsonFile);

            Terminal.WriteNotice("Create " + "/lupo.lock".Pastel(Color.Coral) + " file");
            File.Create(lupoLockFile);

            Terminal.WriteNotice("Create " + "/vendor".Pastel(Color.Coral) + " directory");
            Directory.CreateDirectory(vendorDirectory);

            Terminal.WriteSuccess("Finished");
        }
    }
}