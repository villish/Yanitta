using System;
using System.Text;
using System.Threading;
using MemoryLib.Asm;
using MemoryLib.Memory;
using Yanitta.Properties;

namespace Yanitta
{
    public class Injector : IDisposable
    {
        private Memory mMemory;

        private bool IsApplied = false;

        private IntPtr mCodeCavePtr;
        private IntPtr mDetourPtr;
        private IntPtr mDetour;
        private IntPtr mClientObjectManager;

        private byte[] OverwrittenBytes;

        public Injector(Memory memory)
        {
            this.mMemory              = memory;
            this.OverwrittenBytes     = Settings.Default.OverWritten.ToBytes();

            this.mDetour              = this.mMemory.Find(Settings.Default.OverWrittenPattern, Settings.Default.OverWrittenPatternMask);
            this.mClientObjectManager = this.mMemory.Find(Settings.Default.ClntObjMgrSearch, Settings.Default.ClntObjMgrSearchMask);

            if (this.mDetour == IntPtr.Zero || this.mClientObjectManager == IntPtr.Zero)
                throw new Exception(string.Format("Detour = 0x{0:X8}, ClientObjectManager = 0x{1:X8}", 
                    this.mDetour.ToInt32(), this.mClientObjectManager.ToInt32()));
        }

        ~Injector()
        {
            Dispose();
        }

        public void Apply()
        {
            this.mMemory.Suspend();

            this.Restore();

            this.mDetourPtr   = mMemory.WriteBytes(this.OverwrittenBytes);
            this.mCodeCavePtr = mMemory.Write<IntPtr>(IntPtr.Zero);
            
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
            #endregion

            //this.Inject(ASM_Code, this.mDetourPtr + this.OverwrittenBytes.Length, true);
            //this.Inject(new[] { "jmp " + this.mDetourPtr }, this.mDetour, false);
            this.mMemory.Inject(Extensions.RandomizeASM(ASM_Code), this.mDetourPtr + this.OverwrittenBytes.Length);
            this.mMemory.Inject("jmp " + this.mDetourPtr, this.mDetour);
            this.mMemory.Resume();

            this.IsApplied = true;
        }

        public void Restore()
        {
            if (this.IsApplied)
            {
                this.mMemory.WriteBytes(this.mDetour, this.OverwrittenBytes);
                this.IsApplied = false;
            }
        }

        public void Dispose()
        {
            if (mMemory != null)
            {
                if (mMemory.IsOpened)
                {
                    this.Restore();
                }
            }

            this.mCodeCavePtr = IntPtr.Zero;
            this.mDetourPtr = IntPtr.Zero;
            this.mDetour = IntPtr.Zero;
            this.mClientObjectManager = IntPtr.Zero;
            this.OverwrittenBytes = null;
        }

        public string LuaExecute(string sCommand, bool simple = true)
        {
            if (!this.IsApplied)
                this.Apply();

            string sArgument = "nil";

            var injAddress   = mMemory.Alloc(0x1000);
            var resultAdr    = mMemory.Alloc<IntPtr>();

            var commandAdr   = this.mMemory.WriteString(sCommand);
            var argumentsAdr = this.mMemory.WriteString(sArgument);

            #region ASM_x32
            string[] asmCode = new string[]
            {
                "mov   eax, " + commandAdr,
                "push  0",
                "push  eax",
                "push  eax",
                "mov   eax, " + ((long)this.mMemory.BaseAddress + Settings.Default.FrameScript_ExecuteBufferAddress),
                "call  eax",
                "add   esp, 0xC",
                "call  " + this.mClientObjectManager,
                "test  eax, eax",
                "je    @out",
                "mov   ecx, eax",
                "push  -1",
                "mov   edx, " + argumentsAdr,
                "push  edx",
                "call  " + ((long)this.mMemory.BaseAddress + Settings.Default.FrameScript_GetLocalizedTextAddress),
                "mov   [" + resultAdr + "], eax",
                "@out:",
                "retn"
             };
            #endregion

            //this.Inject(asmCode, injAddress, true);
            this.mMemory.Inject(asmCode, injAddress);
            this.mMemory.Write<IntPtr>(this.mCodeCavePtr, injAddress);

            int tickCount = Environment.TickCount;
            int res;
            while ((res = this.mMemory.Read<int>(mCodeCavePtr)) != 0)
            {
                if ((tickCount + 0xBB8) < Environment.TickCount)
                {
#if DEBUG
                    Console.WriteLine("Out of time");
                    break;
#else
                    throw new Exception("Out of time");
#endif
                }
                Thread.Sleep(10);
            }

            var result = mMemory.Read<IntPtr>(resultAdr);

            mMemory.Free(commandAdr);
            mMemory.Free(argumentsAdr);
            mMemory.Free(resultAdr);

            string message = string.Empty;
            if (result != IntPtr.Zero)
            {
                message = mMemory.ReadString(result);
#if DEBUG
                if (!string.IsNullOrWhiteSpace(message))
                    Console.WriteLine(message);
#endif
            }
            mMemory.Free(injAddress);

            if (simple)
                this.Restore();

            return message;
        }
    }
}