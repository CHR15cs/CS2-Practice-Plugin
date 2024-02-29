using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using CSPracc.DataModules.Constants;
using CSPracc.Managers.BaseManagers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CounterStrikeSharp.API.Core.BasePlugin;

namespace CSPracc.Managers.PracticeManagers
{
    public class PlayerBlindManager : BaseManager
    {
        CSPraccPlugin Plugin;
        ProjectileManager ProjectileManager;
        public PlayerBlindManager(ref CSPraccPlugin plugin,ref ProjectileManager projectileManager, ref CommandManager commandManager) : base(ref commandManager)
        {
            Plugin = plugin;
            ProjectileManager = projectileManager;
            Plugin.RegisterEventHandler<EventPlayerBlind>(OnPlayerBlind, HookMode.Post);
        }
        public new void Dispose()
        {
            GameEventHandler<EventPlayerBlind> playerblind = OnPlayerBlind;
            Plugin.DeregisterEventHandler("player_blind", playerblind, false);
        }
        private HookResult OnPlayerBlind(EventPlayerBlind @event, GameEventInfo info)
        {
            Methods.MsgToServer($" {ChatColors.Red}{@event.Attacker.PlayerName}{ChatColors.White} flashed {ChatColors.Blue}{@event.Userid.PlayerName}{ChatColors.White} for {ChatColors.Green}{@event.BlindDuration.ToString("0.00")}s");
            if (ProjectileManager.NoFlashList.Contains(@event.Userid.SteamID))
            {
                @event.Userid.PlayerPawn.Value!.FlashMaxAlpha = 0.5f;

            }
            return HookResult.Continue;
        }
    }
}
