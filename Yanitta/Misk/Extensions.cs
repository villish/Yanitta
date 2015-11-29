using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace Yanitta
{
    static class Extensions
    {
        public static BitmapImage GetIconFromEnum(Enum value)
        {
            var img = Application.Current.TryFindResource(value?.ToString()) as Image;
            if (img?.Source == null)
                img = Application.Current.TryFindResource("None") as Image;
            if (img?.Source == null)
                return null;
            return new BitmapImage(new Uri(img.Source.ToString()));
        }
    }
}