using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Linq;
using System.Xml.Serialization;

namespace Yanitta
{
    /// <summary>
    /// Описывает состав из кода и статических проверок при вычислении на доступность чтения заклинания.
    /// </summary>
    [Serializable]
    public class Ability
    {
        /// <summary>
        /// Служебное поле.
        /// </summary>
        [XmlIgnore]
        public bool IsChecked              { get; set; }

        /// <summary>
        /// Наименование способности.
        /// </summary>
        [XmlAttribute]
        public string Name                 { get; set; }

        /// <summary>
        /// Ид заклинания.
        /// </summary>
        public uint SpellID                { get; set; }

        /// <summary>
        /// Тип цели.
        /// </summary>
        [XmlElement("Target")]
        public List<TargetType> TargetList { get; set; }

        /// <summary>
        /// Прерывать канальные заклинания.
        /// </summary>
        public bool CancelChannel          { get; set; }

        /// <summary>
        /// Прерывать чтение заклинаний.
        /// </summary>
        public bool CancelCasting          { get; set; }

        /// <summary>
        /// Проверка нахождения в бою.
        /// </summary>
        public bool IsUseIncombat          { get; set; }

        /// <summary>
        /// Использовать задержку на произнесение следующего заклинания.
        /// </summary>
        public bool SetRecastDelay         { get; set; }

        /// <summary>
        /// Проверка, движения персонажа.
        /// </summary>
        public MovingStates IsMovingCheck  { get; set; }

        /// <summary>
        /// Луа код для проверки использования способности.
        /// </summary>
        [XmlIgnore]
        public string Lua                  { get; set; }

        /// <summary>
        ///
        /// </summary>
        [XmlElement("Lua")]
        public XmlCDataSection _lua
        {
            get { return this.Lua.CreateCDataSection(); }
            set { this.Lua = value.GetTrimValue();      }
        }

        public Ability()
        {
            TargetList = new List<TargetType>();
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            // переменные
            //var target            = this.Target.ToString().ToLower();
            var cancelChannel     = this.CancelChannel.ToString().ToLower();
            var cancelCasting     = this.CancelCasting.ToString().ToLower();
            var isUseIncombat     = this.IsUseIncombat.ToString().ToLower();
            var setRecastDelay    = this.SetRecastDelay.ToString().ToLower();
            var isMovingCheck     = this.IsMovingCheck.ToString().ToLower();
            var name              = this.Name.Replace("\"", @"\""");

            // код
            var lua = string.IsNullOrWhiteSpace(this.Lua) ? "return false;" : this.Lua;

            var builder = new StringBuilder();
            builder.AppendFormatLine("    {{   SpellId = {0,6}, Name = \"{1}\",", this.SpellID, name);
            builder.AppendFormatLine("        IsMovingCheck     = \"{0}\",", isMovingCheck);
            builder.AppendFormatLine("        DropChanel        = {0},",     cancelChannel);
            builder.AppendFormatLine("        CancelCasting     = {0},",     cancelCasting);
            builder.AppendFormatLine("        IsCheckInCombat   = {0},",     isUseIncombat);
            builder.AppendFormatLine("        SetRecastDelay    = {0},",     setRecastDelay);

            builder.AppendFormatLine("        TargetList = {{\n            {0}\n        }},",
                string.Join(",\n            ", TargetList.Select(n =>
                    string.Format("{{ Target = \"{0}\", Guid = nil, LastCastingTime = 0, dps = 0 }}", n.ToString().ToLower()))));

            builder.AppendFormatLine("        Func = function(ability, targetInfo, target)\n            {0}\n        end",
                string.Join("\n            ", lua.Split(new [] {'\r','\n'}, StringSplitOptions.RemoveEmptyEntries)));
            builder.Append("    }");

            return builder.ToString();
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public Ability Clone()
        {
            var ability = new Ability {
                Name            = this.Name,
                SpellID         = this.SpellID,
                IsMovingCheck   = this.IsMovingCheck,
                IsUseIncombat   = this.IsUseIncombat,
                CancelCasting   = this.CancelCasting,
                CancelChannel   = this.CancelChannel,
                SetRecastDelay  = this.SetRecastDelay,
                Lua             = this.Lua,
                TargetList = new List<TargetType>()
            };

            foreach (var target in this.TargetList)
                ability.TargetList.Add(target);

            return ability;
        }
    }
}