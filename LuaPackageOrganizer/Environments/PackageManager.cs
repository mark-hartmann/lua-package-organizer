using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using LuaPackageOrganizer.Packages;
using Pastel;

namespace LuaPackageOrganizer.Environments
{
    public class PackageManager : IPackageManager
    {
        private readonly LupoJsonFile _lupoJson;
        private readonly LupoLockFile _lupoLock;
        private readonly FileSystemEnvironment _env;

        public PackageManager(FileSystemEnvironment environment)
        {
            IsModified = false;

            _env = environment;
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

                var colorizedPackageName = searchedPackage.FullName.Pastel(Color.CornflowerBlue);
                var colorizedReleaseName = resolvedPackage.Release.Name.Pastel(Color.CornflowerBlue);

                throw new Exception(
                    $"{colorizedPackageName} already installed, but using release {colorizedReleaseName}");
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
                Terminal.WriteNotice($"Installing {package.FullName.Pastel(Color.CornflowerBlue)}");
                _lupoJson.AddPackage(package);
            }
            else
            {
                Terminal.WriteNotice($"Installing {package.FullName.Pastel(Color.CornflowerBlue)} as dependency");
            }

            IsModified = true;
            _lupoLock.LockPackage(package, repository);

            repository.DownloadFiles(package, InstallPath(package));
            Console.WriteLine();
        }

        public void Uninstall(Package package)
        {
            // Recursively resolves all removable packages related to the package. Packets which were installed
            // explicitly are ignored
            var removablePackages = _lupoLock.GetRemovableDependencies(package)
                .Where(dep => !IsInstalled(dep, true)).ToList();

            Terminal.WriteNotice(
                $"Searched for no longer required dependencies, found {removablePackages.Count.ToString().Pastel(Color.Coral)}");
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
            var packageName = package.FullName.Pastel(Color.CornflowerBlue);

            if (passive)
            {
                Terminal.WriteNotice($"{packageName} is required by another package and will not be removed entirely!",
                    $"Removing {packageName} from lupo.json, leaving the rest as is...");

                _lupoJson.RemovePackage(package);
            }
            else
            {
                // Removes every file withing the packages installation directory
                Directory.Delete(InstallPath(package), true);

                Terminal.WriteDebug($"Removing {packageName}");

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