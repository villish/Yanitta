using MemoryModule;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Windows.Input;
using System.Windows.Threading;
using Yanitta.Properties;

namespace Yanitta
{
    public delegate void WowMemoryHandler(WowMemory memory);

    public class WowMemory : IDisposable
    {
        #region Properties / Fields

        /// <summary>
        /// Код, который останавливает ротацитю
        /// </summary>
        private const string StopCode =
@"if type(ChangeRotation) == ""function"" then
     ChangeRotation();
 end
 AbilityTable = nil;";

        public event WowMemoryHandler GameStateChanged;

        public int ProcessId
        {
#if !TRACE
            get { return this.Memory.Process.Id; }
#else
            get;
            private set;
#endif
        }

        public Profile CurrentProfile
        {
            get { return ProfileDb.Instance[Class]; }
        }

        public WowClass Class { get; private set; }

        public string Name { get; private set; }

        public bool IsFocus { get; private set; }

        public bool IsInGame { get; private set; }

        public bool IsDisposed { get; private set; }

        public bool IsRuning { get { return mTimer.IsEnabled; } }

        public ProcessMemory Memory { get; private set; }

        private bool IsApplied = false;
        private DispatcherTimer mTimer;

        private uint mCodeCavePtr;
        private uint mDetourPtr;
        private uint mDetour;
        private uint mClientObjectManager;

        private byte[] CltObjMrgSeach = new byte[] { 0xE8, 0x00, 0x00, 0x00, 0x00, 0x68, 0x00, 0x00 };
        private byte[] OverwrittenBytes = new byte[] { 0x55, 0x8B, 0xEC, 0x81, 0xEC, 0x94, 0x00, 0x00, 0x00 };
        private byte[] OverwrittenBytesPattern = new byte[] { 0x55, 0x8B, 0xEC, 0x81, 0xEC, 0x94, 0x00, 0x00, 0x00, 0x83, 0x7D, 0x14, 0x00, 0x56, 0x8B, 0x75 };

        #endregion Properties / Fields

        #region Constructors / Destructor

#if TRACE

        public WowMemory(WowClass wowClass, string name, int pid, bool isfocus = true)
        {
            Class = wowClass;
            Name = name;
            ProcessId = pid;
            IsInGame = true;
            IsFocus = isfocus;
            if (IsFocus)
                GameFocusChanged();
        }

#endif

        public WowMemory(Process process)
        {
            this.Memory = new ProcessMemory(process);

            int build = this.Memory.Process.MainModule.FileVersionInfo.FilePrivatePart;
            if (build != Offsets.Default.Build)
                throw new Exception(string.Format("Current build [{0}] WoW is not supported [{0}]", build, Offsets.Default.Build));

            this.mTimer = new DispatcherTimer();
            this.mTimer.Interval = TimeSpan.FromMilliseconds(500);
            this.mTimer.Tick += (o, e) => this.ReadAllValues();

            this.ReadAllValues();
            this.Start();
        }

        #endregion Constructors / Destructor

        public void Start()
        {
            this.mTimer.IsEnabled = true;
            this.mTimer.Start();
        }

        public void Stop()
        {
            if (this.mTimer != null)
            {
                this.mTimer.Stop();
                this.mTimer.IsEnabled = false;
            }

            if (this.Memory != null && !this.Memory.Process.HasExited)
            {
                if (this.IsInGame && this.Memory.IsOpened)
                {
                    LuaExecute(StopCode);
                }
            }
        }

        private void ReadAllValues()
        {
            if (this.Memory.Process.HasExited || Process.GetProcessById(this.ProcessId) == null)
            {
                this.Stop();
                this.IsInGame = false;

                if (this.GameStateChanged != null)
                    this.GameStateChanged(this);

                Console.WriteLine("Wow process exited!");
                this.Dispose(true);
                return;
            }

            var isInGame = this.Memory.Read<bool>((uint)Offsets.Default.IsInGame, true);

            if (isInGame != this.IsInGame)
            {
                this.IsInGame = isInGame;

                if (this.IsInGame)
                {
                    this.Class = this.Memory.Read<WowClass>((uint)Offsets.Default.PlayerClass, true);
                    this.Name  = this.Memory.ReadString((uint)Offsets.Default.PlayerName, true);
                }

                if (this.GameStateChanged != null)
                    this.GameStateChanged(this);
            }

            if (this.IsInGame)
            {
                (App.Current as App).PluginList.ForEach((plugin) => {
                    if (plugin.IsRuning)
                        plugin.ReadMemory(this);
                });
            }

            if (this.Memory != null && this.Memory.IsFocusWindow != this.IsFocus)
            {
                this.IsFocus = this.Memory.IsFocusWindow;
                this.GameFocusChanged();
            }
        }

        private void HotKeyPressed(object sender, HandledEventArgs e)
        {
            try
            {
                var hotKey = sender as HotKey;
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
                (App.Current as App).PluginList.ForEach((plugin) =>
                {
                    if (!plugin.HotKey.IsEmpty)
                        plugin.HotKey.Unregister();
                });

                if (this.IsFocus)
                {
                    // plugin
                    (App.Current as App).PluginList.ForEach((plugin) =>
                    {
                        if (!plugin.HotKey.IsEmpty)
                            plugin.HotKey.Register();
                    });

                    // main
                    ProfileDb.Instance.Exec((profile, rotation) =>
                    {
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

        public void ExecuteProfile(Rotation rotation)
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
            this.LuaExecute(code, true);
#endif
            //this.LuaExecute("print('Hello wow!');", true);
        }

        #region Injector

        protected void Apply()
        {
            if (this.mDetour == 0u || this.mClientObjectManager == 0u)
            {
                this.mDetour = this.Memory.Find(this.OverwrittenBytesPattern);
                this.mClientObjectManager = this.Memory.Find(this.CltObjMrgSeach, "x???xx?x");

                if (this.mDetour == 0u)
                    throw new NullReferenceException("mDetour not found");

                if (this.mClientObjectManager == 0u)
                    throw new NullReferenceException("mClientObjectManager not found");
            }

            this.Memory.Suspend();

            this.Restore();

            this.mDetourPtr = this.Memory.Alloc(0x256);
            this.mCodeCavePtr = this.Memory.Alloc(0x004);

            #region ASM_x32

            var ASM_Code = new string[]
            {
                "pushfd",
                "pushad",
                "mov  eax, [" + this.mCodeCavePtr + "]",
                "cmp  eax,   0x0",
                "je   @out",
                "call eax",
                "mov  eax, "  + this.mCodeCavePtr,
                "xor  edx,   edx",
                "mov  [eax], edx",
                "@out:",
                "popad",
                "popfd",
                "jmp " + (this.mDetour + this.OverwrittenBytes.Length)
            };

            #endregion ASM_x32

            this.Memory.Write<uint>(this.mCodeCavePtr, 0x00);
            this.Memory.WriteBytes(this.mDetourPtr, this.OverwrittenBytes);

            var injAddr = (uint)(this.mDetourPtr + this.OverwrittenBytes.Length);

            this.Inject(ASM_Code, injAddr);
            this.Inject(new[] { "jmp " + this.mDetourPtr }, this.mDetour, false);

            this.Memory.Resume();

            this.IsApplied = true;
        }

        protected void Restore()
        {
            if (this.IsApplied)
            {
                this.Memory.WriteBytes(this.mDetour, this.OverwrittenBytes);
                this.IsApplied = false;
            }
        }

        public string LuaExecute(string sCommand, bool simple = true, string value = "nil")
        {
            if (!this.IsApplied)
                this.Apply();

            var bCommands = Encoding.UTF8.GetBytes(sCommand);
            var bArguments = Encoding.UTF8.GetBytes(value);

            var commandAdr = this.Memory.Alloc(bCommands.Length + 1);
            var argumentsAdr = this.Memory.Alloc(bArguments.Length + 1);
            var resultAdr = this.Memory.Alloc(0x0004);
            var injAddress = this.Memory.Alloc(0x4096);

            this.Memory.WriteBytes(commandAdr, bCommands);
            this.Memory.WriteBytes(argumentsAdr, bArguments);

            #region ASM_x32

            string[] asmCode = new string[]
            {
                "mov   eax, " + commandAdr,
                "push  0",
                "push  eax",
                "push  eax",
                "mov   eax, " + this.Memory.Rebase(Offsets.Default.FrameScript_ExecuteBuffer),
                "call  eax",
                "add   esp, 0xC",
                "call  " + mClientObjectManager,//Settings.Default.ClntObjMgrGetActivePlayer,
                "test  eax, eax",
                "je    @out",
                "mov   ecx, eax",
                "push  -1",
                "mov   edx, " + argumentsAdr,
                "push  edx",
                "call  " + this.Memory.Rebase(Offsets.Default.FrameScript_GetLocalizedText),
                "mov   [" + resultAdr + "], eax",
                "@out:",
                "retn"
             };

            #endregion ASM_x32

            this.Inject(asmCode, injAddress);

            this.Memory.Write<uint>(this.mCodeCavePtr, injAddress);

            int tickCount = Environment.TickCount;
            int res;
            while ((res = this.Memory.Read<int>(mCodeCavePtr)) != 0)
            {
                if ((tickCount + 0xBB8) < Environment.TickCount)
                {
                    Console.WriteLine("Out of time");
                    break;
                }
                Thread.Sleep(10);
            }

            var result = this.Memory.Read<uint>(resultAdr);

            var resStr = "";
            if (result != 0u)
            {
                this.Memory.ReadString(result);
            }
            this.Memory.Free(commandAdr);
            this.Memory.Free(argumentsAdr);
            this.Memory.Free(resultAdr);
            this.Memory.Free(injAddress);

            if (simple)
                this.Restore();

            return resStr;
        }

        private void Inject(IEnumerable<string> ASM_Code, uint address, bool randomize = true)
        {
            //if (randomize)
            //    ASM_Code = Extensions.RandomizeASM(ASM_Code);

            try
            {
                this.Memory.Inject(ASM_Code, address);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        #endregion Injector

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

        private void Dispose(bool disposing)
        {
            if (this.IsDisposed || !disposing)
                return;

            this.Stop();

            if (this.Memory != null)
            {
                if (this.Memory.IsOpened)
                    this.Restore();

                this.mClientObjectManager = 0u;
                this.mCodeCavePtr = 0u;
                this.mDetourPtr = 0u;
                this.mDetour = 0u;

                this.Memory.Dispose();
            }
            this.Memory = null;
            this.IsDisposed = true;
        }
    }
}