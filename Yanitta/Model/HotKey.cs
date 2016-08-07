using System;
using System.ComponentModel;
using System.Windows.Input;
using System.Xml.Serialization;

namespace Yanitta
{
    /// <summary>
    ///
    /// </summary>
    [Serializable]
    public class HotKey : ViewModelBase
    {
        [XmlIgnore]
        Key key;

        [XmlIgnore]
        ModifierKeys modifier;

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
            Key = key;
            Modifier = modifier;
        }

        /// <summary>
        ///
        /// </summary>
        [XmlAttribute]
        [NotifyParentProperty(true)]
        public Key Key
        {
            get { return key; }
            set { Set(ref key, value); }
        }

        /// <summary>
        ///
        /// </summary>
        [XmlAttribute]
        public ModifierKeys Modifier
        {
            get { return modifier; }
            set { Set(ref modifier, value); }
        }

        /// <summary>
        ///
        /// </summary>
        [XmlIgnore]
        public int RawHotKey => KeyInterop.VirtualKeyFromKey(Key) << 16 | (int)Modifier & 0xFFFF;

        /// <summary>
        ///
        /// </summary>
        public bool IsEmpty => Key == Key.None || Modifier == ModifierKeys.None;

        [XmlIgnore]
        public bool Control
        {
            get { return (modifier & ModifierKeys.Control) != 0; }
            set
            {
                if (value)
                    modifier |= ModifierKeys.Control;
                else
                    modifier &= ~ModifierKeys.Control;

                OnPropertyChanged("Control");
                OnPropertyChanged("Modifier");
            }
        }

        [XmlIgnore]
        public bool Shift
        {
            get { return (modifier & ModifierKeys.Shift) != 0; }
            set
            {
                if (value)
                    modifier |= ModifierKeys.Shift;
                else
                    modifier &= ~ModifierKeys.Shift;

                OnPropertyChanged("Shift");
                OnPropertyChanged("Modifier");
            }
        }

        [XmlIgnore]
        public bool Alt
        {
            get { return (modifier & ModifierKeys.Alt) != 0; }
            set
            {
                if (value)
                    modifier |= ModifierKeys.Alt;
                else
                    modifier &= ~ModifierKeys.Alt;

                OnPropertyChanged("Alt");
                OnPropertyChanged("Modifier");
            }
        }

        [XmlIgnore]
        public bool Windows
        {
            get { return (modifier & ModifierKeys.Windows) != 0; }
            set
            {
                if (value)
                    modifier |= ModifierKeys.Windows;
                else
                    modifier &= ~ModifierKeys.Windows;

                OnPropertyChanged("Windows");
                OnPropertyChanged("Modifier");
            }
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
            return (Key == mhotKey.Key && Modifier == mhotKey.Modifier);
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode() => RawHotKey ^ RawHotKey;

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            if (Key == Key.None)
                return "(Empty)";

            if (Modifier == ModifierKeys.None)
                return Key.ToString();

            var modstr = Modifier.ToString()
                .Replace(',', '+')
                .Replace(" ",  "");
            return modstr + "+" + Key;
        }
    }
}