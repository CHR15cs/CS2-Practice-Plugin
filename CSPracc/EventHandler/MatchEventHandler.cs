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
        Dictionary<ulong, DamageInfo> DamageStats = new Dictionary<ulong, DamageInfo>();
        public MatchEventHandler(CSPraccPlugin plugin, MatchCommandHandler mch) : base(plugin, mch)
        {
            plugin.RegisterEventHandler<EventPlayerSpawn>(MatchMode.OnPlayerSpawnHandler, hookMode: HookMode.Post);
            plugin.RegisterEventHandler<EventRoundStart>(OnRoundStart, hookMode: HookMode.Post);
            plugin.RegisterEventHandler<EventRoundEnd>(OnRoundEnd, hookMode: HookMode.Post);
            plugin.RegisterEventHandler<EventRoundFreezeEnd>(MatchMode.OnFreezeTimeEnd, hookMode: HookMode.Post);
            plugin.RegisterEventHandler<EventMatchEndConditions>(OnMatchEnd, hookMode: HookMode.Post);
            plugin.RegisterEventHandler<EventPlayerHurt>(OnPlayerHurt, hookMode: HookMode.Pre);
            Plugin = plugin;
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
            DamageStats = new Dictionary<ulong, DamageInfo>();
           
            return HookResult.Continue;
        }

        public HookResult OnRoundEnd(EventRoundEnd @event, GameEventInfo info)
        {
            //Print Dmg Info
            List<CCSPlayerController> players = Utilities.GetPlayers();
            List<CCSPlayerController> teamCT = players.Where(x => x.GetCsTeam() == CsTeam.CounterTerrorist).ToList();
            List<CCSPlayerController> teamT = players.Where(x => x.GetCsTeam() == CsTeam.Terrorist).ToList();
            foreach (CCSPlayerController player in teamCT)
            {
                foreach (CCSPlayerController enemy in teamT)
                {
                    if (!DamageStats.ContainsKey(player.SteamID))
                    {
                        player.PrintToChat($" {CSPracc.DataModules.Constants.Strings.ChatTag} {ChatColors.DarkBlue} To: [0/0] From  [0/0] - {enemy.PlayerName} ({enemy.PlayerPawn.Value.Health}hp) ");
                    }
                    else
                    {
                        if(!DamageStats[player.SteamID].DamageGiven.ContainsKey(enemy.SteamID))
                        {
                            player.PrintToChat($" {CSPracc.DataModules.Constants.Strings.ChatTag} {ChatColors.DarkBlue} To: [0/0] From  [0/0] - {enemy.PlayerName} ({enemy.PlayerPawn.Value.Health}hp) ");
                        }
                        else
                        {
                            int enemyhp = enemy.PlayerPawn.Value.Health > 0 ? enemy.PlayerPawn.Value.Health : 0;
                            player.PrintToChat($" {CSPracc.DataModules.Constants.Strings.ChatTag} {ChatColors.DarkBlue} To:" +
                                $" [{DamageStats[player.SteamID].DamageGiven[enemy.SteamID].DmgGiven}/{DamageStats[player.SteamID].DamageGiven[enemy.SteamID].HitsGiven}] " +
                                $" From [{DamageStats[player.SteamID].DamageGiven[enemy.SteamID].DmgTaken}/{DamageStats[player.SteamID].DamageGiven[enemy.SteamID].HitsTaken}]" +
                                $"  - {enemy.PlayerName} ({enemyhp}hp) ");                        
                        }
                        
                    }
                    
                }            
            }

            foreach (CCSPlayerController player in teamT)
            {
                foreach (CCSPlayerController enemy in teamCT)
                {
                    if (!DamageStats.ContainsKey(player.SteamID))
                    {
                        player.PrintToChat($" {CSPracc.DataModules.Constants.Strings.ChatTag} {ChatColors.DarkBlue} To: [0/0] From  [0/0] - {enemy.PlayerName} ({enemy.PlayerPawn.Value.Health}hp) ");
                    }
                    else
                    {
                        if (!DamageStats[player.SteamID].DamageGiven.ContainsKey(enemy.SteamID))
                        {
                            player.PrintToChat($" {CSPracc.DataModules.Constants.Strings.ChatTag} {ChatColors.DarkBlue} To: [0/0] From  [0/0] - {enemy.PlayerName} ({enemy.PlayerPawn.Value.Health}hp) ");
                        }
                        else
                        {
                            int enemyhp = enemy.PlayerPawn.Value.Health > 0 ? enemy.PlayerPawn.Value.Health : 0;
                            player.PrintToChat($" {CSPracc.DataModules.Constants.Strings.ChatTag} {ChatColors.DarkBlue} To:" +
                                $" [{DamageStats[player.SteamID].DamageGiven[enemy.SteamID].DmgGiven} / {DamageStats[player.SteamID].DamageGiven[enemy.SteamID].HitsGiven}] " +
                                $" From [{DamageStats[player.SteamID].DamageGiven[enemy.SteamID].DmgTaken} / {DamageStats[player.SteamID].DamageGiven[enemy.SteamID].HitsTaken}]" +
                                $"  - {enemy.PlayerName} ({enemyhp}hp) ");
                        }
                    }

                }
            }
            return HookResult.Continue;
        }

        public HookResult OnPlayerHurt(EventPlayerHurt @event, GameEventInfo info)
        {
            if(!DamageStats.TryGetValue(@event.Attacker.SteamID, out DamageInfo damageInfo)) 
            {
                if(!DamageStats.ContainsKey(@event.Attacker.SteamID))
                {
                    DamageStats.Add(@event.Attacker.SteamID, new DamageInfo(@event.Attacker));
                }
            }
            DamageStats[@event.Attacker.SteamID].AddDamage(@event.Userid.SteamID, @event.DmgHealth);
            
            if (!DamageStats.TryGetValue(@event.Userid.SteamID, out DamageInfo damageInfoTaker))
            {
                if (!DamageStats.ContainsKey(@event.Userid.SteamID))
                {
                    DamageStats.Add(@event.Userid.SteamID, new DamageInfo(@event.Userid));
                }
            }
            DamageStats[@event.Userid.SteamID].TakeDamage(@event.Attacker.SteamID, @event.DmgHealth);
            return HookResult.Continue;
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
            Plugin.DeregisterEventHandler("round_end", eventRoundEnd, true);
            BasePlugin.GameEventHandler<EventRoundFreezeEnd> eventfreezeRoundEnd = MatchMode.OnFreezeTimeEnd;
            Plugin.DeregisterEventHandler("round_freeze_end", eventfreezeRoundEnd, true);
            BasePlugin.GameEventHandler<EventMatchEndConditions> eventMatchEndConditions = OnMatchEnd;
            Plugin.DeregisterEventHandler("match_end_conditions", eventMatchEndConditions, true);
            base.Dispose();
        }
    }
}
