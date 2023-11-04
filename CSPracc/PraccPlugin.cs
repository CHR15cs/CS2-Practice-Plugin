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

    List<Position> SpawnsCT;
    List<Position> SpawnsT;
    List<CSPracc.DataModules.Player>? Players;
    List<CSPracc.DataModules.SavedNade>? Nades;
    private Match match;
    private FileInfo GrenadeFile;
    private static FileInfo adminFile;
    public static List<SteamID> AdminList = new List<SteamID>();


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
                //ToDo read out rcon password
                _rconPassword = "geheim";
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

    ChatMenu _nadeMenu = null;
    ChatMenu NadeMenu
    {
        get
        {
            var NadesMenu = new ChatMenu("Nade Menu");
            var handleGive = (CCSPlayerController player, ChatMenuOption option) => TeleportPlayer(player, option.Text);

            foreach (var nade in Nades)
            {
                if (nade.Map == Server.MapName)
                {
                    NadesMenu.AddMenuOption(nade.Title, handleGive);
                }
            }
            return NadesMenu;
        }
    }

    ConfigManager ConfigManager;
    #endregion

    public override void Load(bool hotReload)      
    {
        base.Load(hotReload);
        _moduleDir = new DirectoryInfo(ModuleDirectory);
        Logging logging = new Logging(new FileInfo(Path.Combine(ModuleDir.FullName, "Logging.txt")));
        adminFile = new FileInfo(Path.Combine(CSPraccPlugin.ModuleDir.FullName, "admin.ini"));
        Players = new List<CSPracc.DataModules.Player>();
        Nades = new List<SavedNade>();
        SpawnsCT = new List<Position>();
        SpawnsT = new List<Position>();
        match = new Match();
        XmlSerializer serializer = new XmlSerializer(typeof(ConfigManager));
        FileInfo configManagerFile = new FileInfo(Path.Combine(ModuleDir.FullName, "configmanager.xml"));
        if(configManagerFile.Exists)
        {
            ConfigManager = (ConfigManager)serializer.Deserialize(File.OpenRead(configManagerFile.FullName));
        }
        else
        {
            ConfigManager = new ConfigManager();
            ConfigManager.RconPassword = "geheim";
            ConfigManager.LoggingFile = "Logging.txt";
            ConfigManager.Admins = new List<string>();
            ConfigManager.SavedNades = new List<SavedNade>();
            ConfigManager.Admins.Add("steamid1234");
            ConfigManager.SavedNades.Add(new SavedNade(new Vector(0,0,0),new QAngle(0,0,0),new Vector(0,0,0),"test nade","test","de_test"));
            if(configManagerFile.Exists)
            {
                configManagerFile.Delete();
            }
            serializer.Serialize(File.OpenWrite(configManagerFile.FullName), ConfigManager);
        }
        
        
        LoadFiles();
        RegisterListener<Listeners.OnMapStart>((mapName) =>
        {
            Reset();
        });
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
                    Server.ExecuteCommand("say command: " + command);
                    Server.ExecuteCommand("say args: " + args);              
                }
                else
                {
                    args = "";
                }
            }
            else
            {
                command = commandWithArgs;
                Server.ExecuteCommand("say command: " + command);
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
                        OnSpawn(player, args);
                    break;
                }
            case PRACC_COMMAND.WARMUP:
                {
                    OnWarmup(player);
                    break;
                }
            case PRACC_COMMAND.PAUSE:
                {
                    OnPause(player);
                    break;
                }
            case PRACC_COMMAND.UNPAUSE:
                {
                    OnUnPause(player);
                    break;
                }
            case PRACC_COMMAND.FORCEREADY:
                {
                    OnForceReady(player); 
                    break;
                }
            case PRACC_COMMAND.COACH:
                {
                    OnCoaching(player); 
                    break;
                }
            case PRACC_COMMAND.STOPCOACH:
                {
                    OnStopCoaching(player);
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
                    OnNades(player);
                    break;
                }


        }

        return HookResult.Changed;
    }
    #endregion

    #region commands
    public void ShowModeMenu(CCSPlayerController? player)
    {
        Server.ExecuteCommand("say command show mode menu reached");
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
    public void OnSpawn(CCSPlayerController? player, string args)
    {
        if(match.CurrentMode != enums.PluginMode.Pracc) return;
        if(SpawnsT.Count == 0) GetSpawns();
        if (player == null) return;
        if (!player.PlayerPawn.IsValid) return;
        int number = -1;

        try
        {
            number = Convert.ToInt32(args);
            number--;
        }
        catch (Exception ex)
        {
            player.PrintToCenter("invalid parameter");
            return;
        }
      
        if(player.TeamNum == (byte)CsTeam.CounterTerrorist)
        {
            if(SpawnsCT.Count <= number)
            {
                Server.ExecuteCommand($"say insufficient number of spawns found. spawns {SpawnsCT.Count} - {number}");
                return;
            }
            player.PlayerPawn.Value.Teleport(SpawnsCT[number].PlayerPosition, SpawnsCT[number].PlayerAngle, new Vector(0, 0, 0));
            player.PrintToCenter($"Teleporting to {number}");
        }
        if (player.TeamNum == (byte)CsTeam.Terrorist)
        {
            if (SpawnsT.Count <= number)
            {
                Server.ExecuteCommand($"say insufficient number of spawns found. spawns {SpawnsT.Count} - {number}");
                return;
            }
            player.PrintToCenter($"Teleporting to {number}");
            player.PlayerPawn.Value.Teleport(SpawnsT[number].PlayerPosition, SpawnsT[number].PlayerAngle, new Vector(0, 0, 0));
        }
    }

    public void OnPause(CCSPlayerController? player)
    {
        if (match.CurrentMode != enums.PluginMode.Match) return;
        if (player == null) return;
        if (!player.PlayerPawn.IsValid) return;
        match.Pause();
    }
    public void OnUnPause(CCSPlayerController? player)
    {
        if (match.CurrentMode != enums.PluginMode.Match) return;
        if (player == null) return;
        if (!player.PlayerPawn.IsValid) return;
        match.Unpause();
    }

    public void OnForceReady(CCSPlayerController? player)
    {
        if (match.CurrentMode != enums.PluginMode.Match) return;
        if (player == null) return;
        if (!player.PlayerPawn.IsValid) return;
        if (!player.IsAdmin())
        {
            player.PrintToCenter("Only admins can execute this command!");
            return;
        }
        match.Start();
    }

    public void OnWarmup(CCSPlayerController? player)
    {
        if (match.CurrentMode != enums.PluginMode.Match) return;
        if (player == null) return;
        if (!player.PlayerPawn.IsValid) return;
        if (!player.IsAdmin())
        {
            player.PrintToCenter("Only admins can execute this command!");
            return;
        }
        match.Rewarmup();
    }

    public void OnCoaching(CCSPlayerController? player)
    {
        if (match.CurrentMode != enums.PluginMode.Match) return;
        if (player == null) return;
        if (!player.PlayerPawn.IsValid) return;
        match.AddCoach(player);
    }

    public void OnStopCoaching(CCSPlayerController? player)
    {
        if (match.CurrentMode != enums.PluginMode.Match) return;
        if (player == null) return;
        if (!player.PlayerPawn.IsValid) return;
        match.StopCoach(player);
    }

    public void OnNades(CCSPlayerController? player)
    {
        if (match.CurrentMode != enums.PluginMode.Pracc) return;
        if (player == null) return;
        if (!player.PlayerPawn.IsValid) return;
        ChatMenus.OpenMenu(player, NadeMenu);
    }

    public void OnSave(CCSPlayerController? player,string args)
    {
        if (match.CurrentMode != enums.PluginMode.Pracc) return;
        if (player == null) return;
        if (!player.PlayerPawn.IsValid) return;

        var absOrigin = player.PlayerPawn.Value.CBodyComponent!.SceneNode!.AbsOrigin;
        string name = args;
        Nades.Add(new SavedNade(absOrigin, player.PlayerPawn.Value.EyeAngles, null, name, "", Server.MapName));
        using (StreamWriter sw = new StreamWriter(File.OpenWrite(GrenadeFile.FullName)))
        {
            foreach (var nade in Nades)
            {
                sw.WriteLine(nade);
            }
        }

    }
    public void OnHelp(CCSPlayerController? player)
    {
        if (player == null) return;
        if (!player.PlayerPawn.IsValid) return;
        PrintHelp(player);     
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

    private void TeleportPlayer(CCSPlayerController player, string grenadeName)
    {
        foreach (var nade in Nades)
        {
            if (nade.Map == Server.MapName)
            {
                if (nade.Title == grenadeName)
                {
                    player.PlayerPawn.Value.Teleport(nade.PlayerPosition, new QAngle(nade.PlayerAngle.X, nade.PlayerAngle.Y, nade.PlayerAngle.Z), nade.Velocity);
                }
            }
        }
    }

    /// <summary>
    /// reading out spawns from map
    /// </summary>
    public void GetSpawns()
    {
        var spawnsct = Utilities.FindAllEntitiesByDesignerName<CBaseEntity>("info_player_counterterrorist");

        foreach (var spawn in spawnsct)
        {
            if (spawn.IsValid)
            {
                SpawnsCT.Add(new Position(spawn.CBodyComponent!.SceneNode!.AbsOrigin, spawn.CBodyComponent.SceneNode.AbsRotation));
            }
        }
        var spawnst = Utilities.FindAllEntitiesByDesignerName<CBaseEntity>("info_player_terrorist");
        foreach (var spawn in spawnst)
        {
            if (spawn.IsValid)
            {
                SpawnsT.Add(new Position(spawn.CBodyComponent!.SceneNode!.AbsOrigin, spawn.CBodyComponent.SceneNode.AbsRotation));
            }
        }

    }
    /// <summary>
    /// Load Required Files
    /// </summary>
    private void LoadFiles()
    {
        if (adminFile.Exists)
        {
            Logging.LogMessage("PRACC, AdminIni Exsits");
            List<string> steamIdsAdmin = File.ReadAllLines(adminFile.FullName).ToList();
            foreach (string steamId in steamIdsAdmin)
            {
                AdminList.Add(new SteamID(steamId));

            }
        }
        Nades = new List<SavedNade> { };
        GrenadeFile = new FileInfo(Path.Combine(ModuleDirectory, "GrenadeFile.cfg"));
        if (GrenadeFile.Exists)
        {
           
            Logging.LogMessage("GrenadeFile exists");
            using (StreamReader sr = new StreamReader(File.OpenRead(GrenadeFile.FullName)))
            {
                Nades.Add(new SavedNade(sr.ReadLine()));
            }
        }
    }

    /// <summary>
    /// Resetting plugin settings
    /// </summary>
    private void Reset()
    {
        SpawnsCT.Clear();
        SpawnsT.Clear();
        match.SwitchTo(match.CurrentMode, true);
    }

    #endregion

}
