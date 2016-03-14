using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection.Emit;
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

        [SuppressMessage("Microsoft.Design", "CA1060:MovePInvokesToNativeMethodsClass"), SuppressMessage("Microsoft.Portability", "CA1901:PInvokeDeclarationsShouldBePortable", MessageId = "2"), DllImport("kernel32", SetLastError = true, ExactSpelling = true)]
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
        static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, IntPtr lpBuffer, int nSize, IntPtr lpNumberOfBytesWritten);
        [SuppressMessage("Microsoft.Portability", "CA1901:PInvokeDeclarationsShouldBePortable", MessageId = "3"), SuppressMessage("Microsoft.Design", "CA1060:MovePInvokesToNativeMethodsClass"), DllImport("kernel32", SetLastError = true)]
        static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, [Out] byte[] lpBuffer, int dwSize, IntPtr lpNumberOfBytesRead);
        [SuppressMessage("Microsoft.Portability", "CA1901:PInvokeDeclarationsShouldBePortable", MessageId = "3"), SuppressMessage("Microsoft.Design", "CA1060:MovePInvokesToNativeMethodsClass"), DllImport("kernel32", SetLastError = true)]
        static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, [Out] IntPtr lpBuffer, int dwSize, IntPtr lpNumberOfBytesRead);
        [SuppressMessage("Microsoft.Design", "CA1060:MovePInvokesToNativeMethodsClass"), DllImport("kernel32", SetLastError = true)]
        static extern uint SuspendThread(IntPtr thandle);
        [SuppressMessage("Microsoft.Design", "CA1060:MovePInvokesToNativeMethodsClass"), DllImport("kernel32", SetLastError = true)]
        static extern uint ResumeThread(IntPtr thandle);
        [SuppressMessage("Microsoft.Design", "CA1060:MovePInvokesToNativeMethodsClass"), DllImport("kernel32", SetLastError = true)]
        static extern bool GetThreadContext(IntPtr thandle, ref WOW64_CONTEXT context);
        [SuppressMessage("Microsoft.Design", "CA1060:MovePInvokesToNativeMethodsClass"), DllImport("kernel32", SetLastError = true)]
        static extern bool SetThreadContext(IntPtr thandle, ref WOW64_CONTEXT context);
        [SuppressMessage("Microsoft.Design", "CA1060:MovePInvokesToNativeMethodsClass"), DllImport("user32")]
        static extern IntPtr GetForegroundWindow();
        [DllImport("kernel32.dll", SetLastError = true, CallingConvention = CallingConvention.Winapi)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool IsWow64Process([In] IntPtr process, [Out] out bool wow64Process);
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool FlushInstructionCache(IntPtr hProcess, IntPtr lpBaseAddress, int dwSize);

        [DllImport("user32.dll", SetLastError = true)]
        static extern uint GetWindowThreadProcessId(IntPtr hWnd, out int lpdwProcessId);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        #endregion

        /// <summary>
        /// Возвращает текущий процесс.
        /// </summary>
        public Process Process { get; private set; }

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="ProcessMemory"/>.
        /// </summary>
        /// <param name="process"></param>
        public ProcessMemory(Process process)
        {
            Process = process;
            Process.EnableRaisingEvents = true;
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
                throw new ArgumentNullException(nameof(size));

            var address = VirtualAllocEx(Process.Handle, IntPtr.Zero, size, allocType, memProtect);

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
                throw new ArgumentNullException(nameof(address));

            if (!VirtualFreeEx(Process.Handle, address, 0, freeType))
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
            if(!ReadProcessMemory(Process.Handle, address, bytes, count, IntPtr.Zero))
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
            fixed (byte* pointer = new byte[MarshalCache<T>.Size])
            {
                ReadProcessMemory(Process.Handle, address, new IntPtr(pointer), MarshalCache<T>.Size, IntPtr.Zero);
                return (T)Marshal.PtrToStructure(new IntPtr(pointer), MarshalCache<T>.RealType);
            }
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
            if (!ReadProcessMemory(Process.Handle, addess, result, length, IntPtr.Zero))
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
            var address = Alloc(MarshalCache<T>.Size);
            if (address == IntPtr.Zero)
                throw new Win32Exception();
            try
            {
                Write(address, value);
            }
            catch
            {
                Free(address);
                address = IntPtr.Zero;
            }

            return address;
        }

        /// <summary>
        /// Записывает в память процесса значение указанного типа.
        /// </summary>
        /// <typeparam name="T">Тип записываемого значения.</typeparam>
        /// <param name="address">Указатель на участок памяти куда надо записать значение.</param>
        /// <param name="value">Значение, которое надо записать в память процесса.</param>
        public unsafe void Write<T>(IntPtr address, T value) where T : struct
        {
            void* pointer = MarshalCache<T>.GetUnsafePtr(ref value);
            if (!WriteProcessMemory(Process.Handle, address, new IntPtr(pointer), MarshalCache<T>.Size, IntPtr.Zero))
                throw new Win32Exception();
        }

        /// <summary>
        /// Записывает массив байт в память процесса.
        /// </summary>
        /// <param name="buffer">Массив байт.</param>
        /// <returns>Указатель на участок памяти куда записан массив.</returns>
        public IntPtr Write(byte[] buffer)
        {
            var addr = Alloc(buffer.Length);
            if (addr == IntPtr.Zero)
                throw new Win32Exception();
            Write(addr, buffer);
            return addr;
        }

        /// <summary>
        /// Записывает массив байт в память процесса.
        /// </summary>
        /// <param name="address">Указатель на участок памяти куда надо записать массив.</param>
        /// <param name="buffer">Массив байт.</param>
        public void Write(IntPtr address, byte[] buffer)
        {
            if (!WriteProcessMemory(Process.Handle, address, buffer, buffer.Length, IntPtr.Zero))
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
            if (!WriteProcessMemory(Process.Handle, address, buffer, buffer.Length, IntPtr.Zero))
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
            if (!WriteProcessMemory(Process.Handle, address, buffer, buffer.Length, IntPtr.Zero))
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
        public void Call_x32(IntPtr injAddress, IntPtr callAddress, params int[] funcArgs)
        {
            var tHandle = OpenThread(ThreadAccess.All, false, Process.Threads[0].Id);
            if (SuspendThread(tHandle) == 0xFFFFFFFF)
                throw new Win32Exception();

            var context = new WOW64_CONTEXT { ContextFlags = 0x10001 /*CONTROL*/ };
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
                // push param address
                bytes.Add(0x68);
                bytes.AddRange(BitConverter.GetBytes(funcArgs[i]));
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
            var oldCode = ReadBytes(injAddress, bytes.Count);
            if (!VirtualProtectEx(Process.Handle, injAddress, bytes.Count, MemoryProtection.ExecuteReadWrite, out oldProtect))
                throw new Win32Exception();

            Write(injAddress, bytes.ToArray());

            context.Eip = (uint)injAddress.ToInt32();

            if (!SetThreadContext(tHandle, ref context) || ResumeThread(tHandle) == 0xFFFFFFFF)
                throw new Win32Exception();

            for (int i = 0; i < 0x100; ++i)
            {
                System.Threading.Thread.Sleep(15);
                if (Read<uint>(retaddr) != 0xDEAD)
                    break;
            }

            // restore protection and original code
            Write(injAddress, oldCode);

            if (!FlushInstructionCache(Process.Handle, injAddress, bytes.Count))
                throw new Win32Exception();

            if (!VirtualProtectEx(Process.Handle, injAddress, bytes.Count, oldProtect, out oldProtect))
                throw new Win32Exception();

            Free(retaddr);
        }

        public void Call_x64(IntPtr injAddress, IntPtr callAddress, params IntPtr[] funcArgs)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Возвращает абсолютный аддресс в процессе.
        /// </summary>
        /// <param name="offset">Относительный аддресс.</param>
        /// <returns>абсолютный аддресс в процессе.</returns>
        public IntPtr Rebase(long offset)
        {
            return new IntPtr(offset + Process.MainModule.BaseAddress.ToInt64());
        }

        public IntPtr SendMessage(uint msg, IntPtr wParam, IntPtr lParam)
        {
            return SendMessage(Process.MainWindowHandle, msg, wParam, lParam);
        }

        /// <summary>
        /// Указывает что главное окно процесса находится на переднем плане.
        /// </summary>
        public bool IsFocusMainWindow
        {
            get
            {
                int id;
                GetWindowThreadProcessId(GetForegroundWindow(), out id);
                return id == Process.Id;
            }
        }

        public bool IsX64
        {
            get
            {
                bool wow64Process;
                IsWow64Process(Process.Handle, out wow64Process);
                return wow64Process;
            }
        }
    }

    public unsafe static class MarshalCache<T>
    {
        internal unsafe delegate void* GetUnsafePtrDelegate(ref T value);

        public static Type RealType { get; }
        public static int Size { get; }
        public static TypeCode TypeCode { get; }

        internal static GetUnsafePtrDelegate GetUnsafePtr { get; }

        static MarshalCache()
        {
            RealType = typeof(T);
            TypeCode = Type.GetTypeCode(RealType);

            if (RealType == typeof(bool))
            {
                Size = 1;
            }
            else if (RealType.IsEnum)
            {
                var underlying = RealType.GetEnumUnderlyingType();
                Size     = Marshal.SizeOf(underlying);
                RealType = underlying;
                TypeCode = Type.GetTypeCode(RealType);
            }
            else
            {
                Size = Marshal.SizeOf(RealType);
            }

            var name = $"GetPinnedPtr<{typeof(T).FullName}>";
            var method = new DynamicMethod(name, typeof(void*), new[] { typeof(T).MakeByRefType() }, typeof(MarshalCache<>).Module);
            var il = method.GetILGenerator();

            var opcode = IntPtr.Size == 4 ? OpCodes.Conv_U : OpCodes.Conv_U8;

            il.Emit(OpCodes.Ldarg_0);
            il.Emit(opcode);
            il.Emit(OpCodes.Ret);

            GetUnsafePtr = (GetUnsafePtrDelegate)method.CreateDelegate(typeof(GetUnsafePtrDelegate));
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

    /// <summary>
    /// Contains processor-specific register data.
    /// </summary>
    [StructLayout(LayoutKind.Explicit, Size = 716)]
    public struct WOW64_CONTEXT
    {
        /// <summary>
        /// Context flag.
        /// </summary>
        [FieldOffset(0x00)]
        public uint ContextFlags;

        /// <summary>
        /// Next instruction pointer.
        /// </summary>
        [FieldOffset(0xB8)]
        public uint Eip;
    };

    /// <summary>
    /// Contains processor-specific register data.
    /// </summary>
    [StructLayout(LayoutKind.Explicit, Size = 1232, Pack = 16)]
    public struct CONTEXT
    {
        /// <summary>
        /// Context flag.
        /// </summary>
        [FieldOffset(0x30)]
        public uint ContextFlags;

        /// <summary>
        /// Next instruction pointer.
        /// </summary>
        [FieldOffset(0xF8)]
        public ulong Rip;
    }

    #endregion
}