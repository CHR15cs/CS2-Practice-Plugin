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
using CSPracc.Managers;
using CSPracc.DataModules.Constants;
using CSPracc.Managers.BaseManagers;

namespace CSPracc
{
    public  class PracticeSpawnManager : BaseManager
    {
        public PracticeSpawnManager(ref CommandManager cmdManager) : base(ref cmdManager)
        {
            CommandManager = cmdManager;
            Commands.Add(PRACC_COMMAND.SPAWN, new PlayerCommand(PRACC_COMMAND.SPAWN, "Teleport user to given spawn", CommandHandlerSpawn, null));
            Commands.Add(PRACC_COMMAND.TSPAWN, new PlayerCommand(PRACC_COMMAND.TSPAWN, "Teleport user to given tspawn", CommandHandlerTSpawn, null));
            Commands.Add(PRACC_COMMAND.CTSPAWN, new PlayerCommand(PRACC_COMMAND.CTSPAWN, "Teleport user to given ctspawn", CommandHandlerCTSpawn, null));
            Commands.Add(PRACC_COMMAND.bestspawn, new PlayerCommand(PRACC_COMMAND.bestspawn, "Teleport user to closest spawn of your current team", CommandHandlerCTSpawn, null));
            Commands.Add(PRACC_COMMAND.worstspawn, new PlayerCommand(PRACC_COMMAND.worstspawn, "Teleport user to farest spawn of your current team", CommandHandlerCTSpawn, null));
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

        private bool CommandHandlerSpawn(CCSPlayerController player, List<string> args)
        {         
            return TeleportToTeamSpawn(player, args);
        }

        private bool CommandHandlerTSpawn(CCSPlayerController player, List<string> args)
        {
            return TeleportToTeamSpawn(player, args,CsTeam.Terrorist);
        }

        private bool CommandHandlerCTSpawn(CCSPlayerController player, List<string> args)
        {
            return TeleportToTeamSpawn(player, args, CsTeam.CounterTerrorist);
        }

        private bool CommandHandlerBestSpawn(CCSPlayerController player, List<string> args)
        {
            return TeleportToBestSpawn(player);
        }

        private bool CommandHandlerWorstSpawn(CCSPlayerController player, List<string> args)
        {
            return TeleportToWorstSpawn(player);
        }

        private bool TeleportToTeamSpawn(CCSPlayerController? player,List<string> args , CsTeam csTeam = CsTeam.None)
        {
            Server.PrintToChatAll($"{String.Join(" ", args)}");

            int number = -1;

            try
            {
                number = Convert.ToInt32(args[0]);
                number--;
            }
            catch (Exception ex)
            {
                CSPraccPlugin.Instance!.Logger.LogError($"{ex.Message}");
                player.PrintToCenter("invalid parameter");
                return false;
            }
            CsTeam targetTeam = csTeam == CsTeam.None ? (CsTeam)player.TeamNum : csTeam;
            if (Spawns[(byte)targetTeam].Count <= number)
            {
                player.PrintToCenter($"insufficient number of spawns found. spawns {Spawns[(byte)targetTeam].Count} - {number}");
                return false;
            }
            CSPraccPlugin.Instance.Logger.LogInformation($"teleport to: {Spawns[(byte)targetTeam][number].PlayerPosition}");
            Utils.RemoveNoClip(player);
            player.PlayerPawn.Value.Teleport(Spawns[(byte)targetTeam][number].PlayerPosition,Spawns[(byte)targetTeam][number].PlayerAngle, new Vector(0, 0, 0));
            
            player.PrintToCenter($"Teleporting to spawn {number + 1}");
            return true;
        }


        private bool TeleportToBestSpawn(CCSPlayerController player)
        {

            List<Position> points = Spawns[player.TeamNum];
            float closestDistance = absolutDistance(player,points.FirstOrDefault()!);
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
            return true;
        }

        public bool TeleportToWorstSpawn(CCSPlayerController player)
        {
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
            return true;
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
    }
}
