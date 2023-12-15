using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Events;
using CSPracc.CommandHandler;
using CSPracc.DataModules;
using CSPracc.DataModules.Constants;
using CSPracc.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using static CounterStrikeSharp.API.Core.BasePlugin;

namespace CSPracc.EventHandler
{
    public class RetakeEventHandler : BaseEventHandler
    {
        BotManager BotManager { get; set; }

        ~RetakeEventHandler()
        {

        }
        RetakeCommandHandler RetakeCommandHandler { get; set; }
        public RetakeEventHandler(CSPraccPlugin plugin, RetakeCommandHandler rch) : base(plugin,rch)
        {
            plugin.RegisterListener<Listeners.OnEntitySpawned>(entity => ProjectileManager.Instance.OnEntitySpawned(entity));          
            plugin.RegisterEventHandler<EventPlayerBlind>(OnPlayerBlind, hookMode: HookMode.Post);
            plugin.RegisterEventHandler<EventPlayerHurt>(OnPlayerHurt, hookMode: HookMode.Post);
            plugin.RegisterEventHandler<EventPlayerSpawn>(OnPlayerSpawn, hookMode: HookMode.Post);
            Plugin = plugin;
            BotManager = new BotManager();
            RetakeCommandHandler = rch;
        }

        public HookResult OnPlayerBlind(EventPlayerBlind @event, GameEventInfo info)
        {
            Methods.MsgToServer($"Player {@event.Attacker.PlayerName} flashed {@event.Userid.PlayerName} for {@event.BlindDuration.ToString("0.00")}s");
            return HookResult.Continue;
        }

        public HookResult OnPlayerHurt(EventPlayerHurt @event, GameEventInfo info)
        {
            Methods.MsgToServer($"Player {@event.Attacker.PlayerName} damaged {@event.Userid.PlayerName} for {@event.DmgHealth}hp with {@event.Weapon}");
            return HookResult.Continue;
        }

        public HookResult OnPlayerSpawn(EventPlayerSpawn @event, GameEventInfo info)
        {
            BotManager!.OnPlayerSpawn(@event, info);
            return HookResult.Continue;
        }

        public override void Dispose()
        {            
            GameEventHandler<EventPlayerBlind> playerblind = OnPlayerBlind;
            Plugin.DeregisterEventHandler("player_blind", playerblind, true);

            GameEventHandler<EventPlayerHurt> playerhurt = OnPlayerHurt;
            Plugin.DeregisterEventHandler("player_hurt", playerhurt, true);

            GameEventHandler<EventPlayerSpawn> playerspawn = OnPlayerSpawn;
            Plugin.DeregisterEventHandler("player_spawn", playerhurt, true);

            base.Dispose();
        }
    }
}
