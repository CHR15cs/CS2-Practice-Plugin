using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CSPracc.DataModules;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CounterStrikeSharp.API.Core.BasePlugin;
using static System.Runtime.InteropServices.JavaScript.JSType;
using String = System.String;

namespace CSPracc.Managers
{
    public class CommandManager : IDisposable
    {
        CSPraccPlugin Plugin;
        ConcurrentDictionary<string,PlayerCommand> Commands { get; set; } = new ConcurrentDictionary<string, PlayerCommand>();
        public CommandManager(ref CSPraccPlugin plugin) 
        {
            Plugin = plugin;
            Plugin.RegisterEventHandler<EventPlayerChat>(EventPlayerChat);
        }

        public void RegisterCommand(PlayerCommand command) 
        { 
            if(Commands.ContainsKey(command.Name)) return;
            Commands.TryAdd(command.Name,command);
        }

        private HookResult EventPlayerChat(EventPlayerChat @event,GameEventInfo info)
        {
            CCSPlayerController? player = Utilities.GetPlayerFromUserid(@event.Userid);
            if (player is null)
            {
                CSPraccPlugin.Instance!.Logger.LogError($"Could not get player from userid {@event.Userid}");
                return HookResult.Continue;
            }
            string textFromPlayer = @event.Text;
            string commandResult = GetCommandFromPlayerText(textFromPlayer);
            if (String.IsNullOrWhiteSpace(commandResult))
            {
                CSPraccPlugin.Instance!.Logger.LogWarning($"Could not get command from player text");
                return HookResult.Continue;
            }
            string command = commandResult;
            CommandAliasManager.Instance.ReplaceAlias(player, commandResult, out command);
            List<string> arguments = GetArgumentListFromPlayerText(textFromPlayer);
            CSPraccPlugin.Instance!.Logger.LogWarning($"Check for command {command}");
            if (!this.Commands.TryGetValue(command, out PlayerCommand? commandToExecute))
            {
                CSPraccPlugin.Instance!.Logger.LogWarning($"Could not get command");
                return HookResult.Continue;
            }
            if (commandToExecute is null)
            {
                CSPraccPlugin.Instance!.Logger.LogWarning($"Could to execute is null");
                return HookResult.Continue;
            }
            CSPraccPlugin.Instance!.Logger.LogInformation($"Executing command: {commandToExecute.Name} {String.Join(", ", arguments)}");
            if (commandToExecute.ExecuteCommand(player, arguments) != true)
            {
                CSPraccPlugin.Instance!.Logger.LogWarning("Error while executing your command");
            }
            return HookResult.Continue;
        }

        private string GetCommandFromPlayerText(string input)
        {
            string command = String.Empty;

            List<string> triggers = CoreConfig.PublicChatTrigger.Concat(CoreConfig.SilentChatTrigger).ToList();
            if (!triggers.Any(x => input.StartsWith(x)))
            {
                return "";
            }
            command = input[1..];

            try
            {
                //detect arguments
                if (command.Contains(' '))
                {
                    command = command[0..command.IndexOf(" ")];
                }
            }
            catch
            {
                return "";
            }
            return command;
        }
        private List<string> GetArgumentListFromPlayerText(string Argument)
        {
            List<string> arguments = new();
            if (String.IsNullOrWhiteSpace(Argument))
            {
                return new List<string>();
            }
            do
            {
                Argument = Argument.Trim();
                int index = Argument.IndexOf(' ');
                if (index == -1)
                {
                    arguments.Add(Argument);
                    Argument = string.Empty;
                    break;
                }
                string foundArgument = Argument.Substring(0, index);
                arguments.Add(foundArgument);
                Argument = Argument.Substring(index);
            } while (Argument.Length > 0 && Argument != String.Empty);
            arguments.RemoveAt(0);
            return arguments;
        }


        void IDisposable.Dispose()
        {
            GameEventHandler<EventPlayerChat> playerChat = new GameEventHandler<EventPlayerChat>(EventPlayerChat);
            Plugin.DeregisterEventHandler("player_chat", playerChat, true);
        }
    }
}
