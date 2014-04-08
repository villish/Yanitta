using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows.Threading;
using Yanitta.Properties;

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
            this.refreshTimer.IsEnabled = true;
            this.refreshTimer.Start();
        }

        private void CheckProcess()
        {
            var wowProcessList = Process.GetProcessesByName(Settings.Default.ProcessName);

            if (!wowProcessList.Any())
            {
                foreach (var process in this)
                {
                    Debug.WriteLine("Dispose dead process [" + process.ProcessId + "]");
                    process.Dispose();
                }
                this.Clear();
                return;
            }

            for (int i = this.Count - 1; i >= 0; --i)
            {
                if (!wowProcessList.Any(n => n.Id == this[i].ProcessId))
                {
                    Debug.WriteLine("Dispose dead process [" + this[i].ProcessId + "]");
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

                    wowMemory.GameExited += (memory) => {
                        if (this.Contains(memory))
                            this.Remove(memory);
                        memory.Dispose();
                    };
                    this.Add(wowMemory);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Error WowMemory: " + ex.Message);
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
