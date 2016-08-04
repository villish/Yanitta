using System.Windows;
using System.Windows.Input;

namespace Yanitta.Windows
{
    public partial class WinCodeExecute : Window
    {
        public WinCodeExecute()
        {
            InitializeComponent();

            if (cbProcess.Items.Count > 0)
                cbProcess.SelectedIndex = 0;
        }

        void CommandBinding_Exec_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(teCode.Text))
            {
                var code = string.IsNullOrWhiteSpace(teCode.SelectedText)
                    ? teCode.Text
                    : teCode.SelectedText;
                var process = cbProcess.SelectedValue as WowMemory;
                if (process != null)
                {
                    process.LuaExecute(code);
                }
                else
                {
                    MessageBox.Show("Process not selected");
                }
            }
        }
    }
}