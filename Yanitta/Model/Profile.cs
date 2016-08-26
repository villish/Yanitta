using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Data;
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
        public Rotation Current => CollectionViewSource.GetDefaultView(RotationList)?.CurrentItem as Rotation;

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
        public IEnumerable<WowSpecializations> SpecList => Class.GetSpecList();

        #region Commands

        public RelayCommand<object> Add { get; }

        public RelayCommand<object> Copy { get; }

        public RelayCommand<object> Delete { get; }

        public RelayCommand<object> Up { get; }

        public RelayCommand<object> Down { get; }

        public Profile()
        {
            Add    = new RelayCommand<object>(_ => RotationList.Add(new Rotation {
                AbilityList = new ObservableCollection<Ability> {
                    new Ability { TargetList = new List<TargetType> { TargetType.None }
                    }
                }
            }));
            Copy   = new RelayCommand<object>(_ => RotationList.Add(Current.Clone()), _ => Current != null);
            Up     = new RelayCommand<object>(_ => Move(RotationList,-1), _ => CanMove(RotationList,-1));
            Down   = new RelayCommand<object>(_ => Move(RotationList, 1), _ => CanMove(RotationList, 1));
            Delete = new RelayCommand<object>(_ => {
                if (MessageBox.Show($"Do you remove the '{Current?.Name}' rotation?",
                    "Yanitta", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    RotationList.Remove(Current);
            }, _ => Current != null);
        }

        #endregion
    }
}