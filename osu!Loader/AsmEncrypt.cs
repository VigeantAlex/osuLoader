using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace osuLoader
{
    class AsmEncrypt
    {
        public static Dictionary<string, string> symbolDictionary = new Dictionary<string, string>();

        public static bool Load(string exeHash)
        {
            if (File.Exists($"{Program.loaderDir}{exeHash}.dat"))
            {
                string[] asmSymbols = File.ReadAllLines($"{Program.loaderDir}{exeHash}.dat");
                for (int i = 0; i < asmSymbols.Length; i++)
                {
                    string line = asmSymbols[i].Trim();

                    if (line.StartsWith("//")) continue;
                    if (line.Contains("//")) line = Regex.Split(line, "//")[0];

                    string[] token = line.Replace(" ", string.Empty).Split(':');

                    if (token[0].Length != 0) symbolDictionary.Add(token[0], token[1]);
                }

                return true;
            }

            return false;
        }
    }
}
