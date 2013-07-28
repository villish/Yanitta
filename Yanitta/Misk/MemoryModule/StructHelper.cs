using System;
using System.Runtime.InteropServices;

namespace MemoryModule
{
    public static class StructHelper<T>
    {
        /// <summary>
        /// The size of the Type
        /// </summary>
        public static int Size;

        /// <summary>
        /// The real, underlying type.
        /// </summary>
        public static Type Type;

        /// <summary>
        /// The type code
        /// </summary>
        public static TypeCode TypeCode;

        static StructHelper()
        {
            TypeCode = Type.GetTypeCode(typeof(T));

            if (typeof(T) == typeof(bool))
            {
                Size = 1;
                Type = typeof(T);
            }
            else if (typeof(T).IsEnum)
            {
                var native = typeof(T).GetEnumUnderlyingType();

                Size     = Marshal.SizeOf(native);
                Type     = native;
                TypeCode = Type.GetTypeCode(native);
            }
            else
            {
                Size = Marshal.SizeOf(typeof(T));
                Type = typeof(T);
            }
        }
    }
}
