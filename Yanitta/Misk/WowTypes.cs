namespace Yanitta
{
    public enum WowClass : byte
    {
        Warrior     = 0x01,
        Paladin     = 0x02,
        Hunter      = 0x03,
        Rogue       = 0x04,
        Priest      = 0x05,
        DeathKnight = 0x06,
        Shaman      = 0x07,
        Mage        = 0x08,
        Warlock     = 0x09,
        Monk        = 0x0A,
        Druid       = 0x0B,
    }

    public enum TargetType
    {
        None,
        Target,
        TargetTarget,
        Focus,
        FocusTarget,
        Pet,
        Player,
        Mouseover,
        MouseLocation
    }

    public enum MovingStates
    {
        None      = 0,
        Moving    = 1,
        NotMoving = 2,
    }
}