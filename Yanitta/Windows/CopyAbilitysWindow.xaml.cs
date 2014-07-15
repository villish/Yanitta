using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace Yanitta
{
    /// <summary>
    /// Логика взаимодействия для CopyAbilitysWindow.xaml
    /// </summary>
    public partial class CopyAbilitysWindow : Window
    {
        public CopyAbilitysWindow(Profile profile)
        {
            this.DataContext = profile;

            InitializeComponent();

            if (cbRotation.Items.Count > 0)
                cbRotation.SelectedIndex = 0;
        }

        public IEnumerable<Ability> SelectedAbilitys
        {
            get { return cbRotation.SelectedValue<Rotation>().AbilityList.Where(a => a.IsChecked); }
        }

        private void CommandBinding_Copy_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (cbRotation.SelectedIndex == -1)
                throw new YanittaException("Not selected rotation");
            this.DialogResult = true;
        }

        private void CommandBinding_Close_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            this.DialogResult = false;
        }
    }
}
