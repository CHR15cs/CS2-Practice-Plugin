using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Events;
using CSPracc.DataModules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CounterStrikeSharp.API.Core.BasePlugin;

namespace CSPracc.Managers.PrefireManagers
{
    public class PrefireBotRespawnManager : IDisposable
    {
        CSPraccPlugin Plugin;
        PrefireRoute currentRoute;
        public PrefireBotRespawnManager(ref CSPraccPlugin plugin, PrefireRoute prefireRoute) 
        { 
            Plugin = plugin;
            currentRoute = prefireRoute;
            Plugin.RegisterEventHandler<EventPlayerSpawn>(OnPlayerSpawn, HookMode.Post);
        }

        public void Dispose()
        {
            GameEventHandler<EventPlayerSpawn> onPlayerSpawned = new GameEventHandler<EventPlayerSpawn>(OnPlayerSpawn);
            Plugin.DeregisterEventHandler("player_spawn", onPlayerSpawned, true);
        }

        public void SpawnNextPosition(CCSPlayerController bot, PrefireRoute prefireRoute)
        {
            var SpawnPointsPerBot = PrefireBotSpawnpointManager.GenerateSpawnpositionsPerBot(prefireRoute);
            if (!SpawnPointsPerBot.TryGetValue(bot, out List<JsonSpawnPoint>? spawnPoints))
            {
                return;
            }
            if (spawnPoints.Count == 0)
            {
                return;
            }
            bot.TeleportToJsonSpawnPoint(spawnPoints.FirstOrDefault());
            spawnPoints.RemoveAt(0);
        }

        public HookResult OnPlayerSpawn(EventPlayerSpawn @event, GameEventInfo info)
        {
            if (@event.Userid == null || !@event.Userid.IsValid) return HookResult.Continue;
            SpawnNextPosition(@event.Userid, currentRoute);
            bool akFound = false;
            foreach (var weapon in @event.Userid!.PlayerPawn!.Value!.WeaponServices!.MyWeapons)
            {

                if (weapon.Value!.DesignerName == "weapon_ak47")
                {
                    akFound = true;
                    continue;
                }
                @event.Userid.PlayerPawn.Value.RemovePlayerItem(weapon.Value);
            }
            if (!akFound)
                @event.Userid.GiveNamedItem("weapon_ak47");
            @event.Userid.ExecuteClientCommand("slot 0");
            return HookResult.Continue;
        }
    }
}
