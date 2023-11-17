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

namespace CSPracc
{
    public static class SpawnManager
    {
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
            foreach (var spawn in spawnsct)
            {

                if (spawn.IsValid && spawn.Enabled && spawn.Priority == 0)
                {
                    spawns[(byte)CsTeam.CounterTerrorist].Add(new Position(spawn.CBodyComponent!.SceneNode!.AbsOrigin, spawn.CBodyComponent.SceneNode.AbsRotation));
                }
            }
            var spawnst = Utilities.FindAllEntitiesByDesignerName<SpawnPoint>("info_player_terrorist");
            foreach (var spawn in spawnst)
            {
                if (spawn.IsValid && spawn.Enabled && spawn.Priority == 0)
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
            player.PlayerPawn.Value.Teleport(SpawnManager.Spawns[(byte)targetTeam][number].PlayerPosition, SpawnManager.Spawns[(byte)targetTeam][number].PlayerAngle, new Vector(0, 0, 0));
            player.PrintToCenter($"Teleporting to spawn {number + 1}");
        }

    }
}
