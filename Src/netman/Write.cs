using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace netman
{
    public static class Write
    {
        public static void EmphasisLine(string text) => Emphasis($"{text}\n");
        public static void Emphasis(string text)
        {
#if DOTNET

#else
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write(text);
            Console.ResetColor();

#endif
        }

        public static void OutcomeLine(string text) => Outcome($"{text}\n");

        public static void Outcome(string text)
        {
#if DOTNET

#else
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write(text);
            Console.ResetColor();
#endif
        }

        public static void NormalLine(string text) => Normal($"{text}\n");

        public static void Normal(string text)
        {
#if DOTNET

#else
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write(text);
            Console.ResetColor();
#endif
        }
    }
}
