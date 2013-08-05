using System;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using Yanitta.Plugins;

namespace Yanitta
{
    /// <summary>
    /// Логика взаимодействия для App.xaml
    /// </summary>
    public partial class App : Application
    {
        [ImportMany(typeof(IYanittaPlugin))]
        public ObservableCollection<IYanittaPlugin> PluginList;

        static App()
        {
            ConsoleWriter.Initialize(true);
        }

        public static new MainWindow MainWindow
        {
            get { return App.Current.Windows.OfType<MainWindow>().FirstOrDefault(); }
        }

        protected override void OnStartup(StartupEventArgs e)
        {
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
            Console.WriteLine("Yanitta stoped!");
            ConsoleWriter.Close();
            if (PluginManager.Instance != null)
                PluginManager.Instance.Dispose();

            base.OnExit(e);
            // hack
            Process.GetCurrentProcess().Kill();
        }
    }
}