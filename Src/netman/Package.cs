using System;
using System.IO;
using System.Reflection;
using System.IO.Compression;

namespace netman
{
    public class Package
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public string Description { get; set; }

        public void Extract()
        {
            var working = System.IO.Path.GetFullPath(".");
            try
            {
                ZipFile.ExtractToDirectory(Path, working);
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
            Clean(System.IO.Path.GetFullPath("."));
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
                    catch(IOException e)
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
            return System.IO.Path.Combine(System.IO.Path.GetDirectoryName(path), "packages");
#endif
        }

        public static void List()
        {
            var packagePath = Package.GetPackagePath();
            if (Directory.Exists(packagePath))
            {
                foreach (var zip in Directory.EnumerateFiles(packagePath, "*.zip"))
                    Write.OutcomeLine(System.IO.Path.GetFileNameWithoutExtension(zip));
            }
        }
    }
}

