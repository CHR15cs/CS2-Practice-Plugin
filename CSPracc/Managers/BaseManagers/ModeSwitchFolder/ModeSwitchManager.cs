using CounterStrikeSharp.API.Core;
using CSPracc.DataModules;
using CSPracc.DataModules.Constants;
using CSPracc.Managers.BaseManagers.CommandManagerFolder;
using CSPracc.Managers.BaseManagers.ModeSwitchFolder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSPracc.Managers.BaseManagers
{
    /// <summary>
    /// Manager to switch between different modes
    /// </summary>
    public class ModeSwitchManager : BaseManager
    {
        HtmlMenu ModeMenu { get; set; }

        /// <summary>
        /// Constructor registering the command
        /// </summary>
        public ModeSwitchManager() : base()
        {   
            ModeMenu = ModeMenuGenerator.GetModeMenu();
            CommandManager.RegisterCommand(new PlayerCommand(BASE_COMMAND.MODE, "Open mode menu", OpenModeMenuCommandHandler, null,null));
            CommandManager.RegisterCommand(new PlayerCommand(BASE_COMMAND.PRACC, "Switch to pracc mode", SwitchPraccCommandHandler, null, null));
            CommandManager.RegisterCommand(new PlayerCommand(BASE_COMMAND.MATCH, "Switch to match mode", SwitchMatchCommandHandler, null, null));
            CommandManager.RegisterCommand(new PlayerCommand(BASE_COMMAND.DryRun, "Switch to dry run mode", SwitchDryRunCommandHandler, null, null));
        }

        private bool OpenModeMenuCommandHandler(CCSPlayerController playerController, PlayerCommandArgument args)
        {
            GuiManager.Instance.AddMenu(playerController.SteamID, ModeMenu);
            return true;
        }
        private bool SwitchPraccCommandHandler(CCSPlayerController playerController, PlayerCommandArgument args)
        {
            CSPraccPlugin.Instance.SwitchMode(Enums.PluginMode.Pracc);
            return true;
        }
        private bool SwitchMatchCommandHandler(CCSPlayerController playerController, PlayerCommandArgument args)
        {
            CSPraccPlugin.Instance.SwitchMode(Enums.PluginMode.Match);
            return true;
        }
        private bool SwitchDryRunCommandHandler(CCSPlayerController playerController, PlayerCommandArgument args)
        {
            CSPraccPlugin.Instance.SwitchMode(Enums.PluginMode.DryRun);
            return true;
        }
    }
}
