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
        private IntPtr mClientObjectManager;

        private byte[] CltObjMrgSeach          = new byte[] { 0xE8, 0x00, 0x00, 0x00, 0x00, 0x68, 0x00, 0x00 };
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
            if (this.mDetour == IntPtr.Zero || this.mClientObjectManager == IntPtr.Zero)
            {
                this.mDetour              = this.Memory.Find(this.OverwrittenBytesPattern);
                this.mClientObjectManager = this.Memory.Find(this.CltObjMrgSeach, "x???xx?x");

                if (this.mDetour == IntPtr.Zero)
                    throw new NullReferenceException("mDetour not found");

                if (this.mClientObjectManager == IntPtr.Zero)
                    throw new NullReferenceException("mClientObjectManager not found");
            }

            this.Memory.Suspend();

            this.Restore();

            this.mDetourPtr   = this.Memory.Alloc(0x256);
            this.mCodeCavePtr = this.Memory.Write<IntPtr>(IntPtr.Zero);

            #region ASM_x32

            var ASM_Code = new string[]
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
            this.Inject(ASM_Code, this.mDetourPtr + this.OverwrittenBytes.Length);
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

        public string LuaExecute(string sCommand, bool simple = true, string value = "nil")
        {
            if (!this.IsApplied)
                this.Apply();

            var commandAdr   = this.Memory.WriteCString(sCommand);
            var argumentsAdr = this.Memory.WriteCString(value);

            var resultAdr    = this.Memory.Alloc(0x0004);
            var injAddress   = this.Memory.Alloc(0x4096);

            #region ASM_x32

            string[] asmCode = new string[]
            {
                "mov   eax, " + commandAdr,
                "push  0",
                "push  eax",
                "push  eax",
                "mov   eax, " + this.Memory.Rebase(Offsets.Default.FrameScript_ExecuteBuffer),
                "call  eax",
                "add   esp, 0xC",
                "call  " + mClientObjectManager,//Settings.Default.ClntObjMgrGetActivePlayer,
                "test  eax, eax",
                "je    @out",
                "mov   ecx, eax",
                "push  -1",
                "mov   edx, " + argumentsAdr,
                "push  edx",
                "call  " + this.Memory.Rebase(Offsets.Default.FrameScript_GetLocalizedText),
                "mov   [" + resultAdr + "], eax",
                "@out:",
                "retn"
             };

            #endregion ASM_x32

            this.Inject(asmCode, injAddress);

            this.Memory.Write<IntPtr>(this.mCodeCavePtr, injAddress);

            int tickCount = Environment.TickCount;
            int res;
            while ((res = this.Memory.Read<int>(mCodeCavePtr)) != 0)
            {
                if ((tickCount + 0xBB8) < Environment.TickCount)
                {
                    Console.WriteLine("Out of time");
                    break;
                }
                Thread.Sleep(15);
            }

            var result = this.Memory.Read<IntPtr>(resultAdr);

            var resStr = "";
            if (result != IntPtr.Zero)
            {
                this.Memory.ReadString(result);
            }
            this.Memory.Free(commandAdr);
            this.Memory.Free(argumentsAdr);
            this.Memory.Free(resultAdr);
            this.Memory.Free(injAddress);

            if (simple)
                this.Restore();

            return resStr;
        }

        private void Inject(IEnumerable<string> ASM_Code, IntPtr address, bool randomize = true)
        {
            //if (randomize)
            //    ASM_Code = Extensions.RandomizeASM(ASM_Code);

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

                this.mClientObjectManager = IntPtr.Zero;
                this.mCodeCavePtr = IntPtr.Zero;
                this.mDetourPtr = IntPtr.Zero;
                this.mDetour = IntPtr.Zero;
            }
        }
    }
}
