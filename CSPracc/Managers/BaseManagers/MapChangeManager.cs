using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CSPracc.DataModules;
using CSPracc.DataModules.Constants;
using CSPracc.Managers.BaseManagers.CommandManagerFolder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSPracc.Managers.BaseManagers
{
    /// <summary>
    /// Manager for changing the map
    /// </summary>
    public class MapChangeManager : BaseManager
    {       
        /// <summary>
        /// Constructor registering the command
        /// </summary>
        public MapChangeManager() : base()
        {
            CommandManager.RegisterCommand(new PlayerCommand(BASE_COMMAND.MAP, "Change map to given name", ChangeMapCommandHandler, null,null));
        }

        /// <summary>
        /// Change map command handler
        /// </summary>
        /// <param name="playerController">player who issued the command</param>
        /// <param name="args">arguments passed form the player, should be map name</param>
        /// <returns>True is execution is successfull</returns>
        private bool ChangeMapCommandHandler(CCSPlayerController playerController, PlayerCommandArgument args) 
        { 
            if(args.ArgumentCount != 1)
            {
                playerController.ChatMessage("Unexpectec amount of arguments");
                return false;
            }
            string mapName = args.ArgumentString;
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
