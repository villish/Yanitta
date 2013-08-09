using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Windows;
using System.Xml;
using System.Xml.Serialization;
using Yanitta.Properties;

namespace Yanitta
{
    [Serializable]
    public class ProfileDb : INotifyPropertyChanged, IDisposable
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
                var sw = new Stopwatch();
                sw.Start();
                var temp = new XmlManager(fileName).Load<ProfileDb>();
                sw.Stop();
                Console.WriteLine("Profiles loading: {0}", sw.Elapsed);
                sw.Reset();
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

        public void Save(string fileName, bool incVersion = false)
        {
            try
            {
                if (ProfileDb.Instance != null)
                {
                    if (incVersion)
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

        [XmlIgnore]
        public int RawVersion
        {
            get
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
                return hVersion;
            }
            set
            {
                this.Version = string.Format("{0}.{1}.{2}.{3}",
                    (value >> 24) & 0xFF,
                    (value >> 16) & 0xFF,
                    (value >> 08) & 0xFF,
                    (value >> 00) & 0xFF
                    );
            }
        }

        public void IncVersion()
        {
            ++RawVersion;
        }

        public static void UpdateProfiles()
        {
            if (string.IsNullOrWhiteSpace(ProfileDb.Instance.Url))
            {
                throw new Exception("Url is empty!");
            }

            using (var response = (HttpWebResponse)WebRequest.Create(ProfileDb.Instance.Url).GetResponse())
            {
                using (var stream = new StreamReader(response.GetResponseStream()))
                {
                    var profile = (ProfileDb)new XmlSerializer(typeof(ProfileDb)).Deserialize(stream);
                    if (profile.RawVersion > ProfileDb.Instance.RawVersion)
                    {
                        var question = string.Format("Обнаружен профиль v{0}, текущий v{1}.\r\nОбновить профиль?",
                            profile.Version, ProfileDb.Instance.Version);
                        var res = MessageBox.Show(question, "Обновление", MessageBoxButton.YesNo, MessageBoxImage.Information);
                        if (res == MessageBoxResult.Yes)
                        {
                            ProfileDb.Instance.Update(profile);
                            ProfileDb.Instance.Save(Settings.Default.ProfilesFileName);
                        }
                    }
                }
            }
        }

        public void Dispose()
        {
            if (ProfileList != null)
                ProfileList.ForEach((p) => p.Dispose());
        }
    }
}