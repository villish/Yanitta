using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using System.Text;

namespace MemoryModule
{
    public partial class ProcessMemory
    {
        [HandleProcessCorruptedStateExceptions]
        private unsafe T BaseRead<T>(uint address) where T : struct
        {
            if (address == 0)
                throw new InvalidOperationException("Cannot retrieve a value at address 0");

            var size = StructHelper<T>.Size;

            fixed (byte* pointer = new byte[size])
            {
                int bytesRead = 0;
                if (!Internals.ReadProcessMemory(this.Handle, address, pointer, size, out bytesRead))
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
                Internals.ReadProcessMemory(this.Handle, address, pointer, size, out bytesRead);

                if (size != bytesRead)
                    throw new Win32Exception();

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
    }
}
