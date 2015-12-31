using System;
using System.IO;
using System.Reflection;
using System.IO.Compression;
using System.Linq;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace netman
{
    public class Package
    {
        public const string MANIFEST_FILE = "manifest.json";
        public string Name { get; set; }

        public string Description { get; set; }

        public string Remote { get; set; }

        public void Extract()
        {
            var working = Path.GetFullPath(".");
            var path = Path.Combine(GetPackagePath(), $"{Name}.zip");
            try
            {
                ZipFile.ExtractToDirectory(path, working);
                PrintDirectory(working);
            }
            catch (IOException error)
            {
                Write.EmphasisLine(error.Message);
                Write.NormalLine("Try running with -c to clean the directory\nWARNING: It will delete all files in the directory");
            }
        }

        public void Clean()
        {
            Clean(Path.GetFullPath("."));
        }

        private static void Clean(string path)
        {
            if (Directory.Exists(path))
            {
                DirectoryInfo directory = new DirectoryInfo(path);

                foreach (DirectoryInfo dir in directory.GetDirectories())
                {
                    Clean(dir.FullName);
                    dir.Delete();
                }

                foreach (FileInfo file in directory.GetFiles())
                {
                    Write.EmphasisLine($"Removing: {file.FullName}");
                    try
                    {
                        file.Delete();
                    }
                    catch (IOException e)
                    {
                        Write.EmphasisLine(e.Message);
                    }
                }
            }
        }

        private static void PrintDirectory(string dir)
        {
            foreach (string f in Directory.GetFiles(dir))
                Write.OutcomeLine($"Created: {f}");

            foreach (string d in Directory.GetDirectories(dir))
                PrintDirectory(d);
        }

        public static string GetPackagePath()
        {
#if DOTNET || DNX
            return System.IO.Path.Combine(AppContext.BaseDirectory, "packages");
#else
            string codeBase = Assembly.GetExecutingAssembly().CodeBase;
            UriBuilder uri = new UriBuilder(codeBase);
            var path = Uri.UnescapeDataString(uri.Path);
            return Path.Combine(Path.GetDirectoryName(path), "packages");
#endif
        }

        public static void Synchronize()
        {
            // packages in manifest (that actually
            // have a corresponding zip file)
            var packagePath = GetPackagePath();
            var merge = GetManifest()
                            .Where(p => File.Exists(Path.Combine(packagePath, $"{p.Name}.zip")))
                            .ToList();

            var packages = merge.ToDictionary(p => p.Name);

            // files in directory
            var files = ManifestFromFiles().ToDictionary(p => p.Name);

            foreach (var key in files.Keys)
            {
                if (!packages.ContainsKey(key))
                    merge.Add(files[key]);
            }


            // nothing in folder
            if (merge.Count == 0)
                Write.EmphasisLine("manifest is empty, try syncing with a specific location");
            else
            {
                foreach (var p in merge)
                {
                    if (string.IsNullOrEmpty(p.Remote))
                        Write.OutcomeLine($"Adding {p.Name} to manifest (missing remote)");
                    else
                    {
                        try
                        {
                            Write.Outcome($"Downloading {p.Name} from {p.Remote}...");
                            Client.Download(p);
                            Write.OutcomeLine("Done!");
                        }
                        catch (Exception e)
                        {
                            Write.EmphasisLine($"\n{e.Message}");
                        }
                        
                    }
                }

                SaveManifest(merge.OrderBy(p => p.Name).ToArray());
            }
        }

        public static void Combine(string location)
        {
            string manifest = Path.Combine(Package.GetPackagePath(), MANIFEST_FILE);
            var json = File.ReadAllText(manifest);
            var packages = Client.GetPackagesFromJson(json).ToDictionary(p => p.Name);
            var remotePackages = Client.GetPackages(location).ToDictionary(p => p.Name);
            var finalPackages = new List<Package>();

            // sort through existing and sync
            foreach (var key in packages.Keys)
            {
                // remote package takes precedence
                if (remotePackages.ContainsKey(key))
                {
                    var p = remotePackages[key];
                    try
                    {
                        Write.Outcome($"Overwriting {p.Name} from {p.Remote}...");
                        Client.Download(p);
                        Write.OutcomeLine(" Done!");
                        finalPackages.Add(p);
                    }
                    catch(Exception e)
                    {
                        Write.EmphasisLine($"\n{e.Message}");
                    }
                }
                // nothing to do, just add it to list
                else
                {
                    var p = packages[key];
                    Write.OutcomeLine($"Ignoring {p.Name} (adding to manifest)");
                    finalPackages.Add(p);
                }
            }

            // sort through remote
            foreach (var key in remotePackages.Keys)
            {
                // if not in local, then sync
                if (!packages.ContainsKey(key))
                {
                    try
                    {
                        var p = remotePackages[key];
                        Write.Outcome($"Creating {p.Name} from {p.Remote}...");
                        Client.Download(p);
                        Write.OutcomeLine(" Done!");
                        finalPackages.Add(p);
                    }
                    catch (Exception e)
                    {
                        Write.EmphasisLine($"\n{e.Message}");
                    }
                }
            }

            Write.Outcome("Saving manifest...");
            SaveManifest(finalPackages.OrderBy(p => p.Name).ToArray());
            Write.OutcomeLine(" Done!");
        }

        public static void List()
        {
            var packages = GetManifest();
            if (packages.Length == 0)
                Write.EmphasisLine("There are no packages, try synchronizing");
            else
            {
                var max = packages.Select(p => p.Name.Length).Max();
                foreach (var p in packages)
                {
                    Write.Outcome($"{p.Name.PadRight(max + 5, ' ')}");
                    Write.NormalLine(p.Description);
                }
            }
        }

        public static Package[] ManifestFromFiles()
        {
            var manifest = Directory.EnumerateFiles(Package.GetPackagePath(), "*.zip")
                             .Select(s => new Package { Name = Path.GetFileNameWithoutExtension(s) })
                             .ToArray();

            return manifest;
        }

        public static Package[] GetManifest()
        {
            string manifest = Path.Combine(Package.GetPackagePath(), MANIFEST_FILE);
            var json = File.ReadAllText(manifest);
            return JsonConvert.DeserializeObject<Package[]>(json);
        }

        public static Package[] SaveManifest(Package[] packages)
        {
            string manifest = Path.Combine(Package.GetPackagePath(), MANIFEST_FILE);
            var json = JsonConvert.SerializeObject(packages, Formatting.Indented);
            File.WriteAllText(manifest, json);
            return packages;
        }
    }
}

