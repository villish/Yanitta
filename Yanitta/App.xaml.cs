using System;
using System.IO;
using System.Linq;
using System.Windows;

namespace Yanitta
{
    /// <summary>
    /// Логика взаимодействия для App.xaml
    /// </summary>
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

            Console.WriteLine("Yanitta startup!...");

            if (!File.Exists("FASM.DLL"))
                throw new FileNotFoundException("Not found", "FASM.DLL");

            var fileName = Yanitta.Properties.Settings.Default.ProfilesFileName;
            if (File.Exists(fileName))
                File.Copy(fileName, fileName + ".bak", true);

            Console.WriteLine(MemoryModule.ProcessMemory.FasmVersion);

            base.OnStartup(e);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            Console.WriteLine("Yanitta stoped ... !");
            ConsoleWriter.CloseWriter();
            base.OnExit(e);
        }
    }
}