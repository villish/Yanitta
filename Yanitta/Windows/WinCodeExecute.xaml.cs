using System.Windows;
using System.Windows.Input;

namespace Yanitta.Windows
{
    public partial class WinCodeExecute : Window
    {
        public WinCodeExecute()
        {
            InitializeComponent();

            if (this.mainGrid.DataContext is ProcessList)
                this.cbProcess.SelectedIndex = (this.mainGrid.DataContext as ProcessList).Count > 0 ? 0 : -1;
        }

        private void Exec()
        {
            if (!string.IsNullOrWhiteSpace(this.teCode.Text))
            {
                var code = string.IsNullOrWhiteSpace(this.teCode.SelectedText)
                    ? this.teCode.Text
                    : this.teCode.SelectedText;
                var process = cbProcess.SelectedValue as WowMemory;
                if (process != null)
                {
                    process.LuaExecute(code);
                }
                else
                {
                    MessageBox.Show(Localization.NotSelectedProcessMessage);
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