using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Entities;
using CounterStrikeSharp.API.Modules.Utils;
using CSPracc.DataModules;
using CSPracc.DataModules.Constants;
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
            return AdminManager.PlayerHasPermissions(playerController, AdminFlags.Standard);
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
