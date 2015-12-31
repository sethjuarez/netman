using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

namespace netman
{
    public class Parser
    {
        public bool Sync { get; set; }

        public string SyncLocation { get; set; }

        public Package Package { get; set; }

        public bool Clean { get; set; }

        public bool List { get; set; }

        public string Message { get; private set; }

        public string Help { get; private set; }

        public Parser()
        {
            Clean = false;
            List = false;
            Sync = false;
            SyncLocation = string.Empty;
            Message = String.Empty;
            Help = @"  
    -c:--clean             Cleans working directory of all files
    -l:--list              Lists all available packages
    -s:--sync [remote]   Synchronizes local packages (optionally synchronizes from remote)

    Samples:
        netman -l             (lists all packages)
        netman mypackage      (extracts mypackage to current working directory)
        netman mypackage -c   (cleans working folder and extracts mypackage)
        netman -s             (synchronizes local packages as defined in the manifest)
        netman -s remote      (synchronizes local packages by combining local and remote manifest)
";

        }

        public bool Parse(string[] args)
        {
            if (args.Length == 0)
                return false;

            
            Func<string, string, bool> cmp = (s1, s2)
                => s1.ToLowerInvariant() == s2.ToLowerInvariant();


            for (int i = 0; i < args.Length; i++)
            {
                var a = args[i];
                if (cmp(a, "-l") || cmp(a, "--list"))
                    List = true;
                if (cmp(a, "-c") || cmp(a, "--clean"))
                    Clean = true;
                if (cmp(a, "-s") || cmp(a, "--sync"))
                {
                    Sync = true;
                    if (i + 1 < args.Length && !args[i + 1].StartsWith("-"))
                    {
                        SyncLocation = args[i + 1];
                        i++;
                    }
                }

            }


            if (!List && !Sync)
            {
                var package = args[0];
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
                        Name = package
                    };
                }
            }

            return true;
        }

    }
}
