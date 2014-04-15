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
    /// <summary>
    /// База профилей.
    /// </summary>
    [Serializable]
    public class ProfileDb : DependencyObject
    {
        public static readonly DependencyProperty LuaProperty            = DependencyProperty.Register("Lua",            typeof(string), typeof(ProfileDb));
        public static readonly DependencyProperty VersionProperty        = DependencyProperty.Register("Version",        typeof(string), typeof(ProfileDb));
        public static readonly DependencyProperty AuthorProperty         = DependencyProperty.Register("Author",         typeof(string), typeof(ProfileDb));
        public static readonly DependencyProperty UrlProperty            = DependencyProperty.Register("Url",            typeof(string), typeof(ProfileDb));
        public static readonly DependencyProperty ProfileListProperty    = DependencyProperty.Register("ProfileList",    typeof(ObservableCollection<Profile>), typeof(ProfileDb));
        public static readonly DependencyProperty DefaultProfileProperty = DependencyProperty.Register("DefaultProfile", typeof(Profile), typeof(ProfileDb));

        /// <summary>
        /// Веррсия базы данных.
        /// </summary>
        [XmlElement]
        public string Version
        {
            get { return (string)GetValue(VersionProperty); }
            set { SetValue(VersionProperty, value); }
        }

        /// <summary>
        /// Автор.
        /// </summary>
        [XmlElement]
        public string Author
        {
            get { return (string)GetValue(AuthorProperty); }
            set { SetValue(AuthorProperty, value); }
        }

        /// <summary>
        /// Аддресс хранилища с обновлениями базы.
        /// </summary>
        [XmlElement]
        public string Url
        {
            get { return (string)GetValue(UrlProperty); }
            set { SetValue(UrlProperty, value); }
        }

        /// <summary>
        /// Код ядра бота.
        /// </summary>
        [XmlIgnore]
        public string Lua
        {
            get { return (string)GetValue(LuaProperty); }
            set { SetValue(LuaProperty, value); }
        }

        /// <summary>
        /// [not used] use for serialization.
        /// </summary>
        [XmlElement("Lua")]
        public XmlCDataSection _lua
        {
            get { return this.Lua.CreateCDataSection(); }
            set { this.Lua = value.GetTrimValue(); }
        }

        /// <summary>
        /// Список профилей.
        /// </summary>
        [XmlElement("Profile")]
        public ObservableCollection<Profile> ProfileList
        {
            get { return (ObservableCollection<Profile>)GetValue(ProfileListProperty); }
            set { SetValue(ProfileListProperty, value); }
        }

        /// <summary>
        /// Профиль по умолчанию.
        /// Этот профиль содержит ротации доступные всем классам.
        /// </summary>
        [XmlIgnore]
        public Profile DefaultProfile
        {
            get { return (Profile)GetValue(DefaultProfileProperty); }
            private set { SetValue(DefaultProfileProperty, value);  }
        }

        /// <summary>
        /// Текущий экземпляр базы данных.
        /// </summary>
        public static ProfileDb Instance { get; private set; }

        /// <summary>
        /// Инициализирует базу при первом обращении к классу.
        /// </summary>
        static ProfileDb()
        {
            Instance = new ProfileDb();
            Instance.Load(Yanitta.Properties.Settings.Default.ProfilesFileName);
        }

        /// <summary>
        /// Инициализирует новый экземпляр объекта <see cref="Yanitta.ProfileDb"/>
        /// </summary>
        public ProfileDb()
        {
            this.RawVersion  = new Version();
            this.ProfileList = new ObservableCollection<Profile>();
            this.DefaultProfile = new Profile();
        }

        #region Extension

        /// <summary>
        /// Возвращает профиль для указанного класса.
        /// </summary>
        /// <param name="wowClass">Класс персонажа.</param>
        /// <returns>Профиль ротаций <see cref="YanittaProfile"/>.</returns>
        public Profile this[WowClass wowClass]
        {
            get { return this.ProfileList.FirstOrDefault(n => n.Class == wowClass); }
        }

        /// <summary>
        /// Загружает значения полей из указанного объекта.
        /// </summary>
        /// <param name="temp">Объект из которого надо скопировать значения.</param>
        public void Update(ProfileDb temp)
        {
            this.Version     = temp.Version;
            this.Author      = temp.Author;
            this.Lua         = temp.Lua;
            this.Url         = temp.Url;
            this.ProfileList = temp.ProfileList;
            this.DefaultProfile = temp[WowClass.None] ?? new Profile();
        }

        /// <summary>
        /// Загружает базу данных из файла.
        /// </summary>
        /// <param name="fileName">Имя файла базы данных.</param>
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

        /// <summary>
        /// Сохраняет базу данных в файл.
        /// </summary>
        /// <param name="fileName">Имя файла базы данных.</param>
        /// <param name="incVersion">Указывает увеличивать ли версию базы данных.</param>
        public void Save(string fileName, bool incVersion = false)
        {
            try
            {
                if (ProfileDb.Instance != null)
                {
                    if (incVersion)
                    {
                        this.RawVersion = new Version(
                            this.RawVersion.Major,
                            this.RawVersion.Minor,
                            this.RawVersion.Build,
                            this.RawVersion.Revision + 1
                            );
                    }
                    new XmlManager(fileName).Save(ProfileDb.Instance);
                    Console.WriteLine("Profiles Saved!");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: {0}", ex);
            }
        }

        /// <summary>
        /// Служебное поле (временно).
        /// </summary>
        [XmlIgnore]
        public Version RawVersion
        {
            get { return new Version(this.Version); }
            set { this.Version = value.ToString(); }
        }

        /// <summary>
        /// Запрос обновления базы данных с указанного в настройках аддресса.
        /// </summary>
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