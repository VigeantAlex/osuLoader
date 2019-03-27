using System;
using System.IO;
using System.Reflection;
using System.Security.Permissions;

namespace osuLoader
{
    class Program
    {
        public static string osuPath = Path.Combine(Directory.GetCurrentDirectory(), "osu!.exe");

        public static string mainServer   = "navisu.moe";
        public static string banchoServer = "c.navisu.moe";
        public static string avatarServer = "a.navisu.moe";

        public static Assembly asm;

        [STAThread]
        static void Main(string[] args)
        {
            Console.ForegroundColor = ConsoleColor.White;

            // Check if osu! executable exists.
            if (!File.Exists(osuPath))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("osu!Loader cannot run without \"osu!.exe\", please place it in the same path as osu!Loader.");
                Console.ForegroundColor = ConsoleColor.White;
                Console.ReadKey();
                return;
            }

            // Check if server file exists, if it does, load it.
            if (File.Exists("server.txt"))
            {
                string[] serverLines = File.ReadAllLines("server.txt");
                if (serverLines.Length != 3)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Invalid server file!");
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.ReadKey();
                    return;
                }

                mainServer   = serverLines[0];
                banchoServer = serverLines[1];
                avatarServer = serverLines[2];
            }

            Console.Write("Loading osu! assembly... ");

            asm = Assembly.LoadFile(osuPath); // Load assembly

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Loaded!");
            Console.ForegroundColor = ConsoleColor.White;

            // OsuMain
            MethodInfo OsuMain_FullPath         = null;
            MethodInfo OsuMain_FullPath_patched = null;

            MethodInfo OsuMain_Filename         = null;
            MethodInfo OsuMain_Filename_patched = null;

            // pWebRequest
            MethodInfo pWebRequest_set_Url         = null;
            MethodInfo pWebRequest_set_Url_patched = null;

            MethodInfo pWebRequest_checkCertificate         = null;
            MethodInfo pWebRequest_checkCertificate_patched = null;

            // Fetch methods
            try
            {
                Type OsuMain     = asm.GetType(AsmEncrypt.class_OsuMain);
                Type pWebRequest = asm.GetType(AsmEncrypt.class_pWebRequest);

                OsuMain_FullPath         = OsuMain.GetMethod(AsmEncrypt.method_OsuMain_FullPath, BindingFlags.Static | BindingFlags.NonPublic);
                OsuMain_FullPath_patched = typeof(MthdPatch).GetMethod("OsuMain_FullPath");

                OsuMain_Filename         = OsuMain.GetMethod(AsmEncrypt.method_OsuMain_Filename, BindingFlags.Static | BindingFlags.NonPublic);
                OsuMain_Filename_patched = typeof(MthdPatch).GetMethod("OsuMain_Filename");

                pWebRequest_set_Url         = pWebRequest.GetMethod(AsmEncrypt.method_pWebRequest_set_Url, BindingFlags.Instance | BindingFlags.NonPublic);
                pWebRequest_set_Url_patched = typeof(MthdPatch).GetMethod("pWebRequest_set_Url");

                pWebRequest_checkCertificate         = pWebRequest.GetMethod(AsmEncrypt.method_pWebRequest_checkCertificate, BindingFlags.Instance | BindingFlags.NonPublic);
                pWebRequest_checkCertificate_patched = typeof(MthdPatch).GetMethod("pWebRequest_checkCertificate");
            }
            catch (Exception)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("It looks like your osu! version is unsupported by osu!Loader, sorry!");
                Console.ForegroundColor = ConsoleColor.White;
                Console.ReadKey();
                return;
            }

            unsafe
            {
                try
                {
                    // Patch out executable checks
                    Console.Write("Patching executable checks... ");

                    int* p_OsuMain_FullPath         = (int*)OsuMain_FullPath.MethodHandle.Value.ToPointer()         + 2;
                    int* p_OsuMain_FullPath_patched = (int*)OsuMain_FullPath_patched.MethodHandle.Value.ToPointer() + 2;

                    int* p_OsuMain_Filename         = (int*)OsuMain_Filename.MethodHandle.Value.ToPointer()         + 2;
                    int* p_OsuMain_Filename_patched = (int*)OsuMain_Filename_patched.MethodHandle.Value.ToPointer() + 2;

                    *p_OsuMain_FullPath = *p_OsuMain_FullPath_patched;
                    *p_OsuMain_Filename = *p_OsuMain_Filename_patched;

                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("Patched!");
                    Console.ForegroundColor = ConsoleColor.White;

                    /******/

                    // Patch out certificate checks
                    Console.Write("Patching URL & certificate checks... ");

                    int* p_pWebRequest_set_Url         = (int*)pWebRequest_set_Url.MethodHandle.Value.ToPointer()         + 2;
                    int* p_pWebRequest_set_Url_patched = (int*)pWebRequest_set_Url_patched.MethodHandle.Value.ToPointer() + 2;

                    int* p_pWebRequest_checkCertificate         = (int*)pWebRequest_checkCertificate.MethodHandle.Value.ToPointer() + 2;
                    int* p_pWebRequest_checkCertificate_patched = (int*)pWebRequest_checkCertificate_patched.MethodHandle.Value.ToPointer() + 2;

                    *p_pWebRequest_set_Url          = *p_pWebRequest_set_Url_patched;
                    *p_pWebRequest_checkCertificate = *p_pWebRequest_checkCertificate_patched;

                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("Patched!");
                    Console.ForegroundColor = ConsoleColor.White;
                }
                catch (Exception)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("It looks like your osu! version is unsupported by osu!Loader, sorry!");
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.ReadKey();
                    return;
                }
            }

            new ReflectionPermission(ReflectionPermissionFlag.RestrictedMemberAccess).Assert();
            asm.EntryPoint.Invoke(null, null);
        }
    }
}
