using CounterStrikeSharp.API.Core.Attributes.Registration;
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
using CounterStrikeSharp.API.Modules.Memory;

namespace CSPracc.CommandHandler
{
    public class PracticeCommandHandler : BaseCommandHandler
    {

        Dictionary<CCSPlayerController, Position> checkpoints = new Dictionary<CCSPlayerController, Position>();
        BotManager BotManager {  get; set; }
        public PracticeCommandHandler(): base()
        {
            BotManager = new BotManager();
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
                        ChatMenus.OpenMenu(player, ProjectileManager.Instance.GetNadeMenu(player));
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
                            player.PlayerPawn.Value.Teleport(checkpoints[player].PlayerPosition, checkpoints[player].PlayerAngle,new Vector(0,0,0));
                            player.PrintToCenter("Teleported to your checkpoint");
                        }
                        break;
                    }
                case ".throw":
                    {
                        CSmokeGrenadeProjectile? ent = Utilities.CreateEntityByName<CSmokeGrenadeProjectile>("smokegrenade_projectile");
                        if (ent != null)
                        {
                            ent!.Teleport(player.PlayerPawn.Value.CBodyComponent!.SceneNode!.AbsOrigin, new QAngle(0, 0, 0), new Vector(0, 0, 0));
                            ent.DispatchSpawn();

                        }
                        else
                        {
                            Server.PrintToChatAll("entity is null");
                        }

                        //player.PlayerPawn.Value.CommitSuicide(true, false);
                        //var addEntiryInput = VirtualFunction.CreateVoid<IntPtr, string,IntPtr,IntPtr,string,IntPtr>(GameData.GetSignature("CEntityInstance_AcceptInput"));
                        //var addEntityEvent = VirtualFunction.CreateVoid<IntPtr, string, IntPtr, IntPtr, string, float>(GameData.GetSignature("CEntitySystem_AddEntityIOEvent"));
                        //addEntiryInput(ent.Handle, "globalname", player.Handle, player.Handle, "custom", 0);
                        //addEntityEvent(ent.Handle, "FireUser1", player.Handle, player.Handle, "", 0f);
                        break;
                    }
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
            base.Dispose();
        }
    }
}
