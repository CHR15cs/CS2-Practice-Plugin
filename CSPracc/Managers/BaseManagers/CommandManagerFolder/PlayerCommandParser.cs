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

    }
}
