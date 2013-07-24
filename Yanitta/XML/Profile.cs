using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Xml;
using System.Xml.Serialization;

namespace Yanitta
{
    /// <summary>
    ///
    /// </summary>
    [Serializable]
    public class Profile : ICloneable, INotifyPropertyChanged
    {
        private WowClass m_class;
        private string m_lua;
        private ObservableCollection<Ability> m_AbilityList;
        private ObservableCollection<Rotation> m_RotationList;

        /// <summary>
        ///
        /// </summary>
        [XmlAttribute("Class")]
        public WowClass Class
        {
            get { return this.m_class; }
            set
            {
                if (this.m_class != value)
                {
                    this.m_class = value;
                    OnPropertyChanged("Class");
                }
            }
        }

        [XmlIgnore]
        public string Lua
        {
            get { return m_lua ?? string.Empty; }
            set
            {
                if (m_lua != value)
                {
                    m_lua = value;
                    OnPropertyChanged("Lua");
                }
            }
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
        public ObservableCollection<Ability> AbilityList
        {
            get { return this.m_AbilityList; }
            set
            {
                if (this.m_AbilityList != value)
                {
                    this.m_AbilityList = value;
                    OnPropertyChanged("AbilityList");
                }
            }
        }

        /// <summary>
        ///
        /// </summary>
        public ObservableCollection<Rotation> RotationList
        {
            get { return this.m_RotationList; }
            set
            {
                if (this.m_RotationList != value)
                {
                    this.m_RotationList = value;
                    OnPropertyChanged("RotationList");
                }
            }
        }

        /// <summary>
        ///
        /// </summary>
        public Profile()
        {
            AbilityList = new ObservableCollection<Ability>();
            RotationList = new ObservableCollection<Rotation>();
        }

        public override string ToString()
        {
            return this.Class.ToString();
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public object Clone()
        {
            return this.MemberwiseClone();
        }

        public IEnumerable<Ability> this[Rotation rotation_filter]
        {
            get
            {
                foreach (var rotation in this.RotationList)
                {
                    if (rotation == rotation_filter)
                    {
                        foreach (var rotationName in rotation.AbilityQueue)
                        {
                            foreach (var ability in this.AbilityList)
                            {
                                if (ability.Name == rotationName)
                                    yield return ability;
                            }
                        }
                    }
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }
    }
}