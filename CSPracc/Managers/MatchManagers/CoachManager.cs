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

namespace CSPracc.Managers.MatchManagers
{
    public class CoachManager : BaseManager
    {
        CSPraccPlugin Plugin;
        public CoachManager(ref CommandManager commandManager, ref CSPraccPlugin plugin) : base(ref commandManager)
        {
            Plugin = plugin;
            Commands.Add(MATCH_COMMAND.COACH, new DataModules.PlayerCommand(MATCH_COMMAND.COACH,"Set yourself as coach",AddCoachCommandHandler,null));
            Commands.Add(MATCH_COMMAND.STOP, new DataModules.PlayerCommand(MATCH_COMMAND.COACH, "Go to player slot", StopCoachCommandHandler, null));
            plugin.RegisterEventHandler<EventPlayerSpawn>(OnPlayerSpawnHandler, HookMode.Post);
            plugin.RegisterEventHandler<EventRoundFreezeEnd>(OnFreezeTimeEnd, HookMode.Post);
        }

        public new void Dispose()
        {
            GameEventHandler<EventPlayerSpawn> playerspawn = OnPlayerSpawnHandler;
            Plugin.DeregisterEventHandler("player_spawn", playerspawn, true);
            GameEventHandler<EventRoundFreezeEnd> freezetimeEnd = OnFreezeTimeEnd;
            Plugin.DeregisterEventHandler("round_freeze_end", freezetimeEnd, true);
            base.Dispose();
        }

        public static List<ulong> ListCoaches { get; set; } = new List<ulong>();

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
                        CSPraccPlugin.Instance!.AddTimer(0.5f, () => player!.PlayerPawn!.Value!.CommitSuicide(false, true));
                        Server.ExecuteCommand("mp_suicide_penalty 1");
                    }
                }

            }
            return HookResult.Changed;
        }
        public bool AddCoachCommandHandler(CCSPlayerController playerController, List<string> args)
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

        public bool StopCoachCommandHandler(CCSPlayerController playerController, List<string> args)
        {
            if (ListCoaches.Remove(playerController.SteamID))
            {
                playerController.PrintToCenter("You`re no longer a coach.");
                return true;
            }
            return false;
        }

        public HookResult OnFreezeTimeEnd(EventRoundFreezeEnd @event, GameEventInfo info)
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
