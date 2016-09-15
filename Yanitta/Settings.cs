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
            Func<string, long> getLong = (key) => GetPrivateProfileInt(build.ToString(), key, 0, SettingsFileName);
            Func<string, int>  getInt  = (key) => (int)GetPrivateProfileInt(build.ToString(), key, 0, SettingsFileName);

            // main offsets
            PlayerName      = getLong("PlayerName");
            PlayerClass     = getLong("PlayerClass");
            IsInGame        = getLong("IsInGame");
            ExecuteBuffer   = getLong("ExecuteBuffer");
            InjectedAddress = getLong("InjectAddress");

            // fish bot offsets
            ObjectMgr       = getLong("ObjectMgr");
            ObjectTrack     = getLong("ObjectTrack");
            TestClient      = getLong("TestClient");
            FishEnbl        = getLong("FishEnable");

            // update fields
            FirstObject     = getInt("FirstObject");
            NextObject      = getInt("NextObject");
            ObjectType      = getInt("ObjectType");
            PlayerGuid      = getInt("PlayerGuid");
            VisibleGuid     = getInt("VisibleGuid");
            AnimationState  = getInt("AnimationState");
            CreatedBy       = getInt("CreatedBy");
        }

        public static long PlayerName       { get; private set; }
        public static long PlayerClass      { get; private set; }
        public static long IsInGame         { get; private set; }
        public static long ExecuteBuffer    { get; private set; }
        public static long InjectedAddress  { get; private set; }

        public static long ObjectMgr        { get; private set; }
        public static long ObjectTrack      { get; private set; }
        public static long TestClient       { get; private set; }
        public static long FishEnbl         { get; private set; }

        public static int FirstObject       { get; private set; }
        public static int NextObject        { get; private set; }
        public static int ObjectType        { get; private set; }
        public static int PlayerGuid        { get; private set; }
        public static int VisibleGuid       { get; private set; }
        public static int AnimationState    { get; private set; }
        public static int CreatedBy         { get; private set; }
    }
}
