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
        DemonHunter = 12,
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
        MouseLocation,
        Boss1,
        Boss2,
        Boss3,
        Boss4,
        Boss5,
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

    public enum WowSpecializations
    {
        None = 0,

        Mage_Arcane           = WowClass.Mage        << 16 | 62,
        Mage_Fire             = WowClass.Mage        << 16 | 63,
        Mage_Frost            = WowClass.Mage        << 16 | 64,

        Paladin_Holy          = WowClass.Paladin     << 16 | 65,
        Paladin_Protection    = WowClass.Paladin     << 16 | 66,
        Paladin_Retribution   = WowClass.Paladin     << 16 | 70,

        Warrior_Arms          = WowClass.Warrior     << 16 | 71,
        Warrior_Fury          = WowClass.Warrior     << 16 | 72,
        Warrior_Protection    = WowClass.Warrior     << 16 | 73,

        Druid_Balance         = WowClass.Druid       << 16 | 102,
        Druid_Feral           = WowClass.Druid       << 16 | 103,
        Druid_Guardian        = WowClass.Druid       << 16 | 104,
        Druid_Restoration     = WowClass.Druid       << 16 | 105,

        DeathKnight_Blood     = WowClass.DeathKnight << 16 | 250,
        DeathKnight_Frost     = WowClass.DeathKnight << 16 | 251,
        DeathKnight_Unholy    = WowClass.DeathKnight << 16 | 252,

        Hunter_Beastmaster    = WowClass.Hunter      << 16 | 253,
        Hunter_Marksmanship   = WowClass.Hunter      << 16 | 254,
        Hunter_Survival       = WowClass.Hunter      << 16 | 255,

        Priest_Discipline     = WowClass.Priest      << 16 | 256,
        Priest_Holy           = WowClass.Priest      << 16 | 257,
        Priest_Shadow         = WowClass.Priest      << 16 | 258,

        Rogue_Assassination   = WowClass.Rogue       << 16 | 259,
        Rogue_Combat          = WowClass.Rogue       << 16 | 260,
        Rogue_Subtlety        = WowClass.Rogue       << 16 | 261,

        Shaman_Elemental      = WowClass.Shaman      << 16 | 262,
        Shaman_Enhancement    = WowClass.Shaman      << 16 | 263,
        Shaman_Restoration    = WowClass.Shaman      << 16 | 264,

        Warlock_Affliction    = WowClass.Warlock     << 16 | 265,
        Warlock_Demonology    = WowClass.Warlock     << 16 | 266,
        Warlock_Destruction   = WowClass.Warlock     << 16 | 267,

        Monk_Brewmaster       = WowClass.Monk        << 16 | 268,
        Monk_Windwalker       = WowClass.Monk        << 16 | 269,
        Monk_Mistweaver       = WowClass.Monk        << 16 | 270,

        DemonHunter_Havoc     = WowClass.DemonHunter << 16 | 577,
        DemonHunter_Vengeance = WowClass.DemonHunter << 16 | 581,
    }
}