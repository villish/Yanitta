using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace MemoryModule.DirecX
{
    internal sealed class D3D9Device : D3DDevice
    {
        [DllImport("d3d9.dll")]
        internal static extern IntPtr Direct3DCreate9(uint sdkVersion);

        private const int D3D9SdkVersion = 0x20;
        private const int D3DCREATE_SOFTWARE_VERTEXPROCESSING = 0x20;

        private VTableFuncDelegate d3DDeviceRelease;
        private VTableFuncDelegate d3DRelease;

        private IntPtr pD3D = IntPtr.Zero;

        public D3D9Device(Process targetProc)
            : base(targetProc, "d3d9.dll")
        {
        }

        public override int BeginSceneVtableIndex
        {
            get { return VTableIndexes.Direct3DDevice9BeginScene; }
        }

        public override int EndSceneVtableIndex
        {
            get { return VTableIndexes.Direct3DDevice9EndScene; }
        }

        public override int PresentVtableIndex
        {
            get { return VTableIndexes.Direct3DDevice9Present; }
        }

        protected override void InitD3D(out IntPtr d3DDevicePtr)
        {
            pD3D = Direct3DCreate9(D3D9SdkVersion);
            if (pD3D == IntPtr.Zero)
                throw new Exception("Failed to create D3D.");

            var parameters = new D3DPresentParameters {
                Windowed = true,
                SwapEffect = 1,
                BackBufferFormat = 0
            };

            var createDevicePtr = GetVTableFuncAddress(pD3D, VTableIndexes.Direct3D9CreateDevice);
            var createDevice    = GetDelegate<CreateDeviceDelegate>(createDevicePtr);

            if (createDevice(pD3D, 0, 1, Form.Handle, D3DCREATE_SOFTWARE_VERTEXPROCESSING, ref parameters, out d3DDevicePtr) < 0)
                throw new Exception("Failed to create device.");

            d3DDeviceRelease = GetDelegate<VTableFuncDelegate>(GetVTableFuncAddress(D3DDevicePtr, VTableIndexes.Direct3DDevice9Release));
            d3DRelease       = GetDelegate<VTableFuncDelegate>(GetVTableFuncAddress(pD3D, VTableIndexes.Direct3D9Release));
        }

        protected override void CleanD3D()
        {
            if (D3DDevicePtr != IntPtr.Zero)
                d3DDeviceRelease(D3DDevicePtr);

            if (pD3D != IntPtr.Zero)
                d3DRelease(pD3D);
        }

        #region Embedded Types

        #region Nested type: CreateDeviceDelegate

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int CreateDeviceDelegate(IntPtr instance, uint adapter, uint deviceType, IntPtr focusWindow, uint behaviorFlags, 
            [In] ref D3DPresentParameters presentationParameters, out IntPtr returnedDeviceInterface);

        #endregion

        #region Nested type: D3DPresentParameters

        [StructLayout(LayoutKind.Sequential)]
        public struct D3DPresentParameters
        {
            public readonly uint BackBufferWidth;
            public readonly uint BackBufferHeight;
            public uint BackBufferFormat;
            public readonly uint BackBufferCount;
            public readonly uint MultiSampleType;
            public readonly uint MultiSampleQuality;
            public uint SwapEffect;
            public readonly IntPtr hDeviceWindow;

            [MarshalAs(UnmanagedType.Bool)]
            public bool Windowed;

            [MarshalAs(UnmanagedType.Bool)]
            public readonly bool EnableAutoDepthStencil;

            public readonly uint AutoDepthStencilFormat;
            public readonly uint Flags;
            public readonly uint FullScreen_RefreshRateInHz;
            public readonly uint PresentationInterval;
        }

        #endregion

        #region Nested type: VTableIndexes

        public struct VTableIndexes
        {
            public const int Direct3D9Release          = 0x02;
            public const int Direct3D9CreateDevice     = 0x10;
            public const int Direct3DDevice9Release    = 0x02;
            public const int Direct3DDevice9Reset      = 0x10;
            public const int Direct3DDevice9Present    = 0x11;
            public const int Direct3DDevice9BeginScene = 0x29;
            public const int Direct3DDevice9EndScene   = 0x2A;
            public const int Direct3DDevice9Clear      = 0x2B;
        }

        #endregion

        #endregion Embedded Types
    }
}