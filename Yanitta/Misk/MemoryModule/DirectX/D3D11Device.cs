using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace MemoryModule.DirecX
{
    internal sealed class D3D11Device : D3DDevice
    {
        [DllImport("d3d11.dll")]
        internal static extern unsafe int D3D11CreateDeviceAndSwapChain(void* pAdapter, int driverType, void* Software,
            int flags, void* pFeatureLevels, int FeatureLevels, int SDKVersion, void* pSwapChainDesc, void* ppSwapChain,
            void* ppDevice, void* pFeatureLevel, void* ppImmediateContext);

        [DllImport("d3d11.dll", EntryPoint = "D3D11CreateDeviceAndSwapChain")]
        static extern int D3D11CreateDeviceAndSwapChain2(
            IntPtr pAdapter,
            int driverType,
            IntPtr Software,
            int flags,
            IntPtr pFeatureLevels,
            int FeatureLevels,
            int SDKVersion,
            out IntPtr pSwapChainDesc,
            out IntPtr ppSwapChain,
            out IntPtr ppDevice,
            IntPtr pFeatureLevel,
            out IntPtr ppImmediateContext);

        const int DXGI_FORMAT_R8G8B8A8_UNORM      = 0x1C;
        const int DXGI_USAGE_RENDER_TARGET_OUTPUT = 0x20;
        const int D3D11_SDK_VERSION               = 0x07;
        const int D3D_DRIVER_TYPE_HARDWARE        = 0x01;

        private IntPtr swapChain    = IntPtr.Zero;
        private IntPtr device       = IntPtr.Zero;
        private IntPtr myDxgiDll    = IntPtr.Zero;
        private IntPtr theirDxgiDll = IntPtr.Zero;

        private VTableFuncDelegate deviceRelease;
        private VTableFuncDelegate deviceContextRelease;
        private VTableFuncDelegate swapchainRelease;

        public D3D11Device(Process targetProc)
            : base(targetProc, "d3d11.dll")
        {
        }

        protected override void InitD3D(out IntPtr d3DDevicePtr)
        {
            LoadDxgiDll();

            var scd = new SwapChainDescription {
                BufferCount = 1,
                ModeDescription = new ModeDescription { Format = DXGI_FORMAT_R8G8B8A8_UNORM },
                Usage = DXGI_USAGE_RENDER_TARGET_OUTPUT,
                OutputHandle = Form.Handle,
                SampleDescription = new SampleDescription { Count = 1 },
                IsWindowed = true
            };

            unsafe
            {
                var pSwapChain = IntPtr.Zero;
                var pDevice    = IntPtr.Zero;
                var pImmediateContext = IntPtr.Zero;

                var ret = D3D11CreateDeviceAndSwapChain(
                    (void*)IntPtr.Zero,
                    D3D_DRIVER_TYPE_HARDWARE,
                    (void*)IntPtr.Zero,
                    0,
                    (void*)IntPtr.Zero,
                    0,
                    D3D11_SDK_VERSION,
                    &scd,
                    &pSwapChain,
                    &pDevice,
                    (void*)IntPtr.Zero,
                    &pImmediateContext
                    );

                Console.WriteLine("D3D11CreateDeviceAndSwapChain result: {0:X}", ret);

                swapChain    = pSwapChain;
                device       = pDevice;
                d3DDevicePtr = pImmediateContext;

                if (ret >= 0)
                {
                    swapchainRelease     = GetDelegate<VTableFuncDelegate>(GetVTableFuncAddress(swapChain,    VTableIndexes.DXGISwapChainRelease));
                    deviceRelease        = GetDelegate<VTableFuncDelegate>(GetVTableFuncAddress(device,       VTableIndexes.D3D11DeviceRelease));
                    deviceContextRelease = GetDelegate<VTableFuncDelegate>(GetVTableFuncAddress(d3DDevicePtr, VTableIndexes.D3D11DeviceContextRelease));
                }
            }
        }

        private void LoadDxgiDll()
        {
            this.myDxgiDll = LoadLibrary("dxgi.dll");
            if (myDxgiDll == IntPtr.Zero)
                throw new FileLoadException(String.Format("Could not load {0}", "dxgi.dll"));

            theirDxgiDll = TargetProcess.Modules.Cast<ProcessModule>().First(m => m.ModuleName == "dxgi.dll").BaseAddress;
        }

        public unsafe IntPtr GetSwapVTableFuncAbsoluteAddress(int funcIndex)
        {
            var pointer = *(IntPtr*)((void*)swapChain);
                pointer = *(IntPtr*)((void*)((int)pointer + funcIndex * 4));
            var offset  = IntPtr.Subtract(pointer, myDxgiDll.ToInt32());
            return IntPtr.Add(theirDxgiDll, offset.ToInt32());
        }

        protected override void CleanD3D()
        {
            if (swapChain != IntPtr.Zero)
                swapchainRelease(swapChain);

            if (device != IntPtr.Zero)
                deviceRelease(device);

            if (D3DDevicePtr != IntPtr.Zero)
                deviceContextRelease(D3DDevicePtr);
        }

        public override int BeginSceneVtableIndex
        {
            get { return VTableIndexes.D3D11DeviceContextBegin; }
        }

        public override int EndSceneVtableIndex
        {
            get { return VTableIndexes.D3D11DeviceContextEnd; }
        }

        public override int PresentVtableIndex
        {
            get { return VTableIndexes.DXGISwapChainPresent; }
        }

        #region Embedded Types

#pragma warning disable 169

        [StructLayout(LayoutKind.Sequential)]
        public struct Rational
        {
            public int Numerator;
            public int Denominator;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct ModeDescription
        {
            public int Width;
            public int Height;
            public Rational RefreshRate;
            public int Format;
            public int ScanlineOrdering;
            public int Scaling;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SampleDescription
        {
            public int Count;
            public int Quality;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SwapChainDescription
        {
            public ModeDescription ModeDescription;
            public SampleDescription SampleDescription;
            public int Usage;
            public int BufferCount;
            public IntPtr OutputHandle;
            [MarshalAs(UnmanagedType.Bool)]
            public bool IsWindowed;
            public int SwapEffect;
            public int Flags;
        }

        public struct VTableIndexes
        {
            public const int DXGISwapChainRelease       = 0x02;
            public const int D3D11DeviceRelease         = 0x02;
            public const int D3D11DeviceContextRelease  = 0x02;
            public const int DXGISwapChainPresent       = 0x08;
            public const int D3D11DeviceContextBegin    = 0x1B;
            public const int D3D11DeviceContextEnd      = 0x1C;
        }


#pragma warning restore 169

        #endregion
    }
}