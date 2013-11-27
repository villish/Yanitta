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
            get { return this.Lua.CreateCDataSection(); }
            set { this.Lua = value.GetTrimValue(); }
        }

        public WowTest()
        {
            this.Name = "none";
            this.Lua  = "-- local spellList = { };";
        }
    }
}
