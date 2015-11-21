using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace Yanitta
{
    static class Extensions
    {
        public static BitmapImage GetClassIcon(WowClass @class)
        {
            var img = Application.Current.TryFindResource(@class.ToString()) as Image;
            if (img == null)
                img = Application.Current.TryFindResource("None") as Image;
            if (img == null)
                throw new Exception();
            return new BitmapImage(new Uri(img.Source.ToString()));
        }
    }
}
