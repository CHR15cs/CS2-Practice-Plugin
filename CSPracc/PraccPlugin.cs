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

public class CSPraccPlugin : BasePlugin
{
    List<Position> SpawnsCT;
    List<Position> SpawnsT;
    List<CSPracc.DataModules.Player>? Players;
    List<CSPracc.DataModules.SavedNade>? Nades;
    private Match match;
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

    public override void Load(bool hotReload)
    {
        Players = new List<CSPracc.DataModules.Player>();
        Nades = new List<SavedNade>();
        SpawnsCT = new List<Position>();
        SpawnsT = new List<Position>();
        base.Load(hotReload);
        match = new Match();    
    }

    public void GetSpawns()
    {
        var spawnsct = Utilities.FindAllEntitiesByDesignerName<CBaseEntity>("info_player_counterterrorist");
        
        foreach (var spawn in spawnsct)
        {
            if(spawn.IsValid)
            {
                // Schema.GetDeclaredClass<int>(spawn.Handle, "SpawnPoint", "m_iPriority");
                //int prio =Schema.GetSchemaValue<int>(spawn.Handle, "SpawnPoint", "m_iPriority");
                //Server.ExecuteCommand($"say found spawn {prio} {spawn.CBodyComponent?.SceneNode?.AbsOrigin.X}, {spawn.CBodyComponent?.SceneNode?.AbsOrigin.Y},{spawn.CBodyComponent?.SceneNode?.AbsOrigin.Z}");
                SpawnsCT.Add(new Position(spawn.CBodyComponent?.SceneNode?.AbsOrigin, spawn.CBodyComponent?.SceneNode?.AbsRotation));              
            }
        }

        var spawnst = Utilities.FindAllEntitiesByDesignerName<CBaseEntity>("info_player_terrorist");
        foreach (var spawn in spawnst)
        {
            if(spawn.IsValid)
            {
                SpawnsT.Add(new Position(spawn.CBodyComponent?.SceneNode?.AbsOrigin, spawn.CBodyComponent?.SceneNode?.AbsRotation));
            }
        }

    }

    #region commands
    [ConsoleCommand("css_menu","open menu")]
    public void OnModes(CCSPlayerController? player, CommandInfo command)
    {
        if (player == null) return;
        if (!player.PlayerPawn.IsValid) return;

        var modeMenu = new ChatMenu("Mode Menu");
        var handleGive = (CCSPlayerController player, ChatMenuOption option) => ModeMenuOption(player, option.Text);

        modeMenu.AddMenuOption("Pracc", handleGive);
        modeMenu.AddMenuOption("Match", handleGive);
        modeMenu.AddMenuOption("Help", handleGive);
        ChatMenus.OpenMenu(player, modeMenu); 
    }

    [ConsoleCommand("css_spawn", "Teleport to spawn")]
    public void OnSpawn(CCSPlayerController? player, CommandInfo command)
    {
        if(match.CurrentMode != enums.PluginMode.Pracc) return;
        if(SpawnsT.Count == 0) GetSpawns();
        if (player == null) return;
        if (!player.PlayerPawn.IsValid) return;
        int number = -1;

        try
        {
            number = Convert.ToInt32(command.ArgString);
            number -= 1;
        }
        catch (Exception ex)
        {
            return;
        }
      
        if(player.TeamNum == (byte)CsTeam.CounterTerrorist)
        {
            if(SpawnsCT.Count <= number)
            {
                return;
            }
            player.PlayerPawn.Value.Teleport(SpawnsCT[number].PlayerPosition, SpawnsCT[number].PlayerAngle, new Vector(0, 0, 0));
        }
        if (player.TeamNum == (byte)CsTeam.Terrorist)
        {
            if (SpawnsT.Count <= number)
            {
                Server.ExecuteCommand($"say insufficient number of spawns found. spawns {SpawnsT.Count} - {number}");
                return;
            }
            player.PlayerPawn.Value.Teleport(SpawnsT[number].PlayerPosition, SpawnsT[number].PlayerAngle, new Vector(0, 0, 0));
        }
    }

    [ConsoleCommand("css_pause", "Pause Game")]
    public void OnPause(CCSPlayerController? player, CommandInfo command)
    {
        if (match.CurrentMode != enums.PluginMode.Match) return;
        if (player == null) return;
        if (!player.PlayerPawn.IsValid) return;
        match.Pause();
    }

    [ConsoleCommand("css_unpause", "Unpause Game")]
    public void OnUnPause(CCSPlayerController? player, CommandInfo command)
    {
        if (match.CurrentMode != enums.PluginMode.Match) return;
        if (player == null) return;
        if (!player.PlayerPawn.IsValid) return;
        match.Unpause();
    }

    [ConsoleCommand("css_forceready", "forceready")]
    public void OnForceReady(CCSPlayerController? player, CommandInfo command)
    {
        if (match.CurrentMode != enums.PluginMode.Match) return;
        if (player == null) return;
        if (!player.PlayerPawn.IsValid) return;
        match.Start();
    }

    [ConsoleCommand("css_warmup", "start warmup")]
    public void OnWarmup(CCSPlayerController? player, CommandInfo command)
    {
        if (match.CurrentMode != enums.PluginMode.Match) return;
        if (player == null) return;
        if (!player.PlayerPawn.IsValid) return;
        match.Rewarmup();
    }

    [ConsoleCommand("css_Nades", "Save Position")]
    public void OnNades(CCSPlayerController? player, CommandInfo command)
    {
        if (match.CurrentMode != enums.PluginMode.Pracc) return;
        if (player == null) return;
        if (!player.PlayerPawn.IsValid) return;

        var modeMenu = new ChatMenu("Nade Menu");
        var handleGive = (CCSPlayerController player, ChatMenuOption option) => TeleportPlayer(player, option.Text);

        foreach (var nade in Nades)
        {
            if (nade.Map == Server.MapName)
            {
                modeMenu.AddMenuOption(nade.Title, handleGive);
            }
        }

        ChatMenus.OpenMenu(player, modeMenu);
    }

    [ConsoleCommand("css_save", "Save Nade")]
    public void OnSave(CCSPlayerController? player, CommandInfo command)
    {
        if (match.CurrentMode != enums.PluginMode.Pracc) return;
        if (player == null) return;
        if (!player.PlayerPawn.IsValid) return;

        var absOrigin = player.PlayerPawn.Value.CBodyComponent!.SceneNode!.AbsOrigin;
        string name = command.ArgString;
        Nades.Add(new SavedNade(absOrigin, player.PlayerPawn.Value.CBodyComponent!.SceneNode!.AbsRotation, null, name, "", Server.MapName));
        Server.ExecuteCommand($"say rotation {player.PlayerPawn.Value.CBodyComponent!.SceneNode!.Rotation.X}, {player.PlayerPawn.Value.CBodyComponent!.SceneNode!.Rotation.Y} , {player.PlayerPawn.Value.CBodyComponent!.SceneNode!.Rotation.Z}");
    }

    [ConsoleCommand("css_help", "print help")]
    public void OnHelp(CCSPlayerController? player, CommandInfo command)
    {
        if (player == null) return;
        if (!player.PlayerPawn.IsValid) return;
        PrintHelp(player);     
    }

    [ConsoleCommand("css_fakercon", "print help")]
    public void OnFakeRcon(CCSPlayerController? player, CommandInfo command)
    {
        if (match.CurrentMode != enums.PluginMode.Pracc) return;
        if (player == null) return;
        if (!player.PlayerPawn.IsValid) return;
        Server.ExecuteCommand(command.ArgString);
    }

    #endregion

    private void ModeMenuOption(CCSPlayerController player,string optionText)
    {
        switch(optionText)
        {
            case "Pracc":
                this.match.SwitchTo(enums.PluginMode.Pracc);
                break;
            case "Match":
                this.match.SwitchTo(enums.PluginMode.Match);
                break;
            case "Help":
                PrintHelp(player);
                break;

        }

    }
  
    private void TeleportPlayer(CCSPlayerController player,string grenadeName)
    {
        foreach (var nade in Nades)
        {
            if (nade.Map == Server.MapName)
            {
               if(nade.Title == grenadeName)
                {                  
                    player.PlayerPawn.Value.Teleport(nade.PlayerPosition, player.PlayerPawn.Value.CBodyComponent.SceneNode.AbsRotation,nade.Velocity);
                    player.PlayerPawn.Value.CBodyComponent.SceneNode.AbsRotation.X = nade.PlayerAngle.X;
                    player.PlayerPawn.Value.CBodyComponent.SceneNode.AbsRotation.Y = nade.PlayerAngle.Y;
                    player.PlayerPawn.Value.CBodyComponent.SceneNode.AbsRotation.Z = nade.PlayerAngle.Z;
                }
            }
        }
    }

    [GameEventHandler]
    public HookResult OnPlayerConnect(EventPlayerConnect @event, GameEventInfo info)
    {     
        Players?.Add(new CSPracc.DataModules.Player((int)@event.Userid.EntityIndex.Value.Value));
        return HookResult.Continue;
    }

    [GameEventHandler]
    public HookResult OnPlayerDisconnect(EventPlayerDisconnect @event, GameEventInfo info)
    {
        foreach(var player in Players)
        {
            if(player.clientindex == @event.Playerid)
            {
                Players.Remove(player);
            }
        }
        return HookResult.Continue;
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
        foreach ( string s in message )
        {
            player?.PrintToChat(s);
        }
    }
}
