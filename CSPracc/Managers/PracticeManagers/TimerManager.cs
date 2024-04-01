using CounterStrikeSharp.API.Core;
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
    public class TimerManager : BaseManager
    {
        public TimerManager() : base()
        { 
            Commands.Add(PRACC_COMMAND.timer,new DataModules.PlayerCommand(PRACC_COMMAND.timer,"Start timer", TimerCommandHandler, null,null));
        }
        public bool TimerCommandHandler(CCSPlayerController playerController, PlayerCommandArgument args)
        {
            StartTimer(playerController);
            return true;
        }
        public void StartTimer(CCSPlayerController player)
        {
            if (player == null) return;
            GuiManager.Instance.StartTimer(player);
        }
    }
}
