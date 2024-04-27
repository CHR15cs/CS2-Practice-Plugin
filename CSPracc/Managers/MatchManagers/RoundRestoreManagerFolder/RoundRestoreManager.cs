using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Menu;
using CSPracc.DataModules;
using CSPracc.DataModules.Constants;
using CSPracc.Managers.BaseManagers;
using CSPracc.Managers.BaseManagers.CommandManagerFolder;
using CSPracc.Managers.MatchManagers.RoundRestoreManagerFolder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CSPracc.Managers
{
    /// <summary>
    /// Manager to handle the round restore
    /// </summary>
    public class RoundRestoreManager : BaseManager
    {
        /// <summary>
        /// RoundRestoreManager constructor
        /// </summary>
        public RoundRestoreManager() : base()
        {
            Commands.Add(MATCH_COMMAND.BACKUPMENU, new PlayerCommand(MATCH_COMMAND.BACKUPMENU, "Open backup menu", OpenBackupMenuCommandHandler, null,null));
            Commands.Add(MATCH_COMMAND.BACKUPMENU, new PlayerCommand(MATCH_COMMAND.RESTORE, "Restore last round", LoadLastBackup, null,null));
        }    
        private bool LoadLastBackup(CCSPlayerController player, PlayerCommandArgument args)
        {
            string? lastBackup = RestoreFileHelper.GetLastBackupFileOrNull();
            if (lastBackup == null)
            {
                player.ChatMessage("Could not find last backup");
                return false;
            }
            Server.ExecuteCommand($"mp_backup_restore_load_file {lastBackup}");
            CSPracc.DataModules.Constants.Methods.MsgToServer("Restored round, to continue match both teams need to use .unpause");
            return true;
        }    
        private bool OpenBackupMenuCommandHandler(CCSPlayerController player, PlayerCommandArgument args)
        {
            BackupMenuBuilder.GetBackupMenu().Show(player);
            return true;
        }
    }
}
