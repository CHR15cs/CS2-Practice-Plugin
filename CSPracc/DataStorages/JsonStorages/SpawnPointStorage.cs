using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using CSPracc.DataModules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSPracc.DataStorages.JsonStorages
{
    public class SpawnPointStorage : JsonStorage<int, List<JsonSpawnPoint>>
    {
        private FileInfo SpawnPointFile { get; }
        private Dictionary<CsTeam, List<JsonSpawnPoint>> spawnPoints;
        public string Map { get; init; }
        public SpawnPointStorage(DirectoryInfo spawnPointDir) :base(new FileInfo(Path.Combine(spawnPointDir.FullName, $"{Server.MapName}_spawnpoints.json")))
        {
            spawnPoints = new Dictionary<CsTeam, List<JsonSpawnPoint>>();
            Map = Server.MapName;
            SpawnPointFile = new FileInfo(Path.Combine(CSPraccPlugin.Instance.ModulePath,"SpawnPoints",$"Spawns_{Map}.json"));
            if(SpawnPointFile.Exists)
            {
                spawnPoints = loadSpawnPointsFromFile(SpawnPointFile);
            }
        }

        private Dictionary<CsTeam,List<JsonSpawnPoint>> loadSpawnPointsFromFile(FileInfo spawnPointFile)
        {
            Dictionary<CsTeam, List<JsonSpawnPoint>> dict = new Dictionary<CsTeam, List<JsonSpawnPoint>>();
            foreach(var item in base.GetAll())
            {
                dict.Add((CsTeam)item.Key, item.Value);
            }
            return dict;
        }

        public Dictionary<CsTeam, List<JsonSpawnPoint>> GetSpawnPoints()
        {
            return spawnPoints;
        }

        public List<JsonSpawnPoint>? GetSpawnPointsFromTeam(CsTeam team)
        {
            if(!spawnPoints.ContainsKey(team))
            {
                return null;
            }
            return spawnPoints[team];
        }

        public void AddSpawnPoint(JsonSpawnPoint spawnPointToAdd,CsTeam team)
        {
            if(!spawnPoints.ContainsKey(team))
            {
                spawnPoints.Add(team, new List<JsonSpawnPoint>());
            }
            List<JsonSpawnPoint>? existingSpawnPoints = new List<JsonSpawnPoint>();
            if(!spawnPoints.TryGetValue(team, out existingSpawnPoints))
            {
                return;
            }
            spawnPoints[team].Add(spawnPointToAdd);
            base.SetOrAdd((int)team, spawnPoints[team]);
        }

        public void RemoveSpawnPoint(JsonSpawnPoint spawnPointToRemove,CsTeam team)
        {
            if (!spawnPoints.TryGetValue(team, out List<JsonSpawnPoint>? existingSpawnPoints))
            {
                return;
            }
            spawnPoints[team].Remove(spawnPointToRemove);
        }

        public void RemoveSpawnPoint(int  spawnPointIndex, CsTeam team)
        {
            if (!spawnPoints.ContainsKey(team))
            {
                return;
            }
            spawnPoints[team].RemoveAt(spawnPointIndex);
        }

        public override bool Get(int key, out List<JsonSpawnPoint> value)
        {
           value = GetSpawnPointsFromTeam((CsTeam)key)!;
            if(value == null)
            {
                return false;
            }
            return true;
        }
    }
}
