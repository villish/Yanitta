using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Data;
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

        protected void Move<T>(ObservableCollection<T> list, int position)
        {
            var item = (T)CollectionViewSource.GetDefaultView(list)?.CurrentItem;
            list.Move(list.IndexOf(item), list.IndexOf(item) + position);
        }

        protected bool CanMove<T>(ObservableCollection<T> list, int position)
        {
            var item = (T)CollectionViewSource.GetDefaultView(list)?.CurrentItem;
            return list != null && list.IndexOf(item) + position >= 0 && list.IndexOf(item) + position < list.Count;
        }

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
