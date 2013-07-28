using System;
using System.ComponentModel;
using System.Diagnostics;

namespace MemoryModule
{
    /// <summary>
    ///
    /// </summary>
    public partial class ProcessMemory : IDisposable
    {
        public static string FasmVersion { get; private set; }

        static ProcessMemory()
        {
            var raw_ver = MemoryModule.FASM.Internals.fasm_GetVersion();
            FasmVersion = string.Format("{0}.{1}", raw_ver & 0xFFFF, raw_ver >> 16 & 0xFFFF);
        }

        /// <summary>
        ///
        /// </summary>
        public bool IsOpened { get; private set; }

        /// <summary>
        /// Get the cutrrent process.
        /// </summary>
        public Process Process { get; private set; }

        /// <summary>
        /// Gets the native handle of the associated process.
        /// </summary>
        public SafeProcessHandle Handle { get; private set; }

        /// <summary>
        /// Gets the native handle of the associated process thread.
        /// </summary>
        public SafeProcessHandle ThreadHandle { get; private set; }

        /// <summary>
        /// Represents an operating system process main thread.
        /// </summary>
        public ProcessThread MainThread
        {
            get { return Process.Threads[0]; }
        }

        /// <summary>
        /// Gets a value indicating whether the associated process has been terminated.
        /// </summary>
        public bool HasExited
        {
            get { return this.Process.HasExited; }
        }

        /// <summary>
        /// Gets the memory address where the module was loaded.
        /// </summary>
        public uint BaseAddress
        {
            get { return (uint)(int)Process.MainModule.BaseAddress; }
        }

        /// <summary>
        ///
        /// </summary>
        public bool IsFocusWindow
        {
            get
            {
                int lProcessId;
                var foregroundWindow = Internals.GetForegroundWindow();
                Internals.GetWindowThreadProcessId(foregroundWindow, out lProcessId);
                return this.Process.Id == lProcessId;
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="process"></param>
        /// <param name="DebugPrivileges"></param>
        /// <param name="UseBaseAddress"></param>
        public ProcessMemory(Process process)
        {
            if (process == null)
                throw new ArgumentNullException("process", "Process exists");

            this.Process = process;

            Process.EnterDebugMode();

            this.Process.EnableRaisingEvents = true;

            this.Handle = Internals.OpenProcess(ProcessAccess.All, false, Process.Id);

            if (this.Handle.IsInvalid)
                throw new Win32Exception();

            this.ThreadHandle = Internals.OpenThread(ThreadAccess.All, false, this.MainThread.Id);

            if (this.ThreadHandle.IsInvalid)
                throw new Win32Exception();

            this.IsOpened = true;
        }

        /// <summary>
        /// Suspends the specified thread.
        /// </summary>
        public void Suspend()
        {
            if (!this.ThreadHandle.IsInvalid)
                Internals.SuspendThread(this.ThreadHandle);
        }

        /// <summary>
        /// Decrements a thread's suspend count.
        /// When the suspend count is decremented to zero, the execution of the thread is resumed.
        /// </summary>
        public void Resume()
        {
            if (!this.ThreadHandle.IsInvalid)
                Internals.ResumeThread(this.ThreadHandle);
        }

        public uint Rebase(long address)
        {
            return (uint)(this.BaseAddress + address);
        }

        ~ProcessMemory()
        {
            Dispose(false);
        }

        /// <summary>
        /// Closes an open process handle and thread.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (!disposing)
                return;

            this.Handle.Close();
            this.ThreadHandle.Close();
            this.IsOpened = false;
        }
    }
}