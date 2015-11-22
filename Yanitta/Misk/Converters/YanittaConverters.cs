using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Yanitta
{
    public class EnumToLocalizedSrtConverter : ConverterBase<EnumToLocalizedSrtConverter>
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is Enum))
                return Binding.DoNothing;

            var name = $"{value.GetType().Name}_{value}";

            if (!string.IsNullOrWhiteSpace(parameter?.ToString()))
                name += "_" + parameter;

            return Localization.ResourceManager.GetString(name, CultureInfo.CurrentUICulture);
        }
    }

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