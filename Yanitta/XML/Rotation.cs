using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using System.Xml;
using System.Xml.Serialization;

namespace Yanitta
{
    /// <summary>
    ///
    /// </summary>
    [Serializable]
    public class Rotation : INotifyPropertyChanged, ICloneable
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private HotKey hotKey;
        private string name;

        /// <summary>
        /// Наименование ротации
        /// </summary>
        [XmlAttribute]
        public string Name
        {
            get { return name; }
            set
            {
                name = value;
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("Name"));
            }
        }

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
            get { return hotKey; }
            set
            {
                hotKey = value;
                this.hotKey.PropertyChanged += OnPropertyChanged;
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("HotKey"));
            }
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

        public override string ToString()
        {
            return this.Name;
        }

        public object Clone()
        {
            return new Rotation() {
                Name   = this.Name + " (1)",
                Notes  = this.Notes,
                HotKey = new HotKey(),

                AbilityQueue = new ObservableCollection<string>(this.AbilityQueue)
            };
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs("HotKey"));
        }
    }
}