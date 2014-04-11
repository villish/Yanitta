namespace Yanitta
{
    /// <summary>
    /// Перечень доступных классов.
    /// </summary>
    public enum WowClass : byte
    {
        None        = 00,
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

    /// <summary>
    /// Тип цели.
    /// </summary>
    public enum TargetType
    {
        None,
        Target,
        TargetTarget,
        Focus,
        FocusTarget,
        Pet,
        Player,
        Vehicle,
        Mouseover,
        MouseLocation
    }

    /// <summary>
    /// Тип движения персонажа.
    /// </summary>
    public enum MovingStates
    {
        None      = 0,
        Moving    = 1,
        NotMoving = 2,
    }
}