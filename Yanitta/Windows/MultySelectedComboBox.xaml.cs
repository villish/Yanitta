using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Yanitta
{
    public class MultySelectedComboBox : ComboBox
    {
        public static DependencyProperty SelectedItemsProperty = DependencyProperty.Register("SelectedItems", typeof(List<TargetType>), typeof(MultySelectedComboBox),
            new FrameworkPropertyMetadata(new List<TargetType>(), FrameworkPropertyMetadataOptions.Journal | FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnSelectedItemsPropertyPropertyChanged, null, true, UpdateSourceTrigger.PropertyChanged));

        public static DependencyProperty EnumSourceProperty = DependencyProperty.Register("EnumSource", typeof(Type), typeof(MultySelectedComboBox),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Journal | FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnEnumSourcePropertyChanged, null, true, UpdateSourceTrigger.PropertyChanged));

        public static DependencyProperty SelectedItemsTextProperty = DependencyProperty.Register("SelectedItemsText", typeof(string), typeof(MultySelectedComboBox));

        static void OnSelectedItemsPropertyPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != e.OldValue)
            {
                var control = d as MultySelectedComboBox;
                if (control.ItemsSource is List<ComboBoxFlagsItem>)
                {
                    foreach (ComboBoxFlagsItem item in control.ItemsSource)
                    {
                        item.IsChecked = control.SelectedItems != null && control.SelectedItems.Contains(item.Value);
                    }
                }
                if (control.SelectedItems != null)
                    control.SelectedItemsText = string.Join(", ", control.SelectedItems);
            }
        }

        static void OnEnumSourcePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != e.OldValue)
            {
                var control = d as MultySelectedComboBox;

                if (e.NewValue == null)
                {
                    control.ItemsSource = null;
                    control.SelectedItemsText = "";
                }
                else
                {
                    var list = new List<ComboBoxFlagsItem>();
                    foreach (TargetType element in Enum.GetValues((Type)e.NewValue))
                    {
                        var isChecked = control.SelectedItems != null && control.SelectedItems.Contains(element); ;
                        list.Add(new ComboBoxFlagsItem(control, element, isChecked));
                    }
                    if (control.SelectedItems != null)
                        control.SelectedItemsText = string.Join(", ", control.SelectedItems);

                    list.Sort((a, b) => a.Value.CompareTo(b.Value));
                    control.ItemsSource = list;
                }
            }
        }

        public List<TargetType> SelectedItems
        {
            get { return (List<TargetType>)GetValue(SelectedItemsProperty); }
            set { SetValue(SelectedItemsProperty, value); }
        }

        public Type EnumSource
        {
            get { return (Type)GetValue(EnumSourceProperty); }
            set { SetValue(EnumSourceProperty, value); }
        }

        public string SelectedItemsText
        {
            get { return (string)GetValue(SelectedItemsTextProperty); }
            set { SetValue(SelectedItemsTextProperty, value); }
        }
    }

    public class ComboBoxFlagsItem : ViewModelBase
    {
        public ComboBoxFlagsItem(MultySelectedComboBox control, TargetType value, bool isChecked)
        {
            this.control   = control;
            this.isChecked = isChecked;
            Value = value;
        }

        MultySelectedComboBox control;

        bool isChecked;

        public TargetType Value { get; set; }

        public bool IsChecked
        {
            get { return isChecked; }
            set
            {
                if (isChecked != value)
                {
                    isChecked = value;
                    OnPropertyChanged();

                    control.SelectedItems.Remove(Value);
                    if (value)
                        control.SelectedItems.Add(Value);

                    control.SelectedItems.Sort((a, b) => a.CompareTo(b));
                    control.SelectedItemsText = string.Join(", ", control.SelectedItems);
                }
            }
        }
    }
}
