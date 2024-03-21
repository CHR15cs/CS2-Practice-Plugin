using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Timers;
using CounterStrikeSharp.API.Modules.Utils;
using CSPracc.DataModules;
using CSPracc.DataStorages.JsonStorages;
using CSPracc.Extensions;
using CSPracc.Managers.PrefireManagers;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace CSPracc.Managers
{
    public class PrefireRouteManager : IDisposable
    {
        private bool editing = false;
        public PrefireRoute? CurrentPrefireRoute {  get; private set; }

        private PrefireRouteStorage PrefireRouteStorage { get; set; }

        PrefireBotViewAngleManager _prefireBotViewAngleManager;
        PrefireBotViewAngleManager PrefireBotViewAngleManager 
        { 
            get 
            {
                return _prefireBotViewAngleManager;
            } 
            set 
            { 
                _prefireBotViewAngleManager = value;
            } 
        }

        PrefireRouteAdderManager _prefireRouteAdderManager;
        PrefireRouteAdderManager PrefireRouteAdderManager
        {
            get
            {
                return _prefireRouteAdderManager;
            }
            set
            {
                _prefireRouteAdderManager = value;
            }
        }
        CCSPlayerController? playerToShoot { get; set; } = null;
        CSPraccPlugin Plugin;
        public PrefireRouteManager(ref CSPraccPlugin plugin) 
        {
            Plugin = plugin;
            PrefireRouteStorage = new PrefireRouteStorage();
        }

        public void EditRoute(string name) 
        {
            editing = true;
            PrefireRoute? prefireRoute = PrefireRouteStorage.GetRouteByNameOrDefault(name);
            if(prefireRoute == null) 
            { 
                Utils.ServerMessage($"Could not find prefire route {name}");
                return;
            }
            CurrentPrefireRoute = prefireRoute;
            Utils.ServerMessage($"You are now editing {name}.");
        }

        private PrefireRoute? GetPrefireRouteByName(string name) 
        {
            foreach (var item in PrefireRouteStorage.GetAll())
            {
                if (item.Value.Name == name)
                {
                   return item.Value;
                }
            }
            return null;
        }


        public HtmlMenu GetPrefireRouteMenu(CCSPlayerController player) 
        {
            List<KeyValuePair<int,PrefireRoute>> PrefireRoutes = PrefireRouteStorage.GetAll();
            List<KeyValuePair<string,Action>> menuOptions = new List<KeyValuePair<string,Action>>();
            foreach(KeyValuePair<int,PrefireRoute> route in  PrefireRoutes)
            {
                menuOptions.Add(new KeyValuePair<string, Action>($"{route.Value.Name}", () => { LoadRouteById(route.Key,player); }));
            }
            return new HtmlMenu("Prefire Menu", menuOptions);
        }

        private bool GenerateRoute(PrefireRoute route)
        {
            Server.ExecuteCommand("bot_kick");
            Server.PrintToConsole($"Generating route {route.Name}");
            Server.PrintToConsole($"Adding bots.");
            //addBots(route);
            Server.ExecuteCommand("bot_freeze 0");
            Server.ExecuteCommand("bot_stop 0");
            Server.ExecuteCommand("bot_quota_mode normal");
            Server.PrintToConsole($"Assigning spawns to bots");
            Server.ExecuteCommand("mp_restartgame 1");
            //CSPraccPlugin.Instance!.AddTimer(2.0f, () => { assignSpawnpositionsToBot(route); spawnFirstBotWave(); teleportPlayerToStartingPoint();  });

            
            Server.PrintToConsole($"Spawning firt wave");
            CurrentPrefireRoute = route;
            Utils.ServerMessage($"Starting prefire route {route.Name}");
            return false;
        }

        private void teleportPlayerToStartingPoint()
        {
            if (CurrentPrefireRoute == null) return;
            var players = Utilities.GetPlayers();
            foreach( var player in players )
            {
                if (player == null) continue;
                if (!player.IsValid) continue;
                if (player.IsBot) continue;
                player.TeleportToJsonSpawnPoint(CurrentPrefireRoute.StartingPoint);
            }
        }

        private void spawnFirstBotWave()
        {           
            //foreach (int key in SpawnPointsPerBot.Keys) 
            //{
            //    Server.PrintToConsole($"Spawn bot slot {key}");
            //    SpawnNextPosition(key);
            //}
        }

        public bool LoadRouteById(int id, CCSPlayerController playerToShoot)
        {
            editing = false;
            PrefireBotViewAngleManager = new PrefireBotViewAngleManager(ref Plugin, playerToShoot);
            if (!PrefireRouteStorage.Get(id, out PrefireRoute route))
            {
                Utils.ServerMessage($"Could not load route with id: {id}");
                return false;
            }
            if(route == null)
            {
                Utils.ServerMessage("Route could not be loaded");
                return false;
            }
            return GenerateRoute(route);
        }

        public bool LoadRouteByName(string name,CCSPlayerController player)
        {
            List<KeyValuePair<int, PrefireRoute>> PrefireRoutes = PrefireRouteStorage.GetAll();
            foreach (KeyValuePair<int, PrefireRoute> route in PrefireRoutes)
            {
                if(route.Value.Name == name)
                {
                    return LoadRouteById(route.Key,player);
                }
            }
            return false;
        }

        public void SetPlayerToShoot(CCSPlayerController player)
        {
            playerToShoot  = player;
        }

        public void Dispose()
        {

        }
    }
}
