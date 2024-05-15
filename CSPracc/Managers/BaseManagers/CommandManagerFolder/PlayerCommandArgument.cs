using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSPracc.Managers.BaseManagers.CommandManagerFolder
{
    public class PlayerCommandArgument
    {
        public List<string> ArgumentList { get; private set; } = new List<string>();
        public string ArgumentString { get; private set; } = string.Empty;
        
        public int ArgumentCount { get; private set; } = 0;

        public PlayerCommandArgument(string argumentString)
        {
            parseArgument(argumentString);
        }

        private void parseArgument(string argument)
        {
            List<string> arguments = new();
            if (String.IsNullOrWhiteSpace(argument))
            {
                return;
            }
            if (!argument.Contains(" "))
            {
                ArgumentString = argument;
                ArgumentList.Add(argument);
                ArgumentCount = 1;
                return;
            }
            else
            {
                argument = argument[0..(argument.IndexOf(" ")+1)];
                ArgumentString = argument;
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
            ArgumentCount = arguments.Count;
            return;
        }

        public override string ToString()
        {
            return ArgumentString;
        }
    }
}
