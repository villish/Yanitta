using System;
using System.Globalization;
using System.Windows.Data;

namespace Yanitta
{
    public class StringToIntConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return string.Format("0x{0:X}", value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null)
                return Binding.DoNothing;

            string str = value.ToString();

            if (string.IsNullOrWhiteSpace(str))
                return Binding.DoNothing;

            uint n;
            if (IsHex(str))
            {
                if (str.StartsWith("0x"))
                    uint.TryParse(str.Substring(2), NumberStyles.AllowHexSpecifier, CultureInfo.CurrentCulture, out n);
                else
                    uint.TryParse(str, NumberStyles.AllowHexSpecifier, CultureInfo.CurrentCulture, out n);
            }
            else
                uint.TryParse(str, out n);

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