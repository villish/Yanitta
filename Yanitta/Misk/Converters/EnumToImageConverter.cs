using System;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace Yanitta
{
    [ValueConversion(typeof(WowClass), typeof(string))]
    public class EnumToImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null || !(value is WowClass))
                return Binding.DoNothing;

            var name = (WowClass)value == (WowClass)(byte)0 ? "None" : value.ToString();
            var path = string.Format(@"pack://application:,,,/Yanitta;component/Resources/{0}.png", name);
            return new BitmapImage(new Uri(path));
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}