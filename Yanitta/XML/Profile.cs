using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml;
using System.Xml.Serialization;

namespace Yanitta
{
    /// <summary>
    /// Контейнер ротаций и способностей привязанный к конкретному классу <see cref="Yanitta.WowClass"/>.
    /// </summary>
    [Serializable]
    public class Profile
    {
        /// <summary>
        /// Класс персонажа.
        /// </summary>
        [XmlAttribute("Class")]
        public WowClass Class { get; set; }

        /// <summary>
        /// Код Lua привязанный к профилю.
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
        /// Список ротаций профиля.
        /// </summary>
        [XmlElement("Rotation")]
        public ObservableCollection<Rotation> RotationList { get; set; }

        /// <summary>
        /// Создает новый экземпляр класса <see cref="Yanitta.Profile"/>
        /// </summary>
        public Profile()
        {
            RotationList = new ObservableCollection<Rotation>();
        }

        /// <summary>
        /// Отменяет регистрацию гарячих клавиш для всех ротаций текущего профиля.
        /// </summary>
        public void UnregisterHotKeys()
        {
            foreach (var rotation in this.RotationList)
            {
                if (rotation.HotKey != null && rotation.HotKey.IsRegistered)
                    rotation.HotKey.Unregister();
            }
        }

        /// <summary>
        /// Регистрирует гарячие клавиши для всех ротаций текущего профиля.
        /// </summary>
        /// <param name="handler">Обрабочик срабатывания гарячих клавиш.</param>
        public void RegisterHotKeys(EventHandler<HandledEventArgs> handler)
        {
            foreach (var rotation in this.RotationList)
            {
                Debug.Assert(rotation.HotKey != null);
                if (!rotation.HotKey.IsRegistered)
                {
                    rotation.HotKey.Tag = rotation;
                    rotation.HotKey.Pressed -= handler;
                    rotation.HotKey.Pressed += handler;
                    try
                    {
                        rotation.HotKey.Register();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("HotKey Error: " + ex.Message);
                    }
                }
            }
        }
    }
}