using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;

namespace Yanitta
{
    /// <summary>
    ///
    /// </summary>
    [Serializable]
    public class Profile : DependencyObject
    {
        public static readonly DependencyProperty LuaProperty          = DependencyProperty.Register("Lua",          typeof(string),                         typeof(Profile));
        public static readonly DependencyProperty RotationListProperty = DependencyProperty.Register("RotationList", typeof(ObservableCollection<Rotation>), typeof(Profile));

        /// <summary>
        ///
        /// </summary>
        [XmlAttribute("Class")]
        public WowClass Class { get; set; }

        [XmlIgnore]
        public string Lua
        {
            get { return (string)(GetValue(LuaProperty) ?? ""); }
            set { SetValue(LuaProperty, value); }
        }

        [XmlElement("Lua")]
        public XmlCDataSection _lua
        {
            get { return new XmlDocument().CreateCDataSection(this.Lua ?? ""); }
            set { this.Lua = value.Value; }
        }

        /// <summary>
        ///
        /// </summary>
        public ObservableCollection<Rotation> RotationList
        {
            get { return (ObservableCollection<Rotation>)GetValue(RotationListProperty); }
            set { SetValue(RotationListProperty, value); }
        }

        public Profile()
        {
            RotationList = new ObservableCollection<Rotation>();
        }
    }
}