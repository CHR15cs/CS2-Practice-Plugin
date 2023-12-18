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
    public class RetakeCommandHandler : BaseCommandHandler
    {
        RetakeMode RetakeMode { get; set; }
        public RetakeCommandHandler(RetakeMode mode):base(mode) 
        { 
            RetakeMode = mode;
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
                case RETAKE_COMMAND.edit:
                    {                      
                        RetakeMode.LoadEditMode(player);
                        break;
                    }
                case RETAKE_COMMAND.stopedit:
                    {
                        RetakeMode.LoadRetakeMode(player);
                        break;
                    }
                case RETAKE_COMMAND.addspawna:
                    {
                        RetakeMode.AddSpawn(player, "A");
                        break;
                    }
                case RETAKE_COMMAND.addspawnb:
                    {
                        RetakeMode.AddSpawn(player, "B");
                        break;
                    }
                case ".guns":
                {
                        RetakeMode.ShowGunMenu(player);
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
