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

            var name = string.Format("{0}_{1}", value.GetType().Name, value);
            if (parameter is string && !string.IsNullOrWhiteSpace((string)parameter))
                name += "_" + parameter;
            return Localization.ResourceManager.GetString(name, System.Globalization.CultureInfo.CurrentUICulture);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}