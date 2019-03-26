using System;
using System.Reflection;

namespace osuLoader
{
    class MthdPatch
    {
        public static string OsuMain_FullPath() { return Program.osuPath; }
        public static string OsuMain_Filename() { return "osu!.exe";      }

        public void pWebRequest_set_Url(string value)
        {
            Assembly asm = Program.asm;

            Type      OsuMain              = asm.GetType(AsmEncrypt.class_OsuMain);
            FieldInfo OsuMain_startupValue = OsuMain.GetField(AsmEncrypt.field_OsuMain_startupValue, BindingFlags.Static | BindingFlags.NonPublic);

            Console.WriteLine($"StartupValue: {OsuMain_startupValue.GetValue(null)}");

            Type      pWebRequest     = asm.GetType(AsmEncrypt.class_pWebRequest);
            FieldInfo pWebRequest_url = pWebRequest.GetField(AsmEncrypt.field_pWebRequest_url, BindingFlags.Instance | BindingFlags.NonPublic);

            if (value.Contains("check-updates.php")) value = "https://pastebin.com/raw/A6SayCDD";

            if (value.Contains("osu.ppy.sh")) value = value.Replace("osu.ppy.sh", Program.mainServer);
            if (value.Contains("a.ppy.sh"))   value = value.Replace("a.ppy.sh",   Program.avatarServer);

            Console.WriteLine($"URL: {value}");

            pWebRequest_url.SetValue(this, value);
        }

        public void pWebRequest_checkCertificate() { } // Stubbed.
    }
}
