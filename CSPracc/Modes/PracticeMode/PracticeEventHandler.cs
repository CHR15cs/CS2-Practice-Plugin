using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Entities;
using CounterStrikeSharp.API.Modules.Events;
using CounterStrikeSharp.API.Modules.Utils;
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
        PracticeBotManager BotManager { get; set; }

        ProjectileManager ProjectileManager { get; set; }
        ~PracticeEventHandler()
        {

        }
        PracticeCommandHandler PracticeCommandHandler { get; set; }
        public PracticeEventHandler(CSPraccPlugin plugin, PracticeCommandHandler pch,ref ProjectileManager projectileManager,ref PracticeBotManager botManager) : base(plugin,pch)
        {
            ProjectileManager = projectileManager;
            plugin.RegisterListener<Listeners.OnEntitySpawned>(entity => ProjectileManager.OnEntitySpawned(entity));          
            plugin.RegisterEventHandler<EventPlayerBlind>(OnPlayerBlind, hookMode: HookMode.Pre);
            plugin.RegisterEventHandler<EventPlayerHurt>(OnPlayerHurt, hookMode: HookMode.Post);
            plugin.RegisterEventHandler<EventPlayerSpawn>(OnPlayerSpawn, hookMode: HookMode.Post);
            plugin.RegisterEventHandler<EventSmokegrenadeDetonate>(ProjectileManager.OnSmokeDetonate,hookMode: HookMode.Post);
            Plugin = plugin;
            BotManager = botManager;
            PracticeCommandHandler = pch;
        }

        public HookResult OnPlayerBlind(EventPlayerBlind @event, GameEventInfo info)
        {
            Methods.MsgToServer($" {ChatColors.Red}{@event.Attacker.PlayerName}{ChatColors.White} flashed {ChatColors.Blue}{@event.Userid.PlayerName}{ChatColors.White} for {ChatColors.Green}{@event.BlindDuration.ToString("0.00")}s");
            if (ProjectileManager.NoFlashList.Contains(@event.Userid.SteamID))
            {
                @event.Userid.PlayerPawn.Value!.FlashMaxAlpha = 0.5f;

            }
            return HookResult.Continue;
        }

        public HookResult OnPlayerHurt(EventPlayerHurt @event, GameEventInfo info)
        {
            Methods.MsgToServer($" {ChatColors.Red}{@event.Attacker.PlayerName}{ChatColors.White} damaged {ChatColors.Blue}{@event.Userid.PlayerName}{ChatColors.White} for {ChatColors.Green}{@event.DmgHealth}{ChatColors.White}hp with {ChatColors.Green}{@event.Weapon}");
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
            Plugin.DeregisterEventHandler("player_blind", playerblind, false);

            GameEventHandler<EventPlayerHurt> playerhurt = OnPlayerHurt;
            Plugin.DeregisterEventHandler("player_hurt", playerhurt, true);

            GameEventHandler<EventPlayerSpawn> playerspawn = OnPlayerSpawn;
            Plugin.DeregisterEventHandler("player_spawn", playerhurt, true);

            GameEventHandler<EventSmokegrenadeDetonate> smokegrenadedetonate = ProjectileManager.OnSmokeDetonate;
            Plugin.DeregisterEventHandler("smokegrenade_detonate", smokegrenadedetonate, true);

            Plugin.RemoveListener("OnEntitySpawned", ProjectileManager.OnEntitySpawned);

            base.Dispose();
        }
    }
}
