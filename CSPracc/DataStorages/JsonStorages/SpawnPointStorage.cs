using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using CSPracc.DataModules;
using CSPracc.Modes;
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

        private Dictionary<CsTeam,List<int>> usedSpawn = new Dictionary<CsTeam, List<int>>();
        public SpawnPointStorage(DirectoryInfo spawnPointDir) :base(new FileInfo(Path.Combine(spawnPointDir.FullName, $"{Server.MapName}_spawnpoints.json")))
        {
            spawnPoints = new Dictionary<CsTeam, List<JsonSpawnPoint>>();
            Map = Server.MapName;
            SpawnPointFile = new FileInfo(Path.Combine(Path.Combine(spawnPointDir.FullName, $"{Server.MapName}_spawnpoints.json")));
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

        public List<JsonSpawnPoint>? GetSpawnPointsFromTeam(CsTeam team,string bombsite)
        {
            if(!spawnPoints.ContainsKey(team))
            {
                return null;
            }
            List<JsonSpawnPoint> teamSpawnPoints = spawnPoints[team];
            List<JsonSpawnPoint> targetSpawnPoints = new List<JsonSpawnPoint>();
            foreach(JsonSpawnPoint jsp in teamSpawnPoints)
            {
                if(jsp.Bombsite == bombsite)
                {
                    targetSpawnPoints.Add(jsp);
                }
            }
            return targetSpawnPoints;
        }

        public void ResetUsedSpawns()
        {
            usedSpawn.Clear();
        }

        public JsonSpawnPoint? GetUnusedSpawnPointFromTeam(CsTeam team,string bombsite)
        {
            if(!spawnPoints.ContainsKey(team))
            {
                return null;
            }
            List<JsonSpawnPoint> spawnsPerTeam = GetSpawnPointsFromTeam(team,bombsite)!;
            List<int>? usedSpawnIds = null;
            if (usedSpawn.ContainsKey(team))
            {
                if(usedSpawn.TryGetValue(team, out usedSpawnIds))
                {
                    if(usedSpawnIds == null)
                    {
                        usedSpawn[team] = new List<int>();
                    }
                }
            }
            else
            {
                usedSpawnIds = new List<int>(); 
                usedSpawn.Add(team, usedSpawnIds);
            }
            if(usedSpawnIds!.Count == spawnsPerTeam.Count)
            {
                //all spawns are in use
                return null;
            }
            Random rnd = new Random();
            int spawnid = -1;
            bool unusedSpawnFound = false;
            do
            {
                spawnid = rnd.Next(0, spawnsPerTeam.Count);
                if(!usedSpawnIds.Contains(spawnid))
                {
                    unusedSpawnFound = true;
                }
            } while (!unusedSpawnFound);
            usedSpawn[team].Add(spawnid);
            return spawnsPerTeam[spawnid];

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
           value = GetSpawnPointsFromTeam((CsTeam)key,"")!;
            if(value == null)
            {
                return false;
            }
            return true;
        }
    }
}
