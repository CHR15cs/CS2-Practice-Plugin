using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Menu;
using CounterStrikeSharp.API.Modules.Utils;
using CounterStrikeSharp.API;
using CSPracc.DataModules.Constants;
using CSPracc.DataModules;
using CSPracc.DataStorages.JsonStorages;
using CSPracc.Modes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSPracc.Managers
{
    internal class CommandAliasManager
    {
        protected static DirectoryInfo JsonBaseDirectory { get; } = new DirectoryInfo(Path.Combine(CSPraccPlugin.ModuleDir.FullName, "CommandAliases"));
        private static CommandAliasManager _instance;
        public static CommandAliasManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new CommandAliasManager();
                }
                return _instance;
            }

        }
        /// <summary>
        /// Alias Storage valid for all players
        /// </summary>
        protected CommandAliasStorage GlobalCommandAliasStorage { get; init; } = new CommandAliasStorage(JsonBaseDirectory);
        protected Dictionary<CCSPlayerController, CommandAliasStorage> PlayerSpecificCommandAliasStorages { get; init; } = new Dictionary<CCSPlayerController, CommandAliasStorage>();
        private CommandAliasManager() { }
        /// <summary>
        /// Gets or Adds Projectile Storage for given map
        /// </summary>
        /// <param name="mapName">Map name</param>
        /// <returns>Projectile Storage for given map</returns>
        protected CommandAliasStorage GetOrAddPersonalAliasStorage(CCSPlayerController player)
        {
            if (!PlayerSpecificCommandAliasStorages.ContainsKey(player))
            {
                //TODO: Get Directory from config.
                PlayerSpecificCommandAliasStorages.Add(player, new CommandAliasStorage(JsonBaseDirectory, player));
            }
            return PlayerSpecificCommandAliasStorages[player];
        }
        public bool ReplaceAlias(CCSPlayerController commandIssuer, string command, out string replacedCommand)
        {
            CommandAliasStorage personalCommandAliasStorage = GetOrAddPersonalAliasStorage(commandIssuer);
            if (personalCommandAliasStorage.Get(command, out replacedCommand))
            {
                Logging.LogMessage($"Replaced command {command} from personal storage.");
                return true;
            }
            else if(GlobalCommandAliasStorage.Get(command, out replacedCommand)){
                Logging.LogMessage($"Replaced command {command} from global storage.");
                return true;
            }
            else
            {
                replacedCommand = command;
                return false;
            }
        }
        public void CreateAlias(CCSPlayerController commandIssuer, string alias, string command, bool global = false)
        {
            //TODO Input validation?
            if(global)
            {
                if (commandIssuer.IsAdmin())
                {
                    if(!GlobalCommandAliasStorage.Add(alias, command))
                    {
                        commandIssuer.PrintToCenter("Alias already exists.");
                        return;
                    }
                    else
                    {
                        commandIssuer.PrintToCenter($"Successfully added global alias {alias}");
                        return;
                    }
                }
                else
                {
                    commandIssuer.PrintToCenter("Only admins are allowed to add global aliases");
                    return;
                }
            }
            else
            {
                CommandAliasStorage personalCommandAliasStorage = GetOrAddPersonalAliasStorage(commandIssuer);
                if (!personalCommandAliasStorage.Add(alias, command))
                {
                    commandIssuer.PrintToCenter("Alias already exists.");
                    return;
                }
                else
                {
                    commandIssuer.PrintToCenter($"Successfully added personal alias {alias}");
                    return;
                }
            }
        }
        public void RemoveAlias(CCSPlayerController commandIssuer, string alias, bool global = false)
        {
            //TODO Input validation?
            if (global)
            {
                if (commandIssuer.IsAdmin())
                {
                    if (!GlobalCommandAliasStorage.RemoveKey(alias))
                    {
                        commandIssuer.PrintToCenter("Failed to remove alias");
                        return;
                    }
                    else
                    {
                        commandIssuer.PrintToCenter($"Successfully removed global alias {alias}");
                        return;
                    }
                }
                else
                {
                    commandIssuer.PrintToCenter("Only admins are allowed to remove global aliases");
                    return;
                }
            }
            else
            {
                CommandAliasStorage personalCommandAliasStorage = GetOrAddPersonalAliasStorage(commandIssuer);
                if (!personalCommandAliasStorage.RemoveKey(alias))
                {
                    commandIssuer.PrintToCenter("Failed to remove alias");
                    return;
                }
                else
                {
                    commandIssuer.PrintToCenter($"Successfully removed personal alias {alias}");
                    return;
                }
            }
        }
    }
}
