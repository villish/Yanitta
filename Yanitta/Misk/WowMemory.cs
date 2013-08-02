using MemoryModule;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Yanitta.Properties;

namespace Yanitta
{
    public delegate void WowMemoryHandler(WowMemory memory);

    public class WowMemory : DependencyObject, IDisposable
    {
        public readonly static DependencyProperty CurrentProfileProperty = DependencyProperty.Register("CurrentProfile",    typeof(Profile),    typeof(WowMemory));
        public readonly static DependencyProperty ClassProperty          = DependencyProperty.Register("Class",             typeof(WowClass),   typeof(WowMemory));
        public readonly static DependencyProperty NameProperty           = DependencyProperty.Register("Name",              typeof(string),     typeof(WowMemory));
        public readonly static DependencyProperty IsInGameProperty       = DependencyProperty.Register("IsInGame",          typeof(bool),       typeof(WowMemory),
            new PropertyMetadata(false, OnIsInGamePropertyChanged));

        private static void OnIsInGamePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != e.OldValue && d is WowMemory)
            {
#if !TRACE
                (d as WowMemory).ReadPlayerData();
#endif
            }
        }

        public event WowMemoryHandler GameExited;

        public int ProcessId
        {
#if !TRACE
            get { return this.Memory.Process.Id; }
#else
            get; private set;
#endif
        }

        public Profile CurrentProfile
        {
            get { return (Profile)GetValue(CurrentProfileProperty); }
            private set { SetValue(CurrentProfileProperty, value);  }
        }

        public WowClass Class
        {
            get { return (WowClass)GetValue(ClassProperty); }
            private set { SetValue(ClassProperty, value);   }
        }

        public string Name
        {
            get { return (string)GetValue(NameProperty); }
            private set { SetValue(NameProperty, value); }
        }

        public bool IsInGame
        {
            get { return (bool)GetValue(IsInGameProperty);   }
            private set { SetValue(IsInGameProperty, value); }
        }

        public ProcessMemory Memory     { get; private set; }
        public LuaHook       LuaHook    { get; private set; }
        public bool          IsDisposed { get; private set; }

        private DispatcherTimer mTimer;
        private bool IsFocus;

#if TRACE

        public WowMemory(WowClass wowClass, string name, int pid, bool isfocus = true)
        {
            ProcessId = pid;
            IsInGame  = isfocus;
            IsFocus   = isfocus;
            if (IsFocus)
            {
                Class = wowClass;
                Name  = name;

                SetValue(CurrentProfileProperty, ProfileDb.Instance[this.Class]);
                GameFocusChanged();
            }
        }

#endif

        public WowMemory(Process process)
        {
            this.Memory = new ProcessMemory(process);

            int build = this.Memory.Process.MainModule.FileVersionInfo.FilePrivatePart;
            if (build != Offsets.Default.Build)
                throw new Exception(string.Format("Current build [{0}] WoW is not supported [{0}]", build, Offsets.Default.Build));

            this.LuaHook = new LuaHook(this.Memory);

            this.mTimer = new DispatcherTimer();
            this.mTimer.Interval = TimeSpan.FromMilliseconds(500);
            this.mTimer.Tick += (o, e) => this.ReadAllValues();

            this.ReadAllValues();

            this.mTimer.IsEnabled = true;
            this.mTimer.Start();
        }

        private void ReadPlayerData()
        {
            if (this.IsInGame)
            {
                this.Class = this.Memory.Read<WowClass>((IntPtr)Offsets.Default.PlayerClass, true);
                this.Name  = this.Memory.ReadString((IntPtr)Offsets.Default.PlayerName, true);
                this.CurrentProfile = ProfileDb.Instance[this.Class];
            }
            else
            {
                this.Class = (WowClass)(byte)0;
                this.Name  = "";
                this.CurrentProfile = null;
            }
        }

        private bool CheckProcess()
        {
            if (this.Memory.Process.HasExited || Process.GetProcessById(this.ProcessId) == null)
            {
                this.IsInGame = false;
                if (this.GameExited != null)
                    this.GameExited(this);

                Console.WriteLine("Wow process exited!");
                this.Dispose();
                return false;
            }
            return true;
        }

        private void ReadAllValues()
        {
            if (!CheckProcess())
                return;

            this.IsInGame = this.Memory.Read<bool>((IntPtr)Offsets.Default.IsInGame, true);

            if (this.IsInGame)
            {
                (App.Current as App).PluginList.ForEach((plugin) => {
                    if (plugin.IsRuning)
                        plugin.ReadMemory(this);
                });
            }

            if (this.Memory.IsFocusWindow != this.IsFocus)
            {
                this.IsFocus = this.Memory.IsFocusWindow;
                this.GameFocusChanged();
            }
        }

        private void HotKeyPressed(object sender, HandledEventArgs e)
        {
            try
            {
                var hotKey   = sender as HotKey;
                var ratation = hotKey.Tag as Rotation;

                ExecuteProfile(ratation);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error when injected rotation: {0}", ex.Message);
            }

            e.Handled = true;
        }

        private void GameFocusChanged()
        {
            try
            {
                // main
                ProfileDb.Instance.Exec((profile, rotation) => rotation.HotKey.Unregister());

                // plugin
                (App.Current as App).PluginList.ForEach((plugin) => {
                    if (!plugin.HotKey.IsEmpty)
                        plugin.HotKey.Unregister();
                });

                if (this.IsFocus)
                {
                    // plugin
                    (App.Current as App).PluginList.ForEach((plugin) => {
                        if (!plugin.HotKey.IsEmpty)
                            plugin.HotKey.Register();
                    });

                    // main
                    ProfileDb.Instance.Exec((profile, rotation) => {
                        if (profile.Class == Class)
                        {
                            rotation.HotKey.SetHandler(rotation, HotKeyPressed);
                            rotation.HotKey.Register();
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("HotKey Error: {0}", ex.Message);
            }
        }

        private void ExecuteProfile(Rotation rotation)
        {
            if (CurrentProfile == null)
                throw new NullReferenceException("CurrentProfile is null");

            if (rotation == null)
                throw new ArgumentNullException("rotation is null");

            var abilityQueue = CurrentProfile[rotation];

            if (abilityQueue == null)
                throw new Exception("abilityQueue is null");

            var builder = new StringBuilder();

            builder.AppendLine(ProfileDb.Instance.Core);
            builder.AppendLine(ProfileDb.Instance.Func);
            builder.AppendLine(CurrentProfile.Lua);

            foreach (var ability in abilityQueue)
            {
                var ability_code = ability.ToString();
                builder.AppendLine(ability_code);
            }

            builder.AppendLine();
            builder.AppendFormatLine(@"ShowInChat   = {0};", Settings.Default.ShowChat.ToString().ToLower());
            builder.AppendFormatLine(@"DebugEnabled = {0};", Settings.Default.DebugMode.ToString().ToLower());
            // Запуск ротации
            builder.AppendFormatLine(@"ChangeRotation(""{0}"", [[{1}]]);", rotation.Name, rotation.Notes);

            var code = builder.ToString();

            System.IO.File.WriteAllText("InjectedLuaCode.lua", code);

#if !TRACE
            this.LuaHook.LuaExecute(code);
#endif
            //this.LuaExecute("print('Hello wow!');", true);
        }

        public override string ToString()
        {
            return string.Format("[{0}] {1} ({2})", this.ProcessId, this.Name, this.Class);
        }

        ~WowMemory()
        {
            Dispose(false);
        }

        /// <summary>
        /// Closes an open wow memory.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        private const string StopCode =
@"if type(ChangeRotation) == ""function"" then
     ChangeRotation();
 end
 AbilityTable = nil;";

        private void Dispose(bool disposing)
        {
            if (this.IsDisposed || !disposing)
                return;

            if (this.mTimer != null)
            {
                this.mTimer.Stop();
                this.mTimer.IsEnabled = false;
            }

            if (this.Memory != null && !this.Memory.Process.HasExited)
            {
                if (this.IsInGame && this.Memory.IsOpened)
                {
                    this.LuaHook.LuaExecute(StopCode);
                }
                this.Memory.Dispose();
            }
            this.Memory     = null;
            this.IsDisposed = true;
        }
    }
}