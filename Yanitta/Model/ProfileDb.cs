using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Input;
using System.Xml;
using System.Xml.Serialization;

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

        static ProfileDb()
        {
            Instance = new ProfileDb();
        }

        /// <summary>
        /// Save batabase to file.
        /// </summary>
        public static void Save()
        {
            try
            {
                XmlManager.Save(Settings.ProfilePath, Instance);
                Console.WriteLine("Profiles Saved!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex}");
            }
        }

        public static void Load(string fileName)
        {
            if (File.Exists(fileName))
            {
                Instance = XmlManager.Load<ProfileDb>(fileName);
                if (File.Exists(fileName))
                    File.Copy(fileName, fileName + ".bak", true);
            }

            Instance.InitStructure();
        }

        void InitStructure()
        {
            Key[] key_list = { Key.X, Key.C, Key.V, Key.B, Key.N };

            foreach (WowClass wowClass in Enum.GetValues(typeof(WowClass)))
            {
                if (!ProfileList.Any(profile => profile.Class == wowClass))
                    ProfileList.Add(new Profile { Class = wowClass });
            }

            foreach (var profile in ProfileList)
            {
                foreach (WowSpecializations spec in Enum.GetValues(typeof(WowSpecializations)))
                {
                    if (spec.IsWowClass(profile.Class))
                    {
                        if (!profile.RotationList.Any(r => r.Spec == spec))
                        {
                            var rotation = new Rotation {
                                Spec = spec,
                                Name = spec.ToString().Split('_').Last(),
                                AbilityList = new ObservableCollection<Ability> {
                                    new Ability {
                                        TargetList = new List<TargetType> { TargetType.None }
                                    }
                                }
                            };

                            if (profile.RotationList.Count < key_list.Length)
                                rotation.HotKey = new HotKey(key_list[profile.RotationList.Count], ModifierKeys.Alt);
                            profile.RotationList.Add(rotation);
                        }
                    }
                }
            }

            var old = ProfileList.IndexOf(ProfileList.First(p => p.Class == WowClass.None));
            if (old > -1 && old != ProfileList.Count - 1)
                ProfileList.Move(old, ProfileList.Count - 1);
        }
    }
}