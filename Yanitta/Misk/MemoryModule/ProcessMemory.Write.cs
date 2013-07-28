using System;
using System.ComponentModel;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using System.Text;

namespace MemoryModule
{
    public partial class ProcessMemory
    {
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
        [HandleProcessCorruptedStateExceptions]
        public unsafe void Write<T>(uint address, T data) where T : struct
        {
            if (address == 0)
                throw new ArgumentNullException("address");

            if (this.Process == null)
                throw new Exception("Process exists");

            if (!this.IsOpened)
                throw new Exception("Can't open process");


            var size = StructHelper<T>.Size;
            int writenBytes = 0;
            fixed (byte* pointer = new byte[size])
            {
                if (StructHelper<T>.TypeCode == TypeCode.Object)
                {
                    Marshal.StructureToPtr(data, new IntPtr(pointer), true);
                    Internals.WriteProcessMemory(this.Handle, address, pointer, size, out writenBytes);
                    Marshal.DestroyStructure(new IntPtr(pointer), typeof(T));

                    if (writenBytes == 0)
                        throw new Win32Exception();
                    return;
                }
                switch (StructHelper<T>.TypeCode)
                {
                    case TypeCode.Boolean: *(byte*)  pointer = (byte)((bool)(object)data ? 1 : 0); break;
                    case TypeCode.Char:    *(char*)  pointer = (char)  (object)data; break;
                    case TypeCode.SByte:   *(sbyte*) pointer = (sbyte) (object)data; break;
                    case TypeCode.Byte:    *(byte*)  pointer = (byte)  (object)data; break;
                    case TypeCode.Int16:   *(short*) pointer = (short) (object)data; break;
                    case TypeCode.UInt16:  *(ushort*)pointer = (ushort)(object)data; break;
                    case TypeCode.Int32:   *(int*)   pointer = (int)   (object)data; break;
                    case TypeCode.UInt32:  *(uint*)  pointer = (uint)  (object)data; break;
                    case TypeCode.Int64:   *(long*)  pointer = (long)  (object)data; break;
                    case TypeCode.UInt64:  *(ulong*) pointer = (ulong) (object)data; break;
                    case TypeCode.Single:  *(float*) pointer = (float) (object)data; break;
                    case TypeCode.Double:  *(double*)pointer = (double)(object)data; break;
                    default: throw new ArgumentOutOfRangeException();
                }

                Internals.WriteProcessMemory(this.Handle, address, pointer, size, out writenBytes);
                if (writenBytes == 0)
                    throw new Win32Exception();
            }
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
            Internals.WriteProcessMemory(this.Handle, address, data, data.Length, out writenBytes);

            if (writenBytes == 0)
                throw new Win32Exception();
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
    }
}
