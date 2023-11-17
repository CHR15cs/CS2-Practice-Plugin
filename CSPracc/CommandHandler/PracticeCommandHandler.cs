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

namespace CSPracc.CommandHandler
{
    public class PracticeCommandHandler : BaseCommandHandler
    {
       
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
                        ChatMenus.OpenMenu(player, ProjectileManager.GetNadeMenu(player));
                        break;
                    }
                case PRACC_COMMAND.SAVE:
                    {
                        ProjectileManager.AddGrenade(player, args);
                        break;
                    }
                case PRACC_COMMAND.REMOVE:
                    {
                        ProjectileManager.RemoveGrenade(player, args);
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
                        Utils.RemoveGrenadeEntities();
                        break;
                    }
                case PRACC_COMMAND.SAVELAST:
                    {
                        ProjectileManager.SaveLastGrenade(player, args);
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
