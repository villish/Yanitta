using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Xml;

namespace Yanitta
{
    public static class Extensions
    {
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

        public static void CopyProperies(object src, object dst)
        {
            if (src == null || dst == null)
                throw new ArgumentNullException();
            var typesrc = src.GetType();
            var typedst = dst.GetType();
            if (typesrc != typedst)
                throw new Exception();
            var flag = System.Reflection.BindingFlags.Public
                     | System.Reflection.BindingFlags.Instance
                     | System.Reflection.BindingFlags.DeclaredOnly
                ;

            foreach (var srcprop in typesrc.GetProperties(flag))
            {
                var dstprop = typedst.GetProperty(srcprop.Name);
                var val = srcprop.GetValue(src, null);
                dstprop.SetValue(dst, val, null);
            }
        }
    }
}