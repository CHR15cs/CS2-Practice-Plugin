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
using CSPracc.Managers.BaseManagers;

namespace CSPracc.Managers.PracticeManagers
{
    /// <summary>
    /// Practice settings manager
    /// </summary>
    public class PracticeSettingsManager : BaseManager
    {
        /// <summary>
        /// Constructor for the practice settings manager
        /// </summary>
        public PracticeSettingsManager() : base()
        {
            CommandManager.RegisterCommand(new DataModules.PlayerCommand(PRACC_COMMAND.settings,"Open Settings Menu", ShowPracticeMenuCommandHandler, null,null));
        }
        
        private bool ShowPracticeMenuCommandHandler(CCSPlayerController player, PlayerCommandArgument args)
        {
            PracticeSettingsMenuBuilder.GetPracticeSettingsMenu(player).Show(player);
            return true;
        }
    }
}
