using System.Collections.Generic;
using System.IO;
using System.Linq;
using LuaPackageOrganizer.Packages;
using Newtonsoft.Json.Linq;

namespace LuaPackageOrganizer
{
    public class LupoLockFile
    {
        private JObject _state;
        private readonly string _source;

        private LupoLockFile(string source)
        {
            _source = source;
        }

        public void LockPackage(Package package, IRepository responsibleRepository)
        {
            var node = new JObject()
            {
                ["name"] = package.FullName,
                ["release"] = package.Release.Name,
                ["packages"] = new JObject()
            };

            var packageDependencies = responsibleRepository.GetDependencies(package);

            foreach (var dependency in packageDependencies)
                node["packages"][dependency.FullName] = dependency.Release.Name;

            ((JArray) _state["packages"]).Add(node);
        }

        public IEnumerable<Package> GetPackages()
        {
            var list = new List<Package>();

            foreach (var node in (JArray) _state["packages"])
            {
                var splitted = ((string) node["name"]).Split('/');
                list.Add(new Package(splitted[0], splitted[1], new Release
                {
                    Name = (string) node["release"]
                }));
            }

            return list;
        }

        public IEnumerable<Package> GetDependencies(Package package)
        {
            var dependencies = new List<Package>();

            foreach (var node in (JArray) _state["packages"])
            {
                if ((string) node["name"] != package.FullName)
                    continue;

                dependencies.AddRange(node["packages"].Select(pkgNode => Package.FromJProperty((JProperty) pkgNode)));
            }

            return dependencies;
        }

        public IEnumerable<Package> GetDependents(Package package)
        {
            var dependents = new List<Package>();

            foreach (var node in (JArray) _state["packages"])
            {
                if (node["packages"].All(n => ((JProperty) n).Name != package.FullName))
                    continue;

                var splitted = ((string) node["name"]).Split('/');
                dependents.Add(new Package(splitted[0], splitted[1], new Release
                {
                    Name = (string) node["release"]
                }));
            }

            return dependents;
        }

        public List<Package> GetRemovableDependencies(Package package)
        {
            List<Package> GetDependenciesRecursive(Package pkg)
            {
                var packages = GetDependencies(pkg);
                var dependencies = new List<Package>();

                foreach (var dependency in packages)
                {
                    dependencies.Add(dependency);
                    dependencies.AddRange(GetDependenciesRecursive(dependency));
                }

                return dependencies.Distinct().ToList();
            }

            var allDependencies = GetDependenciesRecursive(package);
            var removableDependencies = new List<Package>();

            foreach (var dependency in allDependencies)
            {
                // If a dependency itself has another constraint/dependent that is NOT in the recursive dependencies, it
                // must not be removed as it is used by another package 
                var dependents = GetDependents(dependency);
                var hasDependentOutside = dependents.Any(d => !allDependencies.Contains(d) && !d.Equals(package));

                if (!hasDependentOutside)
                    removableDependencies.Add(dependency);
            }

            return removableDependencies;
        }

        public static LupoLockFile ParseFile(string path)
        {
            var instance = new LupoLockFile(path)
            {
                _state = !File.Exists(path)
                    ? new JObject {["packages"] = new JArray()}
                    : JObject.Parse(File.ReadAllText(path))
            };

            return instance;
        }

        public void WriteChanges()
        {
            File.WriteAllText(_source, _state.ToString());
        }
    }
}