using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API;
using CSPracc.DataModules;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CounterStrikeSharp.API.Core.BasePlugin;
using System.Collections.Concurrent;

namespace CSPracc.Managers.BaseManagers.CommandManagerFolder
{
    public class CommandExecuter :IDisposable
    {
        ConcurrentDictionary<string, PlayerCommand> Commands;
        public CommandExecuter(ref ConcurrentDictionary<string, PlayerCommand> commands) 
        { 
            Commands = commands;
            CSPraccPlugin.Instance.RegisterEventHandler<EventPlayerChat>(EventPlayerChat);
        }
        public void Dispose()
        {
            GameEventHandler<EventPlayerChat> playerChat = new GameEventHandler<EventPlayerChat>(EventPlayerChat);
            CSPraccPlugin.Instance.DeregisterEventHandler("player_chat", playerChat, true);
        }
        private HookResult EventPlayerChat(EventPlayerChat @event, GameEventInfo info)
        {
            CCSPlayerController? player = Utilities.GetPlayerFromUserid(@event.Userid);
            if (player is null)
            {
                CSPraccPlugin.Instance.Logger.LogError($"Could not get player from userid {@event.Userid}");
                return HookResult.Continue;
            }
            string textFromPlayer = @event.Text;
            string? command = PlayerCommandParser.GetPlayerCommandFromTextOrDefault(textFromPlayer);
            if (String.IsNullOrWhiteSpace(command))
            {
                CSPraccPlugin.Instance!.Logger.LogWarning($"Could not get command from player text");
                return HookResult.Continue;
            }
            CommandAliasManager.Instance.ReplaceAlias(player, command, out command);
            PlayerCommandArgument playerCommandArgument = new (textFromPlayer);
            if (!Commands.TryGetValue(command, out PlayerCommand? commandToExecute))
            {
                CSPraccPlugin.Instance!.Logger.LogWarning($"Could not get command");
                return HookResult.Continue;
            }
            if (commandToExecute is null)
            {
                CSPraccPlugin.Instance!.Logger.LogWarning($"Could to execute is null");
                return HookResult.Continue;
            }
            CSPraccPlugin.Instance!.Logger.LogInformation($"Executing command: {commandToExecute.Name} {String.Join(", ", playerCommandArgument)}");
            if (commandToExecute.ExecuteCommand(player, playerCommandArgument) != true)
            {
                CSPraccPlugin.Instance!.Logger.LogWarning("Error while executing your command");
            }
            return HookResult.Continue;
        }
    }
}
