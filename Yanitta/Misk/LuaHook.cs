using System;
using System.Collections.Generic;
using System.Threading;
using MemoryModule;
using MemoryModule.DirecX;

namespace Yanitta
{
    public class LuaHook : IDisposable
    {
        private bool IsApplied;
        private IntPtr mCodeCavePtr;
        private IntPtr mDetourPtr;
        private Dirext3D DirectX;

        private byte[] OverwrittenBytes;

        private ProcessMemory Memory;

        public LuaHook(ProcessMemory memory)
        {
            if (memory == null || !memory.IsOpened)
                throw new ArgumentNullException();
            this.Memory = memory;
        }

        public void Apply()
        {
            var directX  = new Dirext3D(this.Memory.Process);
            if (directX.HookPtr == IntPtr.Zero)
                throw new Exception("Can't find detour address");

            this.Memory.Suspend();

            this.OverwrittenBytes = this.Memory.ReadBytes(directX.HookPtr, 6);
            this.mDetourPtr       = this.Memory.Alloc(0x256);
            this.mCodeCavePtr     = this.Memory.Write<IntPtr>(IntPtr.Zero);

            #region ASM_x32

            var asm = new [] {
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
                "jmp " + (this.DirectX.HookPtr + this.OverwrittenBytes.Length)
            };

            #endregion ASM_x32

            this.Memory.WriteBytes(this.mDetourPtr, this.OverwrittenBytes);
            this.Inject(asm, this.mDetourPtr + this.OverwrittenBytes.Length);
            this.Inject(new[] { "jmp " + this.mDetourPtr }, this.DirectX.HookPtr, false);

            this.Memory.Resume();
            this.IsApplied = true;
        }

        public void Restore()
        {
            if (this.IsApplied)
            {
                this.Memory.WriteBytes(this.DirectX.HookPtr, this.OverwrittenBytes);
                this.IsApplied = false;
            }
        }

        public void LuaExecute(string sCommand, bool simple = true)
        {
            if (!this.IsApplied)
                this.Apply();

            var commandAdr = this.Memory.WriteCString(sCommand);
            var pathAdr    = this.Memory.WriteCString("Teldrasil.lua");
            var injAddress = this.Memory.Alloc(0x200);

            #region ASM_x32

            var asm = new List<string> {
                "push 0",
                "push " + pathAdr,
                "push " + commandAdr,
                "call " + this.Memory.Rebase(Offsets.Default.FrameScript_ExecuteBuffer),
                "add  esp, 0xC",
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
                var asm = randomize ? Extensions.RandomizeASM(ASM_Code) : ASM_Code;
                this.Memory.Inject(asm, address);
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
                this.DirectX = null;
            }
        }
    }
}
