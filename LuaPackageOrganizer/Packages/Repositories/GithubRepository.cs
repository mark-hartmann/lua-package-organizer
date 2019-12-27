using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using Newtonsoft.Json;

namespace LuaPackageOrganizer.Packages.Repositories
{
    public class GithubRepository : IRepository
    {
        private Dictionary<Package, List<Release>> _releaseCache;

        private const string GithubUserAgent = "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)";
        private const string PackageDetailsUri = "https://api.github.com/repos/{0}/{1}";
        private const string PackageReleasesUri = "https://api.github.com/repos/{0}/{1}/tags";
        private const string PackageDownloadUri = "https://github.com/{0}/{1}/zipball/{2}";

        private const string RepositoryName = "Github";

        public GithubRepository()
        {
            _releaseCache = new Dictionary<Package, List<Release>>();
        }

        public bool PackageExists(Package package)
        {
            using (var client = new WebClient())
            {
                client.Headers.Add("user-agent", GithubUserAgent);

                try
                {
                    // If this throws a WebException, the request failed (probably 404), so it could be guessed that if
                    // it does not the package exists
                    // todo: Find a better solution for this
                    client.DownloadString(string.Format(PackageDetailsUri, package.Vendor, package.PackageName));
                }
                catch (WebException e)
                {
                    return false;
                }
            }

            return true;
        }

        public bool IsReleaseAvailable(Package package, Release release)
        {
            // If the available releases were not fetched yet we do so
            // todo: If a package does not have any releases GetAvailableReleases gets called every time!
            if (_releaseCache.Count == 0 || _releaseCache.ContainsKey(package) == false)
            {
                GetAvailableReleases(package);
            }

            var availableReleases = _releaseCache[package];
            foreach (var rel in availableReleases)
            {
                if (release.Name.Equals(rel.Name))
                    return true;
            }

            return false;
        }

        public List<Release> GetAvailableReleases(Package package)
        {
            // If the available releases for this package were requested previously just return it from the release
            // cache. This saves additional http requests
            if (_releaseCache.ContainsKey(package))
            {
                return _releaseCache[package];
            }

            List<Release> releases;
            using (var client = new WebClient())
            {
                client.Headers.Add("user-agent", GithubUserAgent);
                // todo: What happens if there are no releases for this package or no package at all? 
                var jsonResponse =
                    client.DownloadString(string.Format(PackageReleasesUri, package.Vendor, package.PackageName));

                releases = JsonConvert.DeserializeObject<List<Release>>(jsonResponse);
            }

            _releaseCache.Add(package, releases);

            return releases;
        }

        public void DownloadFiles(Package package, string packageDirectory)
        {
            var tempFile = Path.GetTempFileName(); // Creates a temporary file to write the zip to
            var downloadUri = new Uri(string.Format(PackageDownloadUri, package.Vendor, package.PackageName,
                package.Release.Name));

            using var client = new WebClient();

            Console.Write($"+ Downloading {package} from {RepositoryName}: ");
            client.DownloadFileAsync(downloadUri, tempFile);

            // While the package is downloading, display a spinner to indicate that something is happening
            while (client.IsBusy)
            {
            }

            Console.WriteLine("- Done.");

            ZipExtractor.ExtractTo(tempFile, packageDirectory);
        }
    }
}