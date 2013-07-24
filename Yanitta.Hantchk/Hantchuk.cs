using System;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using Yanitta.Hantchk.Properties;
using Yanitta.Plugins;

namespace Yanitta.Hantchk
{
    [Export(typeof(IYanittaPlugin))]
    public class Hantchuk : IYanittaPlugin
    {
        private const int BOOBER_ANIM = 0x440001;
        private static object lockObject = new object();

        private SettingsWindow settingWindow;
        private WowMemory wowMemory;

        public Hantchuk()
        {
            settingWindow = new SettingsWindow();
            // запретить изменять во время выполнения.
            HotKey.Pressed += hotKey_Pressed;
        }

        private void hotKey_Pressed(object sender, HandledEventArgs e)
        {
            this.IsRuning = !this.IsRuning;

            if (wowMemory == null)
                return;

            if (!this.IsRuning)
                wowMemory.LuaExecute(Settings.Default.LuaCode);
            else
                wowMemory.LuaExecute("if type(StopHantchuk) == \"function\" then StopHantchuk(); end");
        }

        #region Header

        public int Version
        {
            get { return 1; }
        }

        public string Name
        {
            get { return "Hantchuk Bot"; }
        }

        public string Description
        {
            get { return "Fishing Bot"; }
        }

        public bool IsRuning { get; private set; }

        public HotKey HotKey
        {
            get { return Settings.Default.HotKey; }
        }

        public void ShowSettingWindow(Style style)
        {
            if (settingWindow == null || !settingWindow.IsLoaded)
            {
                settingWindow = new SettingsWindow();
                settingWindow.mainGrid.Style = style;
            }

            settingWindow.Show();

            if (!settingWindow.IsActive)
                settingWindow.Activate();
        }

        #endregion Header

        public void ReadMemory(WowMemory memory)
        {
            if (!ObjectManager.Initialized)
            {
                this.wowMemory = memory;
                ObjectManager.Initialize(wowMemory);
            }
            if (this.IsRuning)
            {
                lock (ObjectManager.ObjPulse)
                {
                    // прочитаем все объекты
                    ObjectManager.Pulse();

                    foreach (WoWGameObject gameobject in ObjectManager.Objects)
                    {
                        if (gameobject.CreatedBy == ObjectManager.PlayerGuid
                            && gameobject.AnimationState == BOOBER_ANIM)
                        {
                            new Thread(new ThreadStart(() =>
                            {
                                var boober_guid = gameobject.Guid;
                                // рандомная задержка перед использованием
                                Thread.Sleep(new Random().Next(300, 1500));
                                ObjectManager.Memory.Write<ulong>(ObjectManager.MouseOverGUID, boober_guid);
                                wowMemory.LuaExecute("InteractUnit(\"mouseover\");");
                            })).Start();
                        }
                    }
                }
            }
        }

        public void Dispose()
        {
            Console.WriteLine("Hantchk disposing...");
            if (wowMemory != null && wowMemory.IsRuning)
                wowMemory.Dispose();

            this.settingWindow = null;
            this.wowMemory = null;
            this.IsRuning = false;
        }
    }
}