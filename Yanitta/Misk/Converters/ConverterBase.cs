using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace Yanitta
{
    public abstract class ConverterBase<T>
        : MarkupExtension, IValueConverter where T : class, new()
    {
        static T converter = new T();

        public ConverterBase()
            : base()
        {
        }

        public abstract object Convert(object value, Type targetType, object parameter, CultureInfo culture);

        public virtual object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return converter;
        }
    }
}
