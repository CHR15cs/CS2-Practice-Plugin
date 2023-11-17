using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Utils;
using CSPracc.DataModules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CounterStrikeSharp.API.Modules.Timers;
using CSPracc.DataModules.Constants;
using CounterStrikeSharp.API.Modules.Entities;
using System.Net.Http.Headers;
using CounterStrikeSharp.API.Modules.Memory;
using System.Numerics;
using Vector = CounterStrikeSharp.API.Modules.Utils.Vector;

namespace CSPracc.Managers
{
    public  class BotManager
    {
        /// <summary>
        /// Dict of a bots Key = userid of bot
        /// </summary>
        private static Dictionary<int, Dictionary<string, object>> spawnedBots = new Dictionary<int, Dictionary<string, object>>();

        /// <summary>
        /// Following code is heavily inspired by https://github.com/shobhit-pathak/MatchZy/blob/main/PracticeMode.cs
        /// </summary>
        /// <param name="player">play who added the bot</param>
        public static void AddBot(CCSPlayerController player,bool crouch = false)
        {
            if (player.TeamNum == (byte)CsTeam.Terrorist)
            {
                Server.ExecuteCommand("bot_join_team T");
                Server.ExecuteCommand("bot_add_t");
            }
            else if (player.TeamNum == (byte)CsTeam.CounterTerrorist)
            {
                Server.ExecuteCommand("bot_join_team CT");
                Server.ExecuteCommand("bot_add_ct");
            }

            // Adding a small timer so that bot can be added in the world
            // Once bot is added, we teleport it to the requested position
            CSPraccPlugin.Instance!.AddTimer(0.1f, () => SpawnBot(player,crouch));
            Server.ExecuteCommand("bot_stop 1");
            Server.ExecuteCommand("bot_freeze 1");
            Server.ExecuteCommand("bot_zombie 1");            
        }

        /// <summary>
        /// Boost player onto bot
        /// </summary>
        /// <param name="player">player called the command</param>
        public static void Boost(CCSPlayerController player)
        {
            AddBot(player);
            Logging.LogMessage("Elevating player");
           CSPraccPlugin.Instance!.AddTimer(0.2f, () => ElevatePlayer(player));
        }

        /// <summary>
        /// Spawn a crouching bot
        /// </summary>
        /// <param name="player">player called the command</param>
        public static void CrouchBot(CCSPlayerController player)
        {
            AddBot(player,true);
        }

        /// <summary>
        /// Boost ontop of crouching bot
        /// </summary>
        /// <param name="player">player called the command</param>
        public static void CrouchingBoostBot(CCSPlayerController player)
        {
            AddBot(player, true);
            CSPraccPlugin.Instance!.AddTimer(0.2f, () => ElevatePlayer(player));
        }

        /// <summary>
        /// Remove closest bot to the player
        /// </summary>
        /// <param name="player">player called the command</param>
        public static void NoBot(CCSPlayerController player)
        {
            CCSPlayerController? closestBot = null;
            float Distance = 0;
            foreach (Dictionary<string, object> botDict in spawnedBots.Values)
            {
                
                CCSPlayerController botOwner = (CCSPlayerController)botDict["owner"];
                CCSPlayerController bot = (CCSPlayerController)botDict["controller"];
                if(!bot.IsValid)
                {
                    continue;
                }
                if (botOwner.UserId == player.UserId)
                {
                    Logging.LogMessage("found bot of player");
                    if (closestBot == null)
                    {
                        closestBot = bot;
                        Distance = absolutDistance(botOwner, bot);
                    }
                    float tempDistance = absolutDistance(botOwner, bot);
                    if(tempDistance< Distance) 
                    {
                        Distance = tempDistance;
                        closestBot = bot;
                    }
                }
            }
            if(closestBot != null)
            {
                Logging.LogMessage($"kickid {closestBot.UserId}");
                Server.ExecuteCommand($"kickid {closestBot.UserId}");            
                spawnedBots.Remove((int)closestBot.UserId!);
            }
        }

        /// <summary>
        /// Calculate difference in coordinates between a player and a bot
        /// </summary>
        /// <param name="player">player</param>
        /// <param name="bot">bot</param>
        /// <returns>absolut distance x+y</returns>
        private static float absolutDistance(CCSPlayerController player, CCSPlayerController bot)
        {
            float distanceX = 0;
            float distanceY = 0;
            float distanceZ = 0;
            Vector playerPos = player.PlayerPawn!.Value.CBodyComponent!.SceneNode!.AbsOrigin;
            Vector botPos = bot!.PlayerPawn!.Value.CBodyComponent!.SceneNode!.AbsOrigin;
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
            Logging.LogMessage($"calculating distance {distanceX + distanceY + distanceZ}");
            return distanceX + distanceY + distanceZ;
        }

        /// <summary>
        /// Remove all bots of the player
        /// </summary>
        /// <param name="player"></param>
        public static void ClearBots(CCSPlayerController player)
        {
            foreach(Dictionary<string,object> botDict in spawnedBots.Values)
            {
                CCSPlayerController botOwner = (CCSPlayerController)botDict["owner"];
                CCSPlayerController bot = (CCSPlayerController)botDict["controller"];
                if (botOwner.UserId == player.UserId)
                {
                    Server.ExecuteCommand($"kickid {bot.UserId}");
                    spawnedBots.Remove((int)bot.UserId!);
                }
                
            }
        }



        private static void ElevatePlayer(CCSPlayerController player)
        {
                player.PlayerPawn.Value.Teleport(new Vector(player.PlayerPawn.Value.CBodyComponent!.SceneNode!.AbsOrigin.X, player.PlayerPawn.Value.CBodyComponent!.SceneNode!.AbsOrigin.Y, player.PlayerPawn.Value.CBodyComponent!.SceneNode!.AbsOrigin.Z + 80.0f), player.PlayerPawn.Value.EyeAngles, new Vector(0, 0, 0));
            Logging.LogMessage($"boosting player: {player.PlayerPawn.Value.CBodyComponent!.SceneNode!.AbsOrigin.X} - {player.PlayerPawn.Value.CBodyComponent!.SceneNode!.AbsOrigin.Y} - {player.PlayerPawn.Value.CBodyComponent!.SceneNode!.AbsOrigin.Z + 80.0f}");
        }

        private static void SpawnBot(CCSPlayerController botOwner, bool crouch = false)
        {
            var playerEntities = Utilities.FindAllEntitiesByDesignerName<CCSPlayerController>("cs_player_controller");
            bool unusedBotFound = false;
            foreach (var tempPlayer in playerEntities)
            {
                if (!tempPlayer.IsBot) continue;
                if (tempPlayer.UserId.HasValue)
                {
                    if (!spawnedBots.ContainsKey(tempPlayer.UserId.Value) && unusedBotFound)
                    {
                        Logging.LogMessage($"UNUSED BOT FOUND: {tempPlayer.UserId.Value} EXECUTING: kickid {tempPlayer.UserId.Value}");
                        // Kicking the unused bot. We have to do this because bot_add_t/bot_add_ct may add multiple bots but we need only 1, so we kick the remaining unused ones
                        Server.ExecuteCommand($"kickid {tempPlayer.UserId.Value}");
                        continue;
                    }
                    if (spawnedBots.ContainsKey(tempPlayer.UserId.Value))
                    {
                        continue;
                    }
                    else
                    {
                        spawnedBots[tempPlayer.UserId.Value] = new Dictionary<string, object>();
                    }
                    Vector pos = new Vector(botOwner.PlayerPawn.Value.CBodyComponent?.SceneNode?.AbsOrigin.X, botOwner.PlayerPawn.Value.CBodyComponent?.SceneNode?.AbsOrigin.Y, botOwner.PlayerPawn.Value.CBodyComponent?.SceneNode?.AbsOrigin.Z);
                    QAngle eyes = new QAngle(botOwner.PlayerPawn.Value.EyeAngles.X, botOwner.PlayerPawn.Value.EyeAngles.Y, botOwner.PlayerPawn.Value.EyeAngles.Z);
                    Position botOwnerPosition = new Position(pos, eyes);
                    // Add key-value pairs to the inner dictionary
                    spawnedBots[tempPlayer.UserId.Value]["controller"] = tempPlayer;
                    spawnedBots[tempPlayer.UserId.Value]["position"] = botOwnerPosition;
                    spawnedBots[tempPlayer.UserId.Value]["owner"] = botOwner;
                    spawnedBots[tempPlayer.UserId.Value]["crouchstate"] = crouch;
                    CCSPlayer_MovementServices movementService = new CCSPlayer_MovementServices(tempPlayer.PlayerPawn.Value.MovementServices!.Handle);
                    CCSBot bot = tempPlayer.PlayerPawn.Value.Bot!;
                    tempPlayer.PlayerPawn.Value.Teleport(botOwnerPosition.PlayerPosition, botOwnerPosition.PlayerAngle, new Vector(0, 0, 0));
                    if ((bool)spawnedBots[tempPlayer.UserId.Value]["crouchstate"])
                    {
                        CSPraccPlugin.Instance!.AddTimer(0.1f, () => movementService.DuckAmount = 1);
                        CSPraccPlugin.Instance!.AddTimer(0.2f, () => bot.IsCrouching = true);

                    }                 
                    unusedBotFound = true;
                }
            }
            if (!unusedBotFound)
            {
                Methods.MsgToServer($"Cannot add bots, the team is full! Use .nobots to remove the current bots.");
            }
        }

        public HookResult OnPlayerSpawn(EventPlayerSpawn @event, GameEventInfo info)
        {
            Logging.LogMessage("----OnPlayerSpawn BotManager---");
            var player = @event.Userid;

            // Respawing a bot where it was actually spawned during practice session
            if (player.IsValid && player.IsBot && player.UserId.HasValue)
            {
                if (spawnedBots.ContainsKey(player.UserId.Value))
                {
                    if (spawnedBots[player.UserId.Value]["position"] is Position botPosition)
                    {                   
                        CCSBot bot = player.PlayerPawn.Value.Bot!;
                        CCSPlayer_MovementServices movementService = new CCSPlayer_MovementServices(player.PlayerPawn.Value.MovementServices!.Handle);                   
                        player.PlayerPawn.Value.Teleport(botPosition.PlayerPosition, botPosition.PlayerAngle, new Vector(0, 0, 0));
                        if ((bool)spawnedBots[player.UserId.Value]["crouchstate"]) player.PlayerPawn.Value.Flags |= (uint)PlayerFlags.FL_DUCKING;
                        if ((bool)spawnedBots[player.UserId.Value]["crouchstate"])
                        {
                            CSPraccPlugin.Instance!.AddTimer(0.1f, () => movementService.DuckAmount = 1);
                            CSPraccPlugin.Instance!.AddTimer(0.2f, () => bot.IsCrouching = true);
                        }
                       
                    }
                }
            }
            return HookResult.Continue;
        }
    }
}
