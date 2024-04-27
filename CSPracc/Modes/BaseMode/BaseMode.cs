using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSPracc.EventHandler;
using CSPracc.CommandHandler;
using CSPracc.Managers;
using CSPracc.DataModules;
using CSPracc.Managers.BaseManagers;

namespace CSPracc.Modes
{
    /// <summary>
    /// Base class for all modes
    /// </summary>
    public class BaseMode : IDisposable
    {
        MapChangeManager MapChangeManager;
        ModeSwitchManager ModeSwitchManager;       
        /// <summary>
        /// Constuctor for the base mode
        /// </summary>
        public BaseMode() 
        {
            MapChangeManager = new MapChangeManager();
            ModeSwitchManager = new ModeSwitchManager();
        }     
        /// <summary>
        /// Configure the environment
        /// </summary>
        public  virtual void ConfigureEnvironment()
        {
            DataModules.Constants.Methods.MsgToServer("Restoring default config.");
            Server.ExecuteCommand("exec CSPRACC\\undo_pracc.cfg");
            Server.ExecuteCommand("exec server.cfg");      
        }
        /// <summary>
        /// Dispose the base mode
        /// </summary>
        public virtual void Dispose()
        {
            CommandManager.Instance.Dispose();
            GuiManager.Instance.Dispose();
        }
    }
}
