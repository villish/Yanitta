using System;
namespace Yanitta.Hantchk
{
    public class WoWObject
    {
        public WoWObject(IntPtr baseAddress)
        {
            BaseAddress = baseAddress;
        }

        public IntPtr BaseAddress { get; set; }

        public virtual ulong Guid
        {
            get { return ObjectManager.Memory.Read<ulong>(BaseAddress + ObjectManager.Addr_VisibleGuid); }
        }

        public int Type
        {
            get { return ObjectManager.Memory.Read<int>(BaseAddress + ObjectManager.Addr_Type); }
        }

        public uint Entry
        {
            get { return GetStorageField<uint>(ObjectManager.Addr_EntryID); }
        }

        protected T GetStorageField<T>(int field) where T : struct
        {
            var m_pStorage = ObjectManager.Memory.Read<IntPtr>(BaseAddress + 0x4);
            return ObjectManager.Memory.Read<T>(m_pStorage + field);
        }
    }
}