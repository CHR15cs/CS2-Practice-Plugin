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
    public class RetakeEventHandler : BaseEventHandler
    {
        BotManager BotManager { get; set; }

        RetakeMode RetakeMode { get; set; }
        ~RetakeEventHandler()
        {

        }
        RetakeCommandHandler RetakeCommandHandler { get; set; }
        public RetakeEventHandler(CSPraccPlugin plugin, RetakeCommandHandler rch, RetakeMode mode) : base(plugin,rch)
        {
            RetakeMode = mode;
            plugin.RegisterListener<Listeners.OnEntitySpawned>(entity => ProjectileManager.Instance.OnEntitySpawned(entity));          
            plugin.RegisterEventHandler<EventPlayerSpawn>(mode.OnPlayerSpawn, hookMode: HookMode.Post);
            plugin.RegisterEventHandler<EventRoundStart>(mode.OnRoundStart, hookMode: HookMode.Post);
            plugin.RegisterEventHandler<EventRoundEnd>(mode.OnRoundEnd, hookMode: HookMode.Post);
            Plugin = plugin;
            BotManager = new BotManager();
            RetakeCommandHandler = rch;
        }

        public override void Dispose()
        {

            GameEventHandler<EventRoundStart> roundstart = RetakeMode.OnRoundStart;
            Plugin.DeregisterEventHandler("round_start", roundstart, true);

            GameEventHandler<EventPlayerSpawn> playerspawn = RetakeMode.OnPlayerSpawn;
            Plugin.DeregisterEventHandler("player_spawn", playerspawn, true);

            GameEventHandler<EventRoundEnd> roundend = RetakeMode.OnRoundEnd;
            Plugin.DeregisterEventHandler("round_end", roundend, true);

            base.Dispose();
        }
    }
}
