using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace LuaPackageOrganizer.Packages.Repositories
{
    public class GithubRepository : IRepository
    {
        private readonly Dictionary<Package, List<Release>> _releaseCache;

        private const string GithubUserAgent = "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)";
        private const string PackageDetailsUri = "https://api.github.com/repos/{0}/{1}";
        private const string PackageReleasesUri = "https://api.github.com/repos/{0}/{1}/tags";
        private const string PackageDownloadUri = "https://github.com/{0}/{1}/zipball/{2}";
        private const string PackageDependenciesUri = "https://raw.githubusercontent.com/{0}/{1}/{2}/lupo.json";

        public GithubRepository()
        {
            _releaseCache = new Dictionary<Package, List<Release>>();
        }

        public bool PackageExists(Package package)
        {
            using (var client = CreateWebClient())
            {
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
            using (var client = CreateWebClient())
            {
                // todo: What happens if there are no releases for this package or no package at all? 
                var jsonResponse =
                    client.DownloadString(string.Format(PackageReleasesUri, package.Vendor, package.PackageName));

                releases = JsonConvert.DeserializeObject<List<Release>>(jsonResponse);
            }

            _releaseCache.Add(package, releases);

            return releases;
        }

        public List<Package> GetRequiredPackages(Package package)
        {
            var requirements = new List<Package>();
            var totalRequirements = new List<Package>();

            try
            {
                using var client = CreateWebClient();

                var uri = string.Format(PackageDependenciesUri, package.Vendor, package.PackageName,
                    package.Release.Name);
                var response = client.DownloadString(uri);

                foreach (var jToken in JObject.Parse(response)["packages"].ToList())
                    requirements.Add(Package.FromJProperty((JProperty) jToken));

                totalRequirements.AddRange(requirements);
                foreach (var requirement in requirements)
                {
                    totalRequirements.AddRange(GetRequiredPackages(requirement));
                }

                return totalRequirements;
            }
            catch (Exception)
            {
                return requirements;
            }
        }

        public List<Package> GetDependencies(Package package)
        {
            var dependencies = new List<Package>();

            try
            {
                using var client = CreateWebClient();

                var uri = string.Format(PackageDependenciesUri, package.Vendor, package.PackageName,
                    package.Release.Name);
                var response = client.DownloadString(uri);

                foreach (var jToken in JObject.Parse(response)["packages"].ToList())
                    dependencies.Add(Package.FromJProperty((JProperty) jToken));

                return dependencies;
            }
            catch (Exception)
            {
                return dependencies;
            }
        }

        public void DownloadFiles(Package package, string packageDirectory)
        {
            var tempFile = Path.GetTempFileName(); // Creates a temporary file to write the zip to
            var downloadUri = new Uri(string.Format(PackageDownloadUri, package.Vendor, package.PackageName,
                package.Release.Name));

            var progress = 0;
            using var client = CreateWebClient();
            client.DownloadProgressChanged += (s, e) => progress = e.ProgressPercentage;
            client.DownloadFileAsync(downloadUri, tempFile);

            using (var progressbar = new ProgressBar())
            {
                while (client.IsBusy)
                    progressbar.Refresh(progress, $"Downloading {package.FullName} @ {package.Release.Name}");
            }

            Console.WriteLine();
            ZipExtractor.ExtractTo(tempFile, packageDirectory);
        }

        private static WebClient CreateWebClient()
        {
            var client = new WebClient();
            client.Headers.Add("user-agent", GithubUserAgent);

            return client;
        }

        public Release GetLatestRelease(Package package, bool useDefaultBranch)
        {
            if (!useDefaultBranch && GetAvailableReleases(package).Count == 0)
                throw new Exception(
                    $"{package.FullName} has no releases, you may want to use --no-release");

            if (!useDefaultBranch && GetAvailableReleases(package).Count != 0)
                return GetAvailableReleases(package).First();

            using var client = CreateWebClient();
            var uri = string.Format(PackageDetailsUri, package.Vendor, package.PackageName);
            var response = JObject.Parse(client.DownloadString(uri));

            return new Release {Name = response["default_branch"].ToString()};
        }
    }
}