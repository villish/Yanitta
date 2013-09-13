﻿using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using System.Xml.Serialization;

namespace System.Windows.Input
{
    /// <summary>
    ///
    /// </summary>
    [Serializable]
    public class HotKey : IDisposable, INotifyPropertyChanged
    {
        private HwndSource m_HandleSource = null;

        #region Win API

        private const int WM_HOTKEY = 0x312;

        [DllImport("user32.dll", EntryPoint = "RegisterHotKey", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool apiRegisterHotKey(IntPtr hWnd, int id, ModifierKeys fsModifiers, int vk);

        [DllImport("user32.dll", EntryPoint = "UnregisterHotKey", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool apiUnregisterHotKey(IntPtr hWnd, int id);

        #endregion Win API

        /// <summary>
        ///
        /// </summary>
        public event EventHandler<HandledEventArgs> Pressed;

        /// <summary>
        ///
        /// </summary>
        public HotKey()
            : this(Key.None, ModifierKeys.None)
        {
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="key"></param>
        /// <param name="modifier"></param>
        public HotKey(Key key, ModifierKeys modifier)
        {
            this.Key = key;
            this.Modifier = modifier;

            this.m_HandleSource = new HwndSource(new HwndSourceParameters());
            this.m_HandleSource.AddHook(HwndSourceHook);
            this.m_HandleSource.Disposed += (o, e) =>
            {
                if (!this.IsEmpty)
                {
                    apiUnregisterHotKey(this.m_HandleSource.Handle, this.RawHotKey);
                }
                this.IsRegistered = false;
            };
        }

        private IntPtr HwndSourceHook(IntPtr hwnd, int message, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (message == WM_HOTKEY)
            {
                if (lParam.ToInt32() == this.RawHotKey)
                {
                    var handledEventArgs = new HandledEventArgs();

                    if (this.Pressed != null)
                    {
                        this.Pressed(this, handledEventArgs);
                        handled = handledEventArgs.Handled;
                    }

                    return new IntPtr(1);
                }
            }

            return IntPtr.Zero;
        }

        /// <summary>
        /// Defines a system-wide hot key.
        /// </summary>
        /// <param name="window">The window that will receive messages generated by the hot key.</param>
        public void Register()
        {
            if (this.IsEmpty)
                throw new Exception("HotKey is empty!");

            this.Unregister();

            var intKey = KeyInterop.VirtualKeyFromKey(this.Key);
            this.IsRegistered = apiRegisterHotKey(this.m_HandleSource.Handle, this.RawHotKey, this.Modifier, intKey);

            if (!this.IsRegistered)
                throw new ApplicationException("HotKey (" + this + ") already in use");
        }

        /// <summary>
        /// Frees a hot key previously registered by the calling thread.
        /// </summary>
        public void Unregister()
        {
            if (this.m_HandleSource != null && !this.m_HandleSource.IsDisposed && !this.IsEmpty)
                apiUnregisterHotKey(this.m_HandleSource.Handle, this.RawHotKey);

            this.IsRegistered = false;
        }

        /// <summary>
        /// <see cref="System.Dispose"/>
        /// </summary>
        public void Dispose()
        {
            this.Unregister();

            if (this.m_HandleSource != null && !this.m_HandleSource.IsDisposed)
            {
                this.m_HandleSource.RemoveHook(HwndSourceHook);
                this.m_HandleSource.Dispose();
            }
            this.m_HandleSource = null;

            this.Modifier = ModifierKeys.None;
            this.Key      = Key.None;
            this.Tag      = null;
            this.Pressed  = null;
        }

        ~HotKey()
        {
            Dispose();
        }

        private Key key;

        /// <summary>
        ///
        /// </summary>
        [XmlAttribute]
        [NotifyParentProperty(true)]
        public Key Key { 
            get { return key; } 
            set
            {
                key = value;
                SendNotify("Key"); 
            }
        }

        /// <summary>
        ///
        /// </summary>
        [XmlAttribute]
        public ModifierKeys Modifier { get; set; }

        /// <summary>
        ///
        /// </summary>
        [XmlIgnore]
        public bool IsRegistered { get; private set; }

        /// <summary>
        ///
        /// </summary>
        [XmlIgnore]
        public int RawHotKey
        {
            get { return KeyInterop.VirtualKeyFromKey(this.Key) << 16 | (int)this.Modifier & 0xFFFF; }
        }

        /// <summary>
        ///
        /// </summary>
        [XmlIgnore]
        public object Tag { get; set; }

        /// <summary>
        ///
        /// </summary>
        public bool IsEmpty
        {
            get { return this.Key == Key.None || this.Modifier == ModifierKeys.None; }
        }

        [XmlIgnore]
        public bool Control
        {
            get { return (this.Modifier & ModifierKeys.Control) != 0; }
            set
            {
                if (value)
                    this.Modifier |= ModifierKeys.Control;
                else
                    this.Modifier &= ~ModifierKeys.Control;

                SendNotify("Control");
            }
        }

        [XmlIgnore]
        public bool Shift
        {
            get { return (this.Modifier & ModifierKeys.Shift) != 0; }
            set
            {
                if (value)
                    this.Modifier |= ModifierKeys.Shift;
                else
                    this.Modifier &= ~ModifierKeys.Shift;

                SendNotify("Shift");
            }
        }

        [XmlIgnore]
        public bool Alt
        {
            get { return (this.Modifier & ModifierKeys.Alt) != 0; }
            set
            {
                if (value)
                    this.Modifier |= ModifierKeys.Alt;
                else
                    this.Modifier &= ~ModifierKeys.Alt;

                SendNotify("Alt");
            }
        }

        [XmlIgnore]
        public bool Windows
        {
            get { return (this.Modifier & ModifierKeys.Windows) != 0; }
            set
            {
                if (value)
                    this.Modifier |= ModifierKeys.Windows;
                else
                    this.Modifier &= ~ModifierKeys.Windows;

                SendNotify("Windows");
            }
        }

        private void SendNotify(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (!(obj is HotKey))
                return false;
            var mhotKey = obj as HotKey;
            return (this.Key == mhotKey.Key && this.Modifier == mhotKey.Modifier);
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return this.RawHotKey ^ this.RawHotKey;
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            if (this.Key == Key.None)
                return "(Empty)";

            if (this.Modifier == ModifierKeys.None)
                return this.Key.ToString();

            var modstr = this.Modifier.ToString().Replace(',', '+');
            return modstr + "+" + this.Key;
        }

        public void SetHandler(object tag, EventHandler<HandledEventArgs> handler)
        {
            this.Tag = tag;
            Pressed = handler;
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}