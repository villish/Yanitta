using System;
using System.Linq;
using System.Windows;

namespace Yanitta
{
    public partial class App : Application
    {
        public static ProcessList ProcessList { get; set; } = new ProcessList();

        protected override void OnStartup(StartupEventArgs e)
        {
            ConsoleWriter.Initialize("Yanitta.log", true);

            Dispatcher.UnhandledException += (o, ex) => {
                if (ex.Exception is YanittaException)
                {
                    MessageBox.Show(ex.Exception.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    Console.WriteLine(ex.Exception.Message);
                    ex.Handled = true;
                }
            };

            if (e.Args.Length > 0 && e.Args[0] == "/editor")
            {
                StartupUri = new Uri("Windows/WinProfileEditor.xaml", UriKind.Relative);
                ProfileDb.Load(Settings.ProfilePath);
            }
            else if (e.Args.Length > 0 && e.Args[0] == "/console")
            {
                StartupUri = new Uri("Windows/WinCodeExecute.xaml", UriKind.Relative);
            }
            else
            {
                ProfileDb.Load(Settings.ProfilePath);
            }
            base.OnStartup(e);
        }

        public static T ShowWindow<T>() where T : Window, new()
        {
            var window = Current.Windows.OfType<T>().FirstOrDefault() ?? new T();

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
            ConsoleWriter.CloseWriter();
            base.OnExit(e);
        }
    }
}