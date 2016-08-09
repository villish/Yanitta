using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Yanitta
{
    public static class Settings
    {
        readonly static string SettingsFileName = System.IO.Path.Combine(Environment.CurrentDirectory, "settings.ini");

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        static extern uint GetPrivateProfileString(string lpAppName, string lpKeyName, string lpDefault, StringBuilder lpReturnedString, int nSize, string lpFileName);

        [DllImport("kernel32.dll")]
        static extern uint GetPrivateProfileInt(string lpAppName, string lpKeyName, int nDefault, string lpFileName);

        public static string ProfilePath
        {
            get
            {
                var value = new StringBuilder(255);
                GetPrivateProfileString("General", "ProfilePath", "", value, value.Capacity, SettingsFileName);
                return value.ToString();
            }
        }

        public static void Load(int build)
        {
            Func<string, long> get = (key) => GetPrivateProfileInt(build.ToString(), key, 0, SettingsFileName);

            PlayerName      = get("UnitName");
            PlayerClass     = get("UnitClas");
            IsInGame        = get("IsInGame");
            ExecuteBuffer   = get("ExecBuff");
            InjectedAddress = get("Inj_Addr");
            ObjectMgr       = get("ObjectMr");
            ObjTrack        = get("ObjTrack");
            TestClnt        = get("TestClnt");
            FishEnbl        = get("FishEnbl");
            FirstObject     = get("FirstObject");
            NextObject      = get("NextObject");
            Type            = get("Type");
            Player          = get("Player");
            VisibleGuid     = get("VisibleGuid");
            AnimationState  = get("AnimationState");
            CreatedBy       = get("CreatedBy");
        }

        public static long PlayerName { get; private set; }
        public static long PlayerClass { get; private set; }
        public static long IsInGame { get; private set; }
        public static long ExecuteBuffer { get; private set; }
        public static long InjectedAddress { get; private set; }

        public static long ObjectMgr { get; private set; }
        public static long ObjTrack { get; private set; }
        public static long TestClnt { get; private set; }
        public static long FishEnbl { get; private set; }

        public static long FirstObject { get; private set; }
        public static long NextObject { get; private set; }
        public static long Type { get; private set; }
        public static long Player { get; private set; }
        public static long VisibleGuid { get; private set; }
        public static long AnimationState { get; private set; }
        public static long CreatedBy { get; private set; }
    }
}
