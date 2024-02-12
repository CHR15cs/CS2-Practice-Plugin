using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CSPracc.DataModules;
using CSPracc.DataModules.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSPracc.Managers.BaseManagers
{
    public class MapChangeManager
    {       
        public MapChangeManager(ref CommandManager commandHandler) 
        {
            commandHandler.RegisterCommand(new PlayerCommand(BASE_COMMAND.MAP, "Change map to given name", ChangeMapCommandHandler, null));
        }

        private bool ChangeMapCommandHandler(CCSPlayerController playerController, List<string> args) 
        { 
            if(args.Count != 1)
            {
                playerController.ChatMessage("Unexpectec amount of arguments");
                return false;
            }
            string mapName = args[0];
            if (!mapName.StartsWith("de_"))
            {
                mapName = "de_" + mapName;
            }
            Utils.ServerMessage($"Changing map in {CSPraccPlugin.Instance!.Config!.DelayMapChange}s to {mapName}");
            CSPraccPlugin.Instance!.AddTimer(CSPraccPlugin.Instance!.Config!.DelayMapChange, () => Server.ExecuteCommand($"changelevel {mapName}"));
            return true;
        }
    }
}
