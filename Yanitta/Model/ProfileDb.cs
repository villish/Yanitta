using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using Yanitta.Properties;

namespace Yanitta
{
    /// <summary>
    /// База профилей.
    /// </summary>
    [Serializable]
    [XmlRoot("Profiles")]
    public class ProfileDb
    {
        /// <summary>
        /// Код ядра бота.
        /// </summary>
        [XmlIgnore]
        public string Lua { get; set; }

        /// <summary>
        /// [not used] use for serialization.
        /// </summary>
        [XmlElement("Lua")]
        public XmlCDataSection _lua
        {
            get { return Extensions.CreateCDataSection(Lua); }
            set { Lua = Extensions.GetTrimValue(value); }
        }

        /// <summary>
        /// Список профилей.
        /// </summary>
        [XmlElement("Profile")]
        public ObservableCollection<Profile> ProfileList { get; set; } = new ObservableCollection<Profile>();

        /// <summary>
        /// Текущий экземпляр базы данных.
        /// </summary>
        public static ProfileDb Instance { get; set; }

        /// <summary>
        /// Возвращает профиль для указанного класса.
        /// </summary>
        /// <param name="wowClass">Класс персонажа.</param>
        /// <returns>Профиль ротаций <see cref="YanittaProfile"/>.</returns>
        public Profile this[WowClass wowClass]
        {
            get { return ProfileList.FirstOrDefault(n => n.Class == wowClass); }
        }

        public void SetNotifyer(WowClass wowClass, NotifyCollectionChangedEventHandler eventHandler)
        {
            if (ProfileList.Any(n => n.Class == WowClass.None))
            {
                this[WowClass.None].RotationList.CollectionChanged -= eventHandler;
                this[WowClass.None].RotationList.CollectionChanged += eventHandler;
            }

            if (wowClass != WowClass.None)
            {
                if (ProfileList.Any(n => n.Class == wowClass))
                {
                    this[wowClass].RotationList.CollectionChanged -= eventHandler;
                    this[wowClass].RotationList.CollectionChanged += eventHandler;
                }
            }
        }

        /// <summary>
        /// Сохраняет базу данных в файл.
        /// </summary>
        /// <param name="fileName">Имя файла базы данных.</param>
        /// <param name="incVersion">Указывает увеличивать ли версию базы данных.</param>
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