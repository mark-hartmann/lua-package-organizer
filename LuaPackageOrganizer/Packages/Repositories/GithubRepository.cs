using System.Collections.Generic;
using System.Net;
using Newtonsoft.Json;

namespace LuaPackageOrganizer.Packages.Repositories
{
    public class GithubRepository : IPackageRepository
    {
        private const string GithubUserAgent = "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)";
        private const string PackageDetailsUri = "https://api.github.com/repos/{0}/{1}";
        private const string PackageReleasesUri = "https://api.github.com/repos/{0}/{1}/tags";

        public bool PackageExists(IPackage package)
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

        public bool IsReleaseAvailable(IPackage package, Release release)
        {
            var availableReleases = GetAvailableReleases(package);
            foreach (var rel in availableReleases)
            {
                if (release.Name.Equals(rel.Name))
                    return true;
            }

            return false;
        }

        public List<Release> GetAvailableReleases(IPackage package)
        {
            List<Release> releases;

            using (var client = new WebClient())
            {
                client.Headers.Add("user-agent", GithubUserAgent);
                var jsonResponse =
                    client.DownloadString(string.Format(PackageReleasesUri, package.Vendor, package.PackageName));

                releases = JsonConvert.DeserializeObject<List<Release>>(jsonResponse);
            }

            return releases;
        }
    }
}