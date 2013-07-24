using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;

namespace Yanitta
{
    [Serializable]
    public class ProfileDb : INotifyPropertyChanged
    {
        private string m_core;
        private string m_func;
        private string m_version;
        private string m_author;
        private string m_url;
        private ObservableCollection<Profile> m_profileList;

        [XmlElement]
        [DefaultValue("0.0.0.1")]
        public string Version
        {
            get { return m_version ?? string.Empty; }
            set
            {
                if (m_version != value)
                {
                    m_version = value;
                    OnPropertyChanged("Version");
                }
            }
        }

        [XmlElement]
        public string Author
        {
            get { return m_author ?? string.Empty; }
            set
            {
                if (m_author != value)
                {
                    m_author = value;
                    OnPropertyChanged("Author");
                }
            }
        }

        [XmlElement]
        public string Url
        {
            get { return m_url ?? string.Empty; }
            set
            {
                if (m_url != value)
                {
                    m_url = value;
                    OnPropertyChanged("Url");
                }
            }
        }

        [XmlIgnore]
        public string Core
        {
            get { return m_core ?? string.Empty; }
            set
            {
                if (m_core != value)
                {
                    m_core = value;
                    OnPropertyChanged("Core");
                }
            }
        }

        [XmlElement("Core")]
        public XmlCDataSection _core
        {
            get { return new XmlDocument().CreateCDataSection(this.Core ?? ""); }
            set { this.Core = value.Value; }
        }

        [XmlIgnore]
        public string Func
        {
            get { return m_func ?? string.Empty; }
            set
            {
                if (m_func != value)
                {
                    m_func = value;
                    OnPropertyChanged("Func");
                }
            }
        }

        [XmlElement("Func")]
        public XmlCDataSection _func
        {
            get { return new XmlDocument().CreateCDataSection(this.Func ?? ""); }
            set { this.Func = value.Value; }
        }

        [XmlElement("Profile")]
        public ObservableCollection<Profile> ProfileList
        {
            get { return this.m_profileList; }
            set
            {
                if (this.m_profileList != value)
                {
                    this.m_profileList = value;
                    OnPropertyChanged("ProfileList");
                }
            }
        }

        public static ProfileDb Instance { get; private set; }

        static ProfileDb()
        {
            Instance = new ProfileDb();
            Instance.Load(Yanitta.Properties.Settings.Default.ProfilesFileName);
        }

        public ProfileDb()
        {
            this.Version     = "0.0.0.1";
            this.ProfileList = new ObservableCollection<Profile>();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }

        public Profile this[WowClass wowClass]
        {
            get { return this.ProfileList.FirstOrDefault(n => n.Class == wowClass); }
        }

        public void Exec(Action<Profile, Rotation> predicate)
        {
            foreach (var profile in this.ProfileList)
            {
                foreach (var rotation in profile.RotationList)
                {
                    predicate(profile, rotation);
                }
            }
        }

        public void Update(ProfileDb temp)
        {
            this.Version    = temp.Version;
            this.Author     = temp.Author;
            this.Core       = temp.Core;
            this.Func       = temp.Func;
            this.Url        = temp.Url;
            this.ProfileList = temp.ProfileList;
        }

        public void Load(string fileName)
        {
            try
            {
                var temp = new XmlManager(fileName).Load<ProfileDb>();
                ProfileDb.Instance.Update(temp);
            }
            catch (FileNotFoundException fex)
            {
                Console.WriteLine("File {0} not found...", fex.FileName);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: {0}", ex.Message);
            }
        }

        public void Save(string fileName)
        {
            try
            {
                if (ProfileDb.Instance != null)
                {
                    IncVersion();
                    new XmlManager(fileName).Save<ProfileDb>(ProfileDb.Instance);
                    Console.WriteLine("Profiles Saved!");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: {0}", ex);
            }
        }

        public void IncVersion()
        {
            int hVersion = 0;
            var tiles = this.Version.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(n => {
                    int t;
                    int.TryParse(n, out t);
                    return t;
                }).ToList();

            for (int i = 3; i >= 0; --i)
                hVersion |= (tiles.Count > i ? (tiles[i] & 0xFF) : 0) << (24 - (i * 8));

            ++hVersion;

            this.Version = string.Format("{0}.{1}.{2}.{3}",
                (hVersion >> 24) & 0xFF,
                (hVersion >> 16) & 0xFF,
                (hVersion >> 08) & 0xFF,
                (hVersion >> 00) & 0xFF
                );
        }
    }
}