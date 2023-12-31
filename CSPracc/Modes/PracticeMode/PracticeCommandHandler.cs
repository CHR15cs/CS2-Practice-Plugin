﻿using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CounterStrikeSharp.API.Modules.Menu;
using CounterStrikeSharp.API.Modules.Utils;
using CounterStrikeSharp.API;
using CSPracc.DataModules.Constants;
using CSPracc.Managers;
using CSPracc.DataModules;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Memory;
using CSPracc.Modes;
using CounterStrikeSharp.API.Modules.Memory.DynamicFunctions;

namespace CSPracc.CommandHandler
{
    public class PracticeCommandHandler : BaseCommandHandler
    {

        Dictionary<CCSPlayerController, Position> checkpoints = new Dictionary<CCSPlayerController, Position>();
        BotManager BotManager {  get; set; }
        PracticeMode PracticeMode { get; set; }
        public PracticeCommandHandler(PracticeMode mode): base(mode)
        {
            PracticeMode = mode;
            BotManager = new BotManager();
            CSPraccPlugin.Instance.AddCommand("css_pracc_smokecolor_enabled", "Enables / disabled smoke color", PraccSmokecolorEnabled);
        }

        public static bool PraccSmokeColorEnabled = true;
        private void PraccSmokecolorEnabled(CCSPlayerController? player, CommandInfo command)
        {
            if (command == null)
            {
                return;
            }
            if(command.ArgString.Length == 0)
            {
                Server.PrintToConsole("PraccSmokecolorEnabled used with invalid args");
            }
            if(command.ArgString.ToLower() == "true" || command.ArgString.ToLower() == "1")
            {
                PraccSmokeColorEnabled=true;
            }
            if (command.ArgString.ToLower() == "false" || command.ArgString.ToLower() == "0")
            {
                PraccSmokeColorEnabled = false;
            }
            return;
        }

        public override bool PlayerChat(EventPlayerChat @event, GameEventInfo info)
        {
            if (!CheckAndGetCommand(@event.Userid, @event.Text, out string command, out string args, out CCSPlayerController player))
            {
                return false;
            }
            switch (command)
            {
                case PRACC_COMMAND.SPAWN:
                    {
                        SpawnManager.TeleportToSpawn(player, args);
                        break;
                    }
                case PRACC_COMMAND.TSPAWN:
                    {
                        SpawnManager.TeleportToTeamSpawn(player, args, CsTeam.Terrorist);
                        break;
                    }
                case PRACC_COMMAND.CTSPAWN:
                    {
                        SpawnManager.TeleportToTeamSpawn(player, args, CsTeam.CounterTerrorist);
                        break;
                    }
                case PRACC_COMMAND.NADES:
                    {
                        if (args.Length > 0)
                        {
                            if (int.TryParse(args, out int id))
                            {
                                ProjectileManager.Instance.RestoreSnapshot(player, id);
                                break;
                            }
                            player.PrintToCenter("Could not parse argument for nade menu");
                        }
                        PracticeMode.ShowNadeMenu(player);
                        break;
                    }
                case PRACC_COMMAND.SAVE:
                    {
                        ProjectileManager.Instance.SaveSnapshot(player, args);
                        break;
                    }
                case PRACC_COMMAND.REMOVE:
                    {
                        ProjectileManager.Instance.RemoveSnapshot(player, args);
                        break;
                    }
                case PRACC_COMMAND.BOT:
                    {
                        BotManager.AddBot(player);
                        break;
                    }
                case PRACC_COMMAND.BOOST:
                    {
                        BotManager.Boost(player);
                        break;
                    }
                case PRACC_COMMAND.NOBOT:
                    {
                        BotManager.NoBot(player);
                        break;
                    }
                case PRACC_COMMAND.CLEARBOTS:
                    {
                        BotManager.ClearBots(player);
                        break;
                    }
                case PRACC_COMMAND.WATCHME:
                    {
                        //TODO: Utilities.GetPlayers?
                        var playerEntities = Utilities.FindAllEntitiesByDesignerName<CCSPlayerController>("cs_player_controller");
                        foreach (var playerEnt in playerEntities)
                        {
                            if (playerEnt == null) continue;
                            if (!playerEnt.IsValid) continue;
                            if (playerEnt.UserId == player.UserId) continue;
                            if (playerEnt.IsBot) continue;
                            playerEnt.ChangeTeam(CsTeam.Spectator);
                            Logging.LogMessage($"Switching {playerEnt.PlayerName} to spectator");
                        }
                        break;
                    }
                case PRACC_COMMAND.CROUCHBOT:
                    {
                        BotManager.CrouchBot(player);
                        break;
                    }
                case PRACC_COMMAND.CROUCHBOOST:
                    {
                        BotManager.CrouchingBoostBot(player);
                        break;
                    }
                case PRACC_COMMAND.GOT:
                    {
                        player.ChangeTeam(CsTeam.Terrorist);
                        break;
                    }
                case PRACC_COMMAND.GOCT:
                    {
                        player.ChangeTeam(CsTeam.CounterTerrorist);
                        break;
                    }
                case PRACC_COMMAND.GOSPEC:
                    {
                        player.ChangeTeam(CsTeam.Spectator);
                        break;
                    }
                case PRACC_COMMAND.CLEAR:
                    {
                        Utils.RemoveGrenadeEntitiesFromPlayer(player);
                        break;
                    }
                case PRACC_COMMAND.ClearAll:
                    {
                        Utils.RemoveGrenadeEntities();
                        break;
                    }
                case PRACC_COMMAND.SAVELAST:
                    {
                        ProjectileManager.Instance.SaveLastGrenade(player, args);
                        break;
                    }
                case PRACC_COMMAND.CHECKPOINT:
                    {
                        if(!checkpoints.ContainsKey(player))
                        {
                            checkpoints.Add(player, player.GetCurrentPosition());
                            player.PrintToCenter("Saved current checkpoint");
                        }
                        else
                        {
                            checkpoints[player] = player.GetCurrentPosition();
                            player.PrintToCenter("Saved current checkpoint");
                        }
                        break;
                    }
                case PRACC_COMMAND.BACK:
                    {
                        if (!checkpoints.ContainsKey(player))
                        {
                            player.PrintToCenter($"You dont have a saved checkpoint");
                        }
                        else
                        {
                            player.PlayerPawn.Value!.Teleport(checkpoints[player].PlayerPosition, checkpoints[player].PlayerAngle,new Vector(0,0,0));
                            player.PrintToCenter("Teleported to your checkpoint");
                        }
                        break;
                    }
                case PRACC_COMMAND.timer:
                    {
                        PracticeMode.StartTimer(player);
                        break;
                    }
                //case ".throw":
                //    {

                //        //               MemoryFunctionVoid<IntPtr, string, IntPtr, IntPtr, string, int> AcceptEntityInput =
                //        // new(@"\x55\x48\x89\xE5\x41\x57\x49\x89\xFF\x41\x56\x48\x8D\x7D\xC0");


                //        Server.PrintToConsole("creating smoke");
                //        CChicken? smoke = Utilities.CreateEntityByName<CChicken>("chicken");
                //        if(smoke == null)
                //        {
                //            Server.PrintToConsole("smoke is  null");
                //        }
                //        smoke.Teleport(player.PlayerPawn.Value.CBodyComponent.SceneNode.AbsOrigin, new QAngle(0, 0, 0), new Vector(0, 0, 0));
                //        //smoke.TeamNum = player.TeamNum;
                //        smoke.DispatchSpawn();
                //        //AcceptEntityInput.Invoke(smoke.Handle, "InitializeSpawnFromWorld", player.Handle, player.Handle, "", 0);
                //        //AcceptEntityInput.Invoke(smoke.Handle,"FireUser1",player.Handle,player.Handle,"",0);
                //        break;
                //    }
                default:
                    {
                        base.PlayerChat(@event, info);
                        return false;
                    }
            }
            return true;
        }

        public override void Dispose()
        {
            CSPraccPlugin.Instance.RemoveCommand("css_pracc_smokecolor_enabled", PraccSmokecolorEnabled);
            base.Dispose();
        }
    }
}
