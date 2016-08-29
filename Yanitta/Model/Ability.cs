using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Linq;
using System.Xml.Serialization;

namespace Yanitta
{
    /// <summary>
    /// Ability structure.
    /// </summary>
    [Serializable]
    public class Ability : ViewModelBase
    {
        [XmlIgnore]
        public bool IsChecked { get; set; }

        string name;
        /// <summary>
        /// Ability name.
        /// </summary>
        [XmlAttribute]
        public string Name
        {
            get { return name; }
            set { Set(ref name, value); }
        }

        uint spellid;
        /// <summary>
        /// Spell id.
        /// </summary>
        public uint SpellID
        {
            get { return spellid; }
            set { Set(ref spellid, value); }
        }

        /// <summary>
        /// Target type.
        /// </summary>
        [XmlElement("Target")]
        public List<TargetType> TargetList { get; set; } = new List<TargetType>();

        bool cencelchanel;
        /// <summary>
        /// Cencel channel spells.
        /// </summary>
        public bool CancelChannel
        {
            get { return cencelchanel; }
            set { Set(ref cencelchanel, value); }
        }

        bool cencelcasting;
        /// <summary>
        /// Cancel spell casting.
        /// </summary>
        public bool CancelCasting
        {
            get { return cencelcasting; }
            set { Set(ref cencelcasting, value); }
        }

        bool useincombat;
        /// <summary>
        /// Combat check.
        /// </summary>
        public bool IsUseIncombat
        {
            get { return useincombat; }
            set { Set(ref useincombat, value); }
        }

        bool isusablecheck = true;
        /// <summary>
        /// Usable spell check.
        /// </summary>
        public bool IsUsableCheck
        {
            get { return isusablecheck; }
            set { Set(ref isusablecheck, value); }
        }

        float recastdelay;
        /// <summary>
        /// Delay for cast next spell.
        /// If 0 then not set delay.
        /// </summary>
        public float RecastDelay
        {
            get { return recastdelay; }
            set { Set(ref recastdelay, value); }
        }

        bool rangeCheck;

        /// <summary>
        /// Range check distance.
        /// </summary>
        public bool RangeCheck
        {
            get { return rangeCheck; }
            set { Set(ref rangeCheck, value); }
        }

        MovingStates movingstate;
        /// <summary>
        /// Moving state.
        /// </summary>
        public MovingStates IsMovingCheck
        {
            get { return movingstate; }
            set { Set(ref movingstate, value); }
        }

        /// <summary>
        /// Lua code to check for the use of ability.
        /// </summary>
        [XmlIgnore]
        public string Lua { get; set; }

        /// <summary>
        ///
        /// </summary>
        [XmlElement("Lua")]
        public XmlCDataSection _lua
        {
            get { return CreateCDataSection(Lua); }
            set { Lua = GetTrimValue(value); }
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
            var m_rangeCheck      = RangeCheck.ToString().ToLower();

            // targets
            var targetList = string.Join($",\n{T + T + T}",
                TargetList.OrderBy(n => n).Select(n =>
                    $"{{ Target = \"{n.ToString().ToLower()}\" }}"));

            // code
            var lua = string.IsNullOrWhiteSpace(Lua) ? "return false;" : Lua;
            var funcContent = string.Join($"\n{T + T + T}",
                lua.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries));

            var builder = new StringBuilder();
            builder.AppendLine($"    {{   SpellId = {SpellID,6}, Name = \"{name.Replace("\"", @"\""")}\",");
            builder.AppendLine($"        IsMovingCheck     = \"{isMovingCheck}\",");
            builder.AppendLine($"        RecastDelay       = {RecastDelay},");
            builder.AppendLine($"        DropChanel        = {cancelChannel},");
            builder.AppendLine($"        CancelCasting     = {cancelCasting},");
            builder.AppendLine($"        IsCheckInCombat   = {isUseIncombat},");
            builder.AppendLine($"        IsUsableCheck     = {isUsableCheck},");
            builder.AppendLine($"        RangeCheck        = {m_rangeCheck},");
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