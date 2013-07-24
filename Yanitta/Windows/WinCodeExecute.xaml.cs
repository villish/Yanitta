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

        //public void Initialise(ObservableCollection<WowMemory> ProcessList, int processIndex)
        //{
        //    if (processIndex == -1)
        //        processIndex = 0;

        //    if (ProcessList.Count > 0)
        //    {
        //        this.DataContext = ProcessList;
        //        cbProcess.SelectedIndex = processIndex;
        //    }
        //}

        private void Exec()
        {
            if (!string.IsNullOrWhiteSpace(this.teCode.Text))
            {
                var process = cbProcess.SelectedValue as WowMemory;
                if (process != null)
                {
                    if (process.IsInGame)
                        process.LuaExecute(this.teCode.Text);
                    else
                        MessageBox.Show("Не в игре!");
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