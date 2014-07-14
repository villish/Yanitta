using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

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
        }

        public IEnumerable<Ability> SelectedAbilitys
        {
            get
            {
                if (cbRotation.SelectedValue is Rotation)
                {
                    foreach (var ability in (cbRotation.SelectedValue as Rotation).AbilityList)
                    {
                        if (ability.IsChecked)
                            yield return ability.Clone();
                    }
                }
            }
        }

        private void CommandBinding_Copy_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        private void CommandBinding_Close_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            this.DialogResult = false;
        }
    }
}
