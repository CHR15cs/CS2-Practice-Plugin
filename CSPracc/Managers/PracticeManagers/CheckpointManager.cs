using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using CSPracc.DataModules;
using CSPracc.DataModules.Constants;
using CSPracc.Extensions;
using CSPracc.Managers.BaseManagers;
using CSPracc.Managers.BaseManagers.CommandManagerFolder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSPracc.Managers.PracticeManagers
{
    /// <summary>
    /// Checkpoint manager
    /// </summary>
    public class CheckpointManager : BaseManager
    {
        private Dictionary<CCSPlayerController, Position> _checkpoints { get; set; }
        /// <summary>
        /// Constructor registering the commands
        /// </summary>
        public CheckpointManager() : base()
        {
            _checkpoints = new Dictionary<CCSPlayerController, Position>();
            Commands.Add(PRACC_COMMAND.CHECKPOINT, new PlayerCommand(PRACC_COMMAND.CHECKPOINT, "Save current position as checkpoint", CheckpointCommandHandler, null, null));
            Commands.Add(PRACC_COMMAND.TELEPORT, new PlayerCommand(PRACC_COMMAND.TELEPORT, "Teleport to last saved Checkpoint", TeleportCommandHandler, null, null));
        }
        private bool CheckpointCommandHandler(CCSPlayerController playerController, PlayerCommandArgument args)
        {
            _checkpoints!.SetOrAdd(playerController, playerController.GetCurrentPosition());
            playerController.ChatMessage("Saved current position as checkpoint");
            return true;
        }
        private bool TeleportCommandHandler(CCSPlayerController playerController, PlayerCommandArgument args)
        {
            if (!_checkpoints.ContainsKey(playerController))
            {
                playerController.ChatMessage($"You dont have a saved checkpoint");
                return false;
            }
            playerController.TeleportToPosition(_checkpoints[playerController]);
            playerController.ChatMessage("Teleported to your checkpoint");
            return true;
        }
    }
}
