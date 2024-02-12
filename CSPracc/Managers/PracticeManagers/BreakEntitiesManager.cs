using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Cvars;
using CounterStrikeSharp.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSPracc.DataModules.Constants;

namespace CSPracc.Managers.PracticeManagers
{
    public class BreakEntitiesManager
    { 
        public BreakEntitiesManager(ref CommandManager commandManager) 
        { 
            commandManager.RegisterCommand(new DataModules.PlayerCommand(PRACC_COMMAND.breakstuff,"Break all breakable entities", BreakStuffCommandHandler, null));
        }

        private bool BreakStuffCommandHandler(CCSPlayerController playerController, List<string> args)
        {
            Utils.BreakAll();
            return true;
        }

    }
}
