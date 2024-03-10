using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using static CounterStrikeSharp.API.Core.BasePlugin;
using CSPracc.CommandHandler;
using CSPracc.Extensions;
using CSPracc.DataModules.Constants;
using CSPracc.Managers;

namespace CSPracc.EventHandler
{
    public class PracticeEventHandler : BaseEventHandler
    {
        PracticeBotManager BotManager { get; set; }

        ProjectileManager ProjectileManager { get; set; }

        Listeners.OnEntitySpawned onESpawn;
        ~PracticeEventHandler()
        {

        }
        PracticeCommandHandler PracticeCommandHandler { get; set; }
        public PracticeEventHandler(CSPraccPlugin plugin, PracticeCommandHandler pch,ref ProjectileManager projectileManager,ref PracticeBotManager botManager) : base(plugin,pch)
        {
            ProjectileManager = projectileManager;
            onESpawn = new Listeners.OnEntitySpawned(entity => ProjectileManager.OnEntitySpawned(entity));
            //plugin.RegisterListener<Listeners.OnEntitySpawned>(entity => ProjectileManager.OnEntitySpawned(entity));
            plugin.RegisterListener<Listeners.OnEntitySpawned>(onESpawn);   
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

        public override void Dispose()
        {            
            GameEventHandler<EventPlayerBlind> playerblind = OnPlayerBlind;
            Plugin.DeregisterEventHandler("player_blind", playerblind, false);

            GameEventHandler<EventPlayerHurt> playerhurt = OnPlayerHurt;
            Plugin.DeregisterEventHandler("player_hurt", playerhurt, true);

            GameEventHandler<EventPlayerSpawn> playerspawn = OnPlayerSpawn;
            Plugin.DeregisterEventHandler("player_spawn", playerspawn, true);

            GameEventHandler<EventSmokegrenadeDetonate> smokegrenadedetonate = ProjectileManager.OnSmokeDetonate;
            Plugin.DeregisterEventHandler("smokegrenade_detonate", smokegrenadedetonate, true);

            
            Plugin.DeregisterListener<Listeners.OnEntitySpawned>(onESpawn);
            base.Dispose();
        }
    }
}
