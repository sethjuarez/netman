using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

namespace netman
{
    public class Parser
    {
        public Package Package { get; set; }

        public bool Clean { get; set; }

        public bool List { get; set; }

        public string Message { get; private set; }

        public string Help { get; private set; }

        public Parser()
        {
            Clean = false;
            List = false;
            Message = String.Empty;
            Help = @"  
    -c : Cleans working directory of all files
    -l : Lists all available packages

    Samples:
        netman -l             (lists all packages)
        netman mypackage      (extracts mypackage to current working directory)
        netman mypackage -c   (cleans working folder and extracts mypackage)";
        }

        public bool Parse(string[] args)
        {
            if (args.Length == 0)
                return false;

            List = Clean = false;

            foreach (var a in args)
            {
                if (a.ToLowerInvariant() == "-l".ToLowerInvariant())
                    List = true;
                if (a.ToLowerInvariant() == "-c".ToLowerInvariant())
                    Clean = true;
            }

            var package = args[0];
            if (!List)
            {
                var packagePath = Path.Combine(netman.Package.GetPackagePath(), $"{package}.zip");
                if (!File.Exists(packagePath))
                {
                    Message = $"{package} is not a valid package.";
                    return false;
                }
                else
                {
                    Package = new Package
                    {
                        Name = package,
                        Path = packagePath
                    };
                }
            }


            return true;
        }

    }
}
