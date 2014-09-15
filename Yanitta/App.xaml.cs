using System;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Linq;
using System.Windows;
using Yanitta.Properties;

namespace Yanitta
{
    public partial class App : Application
    {
        public static ProcessList ProcessList { get; set; }

        static App()
        {
            ProcessList = new ProcessList();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            ConsoleWriter.Initialize("Yanitta.log", true);

            this.Dispatcher.UnhandledException += (o, ex) => {
                if (ex.Exception is YanittaException)
                {
                    MessageBox.Show(ex.Exception.Message,
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    Console.WriteLine(ex.Exception.Message);
                    ex.Handled = true;
                }
            };

            if (e.Args.Length > 0 && e.Args[0] == "/e")
                StartupUri = new Uri("Windows/WinProfileEditor.xaml", UriKind.Relative);
            else if (e.Args.Length > 0 && e.Args[0] == "/ex")
            {
                StartupUri = new Uri("Windows/WinCodeExecute.xaml", UriKind.Relative);
                return;
            }

            if (Settings.Default.Language != "auto")
                Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(Settings.Default.Language);

            var fileName = Settings.Default.ProfilesFileName;
            Console.WriteLine("Yanitta startup!...");

            // Если файл профиля отсутствует, создаем новый (пустой) и запускаем редактор профилей.
            if (!File.Exists(fileName))
            {
                ProfileDb.Instance = new ProfileDb();
                File.WriteAllText(fileName, "");
                StartupUri = new Uri("Windows/WinProfileEditor.xaml", UriKind.Relative);
            }
            else
            {
                ProfileDb.Instance = XmlManager.Load<ProfileDb>(fileName);
                if (File.Exists(fileName))
                    File.Copy(fileName, fileName + ".bak", true);

                Console.WriteLine("База успешно загружена, резервная копия создана.");
            }

            base.OnStartup(e);
        }

        public static T ShowWindow<T>() where T : Window, new()
        {
            var window = App.Current.Windows.OfType<T>().FirstOrDefault() ?? new T();

            window.Show();

            if (!window.IsActive)
                window.Activate();

            if (window.WindowState == WindowState.Minimized)
                window.WindowState = WindowState.Normal;

            return window;
        }

        protected override void OnExit(ExitEventArgs e)
        {
            ProfileDb.Save();
            Console.WriteLine("Yanitta stoped ... !");
            ConsoleWriter.CloseWriter();
            base.OnExit(e);
        }
    }
}