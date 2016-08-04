using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Yanitta
{
    public static class Settings
    {
        readonly static string SettingsFileName = System.IO.Path.Combine(Environment.CurrentDirectory, "settings.ini");

        public static int Build { get; set; }

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

        public static long PlayerName       => GetPrivateProfileInt(Build.ToString(), "UnitName", 0, SettingsFileName);
        public static long PlayerClass      => GetPrivateProfileInt(Build.ToString(), "UnitClas", 0, SettingsFileName);
        public static long IsInGame         => GetPrivateProfileInt(Build.ToString(), "IsInGame", 0, SettingsFileName);
        public static long ExecuteBuffer    => GetPrivateProfileInt(Build.ToString(), "ExecBuff", 0, SettingsFileName);
        public static long InjectedAddress  => GetPrivateProfileInt(Build.ToString(), "Inj_Addr", 0, SettingsFileName);

        public static long ObjectMr => GetPrivateProfileInt(Build.ToString(), "ObjectMr", 0, SettingsFileName);
        public static long ObjTrack => GetPrivateProfileInt(Build.ToString(), "ObjTrack", 0, SettingsFileName);
        public static long TestClnt => GetPrivateProfileInt(Build.ToString(), "TestClnt", 0, SettingsFileName);
        public static long FishEnbl => GetPrivateProfileInt(Build.ToString(), "FishEnbl", 0, SettingsFileName);
    }
}
