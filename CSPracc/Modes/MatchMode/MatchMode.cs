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
using CounterStrikeSharp.API.Modules.Entities;

namespace CSPracc
{
    public  class MatchMode : BaseMode
    {        protected  DataModules.Enums.match_state state = DataModules.Enums.match_state.warmup;

        public static List<ulong>? ListCoaches { get; set; }
        protected  bool ReadyTeamCT = false;
        protected  bool ReadyTeamT = false;

        public  CCSPlayerController? CoachTeam1 { get; set; }
        public  CCSPlayerController? CoachTeam2 { get; set; }


        public MatchMode(CSPraccPlugin plugin) : base(plugin)
        {

        }

        public override void Dispose()
        {
            EventHandler?.Dispose();
        }
        private  BaseEventHandler EventHandler {  get; set; } = null;

        public  void Pause()
        {
            if (state == DataModules.Enums.match_state.warmup) { return; }
            Methods.MsgToServer("Match paused. Waiting for both teams to .unpause");
            Server.ExecuteCommand(DataModules.Constants.COMMANDS.PAUSE_MATCH);
        }

        public  void Ready(CCSPlayerController player)
        {

            if (state != match_state.warmup){ return; }
            if (player == null ) { return; }
            if(!player.IsValid) { return; }
            switch (player.TeamNum)
            {
                case (byte)CsTeam.Terrorist:
                    {
                        if(ReadyTeamT)
                        {
                            break;
                        }
                        ReadyTeamT = true;
                        Methods.MsgToServer("T side is ready!");
                        break;
                    }
                case (byte)CsTeam.CounterTerrorist:
                    {
                        if (ReadyTeamCT)
                        {
                            break;
                        }
                        ReadyTeamCT = true;
                        Methods.MsgToServer("CT side is ready!");
                        break;
                    }
            }
            if(ReadyTeamT && ReadyTeamCT)
            {
                internalStart();
            }
        }

        public  void UnReady(CCSPlayerController player)
        {
            if (player == null) { return; }
            if (!player.IsValid) { return; }

            switch (player.TeamNum)
            {
                case (byte)CsTeam.Terrorist:
                    {
                        if (ReadyTeamT)
                        {
                            break;
                        }
                        ReadyTeamT = false;
                        Methods.MsgToServer("T side is not ready!");
                        break;
                    }
                case (byte)CsTeam.CounterTerrorist:
                    {
                        if (ReadyTeamCT)
                        {
                            break;
                        }
                        ReadyTeamCT = false;
                        Methods.MsgToServer("CT side is not ready!");
                        break;
                    }
            }
        }

        public  void Unpause(CCSPlayerController player)
        {
            if (state == DataModules.Enums.match_state.warmup) { return; }
            if(player.TeamNum == (float)CsTeam.CounterTerrorist)
            {
                ReadyTeamCT = true;
                DataModules.Constants.Methods.MsgToServer("CT Side is now ready!");
            }
            if (player.TeamNum == (float)CsTeam.Terrorist)
            {
                ReadyTeamT = true;
                DataModules.Constants.Methods.MsgToServer("T Side is now ready!");
            }
            if(ReadyTeamCT && ReadyTeamT) 
            {
                Methods.MsgToServer("Both Teams are now ready. Unpausing match!");
                Server.ExecuteCommand(DataModules.Constants.COMMANDS.UNPAUSE_MATCH);
            }
            
        }

        public  void Restart(CCSPlayerController player)
        {
            if (player == null) return;
            if (!player.PlayerPawn.IsValid) return;
            if (!player.IsAdmin())
            {
                player.PrintToCenter("Only admins can execute this command!");
                return;
            }
            if (state == DataModules.Enums.match_state.warmup ) { return; }
            Methods.MsgToServer("Restarting game.");
            Server.ExecuteCommand(DataModules.Constants.COMMANDS.RESTART_GAME);
        }

        public  void Rewarmup(CCSPlayerController? player)
        {
            if (player == null) return;
            if (!player.PlayerPawn.IsValid) return;
            if (!player.IsAdmin())
            {
                player.PrintToCenter("Only admins can execute this command!");
                return;
            }
            state = match_state.warmup;
            Methods.MsgToServer("Starting Warmup.");
            Methods.MsgToServer("Waiting for both teams to be ready.");
            Server.ExecuteCommand("exec CSPRACC\\5on5_warmup.cfg");
        }

        public  void Start(CCSPlayerController? player)
        {
            if(player == null) { return; }
            if(!player.IsValid) { return; }
            if(!player.IsAdmin()) { player.PrintToCenter("Only admins can execute this command!"); return; }
            internalStart();
        }

        protected virtual void internalStart()
        {
            ReadyTeamCT = false;
            ReadyTeamT = false;
            if (state == DataModules.Enums.match_state.live) { return; }
            state = DataModules.Enums.match_state.live;
            RoundRestoreManager.CleanupOldFiles();
            Server.ExecuteCommand("exec CSPRACC\\5on5.cfg");
            Methods.MsgToServer("Starting Match!");
            Server.ExecuteCommand("bot_kick");
            Server.ExecuteCommand("mp_warmup_end 1");
            if (DemoManager.DemoManagerSettings.RecordingMode == Enums.RecordingMode.Automatic)
            {
                DemoManager.StartRecording();
            }
        }

        public  void ForceUnpause(CCSPlayerController player)
        {
            if (player == null) { return; }
            if (!player.IsValid) { return; }
            if (!player.IsAdmin()) { player.PrintToCenter("Only admins can execute this command!"); return; }
            ReadyTeamCT = true;
            ReadyTeamT = true;
            Methods.MsgToServer("Both Teams are now ready. Unpausing match!");
            Server.ExecuteCommand(DataModules.Constants.COMMANDS.UNPAUSE_MATCH);
        }

        public override void ConfigureEnvironment()
        {
            DataModules.Constants.Methods.MsgToServer("Loading match mode.");
            Server.ExecuteCommand("exec CSPRACC\\undo_pracc.cfg");
            Server.ExecuteCommand("exec CSPRACC\\5on5_warmup.cfg");
            EventHandler?.Dispose();
            EventHandler = new MatchEventHandler(CSPraccPlugin.Instance!, new MatchCommandHandler(this),this);
            state = Enums.match_state.warmup;
        }    
    }
}
