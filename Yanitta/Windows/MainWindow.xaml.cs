using Microsoft.Windows.Controls;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Yanitta.Plugins;
using Yanitta.Properties;
using Yanitta.Windows;

namespace Yanitta
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private WinCodeExecute   codeExecuteWindow = null;
        private WinProfileEditor profileWindows    = null;
        private WindowSettings   settingWindow     = null;

        public static ProcessList ProcessList { get; set; }

        public TaskbarIcon TaskbarIcon
        {
            get { return notyfyIcon; }
        }

        static MainWindow()
        {
            ProcessList = new ProcessList();
        }

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Image_MouseDown(object o, RoutedEventArgs e)
        {
            this.DragMove();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            ProfileDb.Instance.Save(Settings.Default.ProfilesFileName);

            if (ProcessList != null)
                ProcessList.Dispose();

            App.Current.Shutdown();
        }

        private void CommandBinding_Executed_Close(object sender, ExecutedRoutedEventArgs e)
        {
            this.Close();
        }

        private void ShowWindow<T>(ref T window) where T : Window, new()
        {
            if (window == null || !window.IsLoaded)
                window = new T();

            window.Show();

            if (!window.IsActive)
                window.Activate();
        }

        private void CommandBinding_Executed_ShowExecuteWindow(object sender, ExecutedRoutedEventArgs e)
        {
            ShowWindow<WinCodeExecute>(ref codeExecuteWindow);
        }

        private void CommandBinding_Executed_ShowProfileWindow(object sender, ExecutedRoutedEventArgs e)
        {
            ShowWindow<WinProfileEditor>(ref profileWindows);
        }

        private void CommandBinding_Executed_ShowSettingWindow(object sender, ExecutedRoutedEventArgs e)
        {
            ShowWindow<WindowSettings>(ref settingWindow);
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