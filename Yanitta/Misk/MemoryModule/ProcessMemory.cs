using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;

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
            FasmVersion = string.Format("Fasm v{0}.{1}", raw_ver & 0xFFFF, raw_ver >> 16 & 0xFFFF);
        }

        /// <summary>
        ///
        /// </summary>
        public bool IsOpened { get; private set; }

        /// <summary>
        ///
        /// </summary>
        public bool IsDisposed { get; private set; }

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
        public IntPtr BaseAddress
        {
            get { return Process.MainModule.BaseAddress; }
        }

        /// <summary>
        ///
        /// </summary>
        public bool IsFocusWindow
        {
            get { return this.Process.MainWindowHandle == Internals.GetForegroundWindow(); }
        }

        /// <summary>
        /// Sends the specified message to a main window.
        /// </summary>
        /// <param name="msg">The message to be sent.</param>
        /// <param name="wParam">Additional message-specific information.</param>
        /// <param name="lParam">Additional message-specific information.</param>
        public void SendMessage(uint msg, int wParam, int lParam)
        {
            Internals.SendMessage(this.Process.MainWindowHandle, msg, wParam, lParam);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="process"></param>
        /// <param name="DebugPrivileges"></param>
        /// <param name="UseBaseAddress"></param>
        public ProcessMemory(Process process)
            : this(process, false)
        {
        }

        public ProcessMemory(Process process, bool noFullAccess)
        {
            if (process == null)
                throw new ArgumentNullException("process", "Process exists");

            this.Process = process;

            this.Process.EnableRaisingEvents = true;

            Process.EnterDebugMode();
            this.Handle = Internals.OpenProcess(
                ProcessAccess.QueryInformation |
                ProcessAccess.VMOperation |
                ProcessAccess.VMRead |
                ProcessAccess.VMWrite,
                false, this.Process.Id);

            this.ThreadHandle = Internals.OpenThread(
                ThreadAccess.SuspendResume |
                ThreadAccess.QueryInformation,
                false, this.MainThread.Id);

            if (this.Handle.IsInvalid && noFullAccess)
            {
                if (Marshal.GetLastWin32Error() == 5) // ERROR_ACCESS_DENIED
                {
                    IntPtr SidOwner;
                    IntPtr SidGroup;
                    IntPtr Dacl;
                    IntPtr Sacl;
                    IntPtr securityDescriptor;

                    var error = Internals.GetSecurityInfo(Process.GetCurrentProcess().Handle, 0x6, 0x4, out SidOwner, out SidGroup, out Dacl, out Sacl, out securityDescriptor);
                    if (error != 0)
                        throw new Exception("Can't get injector's security secriptor, ErrorCode " + Marshal.GetLastWin32Error());

                    this.Handle = Internals.OpenProcess(ProcessAccess.WriteDAC, false, this.Process.Id);
                    if (this.Handle.IsInvalid)
                        throw new Exception("Process open is failed with only WRITE_DAC access, ErrorCode " + Marshal.GetLastWin32Error());

                    error = Internals.SetSecurityInfo(this.Handle, 0x6, 0x20000004, out SidOwner, out SidGroup, out Dacl, out Sacl);
                    if (error != 0)
                        throw new Exception("Can't override client's DACL, ErrorCode " + Marshal.GetLastWin32Error());

                    this.Handle = Internals.OpenProcess(ProcessAccess.All, false, this.Process.Id);
                }
            }

            if (this.Handle.IsInvalid)
                throw new Win32Exception();

            if (this.ThreadHandle.IsInvalid)
                throw new Win32Exception();

            this.IsOpened = true;
        }

        public IntPtr Rebase(long address)
        {
            return new IntPtr(this.BaseAddress.ToInt64() + address);
        }

        public IntPtr Rebase(IntPtr address)
        {
            return new IntPtr(this.BaseAddress.ToInt64() + address.ToInt64());
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
            if (this.IsDisposed || !disposing)
                return;

            this.Handle.Close();
            this.ThreadHandle.Close();
            Process.LeaveDebugMode();
            this.IsOpened    = false;
            this.IsDisposed  = true;
        }
    }
}