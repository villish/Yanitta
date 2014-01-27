using MemoryModule;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Yanitta
{
    public class LuaHook : IDisposable
    {
        private bool IsApplied;
        private IntPtr mCodeCavePtr;
        private IntPtr mDetourPtr;
        private IntPtr mDetour;

        private byte[] OverwrittenBytes        = new byte[] { 0x55, 0x8B, 0xEC, 0x81, 0xEC, 0x94, 0x00, 0x00, 0x00 };
        private byte[] OverwrittenBytesPattern = new byte[] { 0x55, 0x8B, 0xEC, 0x81, 0xEC, 0x94, 0x00, 0x00, 0x00, 0x83, 0x7D, 0x14, 0x00, 0x56, 0x8B, 0x75 };

        private ProcessMemory Memory;

        public LuaHook(ProcessMemory memory)
        {
            if (memory == null || !memory.IsOpened)
                throw new ArgumentNullException();
            this.Memory = memory;
        }

        public void Apply()
        {
            if (this.mDetour == IntPtr.Zero)
            {
                this.mDetour = this.Memory.Find(this.OverwrittenBytesPattern);

                if (this.mDetour == IntPtr.Zero)
                    throw new NullReferenceException("mDetour not found");
            }

            this.Memory.Suspend();

            this.Restore();

            this.mDetourPtr   = this.Memory.Alloc(0x256);
            this.mCodeCavePtr = this.Memory.Write<IntPtr>(IntPtr.Zero);

            #region ASM_x32

            var asm = new string[]
            {
                "pushfd",
                "pushad",
                "mov  eax, [" + this.mCodeCavePtr + "]",
                "cmp  eax,   0x0",
                "je   @out",
                "call eax",
                "mov  eax, "  + this.mCodeCavePtr,
                "xor  edx,   edx",
                "mov  [eax], edx",
                "@out:",
                "popad",
                "popfd",
                "jmp " + (this.mDetour + this.OverwrittenBytes.Length)
            };

            #endregion ASM_x32

            this.Memory.WriteBytes(this.mDetourPtr, this.OverwrittenBytes);
            this.Inject(asm, this.mDetourPtr + this.OverwrittenBytes.Length);
            this.Inject(new[] { "jmp " + this.mDetourPtr }, this.mDetour, false);

            this.Memory.Resume();

            this.IsApplied = true;
        }

        public void Restore()
        {
            if (this.IsApplied)
            {
                this.Memory.WriteBytes(this.mDetour, this.OverwrittenBytes);
                this.IsApplied = false;
            }
        }

        public void LuaExecute(string sCommand, bool simple = true)
        {
            if (!this.IsApplied)
                this.Apply();

            var commandAdr = this.Memory.WriteCString(sCommand);
            var injAddress = this.Memory.Alloc(0x100);

            #region ASM_x32

            var asm = new string[] {
                "mov   eax, " + commandAdr,
                "push  0",
                "push  eax",
                "push  eax",
                "mov   eax, " + this.Memory.Rebase(Offsets.Default.FrameScript_ExecuteBuffer),
                "call  eax",
                "add   esp, 0xC",
                "retn"
             };

            #endregion ASM_x32

            this.Inject(asm, injAddress);

            this.Memory.Write<IntPtr>(this.mCodeCavePtr, injAddress);

            int tickCount = Environment.TickCount;
            while ((this.Memory.Read<int>(mCodeCavePtr)) != 0)
            {
                if ((tickCount + 0xBB8) < Environment.TickCount)
                {
                    Console.WriteLine("Out of time");
                    break;
                }
                Thread.Sleep(15);
            }

            this.Memory.Free(commandAdr);
            this.Memory.Free(injAddress);

            if (simple)
                this.Restore();
        }

        private void Inject(IEnumerable<string> ASM_Code, IntPtr address, bool randomize = true)
        {
            try
            {
                this.Memory.Inject(ASM_Code, address);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        ~LuaHook()
        {
            Dispose(false);
        }

        /// <summary>
        /// Closes an open wow memory.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (this.Memory != null)
            {
                if (this.Memory.IsOpened)
                    this.Restore();

                this.mCodeCavePtr = IntPtr.Zero;
                this.mDetourPtr = IntPtr.Zero;
                this.mDetour = IntPtr.Zero;
            }
        }
    }
}
