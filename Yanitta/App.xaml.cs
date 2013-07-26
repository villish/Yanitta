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

        public static MainWindow MainWindow
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

            Console.WriteLine("using Fasm v{0}", MemoryModule.ProcessMemory.FasmVersion);
            LoadPlugins();
            base.OnStartup(e);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            Console.WriteLine("Yanitta stoped!");
            ConsoleWriter.Close();
            base.OnExit(e);
            // hack
            Process.GetCurrentProcess().Kill();
        }

        private void LoadPlugins()
        {
            Console.WriteLine("Start loading plugins...");
            try
            {
                var catalog = new AggregateCatalog();
                catalog.Catalogs.Add(new AssemblyCatalog(typeof(App).Assembly));
                catalog.Catalogs.Add(new DirectoryCatalog(Environment.CurrentDirectory));

                if (Directory.Exists(Path.Combine(Environment.CurrentDirectory, "Plugins")))
                    catalog.Catalogs.Add(new DirectoryCatalog(Path.Combine(Environment.CurrentDirectory, "Plugins")));

                var container = new CompositionContainer(catalog);
                container.ComposeParts(this);

                foreach (var plugin in PluginList)
                {
                    Console.WriteLine("Loading plugin: {0} version: {1}", plugin.Name, plugin.Version);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: Load plugins: {0}", ex.Message);
            }
        }
    }
}