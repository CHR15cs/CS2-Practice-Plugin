using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CSPracc.DataModules.Enums;
using CSPracc.Managers.BaseManagers;
using CounterStrikeSharp.API.Modules.Utils;
using CSPracc.DataModules.Constants;
using static CounterStrikeSharp.API.Core.BasePlugin;
using CSPracc.Managers.BaseManagers.CommandManagerFolder;

namespace CSPracc.Managers.MatchManagers
{
    /// <summary>
    /// Class to handle the coach slot
    /// </summary>
    public class CoachManager : BaseManager
    {
        /// <summary>
        /// Constructor for the coach manager
        /// </summary>
        public CoachManager() : base()
        {
            Commands.Add(MATCH_COMMAND.COACH, new DataModules.PlayerCommand(MATCH_COMMAND.COACH,"Set yourself as coach",AddCoachCommandHandler,null,null));
            Commands.Add(MATCH_COMMAND.STOP, new DataModules.PlayerCommand(MATCH_COMMAND.COACH, "Go to player slot", StopCoachCommandHandler, null,null));
            CSPraccPlugin.Instance.RegisterEventHandler<EventPlayerSpawn>(OnPlayerSpawnHandler, HookMode.Post);
            CSPraccPlugin.Instance.RegisterEventHandler<EventRoundFreezeEnd>(OnFreezeTimeEnd, HookMode.Post);
        }

        /// <summary>
        /// Disposing the object
        /// </summary>
        public new void Dispose()
        {
            GameEventHandler<EventPlayerSpawn> playerspawn = OnPlayerSpawnHandler;
            CSPraccPlugin.Instance.DeregisterEventHandler("player_spawn", playerspawn, true);
            GameEventHandler<EventRoundFreezeEnd> freezetimeEnd = OnFreezeTimeEnd;
            CSPraccPlugin.Instance.DeregisterEventHandler("round_freeze_end", freezetimeEnd, true);
            base.Dispose();
        }

        private List<ulong> ListCoaches { get; set; } = new List<ulong>();

        private HookResult OnPlayerSpawnHandler(EventPlayerSpawn @event, GameEventInfo info)
        {
            if (ListCoaches != null && ListCoaches.Count > 0)
            {
                foreach (ulong id in ListCoaches)
                {
                    if (id == @event.Userid!.SteamID)
                    {
                        @event.Userid.InGameMoneyServices!.Account = 0;
                        Server.ExecuteCommand("mp_suicide_penalty 0");
                        CCSPlayerController? player = Utilities.GetPlayerFromSteamId(id);
                        if (player == null || !player.IsValid) { return HookResult.Continue; }
                        CSPraccPlugin.Instance.AddTimer(0.5f, () => player!.PlayerPawn!.Value!.CommitSuicide(false, true));
                        Server.ExecuteCommand("mp_suicide_penalty 1");
                    }
                }

            }
            return HookResult.Changed;
        }
        private bool AddCoachCommandHandler(CCSPlayerController playerController, PlayerCommandArgument args)
        {
            if (ListCoaches.Contains(playerController.SteamID))
            {
                playerController.PrintToCenter("You already are a coach.");
                return false;
            }
            ListCoaches.Add(playerController.SteamID);
            playerController.Clan = "COACH";
            playerController.PrintToCenter("You`re a coach now.");
            return true;
        }

        private bool StopCoachCommandHandler(CCSPlayerController playerController, PlayerCommandArgument args)
        {
            if (ListCoaches.Remove(playerController.SteamID))
            {
                playerController.PrintToCenter("You`re no longer a coach.");
                return true;
            }
            return false;
        }

        private HookResult OnFreezeTimeEnd(EventRoundFreezeEnd @event, GameEventInfo info)
        {
            if (ListCoaches != null && ListCoaches.Count > 0)
            {
                CSPraccPlugin.Instance!.AddTimer(2.0f, () => SwitchTeamsCoach(ListCoaches));
            }
            return HookResult.Continue;
        }

        private static void SwitchTeamsCoach(List<ulong> playerList)
        {
            if (playerList == null || playerList.Count == 0) return;


            foreach (ulong id in playerList)
            {
                CCSPlayerController? player = Utilities.GetPlayerFromSteamId(id);
                if (player == null || !player.IsValid)
                {
                    return;
                }
                CsTeam oldTeam = (CsTeam)player.TeamNum;
                player.ChangeTeam(CsTeam.Spectator);
                player.ChangeTeam(oldTeam);
            }
        }

    }
}
