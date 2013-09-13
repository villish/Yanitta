using System;
using System.IO;
using System.Windows;
using System.Xml.Serialization;

namespace Yanitta
{
    [Serializable]
    public class Offsets : DependencyObject
    {
        public static readonly DependencyProperty BuildProperty                         = DependencyProperty.Register("Build",                          typeof(int),  typeof(Offsets));
        public static readonly DependencyProperty PlayerNameProperty                    = DependencyProperty.Register("PlayerName",                     typeof(long), typeof(Offsets));
        public static readonly DependencyProperty PlayerClassProperty                   = DependencyProperty.Register("PlayerClass",                    typeof(long), typeof(Offsets));
        public static readonly DependencyProperty IsInGameProperty                      = DependencyProperty.Register("IsInGame",                       typeof(long), typeof(Offsets));
        public static readonly DependencyProperty FrameScript_ExecuteBufferProperty     = DependencyProperty.Register("FrameScript_ExecuteBuffer",      typeof(long), typeof(Offsets));
        public static readonly DependencyProperty FrameScript_GetLocalizedTextProperty  = DependencyProperty.Register("FrameScript_GetLocalizedText",   typeof(long), typeof(Offsets));
        public static readonly DependencyProperty ClntObjMgrGetActivePlayerObjProperty  = DependencyProperty.Register("ClntObjMgrGetActivePlayerObj",   typeof(long), typeof(Offsets));
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
        public long FrameScript_ExecuteBuffer
        {
            get { return (long)GetValue(FrameScript_ExecuteBufferProperty); }
            set { SetValue(FrameScript_ExecuteBufferProperty, value); }
        }

        [XmlElement]
        public long FrameScript_GetLocalizedText
        {
            get { return (long)GetValue(FrameScript_GetLocalizedTextProperty); }
            set { SetValue(FrameScript_GetLocalizedTextProperty, value); }
        }

        [XmlElement]
        public long ClntObjMgrGetActivePlayerObj
        {
            get { return (long)GetValue(ClntObjMgrGetActivePlayerObjProperty); }
            set { SetValue(ClntObjMgrGetActivePlayerObjProperty, value); }
        }

        static Offsets()
        {
            if (File.Exists(fileName))
                Default = new XmlManager(fileName).Load<Offsets>();
            else
                Default = new Offsets();
        }

        public static Offsets Default { get; set; }

        public void Save()
        {
            new XmlManager(fileName).Save<Offsets>(this);
        }
    }
}