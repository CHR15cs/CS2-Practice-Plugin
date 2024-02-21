using CounterStrikeSharp.API.Core.Attributes;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using CounterStrikeSharp.API;

namespace CSPracc.Extensions
{
    public static class BasePluginExtenstions
    {
        public static void DeregisterEventHandler<T>(this BasePlugin plugin, BasePlugin.GameEventHandler<T> handler, bool post = true) where T : GameEvent
        {
            var name = typeof(T).GetCustomAttribute<EventNameAttribute>()?.Name;
            ArgumentException.ThrowIfNullOrEmpty(name);
            plugin.DeregisterEventHandler(name, handler, post);
        }

        public static void DeregisterListener<T>(this BasePlugin plugin,T listener) where T : Delegate
        {
            var name = typeof(T).GetCustomAttribute<ListenerNameAttribute>()?.Name;
            ArgumentException.ThrowIfNullOrEmpty(name);
            plugin.RemoveListener(name, listener);
        }
    }
}

