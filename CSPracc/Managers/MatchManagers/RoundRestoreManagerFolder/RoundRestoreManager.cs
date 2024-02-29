using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Menu;
using CSPracc.DataModules;
using CSPracc.DataModules.Constants;
using CSPracc.Managers.BaseManagers;
using CSPracc.Managers.MatchManagers.RoundRestoreManagerFolder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CSPracc.Managers
{
    public class RoundRestoreManager : BaseManager
    {
        public RoundRestoreManager(ref CommandManager commandManager) : base(ref commandManager)
        {
            Commands.Add(MATCH_COMMAND.BACKUPMENU, new PlayerCommand(MATCH_COMMAND.BACKUPMENU, "Open backup menu", OpenBackupMenuCommandHandler, null));
            Commands.Add(MATCH_COMMAND.BACKUPMENU, new PlayerCommand(MATCH_COMMAND.RESTORE, "Restore last round", LoadLastBackup, null));
        }    

        public bool LoadLastBackup(CCSPlayerController player, List<string> args)
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

        public bool OpenBackupMenuCommandHandler(CCSPlayerController player, List<string> args)
        {
            BackupMenuBuilder.GetBackupMenu().Show(player);
            return true;
        }


    }
}
