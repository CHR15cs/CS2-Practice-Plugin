using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CSPracc.DataModules.consts;
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
                        Match.Rewarmup(player);
                        break;
                    }
                case PRACC_COMMAND.PAUSE:
                    {
                        Match.Pause();
                        break;
                    }
                case PRACC_COMMAND.UNPAUSE:
                    {
                        Match.Unpause(player);
                        break;
                    }
                case PRACC_COMMAND.READY:
                    {
                        Match.Ready(player);
                        break;
                    }
                case PRACC_COMMAND.UNREADY:
                    {
                        Match.UnReady(player);
                        break;
                    }
                case PRACC_COMMAND.FORCEREADY:
                    {
                        Match.Start(player);
                        break;
                    }
                case PRACC_COMMAND.COACH:
                    {
                        Match.AddCoach(player);
                        break;
                    }
                case PRACC_COMMAND.STOPCOACH:
                    {
                        Match.StopCoach(player);
                        break;
                    }
                case PRACC_COMMAND.BACKUPMENU:
                    {
                        Match.RestoreBackup(player);
                        break;
                    }
                case PRACC_COMMAND.FORCEUNPAUSE:
                    {
                        Match.ForceUnpause(player);
                        break;
                    }
                case PRACC_COMMAND.RESTART:
                    {
                        Match.Restart(player);
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
