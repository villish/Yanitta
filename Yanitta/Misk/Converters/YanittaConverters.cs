using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace Yanitta
{
    public class EnumToImageConverter : ConverterBase<EnumToImageConverter>
    {
        public EnumToImageConverter() : base() { }

        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || !(value is WowClass))
                return Binding.DoNothing;

            var img = Application.Current.TryFindResource(value.ToString()) as Image;
            if (img == null)
                img = Application.Current.TryFindResource("None") as Image;

            return new BitmapImage(new Uri(img.Source.ToString()));
        }
    }

    public class EnumToLocalizedSrtConverter : ConverterBase<EnumToLocalizedSrtConverter>
    {
        public EnumToLocalizedSrtConverter() : base() { }

        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || !(value is Enum))
                return Binding.DoNothing;

            var name = string.Format("{0}_{1}", value.GetType().Name, value);
            if (parameter != null && parameter is string && !string.IsNullOrWhiteSpace((string)parameter))
                name += "_" + parameter;
            return Localization.ResourceManager.GetString(name, CultureInfo.CurrentUICulture);
        }
    }

    public class ItemsControlIndexConverter : ConverterBase<ItemsControlIndexConverter>
    {
        public ItemsControlIndexConverter() : base() { }

        // {Binding RelativeSource={RelativeSource FindAncestor, AncestorType=ContentControl}, Converter={yanitta:ListItemIndexConverter}}
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var item = value as DependencyObject;
            var view = ItemsControl.ItemsControlFromItemContainer(item);
            var index = view.ItemContainerGenerator.IndexFromContainer(item);
            return index + 1;
        }
    }

    public class ResizeConverter : ConverterBase<ResizeConverter>
    {
        public ResizeConverter() : base() { }

        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double mod = 0d;
            double val = 0d;
            if (value == null)
                return 0d;
            if (value is double)
                val = (double)value;
            if (double.IsNaN(val))
                val = 100d;
            if (parameter is string)
                double.TryParse((string)parameter, out mod);
            return val + mod;
        }
    }
}
