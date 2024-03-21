using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using CSPracc.DataModules;
using CSPracc.DataStorages.JsonStorages;
using CSPracc.Managers.BaseManagers.CommandManagerFolder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSPracc.Managers.PrefireManagers
{
    public class PrefireRouteEditor : IDisposable
    {
        CSPraccPlugin Plugin;
        PrefireRoute CurrentPrefireRoute;
        CommandManager CommandManager;
        PrefireRouteStorage Storage;
        public PrefireRouteEditor(ref CSPraccPlugin plugin,ref CommandManager command, ref PrefireRoute prefireRoute, ref PrefireRouteStorage prefireRouteStorage) 
        { 
            Plugin = plugin;
            CurrentPrefireRoute = prefireRoute;
            CommandManager = command;
            Storage = prefireRouteStorage;
        }

        public void SetStartingPointCommandHandler(CCSPlayerController playerController,PlayerCommandArgument args)
        {
            CurrentPrefireRoute.StartingPoint = playerController.GetCurrentPositionAsJsonSpawnPoint();
            playerController.ChatMessage($"Set current position as starting point for {ChatColors.Blue}{CurrentPrefireRoute.Name}{ChatColors.White}.");
        }
        public bool AddSpawnCommandHandler(CCSPlayerController playerController, PlayerCommandArgument args)
        {
            JsonSpawnPoint? spawnPointToAdd = playerController.GetCurrentPositionAsJsonSpawnPoint();
            if(spawnPointToAdd is null )
            {
                playerController.ChatMessage("Could not get current position. Spawn not added.");
                return false;
            }
            CurrentPrefireRoute.spawnPoints.Add(spawnPointToAdd);
            playerController.ChatMessage($"Added current position as bot position for {ChatColors.Blue}{CurrentPrefireRoute.Name}{ChatColors.White}.");
            return true;
        }
        public void SaveCurrentRouteCommandHandler(CCSPlayerController player, PlayerCommandArgument args)
        {
            foreach (var item in Storage.GetAll())
            {
                if (item.Value.Name == CurrentPrefireRoute.Name)
                {
                    Storage.SetOrAdd(item.Key, CurrentPrefireRoute);
                }
            }
        }

        public void Dispose()
        {
            CommandManager.DeregisterCommand("");
        }
    }
}
