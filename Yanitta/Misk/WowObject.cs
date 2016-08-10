using System;

namespace Yanitta
{
    //static class FieldOffsets
    //{
    //    public const int FirstObject    = 0x00D8; // need comment
    //    public const int NextObject     = 0x003C; // need comment
    //
    //    public const int Type           = 0x000C; // need comment
    //    public const int Player         = 0x00F8; // need comment
    //    public const int VisibleGuid    = 0x0028; // need comment
    //    public const int AnimationState = 0x0104; // need comment
    //    public const int CreatedBy      = 0x0030; // need comment
    //}

    public class WowObject
    {
        ProcessMemory Wow;
        public IntPtr BaseAddr { get; set; }

        public WowObject(ProcessMemory wow, IntPtr baseAddr)
        {
            Wow = wow;
            BaseAddr = baseAddr;
        }

        public WowGuid Guid      => Wow.Read<WowGuid>(BaseAddr + (int)Settings.VisibleGuid);
        public int Type          => Wow.Read<int>(BaseAddr + (int)Settings.ObjectType);
        public bool IsBoobing    => Wow.Read<byte>(BaseAddr + (int)Settings.AnimationState) != 0;
        public WowGuid CreatedBy => Wow.Read<WowGuid>(Wow.Read<IntPtr>(BaseAddr + IntPtr.Size) + (int)Settings.CreatedBy);
    }
}
