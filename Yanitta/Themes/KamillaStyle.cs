using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;

namespace Yanitta.Themes
{
    public partial class KamillaStyle
    {
        #region Native

        [StructLayout(LayoutKind.Explicit)]
        struct MONITORINFO
        {
            [FieldOffset(0)]
            public int cbSize;
            [FieldOffset(36)]
            public uint dwFlags;
        }

        [DllImport("user32.dll")]
        static extern bool GetMonitorInfo(IntPtr hMonitor, ref MONITORINFO lpmi);

        [DllImport("user32.dll")]
        static extern IntPtr MonitorFromWindow(IntPtr hwnd, uint dwFlags);

        private bool IsPrimary(Window window)
        {
            var info = new MONITORINFO { cbSize = 40 };
            var handle = new WindowInteropHelper(window).Handle;
            var monitor = MonitorFromWindow(handle, 2);
            GetMonitorInfo(monitor, ref info);
            return (info.dwFlags & 1) != 0;
        }

        #endregion

        public void ForWindowFromTemplate(object element, Action<Window> action)
        {
            if (((FrameworkElement)element).TemplatedParent is Window)
                action(((FrameworkElement)element).TemplatedParent as Window);
        }

        void IconMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount > 1)
                ForWindowFromTemplate(sender, w => SystemCommands.CloseWindow(w));
        }

        void IconMouseUp(object sender, MouseButtonEventArgs e)
        {
            var element = sender as FrameworkElement;
            var point = element.PointToScreen(new Point(element.ActualWidth / 2, element.ActualHeight));
            ForWindowFromTemplate(sender, w => SystemCommands.ShowSystemMenu(w, point));
        }

        void WindowLoaded(object sender, RoutedEventArgs e)
        {
            ((Window)sender).StateChanged += WindowStateChanged;
        }

        void WindowStateChanged(object sender, EventArgs e)
        {
            var window = ((Window)sender);
            var containerBorder = (Border)window.Template.FindName("PART_Container", window);

            if (window.WindowState == WindowState.Maximized)
            {
                if (IsPrimary(window))
                {
                    containerBorder.Padding = new Thickness(
                        SystemParameters.WorkArea.Left + 7,
                        SystemParameters.WorkArea.Top + 7,
                        (SystemParameters.PrimaryScreenWidth - SystemParameters.WorkArea.Right) + 7,
                        (SystemParameters.PrimaryScreenHeight - SystemParameters.WorkArea.Bottom) + 5);
                }
            }
            else
            {
                containerBorder.Padding = new Thickness(7, 7, 7, 5);
            }
        }

        void CloseButtonClick(object sender, RoutedEventArgs e)
        {
            ForWindowFromTemplate(sender, w => SystemCommands.CloseWindow(w));
        }

        void MinButtonClick(object sender, RoutedEventArgs e)
        {
            ForWindowFromTemplate(sender, w => SystemCommands.MinimizeWindow(w));
        }

        void MaxButtonClick(object sender, RoutedEventArgs e)
        {
            ForWindowFromTemplate(sender, w =>
            {
                if (w.WindowState == WindowState.Maximized)
                    SystemCommands.RestoreWindow(w);
                else
                    SystemCommands.MaximizeWindow(w);
            });
        }
    }
}
