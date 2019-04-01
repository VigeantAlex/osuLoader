using System;

namespace osuLoader
{
    class Logger
    {
        private static void WriteConsole(string value, bool newLine)
        {
            if (newLine)
                Console.WriteLine(value);
            else
                Console.Write(value);
        }

        public static void WriteNotice(string value, bool newLine = true)
        {
            Console.ForegroundColor = ConsoleColor.White;
            WriteConsole(value, newLine);
        }

        public static void WriteSuccess(string value, bool newLine = true)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            WriteConsole(value, newLine);
        }

        public static void WriteError(string value, bool newLine = true)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            WriteConsole(value, newLine);
        }
    }
}
