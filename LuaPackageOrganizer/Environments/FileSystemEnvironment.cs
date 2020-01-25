using System;
using System.Drawing;
using System.IO;
using Colorful;

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

            // Highlighting the file/directory looks cool, especially in coral!
            var stylesheet = new StyleSheet(Color.LightGray);
            stylesheet.AddStyle("./[a-zA-Z.]+", Color.Coral);

            Terminal.WriteLine(Terminal.MessageType.Notice, "Create ./lupo.json file", stylesheet);
            File.Create(lupoJsonFile);

            Terminal.WriteLine(Terminal.MessageType.Notice, "Create ./lupo.lock file", stylesheet);
            File.Create(lupoLockFile);

            Terminal.WriteLine(Terminal.MessageType.Notice, "Create ./vendor directory", stylesheet);
            Directory.CreateDirectory(vendorDirectory);

            Terminal.WriteSuccess("Finished");
        }
    }
}