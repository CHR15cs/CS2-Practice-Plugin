using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSPracc.DataModules
{
    public class PrefireRoute
    {
        [JsonProperty("Name")]
        public string Name { get; set; } = "";

        [JsonProperty("SpawnPoints")]
        public List<JsonSpawnPoint> spawnPoints {  get; set; } = new List<JsonSpawnPoint>();

        [JsonProperty("StartingPoint")]
        public JsonSpawnPoint? StartingPoint { get; set; }

        public PrefireRoute(string name) 
        { 
            Name = name;    
        }
    }
}
