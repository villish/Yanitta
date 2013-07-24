namespace Yanitta.Hantchk
{
    public class WoWGameObject : WoWObject
    {
        public WoWGameObject(uint baseAddress)
            : base(baseAddress)
        {
        }

        public int AnimationState
        {
            get { return ObjectManager.Memory.Read<int>(BaseAddress + ObjectManager.Addr_AnimationState); }
        }

        public ulong CreatedBy
        {
            get { return GetStorageField<ulong>(ObjectManager.Addr_CreatedBy); }
        }

        public bool CreatedByMe
        {
            get { return CreatedBy == ObjectManager.PlayerGuid; }
        }
    }
}