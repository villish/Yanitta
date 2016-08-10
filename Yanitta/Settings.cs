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

            // main offsets
            PlayerName      = get("PlayerName");
            PlayerClass     = get("PlayerClass");
            IsInGame        = get("IsInGame");
            ExecuteBuffer   = get("ExecuteBuffer");
            InjectedAddress = get("InjectAddress");

            // fish bot offsets
            ObjectMgr       = get("ObjectMgr");
            ObjectTrack     = get("ObjectTrack");
            TestClient      = get("TestClient");
            FishEnbl        = get("FishEnable");

            // update fields
            FirstObject     = get("FirstObject");
            NextObject      = get("NextObject");
            ObjectType      = get("ObjectType");
            Player          = get("Player");
            VisibleGuid     = get("VisibleGuid");
            AnimationState  = get("AnimationState");
            CreatedBy       = get("CreatedBy");
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

        public static long FirstObject      { get; private set; }
        public static long NextObject       { get; private set; }
        public static long ObjectType       { get; private set; }
        public static long Player           { get; private set; }
        public static long VisibleGuid      { get; private set; }
        public static long AnimationState   { get; private set; }
        public static long CreatedBy        { get; private set; }
    }
}
