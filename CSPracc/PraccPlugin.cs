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
using System.Drawing;

public class CSPraccPlugin : BasePlugin
{
   public static CSPraccPlugin? Instance { get; private set; }
    
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
        XmlSerializer serializer = new XmlSerializer(typeof(ConfigManager));
        if (configManagerFile.Exists)
        {
            Config = serializer.Deserialize(File.OpenRead(configManagerFile.FullName)) as ConfigManager;
            DemoManager.DemoManagerSettings = Config!.DemoManagerSettings;
        }
        else
        {
            Config = new ConfigManager();
            Config.RconPassword = "geheim";
            Config.LoggingFile = "Logging.txt";
            Config.Admins = new List<string>();
            Config.SavedNades = new List<SavedNade>();
            Config.Admins.Add("steamid1234");
            Config.SavedNades.Add(new SavedNade(new Vector(0, 0, 0), new QAngle(0, 0, 0), new Vector(0, 0, 0), "test nade", "test", "de_test", 1));
            WriteConfig(Config);
        }
        RegisterListener<Listeners.OnMapStart>((mapName) =>
        {
            Reset();
        });


RegisterListener<Listeners.OnEntitySpawned>(entity =>
{
    if(Match.CurrentMode == Enums.PluginMode.Pracc)
    {     
        var designerName = entity.DesignerName;
        Logging.LogMessage($"Entity spawned : {designerName}");
        if (designerName != "smokegrenade_projectile") return;

        var projectile = new CSmokeGrenadeProjectile(entity.Handle);

        Server.NextFrame(() =>
        {
            CCSPlayerController player = new CCSPlayerController(projectile.Thrower.Value.Controller.Value.Handle);
            projectile.SmokeColor.X = (float)getTeamColor(player).R;
            projectile.SmokeColor.Y = (float)getTeamColor(player).G;
            projectile.SmokeColor.Z = (float)getTeamColor(player).B;
            Logging.LogMessage($"smoke color {projectile.SmokeColor}");
        });
    }
});

        BotManager = new BotManager();
        Instance = this;
    }

    private System.Drawing.Color getTeamColor(CCSPlayerController playerController)
    {
        Logging.LogMessage($"Getting Color of player {playerController.CompTeammateColor}");
        switch (playerController.CompTeammateColor)
        {
            case 1:
                return Color.FromArgb(50,255,0);
            case 2:
                return Color.FromArgb(255,255,0);
            case 3:
                return Color.FromArgb(255,132,0);
            case 4:
                return Color.FromArgb(255, 0, 255);
            case 0:
                return Color.FromArgb(0,187,255);
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
                    Match.ChangeMap(player,args);
                    break;
            }
            case PRACC_COMMAND.PRACC:
                {
                    if (!player.IsAdmin())
                    {
                        player.PrintToCenter("Only admins can execute this command!");
                        return HookResult.Continue;
                    }
                    Match.SwitchTo(Enums.PluginMode.Pracc);
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
                    Match.SwitchTo(Enums.PluginMode.Match);
                    break;
                }
            case PRACC_COMMAND.SWAP:
                {
                    Server.ExecuteCommand(COMMANDS.SWAP_TEAMS);
                    break;
                }
            case PRACC_COMMAND.ALIAS:
                {
                    if(!player.IsAdmin())
                    {
                        player.PrintToCenter("Only admins can execute this command!");
                        return HookResult.Continue;
                    }
                    if(!GetArgumentList(args,out List<string> ArgumentList))
                    {
                        player.PrintToCenter("Invalid amout of parameters. Command need to be used .alias <newAlias> <commandTobeExecuted>");
                    }
                    if(ArgumentList.Count != 2)
                    {
                        player.PrintToCenter("Invalid amout of parameters. Command need to be used .alias <newAlias> <commandTobeExecuted>");
                    }
                    foreach(CommandAlias cAlias in Config!.CommandAliases)
                    {
                        if(cAlias.Alias == (ArgumentList[0]))
                        {
                            player.PrintToCenter($"Alias {cAlias.Alias} is already existing. Use .ralias <alias> to remove alias.");
                            return HookResult.Continue;
                        }
                    }
                    Config!.AddCommandAlias(new CommandAlias(ArgumentList[0], ArgumentList[1]));
                    player.PrintToCenter($"Added alias {ArgumentList[0]} for command {ArgumentList[1]}");
                    CSPraccPlugin.WriteConfig(CSPraccPlugin.Config);
                    break;
                }
            case PRACC_COMMAND.REMOVEALIAS:
                {
                    if (!player.IsAdmin())
                    {
                        player.PrintToCenter("Only admins can execute this command!");
                        return HookResult.Continue;
                    }
                    args = args.Trim();
                    if(args.Length == 0)
                    {
                        player.PrintToCenter("Invalid command arguments");
                    }
                    for(int i = 0; i < Config!.CommandAliases.Count;i++)
                    {
                        if (Config!.CommandAliases[i].Alias == args)
                        {
                            Config!.CommandAliases.RemoveAt(i);
                            player.PrintToCenter($"Removed alias {args}");
                            break;
                        }
                    }
                    break;
                }
            case PRACC_COMMAND.DEMO:
                {
                    DemoManager.OpenDemoManagerMenu(player);
                    break;
                }
            default:
            {
                    if(Match.CurrentMode == Enums.PluginMode.Match)
                    {
                        MatchCommands(player,command,args);
                    }
                    if(Match.CurrentMode == Enums.PluginMode.Pracc)
                    {
                        PracticeCommands(player,command,args);
                    }
                    break;
            }
        }

        return HookResult.Changed;
    }

    private bool GetArgumentList(string Argument, out List<string> arguments)
    {
        Logging.LogMessage($"Getting command arguments of string {Argument}");
        arguments = new List<string>();
        if(String.IsNullOrEmpty(Argument))
        {
            return false;
        }      
        do
        {
            //Remove Leading or Trailing whitespaces
            Argument = Argument.Trim();
            int index = Argument.IndexOf(' ');
            if(index == -1)
            {
                arguments.Add(Argument);
                Argument = string.Empty;
                break;
            }
            string foundArgument = Argument.Substring(0, index);
            arguments.Add(foundArgument);
            Logging.LogMessage($"Adding argument {foundArgument}");
            Argument = Argument.Substring(index);
        } while (Argument.Length > 0 && Argument != String.Empty);
        return true;
    }

    private void PracticeCommands(CCSPlayerController player,string command,string args)
    {
        Logging.LogMessage($"OnPlayerChat found command {command} args {args}");
        switch (command)
        {
            case PRACC_COMMAND.SPAWN:
                {
                    SpawnManager.TeleportToSpawn(player, args);
                    break;
                }
            case PRACC_COMMAND.TSPAWN:
                {
                    SpawnManager.TeleportToTeamSpawn(player, args, CsTeam.Terrorist);
                    break;
                }
            case PRACC_COMMAND.CTSPAWN:
                {
                    SpawnManager.TeleportToTeamSpawn(player, args, CsTeam.CounterTerrorist);
                    break;
                }
            case PRACC_COMMAND.NADES:
                { 
                    ChatMenus.OpenMenu(player, NadeManager.NadeMenu);
                    break;
                }
            case PRACC_COMMAND.SAVE:
                {
                    NadeManager.AddGrenade(player, args);
                    break;
                }
            case PRACC_COMMAND.REMOVE:
                {
                    NadeManager.RemoveGrenade(player, args);
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
            case PRACC_COMMAND.CLEAR:
                {
                    RemoveGrenadeEntities();
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
                    Match.Rewarmup(player);
                    break;
                }
            case PRACC_COMMAND.PAUSE:
                {
                    Match.Pause();
                    break;
                }
            case PRACC_COMMAND.UNPAUSE:
                {
                    Match.Unpause(player);
                    break;
                }
            case PRACC_COMMAND.READY:
                {
                    Match.Ready(player);
                    break;
                }
            case PRACC_COMMAND.UNREADY:
                {
                    Match.UnReady(player);
                    break;
                }
            case PRACC_COMMAND.FORCEREADY:
                {
                    Match.Start(player);
                    break;
                }
            case PRACC_COMMAND.COACH:
                {
                    Match.AddCoach(player);
                    break;
                }
            case PRACC_COMMAND.STOPCOACH:
                {
                    Match.StopCoach(player);
                    break;
                }
            case PRACC_COMMAND.BACKUPMENU:
                {
                    Match.RestoreBackup(player);
                    break;
                }
            case PRACC_COMMAND.FORCEUNPAUSE:
                {
                    Match.ForceUnpause(player);
                    break;
                }
            case PRACC_COMMAND.RESTART:
                {
                    Match.Restart(player);
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
                Match.SwitchTo(Enums.PluginMode.Pracc);
                break;
            case "Match":
                RoundRestoreManager.CleanupOldFiles();
                Match.SwitchTo(Enums.PluginMode.Match);
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
        string ShortCommand = alias;
        string LongCommand = alias;
        bool aliasFound = false;
        do
        {
            aliasFound = false;
            foreach (CommandAlias calias in Config!.CommandAliases)
            {
                if (calias.Alias == ShortCommand)
                {
                    ShortCommand = calias.Command;
                    aliasFound = true;
                }
            }
        } while (aliasFound);
        LongCommand = ShortCommand;
        return LongCommand;
    }

    /// <summary>
    /// Resetting plugin settings
    /// </summary>
    private void Reset()
    {
        Match.SwitchTo(Match.CurrentMode, true);
    }

    private void RemoveGrenadeEntities()
    {
        var smokes = Utilities.FindAllEntitiesByDesignerName<CSmokeGrenadeProjectile>("smokegrenade_projectile");
        foreach (var entity in smokes)
        {
            if (entity != null)
            {
                entity.Remove();
            }
        }
        var mollys = Utilities.FindAllEntitiesByDesignerName<CSmokeGrenadeProjectile>("molotov_projectile");
        foreach (var entity in mollys)
        {
            if (entity != null)
            {
                entity.Remove();
            }
        }
        var inferno = Utilities.FindAllEntitiesByDesignerName<CSmokeGrenadeProjectile>("inferno");
        foreach (var entity in inferno)
        {
            if (entity != null)
            {
                entity.Remove();
            }
        }
    }

    #endregion

    #region events
    [GameEventHandler(HookMode.Post)]
    public HookResult OnPlayerSpawn(EventPlayerSpawn @event, GameEventInfo info)
    {
        Logging.LogMessage("OnPlayerSpawn");
        if (CSPracc.Match.CurrentMode == Enums.PluginMode.Pracc)
        {
            BotManager!.OnPlayerSpawn(@event, info);
        }
        if (CSPracc.Match.CurrentMode == Enums.PluginMode.Match)
        {
            Logging.LogMessage("Mode Match");
            CSPracc.Match.OnPlayerSpawn(@event, info);
        }
        return HookResult.Continue;
    }


    [GameEventHandler]
    public HookResult OnPlayerBlind(EventPlayerBlind @event, GameEventInfo info)
    {
        if (CSPracc.Match.CurrentMode == Enums.PluginMode.Pracc)
        {
            Methods.MsgToServer($"{@event.Attacker.PlayerName} flashed {@event.Userid.PlayerName} for {@event.BlindDuration.ToString("0.00")}s");
        }
        return HookResult.Continue;
    }

    [GameEventHandler]
    public HookResult OnPlayerHurt(EventPlayerHurt @event, GameEventInfo info)
    {
        if (CSPracc.Match.CurrentMode == Enums.PluginMode.Pracc)
        {
            Methods.MsgToServer($"Player {@event.Attacker.PlayerName} damaged {@event.Userid.PlayerName} for {@event.DmgHealth}hp with {@event.Weapon}");
        }
        return HookResult.Continue;
    }

    [GameEventHandler]
    public HookResult OnFreezeTimeEnd(EventRoundFreezeEnd @event, GameEventInfo info)
    {
        if (CSPracc.Match.CurrentMode == Enums.PluginMode.Match)
        {
           Match.OnFreezeTimeEnd(@event, info);
        }
        return HookResult.Continue;
    }

    [GameEventHandler]
    public HookResult OnMatchEnd(EventCsMatchEndRestart @event, GameEventInfo info)
    {
        if(Match.CurrentMode == Enums.PluginMode.Match)
        {
            if(DemoManager.DemoManagerSettings.isRecording)
            {
                DemoManager.StopRecording();
            }
        }
        return HookResult.Continue;
    }


    #endregion
}