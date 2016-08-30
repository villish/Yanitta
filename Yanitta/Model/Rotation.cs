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
    /// Rotation structure.
    /// </summary>
    [Serializable]
    public class Rotation : ViewModelBase
    {
        public Ability Current => CollectionViewSource.GetDefaultView(AbilityList)?.CurrentItem as Ability;

        string name = "none";
        /// <summary>
        /// Rotation name.
        /// </summary>
        [XmlAttribute]
        public string Name
        {
            get { return name; }
            set { Set(ref name, value); }
        }

        WowSpecializations spec;

        /// <summary>
        /// Rotation spec.
        /// </summary>
        [XmlAttribute]
        public WowSpecializations Spec
        {
            get { return spec; }
            set { Set(ref spec, value, "Spec", "ImageSource"); }
        }


        int inRangeSpell;
        /// <summary>
        /// Spell for range check.
        /// </summary>
        [XmlElement]
        public int InRangeSpell
        {
            get { return inRangeSpell; }
            set { Set(ref inRangeSpell, value); }
        }

        [XmlIgnore]
        public BitmapImage ImageSource => Extensions.GetIconFromEnum(spec);

        /// <summary>
        /// Lua code.
        /// </summary>
        [XmlIgnore]
        public string Lua { get; set; }

        /// <summary>
        /// HotKey for startup/stop rotation.
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
        /// Ability list.
        /// </summary>
        [XmlElement("Ability")]
        public ObservableCollection<Ability> AbilityList { get; set; } = new ObservableCollection<Ability>();

        /// <summary>
        /// Create deep copy from curent instance <see cref="Rotation"/>
        /// </summary>
        public Rotation Clone()
        {
            var rotation = new Rotation {
                Name   = Name + " (Copy)",
                Lua    = Lua,
                HotKey = new HotKey()
            };

            foreach (var ability in AbilityList)
                rotation.AbilityList.Add(ability.Clone());
            return rotation;
        }

        #region Commands

        public RelayCommand<object> Add { get; }

        public RelayCommand<object> Copy { get; }

        public RelayCommand<object> Delete { get; }

        public RelayCommand<object> Up { get; }

        public RelayCommand<object> Down { get; }

        public Rotation()
        {
            Add    = new RelayCommand<object>(_ => AbilityList.Add(new Ability {
                TargetList = new List<TargetType> { TargetType.None }
            }));
            Copy   = new RelayCommand<object>(_ => AbilityList.Add(Current.Clone()), _ => Current != null);
            Up     = new RelayCommand<object>(_ => Move(AbilityList,-1), _ => CanMove(AbilityList,-1));
            Down   = new RelayCommand<object>(_ => Move(AbilityList, 1), _ => CanMove(AbilityList, 1));
            Delete = new RelayCommand<object>(_ => {
                if (MessageBox.Show($"Do you remove the '{Current?.Name}' ability?",
                    "Yanitta", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    AbilityList.Remove(Current); }, _ => Current != null);
        }

        #endregion
    }
}