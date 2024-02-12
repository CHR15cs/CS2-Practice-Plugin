using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Events;
using CSPracc.CommandHandler;
using CSPracc.DataModules;
using CSPracc.DataModules.Constants;
using CSPracc.Managers;
using CSPracc.Modes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using static CounterStrikeSharp.API.Core.BasePlugin;

namespace CSPracc.EventHandler
{
    public class PrefireEventHandler : BaseEventHandler
    {
        PracticeBotManager BotManager { get; set; }

        PrefireMode PrefireMode { get; set; }
        ~PrefireEventHandler()
        {

        }
        PrefireCommandHandler RetakeCommandHandler { get; set; }
        public PrefireEventHandler(CSPraccPlugin plugin, PrefireCommandHandler rch, PrefireMode mode) : base(plugin,rch)
        {
            //PrefireMode = mode;
            ////plugin.RegisterListener<Listeners.OnEntitySpawned>(entity => ProjectileManager.Instance.OnEntitySpawned(entity));          
            //plugin.RegisterEventHandler<EventPlayerSpawn>(mode.OnPlayerSpawn, hookMode: HookMode.Post);
            //plugin.RegisterEventHandler<EventRoundStart>(mode.OnRoundStart, hookMode: HookMode.Post);
            //plugin.RegisterEventHandler<EventPlayerHurt>(mode.OnPlayerHurt, hookMode: HookMode.Pre);
            //plugin.RegisterEventHandler<EventPlayerDeath>(mode.OnPlayerDeath, hookMode: HookMode.Post);
            
            //Plugin = plugin;
            //BotManager = new PracticeBotManager();
            //RetakeCommandHandler = rch;
        }

        public override void Dispose()
        {

            GameEventHandler<EventRoundStart> roundstart = PrefireMode.OnRoundStart;
            Plugin.DeregisterEventHandler("round_start", roundstart, true);

            GameEventHandler<EventPlayerSpawn> playerspawn = PrefireMode.OnPlayerSpawn;
            Plugin.DeregisterEventHandler("player_spawn", playerspawn, true);

            GameEventHandler<EventPlayerHurt> playerhurt = PrefireMode.OnPlayerHurt;
            Plugin.DeregisterEventHandler("player_hurt", playerhurt, false);

            GameEventHandler<EventPlayerDeath> playerdeath = PrefireMode.OnPlayerDeath;
            Plugin.DeregisterEventHandler("player_death", playerdeath, false);

            base.Dispose();
        }
    }
}
