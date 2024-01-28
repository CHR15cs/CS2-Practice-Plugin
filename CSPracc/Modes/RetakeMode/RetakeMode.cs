using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Entities.Constants;
using CounterStrikeSharp.API.Modules.Utils;
using CSPracc.CommandHandler;
using CSPracc.DataModules;
using CSPracc.DataStorages.JsonStorages;
using CSPracc.EventHandler;
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
    public class RetakeMode : BaseMode
    {
        enum RetakeModeStatus
        {
            edit,
            live
        }

        
        SpawnManager SpawnManager { get; set; }
        WeaponKitStorage WeaponKitStorage { get; set; }
        private string CurrentBombsite {  get; set; }
        private RetakeModeStatus status;

        GunManager GunManager { get; set; }
        public RetakeMode() : base()
        {
            this.SpawnManager = new SpawnManager();
            status = RetakeModeStatus.live;
            CurrentBombsite = "A";
            GunManager = new GunManager(GuiManager);
        }

        public void LoadRetakeMode(CCSPlayerController player)
        {
            if (!player.IsAdmin()) player.PrintToCenter("Only Admins can execute this command!");

            Utils.ServerMessage("Admin loaded retake mode.");
            status = RetakeModeStatus.live;
            Server.PrintToConsole("CSPRACC: Loading retake config");
            Server.ExecuteCommand("exec CSPRACC\\undo_pracc.cfg");
            Server.ExecuteCommand("exec CSPRACC\\retake.cfg");
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
            status = RetakeModeStatus.edit;
            Server.ExecuteCommand("exec CSPRACC\\pracc.cfg");
        }

        public void AddSpawn(CCSPlayerController player, string bombsite)
        {
            if (!player.IsAdmin()) player.PrintToCenter("Only Admins can execute this command!");
            SpawnManager.AddCurrentPositionAsSpawnPoint(player, bombsite);
        }

        public HookResult OnPlayerSpawn(EventPlayerSpawn @event,GameEventInfo info)
        {
            if (@event.Userid == null || !@event.Userid.IsValid || @event.Userid.IsBot)return HookResult.Continue;
            SpawnManager.TeleportToUnusedSpawn(@event.Userid,CurrentBombsite);
            GunManager.EquipPlayer(@event.Userid);
            return HookResult.Continue;
        }

        public HookResult OnRoundEnd(EventRoundEnd @event, GameEventInfo info)
        {
            SpawnManager.ClearUsedSpawns();
            Random rnd = new Random();
            if (rnd.Next(10) % 2 == 0)
            {
                CurrentBombsite = "A";
            }
            else
            {
                CurrentBombsite = "B";
            }
            return HookResult.Continue;
        }

        public HookResult OnRoundStart(EventRoundStart @event, GameEventInfo info)
        {                  
            foreach(CCSPlayerController player in  Utilities.GetPlayers())
            {
                if(player == null) continue;
                if (!player.IsValid) continue;
                if(player.IsBot) continue;

                if(player.GetCsTeam() == CounterStrikeSharp.API.Modules.Utils.CsTeam.Terrorist)
                {
                    player.PrintToCenter($"Defend Bombsite {CurrentBombsite}!");
                }
                if (player.GetCsTeam() == CounterStrikeSharp.API.Modules.Utils.CsTeam.CounterTerrorist)
                {
                    player.PrintToCenter($"Retake Bombsite {CurrentBombsite}!");
                }
            }

            return HookResult.Continue;
        }

        public override void ConfigureEnvironment()
        {
            DataModules.Constants.Methods.MsgToServer("Loading retakes mode.");
            Server.ExecuteCommand("exec CSPRACC\\retake.cfg");
            EventHandler?.Dispose();
            EventHandler = new RetakeEventHandler(CSPraccPlugin.Instance!, new RetakeCommandHandler(this),this);
        }
    }
}
