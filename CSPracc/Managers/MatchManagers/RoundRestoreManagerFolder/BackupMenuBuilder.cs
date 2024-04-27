using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Menu;
using CSPracc.DataModules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSPracc.Managers.MatchManagers.RoundRestoreManagerFolder
{
    /// <summary>
    /// Builder for the backup menu
    /// </summary>
    public class BackupMenuBuilder
    {
        /// <summary>
        /// Get the backup menu
        /// </summary>
        /// <returns>Backup Menu</returns>
        public static HtmlMenu GetBackupMenu()
        {
            List<KeyValuePair<string,Action>> htmlMenuOption = new List<KeyValuePair<string,Action>>(); 
            var backupMenu = new ChatMenu("Backup Menu");
            var handleGive = (CCSPlayerController player, ChatMenuOption option) => LoadSelectedBackup(option.Text);

            List<FileInfo> Backupfiles = RestoreFileHelper.GetBackupFiles();

            foreach (var file in Backupfiles)
            {
                string round = RestoreFileHelper.GetScoreOfBackupFile(file);
                htmlMenuOption.Add(new KeyValuePair<string,Action>(round, new Action(() => { LoadSelectedBackup(file.Name); })));              
            }
            return new HtmlMenu("Backups", htmlMenuOption);
        }

        private static void LoadSelectedBackup(string Backup)
        {
            List<FileInfo> backups = RestoreFileHelper.GetBackupFiles();
            bool FileFound = false;
            foreach (var backup in backups)
            {
                if (backup.Name == Backup)
                {
                    Server.ExecuteCommand($"mp_backup_restore_load_file {backup.Name}");
                    FileFound = true;
                }
            }
            if (!FileFound)
            {
                Utils.ServerMessage("Failed to restore round. Please select another backup file.");
            }
            else
            {
                Utils.ServerMessage("Restored round, to continue match both teams need to use .unpause");
            }

        }
    }
}
