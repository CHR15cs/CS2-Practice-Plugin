using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CounterStrikeSharp.API.Modules.Utils;
using System.Text.RegularExpressions;
using CSPracc.DataModules;
using System.IO;
using CSPracc.Managers;
using CSPracc.DataModules.Constants;
using CSPracc.EventHandler;
using CSPracc.CommandHandler;
using CSPracc.Modes;
using static CSPracc.DataModules.Enums;
using CounterStrikeSharp.API.Modules.Cvars;

namespace CSPracc
{
    public  class DryRunMode : MatchMode
    {

        public override HookResult OnPlayerSpawnHandler(EventPlayerSpawn @event, GameEventInfo info)
        {
            foreach(CCSPlayerController player in Utilities.GetPlayers())
            {
                player.InGameMoneyServices!.Account = 16000;
            }
            return base.OnPlayerSpawnHandler(@event, info); ;
        }

        protected override void internalStart()
        {
            ReadyTeamCT = false;
            ReadyTeamT = false;
            if (state == DataModules.Enums.match_state.live) { return; }
            state = DataModules.Enums.match_state.live;
            Server.ExecuteCommand("exec CSPRACC\\DryRun.cfg");
            Methods.MsgToServer("Starting DryRun!");
            Server.ExecuteCommand("bot_kick");
            Server.ExecuteCommand("mp_warmup_end 1");
        }

        public override void ConfigureEnvironment(bool hotReload = true)
        {
            if(hotReload)
            {
                DataModules.Constants.Methods.MsgToServer("Starting Dryrun Mode");
                Server.ExecuteCommand("exec CSPRACC\\undo_pracc.cfg");
                Server.ExecuteCommand("exec CSPRACC\\5on5_warmup.cfg");
            }
            EventHandler?.Dispose();
            EventHandler = new DryRunEventHandler(CSPraccPlugin.Instance!, new DryRunCommandHandler(this),this);
        }    
    }
}
