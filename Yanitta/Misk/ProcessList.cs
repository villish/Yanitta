using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows.Threading;

namespace Yanitta
{
    public class ProcessList : ObservableCollection<WowMemory>, IDisposable
    {
        private DispatcherTimer refreshTimer;

        public ProcessList()
            : base()
        {
            this.refreshTimer = new DispatcherTimer();
            this.refreshTimer.Interval = TimeSpan.FromSeconds(1);
            this.refreshTimer.Tick += (o, e) => CheckProcess();
#if !TRACE
            this.refreshTimer.IsEnabled = true;
            this.refreshTimer.Start();
#else
            new Thread(new ThreadStart(() => {
                Thread.Sleep(5000);
                foreach (WowClass wclass in Enum.GetValues(typeof(WowClass)))
                {
                    App.Current.Dispatcher.BeginInvoke(new Action(() => {
                        this.Add(new WowMemory(wclass, wclass.ToString(), 0, ((byte)wclass == 1)));
                    }));                    
                }
            })).Start();
#endif
        }

        private void CheckProcess()
        {
            var wowProcessList = Process.GetProcessesByName("wow");

            if (!wowProcessList.Any())
            {
                foreach (var process in this)
                {
                    Console.WriteLine("Dispose dead process [" + process.ProcessId + "]");
                    process.Dispose();
                }
                this.Clear();
                return;
            }

            for (int i = this.Count - 1; i >= 0; --i)
            {
                if (!wowProcessList.Any(n => n.Id == this[i].ProcessId))
                {
                    Console.WriteLine("Dispose dead process [" + this[i].ProcessId + "]");
                    this[i].Dispose();
                    this.RemoveAt(i);
                }
            }

            foreach (var wowProcess in wowProcessList)
            {
                if (this.Any(n => n.ProcessId == wowProcess.Id))
                    continue;

                try
                {
                    var wowMemory = new WowMemory(wowProcess);

                    if (!wowMemory.IsInGame)
                    {
                        wowMemory.Dispose();
                        continue;
                    }

                    wowMemory.GameStateChanged += (memory) => {
                        if (!memory.IsInGame)
                        {
                            if (this.Contains(memory))
                                this.Remove(memory);
                            memory.Dispose();
                        }
                    };
                    this.Add(wowMemory);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error WowMemory: {0}", ex.Message);
                }
            }
        }

        public void Dispose()
        {
            if (this.refreshTimer != null)
            {
                this.refreshTimer.IsEnabled = false;
                this.refreshTimer.Stop();
                this.refreshTimer = null;
            }

            foreach (var process in this)
                process.Dispose();
            this.Clear();
        }
    }
}
