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
    public class Ability : ViewModelBase
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
        /// Проверка на доступность заклинания.
        /// По умолчанию должно быть включено.
        /// </summary>
        public bool IsUsableCheck          { get; set; }

        /// <summary>
        /// Задержка на произнесение следующего заклинания.
        /// Если 0, тогда не делать проверку.
        /// </summary>
        public float RecastDelay           { get; set; }

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
            get { return CreateCDataSection(Lua); }
            set { Lua = GetTrimValue(value); }
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
            var T = "    ";
            var cancelChannel     = CancelChannel.ToString().ToLower();
            var cancelCasting     = CancelCasting.ToString().ToLower();
            var isUseIncombat     = IsUseIncombat.ToString().ToLower();
            var isUsableCheck     = IsUsableCheck.ToString().ToLower();
            var isMovingCheck     = IsMovingCheck.ToString().ToLower();
            var name              = Name.Replace("\"", @"\""");

            // targets
            var targetList = string.Join($",\n{T + T + T}",
                TargetList.OrderBy(n => n).Select(n =>
                    $"{{ Target = \"{n.ToString().ToLower()}\" }}"));

            // код
            var lua = string.IsNullOrWhiteSpace(Lua) ? "return false;" : Lua;
            var funcContent = string.Join($"\n{T + T + T}",
                lua.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries));

            var builder = new StringBuilder();
            builder.AppendLine($"    {{   SpellId = {SpellID,6}, Name = \"{name}\",");
            builder.AppendLine($"        IsMovingCheck     = \"{isMovingCheck}\",");
            builder.AppendLine($"        RecastDelay       = {RecastDelay},");
            builder.AppendLine($"        DropChanel        = {cancelChannel},");
            builder.AppendLine($"        CancelCasting     = {cancelCasting},");
            builder.AppendLine($"        IsCheckInCombat   = {isUseIncombat},");
            builder.AppendLine($"        IsUsableCheck     = {isUsableCheck},");
            builder.AppendLine($"        TargetList = {{\n{T + T + T}{targetList}\n{T + T}}},");
            builder.AppendLine($"        Func = function(ability, targetInfo, target)\n{T + T + T}{funcContent}\n{T + T}end");
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
                Name           = Name,
                SpellID        = SpellID,
                IsMovingCheck  = IsMovingCheck,
                IsUseIncombat  = IsUseIncombat,
                IsUsableCheck  = IsUsableCheck,
                CancelCasting  = CancelCasting,
                CancelChannel  = CancelChannel,
                RecastDelay    = RecastDelay,
                Lua            = Lua
            };

            foreach (var targetType in TargetList)
                ability.TargetList.Add(targetType);

            return ability;
        }
    }
}