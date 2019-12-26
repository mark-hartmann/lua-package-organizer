using System.IO;
using System.IO.Compression;
using LuaPackageOrganizer.Environments;
using LuaPackageOrganizer.Packages;
using LuaPackageOrganizer.Packages.Repositories;

namespace LuaPackageOrganizer
{
    public static class PackageInstaller
    {
        private static void Unzip(string file, string targetDir)
        {
            // Create the package directory. If there is some error this folder must be deleted so there is no trash 
            // laying around in the project
            targetDir += Path.DirectorySeparatorChar;
            Directory.CreateDirectory(new FileInfo(targetDir).DirectoryName);

            using var archive = ZipFile.OpenRead(file);

            string replacableZipRoot = null;
            foreach (var entry in archive.Entries)
            {
                if (replacableZipRoot == null)
                {
                    replacableZipRoot = entry.FullName;
                    continue;
                }

                var newFilename = entry.FullName.Replace(replacableZipRoot, targetDir)
                    .Replace('/', Path.DirectorySeparatorChar);
                var stream = entry.Open();

                using var memStream = new MemoryStream();

                stream.CopyTo(memStream);

                // if the zip entry is a directory, the Name attribute is equal to an empty string, if Name
                // is not empty, it means it is a file
                if (entry.Name == "")
                {
                    Directory.CreateDirectory(new FileInfo(newFilename).DirectoryName);
                }
                else
                {
                    var byteArray = memStream.ToArray();
                    File.WriteAllBytes(newFilename, byteArray);
                }
            }
        }

        public static void Install(VirtualRemotePackage package, GithubRepository repository,
            LocalEnvironment environment)
        {
            var packageZipFile = repository.DownloadPackage(package);
            var installationDirectory = environment.GetInstallationDirectoryFor(package);

            Unzip(packageZipFile, installationDirectory);

            // As the file is temporary, it should be removed after extraction
            File.Delete(packageZipFile);

            environment.MarkAsInstalled(package);
            
            // todo: Take a look in the lupo.json file to check if the installed package has any requirements
        }
    }
}