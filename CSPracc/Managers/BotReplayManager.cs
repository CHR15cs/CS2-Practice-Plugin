using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Entities;
using CounterStrikeSharp.API.Modules.Utils;
using CSPracc.DataModules;
using CSPracc.DataModules.Constants;
using CSPracc.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CounterStrikeSharp.API.Core.BasePlugin;

namespace CSPracc.Managers
{
    public class BotReplayManager : IDisposable
    {
        ProjectileManager ProjectileManager { get; set; }
        PracticeBotManager PracticeBotManager { get; set; }
        Dictionary<ulong,PlayerReplay> replays = new Dictionary<ulong, PlayerReplay>();
        Dictionary<ulong,PlayerReplay> replaysToRecord = new Dictionary<ulong,PlayerReplay>();
        Dictionary<int, PlayerReplay> replaysToReplay = new Dictionary<int, PlayerReplay>();
        public BotReplayManager(ref PracticeBotManager practiceBotManager,ref ProjectileManager projectileManager) 
        {
            ProjectileManager = projectileManager;
            PracticeBotManager = practiceBotManager;
            CSPraccPlugin.Instance!.RegisterListener<Listeners.OnTick>(OnTick);

            CSPraccPlugin.Instance.RegisterListener<Listeners.OnEntitySpawned>(entity => OnEntitySpawned(entity));
            CSPraccPlugin.Instance.RegisterEventHandler<EventPlayerShoot>(OnPlayerShoot);
        }

        public void RecordPlayer(CCSPlayerController player)
        {
            if (replaysToRecord.ContainsKey(player.SteamID)) return;

            replaysToRecord.Add(player.SteamID, new PlayerReplay(player.PlayerName));
        }

        public PlayerReplay? StopRecording(CCSPlayerController player)
        {
            if(replaysToRecord.ContainsKey(player.SteamID))
            {
                PlayerReplay replay =  replaysToRecord[player.SteamID];
                replaysToRecord.Remove(player.SteamID);
                replays.SetOrAdd(player.SteamID, replay);
                return replay;
            }
            return null;
        }

        public void ReplayLastReplay(CCSPlayerController player)
        {
            if (replays.ContainsKey(player.SteamID))
            {
                ReplayReplay(player, replays[player.SteamID]);
            }
        }

        public void ReplayReplay(CCSPlayerController player, PlayerReplay replay)
        {
            PracticeBotManager.AddBot(player);
            List<CCSPlayerController?>? playerList = Utilities.GetPlayers()!.ToList();
            if (playerList == null) return;
            CCSPlayerController? bot = null;
            foreach(CCSPlayerController? playerController in playerList)
            {
                if (playerController == null || !playerController.IsValid || !playerController.IsBot || playerController.IsHLTV)
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
            replaysToReplay.SetOrAdd(bot.Slot, replay);  
        }

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
                    Utils.ServerMessage($"Replay finished.");
                    replaysToReplay.Remove(slot); 
                    continue;
                }
                player.TeleportToPosition(frame.Position);
                bool weaponFound = false;
                if (frame.ProjectileSnapshot != null)
                {
                    ProjectileManager.ThrowGrenadePojectile(frame.ProjectileSnapshot, player);
                }
                if (frame.shoot)
                {
                    var wep = new CCSWeaponBase(player.PlayerPawn.Value!.WeaponServices!.ActiveWeapon.Value.Handle);

                    if (wep == null || !wep.IsValid)
                    {
                        Utils.ServerMessage("weapon is null");
                        continue;
                    }


                    Server.NextFrame(() =>
                    {
                        wep.NextPrimaryAttackTick = Server.TickCount + 10;

                    });
                }

                //player.PlayerPawn!.Value.Bot.TargetSpot.X = frame.Position.PlayerPosition.X;
                //player.PlayerPawn!.Value.Bot.TargetSpot.Y = frame.Position.PlayerPosition.Y;
                //player.PlayerPawn!.Value.Bot.TargetSpot.Z = frame.Position.PlayerPosition.Z;
            }
        }

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
        }
    }
}
