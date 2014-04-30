using System;
using System.Collections.ObjectModel;
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
            get { return this.Lua.CreateCDataSection(); }
            set { this.Lua = value.GetTrimValue(); }
        }

        /// <summary>
        /// Список профилей.
        /// </summary>
        [XmlElement("Profile")]
        public ObservableCollection<Profile> ProfileList { get; set; }

        /// <summary>
        /// Профиль по умолчанию.
        /// Этот профиль содержит ротации доступные всем классам.
        /// </summary>
        [XmlIgnore]
        public Profile DefaultProfile
        {
            get { return this[WowClass.None]; }
        }

        /// <summary>
        /// Текущий экземпляр базы данных.
        /// </summary>
        public static ProfileDb Instance { get; set; }

        /// <summary>
        /// Инициализирует новый экземпляр объекта <see cref="Yanitta.ProfileDb"/>
        /// </summary>
        public ProfileDb()
        {
            this.ProfileList = new ObservableCollection<Profile>();
        }

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
        /// Сохраняет базу данных в файл.
        /// </summary>
        /// <param name="fileName">Имя файла базы данных.</param>
        /// <param name="incVersion">Указывает увеличивать ли версию базы данных.</param>
        public static void Save()
        {
            try
            {
                XmlManager.Save(Settings.Default.ProfilesFileName, ProfileDb.Instance);
                Console.WriteLine("Profiles Saved!");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: {0}", ex);
            }
        }
    }
}