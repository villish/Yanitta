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

        [XmlElement]
        public long PlayerName      { get; set; }

        [XmlElement]
        public long PlayerClass     { get; set; }

        [XmlElement]
        public long IsInGame        { get; set; }

        [XmlElement]
        public long ExecuteBuffer   { get; set; }

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