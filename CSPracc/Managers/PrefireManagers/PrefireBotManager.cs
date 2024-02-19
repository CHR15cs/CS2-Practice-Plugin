using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using CSPracc.DataModules;
using CSPracc.DataStorages.JsonStorages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSPracc.Managers.PrefireManagers
{
    public class PrefireBotManager
    {
        private SpawnPointStorage? _spawnPointStorage;
        private SpawnPointStorage? spawnPointStorage
        {
            get
            {
                if (_spawnPointStorage == null || _spawnPointStorage.Map != Server.MapName)
                {
                    _spawnPointStorage = new SpawnPointStorage(new DirectoryInfo(Path.Combine(CSPraccPlugin.Instance.ModuleDirectory, "SpawnPoints")));
                }
                return _spawnPointStorage;
            }
        }
        public void ClearUsedSpawns()
        {
            spawnPointStorage.ResetUsedSpawns();
        }
        public bool TeleportToUnusedSpawn(CCSPlayerController player, string bombsite)
        {
            if (player == null || !player.IsValid || player.IsBot) return false;

            JsonSpawnPoint? unusedSpawn = spawnPointStorage!.GetUnusedSpawnPointFromTeam(player.GetCsTeam(), bombsite);
            if (unusedSpawn == null)
            {
                return false;
            }
            //player.PlayerPawn.Value!.Teleport(unusedSpawn.Position.ToCSVector(), unusedSpawn.QAngle.ToCSQAngle(), new Vector(0, 0, 0));
            return true;
        }

        public void AddCurrentPositionAsSpawnPoint(CCSPlayerController player, string Bombsite)
        {
            if (player == null || !player.IsValid) return;

           // spawnPointStorage!.AddSpawnPoint(new JsonSpawnPoint(player.PlayerPawn.Value.CBodyComponent!.SceneNode!.AbsOrigin.ToVector3(), player.PlayerPawn.Value!.EyeAngles.ToVector3(), Bombsite), player.GetCsTeam());
        }


        public void AddBots(PrefireRoute route)
        {
            for (int i = 0; i < route.spawnPoints.Count; i++)
            {
                Server.ExecuteCommand("bot_join_team CT");
                Server.ExecuteCommand("bot_add_ct");
            }
        }


        public void LoadSpawnsForBombsite(string Bombsite)
        {
            if (Bombsite == null) return;
            Server.PrintToConsole("Check if spawnpoints exist");
            if (spawnPointStorage!.GetSpawnPoints() == null) { Server.PrintToConsole("GetSpawnPoints returned null"); return; }
            Server.PrintToConsole("GetSpawnPoints returned NOT null");
            Dictionary<CsTeam, List<JsonSpawnPoint>> spawns = spawnPointStorage!.GetSpawnPoints();
            if (spawns != null)
            {
                foreach (CsTeam team in spawns.Keys)
                {
                    Server.PrintToConsole($"team {team} exist");
                }
                if (!spawns.ContainsKey(CsTeam.Terrorist))
                {
                    Server.PrintToConsole("No Terrorist spawns exist");
                    return;
                }
                if (!spawns.ContainsKey(CsTeam.CounterTerrorist))
                {
                    Server.PrintToConsole("No Counter-Terrorist spawns exist");
                    return;
                }
                int tcount = 0;
                int ctcount = 0;
                foreach (JsonSpawnPoint spawner in spawns[CsTeam.Terrorist])
                {
                    tcount++;
                }
                foreach (JsonSpawnPoint spawner in spawns[CsTeam.CounterTerrorist])
                {
                    ctcount++;
                }
                Server.PrintToConsole($"TCount: {tcount} - CTCount {ctcount}");
                if (tcount == 0 || ctcount == 0)
                {
                    Server.PrintToConsole("insufficient amount of spawns exist");
                    return;
                }
            }
            else
            {
                Server.PrintToConsole("spawns were null");
                return;
            }
            Server.PrintToConsole("Calling Delete SpawnPoints");
            //DeleteAllSpawnPoints();
            //Server.PrintToConsole("Calling Create Terrorist SpawnPoints");
            //CreateSpawnPointsFromJsonPoints(spawnPointStorage.GetSpawnPointsFromTeam(CsTeam.Terrorist)!, "info_player_terrorist", Bombsite);
            //Server.PrintToConsole("Calling Create Counter Terrorist SpawnPoints");
            //CreateSpawnPointsFromJsonPoints(spawnPointStorage.GetSpawnPointsFromTeam(CsTeam.CounterTerrorist)!, "info_player_counterterrorist", Bombsite);
        }
    }
}
