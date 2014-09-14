using System;
using System.IO;
using System.Xml.Serialization;

namespace Yanitta
{
    [Serializable]
    public class Offsets
    {
        private const string fileName = "offsets.xml";

        [XmlElement]
        public int Build            { get; set; }

        /// <summary>
        /// Имя персонажа.
        ///
        /// Lua function UnitName - в самом конце функции
        /// v3 = sub_A30173(); -- GetPlayerName
        /// sub_4C7477(a1, v3);
        /// sub_4C72F7(a1);
        /// return 2;
        /// </summary>
        [XmlElement]
        public long PlayerName      { get; set; }

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
        [XmlElement]
        public long PlayerClass     { get; set; }

        /// <summary>
        /// Нахождение в мире
        ///
        /// Lua function PlaySound
        ///   if ( !(unsigned __int8)sub_40B039() && (!sub_7D875E() || byte_12AB65E /*IsInWorld*/) )
        /// </summary>
        [XmlElement]
        public long IsInGame        { get; set; }

        /// <summary>
        /// Адрес функции FrameScript::ExecuteBuffer
        ///
        /// Lua function RunScript
        /// </summary>
        [XmlElement]
        public long ExecuteBuffer   { get; set; }

        /// <summary>
        /// Адресс вставки байт кода
        ///
        /// По строке 'compat.lua' ищем функцию инициализации Lua
        /// Начало этой функции и есть адресом.
        /// </summary>
        [XmlElement]
        public long InjectedAddress { get; set; }

        static Offsets()
        {
            if (File.Exists(fileName))
                Default = XmlManager.Load<Offsets>(fileName);
            else
                Default = new Offsets();
        }

        public static Offsets Default { get; set; }

        public static void Save()
        {
            XmlManager.Save(fileName, Default);
        }
    }
}