using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using System.Text;

namespace MemoryModule
{
    /// <summary>
    ///
    /// </summary>
    public class ProcessMemory : IDisposable
    {
        #region Constructors/Destructor

        /// <summary>
        ///
        /// </summary>
        public ProcessMemory()
        {
            this.Handle = IntPtr.Zero;
        }

        static ProcessMemory()
        {
            var raw_ver = fasm_GetVersion();
            FasmVersion = string.Format("{0}.{1}", raw_ver & 0xFFFF, raw_ver >> 16 & 0xFFFF);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="process"></param>
        /// <param name="DebugPrivileges"></param>
        /// <param name="UseBaseAddress"></param>
        public ProcessMemory(Process process)
        {
            this.Process = process;
            this.Open();
        }

        ~ProcessMemory()
        {
            Dispose();
        }

        #endregion Constructors/Destructor

        #region Functions

        /// <summary>
        /// Opens an existing local process object.
        /// </summary>
        public void Open()
        {
            this.Open(ProcessAccess.All);
        }

        /// <summary>
        /// Opens an existing local process object.
        /// </summary>
        /// <param name="rights">
        /// The access to the process object.
        /// This access right is checked against the security descriptor for the process.
        /// This parameter can be one or more of the process access rights.
        /// </param>
        public void Open(ProcessAccess rights)
        {
            if (this.IsOpened)
                return;

            if (this.Process == null)
                throw new Exception("Process exists");

            if (this.MainThread.Id == 0)
                throw new Exception("main thread = 0");

            Process.EnterDebugMode();

            this.Process.EnableRaisingEvents = true;

            this.Handle = OpenProcess(rights, false, Process.Id);

            if (this.Handle == IntPtr.Zero)
                throw new Win32Exception(Marshal.GetLastWin32Error());

            this.ThreadHandle = OpenThread(ThreadAccess.All, false, this.MainThread.Id);

            if (this.ThreadHandle == IntPtr.Zero)
                throw new Win32Exception(Marshal.GetLastWin32Error());

            this.IsOpened = true;
        }

        /// <summary>
        /// Reserves or commits a region of memory within the virtual address space of a specified process.
        /// The function initializes the memory it allocates to zero, unless MEM_RESET is used.
        /// </summary>
        /// <param name="size">
        /// The size of the region of memory to allocate, in bytes.
        /// If lpAddress is NULL, the function rounds dwSize up to the next page boundary.
        /// If lpAddress is not NULL, the function allocates all pages that contain one or more bytes in the range from lpAddress to lpAddress+dwSize.
        /// This means, for example, that a 2-byte range that straddles a page boundary causes the function to allocate both pages.
        /// </param>
        /// <returns>
        /// If the function succeeds, the return value is the base address of the allocated region of pages.
        /// If the function fails, the return value is NULL.
        /// To get extended error information, call GetLastError.
        /// </returns>
        public uint Alloc(int size)
        {
            if (size <= 0)
                throw new ArgumentNullException("size");

            if (this.Process == null)
                throw new Exception("Process exists");

            if (!this.IsOpened)
                throw new Exception("Can't open process");

            uint address = VirtualAllocEx(this.Handle, IntPtr.Zero, size, AllocationType.Commit, MemoryProtection.ExecuteReadWrite);

            if (address == 0)
                throw new Win32Exception();

            return address;
        }

        /// <summary>
        /// Releases, decommits, or releases and decommits a region of memory within the virtual address space of a specified process.
        /// </summary>
        /// <param name="address">
        /// A pointer to the starting address of the region of memory to be freed.
        /// If the dwFreeType parameter is MEM_RELEASE,
        /// lpAddress must be the base address returned by the VirtualAllocEx function when the region is reserved.
        /// </param>
        public void Free(uint address)
        {
            if (address == 0)
                throw new ArgumentNullException("address");

            if (this.Process == null)
                throw new Exception("Process exists");

            if (!VirtualFreeEx(this.Handle, address, 0, FreeType.Release))
                throw new Win32Exception();
        }

        #region Read

        [HandleProcessCorruptedStateExceptions]
        private unsafe T BaseRead<T>(uint address) where T : struct
        {
            if (address == 0)
                throw new InvalidOperationException("Cannot retrieve a value at address 0");

            var size = StructHelper<T>.Size;

            fixed (byte* pointer = new byte[size])
            {
                int bytesRead = 0;
                if (!ReadProcessMemory(this.Handle, address, pointer, size, out bytesRead))
                    throw new AccessViolationException(
                        string.Format("Could not read from 0x{0:X8} [{1}]!",
                            address, Marshal.GetLastWin32Error())
                        );

                switch (StructHelper<T>.TypeCode)
                {
                    case TypeCode.Boolean: return (T)(object)(*(byte*) pointer != 0);
                    case TypeCode.Char:    return (T)(object)*(char*)  pointer;
                    case TypeCode.SByte:   return (T)(object)*(sbyte*) pointer;
                    case TypeCode.Byte:    return (T)(object)*(byte*)  pointer;
                    case TypeCode.Int16:   return (T)(object)*(short*) pointer;
                    case TypeCode.UInt16:  return (T)(object)*(ushort*)pointer;
                    case TypeCode.Int32:   return (T)(object)*(int*)   pointer;
                    case TypeCode.UInt32:  return (T)(object)*(uint*)  pointer;
                    case TypeCode.Int64:   return (T)(object)*(long*)  pointer;
                    case TypeCode.UInt64:  return (T)(object)*(ulong*) pointer;
                    case TypeCode.Single:  return (T)(object)*(float*) pointer;
                    case TypeCode.Double:  return (T)(object)*(double*)pointer;

                    case TypeCode.Object:
                        return (T)Marshal.PtrToStructure(new IntPtr((int)address), typeof(T));

                    default: throw new ArgumentOutOfRangeException();
                }
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="address"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public unsafe byte[] ReadBytes(uint address, int size, bool isRelative = false)
        {
            if (address == 0)
                throw new ArgumentNullException("address");

            if (this.Process == null)
                throw new Exception("Process exists");

            if (!this.IsOpened)
                throw new Exception("Can't open process");

            if (isRelative)
                address = Rebase(address);

            var bytesRead = 0;

            var buffer = new byte[size];
            fixed (byte* pointer = buffer)
            {
                ReadProcessMemory(this.Handle, address, pointer, size, out bytesRead);

                if (size != bytesRead)
                    throw new Win32Exception(Marshal.GetLastWin32Error());

                return buffer;
            }
        }

        /// <summary>
        /// Reads data from an area of memory in a specified process.
        /// The entire area to be read must be accessible or the operation fails.
        /// </summary>
        /// <typeparam name="T">Data type, only struct</typeparam>
        /// <param name="address">
        /// A pointer to the base address in the specified process from which to read.
        /// Before any data transfer occurs,
        /// the system verifies that all data in the base address and memory of the specified size is accessible for read access,
        /// and if it is not accessible the function fails.</param>
        /// <returns>If the function succeeds, the return value is nonzero.
        /// If the function fails, the return value is 0 (zero). To get extended error information, call GetLastError.
        /// The function fails if the requested read operation crosses into an area of the process that is inaccessible.
        /// </returns>
        public unsafe T Read<T>(uint address, bool isRelative = false) where T : struct
        {
            if (address == 0)
                throw new ArgumentNullException("address");

            if (this.Process == null)
                throw new Exception("Process exists");

            if (!this.IsOpened)
                throw new Exception("Can't open process");

            if (isRelative)
                address = Rebase(address);

            return BaseRead<T>(address);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public string ReadString(uint address, bool isRelative = false)
        {
            return ReadString(address, Encoding.UTF8, isRelative);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="address"></param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        public string ReadString(uint address, Encoding encoding, bool isRelative = false)
        {
            if (address == 0)
                throw new ArgumentNullException("address");

            if (this.Process == null)
                throw new Exception("Process exists");

            if (!this.IsOpened)
                throw new Exception("Can't open process");

            if (isRelative)
                address = Rebase(address);

            byte b;
            var list = new List<byte>();
            while ((b = this.BaseRead<byte>(address++)) != 0)
            {
                list.Add(b);
            }

            return encoding.GetString(list.ToArray());
        }

        #endregion Read

        #region Write

        public unsafe uint Write<T>(T data) where T : struct
        {
            var size = Marshal.SizeOf(typeof(T));
            var addr = this.Alloc(size);
            this.Write<T>(addr, data);
            return addr;
        }

        /// <summary>
        /// Writes data to an area of memory in a specified process.
        /// The entire area to be written to must be accessible or the operation fails.
        /// </summary>
        /// <typeparam name="T">
        /// Data type, only struct
        /// </typeparam>
        /// <param name="address">
        /// A pointer to the base address in the specified process to which data is written.
        /// Before data transfer occurs,
        /// the system verifies that all data in the base address and memory of the specified size is accessible for write access,
        /// and if it is not accessible, the function fails.
        /// </param>
        /// <param name="data">
        /// Data to be written in the address space of the specified process.
        /// </param>
        public unsafe void Write<T>(uint address, T data) where T : struct
        {
            if (address == 0)
                throw new ArgumentNullException("address");

            if (this.Process == null)
                throw new Exception("Process exists");

            if (!this.IsOpened)
                throw new Exception("Can't open process");

            var writenBytes = 0;
            var size = Marshal.SizeOf(typeof(T));
            var buffer = Marshal.AllocHGlobal(size);

            Marshal.StructureToPtr(data, buffer, true);
            WriteProcessMemory(this.Handle, address, buffer, size, out writenBytes);
            Marshal.DestroyStructure(buffer, typeof(T));
            Marshal.FreeHGlobal(buffer);

            if (writenBytes == 0)
                throw new Win32Exception(Marshal.GetLastWin32Error());
        }

        public unsafe uint WriteBytes(byte[] data)
        {
            var addr = this.Alloc(data.Length);
            this.WriteBytes(addr, data);
            return addr;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="address"></param>
        /// <param name="data"></param>
        public unsafe void WriteBytes(uint address, byte[] data)
        {
            if (address == 0)
                throw new ArgumentNullException("address");

            if (this.Process == null)
                throw new Exception("Process exists");

            if (!this.IsOpened)
                throw new Exception("Can't open process");

            var writenBytes = 0;
            WriteProcessMemory(this.Handle, address, data, data.Length, out writenBytes);

            if (writenBytes == 0)
                throw new Win32Exception(Marshal.GetLastWin32Error());
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="address"></param>
        /// <param name="format"></param>
        /// <param name="args"></param>
        public void WriteString(uint address, string format, params object[] args)
        {
            WriteString(address, Encoding.UTF8, format, args);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="address"></param>
        /// <param name="encoding"></param>
        /// <param name="format"></param>
        /// <param name="args"></param>
        public void WriteString(uint address, Encoding encoding, string format, params object[] args)
        {
            var str = string.Format(format, args);
            var bytes = encoding.GetBytes(str);
            WriteBytes(address, bytes);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="address"></param>
        /// <param name="format"></param>
        /// <param name="args"></param>
        public void WriteCString(uint address, string format, params object[] args)
        {
            WriteString(address, Encoding.UTF8, format, args);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="address"></param>
        /// <param name="encoding"></param>
        /// <param name="format"></param>
        /// <param name="args"></param>
        public void WriteCString(uint address, Encoding encoding, string format, params object[] args)
        {
            var str = string.Format(format, args);
            var bytes = encoding.GetBytes(str + '\0');
            WriteBytes(address, bytes);
        }

        #endregion Write

        #region Inject

        /// <summary>
        ///
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public unsafe byte[] Assemble(string source)
        {
            var passesLimit = 0x100;
#if DEBUG
            Console.WriteLine("AsmSource:\n{0}", source);
#endif

            var tbuffer = new byte[source.Length * passesLimit];
            fasm_Assemble(source, tbuffer, tbuffer.Length, passesLimit, 0);

            fixed (byte* pointer = tbuffer)
            {
                var fasm_result = *(FasmState*)pointer;

                if (fasm_result.condition == FasmCondition.ERROR)
                    throw new Exception(string.Format("Fasm Syntax Error: {0}, at line {1}",
                        fasm_result.error_code, fasm_result.error_data->line_number));

                if (fasm_result.condition != FasmCondition.OK && fasm_result.condition != FasmCondition.ERROR)
                    throw new Exception(string.Format("Fasm Error: {0}", fasm_result.condition));

                byte[] buffer = new byte[fasm_result.output_lenght];
                Marshal.Copy(new IntPtr(fasm_result.output_data), buffer, 0, fasm_result.output_lenght);
#if DEBUG
                Console.WriteLine("ByteCode:\n{0}", string.Join(" ", buffer.Select(n => n.ToString("X2"))));
#endif
                return buffer;
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="source"></param>
        /// <param name="address"></param>
        public void Inject(IEnumerable<string> source, uint address)
        {
            this.Inject(string.Join("\n", source), address);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="source"></param>
        /// <param name="address"></param>
        public void Inject(string source, uint address)
        {
            var sb = new StringBuilder("use32")
                .AppendLine();
            sb.AppendFormat("org 0x{0:X8}", address)
                .AppendLine();
            sb.AppendLine(source);

            var src = sb.ToString();
            var bytes = Assemble(src);

            this.WriteBytes(address, bytes);
        }

        #endregion Inject

        /// <summary>
        /// Finds a given pattern in an array of bytes.
        /// </summary>
        /// <param name="pattern">
        /// A byte-array representing the pattern to be found.
        /// </param>
        /// <param name="mask">
        /// A string of 'x' (match), '!' (not-match), or '?' (wildcard).
        /// </param>
        /// <returns>
        /// Returns 0 on failure, or the address of the start of the pattern on success.
        /// </returns>
        public unsafe uint Find(byte[] pattern, string mask = "")
        {
            if (this.Process == null)
                throw new Exception("Process exists");

            if (!this.IsOpened)
                throw new Exception("Can't open process");

            if (mask == "")
            {
                mask = new string('x', pattern.Length);
            }
            else
            {
                if (pattern.Length != mask.Length)
                    throw new ArgumentException("Pattern and Mask lengths must be the same.");

                foreach (char c in mask)// check mask
                {
                    if (c != 'x' && c != '!' && c != '?')
                    {
                        throw new ArgumentException("Bad sumbol: '" + c + "'", "mask");
                    }
                }
            }

            //!+Переписать с использованием фиксированного буфера
            var bytesRead = 0;
            var found = false;
            var size = Process.MainModule.ModuleMemorySize;
            var dataLenght = size - pattern.Length;

            var buffer = Marshal.AllocHGlobal(size);

            ReadProcessMemory(this.Handle, this.BaseAddress, buffer, size, out bytesRead);

            if (bytesRead != size)
                throw new Exception("ModuleMemorySize and BytesRead lengths must be the same.");

            var offset = 0u;

            for (; offset < dataLenght; offset++)
            {
                found = true;
                for (int index = 0; index < pattern.Length; index++)
                {
                    byte element = ((byte*)buffer)[offset + index];

                    if ((mask[index] == 'x' && pattern[index] != element)
                     || (mask[index] == '!' && pattern[index] == element))
                    {
                        found = false;
                        break;
                    }
                }

                if (found)
                    break;
            }

            Marshal.FreeHGlobal(buffer);
            return found ? offset + BaseAddress : 0u;
        }

        /// <summary>
        /// Suspends the specified thread.
        /// </summary>
        public void Suspend()
        {
            if (this.MainThread.Id == 0)
                throw new Exception("main thread = 0");

            if (this.ThreadHandle != IntPtr.Zero)
                SuspendThread(this.ThreadHandle);
        }

        /// <summary>
        /// Decrements a thread's suspend count.
        /// When the suspend count is decremented to zero, the execution of the thread is resumed.
        /// </summary>
        public void Resume()
        {
            if (this.MainThread.Id == 0)
                throw new Exception("main thread = 0");

            if (this.ThreadHandle != IntPtr.Zero)
                ResumeThread(this.ThreadHandle);
        }

        public uint Rebase(long address)
        {
            return (uint)(this.BaseAddress + address);
        }

        /// <summary>
        /// Closes an open process handle and thread.
        /// </summary>
        public void Dispose()
        {
            if (IsOpened)
            {
                if (this.ThreadHandle != IntPtr.Zero)
                    CloseHandle(this.ThreadHandle);

                if (this.Handle != IntPtr.Zero)
                    CloseHandle(this.Handle);

                Process.LeaveDebugMode();
            }

            this.Handle = IntPtr.Zero;
            this.ThreadHandle = IntPtr.Zero;
            this.IsOpened = false;
        }

        #endregion Functions

        #region Properties

        /// <summary>
        /// Get the cutrrent process.
        /// </summary>
        public Process Process { get; set; }

        /// <summary>
        /// Gets the native handle of the associated process.
        /// </summary>
        public IntPtr Handle { get; private set; }

        /// <summary>
        /// Gets the native handle of the associated process thread.
        /// </summary>
        public IntPtr ThreadHandle { get; private set; }

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
                var foregroundWindow = GetForegroundWindow();
                GetWindowThreadProcessId(foregroundWindow, out lProcessId);
                return this.Process.Id == lProcessId;
            }
        }

        /// <summary>
        ///
        /// </summary>
        public bool IsOpened { get; private set; }

        public static string FasmVersion { get; private set; }

        #endregion Properties

        #region DllImport

        [DllImport("Kernel32.dll", EntryPoint = "RtlMoveMemory", SetLastError = false)]
        private unsafe static extern void MoveMemory(void* dest, void* src, int size);

        [DllImport("kernel32", EntryPoint = "OpenProcess", SetLastError = true)]
        private static extern IntPtr OpenProcess(ProcessAccess dwDesiredAccess, [MarshalAs(UnmanagedType.Bool)] bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32", EntryPoint = "OpenThread", SetLastError = true)]
        private static extern IntPtr OpenThread(ThreadAccess DesiredAccess, [MarshalAs(UnmanagedType.Bool)] bool bInheritHandle, int dwThreadId);

        [DllImport("kernel32", EntryPoint = "SuspendThread")]
        private static extern uint SuspendThread(IntPtr hThread);

        [DllImport("kernel32", EntryPoint = "ResumeThread")]
        private static extern uint ResumeThread(IntPtr hThread);

        [DllImport("kernel32", EntryPoint = "ReadProcessMemory")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool ReadProcessMemory(IntPtr hProcess, uint dwAddress, IntPtr lpBuffer, int nSize, out int lpBytesRead);

        [DllImport("kernel32", EntryPoint = "ReadProcessMemory")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private unsafe static extern bool ReadProcessMemory(IntPtr hProcess, uint dwAddress, void* lpBuffer, int nSize, out int lpBytesRead);

        [DllImport("kernel32", EntryPoint = "WriteProcessMemory")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool WriteProcessMemory(IntPtr hProcess, uint dwAddress, IntPtr lpBuffer, int nSize, out int iBytesWritten);

        [DllImport("kernel32", EntryPoint = "WriteProcessMemory")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool WriteProcessMemory(IntPtr hProcess, uint dwAddress, byte[] lpBuffer, int nSize, out int iBytesWritten);

        [DllImport("kernel32", EntryPoint = "VirtualAllocEx", SetLastError = true, ExactSpelling = true)]
        private static extern uint VirtualAllocEx(IntPtr hProcess, IntPtr lpAddress, int dwSize, AllocationType flAllocationType, MemoryProtection flProtect);

        [DllImport("kernel32", EntryPoint = "VirtualFreeEx", SetLastError = true, ExactSpelling = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool VirtualFreeEx(IntPtr hProcess, uint lpAddress, int dwSize, FreeType dwFreeType);

        [DllImport("kernel32", EntryPoint = "CloseHandle", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool CloseHandle(IntPtr hObject);

        [DllImport("user32.dll")]
        internal static extern IntPtr GetForegroundWindow();

        [DllImport("user32", SetLastError = true)]
        internal static extern int GetWindowThreadProcessId([In]IntPtr hwnd, [Out]out int lProcessId);

        [DllImport("FASM.DLL", SetLastError = true)]
        internal static extern int fasm_GetVersion();

        [DllImport("FASM.DLL", SetLastError = true)]
        internal static extern unsafe int fasm_Assemble([In]string lpSource, byte[] lpMemory, [In]int cbMemorySize, [In]int nPassesLimit, [In]int hDisplayPipe);

        #endregion DllImport
    }

    public static class StructHelper<T>
    {
        /// <summary>
        /// The size of the Type
        /// </summary>
        public static int Size;

        /// <summary>
        /// The real, underlying type.
        /// </summary>
        public static Type Type;

        /// <summary>
        /// The type code
        /// </summary>
        public static TypeCode TypeCode;

        static StructHelper()
        {
            TypeCode = Type.GetTypeCode(typeof(T));

            if (typeof(T) == typeof(bool))
            {
                Size = 1;
                Type = typeof(T);
            }
            else if (typeof(T).IsEnum)
            {
                var native = typeof(T).GetEnumUnderlyingType();

                Size       = Marshal.SizeOf(native);
                Type       = native;
                TypeCode   = Type.GetTypeCode(native);
            }
            else
            {
                Size = Marshal.SizeOf(typeof(T));
                Type = typeof(T);
            }
        }
    }

    #region Enums

    [Flags]
    public enum ProcessAccess : uint
    {
        Terminate = 0x00000001,
        CreateThread = 0x00000002,
        VMOperation = 0x00000008,
        VMRead = 0x00000010,
        VMWrite = 0x00000020,
        DupHandle = 0x00000040,
        SetInformation = 0x00000200,
        QueryInformation = 0x00000400,
        Synchronize = 0x00100000,
        All = 0x001F0FFF,
    }

    [Flags]
    public enum ThreadAccess : uint
    {
        Terminate = 0x00001,
        SuspendResume = 0x00002,
        GetContext = 0x00008,
        SetContext = 0x00010,
        SetInformation = 0x00020,
        QueryInformation = 0x00040,
        SetThreadToken = 0x00080,
        Impersonate = 0x00100,
        DirectImpersonation = 0x00200,
        All = 0x1F03FF
    }

    [Flags]
    public enum AllocationType : uint
    {
        Commit = 0x00001000,
        Reserve = 0x00002000,
        Decommit = 0x00004000,
        Release = 0x00008000,
        Reset = 0x00080000,
        TopDown = 0x00100000,
        WriteWatch = 0x00200000,
        Physical = 0x00400000,
        LargePages = 0x20000000,
    }

    [Flags]
    public enum MemoryProtection : uint
    {
        NoAccess = 0x001,
        ReadOnly = 0x002,
        ReadWrite = 0x004,
        WriteCopy = 0x008,
        Execute = 0x010,
        ExecuteRead = 0x020,
        ExecuteReadWrite = 0x040,
        ExecuteWriteCopy = 0x080,
        GuardModifierflag = 0x100,
        NoCacheModifierflag = 0x200,
        WriteCombineModifierflag = 0x400,
    }

    [Flags]
    public enum FreeType : uint
    {
        Decommit = 0x4000,
        Release = 0x8000,
    }

    #endregion Enums

    #region fasm struct

    // General errors and conditions
    internal enum FasmCondition : int
    {
        OK = 00, //	; FASM_STATE points to output
        WORKING = 01, //
        ERROR = 02, //	; FASM_STATE contains error code
        INVALID_PARAMETER = -1, //
        OUT_OF_MEMORY = -2, //
        STACK_OVERFLOW = -3, //
        SOURCE_NOT_FOUND = -4, //
        UNEXPECTED_END_OF_SOURCE = -5, //
        CANNOT_GENERATE_CODE = -6, //
        FORMAT_LIMITATIONS_EXCEDDED = -7, //
        WRITE_FAILED = -8, //
    }

    // Error codes for FASM_ERROR condition
    internal enum FasmError : int
    {
        FILE_NOT_FOUND = -101,
        ERROR_READING_FILE = -102,
        INVALID_FILE_FORMAT = -103,
        INVALID_MACRO_ARGUMENTS = -104,
        INCOMPLETE_MACRO = -105,
        UNEXPECTED_CHARACTERS = -106,
        INVALID_ARGUMENT = -107,
        ILLEGAL_INSTRUCTION = -108,
        INVALID_OPERAND = -109,
        INVALID_OPERAND_SIZE = -110,
        OPERAND_SIZE_NOT_SPECIFIED = -111,
        OPERAND_SIZES_DO_NOT_MATCH = -112,
        INVALID_ADDRESS_SIZE = -113,
        ADDRESS_SIZES_DO_NOT_AGREE = -114,
        DISALLOWED_COMBINATION_OF_REGISTERS = -115,
        LONG_IMMEDIATE_NOT_ENCODABLE = -116,
        RELATIVE_JUMP_OUT_OF_RANGE = -117,
        INVALID_EXPRESSION = -118,
        INVALID_ADDRESS = -119,
        INVALID_VALUE = -120,
        VALUE_OUT_OF_RANGE = -121,
        UNDEFINED_SYMBOL = -122,
        INVALID_USE_OF_SYMBOL = -123,
        NAME_TOO_LONG = -124,
        INVALID_NAME = -125,
        RESERVED_WORD_USED_AS_SYMBOL = -126,
        SYMBOL_ALREADY_DEFINED = -127,
        MISSING_END_QUOTE = -128,
        MISSING_END_DIRECTIVE = -129,
        UNEXPECTED_INSTRUCTION = -130,
        EXTRA_CHARACTERS_ON_LINE = -131,
        SECTION_NOT_ALIGNED_ENOUGH = -132,
        SETTING_ALREADY_SPECIFIED = -133,
        DATA_ALREADY_DEFINED = -134,
        TOO_MANY_REPEATS = -135,
        SYMBOL_OUT_OF_SCOPE = -136,
        USER_ERROR = -140,
        ASSERTION_FAILED = -141,
    }

    [StructLayout(LayoutKind.Explicit)]
    internal unsafe struct FasmLineHeader
    {
        [FieldOffset(0)]
        public byte* file_path;

        [FieldOffset(4)]
        public int line_number;

        [FieldOffset(8)]
        public int file_offset;

        [FieldOffset(8)]
        public int macro_offset_line;

        [FieldOffset(12)]
        public FasmLineHeader* macro_line;
    };

    [StructLayout(LayoutKind.Explicit)]
    internal unsafe struct FasmState
    {
        [FieldOffset(0)]
        public FasmCondition condition;

        [FieldOffset(4)]
        public FasmError error_code;

        [FieldOffset(4)]
        public int output_lenght;

        [FieldOffset(8)]
        public byte* output_data;

        [FieldOffset(8)]
        public FasmLineHeader* error_data;
    };

    #endregion fasm struct
}