using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Xml;

namespace Yanitta
{
    public class ViewModelBase : INotifyPropertyChanged
    {
        #region Xml extensions

        protected string GetTrimValue(XmlCDataSection cdataSection)
        {
            if (cdataSection == null || string.IsNullOrWhiteSpace(cdataSection.Value))
                return string.Empty;
            return cdataSection.Value.Trim();
        }

        protected XmlCDataSection CreateCDataSection(string content)
        {
            if (string.IsNullOrWhiteSpace(content))
                return null;
            return new XmlDocument().CreateCDataSection("\n" + content + "\n");
        }

        #endregion

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
