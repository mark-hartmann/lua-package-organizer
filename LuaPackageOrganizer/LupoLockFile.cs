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
        private JObject _initialState;
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

        public static LupoLockFile ParseFile(string path)
        {
            var instance = new LupoLockFile(path);

            if (!File.Exists(path))
            {
                instance._state = new JObject {["packages"] = new JArray()};
                instance._initialState = new JObject {["packages"] = new JArray()};
            }
            else
            {
                instance._state = JObject.Parse(File.ReadAllText(path));
                instance._initialState = JObject.Parse(File.ReadAllText(path));
            }

            return instance;
        }

        public void WriteChanges()
        {
            File.WriteAllText(_source, _state.ToString());
        }
    }
}