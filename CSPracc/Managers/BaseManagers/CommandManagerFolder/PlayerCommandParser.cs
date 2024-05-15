using CounterStrikeSharp.API.Core;
using Microsoft.Extensions.Logging;
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
            CSPraccPlugin.Instance.Logger.LogInformation($"Checking for command in {input}");
            CSPraccPlugin.Instance.Logger.LogInformation($"triggers {String.Join(", ",triggers)}");
            if (!triggers.Any(x => input.StartsWith(x)))
            {
                return null;
            }
            command = input[1..];
            CSPraccPlugin.Instance.Logger.LogInformation($"command: {command}");
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
            command = command.Trim();
            CSPraccPlugin.Instance.Logger.LogInformation($"returning: {command}");
            return command;
        }

    }
}
