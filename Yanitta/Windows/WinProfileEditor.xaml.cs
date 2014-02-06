using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using Yanitta.Properties;

namespace Yanitta
{
    /// <summary>
    /// Логика взаимодействия для WindowProfileEditor.xaml
    /// </summary>
    public partial class WinProfileEditor : Window
    {
        private Key[] Keys = new Key[] { Key.X, Key.Z, Key.C, Key.V, Key.B, Key.N };

        public WinProfileEditor()
        {
            InitializeComponent();
            InitialiseEmptyProfiles();
        }

        private Profile CurrentProfile
        {
            get { return (profiLeList != null && profiLeList.SelectedValue is Profile) ? (Profile)profiLeList.SelectedValue : null; }
        }

        private Ability CurrentAbility
        {
            get { return (abilityList != null && abilityList.SelectedValue is Ability) ? (Ability)abilityList.SelectedValue : null; }
        }

        private Rotation CurrentRotation
        {
            get { return (rotationList != null && rotationList.SelectedValue is Rotation) ? (Rotation)rotationList.SelectedValue : null; }
        }

        private void MoveAbility(int shift)
        {
            var index = abilityList.SelectedIndex;
            if (CurrentRotation != null && index > -1
                && !(shift == -1 && index == 0)
                && !(shift == 1  && index == CurrentRotation.AbilityList.Count - 1))
            {
                CurrentRotation.AbilityList.Move(index, index + shift);
                abilityList.ScrollIntoView(this.abilityList.SelectedItem);
            }
        }

        private void MoveRotation(int shift)
        {
            var index = rotationList.SelectedIndex;
            if (CurrentProfile != null && index > -1
                && !(shift == -1 && index == 0)
                && !(shift == 1 && index == CurrentProfile.RotationList.Count - 1))
            {
                CurrentProfile.RotationList.Move(index, index + shift);
                rotationList.ScrollIntoView(this.rotationList.SelectedItem);
            }
        }

        private void InitialiseEmptyProfiles()
        {
            foreach (WowClass wowClass in Enum.GetValues(typeof(WowClass)))
            {
                if (!ProfileDb.Instance.ProfileList.Any(n => n.Class == wowClass))
                    ProfileDb.Instance.ProfileList.Add(new Profile() { Class = wowClass });
            }
        }

        private void bAbMoveUp_Click_1(object sender, RoutedEventArgs e)
        {
            MoveAbility(-1);
        }

        private void bAbMoveDown_Click_1(object sender, RoutedEventArgs e)
        {
            MoveAbility(1);
        }

        private void bRotMoveUp_Click(object sender, RoutedEventArgs e)
        {
            MoveRotation(-1);
        }

        private void bRotMoveDown_Click(object sender, RoutedEventArgs e)
        {
            MoveRotation(1);
        }

        private void Window_Closing_1(object sender, System.ComponentModel.CancelEventArgs e)
        {
            ProfileDb.Instance.Save(Settings.Default.ProfilesFileName);
        }

        #region Commands

        // ability
        private void CommandBinding_Executed_AddAbility(object sender, ExecutedRoutedEventArgs e)
        {
            if (this.CurrentProfile != null)
            {
                this.CurrentRotation.AbilityList.Add(new Ability());
                this.abilityList.SelectedIndex = this.CurrentRotation.AbilityList.Count - 1;
                this.tbAbilityName.Focus();
                this.tbAbilityName.SelectAll();
                abilityList.ScrollIntoView(this.abilityList.SelectedItem);
            }
        }

        private void CommandBinding_Executed_CopyAbility(object sender, ExecutedRoutedEventArgs e)
        {
            if (this.CurrentAbility != null)
            {
                this.CurrentRotation.AbilityList.Add((Ability)this.CurrentAbility.Clone());
                this.abilityList.SelectedIndex = this.CurrentRotation.AbilityList.Count - 1;
                this.tbAbilityName.Focus();
                this.tbAbilityName.SelectAll();
                this.abilityList.ScrollIntoView(this.abilityList.SelectedItem);
            }
        }

        private void CommandBinding_Executed_DeleteAbility(object sender, ExecutedRoutedEventArgs e)
        {
            var result = MessageBox.Show(Localization.AbilityDelQuestion,
                this.Title, MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (this.CurrentAbility != null && result == MessageBoxResult.Yes)
                this.CurrentRotation.AbilityList.Remove(this.CurrentAbility);
        }

        // rotations
        private void CommandBinding_Executed_AddRotation(object sender, ExecutedRoutedEventArgs e)
        {
            if (this.CurrentProfile != null)
            {
                var mod = ModifierKeys.Alt | (CurrentProfile.Class == WowClass.None
                    ? ModifierKeys.Shift
                    : ModifierKeys.None);

                var rotation = new Rotation();
                if (CurrentProfile.RotationList.Count < Keys.Length)
                    rotation.HotKey = new HotKey(Keys[CurrentProfile.RotationList.Count], mod);
                this.CurrentProfile.RotationList.Add(rotation);
                this.rotationList.SelectedIndex = this.CurrentProfile.RotationList.Count - 1;
                this.tbRotationName.Focus();
                this.tbRotationName.SelectAll();
                this.rotationList.ScrollIntoView(this.rotationList.SelectedItem);
            }
        }

        private void CommandBinding_Executed_CopyRotation(object sender, ExecutedRoutedEventArgs e)
        {
            if (this.CurrentRotation != null)
            {
                this.CurrentProfile.RotationList.Add((Rotation)this.CurrentRotation.Clone());
                this.rotationList.SelectedIndex = this.CurrentProfile.RotationList.Count - 1;
                this.tbRotationName.Focus();
                this.tbRotationName.SelectAll();
                this.rotationList.ScrollIntoView(this.rotationList.SelectedItem);
            }
        }

        private void CommandBinding_Executed_DeleteRotation(object sender, ExecutedRoutedEventArgs e)
        {
            var result = MessageBox.Show(Localization.RotationDelQuestion,
                   this.Title, MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (this.CurrentRotation != null && result == MessageBoxResult.Yes)
                this.CurrentProfile.RotationList.Remove(this.CurrentRotation);
        }

        private void CommandBinding_Executed_Update(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                ProfileDb.UpdateProfiles();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CommandBinding_Executed_Reload(object sender, ExecutedRoutedEventArgs e)
        {
            ProfileDb.Instance.Load(Settings.Default.ProfilesFileName);
        }

        private void CommandBinding_Executed_Save(object sender, ExecutedRoutedEventArgs e)
        {
            ProfileDb.Instance.Save(Settings.Default.ProfilesFileName, true);
            if (App.MainWindow is MainWindow)
                App.MainWindow.TaskbarIcon.ShowBalloonTip("Yanitta", "Profiles saved!", Microsoft.Windows.Controls.BalloonIcon.Info);
        }

        private void CommandBinding_Executed_Close(object sender, ExecutedRoutedEventArgs e)
        {
            e.Handled = true;
            this.Close();
        }

        #endregion Commands

        private void tbAbilityFilter_TextChanged(object sender, TextChangedEventArgs e)
        {
            var abilityView = CollectionViewSource.GetDefaultView(abilityList.ItemsSource);
            if (abilityView != null)
            {
                if (string.IsNullOrWhiteSpace(tbAbilityFilter.Text))
                {
                    abilityView.Filter = null;
                }
                else
                {
                    int spellId = 0;
                    if (int.TryParse(tbAbilityFilter.Text, out spellId))
                    {
                        abilityView.Filter = new Predicate<object>((raw_ability) => {
                            var ability = raw_ability as Ability;
                            if (ability == null)
                                return false;
                            return ability.SpellID == spellId;
                        });
                    }
                    else
                    {
                        abilityView.Filter = new Predicate<object>((raw_ability) => {
                            var ability = raw_ability as Ability;
                            if (ability == null || string.IsNullOrWhiteSpace(ability.Name))
                                return false;
                            return ability.Name.IndexOf(tbAbilityFilter.Text, StringComparison.CurrentCultureIgnoreCase) > -1;
                        });
                    }
                }
            }
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (System.IO.Directory.Exists("Profiles"))
                System.IO.Directory.Delete("Profiles");

            System.IO.Directory.CreateDirectory("Profiles");

            foreach (var profile in ProfileDb.Instance.ProfileList)
            {
                if (!System.IO.Directory.Exists("Profiles\\" + profile.Class))
                    System.IO.Directory.CreateDirectory("Profiles\\" + profile.Class);

                foreach (var rotation in profile.RotationList)
                {
                    using (var stream = System.IO.File.CreateText(("Profiles\\" + profile.Class + "\\" + rotation.Name + ".lua").Replace(':', '_')))
                    {
                        stream.WriteLine("if not ROTATIONS then ROTATIONS = { } end");
                        stream.WriteLine("local rotation = {");
                        stream.WriteLine("    Class = \"{0}\",", profile.Class.ToString().ToUpper());
                        stream.WriteLine("    Spec  = 0,");
                        stream.WriteLine("    Name  = \"{0}\",", rotation.Name);
                        stream.WriteLine("    List  = { }");
                        stream.WriteLine("};");
                        stream.WriteLine("table.insert(ROTATIONS, rotation);");
                        stream.WriteLine("local ABILITY_TABLE = rotation.List;");

                        foreach (var ability in rotation.AbilityList)
                        {
                            stream.WriteLine(ability.ToString());
                        }

                        stream.Flush();
                    }
                }
            }

            MessageBox.Show("Done!");
        }
    }
}