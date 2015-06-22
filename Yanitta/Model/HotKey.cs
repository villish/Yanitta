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
        private Key key;

        [XmlIgnore]
        private ModifierKeys modifier;


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
        }

        /// <summary>
        ///
        /// </summary>
        [XmlAttribute]
        [NotifyParentProperty(true)]
        public Key Key
        {
            get { return this.key; }
            set
            {
                if (this.key != value)
                {
                    this.key = value;
                    OnPropertyChanged("Key");
                }
            }
        }

        /// <summary>
        ///
        /// </summary>
        [XmlAttribute]
        public ModifierKeys Modifier
        {
            get { return this.modifier; }
            set
            {
                if (this.Modifier != value)
                {
                    this.modifier = value;
                    OnPropertyChanged("Modifier");
                }
            }
        }

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
        public bool IsEmpty
        {
            get { return this.Key == Key.None || this.Modifier == ModifierKeys.None; }
        }

        [XmlIgnore]
        public bool Control
        {
            get { return (this.modifier & ModifierKeys.Control) != 0; }
            set
            {
                if (value)
                    this.modifier |= ModifierKeys.Control;
                else
                    this.modifier &= ~ModifierKeys.Control;

                OnPropertyChanged("Control");
                OnPropertyChanged("Modifier");
            }
        }

        [XmlIgnore]
        public bool Shift
        {
            get { return (this.modifier & ModifierKeys.Shift) != 0; }
            set
            {
                if (value)
                    this.modifier |= ModifierKeys.Shift;
                else
                    this.modifier &= ~ModifierKeys.Shift;

                OnPropertyChanged("Shift");
                OnPropertyChanged("Modifier");
            }
        }

        [XmlIgnore]
        public bool Alt
        {
            get { return (this.modifier & ModifierKeys.Alt) != 0; }
            set
            {
                if (value)
                    this.modifier |= ModifierKeys.Alt;
                else
                    this.modifier &= ~ModifierKeys.Alt;

                OnPropertyChanged("Alt");
                OnPropertyChanged("Modifier");
            }
        }

        [XmlIgnore]
        public bool Windows
        {
            get { return (this.modifier & ModifierKeys.Windows) != 0; }
            set
            {
                if (value)
                    this.modifier |= ModifierKeys.Windows;
                else
                    this.modifier &= ~ModifierKeys.Windows;

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

            var modstr = this.Modifier.ToString()
                .Replace(',', '+')
                .Replace(" ",  "");
            return modstr + "+" + this.Key;
        }
    }
}