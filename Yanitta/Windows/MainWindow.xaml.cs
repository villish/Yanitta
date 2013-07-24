using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using Yanitta.Plugins;
using Yanitta.Windows;

namespace Yanitta
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private DispatcherTimer refreshTimer = null;
        private WinCodeExecute codeExecuteWindow = null;
        private WinProfileEditor profileWindows = null;
        private WindowSettings settingWindow = null;

        public static ObservableCollection<WowMemory> ProcessList { get; set; }

        public static ObservableCollection<IYanittaPlugin> PluginList { get; set; }

        static MainWindow()
        {
            ProcessList = new ObservableCollection<WowMemory>();
            PluginList = (App.Current as App).PluginList;
        }

        public MainWindow()
        {
            InitializeComponent();

            ProcessList.CollectionChanged += (o, e) =>
            {
                if (ProcessList.Count > 0 && lbProcessList.SelectedIndex == -1)
                    lbProcessList.SelectedIndex = 0;
            };

            refreshTimer = new DispatcherTimer();
            refreshTimer.Interval = TimeSpan.FromSeconds(1);
            refreshTimer.Tick += (o, e) => UpdateProcessList();
#if !TRACE
            refreshTimer.IsEnabled = true;
            refreshTimer.Start();
#else
            int i = 0;
            foreach (WowClass wclass in Enum.GetValues(typeof(WowClass)))
            {
                ProcessList.Add(new WowMemory(wclass, wclass.GetLocalizedName(), 0, (i++ == 0)));
            }
#endif
        }

        private void UpdateProcessList()
        {
            var wowProcessList = Process.GetProcessesByName("wow");

            #region Чистка "мертвых процессов"

            // Если запущенных процессов нет, тогда просто очищаем список
            if (!wowProcessList.Any())
            {
                foreach (var process in ProcessList)
                {
                    Console.WriteLine("Dispose dead process [" + process.ProcessId + "]");
                    process.Dispose();
                }
                ProcessList.Clear();
                return;
            }

            // далее, проверяем есть ли в списке "мертвые процессы"
            for (int i = ProcessList.Count - 1; i >= 0; --i)
            {
                if (!wowProcessList.Any(n => n.Id == ProcessList[i].ProcessId))
                {
                    Console.WriteLine("Dispose dead process [" + ProcessList[i].ProcessId + "]");
                    ProcessList[i].Dispose();
                    ProcessList.RemoveAt(i);
                }
            }

            #endregion Чистка "мертвых процессов"

            foreach (var wowProcess in wowProcessList)
            {
                if (ProcessList.Any(n => n.ProcessId == wowProcess.Id))
                    continue;

                try
                {
                    var wowMemory = new WowMemory(wowProcess);

                    if (!wowMemory.IsInGame)
                    {
                        wowMemory.Dispose();
                        continue;
                    }

                    wowMemory.GameStateChanged += (memory) =>
                    {
                        if (!memory.IsInGame)
                        {
                            if (ProcessList.Contains(memory))
                                ProcessList.Remove(memory);
                            memory.Dispose();
                        }
                    };
                    ProcessList.Add(wowMemory);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error WowMemory: {0}", ex.Message);
                }
            }
        }

        private void Image_MouseDown(object o, RoutedEventArgs e)
        {
            this.DragMove();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (refreshTimer != null)
            {
                refreshTimer.IsEnabled = false;
                refreshTimer.Stop();
                refreshTimer = null;
            }

            foreach (var plugin in PluginList)
                plugin.Dispose();
            PluginList.Clear();

            if (ProcessList != null)
            {
                foreach (var wow in ProcessList)
                    wow.Dispose();
                ProcessList.Clear();
            }

            if (codeExecuteWindow != null)
            {
                codeExecuteWindow.Close();
                codeExecuteWindow = null;
            }
            if (profileWindows != null)
            {
                profileWindows.Close();
                profileWindows = null;
            }
            if (settingWindow != null)
            {
                settingWindow.Close();
                settingWindow = null;
            }
        }

        private void CommandBinding_Executed_Close(object sender, ExecutedRoutedEventArgs e)
        {
            Application.Current.Shutdown(0);
        }

        private void CommandBinding_Executed_ShowExecuteWindow(object sender, ExecutedRoutedEventArgs e)
        {
            if (codeExecuteWindow == null || !codeExecuteWindow.IsLoaded)
                codeExecuteWindow = new WinCodeExecute();

            codeExecuteWindow.Show();

            if (!codeExecuteWindow.IsActive)
                codeExecuteWindow.Activate();
        }

        private void CommandBinding_Executed_ShowProfileWindow(object sender, ExecutedRoutedEventArgs e)
        {
            if (profileWindows == null || !profileWindows.IsLoaded)
                profileWindows = new WinProfileEditor();

            profileWindows.Show();

            if (!profileWindows.IsActive)
                profileWindows.Activate();
        }

        private void CommandBinding_Executed_ShowSettingWindow(object sender, ExecutedRoutedEventArgs e)
        {
            if (settingWindow == null || !settingWindow.IsLoaded)
                settingWindow = new WindowSettings();

            settingWindow.Show();

            if (!settingWindow.IsActive)
                settingWindow.Activate();
        }

        private void CommandBinding_Executed_Minimize(object sender, ExecutedRoutedEventArgs e)
        {
            this.WindowState = System.Windows.WindowState.Minimized;
        }

        private void CommandBinding_Executed_ShowPluginSettingWindow(object sender, ExecutedRoutedEventArgs e)
        {
            var button = e.OriginalSource as Button;
            var plugin = button.Tag as IYanittaPlugin;
            if (plugin != null)
            {
                var style = App.Current.Resources["KamillaStyle"] as Style;
                plugin.ShowSettingWindow(style);
            }
        }
    }
}