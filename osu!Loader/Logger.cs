using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace osuLoader
{
    class Logger
    {
        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        const int SW_HIDE = 0;
        const int SW_SHOW = 5;

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

        public static void Close(int delay)
        {
            // Using custom Task.Delay function instead of Thread.Sleep 
            // to prevent Thread from freezing and not loading osu!
            IntPtr handle = GetConsoleWindow();
            Task task = TaskHandler.Delay(delay);
            task.ContinueWith(_ => ShowWindow(handle, SW_HIDE));
            
        }

    }
}
