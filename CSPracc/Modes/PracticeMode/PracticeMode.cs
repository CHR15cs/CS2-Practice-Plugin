using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CSPracc.CommandHandler;
using CSPracc.DataModules;
using CSPracc.EventHandler;
using CSPracc.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CSPracc.DataModules.Enums;

namespace CSPracc.Modes
{
    public class PracticeMode : BaseMode
    {

        ProjectileManager projectileManager;
        PracticeBotManager PracticeBotManager;
        public PracticeMode() : base() 
        {
            projectileManager = new ProjectileManager();
            PracticeBotManager = new PracticeBotManager();
        }

        public void StartTimer(CCSPlayerController player)
        {
            if (player == null) return;
            base.GuiManager.StartTimer(player);
        }

        public void AddCountdown(CCSPlayerController player, int countdown)
        {
            if (player == null) return;
            base.GuiManager.StartCountdown(player,countdown);
        }

        public void ShowPlayerBasedNadeMenu(CCSPlayerController player,string tag = "")
        {
            if (player == null) return;
            if(!player.IsValid) return;

            GuiManager.AddMenu(player.SteamID, projectileManager.GetPlayerBasedNadeMenu(player,tag));            
        }


        public void ShowCompleteNadeMenu(CCSPlayerController player)
        {
            if (player == null) return;
            if (!player.IsValid) return;

            GuiManager.AddMenu(player.SteamID, projectileManager.GetNadeMenu(player));
        }

        public override void ConfigureEnvironment()
        {
            DataModules.Constants.Methods.MsgToServer("Loading practice mode.");
            Server.ExecuteCommand("exec CSPRACC\\pracc.cfg");
            EventHandler?.Dispose();
            EventHandler = new PracticeEventHandler(CSPraccPlugin.Instance!, new PracticeCommandHandler(this, ref projectileManager,ref PracticeBotManager),ref projectileManager, ref PracticeBotManager);
        }
    }
}
