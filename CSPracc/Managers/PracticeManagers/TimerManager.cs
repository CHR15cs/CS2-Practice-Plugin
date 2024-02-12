using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSPracc.Managers.PracticeManagers
{
    public class TimerManager : IDisposable
    {
        GuiManager GuiManager;
        public TimerManager(ref CommandManager commandManager, ref GuiManager guiManager) 
        { 
            GuiManager = guiManager;
            commandManager.RegisterCommand(new DataModules.PlayerCommand("","", CountdownCommandHandler,null));
        }


        private bool CountdownCommandHandler(CCSPlayerController playerController, List<string> args)
        {
            if (args.Count != 1)
            {
                if (int.TryParse(args[0], out int time))
                {
                    AddCountdown(playerController, time);
                    return true;
                }
            }
            playerController.ChatMessage($"{ChatColors.Red}Could not parse parameter");
            return false;
        }
        private void AddCountdown(CCSPlayerController player, int countdown)
        {
            GuiManager.StartCountdown(player, countdown);
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


        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
