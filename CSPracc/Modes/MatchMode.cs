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
    public  class MatchMode : BaseMode
    {       
        private static DataModules.Enums.match_state state = DataModules.Enums.match_state.warmup;

        private static bool ReadyTeamCT = false;
        private static bool ReadyTeamT = false;

        public static CCSPlayerController? CoachTeam1 { get; set; }
        public static CCSPlayerController? CoachTeam2 { get; set; }

        private static BaseEventHandler EventHandler {  get; set; } = null;

        public static void Pause()
        {
            if (state == DataModules.Enums.match_state.warmup) { return; }
            Methods.MsgToServer("Match paused. Waiting for both teams to .unpause");
            Server.ExecuteCommand(DataModules.Constants.COMMANDS.PAUSE_MATCH);
        }

        public static void Ready(CCSPlayerController player)
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

        public static void UnReady(CCSPlayerController player)
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

        public static void Unpause(CCSPlayerController player)
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


        public static void Restart(CCSPlayerController player)
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

        public static void Rewarmup(CCSPlayerController? player)
        {
            if (player == null) return;
            if (!player.PlayerPawn.IsValid) return;
            if (!player.IsAdmin())
            {
                player.PrintToCenter("Only admins can execute this command!");
                return;
            }
            Methods.MsgToServer("Starting Warmup.");
            Server.ExecuteCommand("exec CSPRACC\\5on5_warmup.cfg");
            Server.ExecuteCommand(DataModules.Constants.COMMANDS.START_WARMUP);
        }

        public static void Start(CCSPlayerController? player)
        {
            if(player == null) { return; }
            if(!player.IsValid) { return; }
            if(!player.IsAdmin()) { player.PrintToCenter("Only admins can execute this command!"); return; }
            internalStart();
        }

        private static void internalStart()
        {
            ReadyTeamCT = false;
            ReadyTeamT = false;
            if (state == DataModules.Enums.match_state.live) { return; }
            state = DataModules.Enums.match_state.live;
            Server.ExecuteCommand("exec CSPRACC\\5on5.cfg");
            Methods.MsgToServer("Starting Match!");
            Server.ExecuteCommand("bot_kick");
            Server.ExecuteCommand("mp_warmup_end 1");
            if (DemoManager.DemoManagerSettings.RecordingMode == Enums.RecordingMode.Automatic)
            {
                DemoManager.StartRecording();
            }
        }

        public static void StopCoach(CCSPlayerController playerController)
        {
            if (playerController == null) return;

            if (playerController.PlayerPawn.Value.TeamNum == (byte)CsTeam.Terrorist)
            {
                if(CoachTeam1 !=  null)
                {
                    if(CoachTeam1.PlayerPawn.Handle == playerController.PlayerPawn.Handle)
                    {
                        CoachTeam1.PrintToCenter("Your no longer the coach now.");
                        CoachTeam1.Clan = "";
                        CoachTeam1 = null;
                        
                    }
                }
            }
            if (playerController.PlayerPawn.Value.TeamNum == (byte)CsTeam.CounterTerrorist)
            {
                if (CoachTeam2 != null)
                {
                    if (CoachTeam2.PlayerPawn.Handle == playerController.PlayerPawn.Handle)
                    {
                        CoachTeam2.PrintToCenter("Your no longer the coach now.");
                        CoachTeam2.Clan = "";
                        CoachTeam2 = null;
                    }
                }
            }
        }


        public static void AddCoach(CCSPlayerController playerController)
        {
            if (playerController == null) return;
            if (!playerController.PlayerPawn.IsValid) return;
            if (playerController.PlayerPawn.Value.TeamNum == (byte)CsTeam.Terrorist)
            {
                if (CoachTeam1 == null)
                {
                    CoachTeam1 = playerController;
                    CoachTeam1.Clan = "COACH";
                    CoachTeam1.PrintToCenter("Your the T coach now.");
                }
                else
                {
                    playerController.PrintToCenter("There is already someone in coach slot! Use !stopcoach to leave coaching slot");
                }
            }
            if (playerController.PlayerPawn.Value.TeamNum == (byte)CsTeam.CounterTerrorist)
            {
                if (CoachTeam2 == null)
                {
                    CoachTeam2 = playerController;
                    CoachTeam2.Clan = "COACH";
                    CoachTeam2.PrintToCenter("Your the CT coach now.");
                }
                else
                {
                    playerController.PrintToCenter("There is already someone in coach slot! Use !stopcoach to leave coaching slot");
                }
            }

        }

        public static void RestoreBackup(CCSPlayerController player)
        {
            if(player == null) { return; }
            if(!player.IsValid) { return; }
            if(!player.IsAdmin()) { player.PrintToCenter("Only admins can execute this command!"); return; }
            Pause();
            Methods.MsgToServer("Admin is using round restore manager.");
            RoundRestoreManager.OpenBackupMenu(player);
        }

        public static void ForceUnpause(CCSPlayerController player)
        {
            if (player == null) { return; }
            if (!player.IsValid) { return; }
            if (!player.IsAdmin()) { player.PrintToCenter("Only admins can execute this command!"); return; }
            ReadyTeamCT = true;
            ReadyTeamT = true;
        }

        public override void ConfigureEnvironment()
        {
            DataModules.consts.Methods.MsgToServer("Starting match");
            Server.ExecuteCommand("exec CSPRACC\\undo_pracc.cfg");
            Server.ExecuteCommand("exec CSPRACC\\5on5_warmup.cfg");
            EventHandler?.Dispose();
            EventHandler = new MatchEventHandler(CSPraccPlugin.Instance!, new MatchCommandHandler());
            state = Enums.match_state.warmup;
        }

        public static HookResult OnPlayerSpawnHandler(EventPlayerSpawn @event,GameEventInfo info)
        {


            if (CoachTeam1 != null)
            {
                Logging.LogMessage($"CoachT1 {@event.Userid.UserId} - {CoachTeam1!.UserId}");
                if (@event.Userid.UserId == MatchMode.CoachTeam1!.UserId)
                {
                    Logging.LogMessage("T Coach commit suicide now!");
                    CoachTeam1!.InGameMoneyServices!.Account = 0;
            Server.ExecuteCommand(DataModules.Constants.COMMANDS.UNPAUSE_MATCH);
        }

        public override void ConfigureEnvironment()
        {
            DataModules.Constants.Methods.MsgToServer("Starting match");
            Server.ExecuteCommand("exec CSPRACC\\undo_pracc.cfg");
            Server.ExecuteCommand("exec CSPRACC\\5on5_warmup.cfg");
            EventHandler?.Dispose();
            EventHandler = new MatchEventHandler(CSPraccPlugin.Instance!, new MatchCommandHandler());
            state = Enums.match_state.warmup;
        }

        public static HookResult OnPlayerSpawnHandler(EventPlayerSpawn @event,GameEventInfo info)
        {
            if (CoachTeam1 != null)
            {
                Logging.LogMessage($"CoachT1 {@event.Userid.UserId} - {CoachTeam1!.UserId}");
                if (@event.Userid.UserId == MatchMode.CoachTeam1!.UserId)
                {
                    Logging.LogMessage("T Coach commit suicide now!");
                    CoachTeam1!.InGameMoneyServices!.Account = 0;
                    Server.ExecuteCommand("mp_suicide_penalty 0");
                    CSPraccPlugin.Instance!.AddTimer(0.2f, () => CoachTeam1!.PlayerPawn.Value.CommitSuicide(false, true));
                    Server.ExecuteCommand("mp_suicide_penalty 1");

                }
            }
            if (MatchMode.CoachTeam2 != null)
            {
                Logging.LogMessage($"CoachT2 {@event.Userid.UserId} - {CoachTeam2!.UserId}");
                if (@event.Userid.UserId == MatchMode.CoachTeam2!.UserId)
                {
                    Logging.LogMessage("CT Coach commit suicide now!");
                    CoachTeam2!.InGameMoneyServices!.Account = 0;
                    Server.ExecuteCommand("mp_suicide_penalty 0");
                    CSPraccPlugin.Instance!.AddTimer(0.2f, () => CoachTeam2!.PlayerPawn.Value.CommitSuicide(false, true));
                    Server.ExecuteCommand("mp_suicide_penalty 1");
                }
            }
            return HookResult.Handled;
                    Server.ExecuteCommand("mp_suicide_penalty 0");
                    CSPraccPlugin.Instance!.AddTimer(0.2f, () => CoachTeam1!.PlayerPawn.Value.CommitSuicide(false, true));
                    Server.ExecuteCommand("mp_suicide_penalty 1");

                }
            }
            if (MatchMode.CoachTeam2 != null)
            {
                Logging.LogMessage($"CoachT2 {@event.Userid.UserId} - {CoachTeam2!.UserId}");
                if (@event.Userid.UserId == MatchMode.CoachTeam2!.UserId)
                {
                    Logging.LogMessage("CT Coach commit suicide now!");
                    CoachTeam2!.InGameMoneyServices!.Account = 0;
                    Server.ExecuteCommand("mp_suicide_penalty 0");
                    CSPraccPlugin.Instance!.AddTimer(0.2f, () => CoachTeam2!.PlayerPawn.Value.CommitSuicide(false, true));
                    Server.ExecuteCommand("mp_suicide_penalty 1");
                }
            }
            return HookResult.Handled;
        }

    }
}
