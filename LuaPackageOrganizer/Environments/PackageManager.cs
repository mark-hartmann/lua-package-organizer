using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LuaPackageOrganizer.Commands.Output;
using LuaPackageOrganizer.Packages;

namespace LuaPackageOrganizer.Environments
{
    public class PackageManager : IPackageManager
    {
        private readonly LupoJsonFile _lupoJson;
        private readonly LupoLockFile _lupoLock;
        private readonly IOutput _output;
        private readonly FileSystemEnvironment _env;

        public PackageManager(FileSystemEnvironment environment, IOutput output)
        {
            IsModified = false;

            _env = environment;
            _output = output;
            _lupoJson = LupoJsonFile.ParseFile(environment.LupoJsonFile);
            _lupoLock = LupoLockFile.ParseFile(environment.LupoLockFile);
        }

        public bool IsModified { get; private set; }

        public bool IsInstalled(Package package, bool explicitly = false)
        {
            // Only searches in the lupo.json file, if the package is already installed using a different release, an
            // exception is thrown
            static bool ContainedIn(Package searchedPackage, IEnumerable<Package> packages)
            {
                var resolvedPackage = packages.FirstOrDefault(p => p.FullName == searchedPackage.FullName);

                if (resolvedPackage.PackageName == null)
                    return false;

                if (resolvedPackage.Release.Equals(searchedPackage.Release))
                    return true;

                throw new Exception(
                    $"<package>{searchedPackage.FullName}</package> already installed, but using release <release>{resolvedPackage.Release}</release>");
            }

            return ContainedIn(package, explicitly ? _lupoJson.Packages : _lupoLock.GetPackages());
        }

        public Package ResolveInstalled(string vendor, string package)
        {
            return _lupoJson.Packages.FirstOrDefault(p => p.FullName == vendor + '/' + package);
        }

        public bool HasDependents(Package package)
        {
            return _lupoLock.GetDependents(package).Any();
        }

        public void Install(Package package, IRepository repository, bool explicitly = false)
        {
            if (repository.HasPackage(package) == false)
            {
                throw new PackageNotFoundException(package);
            }

            if (explicitly)
            {
                _output.WriteNotice($"Installing <package>{package.FullName}</package>");
                _lupoJson.AddPackage(package);
            }
            else
            {
                _output.WriteNotice($"Installing <package>{package.FullName}</package> as dependency");
            }

            IsModified = true;
            _lupoLock.LockPackage(package, repository);

            repository.DownloadFiles(package, InstallPath(package));
            _output.WriteLine(null);
        }

        public void Uninstall(Package package)
        {
            // Recursively resolves all removable packages related to the package. Packets which were installed
            // explicitly are ignored
            var removablePackages = _lupoLock.GetRemovableDependencies(package)
                .Where(dep => !IsInstalled(dep, true)).ToList();

            _output.WriteNotice($"Searched for no longer required dependencies, found {removablePackages.Count}");
            
            foreach (var removablePackage in removablePackages)
            {
                UninstallPackage(removablePackage);
            }

            // Removes the actual package. If the package has any dependents, it only gets removed from the lupo.json
            UninstallPackage(package, HasDependents(package));
        }

        /// <summary>
        /// The actual package remover. Removes a package completely (files & lupo.lock) if passive equals false,
        /// otherwise the package is only removed from lupo.json
        /// </summary>
        /// <param name="package"></param>
        /// <param name="passive">If true, the package is removed from the lupo.json only</param>
        private void UninstallPackage(Package package, bool passive = false)
        {
            if (passive)
            {
                _output.WriteNotice($"<package>{package.FullName}</package> is required by another package and will not be removed entirely!",
                    $"Removing <package>{package.FullName}</package from lupo.json, leaving the rest as is...");

                _lupoJson.RemovePackage(package);
            }
            else
            {
                // Removes every file withing the packages installation directory
                Directory.Delete(InstallPath(package), true);

                _output.WriteNotice($"Removing <package>{package.FullName}</package>");

                _lupoLock.UnlockPackage(package);
                _lupoJson.RemovePackage(package);
            }

            IsModified = true;
        }

        public void ApplyChanges()
        {
            if (!IsModified)
            {
                return;
            }

            _lupoJson.WriteChanges();
            _lupoLock.WriteChanges();
        }

        public void RollbackChanges()
        {
            throw new NotImplementedException();
        }

        public string InstallPath(Package package) =>
            Path.Join(_env.VendorDirectoryPath, package.Vendor, package.PackageName);
    }
}