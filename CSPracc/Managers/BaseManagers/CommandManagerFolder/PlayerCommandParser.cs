using CounterStrikeSharp.API.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSPracc.Managers.BaseManagers.CommandManagerFolder
{
    public class PlayerCommandParser
    {
        public static string? GetPlayerCommandFromTextOrDefault(string input)
        {
            string command = String.Empty;
            List<string> triggers = CoreConfig.PublicChatTrigger.Concat(CoreConfig.SilentChatTrigger).ToList();
            if (!triggers.Any(x => input.StartsWith(x)))
            {
                return null;
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
                return null;
            }
            return command;
        }

        public static List<string>? GetArgsFromPlayerCommandOrDefault(string argument)
        {
            List<string> arguments = new();
            if (String.IsNullOrWhiteSpace(argument))
            {
                return null;
            }
            do
            {
                argument = argument.Trim();
                int index = argument.IndexOf(' ');
                if (index == -1)
                {
                    arguments.Add(argument);
                    argument = string.Empty;
                    break;
                }
                string foundArgument = argument.Substring(0, index);
                arguments.Add(foundArgument);
                argument = argument.Substring(index);
            } while (argument.Length > 0 && argument != String.Empty);
            arguments.RemoveAt(0);
            return arguments;
        }
    }
}
