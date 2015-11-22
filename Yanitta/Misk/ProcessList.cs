using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows.Threading;
using Yanitta.Properties;

namespace Yanitta
{
    /// <summary>
    /// Самообновляемый список процессов.
    /// </summary>
    public class ProcessList : ObservableCollection<WowMemory>, IDisposable
    {
        // Track whether Dispose has been called.
        bool disposed = false;

        DispatcherTimer refreshTimer = new DispatcherTimer {
            Interval = TimeSpan.FromSeconds(1)
        };

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="ProcessList"/>
        /// </summary>
        public ProcessList()
        {
            refreshTimer.Tick += (o, e) => CheckProcess();
            refreshTimer.Start();
        }

        void CheckProcess()
        {
            if (string.IsNullOrWhiteSpace(Settings.Default.ProcessName))
                throw new Exception("ProcessName is empty");

            var nameList = Settings.Default.ProcessName.ToLower().Split(new[] { ';', ' ' }, StringSplitOptions.RemoveEmptyEntries);
            var wowProcessList = Process.GetProcesses().Where(n => nameList.Contains(n.ProcessName.ToLower()));

            if (!wowProcessList.Any())
            {
                foreach (var process in this)
                {
                    Debug.WriteLine($"Dispose dead process [{ process.ProcessId}]");
                    process.Dispose();
                }
                Clear();
            }

            for (int i = Count - 1; i >= 0; --i)
            {
                if (!wowProcessList.Any(n => n.Id == this[i].ProcessId))
                {
                    Debug.WriteLine($"Dispose dead process [{this[i].ProcessId}]");
                    this[i].Dispose();
                    RemoveAt(i);
                }
            }

            foreach (var wowProcess in wowProcessList)
            {
                if (this.Any(n => n.ProcessId == wowProcess.Id))
                    continue;

                try
                {
                    var wowMemory = new WowMemory(wowProcess);

                    wowMemory.GameExited += (memory) =>
                    {
                        if (Contains(memory))
                            Remove(memory);
                        memory.Dispose();
                    };
                    Add(wowMemory);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error WowMemory: " + ex.Message);
                }
            }
        }

        ~ProcessList()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if (!disposed)
            {
                // If disposing equals true, dispose all managed
                // and unmanaged resources.
                if (disposing)
                {
                    refreshTimer?.Stop();
                    foreach (var process in this)
                        process?.Dispose();
                    Clear();
                }

                refreshTimer = null;

                // Note disposing has been done.
                disposed = true;
            }
        }
    }
}
