using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSPracc
{
    public static class Extensions
    {
        public static bool IsAdmin(this CCSPlayerController playerController)
        {
            bool isAdmin = false;
            foreach (SteamID id in CSPraccPlugin.AdminList)
            {
                SteamID idPlayer = new SteamID(playerController.SteamID);
                if (idPlayer.SteamId3 == id.SteamId3)
                {
                    isAdmin = true;
                }
            }
            return isAdmin;
        }
    }
}
