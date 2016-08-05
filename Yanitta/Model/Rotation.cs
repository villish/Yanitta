using System;
using System.Collections.ObjectModel;
using System.Windows.Media.Imaging;
using System.Xml;
using System.Xml.Serialization;

namespace Yanitta
{
    /// <summary>
    /// Rotation structure.
    /// </summary>
    [Serializable]
    public class Rotation : ViewModelBase, ICloneable
    {
        string name = "none";
        /// <summary>
        /// Rotation name.
        /// </summary>
        [XmlAttribute]
        public string Name
        {
            get { return name; }
            set { Set(ref name, value); }
        }

        WowSpecializations spec;

        /// <summary>
        /// Rotation spec.
        /// </summary>
        [XmlAttribute]
        public WowSpecializations Spec
        {
            get { return spec; }
            set { Set(ref spec, value, "Spec", "ImageSource"); }
        }

        [XmlIgnore]
        public BitmapImage ImageSource => Extensions.GetIconFromEnum(spec);

        /// <summary>
        /// Lua code.
        /// </summary>
        [XmlIgnore]
        public string Lua { get; set; }

        /// <summary>
        /// HotKey for startup/stop rotation.
        /// </summary>
        [XmlElement]
        public HotKey HotKey { get; set; } = new HotKey();

        /// <summary>
        /// [not used] use for serialization.
        /// </summary>
        [XmlElement("Lua")]
        public XmlCDataSection _lua
        {
            get { return CreateCDataSection(Lua); }
            set { Lua = GetTrimValue(value); }
        }

        /// <summary>
        /// Ability list.
        /// </summary>
        [XmlElement("Ability")]
        public ObservableCollection<Ability> AbilityList { get; set; } = new ObservableCollection<Ability>();

        /// <summary>
        /// Create deep copy from curent instance <see cref="Rotation"/>
        /// </summary>
        public object Clone()
        {
            var rotation = new Rotation {
                Name   = Name + " (Copy)",
                Lua    = Lua,
                HotKey = new HotKey(),
            };

            foreach (var ability in AbilityList)
                rotation.AbilityList.Add(ability.Clone());
            return rotation;
        }
    }
}