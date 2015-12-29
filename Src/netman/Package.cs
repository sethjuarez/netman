using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.IO.Compression;
using System.Collections.Generic;

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
                WriteLine(error.Message, ConsoleColor.Red);
                Console.WriteLine("Try running with -c to clean the directory\nWARNING: It will delete all files in the directory");
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
                    WriteLine($"Removing: {file.FullName}", ConsoleColor.Red);
                    try
                    {
                        file.Delete();
                    }
                    catch(IOException e)
                    {
                        WriteLine(e.Message, ConsoleColor.Red);
                    }
                }
            }
        }

        private static void PrintDirectory(string dir)
        {
            foreach (string f in Directory.GetFiles(dir))
                WriteLine($"Created: {f}", ConsoleColor.Green);

            foreach (string d in Directory.GetDirectories(dir))
                PrintDirectory(d);
        }

        public static string GetPackagePath()
        {
            string codeBase = Assembly.GetExecutingAssembly().CodeBase;
            UriBuilder uri = new UriBuilder(codeBase);
            var path = Uri.UnescapeDataString(uri.Path);
            return System.IO.Path.Combine(System.IO.Path.GetDirectoryName(path), "packages");
        }

        public static void List()
        {
            var packagePath = Package.GetPackagePath();
            Console.Write("\n");
            if (Directory.Exists(packagePath))
            {
                foreach (var zip in Directory.EnumerateFiles(packagePath, "*.zip"))
                    WriteLine(System.IO.Path.GetFileNameWithoutExtension(zip), ConsoleColor.Green);
            }
        }

        private static void WriteLine(string text, ConsoleColor color)
        {
            var curColor = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.WriteLine(text);
            Console.ForegroundColor = curColor;
        }
    }
}

