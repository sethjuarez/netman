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

            var parser = new Parser();
            var success = parser.Parse(args);
            if(!success)
            {
                Write.EmphasisLine(parser.Message);
                Write.NormalLine(parser.Help);
                return;
            }

            if (parser.List)
            {
                Package.List();
            }
            else
            {
                var p = parser.Package;
                if (parser.Clean)
                    p.Clean();
                p.Extract();                
            }
        }
    }
}
