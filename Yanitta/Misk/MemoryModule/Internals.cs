using System;
using System.Runtime.InteropServices;

namespace MemoryModule
{
    public class Internals
    {
        [DllImport("Kernel32.dll", EntryPoint = "RtlMoveMemory", SetLastError = false)]
        public unsafe static extern void MoveMemory(void* dest, void* src, int size);

        [DllImport("kernel32", EntryPoint = "OpenProcess", SetLastError = true)]
        public static extern SafeProcessHandle OpenProcess(ProcessAccess dwDesiredAccess, [MarshalAs(UnmanagedType.Bool)] bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32", EntryPoint = "OpenThread", SetLastError = true)]
        public static extern SafeProcessHandle OpenThread(ThreadAccess DesiredAccess, [MarshalAs(UnmanagedType.Bool)] bool bInheritHandle, int dwThreadId);

        [DllImport("kernel32", EntryPoint = "SuspendThread")]
        public static extern uint SuspendThread(SafeProcessHandle hThread);

        [DllImport("kernel32", EntryPoint = "ResumeThread")]
        public static extern uint ResumeThread(SafeProcessHandle hThread);

        [DllImport("kernel32", EntryPoint = "ReadProcessMemory")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public unsafe static extern bool ReadProcessMemory(SafeProcessHandle hProcess, IntPtr dwAddress, void* lpBuffer, int nSize, out int lpBytesRead);

        [DllImport("kernel32", EntryPoint = "WriteProcessMemory")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public unsafe static extern bool WriteProcessMemory(SafeProcessHandle hProcess, IntPtr dwAddress, void* lpBuffer, int nSize, out int iBytesWritten);

        [DllImport("kernel32", EntryPoint = "WriteProcessMemory")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool WriteProcessMemory(SafeProcessHandle hProcess, IntPtr dwAddress, byte[] lpBuffer, int nSize, out int iBytesWritten);

        [DllImport("kernel32", EntryPoint = "VirtualAllocEx", SetLastError = true, ExactSpelling = true)]
        public static extern IntPtr VirtualAllocEx(SafeProcessHandle hProcess, IntPtr lpAddress, int dwSize, AllocationType flAllocationType, MemoryProtection flProtect);

        [DllImport("kernel32", EntryPoint = "VirtualFreeEx", SetLastError = true, ExactSpelling = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool VirtualFreeEx(SafeProcessHandle hProcess, IntPtr lpAddress, int dwSize, FreeType dwFreeType);

        [DllImport("kernel32", EntryPoint = "CloseHandle", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool CloseHandle(SafeProcessHandle hObject);

        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();

        [DllImport("user32", SetLastError = true)]
        public static extern int GetWindowThreadProcessId([In]IntPtr hwnd, [Out]out int lProcessId);
    }

    [Flags]
    public enum ProcessAccess : uint
    {
        Terminate           = 0x00000001,
        CreateThread        = 0x00000002,
        VMOperation         = 0x00000008,
        VMRead              = 0x00000010,
        VMWrite             = 0x00000020,
        DupHandle           = 0x00000040,
        SetInformation      = 0x00000200,
        QueryInformation    = 0x00000400,
        Synchronize         = 0x00100000,
        All                 = 0x001F0FFF,
    }

    [Flags]
    public enum ThreadAccess : uint
    {
        Terminate           = 0x00001,
        SuspendResume       = 0x00002,
        GetContext          = 0x00008,
        SetContext          = 0x00010,
        SetInformation      = 0x00020,
        QueryInformation    = 0x00040,
        SetThreadToken      = 0x00080,
        Impersonate         = 0x00100,
        DirectImpersonation = 0x00200,
        All                 = 0x1F03FF
    }

    [Flags]
    public enum AllocationType : uint
    {
        Commit      = 0x00001000,
        Reserve     = 0x00002000,
        Decommit    = 0x00004000,
        Release     = 0x00008000,
        Reset       = 0x00080000,
        TopDown     = 0x00100000,
        WriteWatch  = 0x00200000,
        Physical    = 0x00400000,
        LargePages  = 0x20000000,
    }

    [Flags]
    public enum MemoryProtection : uint
    {
        NoAccess                    = 0x001,
        ReadOnly                    = 0x002,
        ReadWrite                   = 0x004,
        WriteCopy                   = 0x008,
        Execute                     = 0x010,
        ExecuteRead                 = 0x020,
        ExecuteReadWrite            = 0x040,
        ExecuteWriteCopy            = 0x080,
        GuardModifierflag           = 0x100,
        NoCacheModifierflag         = 0x200,
        WriteCombineModifierflag    = 0x400,
    }

    [Flags]
    public enum FreeType : uint
    {
        Decommit = 0x4000,
        Release  = 0x8000,
    }
}
