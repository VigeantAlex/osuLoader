﻿using System;
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

            Type pWebRequest = asm.GetType(AsmEncrypt.symbolDictionary["class_pWebRequest"]);
            FieldInfo pWebRequest_url = pWebRequest.GetField(AsmEncrypt.symbolDictionary["field_pWebRequest_url"], BindingFlags.Instance | BindingFlags.NonPublic);

            if (value.Contains("check-updates.php")) value = "https://pastebin.com/raw/A6SayCDD"; // Rewrite updates.

            // Change server URLs.
            if (value.Contains("osu.ppy.sh")) value = value.Replace("osu.ppy.sh", Program.mainServer);
            if (value.Contains("a.ppy.sh")) value = value.Replace("a.ppy.sh", Program.avatarServer);
            if (value.StartsWith("https://c") && value.EndsWith(".ppy.sh")) value = $"https://{Program.banchoServer}";

            if (!Program.useHttps)
                value = value.Replace("https://", "http://");
            else
                value = value.Replace("http://", "https://");

            if (value.Contains("ppy.sh")) value = value.Replace("ppy.sh", "wangs.sh"); // Don't allow connections to offical osu! servers.

            pWebRequest_url.SetValue(this, value);
        }

        public void pWebRequest_checkCertificate() { } // Stubbed.
    }
}
