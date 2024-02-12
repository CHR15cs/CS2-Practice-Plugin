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
    public class ToggleImpactManager
    {
        public ToggleImpactManager(ref CommandManager commandManager) 
        { 
            commandManager.RegisterCommand(new DataModules.PlayerCommand(PRACC_COMMAND.impacts,"Toggle impacts",ImpactCommandHandler,null));
        }

        public bool ImpactCommandHandler(CCSPlayerController playerController, List<string> args)
        {
            ConVar? showimpacts = ConVar.Find("sv_showimpacts");
            if (showimpacts == null) return false;
            int currVal = showimpacts.GetPrimitiveValue<int>();
            if (currVal != 0)
            {
                Utils.ServerMessage("Disabling impacts.");
                Server.ExecuteCommand("sv_showimpacts 0");
            }
            else
            {
                Utils.ServerMessage("Enabling impacts.");
                Server.ExecuteCommand("sv_showimpacts 1");
            }
            return true;
        }

    }
}
