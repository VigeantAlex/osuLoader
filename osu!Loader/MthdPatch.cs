namespace osuLoader
{
    class MthdPatch
    {
        // osu.OsuMain
        public static string FullPath() { return Program.osuPath; }
        public static string Filename() { return "osu!.exe";      }

        // osu_common.Helpers.pWebRequest
        public void checkCertificate() { }
    }
}
