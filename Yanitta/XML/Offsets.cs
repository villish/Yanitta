﻿using System;
using System.IO;
using System.Windows;
using System.Xml.Serialization;

namespace Yanitta
{
    [Serializable]
    public class Offsets : DependencyObject
    {
        public static readonly DependencyProperty BuildProperty           = DependencyProperty.Register("Build",           typeof(int),  typeof(Offsets));
        public static readonly DependencyProperty PlayerNameProperty      = DependencyProperty.Register("PlayerName",      typeof(long), typeof(Offsets));
        public static readonly DependencyProperty PlayerClassProperty     = DependencyProperty.Register("PlayerClass",     typeof(long), typeof(Offsets));
        public static readonly DependencyProperty IsInGameProperty        = DependencyProperty.Register("IsInGame",        typeof(long), typeof(Offsets));
        public static readonly DependencyProperty ExecuteBufferProperty   = DependencyProperty.Register("ExecuteBuffer",   typeof(long), typeof(Offsets));
        public static readonly DependencyProperty InjectedAddressProperty = DependencyProperty.Register("InjectedAddress", typeof(long), typeof(Offsets));
        private const string fileName = "offsets.xml";

        [XmlElement]
        public int Build
        {
            get { return (int)GetValue(BuildProperty); }
            set { SetValue(BuildProperty, value); }
        }

        [XmlElement]
        public long PlayerName
        {
            get { return (long)GetValue(PlayerNameProperty); }
            set { SetValue(PlayerNameProperty, value); }
        }

        [XmlElement]
        public long PlayerClass
        {
            get { return (long)GetValue(PlayerClassProperty); }
            set { SetValue(PlayerClassProperty, value); }
        }

        [XmlElement]
        public long IsInGame
        {
            get { return (long)GetValue(IsInGameProperty); }
            set { SetValue(IsInGameProperty, value); }
        }

        [XmlElement]
        public long ExecuteBuffer
        {
            get { return (long)GetValue(ExecuteBufferProperty); }
            set { SetValue(ExecuteBufferProperty, value); }
        }

        [XmlElement]
        public long InjectedAddress
        {
            get { return (long)GetValue(InjectedAddressProperty); }
            set { SetValue(InjectedAddressProperty, value); }
        }

        static Offsets()
        {
            if (File.Exists(fileName))
                Default = XmlManager.Load<Offsets>(fileName);
            else
                Default = new Offsets();
        }

        public static Offsets Default { get; set; }

        public void Save()
        {
            XmlManager.Save(fileName, this);
        }
    }
}