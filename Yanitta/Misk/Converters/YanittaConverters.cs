using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;

namespace Yanitta
{
    public class ItemsControlIndexConverter : ConverterBase<ItemsControlIndexConverter>
    {
        // {Binding RelativeSource={RelativeSource FindAncestor, AncestorType=ContentControl}, Converter={yanitta:ListItemIndexConverter}}
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var item = value as DependencyObject;
            var view = ItemsControl.ItemsControlFromItemContainer(item);
            var index = view.ItemContainerGenerator.IndexFromContainer(item);
            return ++index;
        }
    }
}