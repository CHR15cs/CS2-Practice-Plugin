using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSPracc
{
    public class Match
    {
        
        private DataModules.enums.PluginMode currentMode = DataModules.enums.PluginMode.Standard;
        private DataModules.enums.match_state state = DataModules.enums.match_state.warmup;
        public DataModules.enums.PluginMode CurrentMode => currentMode;
        public Match() 
        {
            SwitchTo(DataModules.enums.PluginMode.Standard, true);
            Server.ExecuteCommand(DataModules.consts.COMMANDS.START_WARMUP);
        }
        public void Pause()
        {
            if (state == DataModules.enums.match_state.warmup || currentMode != DataModules.enums.PluginMode.Match) { return; }
            Server.ExecuteCommand(DataModules.consts.COMMANDS.PAUSE_MATCH);
        }

        public void Unpause()
        {
            if (state == DataModules.enums.match_state.warmup || currentMode != DataModules.enums.PluginMode.Match) { return; }
            Server.ExecuteCommand(DataModules.consts.COMMANDS.UNPAUSE_MATCH);
        }


        public void Restart()
        {
            if (state == DataModules.enums.match_state.warmup || currentMode != DataModules.enums.PluginMode.Match) { return; }
            Server.ExecuteCommand(DataModules.consts.COMMANDS.RESTART_GAME);
        }

        public void Rewarmup()
        {
            if (state == DataModules.enums.match_state.warmup || currentMode != DataModules.enums.PluginMode.Match) { return; }
            Server.ExecuteCommand(DataModules.consts.COMMANDS.START_WARMUP);
        }

        public void Start()
        {
            if (state == DataModules.enums.match_state.live || currentMode != DataModules.enums.PluginMode.Match) { return; }
            state = DataModules.enums.match_state.live;
            Server.ExecuteCommand(DataModules.consts.COMMANDS.START_MATCH);
        }


        public void SwitchTo(DataModules.enums.PluginMode pluginMode, bool force = false)
        {
            if(pluginMode == currentMode) { return; }
            switch (pluginMode)
            {
                case DataModules.enums.PluginMode.Standard:
                    DataModules.consts.Methods.MsgToServer("Restoring default config.");
                    Server.ExecuteCommand("exec CSPRACC\\undo_pracc.cfg");
                    Server.ExecuteCommand("exec server.cfg");
                    currentMode = pluginMode;
                    break;
                case DataModules.enums.PluginMode.Pracc:
                    DataModules.consts.Methods.MsgToServer("Starting practice mode.");
                    Server.ExecuteCommand("exec CSPRACC\\pracc.cfg");
                    currentMode = pluginMode;
                    break;
                case DataModules.enums.PluginMode.Match:
                    DataModules.consts.Methods.MsgToServer("Starting match");
                    Server.ExecuteCommand("exec CSPRACC\\undo_pracc.cfg");
                    Server.ExecuteCommand("exec gamemode_competitive.cfg");
                    currentMode = pluginMode;
                    break;
            }
        }
    }
}
