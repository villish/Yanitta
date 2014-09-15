using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Yanitta.Windows;

namespace Yanitta
{
    /// <summary>
    /// Главное окно программы.
    /// </summary>
    public partial class MainWindow : Window
    {
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
            App.Current.Shutdown();
        }

        private void CommandBinding_Executed_Close(object sender, ExecutedRoutedEventArgs e)
        {
            this.Close();
        }

        private void CommandBinding_Executed_ShowExecuteWindow(object sender, ExecutedRoutedEventArgs e)
        {
            App.ShowWindow<WinCodeExecute>();
        }

        private void CommandBinding_Executed_ShowProfileWindow(object sender, ExecutedRoutedEventArgs e)
        {
            App.ShowWindow<WinProfileEditor>();
        }

        private void CommandBinding_Executed_ShowSettingWindow(object sender, ExecutedRoutedEventArgs e)
        {
            App.ShowWindow<WindowSettings>();
        }

        private void CommandBinding_Executed_Minimize(object sender, ExecutedRoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void CommandBinding_Executed_OpenFile(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                var fileName = (string)e.Parameter;
                Process.Start(fileName);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, Localization.Error, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}