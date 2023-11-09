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
using CSPracc.DataModules.consts;

namespace CSPracc
{
    public class Match
    {
        
        private DataModules.Enums.PluginMode currentMode = DataModules.Enums.PluginMode.Standard;
        private DataModules.Enums.match_state state = DataModules.Enums.match_state.warmup;
        public DataModules.Enums.PluginMode CurrentMode => currentMode;

        bool ReadyTeamCT = false;
        bool ReadyTeamT = false;

        public CCSPlayerController? CoachTeam1 { get; set; }
        public CCSPlayerController? CoachTeam2 { get; set; }

        public Match() 
        {
            SwitchTo(DataModules.Enums.PluginMode.Standard, true);
            Server.ExecuteCommand(DataModules.consts.COMMANDS.START_WARMUP);
        }
        public void Pause()
        {
            if (state == DataModules.Enums.match_state.warmup || currentMode != DataModules.Enums.PluginMode.Match) { return; }
            Methods.MsgToServer("Match paused. Waiting for both teams to .unpause");
            Server.ExecuteCommand(DataModules.consts.COMMANDS.PAUSE_MATCH);
        }

        public void Unpause(CCSPlayerController player)
        {
            if (state == DataModules.Enums.match_state.warmup || currentMode != DataModules.Enums.PluginMode.Match) { return; }
            if(player.TeamNum == (float)CsTeam.CounterTerrorist)
            {
                ReadyTeamCT = true;
                DataModules.consts.Methods.MsgToServer("CT Side is now ready!");
            }
            if (player.TeamNum == (float)CsTeam.Terrorist)
            {
                ReadyTeamT = true;
                DataModules.consts.Methods.MsgToServer("T Side is now ready!");
            }
            if(ReadyTeamCT && ReadyTeamT) 
            {
                Methods.MsgToServer("Both Teams are now ready. Unpausing match!");
                Server.ExecuteCommand(DataModules.consts.COMMANDS.UNPAUSE_MATCH);
            }
            
        }


        public void Restart(CCSPlayerController player)
        {
            if (player == null) return;
            if (!player.PlayerPawn.IsValid) return;
            if (!player.IsAdmin())
            {
                player.PrintToCenter("Only admins can execute this command!");
                return;
            }
            if (state == DataModules.Enums.match_state.warmup || currentMode != DataModules.Enums.PluginMode.Match) { return; }
            Methods.MsgToServer("Restarting game.");
            Server.ExecuteCommand(DataModules.consts.COMMANDS.RESTART_GAME);
        }

        public void Rewarmup(CCSPlayerController? player)
        {
            if (player == null) return;
            if (!player.PlayerPawn.IsValid) return;
            if (!player.IsAdmin())
            {
                player.PrintToCenter("Only admins can execute this command!");
                return;
            }
            if ( currentMode != DataModules.Enums.PluginMode.Match) { return; }
            Methods.MsgToServer("Starting Warmup.");
            Server.ExecuteCommand(DataModules.consts.COMMANDS.START_WARMUP);
        }

        public void Start(CCSPlayerController? player)
        {
            if(player == null) { return; }
            if(!player.IsValid) { return; }
            if(!player.IsAdmin()) { player.PrintToCenter("Only admins can execute this command!"); return; }

            if (state == DataModules.Enums.match_state.live || currentMode != DataModules.Enums.PluginMode.Match) { return; }
            state = DataModules.Enums.match_state.live;
            Methods.MsgToServer("Starting Match!");
            Server.ExecuteCommand(DataModules.consts.COMMANDS.START_MATCH);
        }

        public void StopCoach(CCSPlayerController playerController)
        {
            if (CurrentMode != Enums.PluginMode.Match) return;
            if (playerController == null) return;

            if (playerController.PlayerPawn.Value.TeamNum == (byte)CsTeam.Terrorist)
            {
                if(CoachTeam1 !=  null)
                {
                    if(CoachTeam1.PlayerPawn.Handle == playerController.PlayerPawn.Handle)
                    {
                        CoachTeam1.PrintToCenter("Your no longer the coach now.");
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
                        CoachTeam2 = null;
                    }
                }
            }
        }


        public void AddCoach(CCSPlayerController playerController)
        {
            if (CurrentMode != Enums.PluginMode.Match) return;
            if (playerController == null) return;
            if (!playerController.PlayerPawn.IsValid) return;
            if(playerController.PlayerPawn.Value.TeamNum == (byte)CsTeam.Terrorist)
            {
                if(CoachTeam1 == null)
                {
                    CoachTeam1 = playerController;
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
                    CoachTeam2.PrintToCenter("Your the CT coach now.");
                }
                else
                {
                    playerController.PrintToCenter("There is already someone in coach slot! Use !stopcoach to leave coaching slot");
                }
            }

        }

        public void ChangeMap(CCSPlayerController player,string mapName)
        {
            if (player == null) return;
            if (!player.PlayerPawn.IsValid) { return; }
            if (!player.IsAdmin()) { player.PrintToCenter("Only admins can execute this command!"); return; }
            if (mapName == null) return;
            if(mapName.Length == 0) return;
            if(!mapName.StartsWith("de_"))
            {
                mapName = "de_" + mapName;
            }
            Server.ExecuteCommand($"say Changing map to {mapName}");
            Server.ExecuteCommand($"changelevel {mapName}");

        }

        public void SwitchTo(DataModules.Enums.PluginMode pluginMode, bool force = false)
        {
            if(pluginMode == currentMode && !force) { return; }
            switch (pluginMode)
            {
                case DataModules.Enums.PluginMode.Standard:
                    DataModules.consts.Methods.MsgToServer("Restoring default config.");
                    Server.ExecuteCommand("exec CSPRACC\\undo_pracc.cfg");
                    Server.ExecuteCommand("exec server.cfg");
                    currentMode = pluginMode;
                    break;
                case DataModules.Enums.PluginMode.Pracc:
                    DataModules.consts.Methods.MsgToServer("Starting practice mode.");
                    Server.ExecuteCommand("exec CSPRACC\\pracc.cfg");
                    currentMode = pluginMode;
                    break;
                case DataModules.Enums.PluginMode.Match:
                    DataModules.consts.Methods.MsgToServer("Starting match");
                    Server.ExecuteCommand("exec CSPRACC\\undo_pracc.cfg");
                    Server.ExecuteCommand("exec gamemode_competitive.cfg");
                    currentMode = pluginMode;
                    break;
            }
        }

        public void RestoreBackup(CCSPlayerController player)
        {
            if(CurrentMode != Enums.PluginMode.Match) { return; }
            if(player == null) { return; }
            if(!player.IsValid) { return; }
            if(!player.IsAdmin()) { player.PrintToCenter("Only admins can execute this command!"); return; }
            Pause();
            Methods.MsgToServer("Admin is using round restore manager.");
            RoundRestoreManager.OpenBackupMenu(player);
        }

        public void ForceUnpause(CCSPlayerController player)
        {
            if (CurrentMode != Enums.PluginMode.Match) { return; }
            if (player == null) { return; }
            if (!player.IsValid) { return; }
            if (!player.IsAdmin()) { player.PrintToCenter("Only admins can execute this command!"); return; }
            ReadyTeamCT = true;
            ReadyTeamT = true;
            Server.ExecuteCommand(DataModules.consts.COMMANDS.UNPAUSE_MATCH);
        }

        public HookResult OnPlayerSpawn(EventPlayerSpawn @event, GameEventInfo info)
        {
            if(CoachTeam1 != null)
            {
                Logging.LogMessage($"CoachT1 {@event.Userid.UserId} - {CoachTeam1!.UserId}");
                if (@event.Userid.UserId == CoachTeam1!.UserId)
                {
                    Logging.LogMessage("T Coach commit suicide now!");
                    CoachTeam1!.InGameMoneyServices!.Account = 0;
                    Server.ExecuteCommand("mp_suicide_penalty 0");
                    CSPraccPlugin.Instance!.AddTimer(0.1f, () => CoachTeam1!.PlayerPawn.Value.CommitSuicide(true, true));
                    Server.ExecuteCommand("mp_suicide_penalty 1");

                }
            }
            if(CoachTeam2 != null)
            {
                Logging.LogMessage($"CoachT2 {@event.Userid.UserId} - {CoachTeam2!.UserId}");
                if (@event.Userid.UserId == CoachTeam2!.UserId)
                {
                    Logging.LogMessage("CT Coach commit suicide now!");
                    CoachTeam2!.InGameMoneyServices!.Account = 0;
                    Server.ExecuteCommand("mp_suicide_penalty 0");
                    CSPraccPlugin.Instance!.AddTimer(0.1f, () => CoachTeam2!.PlayerPawn.Value.CommitSuicide(true, true));
                    Server.ExecuteCommand("mp_suicide_penalty 1");
                }
            }
            return HookResult.Changed;
        }
    }
}
