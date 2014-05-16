using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows.Controls.Primitives;
using System.Xml;

namespace Yanitta
{
    public static class Extensions
    {
        public static T SelectedValue<T>(this Selector control) where T : class
        {
            return control == null ? null : control.SelectedValue as T;
        }

        public static void ForEach<T>(this IEnumerable<T> collection, Action<T> predicate)
        {
            if (collection == null)
                return;

            foreach (T element in collection)
                predicate(element);
        }

        public static void AppendFormatLine(this StringBuilder builder, string format, params object[] args)
        {
            if (args == null || args.Length == 0)
                builder.AppendLine(format);
            else
                builder.AppendFormat(CultureInfo.InvariantCulture, format, args).AppendLine();
        }

        public static string GetTrimValue(this XmlCDataSection cdataSection)
        {
            if (cdataSection == null || string.IsNullOrWhiteSpace(cdataSection.Value))
                return string.Empty;
            return cdataSection.Value.Trim();
        }

        public static XmlCDataSection CreateCDataSection(this string content)
        {
            if (string.IsNullOrWhiteSpace(content))
                return null;
            return new XmlDocument().CreateCDataSection("\n" + content + "\n");
        }
    }
}