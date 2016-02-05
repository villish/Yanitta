﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Linq;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Yanitta.Properties;

namespace Yanitta
{
    /// <summary>
    /// Определяет обработчик для <see cref="WowMemory"/>.
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
        KeyBoardProc KeyBoardProck;
        bool isInGame;
        IntPtr keyboardHook;
        bool IsDisposed;
        DispatcherTimer mTimer = new DispatcherTimer {
            Interval = TimeSpan.FromMilliseconds(500)
        };

        /// <summary>
        /// Текущий процесс <see cref="ProcessMemory"/>
        /// </summary>
        public ProcessMemory Memory { get; private set; }

        public Offsets Offsets { get; private set; }

        /// <summary>
        /// Id процесса.
        /// </summary>
        public int ProcessId => Memory?.Process?.Id ?? 0;

        public string ProcessName => $"{Memory.Process.ProcessName}_{Memory.Process.MainModule.FileVersionInfo.FilePrivatePart}";

        public IEnumerable<Rotation> Rotations
        {
            get
            {
                if (IsInGame)
                {
                    foreach (var rotation in ProfileDb.Instance[Class]?.RotationList)
                        yield return rotation;

                    foreach (var rotation in ProfileDb.Instance[WowClass.None]?.RotationList)
                        yield return rotation;
                }
            }
        }

        /// <summary>
        /// Класс персонажа.
        /// </summary>
        public WowClass Class { get; private set; }

        public BitmapImage ImageSource => Extensions.GetIconFromEnum(Class);

        /// <summary>
        /// Имя персонажа.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Состояние, находится ли персонаж в игровом мире.
        /// </summary>
        public bool IsInGame
        {
            get { return isInGame; }
            private set
            {
                if (isInGame != value)
                {
                    isInGame = value;
                    OnPropertyChanged("IsInGame");

                    Class = value ? Memory.Read<WowClass>(Memory.Rebase(Offsets.PlayerClass)) : WowClass.None;
                    Name  = value ? Memory.ReadString(Memory.Rebase(Offsets.PlayerName)) : string.Empty;

                    if (!Enum.IsDefined(typeof(WowClass), Class))
                        throw new Exception("Unsuported wow class: " + Class);

                    foreach (var item in ProfileDb.Instance?.ProfileList)
                    {
                        item.RotationList.CollectionChanged -= OnRotationListChanged;
                        item.RotationList.CollectionChanged += OnRotationListChanged;
                    }

                    OnPropertyChanged("Class");
                    OnPropertyChanged("ImageSource");
                    OnPropertyChanged("Name");
                    OnPropertyChanged("Rotations");
                }
            }
        }

        void OnRotationListChanged(object sender, NotifyCollectionChangedEventArgs e) => OnPropertyChanged("Rotations");

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="Yanitta.WowMemory"/>.
        /// </summary>
        /// <param name="process">Процесс WoW.</param>
        public WowMemory(Process process)
        {
            if (process == null)
                throw new ArgumentNullException(nameof(process));

            var section = $"{process.ProcessName}_{process.MainModule.FileVersionInfo.FilePrivatePart}";

            Offsets = new Offsets(section);
            if (Offsets == null)
                throw new NullReferenceException($"Current game version ({section}) not supported!");

            Memory = new ProcessMemory(process);

            // Мы должны сохранить ссылку на делегат, чтобы его не трогал сборщик мусора
            KeyBoardProck = new KeyBoardProc(HookCallback);
            keyboardHook = SetWindowsHookEx(13, KeyBoardProck, Process.GetCurrentProcess().MainModule.BaseAddress, 0);

            mTimer.Tick += (o, e) => {
                if (CheckProcess())
                {
                    var ingame = Memory.Read<byte>(Memory.Rebase(Offsets.IsInGame));
                    IsInGame = ingame != 0;
                }
            };
            mTimer.Start();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode == 0
                && Keyboard.Modifiers != ModifierKeys.None
                && Memory.IsFocusMainWindow
                && IsInGame
                && (wParam.ToInt32() == 0x104 || wParam.ToInt32() == 0x100))
            {
                var vkCode = Marshal.ReadInt32(lParam);
                var key = KeyInterop.KeyFromVirtualKey(vkCode);

                // не будем обрабатывать, если просто зажат модификатор [116...121]
                if (!(key >= Key.LeftShift && key <= Key.RightAlt))
                {
                    //Debug.WriteLine("[{3}] Mod: {0}, Key: {1} ({2})", Keyboard.Modifiers, key, VkCode, nCode);
                    foreach (var rotation in Rotations)
                    {
                        if (rotation.HotKey.Modifier == Keyboard.Modifiers && rotation.HotKey.Key == key)
                        {
                            Console.WriteLine($"Process: [{ProcessId}] {Class} <{Name}> Start rotation: \"{rotation.Name}\" ({rotation.HotKey})");
                            ExecuteProfile(rotation);
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
        bool CheckProcess()
        {
            if (Memory.Process.HasExited
                || Process.GetProcessById(ProcessId) == null)
            {
                IsInGame = false;
                GameExited?.Invoke(this);
                Console.WriteLine("Wow process exited!");
                Dispose();
                return false;
            }
            return true;
        }

        /// <summary>
        /// Запускает/останавливает ротацию.
        /// </summary>
        /// <param name="rotation">Текущая ротация.</param>
        void ExecuteProfile(Rotation rotation)
        {
            if (rotation == null)
                throw new ArgumentNullException(nameof(rotation));

            var builder = new StringBuilder();
            builder.AppendLine(ProfileDb.Instance.Lua);
            builder.AppendLine();

            builder.AppendLine($"ABILITY_TABLE = {{\n{string.Join(",\n", rotation.AbilityList)}\n}};");
            builder.AppendLine();

            builder.AppendLine(ProfileDb.Instance[Class].Lua);
            builder.AppendLine();

            builder.AppendLine(ProfileDb.Instance[WowClass.None].Lua);
            builder.AppendLine(rotation.Lua);
            builder.AppendLine();

            builder.AppendLine($"DebugMode = {Settings.Default.DebugMode.ToString().ToLower()};");
            // Запуск ротации
            builder.AppendLine("assert(type(ChangeRotation) == \"function\", 'Не найдена функция \"ChangeRotation\"');");
            builder.AppendLine($"ChangeRotation(\"{rotation.Name}\");");

            var code = builder.ToString();

            System.IO.File.WriteAllText("InjectedLuaCode.lua", code);

            LuaExecute(code);
        }

        /// <summary>
        /// Выполняет в текущем процессе указанный скрипт Lua.
        /// </summary>
        /// <param name="command">Скрипт Lua, который неоходимо выполнить.</param>
        public void LuaExecute(string command)
        {
            var bytes = Encoding.UTF8.GetBytes(command + '\0');
            var code  = Memory.Write(bytes);
            var path  = Memory.WriteCString("profile.lua");
            var len   = bytes.Length - 1;
            try
            {
                var injAddress  = Memory.Rebase(Offsets.InjectedAddress);
                var funcAddress = Memory.Rebase(Offsets.ExecuteBuffer);

                //if (Memory.IsX64)
                //    this.Memory.Call_x64(injAddress, funcAddress, code, path, IntPtr.Zero);
                //else
                Memory.Call_x32(injAddress, funcAddress, code.ToInt32(), len, path.ToInt32(), 0, 0, 0);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                Memory.Free(code);
                Memory.Free(path);
            }
        }

        public RelayCommand<object> Test => new RelayCommand<object>((x) => TestBoober());
        private void TestBoober()
        {
            var objManager = Memory.Read<IntPtr>(Memory.Rebase(Offsets.ObjectMr));
            var playerGuid = Memory.Read<WowGuid>(objManager + FieldOffsets.Player);
            Console.WriteLine(playerGuid);
            byte found = 0;
            Console.WriteLine("start >>>");
            var baseAddr = Memory.Read<IntPtr>(objManager + FieldOffsets.FirstObject);
            var cur = new WowObject(Memory, baseAddr);
            while (cur.BaseAddr != IntPtr.Zero && (cur.BaseAddr.ToInt64() & 1) == 0)
            {
                //Console.WriteLine(cur.Guid);
                if (cur.Type == 5 && cur.IsBoobing && cur.CreatedBy == playerGuid)
                {
                    Console.WriteLine("Found boober: " + cur.Guid);
                    found = 1;
                    // write boobers guid to "mouseover"
                    Memory.Write(Memory.Rebase(Offsets.ObjTrack), cur.Guid);
                    break;
                }

                cur.BaseAddr = Memory.Read<IntPtr>(cur.BaseAddr + FieldOffsets.NextObject);
            }

            Console.WriteLine("stop >>>");

            // lua_pushboolean(state, found)
            // 6A 00          push    found ; change
            // FF 75 08       push    [ebp+arg_0]
            // E8 9F B2 CD FF call    lua_pushboolean
            Memory.Write(Memory.Rebase(Offsets.TestClnt), found);
        }

        public override string ToString() => $"[{ProcessId}] {Name} ({Class})";

        ~WowMemory()
        {
            Dispose(false);
        }

        /// <summary>
        /// Closes an open wow memory.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        const string StopCode =
@"if type(ChangeRotation) == ""function"" then
     ChangeRotation();
 end
 ABILITY_TABLE = { };";

        void Dispose(bool disposing)
        {
            if (IsDisposed)
                return;

            mTimer?.Stop();
            if (keyboardHook != IntPtr.Zero)
                UnhookWindowsHookEx(keyboardHook);
            KeyBoardProck = null;

            if (IsInGame && Memory?.Process?.HasExited != true)
                LuaExecute(StopCode);

            keyboardHook = IntPtr.Zero;
            Memory = null;
            IsDisposed = true;
        }
    }
}