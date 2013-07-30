using MemoryModule;
using System;
using System.Collections.Generic;

namespace Yanitta.Hantchk
{
    public static class ObjectManager
    {
        internal static IntPtr CurentObjectManagerAddr  = IntPtr.Zero;//find
        internal static IntPtr MouseOverGUID            = IntPtr.Zero;//find

        internal const int Addr_Guid                    = 0x00;
        internal const int Addr_Data                    = 0x08;
        internal const int Addr_Type                    = 0x0C;
        internal const int Addr_EntryID                 = 0x14;
        internal const int Addr_CreatedBy               = 0x20;
        internal const int Addr_VisibleGuid             = 0x28;
        internal const int Addr_AnimationState          = 0xC4;
        internal const int Addr_LocalGuid               = 0xE0;
        internal const int Addr_FirstObject             = 0xCC;
        internal const int Addr_NextObject              = 0x34;

        public static readonly object ObjPulse = new object();
        public static List<WoWObject> Objects = new List<WoWObject>();

        public static ProcessMemory Memory { get; set; }

        public static bool Initialized
        {
            get { return Memory != null && Memory.IsOpened; }
        }

        public static IntPtr CurrentManager { get; set; }

        public static ulong PlayerGuid { get; set; }

        internal static void Initialize(WowMemory wowMem)
        {
            Memory = wowMem.Memory;

            if (Initialized)
            {
                MouseOverGUID = Memory.Find(new byte[] {
                        0x56, 0x57, 0x33, 0xF6, 0x56, 0x6A, 0x01, 0x53, 0xE8, 0x00, 0x00, 0x00, 0x00,
                        0x8B, 0xF8, 0x68, 0x00, 0x00, 0x00, 0x00, 0x57, 0xE8, 0x00, 0x00, 0x00, 0x00,
                        0x83, 0xC4, 0x14, 0x85, 0xC0, 0x75, 0x00, 0x8B, 0x35, 0x00, 0x00, 0x00, 0x00
                    }, "xxxxxxxxx????xxx????xx????xxxxxx?xx????") + 0x23;

                CurentObjectManagerAddr = Memory.Find(new byte[] {
                        0x55, 0x8B, 0xEC, 0xA1, 0x00, 0x00, 0x00, 0x00, 0x8B, 0x88, 0xCC, 0x00,
                        0x00, 0x00, 0x56, 0x57, 0x33, 0xFF, 0x47, 0xF6, 0xC1, 0x01, 0x75, 0x00
                    }, "xxxx????xxxxxxxxxxxxxxx?") + 0x04;
            }
        }

        public static void Pulse()
        {
            if (!Initialized)
                return;

            CurrentManager = Memory.Read<IntPtr>(CurentObjectManagerAddr);
            PlayerGuid = Memory.Read<ulong>(CurrentManager + Addr_LocalGuid);

            lock (ObjPulse)
            {
                Objects.Clear();

                var baseAddress = Memory.Read<IntPtr>(CurrentManager + Addr_FirstObject);
                var currentObject = new WoWObject(baseAddress);
                baseAddress = currentObject.BaseAddress;

                while ((((int)baseAddress & 1) == 0) && baseAddress != IntPtr.Zero)
                {
                    // gameobject
                    if (currentObject.Type == 5)
                    {
                        var gameobject = new WoWGameObject(baseAddress);
                        Objects.Add(gameobject);
#if DEBUG
                        Console.WriteLine("Find GameObject 0x{0:X8}", gameobject.Guid);
#endif
                    }

                    currentObject.BaseAddress = Memory.Read<IntPtr>(baseAddress + Addr_NextObject);
                }
            }
        }
    }
}