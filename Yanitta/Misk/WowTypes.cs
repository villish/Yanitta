namespace Yanitta
{
    public enum WowClass : byte
    {
        [LocalizedName("нет")]
        None = 00,

        [LocalizedName("Воин")]
        Warrior = 01,

        [LocalizedName("Паладин")]
        Paladin = 02,

        [LocalizedName("Охотник")]
        Hunter = 03,

        [LocalizedName("Разбойник")]
        Rogue = 04,

        [LocalizedName("Жрец")]
        Priest = 05,

        [LocalizedName("Рыцарь смерти")]
        DeathKnight = 06,

        [LocalizedName("Шаман")]
        Shaman = 07,

        [LocalizedName("Маг")]
        Mage = 08,

        [LocalizedName("Чернокнижник")]
        Warlock = 09,

        [LocalizedName("Монах")]
        Monk = 10,

        [LocalizedName("Друид")]
        Druid = 11,
    }

    public enum TargetType
    {
        [LocalizedName("нет")]
        None,

        [LocalizedName("Цель")]
        Target,

        [LocalizedName("Цель цели")]
        TargetTarget,

        [LocalizedName("Фокус")]
        Focus,

        [LocalizedName("Цель фокуса")]
        FocusTarget,

        [LocalizedName("Питомец")]
        Pet,

        [LocalizedName("Игрок")]
        Player,

        [LocalizedName("Под указателем")]
        Mouseover,

        [LocalizedName("По локации")]
        MouseLocation
    }

    [System.AttributeUsage(System.AttributeTargets.Field, Inherited = false)]
    public sealed class LocalizedNameAttribute : System.Attribute
    {
        private readonly string name;

        public LocalizedNameAttribute(string name)
        {
            this.name = name;
        }

        public string Name
        {
            get { return this.name; }
        }
    }

    public static class EnumEx
    {
        public static string GetLocalizedName(this System.Enum enum_value)
        {
            LocalizedNameAttribute[] attr = (LocalizedNameAttribute[])enum_value
                .GetType()
                .GetField(enum_value.ToString())
                .GetCustomAttributes(typeof(LocalizedNameAttribute), false);

            if (attr.Length == 1)
                return attr[0].Name;
            return string.Empty;
        }
    }
}