using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using Yanitta.Properties;

namespace Yanitta
{
    public partial class App : Application
    {
        static App()
        {
            ConsoleWriter.Initialize("Yanitta.log", true);
        }

        public static new MainWindow MainWindow
        {
            get { return App.Current.Windows.OfType<MainWindow>().FirstOrDefault(); }
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            if (e.Args.Length > 0 && e.Args[0] == "/e")
                StartupUri = new Uri("Windows/WinProfileEditor.xaml", UriKind.Relative);

            if (Settings.Default.Language != "auto")
                Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(Settings.Default.Language);

            Console.WriteLine("Yanitta startup!...");

            var fileName = Settings.Default.ProfilesFileName;
            if (File.Exists(fileName))
                File.Copy(fileName, fileName + ".bak", true);
            else if (File.Exists("Profiles.Original.xml"))
                File.Copy("Profiles.Original.xml", fileName, true);

            base.OnStartup(e);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            ProfileDb.Instance.Save(Settings.Default.ProfilesFileName);
            Console.WriteLine("Yanitta stoped ... !");
            ConsoleWriter.CloseWriter();
            base.OnExit(e);
        }
    }
}