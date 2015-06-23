using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
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
            get { return Extensions.CreateCDataSection(this.Lua); }
            set { this.Lua = Extensions.GetTrimValue(value); }
        }

        /// <summary>
        /// Список ротаций профиля.
        /// </summary>
        [XmlElement("Rotation")]
        public ObservableCollection<Rotation> RotationList { get; set; }

        /// <summary>
        /// Создает новый экземпляр класса <see cref="Yanitta.Profile"/>
        /// </summary>
        public Profile()
        {
            this.RotationList = new ObservableCollection<Rotation>();
        }
    }
}