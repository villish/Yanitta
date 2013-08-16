using System;
using System.Windows.Data;

namespace Yanitta
{
    [ValueConversion(typeof(Enum), typeof(string))]
    public class ResizeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
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

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
