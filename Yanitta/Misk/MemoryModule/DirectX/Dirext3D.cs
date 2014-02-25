using System;
using System.Diagnostics;
using System.Linq;

namespace MemoryModule.DirecX
{
    internal class Dirext3D
    {
        public Process TargetProcess { get; private set; }
        public bool UsingDirectX11   { get; private set; }
        public IntPtr HookPtr        { get; private set; }
        public D3DDevice Device      { get; private set; }

        public Dirext3D(Process targetProc)
        {
            this.TargetProcess = targetProc;

            this.UsingDirectX11 = TargetProcess.Modules.Cast<ProcessModule>().Any(m => m.ModuleName == "d3d11.dll");

            this.Device = UsingDirectX11 
                ? (D3DDevice)new D3D11Device(targetProc) 
                : new D3D9Device(targetProc);

            this.HookPtr = UsingDirectX11
                ? ((D3D11Device)Device).GetSwapVTableFuncAbsoluteAddress(Device.PresentVtableIndex)
                : Device.GetDeviceVTableFuncAbsoluteAddress(Device.EndSceneVtableIndex);
        }
    }
}