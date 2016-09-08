using System;

namespace Yanitta
{
    public class WowObject
    {
        readonly ProcessMemory Wow;
        public IntPtr BaseAddr { get; set; }

        public WowObject(ProcessMemory wow, IntPtr baseAddr)
        {
            Wow = wow;
            BaseAddr = baseAddr;
        }

        public WowGuid Guid      => Wow.Read<WowGuid>(BaseAddr + Settings.VisibleGuid);
        public int Type          => Wow.Read<int>(BaseAddr + Settings.ObjectType);
        public bool IsBoobing    => Wow.Read<byte>(BaseAddr + Settings.AnimationState) != 0;
        public WowGuid CreatedBy => Wow.Read<WowGuid>(Wow.Read<IntPtr>(BaseAddr + IntPtr.Size) + Settings.CreatedBy);
    }
}
