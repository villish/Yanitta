using System;
using System.Collections.ObjectModel;
using System.Xml;
using System.Xml.Serialization;

namespace Yanitta
{
    /// <summary>
    /// Контейнер ротаций и способностей привязанный к конкретному классу <see cref="Yanitta.WowClass"/>.
    /// </summary>
    [Serializable]
    public class Profile
    {
        /// <summary>
        /// Класс персонажа.
        /// </summary>
        [XmlAttribute("Class")]
        public WowClass Class { get; set; }

        /// <summary>
        /// Код Lua привязанный к профилю.
        /// </summary>
        [XmlIgnore]
        public string Lua { get; set; }

        /// <summary>
        /// [not used] use for serialization.
        /// </summary>
        [XmlElement("Lua")]
        public XmlCDataSection _lua
        {
            get { return Extensions.CreateCDataSection(Lua); }
            set { Lua = Extensions.GetTrimValue(value); }
        }

        /// <summary>
        /// Список ротаций профиля.
        /// </summary>
        [XmlElement("Rotation")]
        public ObservableCollection<Rotation> RotationList { get; set; } = new ObservableCollection<Rotation>();
    }
}