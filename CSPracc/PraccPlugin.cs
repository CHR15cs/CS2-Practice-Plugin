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
using CSPracc.Managers;
using static System.Formats.Asn1.AsnWriter;
using System.Drawing;

public class CSPraccPlugin : BasePlugin
{
   public static CSPraccPlugin? Instance { get; private set; }

    List<CSPracc.DataModules.Player>? Players;
    
    private Match? match;
    private static List<SteamID>? _adminList = null;
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

    private BotManager? BotManager;

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


    private static DirectoryInfo? _moduleDir;
    public static DirectoryInfo ModuleDir => _moduleDir!;

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
   private static DirectoryInfo? _csgoDir = null;
    public static DirectoryInfo Cs2Dir
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

    ChatMenu? _modeMenu = null;
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

    private static FileInfo? configManagerFile = null;

    public static ConfigManager? Config;
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
            Config = serializer.Deserialize(File.OpenRead(configManagerFile.FullName)) as ConfigManager;
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


RegisterListener<Listeners.OnEntitySpawned>(entity =>
{
    if(match.CurrentMode == Enums.PluginMode.Pracc)
    {
        var designerName = entity.DesignerName;
        if (designerName != "smokegrenade_projectile") return;

        var projectile = new CSmokeGrenadeProjectile(entity.Handle);

        Server.NextFrame(() =>
        {
            CCSPlayerController player = new CCSPlayerController(projectile.Thrower.Value.Controller.Value.Handle);
            projectile.SmokeColor.X = getTeamColor(player).R;
            projectile.SmokeColor.Y = getTeamColor(player).G;
            projectile.SmokeColor.Z = getTeamColor(player).B;
        });
    }

});

        BotManager = new BotManager();
        Instance = this;
    }

    private System.Drawing.Color getTeamColor(CCSPlayerController playerController)
    {
        switch (playerController.CompTeammateColor)
        {
            case 1:
                return Color.Green;
            case 2:
                return Color.Yellow;
            case 3:
                return Color.Orange;
            case 4:
                return Color.Pink;
            case 5:
                return Color.LightBlue;
            default:
                return Color.Red;
        }
    }

    public static void WriteConfig(ConfigManager config)
    {
        XmlSerializer serializer = new XmlSerializer(typeof(ConfigManager));
        if (configManagerFile!.Exists)
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
            Logging.LogMessage($"{ex.Message}");
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
            case PRACC_COMMAND.FAKERCON:
                {
                    OnFakeRcon(player,args); 
                    break;
                }          
            case PRACC_COMMAND.MAP:
            {
                    match!.ChangeMap(player,args);
                    break;
            }
            case PRACC_COMMAND.PRACC:
                {
                    if (!player.IsAdmin())
                    {
                        player.PrintToCenter("Only admins can execute this command!");
                        return HookResult.Continue;
                    }
                    this.match!.SwitchTo(Enums.PluginMode.Pracc);
                    break;
                }
            case PRACC_COMMAND.MATCH:
                {
                    if (!player.IsAdmin())
                    {
                        player.PrintToCenter("Only admins can execute this command!");
                        return HookResult.Continue;
                    }
                    RoundRestoreManager.CleanupOldFiles();
                    this.match!.SwitchTo(Enums.PluginMode.Match);
                    break;
                }
            case PRACC_COMMAND.SWAP:
                {
                    Server.ExecuteCommand(COMMANDS.SWAP_TEAMS);
                    break;
                }
            default:
            {
                    if(match!.CurrentMode == Enums.PluginMode.Match)
                    {
                        MatchCommands(player,command,args);
                    }
                    if(match!.CurrentMode == Enums.PluginMode.Pracc)
                    {
                        PracticeCommands(player,command,args);
                    }
                    break;
            }
        }

        return HookResult.Changed;
    }

    private void PracticeCommands(CCSPlayerController player,string command,string args)
    {
        Logging.LogMessage($"OnPlayerChat found command {command} args {args}");
        switch (command)
        {
            case PRACC_COMMAND.SPAWN:
                {
                    if (match!.CurrentMode != Enums.PluginMode.Pracc) break;
                    SpawnManager.TeleportToSpawn(player, args);
                    break;
                }
            case PRACC_COMMAND.NADES:
                {
                    if (match!.CurrentMode != Enums.PluginMode.Pracc) break;
                    ChatMenus.OpenMenu(player, NadeManager.NadeMenu);
                    break;
                }
            case PRACC_COMMAND.SAVE:
                {
                    NadeManager.AddGrenade(player, args);
                    break;
                }
            case PRACC_COMMAND.BOT:
                {
                    BotManager.AddBot(player);
                    break;
                }
            case PRACC_COMMAND.BOOST:
                {
                    BotManager.Boost(player);
                    break;
                }
            case PRACC_COMMAND.NOBOT:
                {
                    BotManager.NoBot(player);
                    break;
                }
            case PRACC_COMMAND.CLEARBOTS:
                {
                    BotManager.ClearBots(player);
                    break;
                }
            case PRACC_COMMAND.WATCHME:
                {
                    //Untestet
                    var playerEntities = Utilities.FindAllEntitiesByDesignerName<CCSPlayerController>("cs_player_controller");
                    foreach(var playerEnt in playerEntities)
                    {
                        if(playerEnt == null) continue;
                        if(!playerEnt.IsValid) continue;
                        if(playerEnt.UserId == player.UserId) continue;
                        if (playerEnt.IsBot) continue;
                        playerEnt.ChangeTeam(CsTeam.Spectator);
                        Logging.LogMessage($"Switching {playerEnt.PlayerName} to spectator");
                    }
                    break;
                }
            case PRACC_COMMAND.CROUCHBOT:
                {
                    BotManager.CrouchBot(player);
                    break;
                }
            case PRACC_COMMAND.CROUCHBOOST:
                {
                    BotManager.CrouchingBoostBot(player);
                    break;
                }
            case PRACC_COMMAND.GOT:
                {
                    player.ChangeTeam(CsTeam.Terrorist);
                    break;
                }
            case PRACC_COMMAND.GOCT:
                {
                    player.ChangeTeam(CsTeam.CounterTerrorist);
                    break;
                }
            case PRACC_COMMAND.GOSPEC:
                {
                    player.ChangeTeam(CsTeam.Spectator);
                    break;
                }
        }
    }

    private void MatchCommands(CCSPlayerController player, string command, string args)
    {
        Logging.LogMessage($"OnPlayerChat found command {command} args {args}");
        switch (command)
        {
            case PRACC_COMMAND.WARMUP:
                {
                    match!.Rewarmup(player);
                    break;
                }
            case PRACC_COMMAND.PAUSE:
                {
                    match!.Pause();
                    break;
                }
            case PRACC_COMMAND.UNPAUSE:
                {
                    match!.Unpause(player);
                    break;
                }
            case PRACC_COMMAND.FORCEREADY:
                {
                    match!.Start(player);
                    break;
                }
            case PRACC_COMMAND.COACH:
                {
                    match!.AddCoach(player);
                    break;
                }
            case PRACC_COMMAND.STOPCOACH:
                {
                    match!.StopCoach(player);
                    break;
                }
            case PRACC_COMMAND.BACKUPMENU:
                {
                    match!.RestoreBackup(player);
                    break;
                }
            case PRACC_COMMAND.FORCEUNPAUSE:
                {
                    match!.ForceUnpause(player);
                    break;
                }
            case PRACC_COMMAND.RESTART:
                {
                    match!.Restart(player);
                    break;
                }
        }
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
        password = password.Trim();
        if (RconPassword != password)
        {
            player!.PrintToCenter("Invalid Password");
            return;
        }
        AdminList.Add(new SteamID(player!.SteamID));
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

    private void ModeMenuOption(CCSPlayerController player,string optionText)
    {
        switch(optionText)
        {
            case "Pracc":
                this.match!.SwitchTo(Enums.PluginMode.Pracc);
                break;
            case "Match":
                RoundRestoreManager.CleanupOldFiles();
                this.match!.SwitchTo(Enums.PluginMode.Match);
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




    /// <summary>
    /// Resetting plugin settings
    /// </summary>
    private void Reset()
    {
        match!.SwitchTo(match!.CurrentMode, true);
    }

    #endregion

    #region events
    [GameEventHandler(HookMode.Post)]
    public HookResult OnPlayerSpawn(EventPlayerSpawn @event, GameEventInfo info)
    {
        Logging.LogMessage("OnPlayerSpawn");
        if(match!.CurrentMode == Enums.PluginMode.Pracc) 
        { 
            BotManager!.OnPlayerSpawn(@event, info);
        }
        if (match!.CurrentMode == Enums.PluginMode.Match)
        {
            Logging.LogMessage("Mode Match");
            match!.OnPlayerSpawn(@event, info);
        }
        return HookResult.Continue;
    }

  
    [GameEventHandler]
    public HookResult OnPlayerBlind(EventPlayerBlind @event, GameEventInfo info)
    {
        if (match!.CurrentMode == Enums.PluginMode.Pracc)
        {
            Methods.MsgToServer($"Flash duration: {@event.BlindDuration.ToString("0.00")}s");
        }
        return HookResult.Continue;
    }
    #endregion
}
