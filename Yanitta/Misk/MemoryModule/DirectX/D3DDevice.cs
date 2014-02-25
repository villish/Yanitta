using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Windows.Forms;

namespace MemoryModule.DirecX
{
    internal abstract class D3DDevice : IDisposable
    {
        [SuppressUnmanagedCodeSecurity, DllImport("kernel32", CharSet = CharSet.Unicode)]
        internal static extern IntPtr LoadLibrary(string libraryName);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        internal static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool FreeLibrary(IntPtr hModule);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        protected delegate void VTableFuncDelegate(IntPtr instance);

        protected readonly IntPtr D3DDevicePtr = IntPtr.Zero;
        protected readonly Process TargetProcess;
        private readonly string d3DDllName;
        private readonly List<IntPtr> loadedLibraries = new List<IntPtr>();

        private IntPtr myD3DDll    = IntPtr.Zero;
        private IntPtr theirD3DDll = IntPtr.Zero;

        private bool disposed;

        protected Form Form { get; private set; }
        public abstract int BeginSceneVtableIndex { get; }
        public abstract int EndSceneVtableIndex { get; }
        public abstract int PresentVtableIndex { get; }

        protected D3DDevice(Process targetProcess, string d3DDllName)
        {
            this.TargetProcess = targetProcess;
            this.d3DDllName    = d3DDllName;
            this.Form          = new Form();

            this.LoadDll();
            this.InitD3D(out D3DDevicePtr);
        }

        /// <summary>
        /// Initiializes d3d and sets device pointer.
        /// </summary>
        protected abstract void InitD3D(out IntPtr d3DDevicePtr);

        /// <summary>
        /// Cleanup should be done in here.
        /// </summary>
        protected abstract void CleanD3D();

        private void LoadDll()
        {
            this.myD3DDll = LoadLibraryByName(d3DDllName);
            if (this.myD3DDll == IntPtr.Zero)
                throw new Exception(String.Format("Could not load {0}", d3DDllName));

            this.theirD3DDll = TargetProcess.Modules.Cast<ProcessModule>().First(m => m.ModuleName == d3DDllName).BaseAddress;
        }

        protected IntPtr LoadLibraryByName(string library)
        {
            // Attempt to grab the module handle if its loaded already.
            var ret = GetModuleHandle(library);
            if (ret == IntPtr.Zero)
            {
                // Load the lib manually if its not, storing it in a list so we can free it later.
                ret = LoadLibrary(library);
                loadedLibraries.Add(ret);
            }
            return ret;
        }

        protected unsafe IntPtr GetVTableFuncAddress(IntPtr obj, int funcIndex)
        {
            var pointer = *(IntPtr*)((void*)obj);
            return *(IntPtr*)((void*)((int)pointer + funcIndex * 4));
        }

        public unsafe IntPtr GetDeviceVTableFuncAbsoluteAddress(int funcIndex)
        {
            var pointer = *(IntPtr*)((void*)D3DDevicePtr);
            pointer     = *(IntPtr*)((void*)((int)pointer + funcIndex * 4));
            var offset  = IntPtr.Subtract(pointer, myD3DDll.ToInt32());
            return IntPtr.Add(theirD3DDll, offset.ToInt32());
        }

        protected T GetDelegate<T>(IntPtr address) where T : class
        {
            return Marshal.GetDelegateForFunctionPointer(address, typeof(T)) as T;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    CleanD3D();

                    if (Form != null)
                        Form.Dispose();

                    foreach (var loadedLibrary in loadedLibraries)
                    {
                        FreeLibrary(loadedLibrary);
                    }
                }
                disposed = true;
            }
        }

        ~D3DDevice()
        {
            Dispose(false);
        }
    }
}