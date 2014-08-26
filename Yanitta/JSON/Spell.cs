using System.Runtime.Serialization;

namespace Yanitta.JSON
{
    [DataContract]
    public class Spell
    {
        [DataMember(Name = "id")]
        public int Id               { get; set; }

        [DataMember(Name = "name")]
        public string Name          { get; set; }

        [DataMember(Name = "icon")]
        public string Icon          { get; set; }

        [DataMember(Name = "description")]
        public string Description   { get; set; }

        [DataMember(Name = "range")]
        public string Range         { get; set; }

        [DataMember(Name = "powerCost")]
        public string PowerCost     { get; set; }

        [DataMember(Name = "castTime")]
        public string CastTime      { get; set; }
    }
}
