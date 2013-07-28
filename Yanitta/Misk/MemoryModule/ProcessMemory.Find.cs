using System;
using System.Runtime.InteropServices;

namespace MemoryModule
{
    public partial class ProcessMemory
    {
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

            Internals.ReadProcessMemory(this.Handle, this.BaseAddress, (void*)buffer, size, out bytesRead);

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
    }
}