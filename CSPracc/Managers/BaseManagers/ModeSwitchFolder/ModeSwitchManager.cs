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
    public class ModeSwitchManager
    {
        HtmlMenu ModeMenu { get; set; }
        CSPraccPlugin Plugin;
        public ModeSwitchManager(ref CommandManager commandManager, ref CSPraccPlugin plugin) 
        {
            Plugin = plugin;      
            ModeMenu = ModeMenuGenerator.GetModeMenu(plugin);
            commandManager.RegisterCommand(new PlayerCommand(BASE_COMMAND.MODE, "Open mode menu", OpenModeMenuCommandHandler, null,null));
            commandManager.RegisterCommand(new PlayerCommand(BASE_COMMAND.PRACC, "Switch to pracc mode", SwitchPraccCommandHandler, null, null));
            commandManager.RegisterCommand(new PlayerCommand(BASE_COMMAND.PRACC, "Switch to match mode", SwitchMatchCommandHandler, null, null));
            commandManager.RegisterCommand(new PlayerCommand(BASE_COMMAND.PRACC, "Switch to dry run mode", SwitchDryRunCommandHandler, null, null));
        }

        public bool OpenModeMenuCommandHandler(CCSPlayerController playerController, PlayerCommandArgument args)
        {
            GuiManager.Instance.AddMenu(playerController.SteamID, ModeMenu);
            return true;
        }
        public bool SwitchPraccCommandHandler(CCSPlayerController playerController, PlayerCommandArgument args)
        {
            Plugin.SwitchMode(Enums.PluginMode.Pracc);
            return true;
        }
        public bool SwitchMatchCommandHandler(CCSPlayerController playerController, PlayerCommandArgument args)
        {
            Plugin.SwitchMode(Enums.PluginMode.Match);
            return true;
        }

        public bool SwitchDryRunCommandHandler(CCSPlayerController playerController, PlayerCommandArgument args)
        {
            Plugin.SwitchMode(Enums.PluginMode.DryRun);
            return true;
        }
    }
}
