using CSPracc.DataModules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSPracc.Managers.BaseManagers
{
    public abstract class BaseManager : IManager
    {
        protected CommandManager CommandManager;
        protected Dictionary<string, PlayerCommand> Commands { get; set; } = new Dictionary<string, PlayerCommand>();
        public BaseManager() 
        {
            CommandManager = CommandManager.Instance;
        }
        public void DeregisterCommands()
        {
            foreach (var command in Commands)
            {
                CommandManager.DeregisterCommand(command.Key);
            }
        }

        public void Dispose()
        {
            DeregisterCommands();
        }

        public void RegisterCommands()
        {
            foreach (var command in Commands)
            {
                CommandManager.RegisterCommand(command.Value);
            }
        }
    }
}
