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
        Dictionary<CCSPlayerController, List<DamageInfo>> DamageStats = new Dictionary<CCSPlayerController, List<DamageInfo>>();
        public MatchEventHandler(CSPraccPlugin plugin, MatchCommandHandler mch) : base(plugin, mch)
        {
            plugin.RegisterEventHandler<EventPlayerSpawn>(MatchMode.OnPlayerSpawnHandler, hookMode: HookMode.Post);
            plugin.RegisterEventHandler<EventRoundStart>(OnRoundStart, hookMode: HookMode.Post);
            plugin.RegisterEventHandler<EventRoundEnd>(OnRoundEnd, hookMode: HookMode.Post);
            plugin.RegisterEventHandler<EventRoundFreezeEnd>(OnFreezeTimeEnd, hookMode: HookMode.Post);
            plugin.RegisterEventHandler<EventMatchEndConditions>(OnMatchEnd, hookMode: HookMode.Post);
            plugin.RegisterEventHandler<EventPlayerHurt>(OnPlayerHurt, hookMode: HookMode.Post);
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
           
            return HookResult.Continue;
        }

        public HookResult OnRoundEnd(EventRoundEnd @event, GameEventInfo info)
        {

            return HookResult.Continue;
        }

        public HookResult OnPlayerHurt(EventPlayerHurt @event, GameEventInfo info)
        {
            return HookResult.Continue;
        }

        public override void Dispose()
        {
            BasePlugin.GameEventHandler<EventPlayerSpawn> spawnEventHandler = MatchMode.OnPlayerSpawnHandler;
            Plugin.DeregisterEventHandler("player_spawn", spawnEventHandler, true);
            BasePlugin.GameEventHandler<EventPlayerHurt> playerHurtEventHandler = OnPlayerHurt;
            Plugin.DeregisterEventHandler("player_hurt", playerHurtEventHandler, true);
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
