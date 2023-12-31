﻿using CounterStrikeSharp.API;
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
    public class PracticeEventHandler : BaseEventHandler
    {
        BotManager BotManager { get; set; }

        ~PracticeEventHandler()
        {

        }
        PracticeCommandHandler PracticeCommandHandler { get; set; }
        public PracticeEventHandler(CSPraccPlugin plugin, PracticeCommandHandler pch) : base(plugin,pch)
        {
            plugin.RegisterListener<Listeners.OnEntitySpawned>(entity => ProjectileManager.Instance.OnEntitySpawned(entity));          
            plugin.RegisterEventHandler<EventPlayerBlind>(OnPlayerBlind, hookMode: HookMode.Post);
            plugin.RegisterEventHandler<EventPlayerHurt>(OnPlayerHurt, hookMode: HookMode.Post);
            plugin.RegisterEventHandler<EventPlayerSpawn>(OnPlayerSpawn, hookMode: HookMode.Post);
            plugin.RegisterEventHandler<EventSmokegrenadeDetonate>(ProjectileManager.Instance.OnSmokeDetonate,hookMode: HookMode.Post);
            Plugin = plugin;
            BotManager = new BotManager();
            PracticeCommandHandler = pch;
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

        public HookResult OnSmokeDetonate(EventSmokegrenadeDetonate @event, GameEventInfo info)
        {
            ProjectileManager.ReferenceEquals(@event, info);
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

            GameEventHandler<EventSmokegrenadeDetonate> smokegrenadedetonate = ProjectileManager.Instance.OnSmokeDetonate;
            Plugin.DeregisterEventHandler("smokegrenade_detonate", smokegrenadedetonate, true);

            base.Dispose();
        }
    }
}
