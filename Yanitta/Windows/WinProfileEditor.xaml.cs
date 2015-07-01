using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using Yanitta.Windows;

namespace Yanitta
{
    public partial class WinProfileEditor : Window
    {
        private Key[] Keys = { Key.X, Key.Z, Key.C, Key.V, Key.B, Key.N };

        public WinProfileEditor()
        {
            InitializeComponent();
            InitialiseEmptyProfiles();
        }

        private Profile CurrentProfile
        {
            get { return profiLeList.SelectedValue<Profile>(); }
        }

        private Rotation CurrentRotation
        {
            get { return rotationList.SelectedValue<Rotation>(); }
        }

        private Ability CurrentAbility
        {
            get { return abilityList.SelectedValue<Ability>(); }
        }

        private void InitialiseEmptyProfiles()
        {
            foreach (WowClass wowClass in Enum.GetValues(typeof(WowClass)))
            {
                if (!ProfileDb.Instance.ProfileList.Any(profile => profile.Class == wowClass))
                    ProfileDb.Instance.ProfileList.Add(new Profile { Class = wowClass });
            }

            // Профиль общих ротаций должен быть в самом низу.
            var old = ProfileDb.Instance.ProfileList.IndexOf(
                ProfileDb.Instance.ProfileList.First(p => p.Class == WowClass.None));
            if (old > -1 && old != ProfileDb.Instance.ProfileList.Count - 1)
                ProfileDb.Instance.ProfileList.Move(old, ProfileDb.Instance.ProfileList.Count - 1);
        }

        #region Commands

        // ability
        private void CommandBinding_Executed_AddAbility(object sender, ExecutedRoutedEventArgs e)
        {
            if (this.CurrentRotation == null)
                throw new YanittaException("Не выбрана ротация для новой способности!");

            this.CurrentRotation.AbilityList.Add(new Ability {
                IsUsableCheck = true,
                TargetList = new List<TargetType> { TargetType.None }
            });
            this.abilityList.SelectedIndex = this.CurrentRotation.AbilityList.Count - 1;
            this.tbAbilityName.Focus();
            this.tbAbilityName.SelectAll();
            abilityList.ScrollIntoView(this.abilityList.SelectedItem);
            CollectionViewSource.GetDefaultView(abilityList.ItemsSource).Refresh();
        }

        private void CommandBinding_Executed_CopyAbility(object sender, ExecutedRoutedEventArgs e)
        {
            if (this.CurrentAbility == null)
                throw new YanittaException("Не выбрана способность для копирования!");

            this.CurrentRotation.AbilityList.Add(this.CurrentAbility.Clone());
            this.abilityList.SelectedIndex = this.CurrentRotation.AbilityList.Count - 1;
            this.tbAbilityName.Focus();
            this.tbAbilityName.SelectAll();
            this.abilityList.ScrollIntoView(this.abilityList.SelectedItem);
            CollectionViewSource.GetDefaultView(abilityList.ItemsSource).Refresh();
        }

        private void CommandBinding_Executed_DeleteAbility(object sender, ExecutedRoutedEventArgs e)
        {
            if (this.CurrentAbility == null)
                throw new YanittaException("Не выбрана способность для удаления!");

            var result = MessageBox.Show(Localization.AbilityDelQuestion,
                this.Title, MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                this.CurrentRotation.AbilityList.Remove(this.CurrentAbility);
                CollectionViewSource.GetDefaultView(abilityList.ItemsSource).Refresh();
            }
        }

        // rotations
        private void CommandBinding_Executed_AddRotation(object sender, ExecutedRoutedEventArgs e)
        {
            if (this.CurrentProfile == null)
                throw new YanittaException("Не выбран класс для новой ротации!");

            var mod = ModifierKeys.Alt | (CurrentProfile.Class == WowClass.None
                ? ModifierKeys.Shift
                : ModifierKeys.None);

            var rotation = new Rotation();
            if (CurrentProfile.RotationList.Count < this.Keys.Length)
                rotation.HotKey = new HotKey(this.Keys[CurrentProfile.RotationList.Count], mod);
            this.CurrentProfile.RotationList.Add(rotation);
            this.rotationList.SelectedIndex = this.CurrentProfile.RotationList.Count - 1;
            this.tbRotationName.Focus();
            this.tbRotationName.SelectAll();
            this.rotationList.ScrollIntoView(this.rotationList.SelectedItem);
            CollectionViewSource.GetDefaultView(rotationList.ItemsSource).Refresh();
        }

        private void CommandBinding_Executed_CopyRotation(object sender, ExecutedRoutedEventArgs e)
        {
            if (this.CurrentRotation == null)
                throw new YanittaException("Не выбрана ротация для копирования!");

            this.CurrentProfile.RotationList.Add((Rotation)this.CurrentRotation.Clone());
            this.rotationList.SelectedIndex = this.CurrentProfile.RotationList.Count - 1;
            this.tbRotationName.Focus();
            this.tbRotationName.SelectAll();
            this.rotationList.ScrollIntoView(this.rotationList.SelectedItem);
            CollectionViewSource.GetDefaultView(rotationList.ItemsSource).Refresh();
        }

        private void CommandBinding_Executed_DeleteRotation(object sender, ExecutedRoutedEventArgs e)
        {
            if (this.CurrentRotation == null)
                throw new YanittaException("Не выбрана ротация для удаления!");

            var result = MessageBox.Show(Localization.RotationDelQuestion,
                   this.Title, MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                this.CurrentProfile.RotationList.Remove(this.CurrentRotation);
                CollectionViewSource.GetDefaultView(rotationList.ItemsSource).Refresh();
            }
        }

        private void CommandBinding_MoveRotation_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var shift = int.Parse((string)e.Parameter);
            var index = rotationList.SelectedIndex;
            if (CurrentProfile != null && index > -1
                && !(shift == -1 && index == 0)
                && !(shift == 1  && index == CurrentProfile.RotationList.Count - 1))
            {
                CurrentProfile.RotationList.Move(index, index + shift);
                rotationList.ScrollIntoView(this.rotationList.SelectedItem);
                CollectionViewSource.GetDefaultView(rotationList.ItemsSource).Refresh();
            }
        }

        private void CommandBinding_MoveAbility_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var shift = int.Parse((string)e.Parameter);
            var index = abilityList.SelectedIndex;
            if (CurrentRotation != null && index > -1
                && !(shift == -1 && index == 0)
                && !(shift == 1  && index == CurrentRotation.AbilityList.Count - 1))
            {
                CurrentRotation.AbilityList.Move(index, index + shift);
                abilityList.ScrollIntoView(this.abilityList.SelectedItem);
                CollectionViewSource.GetDefaultView(abilityList.ItemsSource).Refresh();
            }
        }

        private void CommandBinding_CopyFromRotation_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (this.CurrentProfile == null)
                throw new YanittaException("Empty profile");

            var window = new CopyAbilitysWindow(this.CurrentProfile);
            window.Owner = this;
            if (window.ShowDialog() == true)
            {
                foreach (var ability in window.SelectedAbilitys)
                {
                    CurrentRotation.AbilityList.Add(ability);
                }
                CollectionViewSource.GetDefaultView(abilityList.ItemsSource).Refresh();
            }
        }

        private void CommandBinding_Executed_Save(object sender, ExecutedRoutedEventArgs e)
        {
            ProfileDb.Save();
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
                abilityView.Refresh();
            }
        }

        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F1 && CurrentAbility.SpellID != 0)
            {
                App.ShowWindow<HelpWindow>().GetSpellData(CurrentAbility.SpellID);
            }
        }

        private void TextEditor_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F1)
            {
                var spellId = 0u;
                var spell = (sender as ICSharpCode.AvalonEdit.TextEditor).GetWord();
                if (uint.TryParse(spell, out spellId))
                    App.ShowWindow<HelpWindow>().GetSpellData(spellId);
            }
        }
    }
}