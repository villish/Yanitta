using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Media.Imaging;
using System.Xml;
using System.Xml.Serialization;

namespace Yanitta
{
    /// <summary>
    /// Profile structure.
    /// </summary>
    [Serializable]
    public class Profile : ViewModelBase
    {
        /// <summary>
        /// Character's class.
        /// </summary>
        [XmlAttribute("Class")]
        public WowClass Class { get; set; }

        /// <summary>
        /// Lua code.
        /// </summary>
        [XmlIgnore]
        public string Lua { get; set; }

        /// <summary>
        /// [not used] use for serialization.
        /// </summary>
        [XmlElement("Lua")]
        public XmlCDataSection _lua
        {
            get { return CreateCDataSection(Lua); }
            set { Lua = GetTrimValue(value); }
        }

        [XmlIgnore]
        public BitmapImage ImageSource => Extensions.GetIconFromEnum(Class);

        /// <summary>
        /// Rotation list.
        /// </summary>
        [XmlElement("Rotation")]
        public ObservableCollection<Rotation> RotationList { get; set; } = new ObservableCollection<Rotation>();

        [XmlIgnore]
        public IEnumerable<WowSpecializations> SpecList
        {
            get
            {
                foreach (WowSpecializations spec in Enum.GetValues(typeof(WowSpecializations)))
                    if ((int)spec >> 16 == (byte)Class || spec == WowSpecializations.None)
                        yield return spec;
            }
        }

        #region Commands

        public RelayCommand<object> Add { get; }

        public RelayCommand<Rotation> Copy { get; }

        public RelayCommand<Rotation> Delete { get; }

        public RelayCommand<object> Up { get; }

        public RelayCommand<object> Down { get; }

        public Profile()
        {
            Add    = new RelayCommand<object>(  _ => RotationList.Add(new Rotation()));
            Delete = new RelayCommand<Rotation>(r => RotationList.Remove(r),      r => r == null);
            Copy   = new RelayCommand<Rotation>(r => RotationList.Add(r.Clone()), r => r == null);
        }

        #endregion
    }
}