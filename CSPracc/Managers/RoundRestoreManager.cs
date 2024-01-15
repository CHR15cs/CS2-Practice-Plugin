using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Menu;
using CSPracc.DataModules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CSPracc.Managers
{
    public static class RoundRestoreManager
    {
        public static void CleanupOldFiles()
        {
            foreach (FileInfo file in GetBackupFiles())
            {
                file.Delete();
            }
        }
        private static List<FileInfo> GetBackupFiles()
        {
            List<FileInfo> unsortedBackups = CSPraccPlugin.Cs2Dir.GetFiles("backup_round*").ToList();
            List<FileInfo> sortedbackupFiles = new List<FileInfo>();
            
            while (unsortedBackups.Count > 0)
            {
                FileInfo earliestBackup = unsortedBackups.First();
                int round = -1;
                try
                {
                    round = Convert.ToInt32(earliestBackup.Name.Substring(earliestBackup.Name.Length - 6, 2));
                }
                catch (Exception ex) { }
               
                foreach (FileInfo file in unsortedBackups) 
                {
                    int round2 = -1;
                    try
                    {
                       round2 = Convert.ToInt32(file.Name.Substring(file.Name.Length - 6, 2));
                    }
                    catch (Exception ex) { }
                    if(round2 != -1 && round2 < round) { earliestBackup = file; }
                }
                unsortedBackups.Remove(earliestBackup);
                sortedbackupFiles.Add(earliestBackup);             
            }
            return sortedbackupFiles;
        }

        public static void LoadLastBackup(CCSPlayerController player)
        {
            if (player == null) return;
            if (!player.PlayerPawn.IsValid) return;
            if (!player.IsAdmin())
            {
                player.PrintToCenter("Only admins can execute this command!");
                return;
            }

            List<FileInfo> Backupfiles = GetBackupFiles();
            int highestRound = 0;
            foreach (FileInfo file in Backupfiles)
            {
                try
                {
                    int round = Convert.ToInt32(file.Name.Substring(file.Name.Length - 6, 2));
                    if (round > highestRound)
                    {
                        highestRound = round;
                    }
                }
                catch { }               
            }
            string sround = highestRound >= 10 ? highestRound.ToString() : "0"+highestRound;
            foreach (FileInfo backup in Backupfiles)
            {
                if(backup.Name.Contains(sround))
                {                   
                    Server.ExecuteCommand($"mp_backup_restore_load_file {backup.Name}");
                }
            }
            CSPracc.DataModules.Constants.Methods.MsgToServer("Restored round, to continue match both teams need to use .unpause");

        }

        private static string getScoreOfBackupFile(FileInfo fileInfo) 
        {
            string score = "0-0";

            string content = File.ReadAllText(fileInfo.FullName);
            int startOfScore = content.IndexOf('{', content.IndexOf("FirstHalfScore"))+1;
            int endOfScore = content.IndexOf('}', startOfScore)-1;
            int length = endOfScore - startOfScore;
            string firsthalf = content.Substring(startOfScore, length);
            int team1 = 0;
            int team2 = 0;
            List<string> firsthalfScore = parseBackupInfo(firsthalf);
            if (firsthalfScore.Count != 4)
            {
                Server.PrintToConsole("Invalid backupfile");
                return "invalid";
            }
            team1 += Convert.ToInt32(firsthalfScore[1]);
            team2 += Convert.ToInt32(firsthalfScore[3]);
            if (content.Contains("SecondHalfScore"))
            {
                 startOfScore = content.IndexOf('{', content.IndexOf("SecondHalfScore")) + 1;
                 endOfScore = content.IndexOf('}', startOfScore) - 1;
                 length = endOfScore - startOfScore;
                 string secondhalf = content.Substring(startOfScore, length);
                List<string> secondhalfScore = parseBackupInfo(secondhalf);
                if(secondhalfScore.Count != 4)
                {
                    Server.PrintToConsole("Invalid backupfile");
                    return "invalid";
                }
                team1 += Convert.ToInt32(secondhalfScore[1]);
                team2 += Convert.ToInt32(secondhalfScore[3]);
            }
            return $"{team1}:{team2}";
        }

        private static List<string> parseBackupInfo(string backupInfo)
        {
            bool beginning = false;
            List<string> lines = new List<string>();
            string laststring = "";
            for (int i = 0; i < backupInfo.Length; i++)
            {
                if (backupInfo[i] == '"')
                {
                    if (!beginning)
                    {
                        beginning = true;
                        continue;
                    }
                    else
                    {
                        lines.Add(laststring);
                        laststring = "";
                        beginning = false;
                        continue;
                    }
                }
                laststring += backupInfo[i];
            }
            return lines;
        }

        public static void OpenBackupMenu(CCSPlayerController player)
        {
            if (player == null) return;
            if (!player.PlayerPawn.IsValid) return;
            if (!player.IsAdmin())
            {
                player.PrintToCenter("Only admins can execute this command!");
                return;
            }
            var backupMenu = new ChatMenu("Backup Menu");
            var handleGive = (CCSPlayerController player, ChatMenuOption option) => LoadSelectedBackup(option.Text);

            List<FileInfo> Backupfiles = GetBackupFiles();

            foreach (var file in Backupfiles)
            {
                string round = getScoreOfBackupFile(file);
                backupMenu.AddMenuOption(round, handleGive);
            }
            ChatMenus.OpenMenu(player, backupMenu);
        }

        private static void LoadSelectedBackup(string Backup)
        {
            List<FileInfo> backups = GetBackupFiles();
            bool FileFound = false;
            foreach (var backup in backups) 
            { 
                if(getScoreOfBackupFile(backup) == Backup)
                {
                    Server.ExecuteCommand($"mp_backup_restore_load_file {backup.Name}");
                    FileFound = true;
                }
            }
            if (!FileFound)
            {
                CSPracc.DataModules.Constants.Methods.MsgToServer("Failed to restore round. Please select another backup file.");
            }
            else
            {
                CSPracc.DataModules.Constants.Methods.MsgToServer("Restored round, to continue match both teams need to use .unpause");
            }
            
        }


    }
}
