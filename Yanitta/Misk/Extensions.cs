using System;
using System.Globalization;
using System.IO;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Xml;

namespace Yanitta
{
    public static class Extensions
    {
        public static void AppendFormatLine(this StringBuilder builder, string format, params object[] args)
        {
            if (args == null || args.Length == 0)
                builder.AppendLine(format);
            else
                builder.AppendFormat(CultureInfo.InvariantCulture, format, args).AppendLine();
        }

        public static string GetTrimValue(XmlCDataSection cdataSection)
        {
            if (cdataSection == null || string.IsNullOrWhiteSpace(cdataSection.Value))
                return string.Empty;
            return cdataSection.Value.Trim();
        }

        public static XmlCDataSection CreateCDataSection(string content)
        {
            if (string.IsNullOrWhiteSpace(content))
                return null;
            return new XmlDocument().CreateCDataSection("\n" + content + "\n");
        }

        public static T GetJSONObject<T>(string url) where T : class
        {
            if (string.IsNullOrWhiteSpace(url))
                throw new ArgumentNullException(nameof(url));

            T result = default(T);

            try
            {
                var request = WebRequest.Create(url) as HttpWebRequest;
                using (var response = request.GetResponse() as HttpWebResponse)
                {
                    using (var reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
                    {
                        var bytes = reader.ReadToEnd();
                        using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(bytes)))
                        {
                            var serializer = new DataContractJsonSerializer(typeof(T));
                            return serializer.ReadObject(stream) as T;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message);
            }
            return result;
        }
    }
}