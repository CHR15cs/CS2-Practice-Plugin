using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Admin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSPracc.DataModules
{
    public class PlayerCommand
    {
        public string Name { get; set; }
        public string Description { get; set; } 
        private string permissionRequirement = "";
        private Func<CCSPlayerController,List<string>,bool> CommandToExecute;
        public PlayerCommand(string name,string description,Func<CCSPlayerController,List<string>,bool> commandToExecute,string? requiredFlag) 
        {
            Name = name;
            Description = description;
            if(requiredFlag != null)
            {
                permissionRequirement = requiredFlag;
            }
            CommandToExecute = commandToExecute;
        }

        public bool ExecuteCommand(CCSPlayerController playerController, List<string> args) 
        {
            if (!playerController.IsAdmin() && !AdminManager.PlayerHasPermissions(playerController, permissionRequirement))
            {
                playerController.PrintToChat($"You don't have enough permissions to execute the command. Required permission is {permissionRequirement}");
                return false;
            }
            return CommandToExecute(playerController, args);
        }
    }
}
