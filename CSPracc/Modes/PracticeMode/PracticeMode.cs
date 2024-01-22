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
        BotReplayManager BotReplayManager { get; set; }
        public HtmlMenu GetPracticeSettingsMenu(CCSPlayerController ccsplayerController)
        {
            HtmlMenu practiceMenu;
            List<KeyValuePair<string,Action>> menuOptions = new List<KeyValuePair<string,Action>>();
            if(!ccsplayerController.GetValueOfCookie("PersonalizedNadeMenu",out string? setting))
            {
                setting = "yes";
                ccsplayerController.SetOrAddValueOfCookie("PersonalizedNadeMenu", "yes");
            }
            if(setting == null)
            {
                setting = "yes";
                ccsplayerController.SetOrAddValueOfCookie("PersonalizedNadeMenu", "yes");
            }
            string MenuText = "";
            if (setting == "yes")
            {
                MenuText = "Show Global Nade Menu";
            }
            else
            {
                MenuText = "Show Personalized Nade Menu";
            }

            KeyValuePair<string, Action> personalizedNadeMenu = new KeyValuePair<string, Action>(MenuText, () =>
            {
                SwitchPersonalizedNadeMenuOption(ccsplayerController);
            });
            menuOptions.Add(personalizedNadeMenu);
            return practiceMenu = new HtmlMenu("Practice Settings",menuOptions);
        }
        private void SwitchPersonalizedNadeMenuOption(CCSPlayerController player)
        {
            if (!player.GetValueOfCookie("PersonalizedNadeMenu", out string? setting))
            {
                setting = "yes";
                player.SetOrAddValueOfCookie("PersonalizedNadeMenu", "yes");
            }
            switch(setting)
            {
                case "yes":
                    player.SetOrAddValueOfCookie("PersonalizedNadeMenu", "no");
                    break;
                case "no":
                    player.SetOrAddValueOfCookie("PersonalizedNadeMenu", "yes");
                    break;
                default:
                    player.SetOrAddValueOfCookie("PersonalizedNadeMenu", "yes");
                    break;
            }
        }

        ProjectileManager projectileManager;
        PracticeBotManager PracticeBotManager;
        public PracticeMode() : base() 
        {
            projectileManager = new ProjectileManager();
            PracticeBotManager = new PracticeBotManager();
            BotReplayManager = new BotReplayManager(ref PracticeBotManager, ref projectileManager);  
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

        public void ShowPlayerBasedNadeMenu(CCSPlayerController player,string tag = "",string name="")
        {
            if (player == null) return;
            if(!player.IsValid) return;

            GuiManager.AddMenu(player.SteamID, projectileManager.GetPlayerBasedNadeMenu(player,tag,name));            
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

        public void ShowPracticeMenu(CCSPlayerController player)
        {
            if (player == null) return;
            if (!player.IsValid) return;
            GuiManager.AddMenu(player.SteamID,GetPracticeSettingsMenu(player));
        }

        public void Record(CCSPlayerController playerController)
        {
            BotReplayManager.RecordPlayer(playerController);
        }

        public void StopRecord(CCSPlayerController playerController)
        {
            BotReplayManager.StopRecording(playerController);
        }

        public void ReplayLastRecord(CCSPlayerController playerController)
        {
            BotReplayManager.ReplayLastReplay(playerController);
        }
        public override void Dispose()
        {
            projectileManager.Dispose();
            base.Dispose();
        }
    }
}
