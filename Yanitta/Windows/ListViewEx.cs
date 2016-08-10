using System.Collections.Specialized;
using System.Windows.Controls;

namespace Yanitta.Windows
{
    public class ListViewEx : ListView
    {
        public event NotifyCollectionChangedEventHandler ItemsChanged;

        protected override void OnItemsChanged(NotifyCollectionChangedEventArgs e)
        {
            ItemsChanged?.Invoke(this, e);
            base.OnItemsChanged(e);
        }
    }
}
