﻿using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace Yanitta
{
    public abstract class ConverterBase<T>
        : MarkupExtension, IValueConverter where T : class, new()
    {
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
            if (m_converter == null)
                m_converter = new T();
            return m_converter;
        }

        private static T m_converter = null;
    }
}