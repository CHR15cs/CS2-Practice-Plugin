using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CSPracc.DataModules.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSPracc.CommandHandler
{
    public class MatchCommandHandler : BaseCommandHandler
    {
        public MatchCommandHandler():base() 
        { 
            
        }

        public override bool PlayerChat(EventPlayerChat @event, GameEventInfo info)
        {
            if (!CheckAndGetCommand(@event.Userid, @event.Text, out string command, out string args, out CCSPlayerController player))
            {
                return false;
            }
            switch (command)
            {
                case PRACC_COMMAND.WARMUP:
                    {
                        MatchMode.Rewarmup(player);
                        break;
                    }
                case PRACC_COMMAND.PAUSE:
                    {
                        MatchMode.Pause();
                        break;
                    }
                case PRACC_COMMAND.UNPAUSE:
                    {
                        MatchMode.Unpause(player);
                        break;
                    }
                case PRACC_COMMAND.READY:
                    {
                        MatchMode.Ready(player);
                        break;
                    }
                case PRACC_COMMAND.UNREADY:
                    {
                        MatchMode.UnReady(player);
                        break;
                    }
                case PRACC_COMMAND.FORCEREADY:
                    {
                        MatchMode.Start(player);
                        break;
                    }
                case PRACC_COMMAND.COACH:
                    {
                        MatchMode.AddCoach(player);
                        break;
                    }
                case PRACC_COMMAND.STOPCOACH:
                    {
                        MatchMode.StopCoach(player);
                        break;
                    }
                case PRACC_COMMAND.BACKUPMENU:
                    {
                        MatchMode.RestoreBackup(player);
                        break;
                    }
                case PRACC_COMMAND.FORCEUNPAUSE:
                    {
                        MatchMode.ForceUnpause(player);
                        break;
                    }
                case PRACC_COMMAND.RESTART:
                    {
                        MatchMode.Restart(player);
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
    }
}
