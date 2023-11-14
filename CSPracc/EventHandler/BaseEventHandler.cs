using CounterStrikeSharp.API.Core;
using CSPracc.CommandHandler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSPracc.EventHandler
{
    public class BaseEventHandler : IDisposable
    {
        protected CSPraccPlugin Plugin { get; set; }
        protected BaseCommandHandler BaseCommandHandler { get;}
        public BaseEventHandler(CSPraccPlugin plugin,BaseCommandHandler commandHandler) 
        {
            Plugin = plugin;
            plugin.RegisterEventHandler<EventPlayerChat>(OnPlayerChat, HookMode.Pre);
            BaseCommandHandler = commandHandler;
        }

        protected HookResult OnPlayerChat(EventPlayerChat @event,GameEventInfo info) 
        {
            BaseCommandHandler.PlayerChat(@event, info);
            return HookResult.Continue;
        }

        public virtual void Dispose()
        {
            BasePlugin.GameEventHandler<EventPlayerChat> playerchat = OnPlayerChat;
            Plugin.DeregisterEventHandler("player_chat", playerchat, false);
        }
    }
}
