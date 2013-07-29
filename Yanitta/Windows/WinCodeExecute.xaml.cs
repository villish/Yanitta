using System.Windows;
using System.Windows.Input;

namespace Yanitta.Windows
{
    /// <summary>
    /// Логика взаимодействия для WinCodeExecute.xaml
    /// </summary>
    public partial class WinCodeExecute : Window
    {
        public WinCodeExecute()
        {
            InitializeComponent();
        }

        private void Exec()
        {
            if (!string.IsNullOrWhiteSpace(this.teCode.Text))
            {
                var process = cbProcess.SelectedValue as WowMemory;
                if (process != null)
                {
                    process.LuaHook.LuaExecute(this.teCode.Text);
                }
                else
                {
                    MessageBox.Show("Нет доступного процесса!");
                }
            }
        }

        private void Window_PreviewKeyDown_1(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F5)
                this.Exec();
        }

        private void CommandBinding_Executed_1(object sender, ExecutedRoutedEventArgs e)
        {
            this.Exec();
        }
    }
}