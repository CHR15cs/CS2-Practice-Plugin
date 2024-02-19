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
    public class BaseMode : IDisposable
    {
        protected GuiManager GuiManager;
        protected CommandManager CommandManager;
        public CSPraccPlugin Plugin;
        MapChangeManager MapChangeManager;
        ModeSwitchManager ModeSwitchManager;
        
        public BaseMode(CSPraccPlugin plugin) 
        {
            GuiManager = new GuiManager();
            CommandManager = new CommandManager(ref plugin);
            Plugin = plugin;
            MapChangeManager = new MapChangeManager(ref CommandManager);
            ModeSwitchManager = new ModeSwitchManager(ref CommandManager, ref plugin, ref GuiManager);
        }
      
        public  virtual void ConfigureEnvironment()
        {
            DataModules.Constants.Methods.MsgToServer("Restoring default config.");
            Server.ExecuteCommand("exec CSPRACC\\undo_pracc.cfg");
            Server.ExecuteCommand("exec server.cfg");      
        }

        public virtual void Dispose()
        {
            CommandManager.Dispose();
            GuiManager.Dispose();
        }

    }


}
