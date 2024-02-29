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
    public class PlayerHurtManager : BaseManager
    {
        CSPraccPlugin Plugin;
        public PlayerHurtManager(ref CSPraccPlugin plugin, ref CommandManager commandManager) : base(ref commandManager) 
        {
            Plugin = plugin;
            Plugin.RegisterEventHandler<EventPlayerHurt>(OnPlayerHurt, HookMode.Post);
        }
        public new void Dispose()
        {
            GameEventHandler<EventPlayerHurt> playerHurt = OnPlayerHurt;
            Plugin.DeregisterEventHandler("player_hurt", playerHurt, false);
        }

        private HookResult OnPlayerHurt(EventPlayerHurt @event, GameEventInfo info)
        {
            if (@event.Userid != null && @event.Attacker != null)
            {
                Methods.MsgToServer($" {ChatColors.Red}{@event.Attacker.PlayerName}{ChatColors.White} damaged {ChatColors.Blue}{@event.Userid.PlayerName}{ChatColors.White} for {ChatColors.Green}{@event.DmgHealth}{ChatColors.White}hp with {ChatColors.Green}{@event.Weapon}");
                return HookResult.Continue;
            }
            Methods.MsgToServer($"Player was damaged for {ChatColors.Green}{@event.DmgHealth}{ChatColors.White}hp with {ChatColors.Green}{@event.Weapon}");
            return HookResult.Continue;
        }
    }
}
