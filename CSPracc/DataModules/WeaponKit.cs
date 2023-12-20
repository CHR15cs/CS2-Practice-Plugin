using CounterStrikeSharp.API.Modules.Entities.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace CSPracc.DataModules
{
    public class WeaponKit
    {
        [JsonPropertyName("Primary")]
        public List<string> Primary {  get; set; }
        [JsonPropertyName("Secondary")]
        public List<string> Secondary { get; set; }
        [JsonPropertyName("Kevlar")]
        public bool Kevlar { get; set; }
        [JsonPropertyName("Helmet")]
        public bool Helmet { get; set; }
        [JsonPropertyName("Nades")]
        public List<string> Nades { get; set; } = new List<string>();
        public WeaponKit(List<string> primary, List<string> secondary,bool kevlar,bool helmet, string[] nades) 
        { 
            Primary = primary; Secondary = secondary; Kevlar = kevlar;
            Nades = new List<string>(nades);
        }
        public WeaponKit(List<string> primary, List<string> secondary, bool kevlar, bool helmet)
        {
            Primary = primary; Secondary = secondary; Kevlar = kevlar; Helmet = helmet;
            Nades = new List<string>();
        }

        public WeaponKit()
        {
        }
    }
}
