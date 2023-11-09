using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Modules.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSPracc.DataModules.consts
{
    public class COMMANDS
    {
        public static string PAUSE_MATCH = "mp_pause_match 1";
        public static string UNPAUSE_MATCH = "mp_unpause_match 1";
        public static string RESTART_GAME = "mp_restartgame 1";
        public static string START_WARMUP = "exec CSPRACC\\warmup.cfg";
        public static string START_MATCH = "exec CSPRACC\\5on5.cfg";
        public static string SWAP_TEAMS = "mp_swapteams 1";
    }

    public class PRACC_COMMAND
    {
        public const  string MODE = ".menu";
        public const string PRACC = ".pracc";
        public const string MATCH = ".match";
        public const string PAUSE = ".pause";
        public const  string UNPAUSE = ".unpause";
        public const string READY = ".ready";
        public const string UNREADY = ".unready";
        public const string FORCEREADY = ".forceready";
        public const string STOP = ".stop";
        public const string WARMUP = ".warmup";
        public const string HELP = ".help";
        public const string RESTART = ".restart";
        public const string SPAWN = ".spawn";
        public const string COACH = ".coach";
        public const string STOPCOACH = ".stopcoach";
        public const string FAKERCON = ".rcon";
        public const string BACKUPMENU = ".backup";
        public const string NADES = ".nades";
        public const string SAVE = ".save";
        public const string MAP = ".map";
        public const string RESTORE = ".restore";
        public const string FORCEUNPAUSE = ".forceunpause";
        public const string BOT = ".bot";
        public const string BOOST = ".boost";
        public const string WATCHME = ".watchme";
        public const string NOBOT = ".nobot";
        public const string CLEARBOTS = ".clearbots";
        public const string CROUCHBOT = ".crouchbot";
        public const string CROUCHBOOST = ".crouchboost";
        public const string GOT = ".t";
        public const string GOCT = ".ct";
        public const string GOSPEC = ".spec";
        public const string SWAP = ".swap";
    }
    public class Strings
    {
        public static string ChatTag = $" {ChatColors.Green}[{ChatColors.Red}CSPRACC{ChatColors.Green}]{ChatColors.White} ";
    }

    public class Methods
    {
        public static void MsgToServer(string msg)
        {
            Server.PrintToChatAll($"{Strings.ChatTag} {msg}");
        }
    }
}
