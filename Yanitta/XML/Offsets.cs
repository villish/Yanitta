using System;
using System.ComponentModel;
using System.IO;
using System.Xml.Serialization;

namespace Yanitta
{
    [Serializable]
    public class Offsets : INotifyPropertyChanged
    {
        [XmlIgnore]
        private const string fileName = "offsets.xml";

        [XmlIgnore]
        private int build;

        [XmlIgnore]
        private long playerName;

        [XmlIgnore]
        private long playerClass;

        [XmlIgnore]
        private long isInGame;

        [XmlIgnore]
        private long frameScript_ExecuteBuffer;

        [XmlIgnore]
        private long frameScript_GetLocalizedText;

        [XmlElement]
        public int Build
        {
            get { return this.build; }
            set
            {
                if (value != build)
                {
                    build = value;
                    OnPropertyChanged("Build");
                }
            }
        }

        [XmlElement]
        public long PlayerName
        {
            get { return this.playerName; }
            set
            {
                if (value != playerName)
                {
                    playerName = value;
                    OnPropertyChanged("PlayerName");
                }
            }
        }

        [XmlElement]
        public long PlayerClass
        {
            get { return this.playerClass; }
            set
            {
                if (value != playerClass)
                {
                    playerClass = value;
                    OnPropertyChanged("PlayerClass");
                }
            }
        }

        [XmlElement]
        public long IsInGame
        {
            get { return this.isInGame; }
            set
            {
                if (value != isInGame)
                {
                    isInGame = value;
                    OnPropertyChanged("IsInGame");
                }
            }
        }

        [XmlElement]
        public long FrameScript_ExecuteBuffer
        {
            get { return this.frameScript_ExecuteBuffer; }
            set
            {
                if (value != frameScript_ExecuteBuffer)
                {
                    frameScript_ExecuteBuffer = value;
                    OnPropertyChanged("FrameScript_ExecuteBuffer");
                }
            }
        }

        [XmlElement]
        public long FrameScript_GetLocalizedText
        {
            get { return this.frameScript_GetLocalizedText; }
            set
            {
                if (value != frameScript_GetLocalizedText)
                {
                    frameScript_GetLocalizedText = value;
                    OnPropertyChanged("FrameScript_GetLocalizedText");
                }
            }
        }

        static Offsets()
        {
            if (File.Exists(fileName))
                Default = new XmlManager(fileName).Load<Offsets>();
            else
                Default = new Offsets();
        }

        [XmlIgnore]
        public static Offsets Default { get; private set; }

        public void Save()
        {
            new XmlManager(fileName)
                .Save<Offsets>(this);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }
    }
}