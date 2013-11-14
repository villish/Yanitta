using System;
using System.Collections.ObjectModel;
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
    public class ProfileDb : DependencyObject
    {
        public static readonly DependencyProperty CoreProperty          = DependencyProperty.Register("Core",        typeof(string), typeof(ProfileDb));
        public static readonly DependencyProperty FuncProperty          = DependencyProperty.Register("Func",        typeof(string), typeof(ProfileDb));
        public static readonly DependencyProperty VersionProperty       = DependencyProperty.Register("Version",     typeof(string), typeof(ProfileDb));
        public static readonly DependencyProperty AuthorProperty        = DependencyProperty.Register("Author",      typeof(string), typeof(ProfileDb));
        public static readonly DependencyProperty UrlProperty           = DependencyProperty.Register("Url",         typeof(string), typeof(ProfileDb));
        public static readonly DependencyProperty ProfileListProperty   = DependencyProperty.Register("ProfileList", typeof(ObservableCollection<Profile>), typeof(ProfileDb));
        public static readonly DependencyProperty WowTestListProperty   = DependencyProperty.Register("WowTestList", typeof(ObservableCollection<WowTest>), typeof(ProfileDb));

        [XmlElement]
        public string Version
        {
            get { return (string)GetValue(VersionProperty); }
            set { SetValue(VersionProperty, value); }
        }

        [XmlElement]
        public string Author
        {
            get { return (string)GetValue(AuthorProperty); }
            set { SetValue(AuthorProperty, value); }
        }

        [XmlElement]
        public string Url
        {
            get { return (string)GetValue(UrlProperty); }
            set { SetValue(UrlProperty, value); }
        }

        [XmlIgnore]
        public string Core
        {
            get { return (string)GetValue(CoreProperty); }
            set { SetValue(CoreProperty, value); }
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
            get { return (string)GetValue(FuncProperty); }
            set { SetValue(FuncProperty, value); }
        }

        [XmlElement("Func")]
        public XmlCDataSection _func
        {
            get { return new XmlDocument().CreateCDataSection(this.Func ?? ""); }
            set { this.Func = value.Value; }
        }

        [XmlArray]
        public ObservableCollection<WowTest> WowTestList
        {
            get { return (ObservableCollection<WowTest>)GetValue(WowTestListProperty); }
            set { SetValue(WowTestListProperty, value); }
        }

        [XmlElement("Profile")]
        public ObservableCollection<Profile> ProfileList
        {
            get { return (ObservableCollection<Profile>)GetValue(ProfileListProperty); }
            set { SetValue(ProfileListProperty, value); }
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
            this.WowTestList = new ObservableCollection<WowTest>();
        }

        #region Extension

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
            this.WowTestList = temp.WowTestList;
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

        #endregion
    }
}