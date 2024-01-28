using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using CSPracc.CommandHandler;
using CSPracc.DataModules;
using CSPracc.DataModules.Constants;
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
  
        /// <summary>
        /// Get settings
        /// </summary>
        /// <param name="ccsplayerController">player who issued the command</param>
        /// <returns>return setting menu</returns>
        public HtmlMenu GetPracticeSettingsMenu(CCSPlayerController ccsplayerController)
        {
            HtmlMenu practiceMenu;
            List<KeyValuePair<string,Action>> menuOptions = new List<KeyValuePair<string,Action>>();
            if(!ccsplayerController.GetValueOfCookie("PersonalizedNadeMenu",out string? setting))
            {
                if(CSPraccPlugin.Instance.Config.UsePersonalNadeMenu)
                {
                    setting = "yes";
                    ccsplayerController.SetOrAddValueOfCookie("PersonalizedNadeMenu", "yes");
                }
                else
                {
                    setting = "no";
                    ccsplayerController.SetOrAddValueOfCookie("PersonalizedNadeMenu", "no");
                }                
            }
            if(setting == null)
            {
                setting = CSPraccPlugin.Instance.Config.UsePersonalNadeMenu ? "yes": "no";
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

        /// <summary>
        /// Return Mimic menu
        /// </summary>
        /// <param name="ccsplayerController">palyer who issued the command</param>
        /// <returns></returns>
        private HtmlMenu getBotMimicMenu(CCSPlayerController ccsplayerController)
        {
            HtmlMenu mimic_menu;
            List<KeyValuePair<string, Action>> menuOptions = new List<KeyValuePair<string, Action>>();
            menuOptions.Add( new KeyValuePair<string, Action>("List existing replay", () => CSPraccPlugin.Instance!.AddTimer(0.5f, () => ShowMimcReplays(ccsplayerController))));
            menuOptions.Add(new KeyValuePair<string, Action>("Create new replay", new Action(() => CreateReplay(ccsplayerController))));
            menuOptions.Add(new KeyValuePair<string, Action>("Delete existing replay", () => CSPraccPlugin.Instance!.AddTimer(0.5f, () => DeleteMimicReplay(ccsplayerController))));
            return mimic_menu = new HtmlMenu("Bot Mimic Menu", menuOptions);
        }

        /// <summary>
        /// Show mimic menu to the player
        /// </summary>
        /// <param name="player">player who issued the command</param>
        public void ShowMimicMenu(CCSPlayerController player)
        {
            HtmlMenu mimicMenu = getBotMimicMenu(player);
            GuiManager.AddMenu(player.SteamID, mimicMenu);
        }

        /// <summary>
        /// Rename last created replay set
        /// </summary>
        /// <param name="player">player who issued the command</param>
        /// <param name="newReplaySetName">new name</param>
        public void RenameCurrentReplaySet(CCSPlayerController player,string newReplaySetName)
        {
            if (newReplaySetName == "")
            {
                player.ChatMessage("Please pass a new replay set name");
                return;
            }
            BotReplayManager.RenameCurrentReplaySet(player,newReplaySetName);
        }

        /// <summary>
        /// Store the last recorded replay
        /// </summary>
        /// <param name="player">player who issued the command</param>
        public void StoreLastReplay(CCSPlayerController player)
        {
            BotReplayManager.SaveLastReplay(player);
        }

        /// <summary>
        /// List all replays and play on selection
        /// </summary>
        /// <param name="player">player who issued the command</param>
        public void ShowMimcReplays(CCSPlayerController player)
        {
            HtmlMenu replay_menu;
            List<KeyValuePair<string, Action>> menuOptions = new List<KeyValuePair<string, Action>>();
            List<KeyValuePair<int, ReplaySet>> replays = BotReplayManager.GetAllCurrentReplays();
            if(replays.Count == 0)
            {
                player.ChatMessage($"There are currently no replays existing. Create one using {PRACC_COMMAND.create_replay} 'name of the replay'");
                return;
            }
            for (int i = 0;i<replays.Count;i++)
            {
                ReplaySet set = replays[i].Value;
                menuOptions.Add(new KeyValuePair<string, Action>($"{replays[i].Value.SetName}", () => BotReplayManager.PlayReplaySet(set)));
            }
            replay_menu = new HtmlMenu("Replays", menuOptions);
            GuiManager.AddMenu(player.SteamID, replay_menu);
            return;
        }

        /// <summary>
        /// Show menu to delete replay
        /// </summary>
        /// <param name="ccsplayerController">player who issued the commands</param>
        public void DeleteMimicReplay(CCSPlayerController player)
        {
            if(!player.IsAdmin())
            {
                player.ChatMessage("Only admins can delete replays!");
                return;
            }
            HtmlMenu deletion_menu;
            List<KeyValuePair<string, Action>> menuOptions = new List<KeyValuePair<string, Action>>();
            List<KeyValuePair<int, ReplaySet>> replays = BotReplayManager.GetAllCurrentReplays();
            if(replays.Count == 0)
            {
                player.ChatMessage($"There are currently no replays existing. Create one using {PRACC_COMMAND.create_replay} 'name of the replay'");
                return;
            }
            for (int i = 0; i < replays.Count; i++)
            {
                Server.PrintToConsole($"Logging {replays[i].Value.SetName}");
                int id = replays[i].Key;
                menuOptions.Add(new KeyValuePair<string, Action>($"{replays[i].Value.SetName}", () => BotReplayManager.DeleteReplaySet(player, id)));
            }
            deletion_menu = new HtmlMenu("Delete Replay", menuOptions);
            GuiManager.AddMenu(player.SteamID, deletion_menu);
            return;
        }

        /// <summary>
        /// Create new replay set
        /// </summary>
        /// <param name="player"></param>
        /// <param name="name"></param>
        public void CreateReplay(CCSPlayerController player,string name = "new replayset")
        {
            if (name == "") name = "new replayset";
            player.ChatMessage($"You are now in editing mode. For replay '{name}'");
            player.ChatMessage($"Use {ChatColors.Green}{PRACC_COMMAND.record_role}{ChatColors.White} to record a new role.");
            player.ChatMessage($"Use {ChatColors.Green}{PRACC_COMMAND.stoprecord}{ChatColors.White} to stop the recording.");
            player.ChatMessage($"Use {ChatColors.Green}{PRACC_COMMAND.store_replay}{ChatColors.White} 'name' to save the record with the given name.");
            player.ChatMessage($"Use {ChatColors.Green}{PRACC_COMMAND.rename_replayset}{ChatColors.White} to set a new name.");
            BotReplayManager.CreateReplaySet(player,name);
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
        SpawnManager SpawnManager;
        public PracticeMode() : base() 
        {
            projectileManager = new ProjectileManager();
            PracticeBotManager = new PracticeBotManager();
            SpawnManager = new SpawnManager();
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
            EventHandler = new PracticeEventHandler(CSPraccPlugin.Instance!, new PracticeCommandHandler(this, ref projectileManager,ref PracticeBotManager, ref SpawnManager),ref projectileManager, ref PracticeBotManager);
        }

        public void ShowPracticeMenu(CCSPlayerController player)
        {
            if (player == null) return;
            if (!player.IsValid) return;
            GuiManager.AddMenu(player.SteamID,GetPracticeSettingsMenu(player));
        }

        public void Record(CCSPlayerController playerController,string name = "")
        {
            BotReplayManager.RecordPlayer(playerController, name);
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
            Server.ExecuteCommand("exec CSPRACC\\undo_pracc.cfg");
            projectileManager.Dispose();
            base.Dispose();
        }
    }
}
