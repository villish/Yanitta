using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace Yanitta
{
    [Serializable]
    public class TestElement
    {
        public string Name { get; set; }
        public string Description { get; set; }

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
    }
}
