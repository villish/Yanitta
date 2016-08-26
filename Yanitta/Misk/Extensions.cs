using System;
using System.Collections.Generic;
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

        public static bool IsWowClass(this WowSpecializations value, WowClass wowClass)
            => (WowClass)((int)value >> 16) == wowClass;
        public static int SpecId(this WowSpecializations value) => (int)value & 0xFFFF;

        public static IEnumerable<WowSpecializations> GetSpecList(this WowClass wowClass)
        {
            foreach (WowSpecializations spec in Enum.GetValues(typeof(WowSpecializations)))
                if (spec.IsWowClass(wowClass) || spec == WowSpecializations.None)
                    yield return spec;
        }
    }
}