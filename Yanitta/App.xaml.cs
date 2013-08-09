using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Yanitta.Plugins;

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
#warning Hack
            var pr = System.Diagnostics.Process.GetProcessesByName("yanitta")
                .Where((n) => n.Id != System.Diagnostics.Process.GetCurrentProcess().Id);

            foreach (var p in pr)
                p.Kill();

            Console.WriteLine("Yanitta startup!...");

            if (!File.Exists("FASM.DLL"))
            {
                MessageBox.Show("FASM.DLL not found", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                throw new FileNotFoundException("Not found", "FASM.DLL");
            }

            var fileName = Yanitta.Properties.Settings.Default.ProfilesFileName;
            if (File.Exists(fileName))
                File.Copy(fileName, fileName + ".bak", true);

            Console.WriteLine(MemoryModule.ProcessMemory.FasmVersion);



            base.OnStartup(e);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            Console.WriteLine("Yanitta stoped ... !");
            PluginManager.Close();
            ConsoleWriter.CloseWriter();
            Console.WriteLine("Yanitta stoped!");
            base.OnExit(e);
        }
    }
}