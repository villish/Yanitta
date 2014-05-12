using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
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
        /// <summary>
        /// Событие для обработки закрытия процесса.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1009:DeclareEventHandlersCorrectly")]
        public event WowMemoryHandler GameExited;

        private bool isInGame;
        private bool isFocus;

        /// <summary>
        /// Текущий процесс <see cref="Yanitta.ProcessMemory"/>
        /// </summary>
        public ProcessMemory Memory { get; private set; }

        private bool IsDisposed;
        private DispatcherTimer mTimer;

        /// <summary>
        /// Id процесса.
        /// </summary>
        public int ProcessId
        {
            get { return this.Memory.Process.Id; }
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

                    if (value)
                    {
                        this.Class = (WowClass)this.Memory.Read<byte>(Memory.Rebase((IntPtr)Offsets.Default.PlayerClass));
                        this.Name  = this.Memory.ReadString(Memory.Rebase((IntPtr)Offsets.Default.PlayerName));
                    }

                    ProfileDb.Instance[WowClass.None].RotationList.CollectionChanged -= OnRotationListChange;
                    ProfileDb.Instance[WowClass.None].RotationList.CollectionChanged += OnRotationListChange;

                    ProfileDb.Instance[this.Class].RotationList.CollectionChanged -= OnRotationListChange;
                    ProfileDb.Instance[this.Class].RotationList.CollectionChanged += OnRotationListChange;

                    ChangeHotKeys();

                    this.OnPropertyChanged("Class");
                    this.OnPropertyChanged("Name");
                    this.OnPropertyChanged("Rotations");
                };
            }
        }

        /// <summary>
        /// Указывает, активно ли основное окно процесса.
        /// </summary>
        public bool IsFocus
        {
            get { return this.isFocus; }
            private set
            {
                if (this.isFocus != value)
                {
                    this.isFocus = value;
                    ChangeHotKeys();
                    this.OnPropertyChanged();
                }
            }
        }

        private void OnRotationListChange(object sender, EventArgs e)
        {
            OnPropertyChanged("Rotations");
        }

        private void ChangeHotKeys()
        {
            if (this.isFocus && this.isInGame)
            {
                // Немного разъяснений, чтобы самому в следующий раз не запутатся:
                // Когда окно становится в фокусе и персонаж находится в игровоом мире
                // сначала надо снять регистрацию всех гарячих клавиш со всех доступных профилей.
                // Возможно это будут профили других процессов.
                ProfileDb.Instance.ProfileList.ForEach(
                    profile => profile.RotationList.ForEach(
                        rotation => rotation.Unregister()));

                // И только тогда делаем регистрацию новых гарячих клавиш на текущие ротации
                this.Rotations.ForEach(rotation => {
                    if (!rotation.HotKey.IsRegistered)
                    {
                        rotation.HotKey.Tag = rotation;
                        rotation.HotKey.Pressed -= HotKeyPressed;
                        rotation.HotKey.Pressed += HotKeyPressed;
                        try
                        {
                            rotation.HotKey.Register();
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("HotKey Error: " + ex.Message);
                        }
                    }
                });
            }
            else
            {
                this.Rotations.ForEach(rotation => rotation.Unregister());
            }
        }

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="Yanitta.WowMemory"/>.
        /// </summary>
        /// <param name="process">Процесс вов.</param>
        public WowMemory(Process process)
        {
            if (process == null)
                throw new ArgumentNullException("process");

            int build = process.MainModule.FileVersionInfo.FilePrivatePart;
            if (build != Offsets.Default.Build)
                throw new Exception(string.Format("Current build [{0}] WoW is not supported [{1}]", build, Offsets.Default.Build));

            this.Memory = new ProcessMemory(process);

            this.mTimer = new DispatcherTimer();
            this.mTimer.Interval = TimeSpan.FromMilliseconds(500);
            this.mTimer.Tick += (o, e) => {
                if (CheckProcess())
                {
                    this.IsFocus  = this.Memory.IsFocusWindow;
                    this.IsInGame = this.Memory.Read<byte>(Memory.Rebase((IntPtr)Offsets.Default.IsInGame)) != 0;
                }
            };

            this.mTimer.IsEnabled = true;
            this.mTimer.Start();
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
        /// Обработчик нажатия гарячих клавиш.
        /// </summary>
        private void HotKeyPressed(object sender, HandledEventArgs e)
        {
            Debug.Assert(sender != null);
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

            foreach (var ability in rotation.AbilityList)
            {
                var ability_code = ability.ToString();
                builder.AppendLine(ability_code);
            }

            builder.AppendLine(ProfileDb.Instance[this.Class].Lua);
            // у профилей по умолчанию не должно быть кода профиля.
            //builder.AppendLine(ProfileDb.Instance[WowClass.None].Lua);
            builder.AppendLine(rotation.Lua);

            builder.AppendFormatLine(@"DebugMode = {0};", Settings.Default.DebugMode.ToString().ToLower());
            // Запуск ротации
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
                this.Memory.Call(new IntPtr(Offsets.Default.ExecuteBuffer),
                    code.ToInt32(), len, path.ToInt32(), 0, 0, 0);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            this.Memory.Free(code);
            this.Memory.Free(path);
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

            this.Rotations.ForEach(rotation => rotation.Unregister());

            if (this.Memory != null && !this.Memory.Process.HasExited)
            {
                if (this.IsInGame && this.Memory != null)
                {
                    this.LuaExecute(StopCode);
                }
            }
            this.Memory     = null;
            this.IsDisposed = true;
        }
    }
}