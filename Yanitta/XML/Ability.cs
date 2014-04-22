using System;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace Yanitta
{
    /// <summary>
    /// Описывает состав из кода и статических проверок при вычислении на доступность чтения заклинания.
    /// </summary>
    [Serializable]
    public class Ability : ICloneable
    {
        /// <summary>
        /// Наименование способности.
        /// </summary>
        [XmlAttribute]
        public string Name                 { get; set; }

        /// <summary>
        /// Ид заклинания
        /// </summary>
        public uint SpellID                { get; set; }

        /// <summary>
        /// Тип цели
        /// </summary>
        public TargetType Target           { get; set; }

        /// <summary>
        /// Прерывать канальные заклинания
        /// </summary>
        public bool CancelChannel          { get; set; }

        /// <summary>
        /// Прерывать чтение заклинаний
        /// </summary>
        public bool CancelCasting          { get; set; }

        /// <summary>
        /// Проверка возможности атаковать цель
        /// </summary>
        public bool IsAttacedTarget        { get; set; }

        /// <summary>
        /// Проверка нахождения в бою
        /// </summary>
        public bool IsUseIncombat          { get; set; }

        /// <summary>
        /// Использовать задержку на произнесение следующего заклинания
        /// </summary>
        public bool SetRecastDelay         { get; set; }

        /// <summary>
        /// Это заклинание не имеет общего ГКД
        /// </summary>
        public bool IsNotGCD               { get; set; }

        /// <summary>
        /// Делает проверку на то, что данное заклинание доступно для персонажа.
        /// Не работает для некоторых заклинаний (например: Увечье (друид))
        /// </summary>
        public bool IsSpellKnownCheck      { get; set; }

        /// <summary>
        /// Проверка, движения персонажа.
        /// </summary>
        public MovingStates IsMovingCheck  { get; set; }

        /// <summary>
        /// Луа код для проверки использования способности
        /// </summary>
        [XmlIgnore]
        public string Lua                  { get; set; }

        /// <summary>
        ///
        /// </summary>
        public Ability()
        {
            this.Name = "ability_name";
        }

        /// <summary>
        ///
        /// </summary>
        [XmlElement("Lua")]
        public XmlCDataSection _lua
        {
            get { return this.Lua.CreateCDataSection(); }
            set { this.Lua = value.GetTrimValue();      }
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            // переменные
            var target            = this.Target.ToString().ToLower();
            var cancelChannel     = this.CancelChannel.ToString().ToLower();
            var cancelCasting     = this.CancelCasting.ToString().ToLower();
            var isAttacedTarget   = this.IsAttacedTarget.ToString().ToLower();
            var isUseIncombat     = this.IsUseIncombat.ToString().ToLower();
            var setRecastDelay    = this.SetRecastDelay.ToString().ToLower();
            var isNotHCD          = this.IsNotGCD.ToString().ToLower();
            var isSpellKnownCheck = this.IsSpellKnownCheck.ToString().ToLower();
            var isMovingCheck     = this.IsMovingCheck.ToString().ToLower();

            // код
            var lua = string.IsNullOrWhiteSpace(this.Lua) ? "return false;" : this.Lua;

            var builder = new StringBuilder();
            builder.AppendFormatLine("table.insert(ABILITY_TABLE, {");
            builder.AppendFormatLine("    SpellId           = {0},",     this.SpellID);
            builder.AppendFormatLine("    Name              = \"{0}\",", this.Name);
            builder.AppendFormatLine("    Target            = \"{0}\",", target);
            builder.AppendFormatLine("    IsMovingCheck     = \"{0}\",", isMovingCheck);
            builder.AppendFormatLine("    DropChanel        = {0},",     cancelChannel);
            builder.AppendFormatLine("    CancelCasting     = {0},",     cancelCasting);
            builder.AppendFormatLine("    IsAttackedCheck   = {0},",     isAttacedTarget);
            builder.AppendFormatLine("    IsCheckInCombat   = {0},",     isUseIncombat);
            builder.AppendFormatLine("    SetRecastDelay    = {0},",     setRecastDelay);
            builder.AppendFormatLine("    IsSpellKnownCheck = {0},",     isSpellKnownCheck);
            builder.AppendFormatLine("    IsNotGCD          = {0},",     isNotHCD);
            builder.AppendFormatLine("    LastCastingTime   = 0,");
            builder.AppendFormatLine("    Func = function(ability)\n        {0}\n    end",
                string.Join("\n        ", lua.Split(new [] {'\r','\n'}, StringSplitOptions.RemoveEmptyEntries)));
            builder.AppendFormatLine("});");

            return builder.ToString();
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
}