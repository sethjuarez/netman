using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.IO.Compression;
using System.Collections.Generic;


namespace netman
{
    class Program
    {
        static void Main(string[] args)
        {
            // create package directory if it does not exist
            if (!Directory.Exists(Package.GetPackagePath()))
                Directory.CreateDirectory(Package.GetPackagePath());

            // create empty manifest file if it does not exist
            string manifest = Path.Combine(Package.GetPackagePath(), Package.MANIFEST_FILE);
            if (!File.Exists(manifest))
                File.WriteAllText(manifest, "[ ]");

            var parser = new Parser();
            var success = parser.Parse(args);
            if (!success)
            {
                Write.EmphasisLine(parser.Message);
                Write.NormalLine(parser.Help);
                return;
            }

            if (parser.List)
            {
                Package.List();
            }
            else if (parser.Sync)
            {
                // download current manifest items
                if (parser.SyncLocation == string.Empty)
                    Package.Synchronize();
                else // rebuild manifest
                    Package.Combine(parser.SyncLocation);
            }
            else
            {
                var p = parser.Package;
                if (parser.Clean)
                    p.Clean();
                p.Extract();
            }

#if DEBUG
            Console.ReadKey();
#endif
        }
    }
}
