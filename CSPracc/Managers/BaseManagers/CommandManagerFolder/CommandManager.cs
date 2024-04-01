using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CSPracc.DataModules;
using CSPracc.Managers.BaseManagers.CommandManagerFolder;
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
        private static CommandManager? _instance = null;
        public static CommandManager Instance
        {
            get
            {
                if(_instance == null)
                {
                    _instance = new CommandManager();    
                }
                return _instance;
            }
        }
        public ConcurrentDictionary<string, PlayerCommand> Commands;
        CommandExecuter CommandExecuter;
        private CommandManager() 
        {
            Commands = new ConcurrentDictionary<string, PlayerCommand>();
            CommandExecuter = new CommandExecuter(ref Commands);
        }
        public void RegisterCommand(PlayerCommand command) 
        { 
            if(Commands.ContainsKey(command.Name)) return;
            Commands.TryAdd(command.Name,command);
        }
        public void DeregisterCommand(string name)
        {
            Commands.TryRemove(name, out _);
        }

        public void Dispose()
        {
            CommandExecuter.Dispose();
        }
    }
}
