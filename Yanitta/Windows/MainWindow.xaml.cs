using Microsoft.Windows.Controls;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using Yanitta.Plugins;
using Yanitta.Windows;

namespace Yanitta
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private WinCodeExecute   codeExecuteWindow = null;
        private WinProfileEditor profileWindows = null;
        private WindowSettings   settingWindow = null;

        public static ProcessList ProcessList { get; set; }
        public static ObservableCollection<IYanittaPlugin> PluginList { get; set; }

        public TaskbarIcon TaskbarIcon
        {
            get { return notyfyIcon; }
        }

        static MainWindow()
        {
            ProcessList = new ProcessList();
            PluginList  = (App.Current as App).PluginList;
        }

        public MainWindow()
        {
            InitializeComponent();

            ProcessList.CollectionChanged += (o, e) => {
                if (ProcessList.Count > 0 && lbProcessList.SelectedIndex == -1)
                    lbProcessList.SelectedIndex = 0;
            };
        }

        private void Image_MouseDown(object o, RoutedEventArgs e)
        {
            this.DragMove();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            foreach (var plugin in PluginList)
                plugin.Dispose();
            PluginList.Clear();

            if (ProcessList != null)
                ProcessList.Dispose();

            if (codeExecuteWindow != null)
            {
                codeExecuteWindow.Close();
                codeExecuteWindow = null;
            }
            if (profileWindows != null)
            {
                profileWindows.Close();
                profileWindows = null;
            }
            if (settingWindow != null)
            {
                settingWindow.Close();
                settingWindow = null;
            }
        }

        private void CommandBinding_Executed_Close(object sender, ExecutedRoutedEventArgs e)
        {
            App.Current.ShutdownMode = ShutdownMode.OnMainWindowClose;
            App.Current.Shutdown();
        }

        private void CommandBinding_Executed_ShowExecuteWindow(object sender, ExecutedRoutedEventArgs e)
        {
            if (codeExecuteWindow == null || !codeExecuteWindow.IsLoaded)
                codeExecuteWindow = new WinCodeExecute();

            codeExecuteWindow.Show();

            if (!codeExecuteWindow.IsActive)
                codeExecuteWindow.Activate();
        }

        private void CommandBinding_Executed_ShowProfileWindow(object sender, ExecutedRoutedEventArgs e)
        {
            if (profileWindows == null || !profileWindows.IsLoaded)
                profileWindows = new WinProfileEditor();

            profileWindows.Show();

            if (!profileWindows.IsActive)
                profileWindows.Activate();
        }

        private void CommandBinding_Executed_ShowSettingWindow(object sender, ExecutedRoutedEventArgs e)
        {
            if (settingWindow == null || !settingWindow.IsLoaded)
                settingWindow = new WindowSettings();

            settingWindow.Show();

            if (!settingWindow.IsActive)
                settingWindow.Activate();
        }

        private void CommandBinding_Executed_Minimize(object sender, ExecutedRoutedEventArgs e)
        {
            this.WindowState = System.Windows.WindowState.Minimized;
        }

        private void CommandBinding_Executed_ShowPluginSettingWindow(object sender, ExecutedRoutedEventArgs e)
        {
            var button = e.OriginalSource as Button;
            var plugin = button.Tag as IYanittaPlugin;
            if (plugin != null)
            {
                var style = App.Current.Resources["KamillaStyle"] as Style;
                plugin.ShowSettingWindow(style);
            }
        }
    }
}