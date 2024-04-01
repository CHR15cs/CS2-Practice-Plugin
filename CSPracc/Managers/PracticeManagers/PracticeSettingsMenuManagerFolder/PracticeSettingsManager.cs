using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Cvars;
using CounterStrikeSharp.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSPracc.DataModules.Constants;
using CounterStrikeSharp.API.Modules.Utils;
using CSPracc.DataModules;
using CSPracc.Managers.PracticeManagers.PracticeSettingsMenuManagerFolder;
using CSPracc.Managers.BaseManagers.CommandManagerFolder;

namespace CSPracc.Managers.PracticeManagers
{
    public class PracticeSettingsManager
    {
        public PracticeSettingsManager(ref CommandManager commandManager, ref GuiManager guiManager, ref CSPraccPlugin plugin) 
        {
            commandManager.RegisterCommand(new DataModules.PlayerCommand(PRACC_COMMAND.settings,"Open Settings Menu", ShowPracticeMenuCommandHandler, null,null));
        }
        
        public bool ShowPracticeMenuCommandHandler(CCSPlayerController player, PlayerCommandArgument args)
        {
            PracticeSettingsMenu.GetPracticeSettingsMenu(player).Show(player);
            return true;
        }
    }
}
