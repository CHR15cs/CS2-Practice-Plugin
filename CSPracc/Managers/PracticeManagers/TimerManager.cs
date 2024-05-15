using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Entities;
using CounterStrikeSharp.API.Modules.Utils;
using CSPracc.DataModules.Constants;
using CSPracc.Managers.BaseManagers;
using CSPracc.Managers.BaseManagers.CommandManagerFolder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSPracc.Managers.PracticeManagers
{
    /// <summary>
    /// Class for handling timer
    /// </summary>
    public class TimerManager : BaseManager
    {
        /// <summary>
        /// Constructor for the timer manager
        /// </summary>
        public TimerManager() : base()
        { 
            Commands.Add(PRACC_COMMAND.timer,new DataModules.PlayerCommand(PRACC_COMMAND.timer,"Start timer", TimerCommandHandler, null,null));
        }

        private bool TimerCommandHandler(CCSPlayerController playerController, PlayerCommandArgument args)
        {
            GuiManager.Instance.StartTimer(playerController);
            return true;
        }
    }
}
