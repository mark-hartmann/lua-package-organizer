using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using LuaPackageOrganizer.Packages;
using Newtonsoft.Json.Linq;
using Pastel;

namespace LuaPackageOrganizer
{
    public class LupoJsonFile
    {
        private JObject _state;
        private JObject _initialState;
        private readonly string _source;

        private readonly List<Package> _addedPackages;
        private readonly List<Package> _initialPackages;

        public List<Package> Packages
        {
            get
            {
                var list = new List<Package>();

                list.AddRange(_initialPackages);
                list.AddRange(_addedPackages);

                return list;
            }
        }

        public bool IsModified => _addedPackages.Count > 0 || _state["packages"].Count() != _initialPackages.Count;

        private LupoJsonFile(string source)
        {
            _source = source;
            _addedPackages = new List<Package>();
            _initialPackages = new List<Package>();
        }

        public void AddPackage(Package package)
        {
            // todo: Check if the passed package is already contained
            _addedPackages.Add(package);
        }

        public void RemovePackage(Package package)
        {
            var index = _initialPackages.IndexOf(package);
            if (index == -1)
                return;

            _initialPackages.RemoveAt(index);
        }

        public void WriteChanges()
        {
            if (!IsModified)
                return;

            var packages = new JObject();
            foreach (var package in Packages)
                packages[package.FullName] = package.Release.Name;

            _state["packages"] = packages;

            File.WriteAllText(_source, _state.ToString());
        }

        public static LupoJsonFile ParseFile(string path)
        {
            if (File.Exists(path) != true)
                throw new Exception("lupo.json not found");

            var lupoJsonContent = File.ReadAllText(path);

            if (lupoJsonContent == "")
            {
                throw new Exception("lupo.json".Pastel(Color.Coral) + " file is either empty or invalid json");
            }

            // todo: Validate lupo.json schema before creating the instance!
            var instance = new LupoJsonFile(path)
            {
                _state = JObject.Parse(lupoJsonContent),
                _initialState = JObject.Parse(lupoJsonContent)
            };

            foreach (var jToken in instance._initialState["packages"].ToList())
                instance._initialPackages.Add(Package.FromJProperty((JProperty) jToken));

            return instance;
        }
    }
}