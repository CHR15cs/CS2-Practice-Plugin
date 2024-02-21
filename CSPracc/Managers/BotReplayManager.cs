using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Entities;
using CounterStrikeSharp.API.Modules.Utils;
using CSPracc.DataModules;
using CSPracc.DataModules.Constants;
using CSPracc.DataStorages.JsonStorages;
using CSPracc.Extensions;
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
    public class BotReplayManager : IDisposable
    {
        ProjectileManager ProjectileManager { get; set; }
        PracticeBotManager PracticeBotManager { get; set; }

        BotReplayStorage BotReplayStorage { get; set; }
        Dictionary<ulong,int> ReplayToEdit {  get; set; } = new Dictionary<ulong,int>();
        Dictionary<ulong,PlayerReplay> replays = new Dictionary<ulong, PlayerReplay>();
        Dictionary<ulong,PlayerReplay> replaysToRecord = new Dictionary<ulong,PlayerReplay>();
        Dictionary<int, PlayerReplay> replaysToReplay = new Dictionary<int, PlayerReplay>();

        Listeners.OnEntitySpawned onESpawn;
        public BotReplayManager(ref PracticeBotManager practiceBotManager,ref ProjectileManager projectileManager) 
        {
            ProjectileManager = projectileManager;
            PracticeBotManager = practiceBotManager;
            CSPraccPlugin.Instance.Logger.LogInformation("Creating Bot Replay storage");
            BotReplayStorage = new BotReplayStorage(new FileInfo(Path.Combine(CSPraccPlugin.ModuleDir.FullName, "BotReplays", "BotReplay_" + Server.MapName + ".json")));
            CSPraccPlugin.Instance.Logger.LogInformation($"Created Bot Replay storage Count: {BotReplayStorage.GetAll().Count}");

            CSPraccPlugin.Instance!.RegisterListener<Listeners.OnTick>(OnTick);
            onESpawn = new Listeners.OnEntitySpawned(entity => OnEntitySpawned(entity));
            CSPraccPlugin.Instance.RegisterListener<Listeners.OnEntitySpawned>(onESpawn);
            CSPraccPlugin.Instance.RegisterEventHandler<EventPlayerShoot>(OnPlayerShoot);
        }

        public void CreateReplaySet(CCSPlayerController player, string name) 
        {
            int id = BotReplayStorage.Add(new ReplaySet(new List<PlayerReplay>(), name));
            ReplayToEdit.SetOrAdd(player.SteamID, id);
        }


        /// <summary>
        /// Record player and add to replay
        /// </summary>
        /// <param name="player">who issued the command</param>
        /// <param name="name">name of the replay, if not set, playername is used</param>
        public void RecordPlayer(CCSPlayerController player,string name = "")
        {
            if (replaysToRecord.ContainsKey(player.SteamID)) return;

            if (name == "") name = player.PlayerName;
            replaysToRecord.Add(player.SteamID, new PlayerReplay(name));
            player.ChatMessage($"Recording role {name}");
        }
        /// <summary>
        /// Stop recording current replay
        /// </summary>
        /// <param name="player">player who issued the command</param>
        /// <returns>Return the replay</returns>
        public PlayerReplay? StopRecording(CCSPlayerController player)
        {
            if(replaysToRecord.ContainsKey(player.SteamID))
            {
                PlayerReplay replay =  replaysToRecord[player.SteamID];
                replaysToRecord.Remove(player.SteamID);
                replays.SetOrAdd(player.SteamID, replay);
                player.ChatMessage($"Stopped recording for role {replay.ReplayName}");
                return replay;

            }
            else
            {
                player.ChatMessage("You are currently not recording.");
            }
            return null;
        }
        /// <summary>
        /// Rename last recorded replay
        /// </summary>
        /// <param name="player">who issued the command</param>
        /// <param name="name">name of the replay</param>
        public void NameReplay(CCSPlayerController player,string name)
        {
            if (replays.TryGetValue(player.SteamID, out PlayerReplay? replay))
            {
                replay.ReplayName = name;
            }
            else
            {
                player.ChatMessage("Could not get last replay recorded.");
            }
        }

        /// <summary>
        /// Save last recorded replay and add it to the current replay set
        /// </summary>
        /// <param name="player"></param>
        public void SaveLastReplay(CCSPlayerController player)
        {
            if(!replays.TryGetValue(player.SteamID, out PlayerReplay? replay))
            {
                Utils.ClientChatMessage("No Replay recorded.", player);
                return;
            }
            if (replay == null)
            {
                Utils.ClientChatMessage("Error getting replays from storage.", player);
                return;
            }

            if (!getPlayerEditReplay(player, out int replayid))
            {
                Utils.ClientChatMessage("You need to select a replay to edit.", player);
                return;
            }

            if (BotReplayStorage.Get(replayid, out ReplaySet? replayList))
            {
                if (replayList == null)
                {
                    replayList = new ReplaySet(new List<PlayerReplay>(), "");
                }
                replayList.Replays.Add(replay);
                BotReplayStorage.SetOrAdd(replayid, replayList);
                replays.Remove(player.SteamID);
                Utils.ClientChatMessage("Added last recorded replay to storage", player);
            }
            else
            {
                Utils.ClientChatMessage("Error getting replays from storage.", player);
                return;
            }
            player.ChatMessage("Stored last replay");
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
        public void RenameCurrentReplaySet(CCSPlayerController player,string name)
        {
            if(!getPlayerEditReplay(player, out int ReplayId))
            {
                player.ChatMessage("Could not get current replay set.");
                return;
            }
            if (!BotReplayStorage.ContainsKey(ReplayId))
            {
                player.ChatMessage($"Could not get current replay with id {ReplayId}.");
                return;
            }
            if (!BotReplayStorage.Get(ReplayId, out ReplaySet set))
            {
                player.ChatMessage("Could not get current replay set.");
                return;
            }
            if (set == null)
            {
                player.ChatMessage("Could not get current replay set.");
                return;
            }
            set.SetName = name;
            BotReplayStorage.SetOrAdd(ReplayId, set);
            player.ChatMessage($"Updated setname to {ChatColors.Green}{name}{ChatColors.White}.");
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
            CSPraccPlugin.Instance.DeregisterListener<Listeners.OnEntitySpawned>(onESpawn);
            GameEventHandler<EventPlayerShoot> playershoot = OnPlayerShoot;
            CSPraccPlugin.Instance!.DeregisterEventHandler("player_shoot", playershoot, true);
        }
    }
}
