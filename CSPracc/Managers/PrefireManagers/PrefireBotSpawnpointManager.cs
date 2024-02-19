using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API;
using CSPracc.DataModules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSPracc.Managers.PrefireManagers
{
    public class PrefireBotSpawnpointManager
    {    
        public PrefireBotSpawnpointManager()
        {
        }      
        public static Dictionary<CCSPlayerController, List<JsonSpawnPoint>> GenerateSpawnpositionsPerBot(PrefireRoute route)
        {
            var SpawnPointsPerBot = new Dictionary<CCSPlayerController, List<JsonSpawnPoint>>();
            List<CCSPlayerController> bots = Utilities.GetPlayers().Where(x => x.IsBot && x.IsValid && !x.IsHLTV).ToList();
            int botNumber = 0;
            foreach (JsonSpawnPoint? spawnPoint in route.spawnPoints)
            {
                if (spawnPoint == null) continue;

                if (!SpawnPointsPerBot.ContainsKey(bots[botNumber]))
                {
                    SpawnPointsPerBot.Add(bots[botNumber], new List<JsonSpawnPoint>());
                }
                SpawnPointsPerBot[bots[botNumber]].Add(spawnPoint);
                if (botNumber + 1 < bots.Count)
                {
                    botNumber++;
                }
                else
                {
                    botNumber = 0;
                }
            }
            return SpawnPointsPerBot;
        }
    }
}
