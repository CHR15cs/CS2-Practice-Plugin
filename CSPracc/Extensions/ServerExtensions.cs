using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSPracc.Extensions
{
    public static class ServerExtensions
    {
        public static void ChatMessage(this Server server, string message)
        {
            Utils.ServerMessage($" {message}");
        }
    }
}
