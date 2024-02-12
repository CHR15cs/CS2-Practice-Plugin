using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Modules.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSPracc.DataModules.Constants
{
    public class COMMANDS
    {
        public static string PAUSE_MATCH = "mp_pause_match 1";
        public static string UNPAUSE_MATCH = "mp_unpause_match 1";
        public static string RESTART_GAME = "mp_restartgame 1";
        public static string START_MATCH = "exec CSPRACC\\5on5.cfg";
        public static string SWAP_TEAMS = "mp_swapteams 1";
    }

    public class RETAKE_COMMAND
    {
        public const string edit = ".edit";
        public const string stopedit = ".stopedit";
        public const string addspawna = ".addspawna";
        public const string addspawnb = ".addspawnb";
    }

    public class PREFIRE_COMMAND
    {
        public const string options = ".options";
        public const string routes = ".routes";
        public const string route = ".route";
        public const string addroute = ".addroute";
        public const string deleteroute = ".deleteroute";
        public const string editroute = ".editroute";
        public const string next = ".next";
        public const string back = ".back";
        public const string guns = ".guns";
        public const string addspawn = ".addspawn";
        public const string addstartingpoint = ".addstart";
        public const string savecurrentroute = ".save";
        public const string restart = ".restart";
    }


    public class BASE_COMMAND
    {
        public const string MODE = ".menu";
        public const string PRACC = ".pracc";
        public const string MATCH = ".match";
        public const string DryRun = ".dryrun";
        public const string Prefire = ".prefire";
        public const string Retake = ".retake";
        public const string Unload = ".unload";
        public const string HELP = ".help";
        public const string FAKERCON = ".rcon";
        public const string MAP = ".map";
        public const string GOT = ".t";
        public const string GOCT = ".ct";
        public const string GOSPEC = ".spec";
        public const string SWAP = ".swap";
        public const string ALIAS = ".alias";
        public const string REMOVEALIAS = ".ralias";
    }

    public class MATCH_COMMAND
    {
        public const string PAUSE = ".pause";
        public const string UNPAUSE = ".unpause";
        public const string READY = ".ready";
        public const string UNREADY = ".unready";
        public const string FORCEREADY = ".forceready";
        public const string STOP = ".stop";
        public const string WARMUP = ".warmup";
        public const string RESTART = ".restart";
        public const string COACH = ".coach";
        public const string STOPCOACH = ".stopcoach";
        public const string BACKUPMENU = ".backup";
        public const string RESTORE = ".restore";
        public const string FORCEUNPAUSE = ".forceunpause";
        public const string DEMO = ".demo";
    }

    public class DRYRUN_COMMAND
    {
        public const string refill = ".refill";
        public const string ak = ".ak";
        public const string m4 = ".m4";
        public const string awp = ".awp";
        public const string m4a1 = ".m4a1";
    }

    public class PROJECTILE_COMMAND
    {
        public const string NADES = "nades";
        public const string find = "find";
        public const string SAVE = "save";
        public const string delay = "delay";
        public const string Description = "desc";
        public const string Rename = "rename";
        public const string AddTag = "addtag";
        public const string RemoveTag = "removetag";
        public const string ClearTags = "cleartags";
        public const string DeleteTag = "deletetag";
        public const string UpdatePos = "updatepos";
        public const string Last = "last";
        public const string forward = "forward";
        public const string BACK = "back";
        public const string Delete = "delete";
        public const string CLEAR = "clear";
        public const string ClearAll = "clearall";
        public const string rethrow = "throw";
        public const string flash = "flash";
        public const string noflash = "noflash";
        public const string stop = "stop";
        public const string showtags = "showtags";
    }

    public class BotReplayCommands
    {
        public const string mimic_menu = "mimic";
        public const string create_replay = "createreplay";
        public const string record_role = "record";
        public const string stoprecord = "stoprecord";
        public const string store_replay = "storereplay";
        public const string rename_replayset = "renameset";
        public const string replay_menu = "replays";
    }

    public class PRACC_COMMAND
    {      
        public const string SPAWN = "spawn";
        public const string TSPAWN = "tspawn";
        public const string CTSPAWN = "ctspawn";           
       
        
        public const string bestspawn = "bestspawn";
        public const string worstspawn = "worstspawn";

                   
        public const string BOT = "bot";
        public const string tBOT = "tbot";
        public const string ctBOT = "ctbot";
        public const string BOOST = "boost";
        public const string WATCHME = "watchme";
        public const string NOBOT = "nobot";
        public const string CLEARBOTS = "clearbots";
        public const string CROUCHBOT = "crouchbot";
        public const string CROUCHBOOST = "crouchboost";
        public const string SwapBot = "swapbot";
        public const string MoveBot = "movebot";

              
        public const string SAVELAST = "savelast";
        public const string CHECKPOINT = "checkpoint";
        public const string TELEPORT = "tp";
       
        public const string UserRole = "userrole";        
        public const string timer = "timer";
        public const string countdown = "countdown";
      

        public const string settings = "settings";
        public const string editnade = "editnade";
       

        public const string breakstuff = "break";
        public const string impacts = "impacts";

      
    }

    public class AdminFlags
    {
        public const string Standard = "@CSPracc/admin";
    }
    public class Methods
    {
        public static void MsgToServer(string msg)
        {
            Server.PrintToChatAll($"{CSPraccPlugin.Instance!.Config.ChatPrefix} {msg}");
        }
    }
    public class DesignerNames
    {
        public const string ProjectileSmoke = "smokegrenade_projectile";
        public const string ProjectileFlashbang = "flashbang_projectile";
        public const string ProjectileHE = "hegrenade_projectile";
        public const string ProjectileDecoy = "decoy_projectile";
        public const string ProjectileMolotov = "molotov_projectile";
    }
}
