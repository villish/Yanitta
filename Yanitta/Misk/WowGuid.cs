namespace Yanitta
{
    public enum GuidType : byte
    {
        Null             = 0,
        Uniq             = 1,
        Player           = 2,
        Item             = 3,
        StaticDoor       = 4,
        Transport        = 5,
        Conversation     = 6,
        Creature         = 7,
        Vehicle          = 8,
        Pet              = 9,
        GameObject       = 10,
        DynamicObject    = 11,
        AreaTrigger      = 12,
        Corpse           = 13,
        LootObject       = 14,
        SceneObject      = 15,
        Scenario         = 16,
        AIGroup          = 17,
        DynamicDoor      = 18,
        ClientActor      = 19,
        Vignette         = 20,
        CallForHelp      = 21,
        AIResource       = 22,
        AILock           = 23,
        AILockTicket     = 24,
        ChatChannel      = 25,
        Party            = 26,
        Guild            = 27,
        WowAccount       = 28,
        BNetAccount      = 29,
        GMTask           = 30,
        MobileSession    = 31,
        RaidGroup        = 32,
        Spell            = 33,
        Mail             = 34,
        WebObj           = 35,
        LFGObject        = 36,
        LFGList          = 37,
        UserRouter       = 38,
        PVPQueueGroup    = 39,
        UserClient       = 40,
        PetBattle        = 41,
        UniqueUserClient = 42,
        BattlePet        = 43
    }

    public struct WowGuid
    {
        private long lo;
        private long hi;

        public static readonly WowGuid Empty = new WowGuid(0L, 0L);

        public WowGuid(long lo, long hi)
        {
            this.hi = hi;
            this.lo = lo;
        }

        public GuidType Type    => (GuidType)(byte)((hi >> 58) & 0x3F);
        public byte SubType     => (byte)((lo   >> 56)  & 0x3F);
        public ushort RealmId   => (ushort)((hi >> 42)  & 0x1FFF);
        public ushort ServerId  => (ushort)((lo >> 40)  & 0x1FFF);
        public ushort MapId     => (ushort)((hi >> 29)  & 0x1FFF);
        public uint Entry       => (uint)((hi >> 6)     & 0x7FFFFF);
        public ulong Counter    => (ulong)(lo & 0x000000FFFFFFFFFFL);

        public override string ToString()
        {
            switch (Type)
            {
                case GuidType.Creature:
                case GuidType.Vehicle:
                case GuidType.Pet:
                case GuidType.GameObject:
                case GuidType.AreaTrigger:
                case GuidType.DynamicObject:
                case GuidType.Corpse:
                case GuidType.LootObject:
                case GuidType.SceneObject:
                case GuidType.Scenario:
                case GuidType.AIGroup:
                case GuidType.DynamicDoor:
                case GuidType.Vignette:
                case GuidType.Conversation:
                case GuidType.CallForHelp:
                case GuidType.AIResource:
                case GuidType.AILock:
                case GuidType.AILockTicket:
                    return $"{Type}-{SubType}-{RealmId}-{MapId}-{ServerId}-{Entry}-{Counter:X10}";
                case GuidType.Player:
                    return $"{Type}-{RealmId}-{(ulong)lo:X8}";
                case GuidType.Item:
                    return $"{Type}-{RealmId}-{(uint)((hi >> 18) & 0xFFFFFF)}-{(ulong)lo:X10}";
                case GuidType.ClientActor:
                case GuidType.Transport:
                case GuidType.StaticDoor:
                    return $"{Type}-{RealmId}-{Counter}";
                default:
                    return $"{Type}-0x{(ulong)lo:X16}{(ulong)hi:X16}";
            }
        }

        public override int GetHashCode() => lo.GetHashCode() ^ hi.GetHashCode();
        public static bool operator ==(WowGuid left, WowGuid right) => left.hi == right.hi && left.lo == right.lo;
        public static bool operator !=(WowGuid left, WowGuid right) => left.lo != right.lo || left.hi != right.hi;

        public override bool Equals(object obj)
        {
            if (obj is WowGuid)
                return hi == ((WowGuid)obj).hi && lo == ((WowGuid)obj).lo;
            return false;
        }
    }
}