using System.Collections.Generic;
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void Set<T>(ref T field, T value, [CallerMemberName]string propertyName = "", params string[] fields)
        {
            if (!EqualityComparer<T>.Default.Equals(field, value))
            {
                field = value;
                OnPropertyChanged(propertyName);

                foreach (var fname in fields)
                    OnPropertyChanged(fname);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void OnPropertyChanged([CallerMemberName]string propertyName = "")
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
