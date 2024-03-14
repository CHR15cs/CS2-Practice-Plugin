﻿using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Cvars;
using CounterStrikeSharp.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSPracc.DataModules.Constants;
using CounterStrikeSharp.API.Modules.Utils;
using CSPracc.Managers.BaseManagers;

namespace CSPracc.Managers.PracticeManagers
{
    public class WatchMeManager : BaseManager
    {
        public WatchMeManager(ref CommandManager commandManager) : base(ref commandManager)
        { 
            Commands.Add(PRACC_COMMAND.WATCHME,new DataModules.PlayerCommand(PRACC_COMMAND.WATCHME,"Break all breakable entities", WatchMeCommandHandler, null));
        }

        public bool WatchMeCommandHandler(CCSPlayerController playerController, List<string> args)
        {
            var playerEntities = Utilities.GetPlayers();
            foreach (var playerEnt in playerEntities)
            {
                if (playerEnt == null) continue;
                if (!playerEnt.IsValid) continue;
                if (playerEnt.UserId == playerController.UserId) continue;
                if (playerEnt.IsBot) continue;
                playerEnt.ChangeTeam(CsTeam.Spectator);
                Logging.LogMessage($"Switching {playerEnt.PlayerName} to spectator");
            }
            return true;
        }

    }
}