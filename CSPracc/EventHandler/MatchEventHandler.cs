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

        public MatchEventHandler(CSPraccPlugin plugin, MatchCommandHandler mch) : base(plugin, mch)
        {
            plugin.RegisterEventHandler<EventPlayerSpawn>(OnPlayerSpawn, hookMode: HookMode.Post);
            plugin.RegisterEventHandler<EventRoundFreezeEnd>(OnFreezeTimeEnd, hookMode: HookMode.Post);
            plugin.RegisterEventHandler<EventMatchEndConditions>(OnMatchEnd, hookMode: HookMode.Post);
        }

        public HookResult OnPlayerSpawn(EventPlayerSpawn @event, GameEventInfo info)
        {
            if (Match.CoachTeam1 != null)
            {
                Logging.LogMessage($"CoachT1 {@event.Userid.UserId} - {Match.CoachTeam1!.UserId}");
                if (@event.Userid.UserId == Match.CoachTeam1!.UserId)
                {
                    Logging.LogMessage("T Coach commit suicide now!");
                    Match.CoachTeam1!.InGameMoneyServices!.Account = 0;
                    Server.ExecuteCommand("mp_suicide_penalty 0");
                    CSPraccPlugin.Instance!.AddTimer(0.2f, () => Match.CoachTeam1!.PlayerPawn.Value.CommitSuicide(false, true));
                    Server.ExecuteCommand("mp_suicide_penalty 1");

                }
            }
            if (Match.CoachTeam2 != null)
            {
                Logging.LogMessage($"CoachT2 {@event.Userid.UserId} - {Match.CoachTeam2!.UserId}");
                if (@event.Userid.UserId == Match.CoachTeam2!.UserId)
                {
                    Logging.LogMessage("CT Coach commit suicide now!");
                    Match.CoachTeam2!.InGameMoneyServices!.Account = 0;
                    Server.ExecuteCommand("mp_suicide_penalty 0");
                    CSPraccPlugin.Instance!.AddTimer(0.2f, () => Match.CoachTeam2!.PlayerPawn.Value.CommitSuicide(false, true));
                    Server.ExecuteCommand("mp_suicide_penalty 1");
                }
            }
            return HookResult.Continue;
        }

        public HookResult OnFreezeTimeEnd(EventRoundFreezeEnd @event,GameEventInfo info)
        {
            if (Match.CoachTeam1 != null)
            {
                CSPraccPlugin.Instance!.AddTimer(2.0f, () => SwitchTeamsCoach(Match.CoachTeam1));
            }
            if (Match.CoachTeam2 != null)
            {
                CSPraccPlugin.Instance!.AddTimer(2.0f, () => SwitchTeamsCoach(Match.CoachTeam2));
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

        public override void Dispose()
        {
            BasePlugin.GameEventHandler<EventPlayerSpawn> spawnEventHandler = OnPlayerSpawn;
            Plugin.DeregisterEventHandler("player_spawn", spawnEventHandler, true);
            BasePlugin.GameEventHandler<EventRoundFreezeEnd> eventfreezeRoundEnd = OnFreezeTimeEnd;
            Plugin.DeregisterEventHandler("round_freeze_end", eventfreezeRoundEnd, true);
            BasePlugin.GameEventHandler<EventMatchEndConditions> eventMatchEndConditions = OnMatchEnd;
            Plugin.DeregisterEventHandler("match_end_conditions", eventMatchEndConditions, true);
            base.Dispose();
        }
    }
}
