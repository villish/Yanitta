using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using Microsoft.Windows.Controls;
using Yanitta.Properties;
using Yanitta.Windows;

namespace Yanitta
{
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

            if (window.WindowState == WindowState.Minimized)
                window.WindowState = WindowState.Normal;
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
            this.WindowState = WindowState.Minimized;
        }
    }
}