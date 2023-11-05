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

namespace CSPracc
{
    public static class SpawnManager
    {
        private static string lastMap = String.Empty;
        private static Dictionary<byte, List<Position>> _spawns = null;
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
                    }
                    getSpawns(ref _spawns);
                }
                if(lastMap != Server.MapName)
                {
                    getSpawns(ref _spawns);
                }
                return _spawns;
            }
        }
        /// <summary>
        /// Read out the map based spawns and store them in a dictionary
        /// </summary>
        /// <param name="spawns">dictionary to store the spawns in</param>
        private static void getSpawns(ref Dictionary<byte,List<Position>> spawns)
        {          
            spawns = new Dictionary<byte, List<Position>>();
            spawns.Add((byte)CsTeam.CounterTerrorist, new List<Position>());
            spawns.Add((byte)CsTeam.Terrorist, new List<Position>());
            var spawnsct = Utilities.FindAllEntitiesByDesignerName<CBaseEntity>("info_player_counterterrorist");
            foreach (var spawn in spawnsct)
            {
                if (spawn.IsValid)
                {
                    spawns[(byte)CsTeam.CounterTerrorist].Add(new Position(spawn.CBodyComponent!.SceneNode!.AbsOrigin, spawn.CBodyComponent.SceneNode.AbsRotation));
                }
            }
            var spawnst = Utilities.FindAllEntitiesByDesignerName<CBaseEntity>("info_player_terrorist");
            foreach (var spawn in spawnst)
            {
                if (spawn.IsValid)
                {
                    spawns[(byte)CsTeam.Terrorist].Add(new Position(spawn.CBodyComponent!.SceneNode!.AbsOrigin, spawn.CBodyComponent.SceneNode.AbsRotation));
                }
            }
        }

        public static void TeleportToSpawn(CCSPlayerController? player, string args)
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
                player.PrintToCenter("invalid parameter");
                return;
            }

            if (SpawnManager.Spawns[player.TeamNum].Count <= number)
            {
                player.PrintToCenter($"insufficient number of spawns found. spawns {SpawnManager.Spawns[player.TeamNum].Count} - {number}");
                return;
            }
            player.PlayerPawn.Value.Teleport(SpawnManager.Spawns[player.TeamNum][number].PlayerPosition, SpawnManager.Spawns[player.TeamNum][number].PlayerAngle, new Vector(0, 0, 0));
            player.PrintToCenter($"Teleporting to spawn {number + 1}");
        }
    }
}
