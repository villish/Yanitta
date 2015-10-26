using System;
using System.Collections.ObjectModel;
using System.Xml;
using System.Xml.Serialization;

namespace Yanitta
{
    /// <summary>
    /// Боевая ротация для персонажа.
    /// </summary>
    [Serializable]
    public class Rotation : ViewModelBase, ICloneable
    {
        string name = "none";
        /// <summary>
        /// Наименование ротации.
        /// </summary>
        [XmlAttribute]
        public string Name
        {
            get { return name; }
            set
            {
                if (name != value)
                {
                    name = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Код Lua привязанный к ротации.
        /// </summary>
        [XmlIgnore]
        public string Lua { get; set; }

        /// <summary>
        /// Горячие клавиши для управления ротацией.
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
        /// Список способностей в порядке их приоритета.
        /// </summary>
        [XmlElement("Ability")]
        public ObservableCollection<Ability> AbilityList { get; set; } = new ObservableCollection<Ability>();

        /// <summary>
        /// Создает новый экземпляр класса с текщими значениями.
        /// </summary>
        public object Clone()
        {
            var rotation = new Rotation {
                Name   = Name + " (Копия)",
                Lua    = Lua,
                HotKey = new HotKey(),
            };

            foreach (var ability in AbilityList)
                rotation.AbilityList.Add(ability.Clone());
            return rotation;
        }
    }
}