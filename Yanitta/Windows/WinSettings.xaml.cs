using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using Yanitta.Properties;

namespace Yanitta
{
    public partial class WindowSettings : Window
    {
        public WindowSettings()
        {
            InitializeComponent();
        }

        void CommandBinding_Save_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Settings.Default.Save();
            Close();
        }

        void CommandBinding_Open_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                FileName = Settings.Default.ProfilesFileName,
                Filter = Localization.ProfileFileMask
            };
            if (dialog.ShowDialog() == true)
                Settings.Default.ProfilesFileName = dialog.FileName;
        }

        void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Settings.Default.Reload();
        }
    }
}