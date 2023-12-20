using CounterStrikeSharp.API.Modules.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace CSPracc.DataModules
{
    public class JsonSpawnPoint
    {
        [JsonProperty("Position")]
        public System.Numerics.Vector3 Position { get; set; }
        [JsonProperty("Angle")]
        public System.Numerics.Vector3 QAngle { get; set; }

        [JsonProperty("Bombsite")]
        public string? Bombsite { get; set; } 

        public JsonSpawnPoint(Vector3 position, Vector3 qangle,string bombsite) 
        { 
            Position = position;
            QAngle = qangle;
            Bombsite = bombsite;
        }

    }
}
