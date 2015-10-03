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

        void Image_MouseDown(object o, RoutedEventArgs e)
        {
            DragMove();
        }

        void Window_Closing(object sender, CancelEventArgs e)
        {
            App.Current.Shutdown();
        }

        void CommandBinding_Executed_Close(object sender, ExecutedRoutedEventArgs e)
        {
            Close();
        }

        void CommandBinding_Executed_ShowExecuteWindow(object sender, ExecutedRoutedEventArgs e)
        {
            App.ShowWindow<WinCodeExecute>();
        }

        void CommandBinding_Executed_ShowProfileWindow(object sender, ExecutedRoutedEventArgs e)
        {
            App.ShowWindow<WinProfileEditor>();
        }

        void CommandBinding_Executed_ShowSettingWindow(object sender, ExecutedRoutedEventArgs e)
        {
            App.ShowWindow<WindowSettings>();
        }

        void CommandBinding_Executed_Minimize(object sender, ExecutedRoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        void CommandBinding_Executed_OpenFile(object sender, ExecutedRoutedEventArgs e)
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