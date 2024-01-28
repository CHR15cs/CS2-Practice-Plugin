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
using Microsoft.Extensions.Logging;

namespace CSPracc
{
    public  class SpawnManager
    {
        private  SpawnPointStorage? _spawnPointStorage;
        private  SpawnPointStorage? spawnPointStorage
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
        private  string lastMap = String.Empty;
        private  Dictionary<byte, List<Position>>? _spawns = null;
        /// <summary>
        /// Dictionary to store the spawns of the current map in
        /// </summary>
        public  Dictionary<byte,List<Position>> Spawns
        {
            get
            {
                if(_spawns == null)
                {
                    _spawns = new Dictionary<byte, List<Position>>();
                    if(lastMap == String.Empty)
                    {
                        lastMap = Server.MapName;
                    }
                    getSpawns(ref _spawns);
                }
                if(lastMap != Server.MapName)
                {
                    getSpawns(ref _spawns);
                }
                return _spawns;
            }
            set { _spawns = value; }
        }
        /// <summary>
        /// Read out the map based spawns and store them in a dictionary
        /// </summary>
        /// <param name="spawns">dictionary to store the spawns in</param>
        private void getSpawns(ref Dictionary<byte,List<Position>> spawns)
        {
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

        public void TeleportToSpawn(CCSPlayerController? player, string args)
        {         
            TeleportToTeamSpawn(player, args);
        }

        public void TeleportToTeamSpawn(CCSPlayerController? player,string args , CsTeam csTeam = CsTeam.None)
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
                CSPraccPlugin.Instance!.Logger.LogError($"{ex.Message}");
                player.PrintToCenter("invalid parameter");
                return;
            }
            CsTeam targetTeam = csTeam == CsTeam.None ? (CsTeam)player.TeamNum : csTeam;
            if (Spawns[(byte)targetTeam].Count <= number)
            {
                player.PrintToCenter($"insufficient number of spawns found. spawns {Spawns[(byte)targetTeam].Count} - {number}");
                return;
            }
            CSPraccPlugin.Instance.Logger.LogInformation($"teleport to: {Spawns[(byte)targetTeam][number].PlayerPosition}");
            Utils.RemoveNoClip(player);
            player.PlayerPawn.Value.Teleport(Spawns[(byte)targetTeam][number].PlayerPosition,Spawns[(byte)targetTeam][number].PlayerAngle, new Vector(0, 0, 0));
            
            player.PrintToCenter($"Teleporting to spawn {number + 1}");
        }

        public void ClearUsedSpawns()
        {
            spawnPointStorage.ResetUsedSpawns();
        }

        public void TeleportToBestSpawn(CCSPlayerController player)
        {
            if (player == null || !player.IsValid || player.IsBot) return;

            List<Position> points = Spawns[player.TeamNum];
            float closestDistance = absolutDistance(player,points.FirstOrDefault());
            int closestIndex = 0;
            for(int i = 0; i < points.Count; i++) 
            {
               float maybeCloser = absolutDistance(player, points[i]); 
                if(maybeCloser< closestDistance ) 
                { 
                    closestIndex = i;
                    closestDistance = maybeCloser;
                    continue;
                }
            }
            player.TeleportToPosition(points[closestIndex]);
        }

        public void TeleportToWorstSpawn(CCSPlayerController player)
        {
            if (player == null || !player.IsValid || player.IsBot) return;

            List<Position> points = Spawns[player.TeamNum];
            float maxDistance = absolutDistance(player, points.FirstOrDefault());
            int maxIndex = 0;
            for (int i = 0; i < points.Count; i++)
            {
                float maybeCloser = absolutDistance(player, points[i]);
                if (maybeCloser > maxDistance)
                {
                    maxIndex = i;
                    maxDistance = maybeCloser;
                    continue;
                }
            }
            player.TeleportToPosition(points[maxIndex]);
        }

        private float absolutDistance(CCSPlayerController player, Position spawnPoint)
        {
            float distanceX = 0;
            float distanceY = 0;
            float distanceZ = 0;
            Vector playerPos = player.PlayerPawn!.Value.CBodyComponent!.SceneNode!.AbsOrigin;
            Vector botPos = spawnPoint.PlayerPosition;
            distanceX = playerPos.X - botPos.X;
            distanceY = playerPos.Y - botPos.Y;
            distanceZ = playerPos.Z - botPos.Z;
            if (distanceX < 0)
            {
                distanceX *= -1;
            }
            if (distanceY < 0)
            {
                distanceY *= -1;
            }
            if (distanceZ < 0)
            {
                distanceZ *= -1;
            }
            CSPraccPlugin.Instance!.Logger.LogInformation($"calculating distance {distanceX + distanceY + distanceZ}");
            return distanceX + distanceY + distanceZ;
        }

        public bool TeleportToUnusedSpawn(CCSPlayerController player,string bombsite)
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

        public void AddCurrentPositionAsSpawnPoint(CCSPlayerController player, string Bombsite)
        {
            if (player == null || !player.IsValid) return;

            spawnPointStorage!.AddSpawnPoint(new JsonSpawnPoint(player.PlayerPawn.Value.CBodyComponent!.SceneNode!.AbsOrigin.ToVector3(), player.PlayerPawn.Value!.EyeAngles.ToVector3(),Bombsite),player.GetCsTeam());
        }

        public void LoadSpawnsForBombsite(string Bombsite)
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

        private void CreateSpawnPointsFromJsonPoints(List<JsonSpawnPoint> jsonSpawnPoints,string entityName,string Bombsite)
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
