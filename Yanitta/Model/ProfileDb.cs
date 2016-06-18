using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using Yanitta.Properties;

namespace Yanitta
{
    /// <summary>
    /// Profile DataBase.
    /// </summary>
    [Serializable]
    [XmlRoot("Profiles")]
    public class ProfileDb : ViewModelBase
    {
        /// <summary>
        /// Main Lua code.
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

        /// <summary>
        /// Profile list.
        /// </summary>
        [XmlElement("Profile")]
        public ObservableCollection<Profile> ProfileList { get; set; } = new ObservableCollection<Profile>();

        /// <summary>
        /// Current database instance.
        /// </summary>
        public static ProfileDb Instance { get; set; }

        /// <summary>
        /// Return current <see cref="Profile"/>.
        /// </summary>
        /// <param name="wowClass">Character's <see cref="WowClass"></param>
        /// <returns><see cref="Profile"/>.</returns>
        public Profile this[WowClass wowClass] => ProfileList.FirstOrDefault(n => n.Class == wowClass);

        /// <summary>
        /// Save batabase to file.
        /// </summary>
        public static void Save()
        {
            try
            {
                XmlManager.Save(Settings.Default.ProfilesFileName, Instance);
                Console.WriteLine("Profiles Saved!");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: {0}", ex);
            }
        }
    }
}