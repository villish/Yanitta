using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Xml;
using System.Xml.Serialization;

namespace Yanitta
{
    /// <summary>
    /// Контейнер ротаций и способностей привязанный к конкретному классу <see cref="Yanitta.WowClass"/>.
    /// </summary>
    [Serializable]
    public class Profile : DependencyObject
    {
        public static readonly DependencyProperty LuaProperty          = DependencyProperty.Register("Lua",          typeof(string),                         typeof(Profile));
        public static readonly DependencyProperty RotationListProperty = DependencyProperty.Register("RotationList", typeof(ObservableCollection<Rotation>), typeof(Profile));

        /// <summary>
        /// Класс персонажа.
        /// </summary>
        [XmlAttribute("Class")]
        public WowClass Class { get; set; }

        /// <summary>
        /// Код Lua привязанный к профилю.
        /// </summary>
        [XmlIgnore]
        public string Lua
        {
            get { return (string)(GetValue(LuaProperty) ?? ""); }
            set { SetValue(LuaProperty, value); }
        }

        /// <summary>
        /// [not used] use for serialization.
        /// </summary>
        [XmlElement("Lua")]
        public XmlCDataSection _lua
        {
            get { return this.Lua.CreateCDataSection(); }
            set { this.Lua = value.GetTrimValue(); }
        }

        /// <summary>
        /// Список ротаций профиля.
        /// </summary>
        [XmlElement("Rotation")]
        public ObservableCollection<Rotation> RotationList
        {
            get { return (ObservableCollection<Rotation>)GetValue(RotationListProperty); }
            set { SetValue(RotationListProperty, value); }
        }

        /// <summary>
        /// Создает новый экземпляр класса <see cref="Yanitta.Profile"/>
        /// </summary>
        public Profile()
        {
            RotationList = new ObservableCollection<Rotation>();
        }
    }
}