using System.Windows;
using System.Windows.Input;

namespace Yanitta.Windows
{
    public partial class WinCodeExecute : Window
    {
        public WinCodeExecute()
        {
            InitializeComponent();

            if (this.cbProcess.Items.Count > 0)
                this.cbProcess.SelectedIndex = 0;
        }

        private void CommandBinding_Exec_Executed(object sender, ExecutedRoutedEventArgs e)
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
    }
}