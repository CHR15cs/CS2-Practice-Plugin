using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API;
using CSPracc.DataModules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using CounterStrikeSharp.API.Modules.Utils;
using CounterStrikeSharp.API.Modules.Entities;
using System.Text.RegularExpressions;
using CounterStrikeSharp.API.Modules.Memory;
using CSPracc.DataStorages.JsonStorages;
using CSPracc.Extensions;

namespace CSPracc
{
    public static class SpawnManager
    {
        private static SpawnPointStorage? _spawnPointStorage;
        private static SpawnPointStorage? spawnPointStorage
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
        private static string lastMap = String.Empty;
        private static Dictionary<byte, List<Position>>? _spawns = null;
        /// <summary>
        /// Dictionary to store the spawns of the current map in
        /// </summary>
        public static Dictionary<byte,List<Position>> Spawns
        {
            get
            {
                if(_spawns == null)
                {
                    _spawns = new Dictionary<byte, List<Position>>();
                    if(lastMap == String.Empty)
                    {
                        lastMap = Server.MapName;
                        Logging.LogMessage("Map now : " + lastMap);
                    }
                    getSpawns(ref _spawns);
                }
                if(lastMap != Server.MapName)
                {
                    getSpawns(ref _spawns);
                }
                else
                {
                    Logging.LogMessage("Map still : " +  lastMap);  
                }
                return _spawns;
            }
            set { _spawns = value; }
        }
        /// <summary>
        /// Read out the map based spawns and store them in a dictionary
        /// </summary>
        /// <param name="spawns">dictionary to store the spawns in</param>
        private static void getSpawns(ref Dictionary<byte,List<Position>> spawns)
        {
            Logging.LogMessage("Getting spawns");
            _spawns!.Clear();
            _spawns = new Dictionary<byte, List<Position>>();
            _spawns.Add((byte)CsTeam.CounterTerrorist, new List<Position>());
            _spawns.Add((byte)CsTeam.Terrorist, new List<Position>());
            var spawnsct = Utilities.FindAllEntitiesByDesignerName<SpawnPoint>("info_player_counterterrorist");
            int minprio = 1;
            foreach( var spawn in spawnsct )
            {
                if( spawn.IsValid && spawn.Enabled && spawn.Priority < minprio )
                {
                    minprio = spawn.Priority;
                }
            }
            foreach (var spawn in spawnsct)
            {

                if (spawn.IsValid && spawn.Enabled && spawn.Priority == minprio)
                {
                    spawns[(byte)CsTeam.CounterTerrorist].Add(new Position(spawn.CBodyComponent!.SceneNode!.AbsOrigin, spawn.CBodyComponent.SceneNode.AbsRotation));
                }
            }
            var spawnst = Utilities.FindAllEntitiesByDesignerName<SpawnPoint>("info_player_terrorist");
            minprio = 1;
            foreach (var spawn in spawnst)
            {
                if (spawn.IsValid && spawn.Enabled && spawn.Priority < minprio)
                {
                    minprio = spawn.Priority;
                }
            }
            foreach (var spawn in spawnst)
            {
                if (spawn.IsValid && spawn.Enabled && spawn.Priority == minprio)
                {
                    spawns[(byte)CsTeam.Terrorist].Add(new Position(spawn.CBodyComponent!.SceneNode!.AbsOrigin, spawn.CBodyComponent.SceneNode.AbsRotation));
                }
            }
        }

        public static void TeleportToSpawn(CCSPlayerController? player, string args)
        {         
            TeleportToTeamSpawn(player, args);
        }

        public static void TeleportToTeamSpawn(CCSPlayerController? player,string args , CsTeam csTeam = CsTeam.None)
        {
            if (player == null) return;
            if (!player.PlayerPawn.IsValid) return;

            int number = -1;

            try
            {
                number = Convert.ToInt32(args);
                number--;
            }
            catch (Exception ex)
            {
                Logging.LogMessage($"{ex.Message}");
                player.PrintToCenter("invalid parameter");
                return;
            }
            CsTeam targetTeam = csTeam == CsTeam.None ? (CsTeam)player.TeamNum : csTeam;
            if (SpawnManager.Spawns[(byte)targetTeam].Count <= number)
            {
                player.PrintToCenter($"insufficient number of spawns found. spawns {SpawnManager.Spawns[(byte)targetTeam].Count} - {number}");
                return;
            }
            Logging.LogMessage($"teleport to: {SpawnManager.Spawns[(byte)targetTeam][number].PlayerPosition}");
            Utils.RemoveNoClip(player);
            player.PlayerPawn.Value.Teleport(SpawnManager.Spawns[(byte)targetTeam][number].PlayerPosition, SpawnManager.Spawns[(byte)targetTeam][number].PlayerAngle, new Vector(0, 0, 0));
            
            player.PrintToCenter($"Teleporting to spawn {number + 1}");
        }

        public static void ClearUsedSpawns()
        {
            spawnPointStorage.ResetUsedSpawns();
        }

        public static bool TeleportToUnusedSpawn(CCSPlayerController player,string bombsite)
        {
            if (player == null || !player.IsValid || player.IsBot) return false;

            JsonSpawnPoint? unusedSpawn = spawnPointStorage!.GetUnusedSpawnPointFromTeam(player.GetCsTeam(),bombsite);
            if (unusedSpawn == null)
            {
                return false;
            }
            player.PlayerPawn.Value!.Teleport(unusedSpawn.Position.ToCSVector(),unusedSpawn.QAngle.ToCSQAngle(),new Vector(0, 0, 0));         
            return true;
        }

        public static void AddCurrentPositionAsSpawnPoint(CCSPlayerController player, string Bombsite)
        {
            if (player == null || !player.IsValid) return;

            spawnPointStorage!.AddSpawnPoint(new JsonSpawnPoint(player.PlayerPawn.Value.CBodyComponent!.SceneNode!.AbsOrigin.ToVector3(), player.PlayerPawn.Value!.EyeAngles.ToVector3(),Bombsite),player.GetCsTeam());
        }

        public static void LoadSpawnsForBombsite(string Bombsite)
        {
            if(Bombsite == null) return;
            Server.PrintToConsole("Check if spawnpoints exist");
            if(spawnPointStorage!.GetSpawnPoints() == null) { Server.PrintToConsole("GetSpawnPoints returned null"); return; }
            Server.PrintToConsole("GetSpawnPoints returned NOT null");
            Dictionary<CsTeam, List<JsonSpawnPoint>> spawns = spawnPointStorage!.GetSpawnPoints();
            if(spawns != null) 
            {
                foreach(CsTeam team in spawns.Keys)
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
                if(tcount == 0 || ctcount == 0)
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

        private static void CreateSpawnPointsFromJsonPoints(List<JsonSpawnPoint> jsonSpawnPoints,string entityName,string Bombsite)
        {
            foreach (JsonSpawnPoint point in jsonSpawnPoints)
            {
                if(point.Bombsite != Bombsite) continue;
                Server.PrintToConsole("Found spawnpoint for bombsite");
                SpawnPoint? sp = Utilities.CreateEntityByName<SpawnPoint>("spawnpoint");
                if (sp == null) continue;
                Server.PrintToConsole("SpawnPoint not null");
                Vector absOrig = sp.AbsOrigin!;
                absOrig = point.Position.ToCSVector();
                QAngle eyeAngle = sp.AbsRotation!;
                eyeAngle = point.QAngle.ToCSQAngle();
                sp.TeamNum = (int)CsTeam.Terrorist;
                sp.Priority = 0;
                sp.DispatchSpawn();
            }
        }

    }
}
