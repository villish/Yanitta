using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace Yanitta.Windows
{
    public partial class CopyAbilitysWindow : Window
    {
        public CopyAbilitysWindow(Window owner, Profile profile)
        {
            Owner = owner;
            DataContext = profile;
            InitializeComponent();

            if (cbRotation.Items.Count > 0)
                cbRotation.SelectedIndex = 0;
        }

        public IEnumerable<Ability> SelectedAbilitys => (cbRotation?.SelectedValue as Rotation)?.AbilityList?.Where(a => a.IsChecked);

        void CommandBinding_Copy_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (cbRotation.SelectedIndex == -1)
            {
                MessageBox.Show("Not selected rotation", "Yanitta", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            DialogResult = true;
        }

        void CommandBinding_Close_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
