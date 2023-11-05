 using System;
 using System.Collections.Generic;
 using System.IO;
 using System.Linq;
 using System.Net.Http;
 using System.Threading;
 using System.Threading.Tasks;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Entities;
using CounterStrikeSharp.API.Modules.Events;
using CounterStrikeSharp.API.Modules.Memory;
using CounterStrikeSharp.API.Modules.Menu;
using CounterStrikeSharp.API.Modules.Utils;
using CSPracc;
using CSPracc.DataModules;
using CSPracc.DataModules.consts;
using System.Xml;
using System.Xml.Serialization;

public class CSPraccPlugin : BasePlugin
{
    List<CSPracc.DataModules.Player>? Players;
    
    private Match match;
    private static List<SteamID> _adminList = null;
    public static List<SteamID> AdminList
    {
        get
        {
            if(_adminList == null)
            {
                _adminList = new List<SteamID>();
                if (Config != null)
                {
                    foreach (string admin in Config.Admins)
                    {
                        try
                        {
                            _adminList.Add(new SteamID(admin));
                        }
                        catch 
                        {
                            Server.PrintToConsole("ignored admin: " + admin);
                        }
                        
                    }
                }
            }
            return _adminList;

        }
    }


    #region properties
    public override string ModuleName
    {
        get
        {
            return "CSPraccPlugin";
        }
    }
    public override string ModuleVersion
    {
        get
        {
            return "1.0.0.0";
        }
    }


    private static DirectoryInfo _moduleDir;
    public static DirectoryInfo ModuleDir => _moduleDir;
    public static FileInfo AdminIni
    {
        get
        {
            return new FileInfo(Path.Combine(ModuleDir.FullName, "admin.ini"));
        }
    }
    private string _rconPassword = String.Empty;
    private string RconPassword
    {
        get
        {
            if (_rconPassword == String.Empty)
            {
               if(Config != null)
                {
                    _rconPassword = Config.RconPassword;
                }
            }
            return _rconPassword;
        }
    }
    DirectoryInfo _csgoDir = null;
    DirectoryInfo CsgoDir
    {
        get
        {
            if (_csgoDir == null)
            {
                _csgoDir = new DirectoryInfo(Path.Combine(Server.GameDirectory, "csgo"));
            }
            return _csgoDir;
        }
    }

    ChatMenu _modeMenu = null;
    ChatMenu ModeMenu
    {
        get
        {
            if (_modeMenu == null)
            {
                _modeMenu = new ChatMenu("Mode Menu");
                var handleGive = (CCSPlayerController player, ChatMenuOption option) => ModeMenuOption(player, option.Text);
                _modeMenu.AddMenuOption("Pracc", handleGive);
                _modeMenu.AddMenuOption("Match", handleGive);
                _modeMenu.AddMenuOption("Help", handleGive);
            }
            return _modeMenu;
        }
    }

    private static FileInfo configManagerFile = null;

    public static ConfigManager Config;
    #endregion

    public override void Load(bool hotReload)      
    {
        base.Load(hotReload);
        _moduleDir = new DirectoryInfo(ModuleDirectory);
        Logging logging = new Logging(new FileInfo(Path.Combine(ModuleDir.FullName, "Logging.txt")));
        configManagerFile = new FileInfo(Path.Combine(ModuleDir.FullName, "configmanager.xml"));
        Players = new List<CSPracc.DataModules.Player>();
        match = new Match();
        XmlSerializer serializer = new XmlSerializer(typeof(ConfigManager));
        if (configManagerFile.Exists)
        {
            Config = (ConfigManager)serializer.Deserialize(File.OpenRead(configManagerFile.FullName));
        }
        else
        {
            Config = new ConfigManager();
            Config.RconPassword = "geheim";
            Config.LoggingFile = "Logging.txt";
            Config.Admins = new List<string>();
            Config.SavedNades = new List<SavedNade>();
            Config.Admins.Add("steamid1234");
            Config.SavedNades.Add(new SavedNade(new Vector(0, 0, 0), new QAngle(0, 0, 0), new Vector(0, 0, 0), "test nade", "test", "de_test"));
            WriteConfig(Config);
        }
        RegisterListener<Listeners.OnMapStart>((mapName) =>
        {
            Reset();
        });
    }

    public static void WriteConfig(ConfigManager config)
    {
        XmlSerializer serializer = new XmlSerializer(typeof(ConfigManager));
        if (configManagerFile.Exists)
        {
            configManagerFile.Delete();
        }
        serializer.Serialize(File.OpenWrite(configManagerFile.FullName), config);
    }

    [ConsoleCommand("css_rcon_password", "allow temporary admin control")]
    public void OnRconPassword(CCSPlayerController? player, CommandInfo command)
    {
        if (player == null) return;
        if (!player.PlayerPawn.IsValid) return;
        rconPassword(player, command.ArgString);
    }

    #region ChatFiltering
    /// <summary>
    /// Parsing player chat text, look for commands etc
    /// </summary>
    /// <param name="event"></param>
    /// <param name="info"></param>
    /// <returns></returns>
    [GameEventHandler(HookMode.Pre)]
    public HookResult OnPlayerChat(EventPlayerChat @event, GameEventInfo info)
    {
        if(!@event.Text.StartsWith("."))
        {
            return HookResult.Continue;
        }
        info.DontBroadcast = true;
        var player = new CCSPlayerController(NativeAPI.GetEntityFromIndex(@event.Userid));
        if (!player.IsValid)
        {
            Logging.LogMessage("EventPlayerChat invalid entity");
            return HookResult.Continue;
        }
        string commandWithArgs = ReplaceAlias(@event.Text);
        Logging.LogMessage("found command " + commandWithArgs);
        string command = string.Empty;
        //commandWithArgs.Substring(0, commandWithArgs.IndexOf(' '));
        string args = string.Empty ;
        try
        {
            //detect arguments
            if(commandWithArgs.Contains(' '))
            {
                command = commandWithArgs.Substring(0, commandWithArgs.IndexOf(" "));
                if(command.IndexOf(' ') != commandWithArgs.Length -1)
                {
                    args = commandWithArgs.Substring(commandWithArgs.IndexOf(' ') + 1);          
                }
                else
                {
                    args = "";
                }
            }
            else
            {
                command = commandWithArgs;
            }
        }
        catch(Exception ex)
        {

        }
        Logging.LogMessage($"OnPlayerChat found command {commandWithArgs}");
        switch (command)
        {
            case PRACC_COMMAND.HELP:
                {
                PrintHelp(player);
                break;
                }
            case PRACC_COMMAND.MODE:
                {
                    ShowModeMenu(player);
                    break;
                }
            case PRACC_COMMAND.SPAWN:
                {
                    if (match.CurrentMode != enums.PluginMode.Pracc) break;
                    SpawnManager.TeleportToSpawn(player, args);
                    break;
                }
            case PRACC_COMMAND.WARMUP:
                {
                    match.Rewarmup(player);
                    break;
                }
            case PRACC_COMMAND.PAUSE:
                {
                    match.Pause();
                    break;
                }
            case PRACC_COMMAND.UNPAUSE:
                {
                    match.Unpause();
                    break;
                }
            case PRACC_COMMAND.FORCEREADY:
                {
                    match.Start(player); 
                    break;
                }
            case PRACC_COMMAND.COACH:
                {
                    match.AddCoach(player);
                    break;
                }
            case PRACC_COMMAND.STOPCOACH:
                {
                    match.StopCoach(player);
                    break;
                }
            case PRACC_COMMAND.FAKERCON:
                {
                    OnFakeRcon(player,args); 
                    break;
                }
            case PRACC_COMMAND.BACKUPMENU:
                {
                    OnLoadBackupMenu(player); 
                    break;
                }
            case PRACC_COMMAND.NADES:
                {
                    if (match.CurrentMode != enums.PluginMode.Pracc) break;
                    ChatMenus.OpenMenu(player, NadeManager.NadeMenu);
                    break;
                }
            case PRACC_COMMAND.SAVE:
                {
                    NadeManager.AddGrenade(player, args);
                    break;
                }
            case PRACC_COMMAND.MAP:
            {
                    match.ChangeMap(player,args);
                    break;
            }
        }

        return HookResult.Changed;
    }
    #endregion

    #region commands
    public void ShowModeMenu(CCSPlayerController? player)
    {
        if (player == null)
        {
            Server.ExecuteCommand("say player is null");
            return;
        }
        if (!player.PlayerPawn.IsValid)
        {
            Server.ExecuteCommand("say player not valid");
            return;
        }
        if(!player.IsAdmin())
        {
            player.PrintToCenter("Only admins can execute this command!");
            return;
        }
        ChatMenus.OpenMenu(player, ModeMenu); 
    }

    private void rconPassword(CCSPlayerController? player,string password)
    {
        if (RconPassword != password)
        {
            player.PrintToCenter("Invalid Password");
            return;
        }
        AdminList.Add(new SteamID(player.SteamID));
    }

    public void OnFakeRcon(CCSPlayerController? player,string args)
    {
        if (player == null) return;
        if (!player.PlayerPawn.IsValid) return;
        if (!player.IsAdmin())
        {
            player.PrintToCenter("Only admins can execute this command!");
            return;
        }
        Server.ExecuteCommand(args);
    }

    public void OnLoadBackupMenu(CCSPlayerController? player)
    {
        if (player == null) return;
        if (!player.PlayerPawn.IsValid) return;
        if (match.CurrentMode != enums.PluginMode.Match)
        {
            player?.PrintToCenter("Command can only be used in Match mode.");
            return;
        }
        if (!player.IsAdmin())
        {
            player.PrintToCenter("Only admins can execute this command!");
            return;
        }
        var backupMenu = new ChatMenu("Backup Menu");
        var handleGive = (CCSPlayerController player, ChatMenuOption option) => ModeMenuOption(player, option.Text);

        List<FileInfo> Backupfiles = new List<FileInfo>();
             
        Backupfiles = CsgoDir.GetFiles("backup_round*").ToList();
        foreach(var file in Backupfiles)
        {
            string round = file.Name.Substring(file.Name.Length - 6,2);
            backupMenu.AddMenuOption(round, handleGive);
        }
        ChatMenus.OpenMenu(player, backupMenu);
    }

    public void PrintHelp(CCSPlayerController? player)
    {
        List<string> message = new List<string>();
        message.Add($" {CSPracc.DataModules.consts.Strings.ChatTag} Command list:");
        message.Add($" {ChatColors.Green} {PRACC_COMMAND.PAUSE} {ChatColors.White} - Switching mode. Available modes: standard - unloading changes, pracc - loading practice config, match - loading match config");
        message.Add($" {ChatColors.Green} {PRACC_COMMAND.UNPAUSE} {ChatColors.White} - Starting the match. Works only in the warmup during match mode.");
        message.Add($" {ChatColors.Green} {PRACC_COMMAND.STOP}  {ChatColors.White} - Stopping the match.");
        message.Add($" {ChatColors.Green} {PRACC_COMMAND.WARMUP} {ChatColors.White} - (Re)starting warmup. Works only during match.");
        message.Add($" {ChatColors.Green} {PRACC_COMMAND.RESTART}  {ChatColors.White} - Restarting the match. Works only during a live match.");
        message.Add($" {ChatColors.Green} {PRACC_COMMAND.READY}  {ChatColors.White} - Ready up as a player. Works only during a warmup of a  match.");
        message.Add($" {ChatColors.Green} {PRACC_COMMAND.FORCEREADY}  {ChatColors.White} - Forcing all players to ready up. Works only during a warmup of a  match.");
        message.Add($" {ChatColors.Green} {PRACC_COMMAND.SPAWN}  {ChatColors.White} - Works only in practice mode. Teleports you to spawn number X");
        message.Add($" {ChatColors.Green} {PRACC_COMMAND.HELP}  {ChatColors.White} - Prints the help command.");
        foreach (string s in message)
        {
            player?.PrintToChat(s);
        }
    }

    #endregion

    #region MenuOptions
    private void BackupMenuOption(string BackupFile)
    {
        //ToDo Load Backup
    }


    private void ModeMenuOption(CCSPlayerController player,string optionText)
    {
        switch(optionText)
        {
            case "Pracc":
                this.match.SwitchTo(enums.PluginMode.Pracc);
                break;
            case "Match":
                DeleteBackupFiles();
                this.match.SwitchTo(enums.PluginMode.Match);
                break;
            case "Help":
                PrintHelp(player);
                break;

        }

    }
    #endregion

    #region HelperMethods

    /// <summary>
    /// detects input for aliases and replaces them
    /// </summary>
    /// <param name="alias">input</param>
    /// <returns>returns string with full command</returns>
    public string ReplaceAlias(string alias)
    {
        string ShortCommand = string.Empty;
        string LongCommand = alias;

        //ToDo Implement
        return LongCommand;
    }


    private void DeleteBackupFiles()
    {
        foreach (FileInfo file in CsgoDir.GetFiles("backup_round*"))
        {
            file.Delete();
        }
    }

    /// <summary>
    /// Resetting plugin settings
    /// </summary>
    private void Reset()
    {
        match.SwitchTo(match.CurrentMode, true);
    }

    #endregion

}
