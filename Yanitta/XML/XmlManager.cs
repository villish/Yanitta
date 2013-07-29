using System;
using System.IO;
using System.Xml.Serialization;

namespace Yanitta
{
    public class XmlManager
    {
        private string path;

        public XmlManager(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentNullException("path");

            this.path = path;
        }

        public T Load<T>() where T : class
        {
            if (!File.Exists(path))
                throw new FileNotFoundException("File not found", path);

            using (var fstream = File.OpenRead(path))
            {
                var serialiser = new XmlSerializer(typeof(T));

                serialiser.UnknownAttribute += (o, e) => {
                    Console.WriteLine("Unknown attribute: {0} at line: {1} position: {2}",
                        e.Attr, e.LineNumber, e.LinePosition);
                };

                serialiser.UnknownElement += (o, e) => {
                    Console.WriteLine("Unknown Element: {0} at line: {1} position: {2}",
                        e.Element, e.LineNumber, e.LinePosition);
                };

                return (T)serialiser.Deserialize(fstream);
            }
        }

        public void Save<T>(object obj) where T : class
        {
            if (obj == null)
                throw new ArgumentNullException("obj");

            using (var fstream = File.Open(path, FileMode.Create))
            {
                var serialiser = new XmlSerializer(typeof(T));
                serialiser.Serialize(fstream, obj);
                fstream.Flush();
            }
        }
    }
}