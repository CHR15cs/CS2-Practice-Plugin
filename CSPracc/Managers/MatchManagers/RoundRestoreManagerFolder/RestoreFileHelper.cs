using CounterStrikeSharp.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSPracc.Managers.MatchManagers.RoundRestoreManagerFolder
{
    /// <summary>
    /// Helper class to handle the backup files
    /// </summary>
    public class RestoreFileHelper
    {
        /// <summary>
        /// Cleanup old backup files
        /// </summary>
        public static void CleanupOldFiles()
        {
            foreach (FileInfo file in GetBackupFiles())
            {
                file.Delete();
            }
        }

        /// <summary>
        /// Get the last backup file
        /// </summary>
        /// <returns>null if no backupfile is found</returns>
        public static string? GetLastBackupFileOrNull()
        {
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
            string sround = highestRound >= 10 ? highestRound.ToString() : "0" + highestRound;
            foreach (FileInfo backup in Backupfiles)
            {
                if (backup.Name.Contains(sround))
                {
                    return backup.Name;
                }
            }
            return null;
        }

        /// <summary>
        /// Get all backup files
        /// </summary>
        /// <returns>List of Backup files</returns>
        public static List<FileInfo> GetBackupFiles()
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
                catch (Exception) { }

                foreach (FileInfo file in unsortedBackups)
                {
                    int round2 = -1;
                    try
                    {
                        round2 = Convert.ToInt32(file.Name.Substring(file.Name.Length - 6, 2));
                    }
                    catch (Exception) { }
                    if (round2 != -1 && round2 < round) { earliestBackup = file; }
                }
                unsortedBackups.Remove(earliestBackup);
                sortedbackupFiles.Add(earliestBackup);
            }
            return sortedbackupFiles;
        }

        /// <summary>
        /// Parse Backupfile and return score
        /// </summary>
        /// <param name="fileInfo">backupfile to readout</param>
        /// <returns>score of file</returns>
        public static string GetScoreOfBackupFile(FileInfo fileInfo)
        {
            string content = File.ReadAllText(fileInfo.FullName);
            int startOfScore = content.IndexOf('{', content.IndexOf("FirstHalfScore")) + 1;
            int endOfScore = content.IndexOf('}', startOfScore) - 1;
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
                if (secondhalfScore.Count != 4)
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
    }
}
