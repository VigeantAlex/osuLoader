using System;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Security.Permissions;
using System.Text;

namespace osuLoader
{
    class Program
    {
        public const string loaderDir = "osu.Loader-Data/";

        public static string osuPath = Path.Combine(Directory.GetCurrentDirectory(), "osu!.exe");
        public static string osuHash = getOsuHash();

        public static string mainServer   = "navisu.moe";
        public static string banchoServer = "c.navisu.moe";
        public static string avatarServer = "a.navisu.moe";

        public static bool useHttps = true;

        public static Assembly asm;

        [STAThread]
        static void Main(string[] args)
        {
            if (!Directory.Exists(loaderDir)) Directory.CreateDirectory(loaderDir);

            // Check if osu! executable exists.
            if (!File.Exists(osuPath))
            {
                Logger.WriteError("osu!Loader cannot run without \"osu!.exe\", please place it in the same path as osu!Loader.");
                Console.ReadKey();
                return;
            }

            Logger.WriteNotice("Loading osu! assembly... ", false);

            asm = Assembly.LoadFile(osuPath); // Load assembly

            Logger.WriteSuccess("Loaded!");

            if (!AsmEncrypt.Load(osuHash))
            {
                Logger.WriteError($"It looks like your osu! version is unsupported by osu!Loader, sorry! osuHash = {osuHash}");
                Console.ReadKey();
                return;
            }

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
                Type OsuMain     = asm.GetType(AsmEncrypt.symbolDictionary["class_OsuMain"]);
                Type pWebRequest = asm.GetType(AsmEncrypt.symbolDictionary["class_pWebRequest"]);

                OsuMain_FullPath         = OsuMain.GetMethod(AsmEncrypt.symbolDictionary["method_OsuMain_FullPath"], BindingFlags.Static | BindingFlags.NonPublic);
                OsuMain_FullPath_patched = typeof(MthdPatch).GetMethod("OsuMain_FullPath");

                OsuMain_Filename         = OsuMain.GetMethod(AsmEncrypt.symbolDictionary["method_OsuMain_Filename"], BindingFlags.Static | BindingFlags.NonPublic);
                OsuMain_Filename_patched = typeof(MthdPatch).GetMethod("OsuMain_Filename");

                pWebRequest_set_Url         = pWebRequest.GetMethod(AsmEncrypt.symbolDictionary["method_pWebRequest_set_Url"], BindingFlags.Instance | BindingFlags.NonPublic);
                pWebRequest_set_Url_patched = typeof(MthdPatch).GetMethod("pWebRequest_set_Url");

                pWebRequest_checkCertificate         = pWebRequest.GetMethod(AsmEncrypt.symbolDictionary["method_pWebRequest_checkCertificate"], BindingFlags.Instance | BindingFlags.NonPublic);
                pWebRequest_checkCertificate_patched = typeof(MthdPatch).GetMethod("pWebRequest_checkCertificate");
            }
            catch (Exception)
            {
                Logger.WriteError($"It looks like your osu! version is unsupported by osu!Loader, sorry! osuHash = {osuHash}");
                Console.ReadKey();
                return;
            }

            // Check if server file exists, if it does, load it.
            if (File.Exists($"{loaderDir}server.dat"))
            {
                string[] serverLines = File.ReadAllLines($"{loaderDir}server.dat");
                if (serverLines.Length < 3)
                {
                    Logger.WriteError("Invalid server file!");
                    Console.ReadKey();
                    return;
                }

                mainServer   = serverLines[0];
                banchoServer = serverLines[1];
                avatarServer = serverLines[2];

                if (serverLines.Length >= 4) useHttps = bool.Parse(serverLines[3]);
            }

            unsafe
            {
                try
                {
                    // Patch out executable checks
                    Logger.WriteNotice("Patching executable checks... ", false);

                    int* p_OsuMain_FullPath         = (int*)OsuMain_FullPath.MethodHandle.Value.ToPointer()         + 2;
                    int* p_OsuMain_FullPath_patched = (int*)OsuMain_FullPath_patched.MethodHandle.Value.ToPointer() + 2;

                    int* p_OsuMain_Filename         = (int*)OsuMain_Filename.MethodHandle.Value.ToPointer()         + 2;
                    int* p_OsuMain_Filename_patched = (int*)OsuMain_Filename_patched.MethodHandle.Value.ToPointer() + 2;

                    *p_OsuMain_FullPath = *p_OsuMain_FullPath_patched;
                    *p_OsuMain_Filename = *p_OsuMain_Filename_patched;

                    Logger.WriteSuccess("Patched!");

                    /******/

                    // Patch out certificate checks
                    Logger.WriteNotice("Patching URL & certificate checks... ", false);

                    int* p_pWebRequest_set_Url         = (int*)pWebRequest_set_Url.MethodHandle.Value.ToPointer()         + 2;
                    int* p_pWebRequest_set_Url_patched = (int*)pWebRequest_set_Url_patched.MethodHandle.Value.ToPointer() + 2;

                    int* p_pWebRequest_checkCertificate         = (int*)pWebRequest_checkCertificate.MethodHandle.Value.ToPointer()         + 2;
                    int* p_pWebRequest_checkCertificate_patched = (int*)pWebRequest_checkCertificate_patched.MethodHandle.Value.ToPointer() + 2;

                    *p_pWebRequest_set_Url          = *p_pWebRequest_set_Url_patched;
                    *p_pWebRequest_checkCertificate = *p_pWebRequest_checkCertificate_patched;

                    Logger.WriteSuccess("Patched!");
                }
                catch (Exception)
                {
                    Logger.WriteError($"It looks like your osu! version is unsupported by osu!Loader, sorry! osuHash = {osuHash}");
                    Console.ReadKey();
                    return;
                }
            }

            new ReflectionPermission(ReflectionPermissionFlag.RestrictedMemberAccess).Assert();
            asm.EntryPoint.Invoke(null, null);
        }

        private static string getOsuHash()
        {
            MD5CryptoServiceProvider hashAlgo = new MD5CryptoServiceProvider();
            FileStream               osuExe   = new FileStream(osuPath, FileMode.Open, FileAccess.Read);

            hashAlgo.ComputeHash(osuExe);
            osuExe.Close();

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hashAlgo.Hash.Length; i++)
            {
                sb.Append(hashAlgo.Hash[i].ToString("x2"));
            }

            return sb.ToString().ToLowerInvariant();
        }
    }
}
