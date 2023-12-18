using CounterStrikeSharp.API.Modules.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSPracc.DataModules
{
    public class UserSelectedEquipment
    {
        [JsonProperty("SelectedEquipment")]
        public Dictionary<CsTeam, PlayerEquipment> SelectedEquipment {  get; set; } = new Dictionary<CsTeam, PlayerEquipment>();
    }
}
