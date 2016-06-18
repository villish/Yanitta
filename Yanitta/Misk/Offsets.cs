using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Yanitta
{
    public class Offsets
    {
        readonly string fileName = Path.Combine(Environment.CurrentDirectory, "offsets.ini");

        public long PlayerName;
        public long PlayerClass;
        public long IsInGame;
        public long ExecuteBuffer;
        public long InjectedAddress;

        public long ObjectMr;
        public long ObjTrack;
        public long TestClnt;
        public long FishEnbl;

        public Offsets(string section)
        {
            if (!File.Exists(fileName))
                throw new FileNotFoundException("File not found", fileName);

            PlayerName      = GetValue(section, "UnitName", fileName);
            PlayerClass     = GetValue(section, "UnitClas", fileName);
            IsInGame        = GetValue(section, "IsInGame", fileName);
            ExecuteBuffer   = GetValue(section, "ExecBuff", fileName);
            InjectedAddress = GetValue(section, "Inj_Addr", fileName);

            ObjectMr = GetValueOrZero(section, "ObjectMr", fileName);
            ObjTrack = GetValueOrZero(section, "ObjTrack", fileName);
            TestClnt = GetValueOrZero(section, "TestClnt", fileName);
            FishEnbl = GetValueOrZero(section, "FishEnbl", fileName);
        }

        #region WinApi

        [DllImport("kernel32.dll")]
        static extern int GetPrivateProfileInt(string lpAppName, string lpKeyName, int nDefault, string lpFileName);

        long GetValue(string section, string key, string file)
        {
            if (string.IsNullOrWhiteSpace(section))
                throw new ArgumentNullException(nameof(section));

            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));

            var val = GetPrivateProfileInt(section, key, 0, file);

            if (val == 0L)
                throw new NullReferenceException("key");

            return val;
        }

        long GetValueOrZero(string section, string key, string file)
        {
            if (string.IsNullOrWhiteSpace(section))
                throw new ArgumentNullException(nameof(section));

            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));

            var val = GetPrivateProfileInt(section, key, 0, file);
            return val;
        }

        #endregion
    }
}