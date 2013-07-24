namespace Yanitta
{
    public enum WowClass : byte
    {
        Warrior     = 01,
        Paladin     = 02,
        Hunter      = 03,
        Rogue       = 04,
        Priest      = 05,
        DeathKnight = 06,
        Shaman      = 07,
        Mage        = 08,
        Warlock     = 09,
        Monk        = 10,
        Druid       = 11,
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
}