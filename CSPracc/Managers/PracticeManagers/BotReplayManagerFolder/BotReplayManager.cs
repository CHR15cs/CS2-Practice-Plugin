using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Entities;
using CounterStrikeSharp.API.Modules.Utils;
using CSPracc.DataModules;
using CSPracc.DataModules.Constants;
using CSPracc.DataStorages.JsonStorages;
using CSPracc.Extensions;
using CSPracc.Managers.BaseManagers.CommandManagerFolder;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using static CounterStrikeSharp.API.Core.BasePlugin;

namespace CSPracc.Managers
{
    public class BotReplayManager : IManager
    {
        ProjectileManager ProjectileManager { get; set; }
        PracticeBotManager PracticeBotManager { get; set; }
        BotReplayStorage BotReplayStorage { get; set; }
        Dictionary<ulong,int> ReplayToEdit {  get; set; } = new Dictionary<ulong,int>();
        Dictionary<ulong,PlayerReplay> replays = new Dictionary<ulong, PlayerReplay>();
        Dictionary<ulong,PlayerReplay> replaysToRecord = new Dictionary<ulong,PlayerReplay>();
        Dictionary<int, PlayerReplay> replaysToReplay = new Dictionary<int, PlayerReplay>();
        public BotReplayManager(ref PracticeBotManager practiceBotManager,ref ProjectileManager projectileManager, ref CommandManager commandManager, ref GuiManager guiManager) 
        {
            ProjectileManager = projectileManager;
            PracticeBotManager = practiceBotManager;
            CSPraccPlugin.Instance.Logger.LogInformation("Creating Bot Replay storage");
            BotReplayStorage = new BotReplayStorage(new FileInfo(Path.Combine(CSPraccPlugin.ModuleDir.FullName, "BotReplays", "BotReplay_" + Server.MapName + ".json")));
            CSPraccPlugin.Instance.Logger.LogInformation($"Created Bot Replay storage Count: {BotReplayStorage.GetAll().Count}");
            CSPraccPlugin.Instance!.RegisterListener<Listeners.OnTick>(OnTick);
            CSPraccPlugin.Instance.RegisterListener<Listeners.OnEntitySpawned>(entity => OnEntitySpawned(entity));
            CSPraccPlugin.Instance.RegisterEventHandler<EventPlayerShoot>(OnPlayerShoot);
        }

        /// <summary>
        /// Return Mimic menu
        /// </summary>
        /// <param name="ccsplayerController">palyer who issued the command</param>
        /// <returns></returns>
        private HtmlMenu GetBotMimicMenu(CCSPlayerController ccsplayerController)
        {
            HtmlMenu mimic_menu;
            List<KeyValuePair<string, Action>> menuOptions = new List<KeyValuePair<string, Action>>();
            menuOptions.Add(new KeyValuePair<string, Action>("List existing replay", () => CSPraccPlugin.Instance!.AddTimer(0.5f, () => ShowMimcReplays(ccsplayerController, new List<string>()))));
            menuOptions.Add(new KeyValuePair<string, Action>("Create new replay", new Action(() => CreateReplaySet(ccsplayerController, ""))));
            menuOptions.Add(new KeyValuePair<string, Action>("Delete existing replay", () => CSPraccPlugin.Instance!.AddTimer(0.5f, () => DeleteMimicReplay(ccsplayerController))));
            return mimic_menu = new HtmlMenu("Bot Mimic Menu", menuOptions);
        }

        /// <summary>
        /// List all replays and play on selection
        /// </summary>
        /// <param name="player">player who issued the command</param>
        public bool ShowMimcReplays(CCSPlayerController player,PlayerCommandArgument args)
        {
            HtmlMenu replay_menu;
            List<KeyValuePair<string, Action>> menuOptions = new List<KeyValuePair<string, Action>>();
            List<KeyValuePair<int, ReplaySet>> replays = GetAllCurrentReplays();
            if (replays.Count == 0)
            {
                player.ChatMessage($"There are currently no replays existing. Create one using {BotReplayCommands.create_replay} 'name of the replay'");
                return false;
            }
            for (int i = 0; i < replays.Count; i++)
            {
                ReplaySet set = replays[i].Value;
                menuOptions.Add(new KeyValuePair<string, Action>($"{replays[i].Value.SetName}", () => PlayReplaySet(set)));
            }
            replay_menu = new HtmlMenu("Replays", menuOptions);
            GuiManager.Instance.AddMenu(player.SteamID, replay_menu);
            return true;
        }


        /// <summary>
        /// Show mimic menu to the player
        /// </summary>
        /// <param name="player">player who issued the command</param>
        public bool ShowMimicMenu(CCSPlayerController player,PlayerCommandArgument args)
        {
            HtmlMenu mimicMenu = GetBotMimicMenu(player);
            GuiManager.Instance.AddMenu(player.SteamID, mimicMenu);
            return true;
        }

        public bool ShowDeleteMenuCommandHandler(CCSPlayerController playerController, PlayerCommandArgument args)
        {
            DeleteMimicReplay(playerController);
            return true;
        }

        /// <summary>
        /// Show menu to delete replay
        /// </summary>
        /// <param name="ccsplayerController">player who issued the commands</param>
        public void DeleteMimicReplay(CCSPlayerController player)
        {
            if (!player.IsAdmin())
            {
                player.ChatMessage("Only admins can delete replays!");
                return;
            }
            HtmlMenu deletion_menu;
            List<KeyValuePair<string, Action>> menuOptions = new List<KeyValuePair<string, Action>>();
            List<KeyValuePair<int, ReplaySet>> replays = GetAllCurrentReplays();
            if (replays.Count == 0)
            {
                player.ChatMessage($"There are currently no replays existing. Create one using {BotReplayCommands.create_replay} 'name of the replay'");
                return;
            }
            for (int i = 0; i < replays.Count; i++)
            {
                Server.PrintToConsole($"Logging {replays[i].Value.SetName}");
                int id = replays[i].Key;
                menuOptions.Add(new KeyValuePair<string, Action>($"{replays[i].Value.SetName}", () => DeleteReplaySet(player, id)));
            }
            deletion_menu = new HtmlMenu("Delete Replay", menuOptions);
            GuiManager.Instance.AddMenu(player.SteamID, deletion_menu);
            return;
        }


        /// <summary>
        /// Create new replay set
        /// </summary>
        /// <param name="player"></param>
        /// <param name="name"></param>
        public bool CreateReplayCommandHandler(CCSPlayerController player, PlayerCommandArgument args)
        {
            string name = "new replayset";
            if (args.ArgumentCount > 0)
            {
                name = args.ArgumentString;
            }           
            CreateReplaySet(player, name);
            return true;
        }


        public void CreateReplaySet(CCSPlayerController player, string name) 
        {
            player.ChatMessage($"You are now in editing mode. For replay '{name}'");
            player.ChatMessage($"Use {ChatColors.Green}{BotReplayCommands.record_role}{ChatColors.White} to record a new role.");
            player.ChatMessage($"Use {ChatColors.Green}{BotReplayCommands.stoprecord}{ChatColors.White} to stop the recording.");
            player.ChatMessage($"Use {ChatColors.Green}{BotReplayCommands.store_replay}{ChatColors.White} 'name' to save the record with the given name.");
            player.ChatMessage($"Use {ChatColors.Green}{BotReplayCommands.rename_replayset}{ChatColors.White} to set a new name.");
            if (String.IsNullOrWhiteSpace(name))
            {
                name = "new replayset";
            }
            int id = BotReplayStorage.Add(new ReplaySet(new List<PlayerReplay>(), name));
            ReplayToEdit.SetOrAdd(player.SteamID, id);
        }


        /// <summary>
        /// Record player and add to replay
        /// </summary>
        /// <param name="player">who issued the command</param>
        /// <param name="name">name of the replay, if not set, playername is used</param>
        public bool RecordPlayerCommandHandler(CCSPlayerController playerController, PlayerCommandArgument args)
        {
            if (replaysToRecord.ContainsKey(playerController.SteamID)) return false;
            string name = playerController.PlayerName;
            if(args.ArgumentCount >= 0)
            {
                name = args.ArgumentString;
            }           
            replaysToRecord.Add(playerController.SteamID, new PlayerReplay(name));
            playerController.ChatMessage($"Recording role {name}");
            return true;
        }
        /// <summary>
        /// Stop recording current replay
        /// </summary>
        /// <param name="player">player who issued the command</param>
        /// <returns>Return the replay</returns>
        public bool StopRecordingCommandHandler(CCSPlayerController player, PlayerCommandArgument args)
        {
            if(replaysToRecord.ContainsKey(player.SteamID))
            {
                PlayerReplay replay =  replaysToRecord[player.SteamID];
                replaysToRecord.Remove(player.SteamID);
                replays.SetOrAdd(player.SteamID, replay);
                player.ChatMessage($"Stopped recording for role {replay.ReplayName}");
                return true;

            }
            else
            {
                player.ChatMessage("You are currently not recording.");
            }
            return false;
        }
        /// <summary>
        /// Rename last recorded replay
        /// </summary>
        /// <param name="player">who issued the command</param>
        /// <param name="name">name of the replay</param>
        public bool NameReplayCommandHandler(CCSPlayerController playerController, PlayerCommandArgument args)
        {
            if(args.ArgumentCount == 0)
            {
                playerController.ChatMessage("Cannot set empty name");
                return false;
            }
            string name = args.ArgumentString;
            if (replays.TryGetValue(playerController.SteamID, out PlayerReplay? replay))
            {
                replay.ReplayName = name;
                return true;
            }
            else
            {
                playerController.ChatMessage("Could not get last replay recorded.");               
            }
            return false;
        }

        /// <summary>
        /// Save last recorded replay and add it to the current replay set
        /// </summary>
        /// <param name="player"></param>
        public bool SaveLastReplayCommandHandler(CCSPlayerController playerController, PlayerCommandArgument args)
        {
            if(!replays.TryGetValue(playerController.SteamID, out PlayerReplay? replay))
            {
                playerController.ChatMessage("No Replay recorded.");
                return false;
            }
            if (replay == null)
            {
                playerController.ChatMessage("Error getting replays from storage.");
                return false;
            }

            if (!getPlayerEditReplay(playerController, out int replayid))
            {
                playerController.ChatMessage("You need to select a replay to edit.");
                return false;
            }

            if (BotReplayStorage.Get(replayid, out ReplaySet? replayList))
            {
                if (replayList == null)
                {
                    replayList = new ReplaySet(new List<PlayerReplay>(), "");
                }
                replayList.Replays.Add(replay);
                BotReplayStorage.SetOrAdd(replayid, replayList);
                replays.Remove(playerController.SteamID);
                playerController.ChatMessage("Added last recorded replay to storage");
                return true;
            }
            else
            {
                playerController.ChatMessage("Error getting replays from storage.");
                
            }
            return false;
        }

        public bool DeleteReplaySetCommandHandler(CCSPlayerController playerController, PlayerCommandArgument args)
        {
            if(args.ArgumentCount == 0)
            {
                playerController.ChatMessage("Cannot delete replayset without id");
                return false;
            }
            int id = -1;
            if(!int.TryParse(args.ArgumentString, out id))
            {
                playerController.ChatMessage("Id needs to be a number");
                return false;
            }
            DeleteReplaySet(playerController, id);
            return true;
        }
        public void DeleteReplaySet(CCSPlayerController player, int id) 
        {
            if(BotReplayStorage.ContainsKey(id))
            {
                BotReplayStorage.RemoveKey(id);
                player.ChatMessage($"Deleted replay with id {id}");
                return;
            }
            player.ChatMessage($"Could not find replay with id {id}");
        }


        /// <summary>
        /// Rename current replay set
        /// </summary>
        /// <param name="player">player who issued the command</param>
        /// <param name="name">new name</param>
        public bool RenameCurrentReplaySetCommandHandler(CCSPlayerController player, PlayerCommandArgument args)
        {
            if(args.ArgumentCount == 0)
            {
                player.ChatMessage("Cannot set empty name");
                return false;
            }
            string name = args.ArgumentString;
            if(!getPlayerEditReplay(player, out int ReplayId))
            {
                player.ChatMessage("Could not get current replay set.");
                return false;
            }
            if (!BotReplayStorage.ContainsKey(ReplayId))
            {
                player.ChatMessage($"Could not get current replay with id {ReplayId}.");
                return false;
            }
            if (!BotReplayStorage.Get(ReplayId, out ReplaySet set))
            {
                player.ChatMessage("Could not get current replay set.");
                return false;
            }
            if (set == null)
            {
                player.ChatMessage("Could not get current replay set.");
                return false;
            }
            set.SetName = name;
            BotReplayStorage.SetOrAdd(ReplayId, set);
            player.ChatMessage($"Updated setname to {ChatColors.Green}{name}{ChatColors.White}.");
            return true;
        }

        private bool getPlayerEditReplay(CCSPlayerController player, out int replayid)
        {
            return ReplayToEdit.TryGetValue(player.SteamID, out replayid);
        }

        public void PlayReplaySet(int replayid)
        {
            if(!BotReplayStorage.Get(replayid,out ReplaySet? replayList))
            {
                Utils.ServerMessage($"Could not find replay set with id {replayid}");
                return;
            }
            if (replayList == null)
            {
                Utils.ServerMessage($"Could not load replay set with id {replayid}");
                return;
            }
            for (int i = 0; i < replayList.Replays.Count; i++)
            {
                Server.ExecuteCommand("bot_add_t");
            }
            foreach (PlayerReplay replay in replayList.Replays)
            {
                Utils.ServerMessage($"Playing replay: {ChatColors.Green}{replay.ReplayName}");
                ReplayReplay(replay);
            }
        }
        public void PlayReplaySet(ReplaySet replaysSet)
        {
            for (int i = 0; i < replaysSet.Replays.Count; i++)
            {
                Server.ExecuteCommand("bot_add_t");
            }
            CSPraccPlugin.Instance.AddTimer(1.0f,()=> Replayset(replaysSet));
        }

        private void Replayset(ReplaySet replayset)
        {
            foreach (PlayerReplay replay in replayset.Replays)
            {
                Utils.ServerMessage($"Playing replay: {ChatColors.Green}{replay.ReplayName}");
                ReplayReplay(replay);
            }
        }

        public void ReplayLastReplay(CCSPlayerController player)
        {
            if (replays.ContainsKey(player.SteamID))
            {
                ReplayReplay(replays[player.SteamID]);
            }
        }

        public void ReplayReplay(PlayerReplay replay)
        {
            //PracticeBotManager.AddBot(player);
            List<CCSPlayerController?>? playerList = Utilities.GetPlayers()!.ToList();
            if (playerList == null) return;
            CCSPlayerController? bot = null;
            foreach(CCSPlayerController? playerController in playerList)
            {
                if (playerController == null || !playerController.IsValid || !playerController.IsBot || playerController.IsHLTV)
                {
                    continue;
                }
                if(replaysToReplay.ContainsKey(playerController.Slot))
                {
                    continue;
                }
                bot = playerController;
                break;
            }
            if (bot == null || !bot.IsValid)
            {
                Utils.ServerMessage("bot is invalid");
                return;
            }
            replay.frameCount = 0;
            replaysToReplay.SetOrAdd(bot.Slot, replay);  
        }

        /// <summary>
        /// Replaying the replay
        /// </summary>
        public void OnTick()
        {
            foreach(ulong steamid in replaysToRecord.Keys)
            {
                CCSPlayerController? player = Utilities.GetPlayerFromSteamId(steamid);
                if (player == null || !player.IsValid)
                {
                    replaysToRecord.Remove(steamid);
                    continue;
                }
                replaysToRecord[steamid].AddFrame(new PlayerFrame(player));              
            }

            foreach (int slot in replaysToReplay.Keys)
            {
                CCSPlayerController? player = Utilities.GetPlayerFromSlot(slot);
                if (player == null || !player.IsValid)
                {
                    replaysToReplay.Remove(player.Slot);
                    Utils.ServerMessage($"Player invalid");
                    continue;
                }
                PlayerFrame? frame = replaysToReplay[slot].GetNextFrame();
                if(frame == null)
                {
                    Utils.ServerMessage($"Replaying role {replaysToReplay[slot].ReplayName} finished.");
                    replaysToReplay.Remove(slot); 
                    continue;
                }
                player.TeleportToJsonSpawnPoint(frame.Position);
                if (frame.ProjectileSnapshot != null)
                {
                    ProjectileManager.ThrowGrenadePojectile(frame.ProjectileSnapshot, player);
                }
            }
        }

        /// <summary>
        /// Get all replays currently available
        /// </summary>
        /// <returns>List of all replay sets available</returns>
        public List<KeyValuePair<int,ReplaySet>> GetAllCurrentReplays()
        {
            return BotReplayStorage.GetAll();
        }

        /// <summary>
        /// This is really ugly.
        /// Basicly the same as the porjectile manager does
        /// Probably should fire an event in the projectile manager and catch it here.
        /// </summary>
        /// <param name="entity"></param>
        public void OnEntitySpawned(CEntityInstance entity)
        {
            if (entity == null) return;
            if (!entity.IsProjectile())
            {
                return;
            }
            CBaseCSGrenadeProjectile projectile;

            switch (entity.Entity!.DesignerName)
            {
                case (DesignerNames.ProjectileSmoke):
                    {
                        projectile = new CSmokeGrenadeProjectile(entity.Handle);
                        break;
                    }
                default:
                    {
                        projectile = new CBaseCSGrenadeProjectile(entity.Handle);
                        break;
                    }
            };

            Server.NextFrame(() =>
            {
                CCSPlayerController player = new CCSPlayerController(projectile.Thrower.Value.Controller.Value.Handle);
                CounterStrikeSharp.API.Modules.Utils.Vector playerPosition = player.PlayerPawn.Value.CBodyComponent!.SceneNode!.AbsOrigin;
                //TODO provide actual projectile Position
                CounterStrikeSharp.API.Modules.Utils.Vector projectilePosition = new CounterStrikeSharp.API.Modules.Utils.Vector();
                QAngle playerAngle = player.PlayerPawn.Value.EyeAngles;
                string name = "LastThrown";
                //TODO parse actual description if provided
                string description = "";
                GrenadeType_t type = GrenadeType_t.GRENADE_TYPE_SMOKE;
                switch (projectile.DesignerName)
                {
                    case DesignerNames.ProjectileSmoke:
                        {
                            type = GrenadeType_t.GRENADE_TYPE_SMOKE;
                            break;
                        }
                    case DesignerNames.ProjectileFlashbang:
                        {
                            type = GrenadeType_t.GRENADE_TYPE_FLASH;
                            break;
                        }
                    case DesignerNames.ProjectileHE:
                        {
                            type = GrenadeType_t.GRENADE_TYPE_EXPLOSIVE;
                            break;
                        }
                    case DesignerNames.ProjectileMolotov:
                        {
                            type = GrenadeType_t.GRENADE_TYPE_FIRE;
                            break;
                        }
                    case DesignerNames.ProjectileDecoy:
                        {
                            type = GrenadeType_t.GRENADE_TYPE_DECOY;
                            break;
                        }
                    default:
                        {
                            type = GrenadeType_t.GRENADE_TYPE_SMOKE;
                            break;
                        }

                }
                ProjectileSnapshot tmpSnapshot = new ProjectileSnapshot(playerPosition.ToVector3(), projectile.InitialPosition.ToVector3(), playerAngle.ToVector3(), projectile.InitialVelocity.ToVector3(), name, description, type);
                if(replaysToRecord.ContainsKey(player.SteamID))
                {
                    replaysToRecord[player.SteamID].frames.LastOrDefault()!.ProjectileSnapshot = tmpSnapshot;
                }


            });

        }

        public HookResult OnPlayerShoot(EventPlayerShoot @event,GameEventInfo info)
        {
            if (replaysToRecord.ContainsKey(@event.Userid.SteamID))
            {
                replaysToRecord[@event.Userid.SteamID].frames.LastOrDefault()!.shoot = true;
            }
            return HookResult.Continue;
        }

        public void Dispose()
        {
            Listeners.OnTick onTick = new Listeners.OnTick(OnTick);
            CSPraccPlugin.Instance!.RemoveListener("OnTick", onTick);
            Listeners.OnEntitySpawned onEntitySpawned = new Listeners.OnEntitySpawned(OnEntitySpawned);
            CSPraccPlugin.Instance!.RemoveListener("OnEntitySpawned", onEntitySpawned);

            GameEventHandler<EventPlayerShoot> playershoot = OnPlayerShoot;
            CSPraccPlugin.Instance!.DeregisterEventHandler("player_shoot", playershoot, true);
            DeregisterCommands();
        }

        public void RegisterCommands()
        {
            CommandManager.Instance.RegisterCommand(new PlayerCommand(BotReplayCommands.mimic_menu, "open bot mimic menu", ShowMimicMenu, null, null));
            CommandManager.Instance.RegisterCommand(new PlayerCommand(BotReplayCommands.replay_menu, "open bot mimic menu", ShowMimcReplays, null, null));
            CommandManager.Instance.RegisterCommand(new PlayerCommand(BotReplayCommands.store_replay, "store last recorded replay", SaveLastReplayCommandHandler, null, null));
            CommandManager.Instance.RegisterCommand(new PlayerCommand(BotReplayCommands.create_replay, "create new replay", CreateReplayCommandHandler, null, null));
            CommandManager.Instance.RegisterCommand(new PlayerCommand(BotReplayCommands.record_role, "record role", RecordPlayerCommandHandler, null, null));
            CommandManager.Instance.RegisterCommand(new PlayerCommand(BotReplayCommands.rename_replayset, "rename replay set", RenameCurrentReplaySetCommandHandler, null, null));
            CommandManager.Instance.RegisterCommand(new PlayerCommand(BotReplayCommands.stoprecord, "stop current recording", StopRecordingCommandHandler, null, null));
        }

        public void DeregisterCommands()
        {
            CommandManager.Instance.DeregisterCommand(BotReplayCommands.mimic_menu);
            CommandManager.Instance.DeregisterCommand(BotReplayCommands.replay_menu);
            CommandManager.Instance.DeregisterCommand(BotReplayCommands.store_replay);
            CommandManager.Instance.DeregisterCommand(BotReplayCommands.create_replay);
            CommandManager.Instance.DeregisterCommand(BotReplayCommands.record_role);
            CommandManager.Instance.DeregisterCommand(BotReplayCommands.rename_replayset);
            CommandManager.Instance.DeregisterCommand(BotReplayCommands.stoprecord);
        }
    }
}
