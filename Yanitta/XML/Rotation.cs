using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using System.Xml;
using System.Xml.Serialization;

namespace Yanitta
{
    /// <summary>
    ///
    /// </summary>
    [Serializable]
    public class Rotation : DependencyObject, ICloneable
    {
        public static readonly DependencyProperty NameProperty        = DependencyProperty.Register("Name",        typeof(string),                        typeof(Rotation));
        public static readonly DependencyProperty HotKeyProperty      = DependencyProperty.Register("HotKey",      typeof(HotKey),                        typeof(Rotation));
        public static readonly DependencyProperty AbilityListProperty = DependencyProperty.Register("AbilityList", typeof(ObservableCollection<Ability>), typeof(Rotation));
        /// <summary>
        /// Наименование ротации
        /// </summary>
        [XmlAttribute]
        public string Name
        {
            get { return (string)GetValue(NameProperty); }
            set { SetValue(NameProperty, value); }
        }

        /// <summary>
        /// Примечание по ротации
        /// </summary>
        [XmlIgnore]
        public string Notes { get; set; }

        /// <summary>
        /// Rotation code
        /// </summary>
        [XmlIgnore]
        public string Lua { get; set; }

        /// <summary>
        /// Горячие клавиши для управления ротацией
        /// </summary>
        public HotKey HotKey
        {
            get { return (HotKey)GetValue(HotKeyProperty); }
            set { SetValue(HotKeyProperty, value); }
        }

        /// <summary>
        ///
        /// </summary>
        public Rotation()
        {
            this.HotKey         = new HotKey();
            this.AbilityList    = new ObservableCollection<Ability>();
            this.Name           = "none";
        }

        /// <summary>
        /// [not used] use for serialisation
        /// </summary>
        [XmlElement("Notes")]
        public XmlCDataSection _rotationNotes
        {
            get { return this.Notes.CreateCDataSection(); }
            set { this.Notes = value.GetTrimValue(); }
        }

        /// <summary>
        /// [not used] use for serialisation
        /// </summary>
        [XmlElement("Lua")]
        public XmlCDataSection _lua
        {
            get { return this.Lua.CreateCDataSection(); }
            set { this.Lua = value.GetTrimValue(); }
        }

        /// <summary>
        /// Список способностей в порядке их приоритета
        /// </summary>
        public ObservableCollection<Ability> AbilityList
        {
            get { return (ObservableCollection<Ability>)GetValue(AbilityListProperty); }
            set { SetValue(AbilityListProperty, value); }
        }

        public object Clone()
        {
            var rotation = new Rotation() {
                Name         = this.Name + " (1)",
                Notes        = this.Notes,
                Lua          = this.Lua,
                HotKey       = new HotKey(),
            };

            foreach (var ability in this.AbilityList)
                rotation.AbilityList.Add((Ability)ability.Clone());
            return rotation;
        }
    }
}