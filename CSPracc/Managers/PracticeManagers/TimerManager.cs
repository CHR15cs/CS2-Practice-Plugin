﻿using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using CSPracc.DataModules.Constants;
using CSPracc.Managers.BaseManagers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSPracc.Managers.PracticeManagers
{
    public class TimerManager : BaseManager
    {
        GuiManager GuiManager;
        public TimerManager(ref CommandManager commandManager, ref GuiManager guiManager) : base(ref commandManager)
        { 
            GuiManager = guiManager;
            Commands.Add(PRACC_COMMAND.timer,new DataModules.PlayerCommand(PRACC_COMMAND.timer,"Start timer", TimerCommandHandler, null));
        }
        public bool TimerCommandHandler(CCSPlayerController playerController, List<string> args)
        {
            StartTimer(playerController);
            return true;
        }
        public void StartTimer(CCSPlayerController player)
        {
            if (player == null) return;
            GuiManager.StartTimer(player);
        }
    }
}
