using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Timers;
using CounterStrikeSharp.API.Modules.Utils;
using CSPracc.DataModules;
using CSPracc.DataStorages.JsonStorages;
using CSPracc.Extensions;
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
    public class PrefireRouteManager
    {
        private bool editing = false;
        public PrefireRoute? CurrentPrefireRoute {  get; private set; }

        private PrefireRouteStorage PrefireRouteStorage { get; set; }

        Dictionary<int,List<JsonSpawnPoint>> SpawnPointsPerBot { get; set; }

        public PrefireRouteManager() 
        {
            PrefireRouteStorage = new PrefireRouteStorage(new FileInfo(Path.Combine(CSPraccPlugin.Instance.ModuleDirectory, "Prefire", $"{Server.MapName}.json")));
            SpawnPointsPerBot = new Dictionary<int, List<JsonSpawnPoint>>();
        }

        public bool AddNewRoute(string name)
        {
            foreach(var item in PrefireRouteStorage.GetAll())
            {
                if(item.Value.Name == name)
                {
                    Utils.ServerMessage($" A route with name {name} already exists.");
                    return false;
                }
            }
            PrefireRouteStorage.Add(PrefireRouteStorage.GetAll().Count + 1, new PrefireRoute(name));
            Utils.ServerMessage($"Added route {name}. Use .editroute {name} to edit the route.");
            return true;
        }
        public void SetStartingPoint(JsonSpawnPoint? startingPoint)
        {
            if(!editing)
            {
                Utils.ServerMessage("Need to be in editing mode.");
                return;
            }
            if(CurrentPrefireRoute == null)
            {
                Utils.ServerMessage("No route is currently selected.");
                return;
            }
            if (startingPoint == null)
            {
                Utils.ServerMessage("Could not set empty starting point.");
                return;
            }
            CurrentPrefireRoute.StartingPoint = startingPoint;
        }

        public void EditRoute(string name) 
        {
            editing = true;
            PrefireRoute? prefireRoute = GetPrefireRouteByName(name);
            if(prefireRoute == null) 
            { 
                Utils.ServerMessage($"Could not find prefire route {name}");
                return;
            }
            CurrentPrefireRoute = prefireRoute;
            Utils.ServerMessage($"You are now editing {name}.");
        }

        public void AddSpawn(JsonSpawnPoint spawnPoint)
        {
            if(!editing)
            {
                Utils.ServerMessage($"You are not in editing mode.");
            }
            if(CurrentPrefireRoute == null)
            {
                Utils.ServerMessage($"No route is currently being edited.");
                return;
            }
            CurrentPrefireRoute!.spawnPoints.Add(spawnPoint);
            Utils.ServerMessage($"Added spawn. {spawnPoint.Position}");
        }

        public void SaveCurrentRoute()
        {
            if (CurrentPrefireRoute == null) return;
            foreach( var item in PrefireRouteStorage.GetAll())
            {
                if(item.Value.Name == CurrentPrefireRoute.Name)
                {
                    PrefireRouteStorage.SetOrAdd(item.Key, CurrentPrefireRoute);
                }
            }
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


        public HtmlMenu GetPrefireRouteMenu() 
        {
            List<KeyValuePair<int,PrefireRoute>> PrefireRoutes = PrefireRouteStorage.GetAll();
            List<KeyValuePair<string,Action>> menuOptions = new List<KeyValuePair<string,Action>>();
            foreach(KeyValuePair<int,PrefireRoute> route in  PrefireRoutes)
            {
                menuOptions.Add(new KeyValuePair<string, Action>($"{route.Value.Name}", () => { LoadRouteById(route.Key); }));
            }
            return new HtmlMenu("Prefire Menu", menuOptions);
        }

        private bool GenerateRoute(PrefireRoute route)
        {
            Server.PrintToConsole($"Generating route {route.Name}");
            Server.PrintToConsole($"Adding bots.");
            addBots();
            Server.PrintToConsole($"Assigning spawns to bots");
            CSPraccPlugin.Instance!.AddTimer(1.0f, () => { assignSpawnpositionsToBot(route); spawnFirstBotWave(); teleportPlayerToStartingPoint(); });
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

        private void addBots()
        {
            for (int i = 0; i < 5; i++)
            {
                Server.ExecuteCommand("bot_join_team CT");
                Server.ExecuteCommand("bot_add_ct");
            }
        }

        private void assignSpawnpositionsToBot(PrefireRoute route)
        {
            List<CCSPlayerController> bots = Utilities.GetPlayers().Where(x => x.IsBot && x.IsValid && !x.IsHLTV).ToList();
            int botNumber = 0;
            foreach (JsonSpawnPoint? spawnPoint in route.spawnPoints)
            {
                if (spawnPoint == null) continue;

                if (!SpawnPointsPerBot.ContainsKey(bots[botNumber].Slot))
                {
                    SpawnPointsPerBot.Add(bots[botNumber].Slot, new List<JsonSpawnPoint>());
                }
                SpawnPointsPerBot[bots[botNumber].Slot].Add(spawnPoint);
                if (botNumber + 1 < bots.Count)
                {
                    botNumber++;
                }
                else
                {
                    botNumber = 0;
                }
            }
        }

        private void spawnFirstBotWave()
        {           
            foreach (int key in SpawnPointsPerBot.Keys) 
            {
                Server.PrintToConsole($"Spawn bot slot {key}");
                SpawnNextPosition(key);
            }
        }

        public void SpawnNextPosition(int botIndex)
        {
            CCSPlayerController? botToSpawn = Utilities.GetPlayerFromSlot(botIndex);
            if (botToSpawn == null) return;
            if (editing) return;
            if (!SpawnPointsPerBot.TryGetValue(botIndex, out List<JsonSpawnPoint>? spawnPoints))
            {
                return;
            }
            if (spawnPoints.Count == 0)
            {
                return;
            }
   ;

            Server.PrintToConsole($"Teleporting bot {botToSpawn.PlayerName}");
            Server.PrintToConsole($"location: {spawnPoints.FirstOrDefault()!.Position}");
            Server.PrintToConsole($"QAngle: {spawnPoints.FirstOrDefault()!.QAngle.ToCSQAngle()}");
            Server.PrintToConsole($"Valid: {botToSpawn.PlayerPawn.IsValid}");
            botToSpawn.TeleportToJsonSpawnPoint(spawnPoints.FirstOrDefault());
            spawnPoints.RemoveAt(0);
        }

        public bool LoadRouteById(int id)
        {
            editing = false;
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

        public bool LoadRouteByName(string name)
        {
            List<KeyValuePair<int, PrefireRoute>> PrefireRoutes = PrefireRouteStorage.GetAll();
            foreach (KeyValuePair<int, PrefireRoute> route in PrefireRoutes)
            {
                if(route.Value.Name == name)
                {
                    return LoadRouteById(route.Key);
                }
            }
            return false;
        }

    }
}
