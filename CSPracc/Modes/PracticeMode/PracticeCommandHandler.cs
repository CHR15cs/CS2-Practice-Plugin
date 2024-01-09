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
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Memory;
using CSPracc.Modes;
using CounterStrikeSharp.API.Modules.Memory.DynamicFunctions;

namespace CSPracc.CommandHandler
{
    public class PracticeCommandHandler : BaseCommandHandler
    {

        Dictionary<CCSPlayerController, Position> checkpoints = new Dictionary<CCSPlayerController, Position>();
        PracticeBotManager BotManager {  get; set; }
        PracticeMode PracticeMode { get; set; }

        ProjectileManager ProjectileManager { get; set; }
        public PracticeCommandHandler(PracticeMode mode,ref ProjectileManager projectileManager, ref PracticeBotManager botManager): base(mode)
        {
            PracticeMode = mode;
            BotManager = botManager;
            ProjectileManager =  projectileManager;
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
                                ProjectileManager.RestoreSnapshot(player, id);
                                break;
                            }
                            player.PrintToCenter("Could not parse argument for nade menu");
                        }
                        PracticeMode.ShowNadeMenu(player);
                        break;
                    }
                case PRACC_COMMAND.SAVE:
                    {
                        ProjectileManager.SaveSnapshot(player, args);
                        break;
                    }
                case PRACC_COMMAND.REMOVE:
                    {
                        ProjectileManager.RemoveSnapshot(player, args);
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
                        ProjectileManager.SaveLastGrenade(player, args);
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
                case PRACC_COMMAND.rethrow:
                    {                                              
                        ProjectileManager.ReThrow(player);
                        break;
                    }
                case PRACC_COMMAND.flash:
                    {       
                        ProjectileManager.Flash(player);
                        break;
                    }
                case PRACC_COMMAND.noflash:
                    {
                        ProjectileManager.NoFlash(player);
                        break;
                    }
                case PRACC_COMMAND.stop:
                    {
                        ProjectileManager.Stop(player);
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
            CSPraccPlugin.Instance.RemoveCommand("css_pracc_smokecolor_enabled", PraccSmokecolorEnabled);
            base.Dispose();
        }

        public override void PrintHelp(CCSPlayerController? player)
        {
            base.PrintHelp(player);
            List<string> message = new List<string>();
            message.Add($" {ChatColors.Green}{PRACC_COMMAND.SPAWN}{ChatColors.Red} 'number'{ChatColors.White}. Teleports you to the given spawn of your current team.");
            message.Add($" {ChatColors.Green}{PRACC_COMMAND.TSPAWN}{ChatColors.White}{ChatColors.Red} 'number'{ChatColors.White}. Teleports you to the given spawn of the terrorist team.");
            message.Add($" {ChatColors.Green}{PRACC_COMMAND.CTSPAWN}{ChatColors.White}{ChatColors.Red} 'number'{ChatColors.White}. Teleports you to the given spawn of the counter-terrorist team.");
            message.Add($" {ChatColors.Green}{PRACC_COMMAND.NADES}{ChatColors.White}{ChatColors.Blue} [id]{ChatColors.White}. If id is passed an available. Teleport to given nade lineup. Else open nade menu.");
            message.Add($" {ChatColors.Green}{PRACC_COMMAND.SAVE}{ChatColors.White}{ChatColors.Red} 'name'{ChatColors.White}. Saves current lineup.");
            message.Add($" {ChatColors.Green}{PRACC_COMMAND.SAVELAST}{ChatColors.White}{ChatColors.Red} 'name'{ChatColors.White}. Saves  lineup of last thrown nade.");
            message.Add($" {ChatColors.Green}{PRACC_COMMAND.rethrow}{ChatColors.White} Rethrows your last grenade.");
            message.Add($" {ChatColors.Green}{PRACC_COMMAND.BOT}{ChatColors.White} Spawns bot at your current location.");
            message.Add($" {ChatColors.Green}{PRACC_COMMAND.BOOST}{ChatColors.White} Spawns bot at your current location and teleports you ontop.");
            message.Add($" {ChatColors.Green}{PRACC_COMMAND.CROUCHBOT}{ChatColors.White} Spawns a croucing bot at your current location.");
            message.Add($" {ChatColors.Green}{PRACC_COMMAND.CROUCHBOOST}{ChatColors.White} Spawns a croucing bot at your current location and teleports you ontop.");
            message.Add($" {ChatColors.Green}{PRACC_COMMAND.NOBOT}{ChatColors.White} Removes closest bot to your location that you spawned.");
            message.Add($" {ChatColors.Green}{PRACC_COMMAND.CLEARBOTS}{ChatColors.White} Clears all of your bots.");
            message.Add($" {ChatColors.Green}{PRACC_COMMAND.CLEAR}{ChatColors.White} Clear your smokes / molotovs.");
            message.Add($" {ChatColors.Green}{PRACC_COMMAND.ClearAll}{ChatColors.White} Clear all smokes / molotovs on the server");
            message.Add($" {ChatColors.Green}{PRACC_COMMAND.flash}{ChatColors.White} Save current position, if flash is thrown, then you will be teleported back to the position.");
            message.Add($" {ChatColors.Green}{PRACC_COMMAND.stop}{ChatColors.White} Stop the flash command.");
            message.Add($" {ChatColors.Green}{PRACC_COMMAND.noflash}{ChatColors.White} Removing flash effect.");
            message.Add($" {ChatColors.Green}{PRACC_COMMAND.timer}{ChatColors.White} Toggle Timer. Use command again to stop timer.");
            message.Add($" {ChatColors.Green}{PRACC_COMMAND.CHECKPOINT}{ChatColors.White} Save current position as checkpoint. Use .back to return.");
            message.Add($" {ChatColors.Green}{PRACC_COMMAND.BACK}{ChatColors.White} Return to saved checkpoint.");
            message.Add($" {ChatColors.Green}{PRACC_COMMAND.WATCHME}{ChatColors.White} Moves all players except you to spectator.");

            foreach (string s in message)
            {
                player?.PrintToChat(s);
            }
        }
    }
}
