using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace MemoryModule
{
    public partial class ProcessMemory
    {
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
            FASM.Internals.fasm_Assemble(source, tbuffer, tbuffer.Length, passesLimit, 0);

            fixed (byte* pointer = tbuffer)
            {
                var fasm_result = *(FASM.FasmState*)pointer;

                if (fasm_result.condition == FASM.FasmCondition.ERROR)
                    throw new Exception(string.Format("Fasm Syntax Error: {0}, at line {1}",
                        fasm_result.error_code, fasm_result.error_data->line_number));

                if (fasm_result.condition != FASM.FasmCondition.OK && fasm_result.condition != FASM.FasmCondition.ERROR)
                    throw new Exception(string.Format("Fasm Error: {0}", fasm_result.condition));

                var buffer = new byte[fasm_result.output_lenght];
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
        public void Inject(IEnumerable<string> source, IntPtr address)
        {
            this.Inject(string.Join("\n", source), address);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="source"></param>
        /// <param name="address"></param>
        public void Inject(string source, IntPtr address)
        {
            var sb = new StringBuilder("use32")
                .AppendLine();
            sb.AppendFormat("org {0}", address.ToInt64())
                .AppendLine();
            sb.AppendLine(source);

            var src   = sb.ToString();
            var bytes = Assemble(src);

            WriteBytes(address, bytes);
        }
    }
}
