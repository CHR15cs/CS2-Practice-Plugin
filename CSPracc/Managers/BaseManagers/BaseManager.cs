using CSPracc.DataModules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSPracc.Managers.BaseManagers
{
    /// <summary>
    /// Base class for all managers
    /// </summary>
    public abstract class BaseManager : IManager
    {
        /// <summary>
        /// Command Manager
        /// </summary>
        protected CommandManager CommandManager
        {
            get
            {
                return CommandManager.Instance;
            }
        }
        /// <summary>
        /// Dictionary of commands
        /// </summary>
        protected Dictionary<string, PlayerCommand> Commands { get; set; } = new Dictionary<string, PlayerCommand>();

        /// <summary>
        /// Constructor for the base manager
        /// </summary>
        public BaseManager() 
        {
            
        }
        /// <summary>
        /// Deregister all commands
        /// </summary>
        public void DeregisterCommands()
        {
            foreach (var command in Commands)
            {
                CommandManager.DeregisterCommand(command.Key);
            }
        }

        /// <summary>
        /// Dispose of the manager
        /// </summary>
        public void Dispose()
        {
            DeregisterCommands();
        }

        /// <summary>
        /// Register all commands
        /// </summary>
        public void RegisterCommands()
        {
            foreach (var command in Commands)
            {
                CommandManager.RegisterCommand(command.Value);
            }
        }
    }
}
