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

namespace CSPracc.Managers.PracticeManagers
{
    public class PracticeSettingsManager
    {
        GuiManager _guiManager;
        GuiManager GuiManager
        {
            get { return _guiManager; }
        }
        CSPraccPlugin _plugin;
        CSPraccPlugin Plugin
        {
            get
            {
                return _plugin;
            }
        }
        public PracticeSettingsManager(ref CommandManager commandManager, ref GuiManager guiManager, ref CSPraccPlugin plugin) 
        {
            _guiManager = guiManager;
            _plugin = plugin;
            commandManager.RegisterCommand(new DataModules.PlayerCommand(PRACC_COMMAND.settings,"Open Settings Menu", ShowPracticeMenuCommandHandler, null));
        }
        

        public bool ShowPracticeMenuCommandHandler(CCSPlayerController player, List<string> args)
        {
            GuiManager.AddMenu(player.SteamID, PracticeSettingsMenu.GetPracticeSettingsMenu(player,Plugin));
            return true;
        }
    }
}
