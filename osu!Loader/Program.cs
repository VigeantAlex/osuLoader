using System;
using System.IO;
using System.Reflection;
using System.Security.Permissions;

namespace osuLoader
{
    class Program
    {
        public static string osuPath = Path.Combine(Directory.GetCurrentDirectory(), "osu!.exe");

        private static string banchoServer = "https://c.ripple.moe";

        [STAThread]
        static void Main(string[] args)
        {
            if (!File.Exists(osuPath))
            {
                Console.WriteLine("osu!Loader cannot run without \"osu!.exe\", please place it in the same path as osu!Loader.");
                Console.ReadKey();
                return;
            }

            if (File.Exists("bancho.txt")) banchoServer = File.ReadAllLines("bancho.txt")[0];

            Console.Write("Loading osu! assembly... ");

            Assembly asm = Assembly.LoadFile(osuPath);

            Console.WriteLine("Loaded!");

            Type OsuMain             = asm.GetType(AsmEncrypt.class_OsuMain);
            Type AuthenticodeTools   = asm.GetType(AsmEncrypt.class_AuthenticodeTools);
            Type pWebRequest         = asm.GetType(AsmEncrypt.class_pWebRequest);
            Type BanchoClient        = asm.GetType(AsmEncrypt.class_BanchoClient);
            Type VoidDelegate        = asm.GetType(AsmEncrypt.delegate_VoidDelegate);
            Type NotificationManager = asm.GetType(AsmEncrypt.class_NotificationManager);

            Type Color = asm.GetType("Microsoft.Xna.Framework.Graphics.Color");

            MethodInfo FullPath         = OsuMain.GetMethod(AsmEncrypt.method_FullPath, BindingFlags.Static | BindingFlags.NonPublic);
            MethodInfo FullPath_patched = typeof(MthdPatch).GetMethod("FullPath");

            MethodInfo Filename         = OsuMain.GetMethod(AsmEncrypt.method_Filename, BindingFlags.Static | BindingFlags.NonPublic);
            MethodInfo Filename_patched = typeof(MthdPatch).GetMethod("Filename");

            MethodInfo IsTrusted         = AuthenticodeTools.GetMethod(AsmEncrypt.method_IsTrusted, BindingFlags.Static | BindingFlags.Public);
            MethodInfo IsTrusted_patched = typeof(MthdPatch).GetMethod("IsTrusted");

            MethodInfo checkCertificate         = pWebRequest.GetMethod(AsmEncrypt.method_checkCertificate, BindingFlags.Instance | BindingFlags.NonPublic);
            MethodInfo checkCertificate_patched = typeof(MthdPatch).GetMethod("checkCertificate");

            MethodInfo SetServer   = BanchoClient.GetMethod(AsmEncrypt.method_SetServer, BindingFlags.Static | BindingFlags.NonPublic, null, new[] { typeof(string[]) }, null);
            MethodInfo ShowMessage = NotificationManager.GetMethod(AsmEncrypt.method_ShowMessage, BindingFlags.Static | BindingFlags.NonPublic, null, new[] { typeof(string), Color, typeof(int), VoidDelegate }, null);

            unsafe
            {
                // Patch out executable checks
                Console.Write("Patching executable checks... ");

                int* p_FullPath         = (int*)FullPath.MethodHandle.Value.ToPointer()         + 2;
                int* p_FullPath_patched = (int*)FullPath_patched.MethodHandle.Value.ToPointer() + 2;

                int* p_Filename         = (int*)Filename.MethodHandle.Value.ToPointer()         + 2;
                int* p_Filename_patched = (int*)Filename_patched.MethodHandle.Value.ToPointer() + 2;

                int* p_IsTrusted         = (int*)IsTrusted.MethodHandle.Value.ToPointer()         + 2;
                int* p_IsTrusted_patched = (int*)IsTrusted_patched.MethodHandle.Value.ToPointer() + 2;

                *p_FullPath  = *p_FullPath_patched;
                *p_Filename  = *p_Filename_patched;
                *p_IsTrusted = *p_IsTrusted_patched;

                Console.WriteLine("Patched!");

                /******/

                // Patch out certificate checks
                Console.Write("Patching certificate checks... ");

                int* p_checkCertificate         = (int*)checkCertificate.MethodHandle.Value.ToPointer()         + 2;
                int* p_checkCertificate_patched = (int*)checkCertificate_patched.MethodHandle.Value.ToPointer() + 2;

                *p_checkCertificate = *p_checkCertificate_patched;

                Console.WriteLine("Patched!");

                /******/

                // Set server endpoints
                Console.Write("Setting server endpoints... ");

                SetServer.Invoke(null, new object[] { new string[] { banchoServer } });

                Console.WriteLine("Done!");
            }

            ShowMessage.Invoke(null, new object[] { "osu!Loader is now running.", Color.GetMethod("get_Orange", BindingFlags.Static | BindingFlags.Public).Invoke(null, null), 20000, null });

            new ReflectionPermission(ReflectionPermissionFlag.RestrictedMemberAccess).Assert();
            asm.EntryPoint.Invoke(null, null);
        }
    }
}
