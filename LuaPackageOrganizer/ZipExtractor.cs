using System.IO;
using System.IO.Compression;

namespace LuaPackageOrganizer
{
    public static class ZipExtractor
    {
        public static void ExtractTo(string file, string target)
        {
            var dirSeparator = Path.DirectorySeparatorChar;

            if (target.EndsWith(dirSeparator) == false)
                target += dirSeparator;

            if (Directory.Exists(target) == false)
                Directory.CreateDirectory(new FileInfo(target).DirectoryName);

            using var archive = ZipFile.OpenRead(file);

            string replaceable = null;
            foreach (var archiveEntry in archive.Entries)
            {
                if (replaceable == null)
                {
                    replaceable = archiveEntry.FullName;
                    continue;
                }

                var stream = archiveEntry.Open();
                var filename = archiveEntry.FullName.Replace(replaceable, target);

                using var memStream = new MemoryStream();

                stream.CopyTo(memStream);

                // if the zip entry is a directory, the Name attribute is equal to an empty string, if Name
                // is not empty, it means it is a file
                if (archiveEntry.Name == "")
                    Directory.CreateDirectory(new FileInfo(filename).DirectoryName);
                else
                    File.WriteAllBytes(filename, memStream.ToArray());
            }
        }
    }
}