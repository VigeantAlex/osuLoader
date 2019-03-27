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

            if (!File.Exists(osuPath))
            {
                Console.WriteLine("osu!Loader cannot run without \"osu!.exe\", please place it in the same path as osu!Loader.");
                Console.ReadKey();
                return;
            }

            if (File.Exists("server.txt"))
            {
                string[] serverLines = File.ReadAllLines("server.txt");

                mainServer   = serverLines[0];
                banchoServer = serverLines[1];
                avatarServer = serverLines[2];
            }

            Console.Write("Loading osu! assembly... ");

            asm = Assembly.LoadFile(osuPath);

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Loaded!");
            Console.ForegroundColor = ConsoleColor.White;

            Type OsuMain     = asm.GetType(AsmEncrypt.class_OsuMain);
            Type pWebRequest = asm.GetType(AsmEncrypt.class_pWebRequest);

            MethodInfo OsuMain_FullPath         = OsuMain.GetMethod(AsmEncrypt.method_OsuMain_FullPath, BindingFlags.Static | BindingFlags.NonPublic);
            MethodInfo OsuMain_FullPath_patched = typeof(MthdPatch).GetMethod("OsuMain_FullPath");

            MethodInfo OsuMain_Filename         = OsuMain.GetMethod(AsmEncrypt.method_OsuMain_Filename, BindingFlags.Static | BindingFlags.NonPublic);
            MethodInfo OsuMain_Filename_patched = typeof(MthdPatch).GetMethod("OsuMain_Filename");

            MethodInfo pWebRequest_set_Url         = pWebRequest.GetMethod(AsmEncrypt.method_pWebRequest_set_Url, BindingFlags.Instance | BindingFlags.NonPublic);
            MethodInfo pWebRequest_set_Url_patched = typeof(MthdPatch).GetMethod("pWebRequest_set_Url");

            MethodInfo pWebRequest_checkCertificate         = pWebRequest.GetMethod(AsmEncrypt.method_pWebRequest_checkCertificate, BindingFlags.Instance | BindingFlags.NonPublic);
            MethodInfo pWebRequest_checkCertificate_patched = typeof(MthdPatch).GetMethod("pWebRequest_checkCertificate");

            unsafe
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

                int* p_pWebRequest_checkCertificate         = (int*)pWebRequest_checkCertificate.MethodHandle.Value.ToPointer()         + 2;
                int* p_pWebRequest_checkCertificate_patched = (int*)pWebRequest_checkCertificate_patched.MethodHandle.Value.ToPointer() + 2;

                *p_pWebRequest_set_Url          = *p_pWebRequest_set_Url_patched;
                *p_pWebRequest_checkCertificate = *p_pWebRequest_checkCertificate_patched;

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Patched!");
                Console.ForegroundColor = ConsoleColor.White;
            }

            new ReflectionPermission(ReflectionPermissionFlag.RestrictedMemberAccess).Assert();
            asm.EntryPoint.Invoke(null, null);
        }
    }
}
