using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using CSPracc.CommandHandler;
using CSPracc.DataModules;
using CSPracc.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSPracc.EventHandler
{
    public class MatchEventHandler : BaseEventHandler
    {
        MatchMode MatchMode { get; init; }
        Dictionary<CCSPlayerController, DamageInfo> DamageStats = new Dictionary<CCSPlayerController, DamageInfo>();
        public MatchEventHandler(CSPraccPlugin plugin, MatchCommandHandler mch,MatchMode mode) : base(plugin, mch)
        {
            MatchMode = mode;
            plugin.RegisterEventHandler<EventPlayerSpawn>(MatchMode.OnPlayerSpawnHandler, hookMode: HookMode.Post);
            plugin.RegisterEventHandler<EventRoundStart>(OnRoundStart, hookMode: HookMode.Post);
            plugin.RegisterEventHandler<EventRoundEnd>(OnRoundEnd, hookMode: HookMode.Post);
            plugin.RegisterEventHandler<EventRoundFreezeEnd>(OnFreezeTimeEnd, hookMode: HookMode.Post);
            plugin.RegisterEventHandler<EventMatchEndConditions>(OnMatchEnd, hookMode: HookMode.Post);
            plugin.RegisterEventHandler<EventPlayerHurt>(OnPlayerHurt, hookMode: HookMode.Pre);
        }

        public HookResult OnFreezeTimeEnd(EventRoundFreezeEnd @event,GameEventInfo info)
        {
            if (MatchMode.CoachTeam1 != null)
            {
                CSPraccPlugin.Instance!.AddTimer(2.0f, () => SwitchTeamsCoach(MatchMode.CoachTeam1));
            }
            if (MatchMode.CoachTeam2 != null)
            {
                CSPraccPlugin.Instance!.AddTimer(2.0f, () => SwitchTeamsCoach(MatchMode.CoachTeam2));
            }
            return HookResult.Changed;
        }

        private  void SwitchTeamsCoach(CCSPlayerController playerController)
        {
            if (playerController == null)
            {
                return;
            }
            CsTeam oldTeam = (CsTeam)playerController.TeamNum;
            playerController.ChangeTeam(CsTeam.Spectator);
            playerController.ChangeTeam(oldTeam);
        }

        public HookResult OnMatchEnd(EventMatchEndConditions @event, GameEventInfo info)
        {
            if (DemoManager.DemoManagerSettings.isRecording)
            {
                DemoManager.StopRecording();
            }
            return HookResult.Continue;
        }

        public HookResult OnRoundStart(EventRoundStart @event, GameEventInfo info)
        {
            DamageStats = new Dictionary<CCSPlayerController, DamageInfo>();
           
            return HookResult.Continue;
        }

        public HookResult OnRoundEnd(EventRoundEnd @event, GameEventInfo info)
        {
            Server.PrintToChatAll("Round end");
            //Print Dmg Info
            List<CCSPlayerController> players = Utilities.GetPlayers();
            List<CCSPlayerController> teamCT = players.Where(x => x.GetCsTeam() == CsTeam.CounterTerrorist).ToList();
            List<CCSPlayerController> teamT = players.Where(x => x.GetCsTeam() == CsTeam.Terrorist).ToList();
            foreach (CCSPlayerController player in teamCT)
            {
                foreach (CCSPlayerController enemy in teamT)
                {
                    if (!DamageStats.ContainsKey(player))
                    {
                        player.PrintToChat($" {CSPracc.DataModules.Constants.Strings.ChatTag} {ChatColors.DarkBlue} To: [0/0] From  [0/0] - {enemy.PlayerName} ({enemy.PlayerPawn.Value.Health}hp) ");
                    }
                    else
                    {
                        if(!DamageStats[player].DamageGiven.ContainsKey(enemy))
                        {
                            player.PrintToChat($" {CSPracc.DataModules.Constants.Strings.ChatTag} {ChatColors.DarkBlue} To: [0/0] From  [0/0] - {enemy.PlayerName} ({enemy.PlayerPawn.Value.Health}hp) ");
                        }
                        else
                        {
                            int enemyhp = enemy.PlayerPawn.Value.Health > 0 ? enemy.PlayerPawn.Value.Health : 0;
                            player.PrintToChat($" {CSPracc.DataModules.Constants.Strings.ChatTag} {ChatColors.DarkBlue} To:" +
                                $" [{DamageStats[player].DamageGiven[enemy].DmgGiven}/{DamageStats[player].DamageGiven[enemy].HitsGiven}] " +
                                $" From [{DamageStats[player].DamageGiven[enemy].DmgTaken}/{DamageStats[player].DamageGiven[enemy].HitsTaken}]" +
                                $"  - {enemy.PlayerName} ({enemyhp}hp) ");                        
                        }
                        
                    }
                    
                }            
            }

            foreach (CCSPlayerController player in teamT)
            {
                foreach (CCSPlayerController enemy in teamCT)
                {
                    if (!DamageStats.ContainsKey(player))
                    {
                        player.PrintToChat($" {CSPracc.DataModules.Constants.Strings.ChatTag} {ChatColors.DarkBlue} To: [0/0] From  [0/0] - {enemy.PlayerName} ({enemy.PlayerPawn.Value.Health}hp) ");
                    }
                    else
                    {
                        if (!DamageStats[player].DamageGiven.ContainsKey(enemy))
                        {
                            player.PrintToChat($" {CSPracc.DataModules.Constants.Strings.ChatTag} {ChatColors.DarkBlue} To: [0/0] From  [0/0] - {enemy.PlayerName} ({enemy.PlayerPawn.Value.Health}hp) ");
                        }
                        else
                        {
                            int enemyhp = enemy.PlayerPawn.Value.Health > 0 ? enemy.PlayerPawn.Value.Health : 0;
                            player.PrintToChat($" {CSPracc.DataModules.Constants.Strings.ChatTag} {ChatColors.DarkBlue} To:" +
                                $" [{DamageStats[player].DamageGiven[enemy].DmgGiven}/{DamageStats[player].DamageGiven[enemy].HitsGiven}] " +
                                $" From [{DamageStats[player].DamageGiven[enemy].DmgTaken}/{DamageStats[player].DamageGiven[enemy].HitsTaken}]" +
                                $"  - {enemy.PlayerName} ({enemyhp}hp) ");
                        }

                    }

                }
            }
            return HookResult.Continue;
        }

        public HookResult OnPlayerHurt(EventPlayerHurt @event, GameEventInfo info)
        {
            if(!DamageStats.TryGetValue(@event.Attacker, out DamageInfo damageInfo)) 
            { 
                if(!DamageStats.ContainsKey(@event.Attacker))
                {
                    DamageStats.Add(@event.Attacker, new DamageInfo(@event.Attacker));
                }
            }
            DamageStats[@event.Attacker].AddDamage(@event.Userid, @event.DmgHealth);
            
            if (!DamageStats.TryGetValue(@event.Userid, out DamageInfo damageInfoTaker))
            {
                if (!DamageStats.ContainsKey(@event.Userid))
                {
                    DamageStats.Add(@event.Userid, new DamageInfo(@event.Userid));
                }
            }
            DamageStats[@event.Userid].TakeDamage(@event.Attacker, @event.DmgHealth);
            return HookResult.Handled;
        }

        public override void Dispose()
        {
            BasePlugin.GameEventHandler<EventPlayerSpawn> spawnEventHandler = MatchMode.OnPlayerSpawnHandler;
            Plugin.DeregisterEventHandler("player_spawn", spawnEventHandler, true);
            BasePlugin.GameEventHandler<EventPlayerHurt> playerHurtEventHandler = OnPlayerHurt;
            Plugin.DeregisterEventHandler("player_hurt", playerHurtEventHandler, false);
            BasePlugin.GameEventHandler<EventRoundStart> eventRoundStart = OnRoundStart;
            Plugin.DeregisterEventHandler("round_start", eventRoundStart, true);
            BasePlugin.GameEventHandler<EventRoundEnd> eventRoundEnd = OnRoundEnd;
            Plugin.DeregisterEventHandler("round_end", eventRoundStart, true);
            BasePlugin.GameEventHandler<EventRoundFreezeEnd> eventfreezeRoundEnd = OnFreezeTimeEnd;
            Plugin.DeregisterEventHandler("round_freeze_end", eventfreezeRoundEnd, true);
            BasePlugin.GameEventHandler<EventMatchEndConditions> eventMatchEndConditions = OnMatchEnd;
            Plugin.DeregisterEventHandler("match_end_conditions", eventMatchEndConditions, true);
            base.Dispose();
        }
    }
}
