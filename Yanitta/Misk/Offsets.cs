using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Yanitta
{
    public class Offsets
    {
        private const string fileName = "offsets.ini";

        /// <summary>
        /// Имя персонажа.
        ///
        /// Lua function UnitName - в самом конце функции
        /// v3 = sub_A30173(); -- GetPlayerName
        /// sub_4C7477(a1, v3);
        /// sub_4C72F7(a1);
        /// return 2;
        /// </summary>
        public long PlayerName;

        /// <summary>
        /// Класс персонажа
        ///
        /// Lua function UnitClass
        ///   else
        ///  {
        ///    v3 = sub_A30197();  // GetPlayerClass
        ///    v4 = sub_4027A0((int)&dword_11A1C88, (unsigned __int8)v3);
        ///    v5 = sub_A3019D();
        ///    v6 = sub_73EB65(v4, (unsigned __int8)v5, 0);
        ///  }
        /// </summary>
        public long PlayerClass;

        /// <summary>
        /// Нахождение в мире
        ///
        /// Lua function PlaySound
        ///   if ( !(unsigned __int8)sub_40B039() && (!sub_7D875E() || byte_12AB65E /*IsInWorld*/) )
        /// </summary>
        public long IsInGame;

        /// <summary>
        /// Адрес функции FrameScript::ExecuteBuffer
        ///
        /// Lua function RunScript
        /// </summary>
        public long ExecuteBuffer;

        /// <summary>
        /// Адресс вставки байт кода
        ///
        /// По строке 'compat.lua' ищем функцию инициализации Lua
        /// Начало этой функции и есть адресом.
        /// </summary>
        public long InjectedAddress;

        public Offsets(string section)
        {
            var file = Path.Combine(Environment.CurrentDirectory, fileName);

            if (!File.Exists(file))
                throw new FileNotFoundException("File not found", file);

            PlayerName      = GetValue(section, "UnitName", file);
            PlayerClass     = GetValue(section, "UnitClas", file);
            IsInGame        = GetValue(section, "IsInGame", file);
            ExecuteBuffer   = GetValue(section, "ExecBuff", file);
            InjectedAddress = GetValue(section, "Inj_Addr", file);
        }

        #region WinApi

        [DllImport("kernel32.dll")]
        static extern int GetPrivateProfileInt(string lpAppName, string lpKeyName, int nDefault, string lpFileName);

        private long GetValue(string section, string key, string file)
        {
            if (string.IsNullOrWhiteSpace(section))
                throw new ArgumentNullException("section");

            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException("key");

            var val = GetPrivateProfileInt(section, key, 0, file);

            if (val == 0L)
                throw new NullReferenceException("key");

            return val;
        }

        #endregion
    }
}