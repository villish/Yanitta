using System;
using System.Windows;
using System.Windows.Input;

namespace Yanitta.Plugins
{
    /// <summary>
    ///
    /// </summary>
    public interface IYanittaPlugin : IDisposable
    {
        /// <summary>
        /// Версия плагина
        /// </summary>
        int Version { get; }

        /// <summary>
        /// Наименование плагина
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Описание плагина
        /// </summary>
        string Description { get; }

        /// <summary>
        /// Указывает, запущен ли плагин
        /// </summary>
        bool IsRuning { get; }

        /// <summary>
        /// Гарячие клавиши для управления дополнением.
        /// </summary>
        HotKey HotKey { get; }

        /// <summary>
        /// Визуальная панель с настройками
        /// </summary>
        void ShowSettingWindow(Style style);

        /// <summary>
        /// Читает память клиента
        /// </summary>
        /// <param name="memory">Память процесса</param>
        void ReadMemory(WowMemory memory);
    }
}