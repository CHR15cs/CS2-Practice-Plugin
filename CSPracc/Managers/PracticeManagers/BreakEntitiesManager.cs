using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Cvars;
using CounterStrikeSharp.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSPracc.DataModules.Constants;
using CSPracc.Managers.BaseManagers;
using CSPracc.Managers.BaseManagers.CommandManagerFolder;

namespace CSPracc.Managers.PracticeManagers
{
    public class BreakEntitiesManager : BaseManager
    { 
        public BreakEntitiesManager(ref CommandManager commandManager)  : base(ref commandManager)
        {
            Commands.Add(PRACC_COMMAND.breakstuff, new DataModules.PlayerCommand(PRACC_COMMAND.breakstuff, "Break all breakable entities", BreakStuffCommandHandler, null, null));
        }

        private bool BreakStuffCommandHandler(CCSPlayerController playerController, PlayerCommandArgument args)
        {
            Utils.BreakAll();
            return true;
        }
    }
}
