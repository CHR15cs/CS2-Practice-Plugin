using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Entities;
using CounterStrikeSharp.API.Modules.Utils;
using CSPracc.DataModules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSPracc
{
    public static class CCSPlayerControllerExtensions
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

        public static CsTeam GetCsTeam(this CCSPlayerController playerController)
        {
            if (playerController == null) { return CsTeam.None; }
            if (!playerController.IsValid) { return CsTeam.None; }
            return (CsTeam)playerController.TeamNum;
        }

        public static Position GetCurrentPosition(this CCSPlayerController playerController)
        {
            if (playerController == null) { return null; }
            if (!playerController.IsValid) { return null; }
            return new Position(playerController.PlayerPawn.Value.CBodyComponent.SceneNode.AbsOrigin, playerController.PlayerPawn.Value.EyeAngles);
        }
    }
}
