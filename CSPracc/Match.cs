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

namespace CSPracc
{
    public class Match
    {
        
        private DataModules.enums.PluginMode currentMode = DataModules.enums.PluginMode.Standard;
        private DataModules.enums.match_state state = DataModules.enums.match_state.warmup;
        public DataModules.enums.PluginMode CurrentMode => currentMode;

        public CCSPlayerController CoachTeam1 { get; set; }
        public CCSPlayerController CoachTeam2 { get; set; }

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

        public void Rewarmup(CCSPlayerController? player)
        {
            if (player == null) return;
            if (!player.PlayerPawn.IsValid) return;
            if (!player.IsAdmin())
            {
                player.PrintToCenter("Only admins can execute this command!");
                return;
            }
            if (state == DataModules.enums.match_state.warmup || currentMode != DataModules.enums.PluginMode.Match) { return; }
            Server.ExecuteCommand(DataModules.consts.COMMANDS.START_WARMUP);
        }

        public void Start(CCSPlayerController? player)
        {
            if(player == null) { return; }
            if(!player.IsValid) { return; }
            if(!player.IsAdmin()) { player.PrintToCenter("Only admins can execute this command!"); return; }

            if (state == DataModules.enums.match_state.live || currentMode != DataModules.enums.PluginMode.Match) { return; }
            state = DataModules.enums.match_state.live;
            Server.ExecuteCommand(DataModules.consts.COMMANDS.START_MATCH);
        }

        public void StopCoach(CCSPlayerController playerController)
        {
            if (CurrentMode != enums.PluginMode.Match) return;
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
            if (CurrentMode != enums.PluginMode.Match) return;
            if (playerController == null) return;
            if (!playerController.PlayerPawn.IsValid) return;
            if(playerController.PlayerPawn.Value.TeamNum == (byte)CsTeam.Terrorist)
            {
                if(CoachTeam1 == null)
                {
                    CoachTeam1 = playerController;
                    Server.ExecuteCommand($"say {CoachTeam1.Clan.Value}");
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

        public void SwitchTo(DataModules.enums.PluginMode pluginMode, bool force = false)
        {
            if(pluginMode == currentMode && !force) { return; }
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
