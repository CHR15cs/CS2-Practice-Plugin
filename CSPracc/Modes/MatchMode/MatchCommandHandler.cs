using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using CSPracc.DataModules.Constants;
using CSPracc.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSPracc.CommandHandler
{
    public class MatchCommandHandler : BaseCommandHandler
    {
        MatchMode MatchMode { get; init; }
        public MatchCommandHandler(MatchMode mode):base(mode) 
        {
            MatchMode = mode;
        }

        public override bool PlayerChat(EventPlayerChat @event, GameEventInfo info)
        {
            if (!CheckAndGetCommand(@event.Userid, @event.Text, out string command, out string args, out CCSPlayerController player))
            {
                return false;
            }
            switch (command)
            {
                case MATCH_COMMAND.WARMUP:
                    {
                        MatchMode.Rewarmup(player);
                        break;
                    }
                case MATCH_COMMAND.PAUSE:
                    {
                        MatchMode.Pause();
                        break;
                    }
                case MATCH_COMMAND.UNPAUSE:
                    {
                        MatchMode.Unpause(player);
                        break;
                    }
                case MATCH_COMMAND.READY:
                    {
                        MatchMode.Ready(player);
                        break;
                    }
                case MATCH_COMMAND.UNREADY:
                    {
                        MatchMode.UnReady(player);
                        break;
                    }
                case MATCH_COMMAND.FORCEREADY:
                    {
                        MatchMode.Start(player);
                        break;
                    }
                case MATCH_COMMAND.COACH:
                    {
                        MatchMode.AddCoach(player);
                        break;
                    }
                case MATCH_COMMAND.STOPCOACH:
                    {
                        MatchMode.StopCoach(player);
                        break;
                    }
                case MATCH_COMMAND.BACKUPMENU:
                    {
                        MatchMode.RestoreBackup(player);
                        break;
                    }
                case MATCH_COMMAND.RESTORE:
                    {
                        MatchMode.RestoreLastRound(player);
                        break;
                    }
                case MATCH_COMMAND.FORCEUNPAUSE:
                    {
                        MatchMode.ForceUnpause(player);
                        break;
                    }
                case MATCH_COMMAND.RESTART:
                    {
                        MatchMode.Restart(player);
                        break;
                    }
                case MATCH_COMMAND.DEMO:
                    {
                        DemoManager.OpenDemoManagerMenu(player);
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


        public override void PrintHelp(CCSPlayerController? player)
        {
            base.PrintHelp(player);
            List<string> message = new List<string>();
            message.Add($" {ChatColors.Green} {MATCH_COMMAND.WARMUP}{ChatColors.White} Switches to warmup.");
            message.Add($" {ChatColors.Green} {MATCH_COMMAND.PAUSE}{ChatColors.White} Pauses the match.");
            message.Add($" {ChatColors.Green} {MATCH_COMMAND.UNPAUSE}{ChatColors.White} Unpauses the match.");
            message.Add($" {ChatColors.Green} {MATCH_COMMAND.FORCEUNPAUSE}{ChatColors.White} Forcing unpause of the match.");
            message.Add($" {ChatColors.Green} {MATCH_COMMAND.READY}{ChatColors.White} Ready up the entire team.");
            message.Add($" {ChatColors.Green} {MATCH_COMMAND.UNREADY}{ChatColors.White} Unready the entire team.");
            message.Add($" {ChatColors.Green} {MATCH_COMMAND.FORCEREADY}{ChatColors.White} Forcing game to start.");
            message.Add($" {ChatColors.Green} {MATCH_COMMAND.COACH}{ChatColors.White} Switch to coach slot.");
            message.Add($" {ChatColors.Green} {MATCH_COMMAND.STOPCOACH}{ChatColors.White} Switch to player slot.");
            message.Add($" {ChatColors.Green} {MATCH_COMMAND.BACKUPMENU}{ChatColors.White} Open backup menu.");
            message.Add($" {ChatColors.Green} {MATCH_COMMAND.RESTORE}{ChatColors.White} Restoring last round.");
            message.Add($" {ChatColors.Green} {MATCH_COMMAND.RESTART}{ChatColors.White} Restarting match from 0-0.");
            message.Add($" {ChatColors.Green} {MATCH_COMMAND.DEMO}{ChatColors.White} Demo menu.");
            foreach (string s in message)
            {
                player?.PrintToChat(s);
            }
        }
    }
}
