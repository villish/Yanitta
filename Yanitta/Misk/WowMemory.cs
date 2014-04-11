using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
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
    public class WowMemory : DependencyObject, IDisposable
    {
        public readonly static DependencyProperty CurrentProfileProperty = DependencyProperty.Register("CurrentProfile",    typeof(Profile),    typeof(WowMemory));
        public readonly static DependencyProperty ClassProperty          = DependencyProperty.Register("Class",             typeof(WowClass),   typeof(WowMemory));
        public readonly static DependencyProperty NameProperty           = DependencyProperty.Register("Name",              typeof(string),     typeof(WowMemory));
        public readonly static DependencyProperty IsInGameProperty       = DependencyProperty.Register("IsInGame",          typeof(bool),       typeof(WowMemory));

        /// <summary>
        /// Событие для обработки закрытия процесса.
        /// </summary>
        public event WowMemoryHandler GameExited;

        /// <summary>
        /// Id процесса.
        /// </summary>
        public int ProcessId
        {
            get
            {
                if (this.Memory == null)
                    return 0;
                return this.Memory.Process.Id;
            }
        }

        /// <summary>
        /// Профиль персонажа.
        /// </summary>
        public Profile CurrentProfile
        {
            get { return ProfileDb.Instance[this.Class]; }
            private set { SetValue(CurrentProfileProperty, value);  }
        }

        /// <summary>
        /// Класс персонажа.
        /// </summary>
        public WowClass Class
        {
            get { return (WowClass)GetValue(ClassProperty); }
            private set { SetValue(ClassProperty, value);   }
        }

        /// <summary>
        /// Имя персонажа.
        /// </summary>
        public string Name
        {
            get { return (string)GetValue(NameProperty); }
            private set { SetValue(NameProperty, value); }
        }

        /// <summary>
        /// Состояние, находится ли персонаж в игровом мире.
        /// </summary>
        public bool IsInGame
        {
            get { return (bool)GetValue(IsInGameProperty);   }
            private set { SetValue(IsInGameProperty, value); }
        }

        /// <summary>
        /// Текущий процесс <see cref="Yanitta.ProcessMemory"/>
        /// </summary>
        public ProcessMemory Memory { get; private set; }

        private bool IsDisposed;
        private DispatcherTimer mTimer;
        private DateTime LastAction = DateTime.Now;

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="Yanitta.WowMemory"/>.
        /// </summary>
        /// <param name="process">Процесс вов.</param>
        public WowMemory(Process process)
        {
            this.Memory = new ProcessMemory(process);

            int build = this.Memory.Process.MainModule.FileVersionInfo.FilePrivatePart;
            if (build != Offsets.Default.Build)
                throw new Exception(string.Format("Current build [{0}] WoW is not supported [{1}]", build, Offsets.Default.Build));

            this.mTimer = new DispatcherTimer();
            this.mTimer.Interval = TimeSpan.FromMilliseconds(500);
            this.mTimer.Tick += (o, e) => this.ReadAllValues();

            this.ReadAllValues();

            this.mTimer.IsEnabled = true;
            this.mTimer.Start();
        }

        /// <summary>
        /// Проверяет доступность текущего процесса.
        /// </summary>
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

        /// <summary>
        /// Считывает из текущего процесса значения имени, класса и нахождение в мире.
        /// В зависимости от считанных данных регистрирует или удаляет гарячие клавиши для управления ротациями.
        /// </summary>
        private void ReadAllValues()
        {
            if (!CheckProcess())
                return;

            var isInGame = this.Memory.Read<byte>(Memory.Rebase((IntPtr)Offsets.Default.IsInGame)) != 0;
            if (isInGame != this.IsInGame)
            {
                this.IsInGame = isInGame;

                Debug.WriteLine("IsInGame: " + this.IsInGame);

                if (this.IsInGame)
                {
                    this.Class = (WowClass)this.Memory.Read<byte>(Memory.Rebase((IntPtr)Offsets.Default.PlayerClass));
                    this.Name  = this.Memory.ReadString(Memory.Rebase((IntPtr)Offsets.Default.PlayerName));
                    this.CurrentProfile = ProfileDb.Instance[this.Class];
                }
                else
                {
                    this.UnregisterAllHotKeys();
                    this.CurrentProfile = null;
                    this.Class = WowClass.None;
                    this.Name  = string.Empty;
                }
            }

            if (Memory.IsFocusWindow)
            {
                MainWindow.ProcessList.Where(process => process != this)
                    .ForEach(process => process.UnregisterAllHotKeys());

                if (CurrentProfile != null)
                {
                    CurrentProfile.RotationList.ForEach((rotation) =>
                    {
                        if (!rotation.HotKey.IsRegistered)
                        {
                            rotation.HotKey.SetHandler(rotation, HotKeyPressed);
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

                if (ProfileDb.Instance.DefaultProfile != null)
                {
                    ProfileDb.Instance.DefaultProfile.RotationList.ForEach((rotation) =>
                    {
                        if (!rotation.HotKey.IsRegistered)
                        {
                            rotation.HotKey.SetHandler(rotation, HotKeyPressed);
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
            }
            else if (CurrentProfile != null)
            {
                this.UnregisterAllHotKeys();
            }
        }

        /// <summary>
        /// Обработчик нажатия гарячих клавиш.
        /// </summary>
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

        /// <summary>
        /// Запускает/останавливает ротацию.
        /// </summary>
        /// <param name="rotation">Текущая ротация.</param>
        private void ExecuteProfile(Rotation rotation)
        {
            if (CurrentProfile == null)
                throw new NullReferenceException("CurrentProfile is null");

            if (rotation == null)
                throw new ArgumentNullException("rotation is null");

            var builder = new StringBuilder();
            builder.AppendLine(ProfileDb.Instance.Lua);

            foreach (var ability in rotation.AbilityList)
            {
                var ability_code = ability.ToString();
                builder.AppendLine(ability_code);
            }

            builder.AppendLine(CurrentProfile.Lua);
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

        /// <summary>
        /// Удаляет все зарегистрированные гарячие клавиши в системе.
        /// </summary>
        private void UnregisterAllHotKeys()
        {
            if (this.CurrentProfile != null && this.CurrentProfile.RotationList != null)
            {
                this.CurrentProfile.RotationList.ForEach((rotation) => {
                    if (rotation.HotKey != null && rotation.HotKey.IsRegistered)
                        rotation.HotKey.Unregister();
                });
            }

            if (ProfileDb.Instance.DefaultProfile != null && ProfileDb.Instance.DefaultProfile.RotationList != null)
            {
                ProfileDb.Instance.DefaultProfile.RotationList.ForEach((rotation) =>
                {
                    if (rotation.HotKey != null && rotation.HotKey.IsRegistered)
                        rotation.HotKey.Unregister();
                });
            }
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
            if (this.IsDisposed || !disposing)
                return;

            if (this.mTimer != null)
            {
                this.mTimer.Stop();
                this.mTimer.IsEnabled = false;
            }

            this.UnregisterAllHotKeys();

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