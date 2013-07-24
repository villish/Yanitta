using System;
using System.Windows.Data;

namespace Yanitta
{
    [ValueConversion(typeof(Enum), typeof(string))]
    public class EnumToLocalizedSrtConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null || !(value is Enum))
                return Binding.DoNothing;

            LocalizedNameAttribute[] attr = (LocalizedNameAttribute[])value
                .GetType()
                .GetField(value.ToString())
                .GetCustomAttributes(typeof(LocalizedNameAttribute), false);

            if (attr.Length == 1)
                return attr[0].Name;

            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}