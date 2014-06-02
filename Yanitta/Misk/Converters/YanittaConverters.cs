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
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || !(value is WowClass))
                return Binding.DoNothing;

            var name = (WowClass)value == (WowClass)(byte)0 ? "None" : value.ToString();
            var path = string.Format(@"pack://application:,,,/Yanitta;component/Resources/{0}.png", name);
            return new BitmapImage(new Uri(path));
        }
    }

    public class EnumToLocalizedSrtConverter : ConverterBase<EnumToLocalizedSrtConverter>
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || !(value is Enum))
                return Binding.DoNothing;

            var name = string.Format("{0}_{1}", value.GetType().Name, value);
            if (parameter is string && !string.IsNullOrWhiteSpace((string)parameter))
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
            return index + 1;
        }
    }

    public class ResizeConverter : ConverterBase<ResizeConverter>
    {
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

    public class StringToIntConverter : ConverterBase<StringToIntConverter>
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return string.Format("0x{0:X}", value);
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return Binding.DoNothing;

            string str = value.ToString();

            if (string.IsNullOrWhiteSpace(str))
                return Binding.DoNothing;

            long n;
            if (IsHex(str))
            {
                if (str.StartsWith("0x"))
                    long.TryParse(str.Substring(2), NumberStyles.AllowHexSpecifier, CultureInfo.CurrentCulture, out n);
                else
                    long.TryParse(str, NumberStyles.AllowHexSpecifier, CultureInfo.CurrentCulture, out n);
            }
            else
                long.TryParse(str, out n);

            return n;
        }

        private bool IsHex(string str)
        {
            if (!string.IsNullOrWhiteSpace(str))
                foreach (char c in str.ToUpper())
                {
                    if (c >= 'A' && c <= 'F')
                        return true;
                }
            return false;
        }
    }
}
