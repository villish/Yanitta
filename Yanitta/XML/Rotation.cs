using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
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
        private string name;
        /// <summary>
        /// Наименование ротации.
        /// </summary>
        [XmlAttribute]
        public string Name
        {
            get { return this.name; }
            set
            {
                if (this.name != value)
                {
                    this.name = value;
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
        public HotKey HotKey { get; set; }

        /// <summary>
        /// Создает новый экземпляр класс <see cref="Yanitta.Rotation"/>.
        /// </summary>
        public Rotation()
        {
            this.HotKey      = new HotKey();
            this.AbilityList = new ObservableCollection<Ability>();
            this.name        = "none";
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
        /// Список способностей в порядке их приоритета.
        /// </summary>
        [XmlElement("Ability")]
        public ObservableCollection<Ability> AbilityList { get; set; }

        /// <summary>
        /// Создает новый экземпляр класса с текщими значениями.
        /// </summary>
        public object Clone()
        {
            var rotation = new Rotation {
                Name   = this.Name + " (1)",
                Lua    = this.Lua,
                HotKey = new HotKey(),
            };

            foreach (var ability in this.AbilityList)
                rotation.AbilityList.Add((Ability)ability.Clone());
            return rotation;
        }
    }
}