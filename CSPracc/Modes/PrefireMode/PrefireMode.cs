using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Entities;
using CounterStrikeSharp.API.Modules.Entities.Constants;
using CounterStrikeSharp.API.Modules.Memory;
using CounterStrikeSharp.API.Modules.Utils;
using CSPracc.CommandHandler;
using CSPracc.DataModules;
using CSPracc.DataStorages.JsonStorages;
using CSPracc.EventHandler;
using CSPracc.Extensions;
using CSPracc.Managers;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CSPracc.DataModules.Enums;

namespace CSPracc.Modes
{
    public class PrefireMode : BaseMode
    {        
        PrefireRouteManager PrefireRouteManager { get; set; }

        PracticeSpawnManager SpawnManager { get; set; } 

        private int KilledBots { get; set; }

        GunManager GunManager { get; set; }

        private ulong _playerToShoot { get; set; } = 0;


        public PrefireMode(CSPraccPlugin plugin) : base(plugin)
        {
            //this.SpawnManager = new PracticeSpawnManager();
            GunManager = new GunManager(GuiManager);
            PrefireRouteManager = new PrefireRouteManager();
            KilledBots = 0;
        }

        public void LoadPrefireMode(CCSPlayerController player)
        {
            if (!player.IsAdmin()) player.PrintToCenter("Only Admins can execute this command!");

            Utils.ServerMessage("Admin loaded prefire mode.");
            Server.PrintToConsole("CSPRACC: Loading retake config");
            Server.ExecuteCommand("exec CSPRACC\\undo_pracc.cfg");
           //Server.ExecuteCommand("exec CSPRACC\\5on5.cfg");
            Server.ExecuteCommand("mp_limitteams 0");
            Server.ExecuteCommand("bot_add_ct");
            Server.ExecuteCommand("bot_add_ct");
            Server.ExecuteCommand("bot_add_ct");
            Server.ExecuteCommand("bot_add_ct");
            Server.ExecuteCommand("bot_add_ct");
            Server.ExecuteCommand("bot_add_ct");
        }

        public void SetStartingPoint(CCSPlayerController? player)
        {
            if (player == null) return;
            if (!player.IsValid) return;
            if (!player.IsAdmin()) { player.PrintToCenter("Only admins can edit routes"); }

            PrefireRouteManager.SetStartingPoint(player.GetCurrentPositionAsJsonSpawnPoint());
            Utils.ServerMessage("Set starting point. for route");
        }

        public void ShowOptions(CCSPlayerController player) 
        {
        
        }
        public void LoadRoute(CCSPlayerController player,string route)
        {
            if (player == null) return;
            if (!player.IsValid) return;
            PrefireRouteManager.LoadRouteByName(route,player);
            KilledBots = 0;
        }

        public void ShowRouteMenu(CCSPlayerController player)
        {
            GuiManager.AddMenu(player.SteamID, PrefireRouteManager.GetPrefireRouteMenu(player));
        }

        public void AddRoute(CCSPlayerController player,string route) 
        {
            if (player == null) return;
            if (!player.IsValid) return;
            if (!player.IsAdmin()) { player.PrintToCenter("Only admins can add routes"); }

            PrefireRouteManager.AddNewRoute(route);
        }

        public void EditRoute(CCSPlayerController player, string route)
        {
            if (player == null) return;
            if (!player.IsValid) return;
            if (!player.IsAdmin()) { player.PrintToCenter("Only admins can edit routes"); }
            PrefireRouteManager.EditRoute(route);
        }

        public void AddSpawn(CCSPlayerController player)
        {
            if(player == null) return;
            if (!player.IsValid) return;
            if (!player.IsAdmin()) { player.PrintToCenter("Only admins can edit routes"); }

            Utils.ServerMessage($"Spawn {player.PlayerPawn.Value!.CBodyComponent!.SceneNode!.AbsOrigin}");
            PrefireRouteManager.AddSpawn(new JsonSpawnPoint(player.PlayerPawn.Value!.CBodyComponent!.SceneNode!.AbsOrigin.ToVector3(), player.PlayerPawn.Value.EyeAngles.ToVector3(), ""));   
        }

        public void SaveCurrentRoute()
        {
            PrefireRouteManager.SaveCurrentRoute();
        }

        public void ShowGunMenu(CCSPlayerController player)
        {
            if(player == null || !player.IsValid) return;

            GunManager.ShowGunMenu(player);
        }

        public void LoadEditMode(CCSPlayerController player)
        {
            if (!player.IsAdmin()) player.PrintToCenter("Only Admins can execute this command!");

            Utils.ServerMessage("Admin loaded edit mode.");
            Server.ExecuteCommand("exec CSPRACC\\pracc.cfg");
        }

        public void AddSpawn(CCSPlayerController player, string bombsite)
        {
            if (!player.IsAdmin()) player.PrintToCenter("Only Admins can execute this command!");
            //SpawnManager.AddCurrentPositionAsSpawnPoint(player, bombsite);
        }

        public HookResult OnPlayerSpawn(EventPlayerSpawn @event,GameEventInfo info)
        {
            if (@event.Userid == null || !@event.Userid.IsValid) return HookResult.Continue;

            if(!@event.Userid.IsBot) 
            {
                GunManager.EquipPlayer(@event.Userid);
            }
            else
            {
                if(PrefireRouteManager.CurrentPrefireRoute == null) return HookResult.Continue;
                PrefireRouteManager.SpawnNextPosition(@event.Userid.Slot);
                bool akFound = false;   
                foreach(var weapon in @event.Userid.PlayerPawn.Value.WeaponServices.MyWeapons)
                {
                    
                    if(weapon.Value.DesignerName  == "weapon_ak47")
                    {
                        akFound = true;
                        continue;
                    }
                    @event.Userid.PlayerPawn.Value.RemovePlayerItem(weapon.Value);
                    //weapon.Value.Remove();
                }
                if(!akFound)
                @event.Userid.GiveNamedItem("weapon_ak47");

                @event.Userid.ExecuteClientCommand("slot 0");


            }
            return HookResult.Continue;
        }

        public HookResult OnPlayerDeath(EventPlayerDeath @event, GameEventInfo info)
        {
            if (@event.Userid == null || !@event.Userid.IsValid || !@event.Userid.IsBot) return HookResult.Continue;

            if (@event.Userid.IsBot)
            {
                //CSPraccPlugin.Instance.AddTimer(0.5f, () => @event.Userid.Respawn());
                if (PrefireRouteManager.CurrentPrefireRoute == null) return HookResult.Continue;
                KilledBots++;
                @event.Attacker.HtmlMessage($"{KilledBots}/{PrefireRouteManager.CurrentPrefireRoute!.spawnPoints.Count}");              
                if (KilledBots == PrefireRouteManager.CurrentPrefireRoute!.spawnPoints.Count)
                {
                    @event.Attacker.HtmlMessage($"Finished {PrefireRouteManager.CurrentPrefireRoute.Name}!");
                    PrefireRouteManager.LoadRouteByName(PrefireRouteManager.CurrentPrefireRoute.Name, @event.Attacker);
                    KilledBots = 0;
                }
            }
            else
            {
                @event.Userid.HtmlMessage($"You lost {PrefireRouteManager.CurrentPrefireRoute.Name}!");
                PrefireRouteManager.LoadRouteByName(PrefireRouteManager.CurrentPrefireRoute.Name, @event.Userid);
                KilledBots = 0;
            }
                return HookResult.Continue;
        }

        public HookResult OnPlayerHurt(EventPlayerHurt @event, GameEventInfo info)
        {
            if (@event.Userid == null || !@event.Userid.IsValid || !@event.Userid.IsBot) return HookResult.Continue;
           

            //if(PrefireRouteManager.CurrentPrefireRoute == null) return HookResult.Continue;
            //if(@event.Userid.IsBot)
            //{            
            //    if (@event.Health <= 0)
            //    {
            //        CCSPlayerController? bot = Utilities.GetPlayerFromSlot(@event.Userid.Slot);
            //        if(bot == null) return HookResult.Continue;
            //        KilledBots++;
            //        @event.Userid.Respawn();
                    
            //        if(KilledBots == PrefireRouteManager.CurrentPrefireRoute!.spawnPoints.Count)
            //        {
            //            @event.Attacker.HtmlMessage($"Finished {PrefireRouteManager.CurrentPrefireRoute.Name}!");
            //            PrefireRouteManager.LoadRouteByName(PrefireRouteManager.CurrentPrefireRoute.Name);
            //            KilledBots = 0;
            //        }
            //        return HookResult.Continue;
            //    }
               

            //}
            return HookResult.Continue;
        }

        public HookResult OnRoundStart(EventRoundStart @event, GameEventInfo info)
        {                  
            foreach(CCSPlayerController player in  Utilities.GetPlayers())
            {
                if(player == null) continue;
                if (!player.IsValid) continue;
                if(player.IsBot) continue;

            }

            return HookResult.Continue;
        }

        public void Restart(CCSPlayerController player)
        {
            if(PrefireRouteManager.CurrentPrefireRoute != null)
            {
                Utils.ServerMessage($"Restarting route {PrefireRouteManager.CurrentPrefireRoute.Name}");
                PrefireRouteManager.LoadRouteByName(PrefireRouteManager.CurrentPrefireRoute.Name,player);
            }
            else
            {
                Utils.ServerMessage("Cannot restart route. No route is currently loaded.");
            }
        }

        public override void ConfigureEnvironment()
        {
            DataModules.Constants.Methods.MsgToServer("Loading prefire mode.");
            Server.ExecuteCommand("exec CSPRACC\\undo_pracc.cfg");
            Server.ExecuteCommand("exec CSPRACC\\prefire.cfg");
            EventHandler?.Dispose();
            EventHandler = new PrefireEventHandler(CSPraccPlugin.Instance!, new PrefireCommandHandler(this),this);
            Utils.ServerMessage($"Use {ChatColors.Green}.routes{ChatColors.White} to show menu of routes.");
            Utils.ServerMessage($"Use {ChatColors.Green}.addroute (routename){ChatColors.White} to add a empty route.");
            Utils.ServerMessage($"Use {ChatColors.Green}.editroute (routename){ChatColors.White} to edit given route.");
            Utils.ServerMessage($"Use {ChatColors.Green}.addspawn{ChatColors.White} to add spawn to current route.");
            Utils.ServerMessage($"Use {ChatColors.Green}.startpoint{ChatColors.White} to set startpoint of current route.");
            Utils.ServerMessage($"Use {ChatColors.Green}.next{ChatColors.White} to select next route of current map.");
            Utils.ServerMessage($"Use {ChatColors.Green}.back{ChatColors.White} to go back to last route.");
        }
    }
}
