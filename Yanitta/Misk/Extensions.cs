using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Xml;

namespace Yanitta
{
    public static class Extensions
    {
        public static void ForEach<T>(this IEnumerable<T> collection, Action<T> predicate)
        {
            if (collection == null)
                return;

            foreach (T element in collection)
                predicate(element);
        }

        public static void AppendFormatLine(this StringBuilder builder, string format, params object[] args)
        {
            if (args == null || args.Length == 0)
                builder.AppendLine(format);
            else
                builder.AppendFormat(CultureInfo.InvariantCulture, format, args).AppendLine();
        }

        public static string GetTrimValue(this XmlCDataSection cdataSection)
        {
            if (cdataSection == null || string.IsNullOrWhiteSpace(cdataSection.Value))
                return string.Empty;
            return cdataSection.Value.Trim();
        }

        public static XmlCDataSection CreateCDataSection(this string content)
        {
            if (string.IsNullOrWhiteSpace(content))
                return null;
            return new XmlDocument().CreateCDataSection("\n" + content + "\n");
        }

        public static void CopyProperies(object src, object dst)
        {
            if (src == null || dst == null)
                throw new ArgumentNullException();
            var typesrc = src.GetType();
            var typedst = dst.GetType();
            if (typesrc != typedst)
                throw new Exception();
            var flag = System.Reflection.BindingFlags.Public
                     | System.Reflection.BindingFlags.Instance
                     | System.Reflection.BindingFlags.DeclaredOnly
                ;

            foreach (var srcprop in typesrc.GetProperties(flag))
            {
                var dstprop = typedst.GetProperty(srcprop.Name);
                var val = srcprop.GetValue(src, null);
                dstprop.SetValue(dst, val, null);
            }
        }

        public static IEnumerable<string> RandomizeASM(IEnumerable<string> ASM_Code)
        {
            if (ASM_Code == null || ASM_Code.Count() == 0)
                throw new ArgumentNullException("ASM_Code");

            #region Random ASM Code

            var RandomASM = new[] {
                "nop",
                "mov eax, eax",
                "mov ecx, ecx",
                "mov ebp, ebp",
                "mov edx, edx",
                "mov ebx, ebx",
                "mov esp, esp",
                "mov esi, esi",
                "mov edi, edi",
                "push ebp|pop ebp",
                "push eax|pop eax",
                "push ecx|pop ecx",
                "push edx|pop edx",
                "push ebx|pop ebx",
                "push esp|pop esp",
                "push edi|pop edi",
                "xchg eax, eax",
                "xchg ebp, ebp",
                "xchg ecx, ecx",
                "xchg edx, edx",
                "xchg ebx, ebx",
                "xchg esp, esp",
                "xchg edi, edi",
                "xchg eax, ebp|xchg ebp, eax",
                "xchg ecx, ebp|xchg ebp, ecx",
                "xchg eax, edx|xchg edx, eax",
                "xchg eax, ebx|xchg ebx, eax",
                "xchg eax, edi|xchg edi, eax",
                "xchg edi, edx|xchg edx, edi",
                "xchg ecx, ebx|xchg ebx, ecx",
                "xchg ebp, edi|xchg edi, ebp"
            };

            #endregion Random ASM Code

            var random = new Random();
            var list = new List<string>();

            foreach (var asmLine in ASM_Code)
            {
                for (var i = 0; i < random.Next(1, 5); i++)
                {
                    var item = RandomASM[random.Next(0, RandomASM.Length - 1)];
                    var subitems = item.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries);

                    foreach (var randomitem in subitems)
                        list.Add(randomitem);
                }
                list.Add(asmLine);
            }
            return list;
        }
    }
}