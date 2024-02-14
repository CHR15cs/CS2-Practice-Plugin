using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CSPracc.DataModules;
using CSPracc.DataModules.Constants;
using CSPracc.DataStorages.JsonStorages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSPracc.Managers.PrefireManagers
{
    public class PrefireRouteAdderManager : IDisposable
    {
        PrefireRouteStorage PrefireRouteStorage;

        public PrefireRouteAdderManager(ref CommandManager commandManager,ref PrefireRouteStorage prefireRouteStorage)
        {
            PrefireRouteStorage = prefireRouteStorage;
            commandManager.RegisterCommand(new PlayerCommand("addroute", "Add new route", AddNewRouteCommandHandler, null));
        }

        public void Dispose()
        {
            
        }

        public bool AddNewRouteCommandHandler(CCSPlayerController playerController,List<string> args)
        {
            if(args.Count == 0)
            {
                playerController.ChatMessage("Need to pass a name");
                return false;
            }
            string name = String.Join(" ", args);
            foreach (var item in PrefireRouteStorage.GetAll())
            {
                if (item.Value.Name == name)
                {
                    playerController.ChatMessage($" A route with name {name} already exists.");
                    return false;
                }
            }
            PrefireRouteStorage.Add(PrefireRouteStorage.GetAll().Count + 1, new PrefireRoute(name));
            playerController.ChatMessage($"Added route {name}. Use .editroute {name} to edit the route.");
            return true;
        }
    }
}
