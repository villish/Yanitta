using System;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.IO;

namespace Yanitta.Plugins
{
    public class PluginManager : IDisposable
    {
        public static PluginManager Instance { get; set; }

        [ImportMany(typeof(IYanittaPlugin))]
        public ObservableCollection<IYanittaPlugin> PluginList { get; set; }

        static PluginManager()
        {
            Instance = new PluginManager();
        }

        public static void ForEach(Action<IYanittaPlugin> predicate)
        {
            if (Instance != null)
            {
                foreach (IYanittaPlugin element in Instance.PluginList)
                    predicate(element);
            }
        }

        public static void Close()
        {
            if (Instance != null)
                Instance.Dispose();
        }

        public PluginManager()
        {
            var catalog = new AggregateCatalog();
            catalog.Catalogs.Add(new AssemblyCatalog(typeof(App).Assembly));
            catalog.Catalogs.Add(new DirectoryCatalog(Environment.CurrentDirectory));

            var pluginsPath = Path.Combine(Environment.CurrentDirectory, "Plugins");
            if (Directory.Exists(pluginsPath))
                catalog.Catalogs.Add(new DirectoryCatalog(pluginsPath));

            var container = new CompositionContainer(catalog);
            container.ComposeParts(this);
        }

        public void Dispose()
        {
            if (PluginList == null)
                return;
            PluginList.ForEach(n => n.Dispose());
        }
    }
}
