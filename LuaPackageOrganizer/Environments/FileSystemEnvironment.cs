using System;
using System.Drawing;
using System.IO;
using LuaPackageOrganizer.Commands.Output;
using Pastel;

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
                Terminal.WriteNotice("Project already initialized");
                return;
            }

            var lupoFileContent = @"{
  ""packages"": {}
}";
            
            Terminal.WriteNotice("Writing " + "/lupo.json".Pastel(Color.Coral) + " file");
            File.WriteAllText(lupoJsonFile, lupoFileContent);

            Terminal.WriteNotice("Writing " + "/lupo.lock".Pastel(Color.Coral) + " file");
            File.WriteAllText(lupoLockFile, lupoFileContent);

            Terminal.WriteNotice("Writing " + "/vendor".Pastel(Color.Coral) + " directory");
            Directory.CreateDirectory(vendorDirectory);

            Terminal.WriteSuccess("Finished");
        }
    }
}