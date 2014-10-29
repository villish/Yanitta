using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Yanitta
{
    /// <summary>
    /// Предоставляет методы для записи, чтения и выполнение в процессе.
    /// </summary>
    public class ProcessMemory
    {
        #region API

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1060:MovePInvokesToNativeMethodsClass"), SuppressMessage("Microsoft.Portability", "CA1901:PInvokeDeclarationsShouldBePortable", MessageId = "2"), DllImport("kernel32", SetLastError = true, ExactSpelling = true)]
        static extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr lpAddress, int dwSize, AllocationType flAllocationType, MemoryProtection flProtect);
        [SuppressMessage("Microsoft.Design", "CA1060:MovePInvokesToNativeMethodsClass"), DllImport("kernel32", SetLastError = true)]
        static extern IntPtr OpenThread(ThreadAccess DesiredAccess, [MarshalAs(UnmanagedType.Bool)] bool bInheritHandle, int dwThreadId);
        [SuppressMessage("Microsoft.Design", "CA1060:MovePInvokesToNativeMethodsClass"), SuppressMessage("Microsoft.Portability", "CA1901:PInvokeDeclarationsShouldBePortable", MessageId = "2"), DllImport("kernel32", SetLastError = true, ExactSpelling = true)]
        static extern bool VirtualFreeEx(IntPtr hProcess, IntPtr lpAddress, int dwSize, FreeType dwFreeType);
        [SuppressMessage("Microsoft.Design", "CA1060:MovePInvokesToNativeMethodsClass"), SuppressMessage("Microsoft.Portability", "CA1901:PInvokeDeclarationsShouldBePortable", MessageId = "2"), DllImport("kernel32", SetLastError = true)]
        static extern bool VirtualProtectEx(IntPtr hProcess, IntPtr lpAddress, int dwSize, MemoryProtection flNewProtect, out MemoryProtection lpflOldProtect);
        [SuppressMessage("Microsoft.Portability", "CA1901:PInvokeDeclarationsShouldBePortable", MessageId = "3"), SuppressMessage("Microsoft.Design", "CA1060:MovePInvokesToNativeMethodsClass"), DllImport("kernel32", SetLastError = true)]
        static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, int nSize, IntPtr lpNumberOfBytesWritten);
        [SuppressMessage("Microsoft.Portability", "CA1901:PInvokeDeclarationsShouldBePortable", MessageId = "3"), SuppressMessage("Microsoft.Design", "CA1060:MovePInvokesToNativeMethodsClass"), DllImport("kernel32", SetLastError = true)]
        static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, [Out] byte[] lpBuffer, int dwSize, IntPtr lpNumberOfBytesRead);
        [SuppressMessage("Microsoft.Design", "CA1060:MovePInvokesToNativeMethodsClass"), DllImport("kernel32", SetLastError = true)]
        static extern uint SuspendThread(IntPtr thandle);
        [SuppressMessage("Microsoft.Design", "CA1060:MovePInvokesToNativeMethodsClass"), DllImport("kernel32", SetLastError = true)]
        static extern uint ResumeThread(IntPtr thandle);
        [SuppressMessage("Microsoft.Design", "CA1060:MovePInvokesToNativeMethodsClass"), DllImport("kernel32", SetLastError = true)]
        static extern bool GetThreadContext(IntPtr thandle, ref CONTEXT context);
        [SuppressMessage("Microsoft.Design", "CA1060:MovePInvokesToNativeMethodsClass"), DllImport("kernel32", SetLastError = true)]
        static extern bool SetThreadContext(IntPtr thandle, ref CONTEXT context);
        [SuppressMessage("Microsoft.Design", "CA1060:MovePInvokesToNativeMethodsClass"), DllImport("user32")]
        static extern IntPtr GetForegroundWindow();
        [DllImport("kernel32.dll", SetLastError = true, CallingConvention = CallingConvention.Winapi)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool IsWow64Process([In] IntPtr process, [Out] out bool wow64Process);
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool FlushInstructionCache(IntPtr hProcess, IntPtr lpBaseAddress, int dwSize);

        #endregion

        /// <summary>
        /// Возвращает текущий процесс.
        /// </summary>
        public Process Process { get; private set; }

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="Yanitta.ProcessMemory"/>.
        /// </summary>
        /// <param name="process"></param>
        public ProcessMemory(Process process)
        {
            this.Process = process;
            this.Process.EnableRaisingEvents = true;
        }

        /// <summary>
        /// Выделяет в процессе участок памяти.
        /// </summary>
        /// <param name="size">Размер выделяемой памяти.</param>
        /// <param name="allocType">Тип выделяемой памяти.</param>
        /// <param name="memProtect">Тип защиты памяти.</param>
        /// <returns>Указатель на выделенный участок памяти.</returns>
        public IntPtr Alloc(int size, AllocationType allocType = AllocationType.Commit, MemoryProtection memProtect = MemoryProtection.ExecuteReadWrite)
        {
            if (size <= 0)
                throw new ArgumentNullException("size");

            var address = VirtualAllocEx(this.Process.Handle, IntPtr.Zero, size, allocType, memProtect);

            if (address == IntPtr.Zero)
                throw new Win32Exception();

            return address;
        }

        /// <summary>
        /// Осводождает ранее выделенный участок памяти.
        /// </summary>
        /// <param name="address">Указатель на выделенный участок памяти.</param>
        /// <param name="freeType">Тип осводождения памяти.</param>
        public void Free(IntPtr address, FreeType freeType = FreeType.Release)
        {
            if (address == IntPtr.Zero)
                throw new ArgumentNullException("address");

            if (!VirtualFreeEx(this.Process.Handle, address, 0, freeType))
                throw new Win32Exception();
        }

        /// <summary>
        /// Считывает массив байт из текущего процесса.
        /// </summary>
        /// <param name="address">Указатель на участок памяти с которого надо начать считывание.</param>
        /// <param name="count">Размер считываемого массива.</param>
        /// <returns>Считанный из процесса масив.</returns>
        public unsafe byte[] ReadBytes(IntPtr address, int count)
        {
            var bytes = new byte[count];
            if(!ReadProcessMemory(this.Process.Handle, address, bytes, count, IntPtr.Zero))
                throw new Win32Exception();
            return bytes;
        }

        /// <summary>
        /// Считывает из процесса значение указанного типа.
        /// </summary>
        /// <typeparam name="T">Тип считываемого значения.</typeparam>
        /// <param name="address">Указатель на участок памяти от куда надо считать значение.</param>
        /// <returns>Значение указанного типа.</returns>
        public unsafe T Read<T>(IntPtr address) where T : struct
        {
            var result = new byte[Marshal.SizeOf(typeof(T))];
            ReadProcessMemory(this.Process.Handle, address, result, result.Length, IntPtr.Zero);
            var handle = GCHandle.Alloc(result, GCHandleType.Pinned);
            T returnObject = (T)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(T));
            handle.Free();
            return returnObject;
        }

        /// <summary>
        /// Считывает из процесса строку заканчивающуюся 0 в кодировке utf-8.
        /// </summary>
        /// <param name="addess">Указатель на участок памяти от куда надо считать значение.</param>
        /// <param name="length">Длинна строки (ограничение).</param>
        /// <returns>Считанная строка</returns>
        public string ReadString(IntPtr addess, int length = 100)
        {
            var result = new byte[length];
            if (!ReadProcessMemory(this.Process.Handle, addess, result, length, IntPtr.Zero))
                throw new Win32Exception();
            return Encoding.UTF8.GetString(result.TakeWhile(ret => ret != 0).ToArray());
        }

        /// <summary>
        /// Записывает в память процесса значение указанного типа.
        /// </summary>
        /// <typeparam name="T">Тип записываемого значения.</typeparam>
        /// <param name="value">Значение, которое надо записать в память процесса.</param>
        /// <returns>Указатель на участок памяти куда записано значение.</returns>
        public IntPtr Write<T>(T value) where T : struct
        {
            var buffer  = new byte[Marshal.SizeOf(value)];
            var hObj    = Marshal.AllocHGlobal(buffer.Length);
            var address = Alloc(buffer.Length);
            if (address == IntPtr.Zero)
                throw new Win32Exception();
            try
            {
                Marshal.StructureToPtr(value, hObj, false);
                Marshal.Copy(hObj, buffer, 0, buffer.Length);
                if (!WriteProcessMemory(this.Process.Handle, address, buffer, buffer.Length, IntPtr.Zero))
                    throw new Win32Exception();
            }
            catch
            {
                Free(address);
            }
            finally
            {
                Marshal.FreeHGlobal(hObj);
            }

            return address;
        }

        /// <summary>
        /// Записывает в память процесса значение указанного типа.
        /// </summary>
        /// <typeparam name="T">Тип записываемого значения.</typeparam>
        /// <param name="address">Указатель на участок памяти куда надо записать значение.</param>
        /// <param name="value">Значение, которое надо записать в память процесса.</param>
        public void Write<T>(IntPtr address, T value) where T : struct
        {
            var buffer = new byte[Marshal.SizeOf(value)];
            var hObj   = Marshal.AllocHGlobal(buffer.Length);
            try
            {
                Marshal.StructureToPtr(value, hObj, false);
                Marshal.Copy(hObj, buffer, 0, buffer.Length);
                if (!WriteProcessMemory(this.Process.Handle, address, buffer, buffer.Length, IntPtr.Zero))
                    throw new Win32Exception();
            }
            finally
            {
                Marshal.FreeHGlobal(hObj);
            }
        }

        /// <summary>
        /// Затисывает массив байт в память процесса.
        /// </summary>
        /// <param name="buffer">Массив байт.</param>
        /// <returns>Указатель на участок памяти куда записан массив.</returns>
        public IntPtr Write(byte[] buffer)
        {
            var addr = this.Alloc(buffer.Length);
            if (addr == IntPtr.Zero)
                throw new Win32Exception();
            this.Write(addr, buffer);
            return addr;
        }

        /// <summary>
        /// Затисывает массив байт в память процесса.
        /// </summary>
        /// <param name="address">Указатель на участок памяти куда надо записать массив.</param>
        /// <param name="buffer">Массив байт.</param>
        public void Write(IntPtr address, byte[] buffer)
        {
            if (!WriteProcessMemory(this.Process.Handle, address, buffer, buffer.Length, IntPtr.Zero))
                throw new Win32Exception();
        }

        /// <summary>
        /// Записывает в память процесса строку по указанному аддрессу в кодировке utf-8.
        /// </summary>
        /// <param name="address">Указатель на участок памяти куда надо записать строку.</param>
        /// <param name="str">Записываемая строка.</param>
        public void WriteCString(IntPtr address, string str)
        {
            var buffer = Encoding.UTF8.GetBytes(str + '\0');
            if (!WriteProcessMemory(this.Process.Handle, address, buffer, buffer.Length, IntPtr.Zero))
                throw new Win32Exception();
        }

        /// <summary>
        /// Записывает в память процесса указанную строку.
        /// </summary>
        /// <param name="str">Строка для записи в память.</param>
        /// <returns>Указатель на строку в памяти.</returns>
        public IntPtr WriteCString(string str)
        {
            var buffer = Encoding.UTF8.GetBytes(str + '\0');
            var address = Alloc(buffer.Length);
            if (!WriteProcessMemory(this.Process.Handle, address, buffer, buffer.Length, IntPtr.Zero))
                throw new Win32Exception();
            return address;
        }

        /// <summary>
        /// Выполняет функцию по указанному адрессу с указанным списком аргуметов.
        /// </summary>
        /// <param name="injAddress">Адрес в памяти куда записывается исполнимый байткод.</param>
        /// <param name="callAddress">Относительный адресс выполняемой функции.</param>
        /// <param name="funcArgs">
        /// Параметры функции.
        /// Параметрами могут выступать как и значения так и указатели на значения.
        /// </param>
        public void Call(IntPtr injAddress, IntPtr callAddress, params int[] funcArgs)
        {
            var tHandle = OpenThread(ThreadAccess.All, false, this.Process.Threads[0].Id);
            if (SuspendThread(tHandle) == 0xFFFFFFFF)
                throw new Win32Exception();

            var context = new CONTEXT { ContextFlags = ContextFlags.Control };
            if (!GetThreadContext(tHandle, ref context))
                throw new Win32Exception();

            var retaddr = Write<uint>(0xDEAD);

            var bytes = new List<byte>();

            #region ASM

            // push eip (stored refernse to next inctruction)
            bytes.Add(0x68);
            bytes.AddRange(BitConverter.GetBytes(context.Eip));

            // pushad (stored general registers)
            bytes.Add(0x60);
            // pushfd (stored flags)
            bytes.Add(0x9C);

            // pushed to the stack function arguments
            for (int i = funcArgs.Length - 1; i >= 0; --i)
            {
                if (funcArgs[i] == 0)
                {
                    // push 0
                    bytes.Add(0x6A);
                    bytes.Add(0x00);
                }
                else
                {
                    // push param address
                    bytes.Add(0x68);
                    bytes.AddRange(BitConverter.GetBytes(funcArgs[i]));
                }
            }

            // mov eax, callAddress
            bytes.Add(0xB8);
            bytes.AddRange(BitConverter.GetBytes(callAddress.ToInt32()));

            // call eax
            bytes.Add(0xFF);
            bytes.Add(0xD0);

            // add esp, arg_count * pointersize (__cdecl correct stack)
            bytes.Add(0x83);
            bytes.Add(0xC4);
            bytes.Add((byte)(funcArgs.Length * IntPtr.Size));

            // mov [retaddr], eax
            bytes.Add(0xA3);
            bytes.AddRange(BitConverter.GetBytes(retaddr.ToInt32()));

            // popfd (restore flags)
            bytes.Add(0x9D);
            // popad (restore general registers)
            bytes.Add(0x61);
            // retn
            bytes.Add(0xC3);

            #endregion

            var oldProtect = MemoryProtection.ReadOnly;

            // Save original code and disable protect
            var oldCode = this.ReadBytes(injAddress, bytes.Count);
            if (!VirtualProtectEx(this.Process.Handle, injAddress, bytes.Count, MemoryProtection.ExecuteReadWrite, out oldProtect))
                throw new Win32Exception();

            this.Write(injAddress, bytes.ToArray());

            context.Eip          = (uint)injAddress.ToInt32();
            context.ContextFlags = ContextFlags.Control;

            if (!SetThreadContext(tHandle, ref context) || ResumeThread(tHandle) == 0xFFFFFFFF)
                throw new Win32Exception();

            for (int i = 0; i < 0x100; ++i)
            {
                System.Threading.Thread.Sleep(15);
                if (this.Read<uint>(retaddr) != 0xDEAD)
                    break;
            }

            // restore protection and original code
            this.Write(injAddress, oldCode);

            if (!FlushInstructionCache(this.Process.Handle, injAddress, bytes.Count))
                throw new Win32Exception();

            if (!VirtualProtectEx(this.Process.Handle, injAddress, bytes.Count, oldProtect, out oldProtect))
                throw new Win32Exception();

            this.Free(retaddr);
        }

        /// <summary>
        /// Возвращает абсолютный аддресс в процессе.
        /// </summary>
        /// <param name="offset">Относительный аддресс.</param>
        /// <returns>абсолютный аддресс в процессе.</returns>
        public IntPtr Rebase(long offset)
        {
            return new IntPtr(offset + this.Process.MainModule.BaseAddress.ToInt64());
        }

        /// <summary>
        /// Указывает что главное окно процесса находится на переднем плане.
        /// </summary>
        public bool IsFocusMainWindow
        {
            get { return this.Process.MainWindowHandle == GetForegroundWindow(); }
        }

        public bool IsX64
        {
            get
            {
                bool wow64Proxess;
                var ver = Environment.OSVersion.Version;
                IsWow64Process(this.Process.Handle, out wow64Proxess);
                return wow64Proxess && (ver.Major > 5 || (ver.Major == 5 && ver.Minor >= 1));
            }
        }
    }

    #region Enums

    /// <summary>
    /// Тип выделения памяти.
    /// </summary>
    [Flags]
    public enum AllocationType : uint
    {
        Commit     = 0x00001000,
        Reserve    = 0x00002000,
        Decommit   = 0x00004000,
        Release    = 0x00008000,
        Reset      = 0x00080000,
        TopDown    = 0x00100000,
        WriteWatch = 0x00200000,
        Physical   = 0x00400000,
        LargePages = 0x20000000,
    }

    /// <summary>
    /// Тип защиты памяти.
    /// </summary>
    [Flags]
    public enum MemoryProtection : uint
    {
        NoAccess                 = 0x001,
        ReadOnly                 = 0x002,
        ReadWrite                = 0x004,
        WriteCopy                = 0x008,
        Execute                  = 0x010,
        ExecuteRead              = 0x020,
        ExecuteReadWrite         = 0x040,
        ExecuteWriteCopy         = 0x080,
        GuardModifierflag        = 0x100,
        NoCacheModifierflag      = 0x200,
        WriteCombineModifierflag = 0x400,
    }

    /// <summary>
    /// Тип освобождения памяти.
    /// </summary>
    [Flags]
    public enum FreeType : uint
    {
        Decommit = 0x4000,
        Release  = 0x8000,
    }

    /// <summary>
    /// Тип доступа к процессу.
    /// </summary>
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
    public enum ContextFlags : uint
    {
        i386              = 0x10000,
        Control           = i386    | 0x01, // SS:SP, CS:IP, FLAGS, BP
        Integer           = i386    | 0x02, // AX, BX, CX, DX, SI, DI
        Segments          = i386    | 0x04, // DS, ES, FS, GS
        FloatingPoint     = i386    | 0x08, // 387 state
        DebugRegisters    = i386    | 0x10, // DB 0-3,6,7
        ExtendedRegisters = i386    | 0x20, // cpu specific extensions
        Full              = Control | Integer | Segments,
        All               = Control | Integer | Segments | FloatingPoint | DebugRegisters | ExtendedRegisters
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct CONTEXT
    {
        public ContextFlags ContextFlags; //set this to an appropriate value
        // Retrieved by CONTEXT_DEBUG_REGISTERS
        public uint Dr0;
        public uint Dr1;
        public uint Dr2;
        public uint Dr3;
        public uint Dr6;
        public uint Dr7;
        // Retrieved by CONTEXT_FLOATING_POINT
        [MarshalAs(UnmanagedType.ByValArray, SizeConst=112)]//512
        public byte[] FloatSave;
        // Retrieved by CONTEXT_SEGMENTS
        public uint SegGs;
        public uint SegFs;
        public uint SegEs;
        public uint SegDs;
        // Retrieved by CONTEXT_INTEGER
        public uint Edi;
        public uint Esi;
        public uint Ebx;
        public uint Edx;
        public uint Ecx;
        public uint Eax;
        // Retrieved by CONTEXT_CONTROL
        public uint Ebp;
        public uint Eip;
        public uint SegCs;
        public uint EFlags;
        public uint Esp;
        public uint SegSs;
        // Retrieved by CONTEXT_EXTENDED_REGISTERS
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 512)]
        public byte[] ExtendedRegisters;
    }

    #endregion
}