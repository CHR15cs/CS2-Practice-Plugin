using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CSPracc.DataModules.Constants;
using CSPracc.Modes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSPracc.CommandHandler
{
    public class PrefireCommandHandler : BaseCommandHandler
    {
        PrefireMode PrefireMode { get; set; }
        public PrefireCommandHandler(PrefireMode mode):base(mode) 
        {
            PrefireMode = mode;
        }

        public override bool PlayerChat(EventPlayerChat @event, GameEventInfo info)
        {
            if (!CheckAndGetCommand(@event.Userid, @event.Text, out string command, out string args, out CCSPlayerController player))
            {
                return false;
            }
            if (!player.IsValid) return false;

            switch (command)
            {
                case PREFIRE_COMMAND.options:
                    {                      
                        PrefireMode.ShowOptions(player);
                        break;
                    }
                case PREFIRE_COMMAND.route:
                    {
                        PrefireMode.LoadRoute(player,args);
                        break;
                    }
                case PREFIRE_COMMAND.routes:
                    {
                        PrefireMode.ShowRouteMenu(player);
                        break;
                    }
                case PREFIRE_COMMAND.addroute:
                    {
                        PrefireMode.AddRoute(player,args);
                        break;
                    }
                case PREFIRE_COMMAND.editroute:
                    {
                        PrefireMode.EditRoute(player, args);
                        break;
                    }
                case PREFIRE_COMMAND.savecurrentroute:
                    {
                        PrefireMode.SaveCurrentRoute();
                        break;
                    }
                case PREFIRE_COMMAND.addspawn:
                    {
                        PrefireMode.AddSpawn(player);
                        break;
                    }
                case PREFIRE_COMMAND.guns:
                {
                        PrefireMode.ShowGunMenu(player);
                        break;
                }
                case PREFIRE_COMMAND.addstartingpoint:
                    {
                        PrefireMode.SetStartingPoint(player);
                        break;
                    }
                case PREFIRE_COMMAND.restart:
                    {
                        PrefireMode.Restart(player);
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
