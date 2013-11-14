using System;
using System.Xml;
using System.Xml.Serialization;

namespace Yanitta
{
    [Serializable]
    public class WowTest
    {
        public string Name { get; set; }

        [XmlIgnore]
        public string Lua { get; set; }

        /// <summary>
        ///
        /// </summary>
        [XmlElement("Lua")]
        public XmlCDataSection _lua
        {
            get { return new XmlDocument().CreateCDataSection(this.Lua ?? ""); }
            set { this.Lua = value.Value; }
        }

        public WowTest()
        {
            this.Name = "none";
            this.Lua  = "-- local spellList = { };";
        }
    }
}
