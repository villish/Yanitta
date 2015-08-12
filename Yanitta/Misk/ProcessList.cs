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
        private bool disposed = false;

        private DispatcherTimer refreshTimer;

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="Yanitta.ProcessList"/>
        /// </summary>
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
            if (string.IsNullOrWhiteSpace(Settings.Default.ProcessName))
                throw new Exception("ProcessName is empty");

            var nameList = Settings.Default.ProcessName.Split(new[] { ';', ' ' }, StringSplitOptions.RemoveEmptyEntries);
            var wowProcessList = Process.GetProcesses().Where(n => nameList.Contains(n.ProcessName));

            if (!wowProcessList.Any())
            {
                foreach (var process in this)
                {
                    Debug.WriteLine("Dispose dead process [" + process.ProcessId + "]");
                    process.Dispose();
                }
                this.Clear();
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

                    wowMemory.GameExited += (memory) =>
                    {
                        if (this.Contains(memory))
                            this.Remove(memory);
                        memory.Dispose();
                    };
                    this.Add(wowMemory);
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
            if (!this.disposed)
            {
                // If disposing equals true, dispose all managed
                // and unmanaged resources.
                if (disposing)
                {
                    if (this.refreshTimer != null)
                    {
                        this.refreshTimer.IsEnabled = false;
                        this.refreshTimer.Stop();
                    }

                    foreach (var process in this)
                        process.Dispose();
                    this.Clear();
                }

                this.refreshTimer = null;

                // Note disposing has been done.
                disposed = true;
            }
        }
    }
}
