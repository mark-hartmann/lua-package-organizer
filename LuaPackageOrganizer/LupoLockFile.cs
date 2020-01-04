using System.Collections.Generic;
using System.IO;
using LuaPackageOrganizer.Packages;
using LuaPackageOrganizer.Packages.Repositories;
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

        public void LockPackage(Package package, GithubRepository responsibleRepository)
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

        public List<Package> GetPackages()
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