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
        public static readonly DependencyProperty NameProperty   = DependencyProperty.Register("Name",   typeof(string), typeof(Rotation));
        public static readonly DependencyProperty HotKeyProperty = DependencyProperty.Register("HotKey", typeof(HotKey), typeof(Rotation));

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
        /// Установка режима уведомления о проках
        /// </summary>
        [XmlAttribute]
        public bool ProcNotifyer { get; set; }

        /// <summary>
        /// Примечание по ротации
        /// </summary>
        [XmlIgnore]
        public string Notes { get; set; }

        /// <summary>
        /// Горячие клавиши для управления ротацией
        /// </summary>
        public HotKey HotKey
        {
            get { return (HotKey)GetValue(HotKeyProperty); }
            set { SetValue(HotKeyProperty, value); }
        }

        /// <summary>
        /// Ability queue
        /// </summary>
        public ObservableCollection<string> AbilityQueue { get; set; }

        /// <summary>
        ///
        /// </summary>
        public Rotation()
        {
            this.HotKey = new HotKey();
            this.AbilityQueue = new ObservableCollection<string>();
            this.Name = "<>";
        }

        /// <summary>
        /// [not used] use for serialisation
        /// </summary>
        [XmlElement("Notes")]
        public XmlCDataSection _rotationNotes
        {
            get { return new XmlDocument().CreateCDataSection(this.Notes ?? ""); }
            set { this.Notes = value.Value; }
        }

        public object Clone()
        {
            return new Rotation() {
                Name   = this.Name + " (1)",
                Notes  = this.Notes,
                HotKey = new HotKey(),
                ProcNotifyer = this.ProcNotifyer,
                AbilityQueue = new ObservableCollection<string>(this.AbilityQueue)
            };
        }
    }
}