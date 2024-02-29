using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using CSPracc.DataModules;
using CSPracc.DataModules.Constants;
using CSPracc.Extensions;
using CSPracc.Managers.BaseManagers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSPracc.Managers.PracticeManagers
{
    public class CheckpointManager : BaseManager
    {
        GuiManager GuiManager;
        Dictionary<CCSPlayerController, Position> checkpoints { get; set; }
        public CheckpointManager(ref CommandManager commandManager, ref GuiManager guiManager) : base(ref commandManager)
        { 
            GuiManager = guiManager;
            checkpoints = new Dictionary<CCSPlayerController, Position>();
            Commands.Add(PRACC_COMMAND.CHECKPOINT, new PlayerCommand(PRACC_COMMAND.CHECKPOINT, "Save current position as checkpoint", CheckpointCommandHandler, null));
            Commands.Add(PRACC_COMMAND.TELEPORT, new PlayerCommand(PRACC_COMMAND.TELEPORT, "Teleport to last saved Checkpoint", TeleportCommandHandler, null));
        }

        public bool CheckpointCommandHandler(CCSPlayerController playerController, List<string> args)
        {
            checkpoints!.SetOrAdd(playerController, playerController.GetCurrentPosition());
            playerController.ChatMessage("Saved current position as checkpoint");
            return true;
        }

        public bool TeleportCommandHandler(CCSPlayerController playerController, List<string> args)
        {
            if (!checkpoints.ContainsKey(playerController))
            {
                playerController.ChatMessage($"You dont have a saved checkpoint");
                return false;
            }
            playerController.TeleportToPosition(checkpoints[playerController]);
            playerController.ChatMessage("Teleported to your checkpoint");
            return true;
        }
    }
}
