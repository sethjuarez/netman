using Fclp;
using System;
using System.Collections.Generic;

using System.Linq;
using System.Reflection;
using System.Text;

using System.Threading.Tasks;
using System.IO;
using System.IO.Compression;

namespace netman
{
    class Program
    {
        static void Main(string[] args)
        {
            string package = string.Empty;
            bool clean = false;
            bool list = false;

            var p = new FluentCommandLineParser();

            p.Setup<bool>('c', "clean")
             .Callback(c => clean = c)
             .SetDefault(false)
             .WithDescription("Cleans current directory [DELETES STUFF]");

            p.Setup<bool>('l', "list")
             .Callback(l => list = l)
             .SetDefault(false)
             .WithDescription("Lists available packages");

            p.SetupHelp("?", "help")
             .Callback(text => Console.WriteLine(text));

            p.Parse(args);

            if (args.Length == 0)
            {
                p.HelpOption.ShowHelp(p.Options);
                return;
            }

            package = args[0];

            if (list)
            {
                List();
                return;
            }


            var packagePath = $"{GetExecutingPath()}\\packages\\{package}.zip";

            if (!File.Exists(packagePath))
            {
                Console.WriteLine($"Package {package} does not exist");
                return;
            }

            var working = Path.GetFullPath(".");

            if (clean)
                Clean(working);

            try
            {
                ZipFile.ExtractToDirectory(packagePath, working);
                PrintDirectory(working);
            }
            catch (IOException error)
            {
                WriteLine(error.Message, ConsoleColor.Red);
                Console.WriteLine("Try running with -c to clean the directory\nWARNING: It will delete all files in the directory");
            }

        }

        private static void PrintDirectory(string dir)
        {
            foreach (string f in Directory.GetFiles(dir))
                WriteLine($"Created: {f}", ConsoleColor.Green);

            foreach (string d in Directory.GetDirectories(dir))
                PrintDirectory(d);
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
                    file.Delete();
                }
            }
        }


        private static string GetExecutingPath()
        {
            string codeBase = Assembly.GetExecutingAssembly().CodeBase;
            UriBuilder uri = new UriBuilder(codeBase);
            var path = Uri.UnescapeDataString(uri.Path);
            return Path.GetDirectoryName(path);
        }

        public static void List()
        {
            var packagePath = $"{GetExecutingPath()}\\packages";
            if (Directory.Exists(packagePath))
            {
                foreach (var zip in Directory.EnumerateFiles(packagePath, "*.zip"))
                    Console.WriteLine(Path.GetFileNameWithoutExtension(zip));
            }
        }

        private static void WriteLine(string text, ConsoleColor color)
        {
            var curColor = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.WriteLine(text);
            Console.ForegroundColor = curColor;
        }

        private static void Write(string text, ConsoleColor color)
        {
            var curColor = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.Write(text);
            Console.ForegroundColor = curColor;
        }
    }
}
