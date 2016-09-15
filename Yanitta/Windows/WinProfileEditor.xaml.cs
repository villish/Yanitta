using ICSharpCode.AvalonEdit;
using System;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace Yanitta.Windows
{
    public partial class WinProfileEditor : Window
    {
        public WinProfileEditor()
        {
            InitializeComponent();
        }

        void CommandBinding_Executed_Save(object sender, ExecutedRoutedEventArgs e) => ProfileDb.Save();

        void CommandBinding_CopyFromRotation_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var window = new CopyAbilitysWindow(this, profiLeList?.SelectedValue as Profile);
            var rotation = rotationList?.SelectedValue as Rotation;
            if (window.ShowDialog() == true && rotation != null)
            {
                foreach (var ability in window.SelectedAbilitys)
                {
                    rotation.AbilityList.Add(ability.Clone());
                }
            }
        }

        void tbAbilityFilter_TextChanged(object sender, TextChangedEventArgs e)
        {
            var abilityView = CollectionViewSource.GetDefaultView(abilityList.ItemsSource);
            if (abilityView != null)
            {
                var text = (sender as TextBox)?.Text;
                if (string.IsNullOrWhiteSpace(text))
                {
                    abilityView.Filter = null;
                }
                else
                {
                    int spellId = 0;
                    if (int.TryParse(text, out spellId))
                    {
                        abilityView.Filter = new Predicate<object>((raw_ability) =>
                        {
                            var ability = raw_ability as Ability;
                            if (ability == null)
                                return false;
                            return ability.SpellID == spellId;
                        });
                    }
                    else
                    {
                        abilityView.Filter = new Predicate<object>((raw_ability) =>
                        {
                            var ability = raw_ability as Ability;
                            if (ability == null || string.IsNullOrWhiteSpace(ability.Name))
                                return false;
                            return ability.Name.IndexOf(text, StringComparison.CurrentCultureIgnoreCase) > -1;
                        });
                    }
                }
                abilityView.Refresh();
            }
        }

        void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F1
                && uint.TryParse((sender as TextBox).Text, out var spellId))
            {
                App.ShowWindow<HelpWindow>().SetSpellData(spellId);
            }
        }

        void TextEditor_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F1)
            {
                var spell = (sender as TextEditor).GetWord();
                if (uint.TryParse(spell, out var spellId))
                    App.ShowWindow<HelpWindow>().SetSpellData(spellId);
            }
        }

        void Button_Click(object sender, RoutedEventArgs e)
        {
            var spellId = (abilityList?.SelectedItem as Ability)?.SpellID;
            if (spellId > 0)
            {
                var data = HelpWindow.GetSpellData(spellId.Value);
                tbAbilityName.Text = data?.Name;
            }
        }

        void listView_ItemsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            var listView = (ListView)sender;
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    if (listView.Name == "abilityList")
                    {
                        tbSpellId.Focus();
                        tbSpellId.SelectAll();
                    }
                    else if (listView.Name == "rotationList")
                    {
                        tbRotationName.Focus();
                        tbRotationName.SelectAll();
                    }
                    goto case NotifyCollectionChangedAction.Move;
                case NotifyCollectionChangedAction.Remove:
                case NotifyCollectionChangedAction.Move:
                    if (listView.HasItems)
                    {
                        listView.SelectedIndex = e.NewStartingIndex > -1 ? e.NewStartingIndex : 0;
                        listView.ScrollIntoView(listView.SelectedItem);
                    }
                    CollectionViewSource.GetDefaultView(listView.ItemsSource)?.Refresh();
                    break;
            }
        }
    }
}