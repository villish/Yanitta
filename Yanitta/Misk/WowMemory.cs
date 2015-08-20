using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Input;
using System.Windows.Threading;
using Yanitta.Properties;

namespace Yanitta
{
    /// <summary>
    /// Определяет обработчик для <see cref="Yanitta.WowMemory"/>.
    /// </summary>
    /// <param name="memory"></param>
    public delegate void WowMemoryHandler(WowMemory memory);

    /// <summary>
    /// Посредник для взаимодействия с процессом.
    /// </summary>
    public class WowMemory : ViewModelBase, IDisposable
    {
        #region Win API

        /// <summary>
        /// The HOOKPROC type defines a pointer to this callback function.
        /// </summary>
        /// <param name="code">A code the hook procedure uses to determine how to process the message.</param>
        /// <param name="wParam">Keyboard action.</param>
        /// <param name="lParam">The virtual-key code of the key that generated the keystroke message.</param>
        /// <returns></returns>
        [UnmanagedFunctionPointer(CallingConvention.StdCall, SetLastError = true)]
        delegate IntPtr KeyBoardProc(int code, IntPtr wParam, IntPtr lParam);

        [SuppressMessage("Microsoft.Design", "CA1060:MovePInvokesToNativeMethodsClass")]
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern IntPtr SetWindowsHookEx(int idHook, KeyBoardProc lpfn, IntPtr hMod, uint dwThreadId);

        [SuppressMessage("Microsoft.Design", "CA1060:MovePInvokesToNativeMethodsClass")]
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [SuppressMessage("Microsoft.Design", "CA1060:MovePInvokesToNativeMethodsClass")]
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        #endregion

        /// <summary>
        /// Событие для обработки закрытия процесса.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1009:DeclareEventHandlersCorrectly")]
        public event WowMemoryHandler GameExited;

        // Сохраним ссылку на обработчик, чтобы ее не трогал сборщик мусора.
        private KeyBoardProc KeyBoardProck;

        private bool isInGame;

        /// <summary>
        /// Текущий процесс <see cref="Yanitta.ProcessMemory"/>
        /// </summary>
        public ProcessMemory Memory { get; private set; }

        public Offsets Offsets { get; private set; }

        private IntPtr keyboardHook;
        private bool IsDisposed;
        private DispatcherTimer mTimer;

        /// <summary>
        /// Id процесса.
        /// </summary>
        public int ProcessId
        {
            get { return this.Memory == null ? 0 : this.Memory.Process.Id; }
        }

        public string ProcessName
        {
            get
            {
                return string.Format("{0}_{1}",
                    this.Memory.Process.ProcessName,
                    this.Memory.Process.MainModule.FileVersionInfo.FilePrivatePart);
            }
        }

        public IEnumerable<Rotation> Rotations
        {
            get
            {
                if (this.IsInGame)
                {
                    foreach (var rotation in ProfileDb.Instance[this.Class].RotationList)
                        yield return rotation;

                    foreach (var rotation in ProfileDb.Instance[WowClass.None].RotationList)
                        yield return rotation;
                }
            }
        }

        /// <summary>
        /// Класс персонажа.
        /// </summary>
        public WowClass Class { get; private set; }

        /// <summary>
        /// Имя персонажа.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Состояние, находится ли персонаж в игровом мире.
        /// </summary>
        public bool IsInGame
        {
            get { return this.isInGame; }
            private set
            {
                if (this.isInGame != value)
                {
                    this.isInGame = value;
                    this.OnPropertyChanged("IsInGame");

                    this.Class = value ? (WowClass)this.Memory.Read<byte>(this.Memory.Rebase(this.Offsets.PlayerClass)) : WowClass.None;
                    this.Name  = value ? this.Memory.ReadString(this.Memory.Rebase(this.Offsets.PlayerName)) : string.Empty;

                    if (this.Class < WowClass.None || this.Class > WowClass.Druid)
                        throw new Exception("Unsuported wow class: " + this.Class);

                    ProfileDb.Instance[WowClass.None].RotationList.CollectionChanged -= OnRotationListChange;
                    ProfileDb.Instance[WowClass.None].RotationList.CollectionChanged += OnRotationListChange;

                    ProfileDb.Instance[this.Class].RotationList.CollectionChanged -= OnRotationListChange;
                    ProfileDb.Instance[this.Class].RotationList.CollectionChanged += OnRotationListChange;

                    this.OnPropertyChanged("Class");
                    this.OnPropertyChanged("Name");
                    this.OnPropertyChanged("Rotations");
                };
            }
        }

        private void OnRotationListChange(object sender, EventArgs e)
        {
            OnPropertyChanged("Rotations");
        }

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="Yanitta.WowMemory"/>.
        /// </summary>
        /// <param name="process">Процесс WoW.</param>
        public WowMemory(Process process)
        {
            if (process == null)
                throw new ArgumentNullException("process");

            var section = string.Format("{0}_{1}",
                process.ProcessName,
                process.MainModule.FileVersionInfo.FilePrivatePart);

            this.Offsets = new Offsets(section);
            if (this.Offsets == null)
                throw new NullReferenceException(string.Format("Current game version ({0}) not supported!", section));

            this.Memory = new ProcessMemory(process);

            this.mTimer = new DispatcherTimer();
            this.mTimer.Interval = TimeSpan.FromMilliseconds(500);
            this.mTimer.Tick += (o, e) => {
                if (CheckProcess())
                    this.IsInGame = this.Memory.Read<byte>(this.Memory.Rebase(this.Offsets.IsInGame)) != 0;
            };

            // Мы должны сохранить ссылку на делегат, чтобы его не трогал сборщик мусора
            this.KeyBoardProck = new KeyBoardProc(HookCallback);
            this.keyboardHook = SetWindowsHookEx(13, this.KeyBoardProck, Process.GetCurrentProcess().MainModule.BaseAddress, 0);

            this.mTimer.IsEnabled = true;
            this.mTimer.Start();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode == 0
                && Keyboard.Modifiers != ModifierKeys.None
                && this.Memory.IsFocusMainWindow
                && this.IsInGame
                && (wParam.ToInt32() == 0x104 || wParam.ToInt32() == 0x100))
            {
                var vkCode = Marshal.ReadInt32(lParam);
                var key    = KeyInterop.KeyFromVirtualKey(vkCode);

                // не будем обрабатывать, если просто зажат модификатор [116...121]
                if (!(key >= Key.LeftShift && key <= Key.RightAlt))
                {
                    //Debug.WriteLine("[{3}] Mod: {0}, Key: {1} ({2})", Keyboard.Modifiers, key, VkCode, nCode);
                    foreach (var rotation in this.Rotations)
                    {
                        if (rotation.HotKey.Modifier == Keyboard.Modifiers
                            && rotation.HotKey.Key == key)
                        {
                            Console.WriteLine("Процесс: [{0}] {1} <{2}> Запуск ротации \"{3}\" ({4})",
                                this.ProcessId, this.Class, this.Name, rotation.Name, rotation.HotKey);

                            this.ExecuteProfile(rotation);
                            return (IntPtr)1;
                        }
                    }
                }
            }
            return CallNextHookEx(keyboardHook, nCode, wParam, lParam);
        }

        /// <summary>
        /// Проверяет доступность текущего процесса.
        /// </summary>
        private bool CheckProcess()
        {
            if (this.Memory.Process.HasExited
                || Process.GetProcessById(this.ProcessId) == null)
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

        /// <summary>
        /// Запускает/останавливает ротацию.
        /// </summary>
        /// <param name="rotation">Текущая ротация.</param>
        private void ExecuteProfile(Rotation rotation)
        {
            if (rotation == null)
                throw new ArgumentNullException("rotation");

            var builder = new StringBuilder();
            builder.AppendLine(ProfileDb.Instance.Lua);
            builder.AppendLine();

            builder.AppendFormatLine("ABILITY_TABLE = {{\n{0}\n}};",
                string.Join(",\n", rotation.AbilityList));
            builder.AppendLine();

            builder.AppendLine(ProfileDb.Instance[this.Class].Lua);
            builder.AppendLine();
            // у профилей по умолчанию не должно быть кода профиля.
            //builder.AppendLine(ProfileDb.Instance[WowClass.None].Lua);
            builder.AppendLine(rotation.Lua);
            builder.AppendLine();

            builder.AppendFormatLine(@"DebugMode = {0};", Settings.Default.DebugMode.ToString().ToLower());
            // Запуск ротации
            builder.AppendFormatLine("assert(type(ChangeRotation) == \"function\", 'Не найдена функция \"ChangeRotation\"');");
            builder.AppendFormatLine(@"ChangeRotation(""{0}"");", rotation.Name);

            var code = builder.ToString();

            System.IO.File.WriteAllText("InjectedLuaCode.lua", code);

            this.LuaExecute(code);
        }

        /// <summary>
        /// Выполняет в текущем процессе указанный скрипт Lua.
        /// </summary>
        /// <param name="command">Скрипт Lua, который неоходимо выполнить.</param>
        public void LuaExecute(string command)
        {
            var bytes = Encoding.UTF8.GetBytes(command + '\0');
            var code  = this.Memory.Write(bytes);
            var path  = this.Memory.WriteCString("profile.lua");
            var len   = bytes.Length - 1;
            try
            {
                var injAddress  = this.Memory.Rebase(this.Offsets.InjectedAddress);
                var funcAddress = this.Memory.Rebase(this.Offsets.ExecuteBuffer);

                //if (Memory.IsX64)
                //    this.Memory.Call_x64(injAddress, funcAddress, code, path, IntPtr.Zero);
                //else
                    this.Memory.Call_x32(injAddress, funcAddress, code.ToInt32(), len, path.ToInt32(), 0, 0, 0);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                this.Memory.Free(code);
                this.Memory.Free(path);
            }
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
 ABILITY_TABLE = { };";

        private void Dispose(bool disposing)
        {
            if (this.IsDisposed)
                return;

            if (this.mTimer != null)
            {
                this.mTimer.Stop();
                this.mTimer.IsEnabled = false;
            }

            if (this.keyboardHook != IntPtr.Zero)
                UnhookWindowsHookEx(this.keyboardHook);
            this.KeyBoardProck = null;

            if (this.Memory != null && !this.Memory.Process.HasExited)
            {
                if (this.IsInGame && this.Memory != null)
                {
                    this.LuaExecute(StopCode);
                }
            }

            this.keyboardHook = IntPtr.Zero;
            this.Memory     = null;
            this.IsDisposed = true;
        }
    }
}