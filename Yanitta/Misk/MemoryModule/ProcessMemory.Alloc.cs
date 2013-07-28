using System;
using System.ComponentModel;

namespace MemoryModule
{
    public partial class ProcessMemory
    {
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

            uint address = Internals.VirtualAllocEx(this.Handle, IntPtr.Zero, size, AllocationType.Commit, MemoryProtection.ExecuteReadWrite);

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

            if (!Internals.VirtualFreeEx(this.Handle, address, 0, FreeType.Release))
                throw new Win32Exception();
        }
    }
}