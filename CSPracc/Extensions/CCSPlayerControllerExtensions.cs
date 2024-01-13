using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Entities;
using CounterStrikeSharp.API.Modules.Utils;
using CSPracc.DataModules;
using CSPracc.DataModules.Constants;
using CSPracc.Extensions;
using CSPracc.Managers;
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
            if(!CSPraccPlugin.Instance!.Config!.AdminRequirement)
            {
                return true;
            }
            return AdminManager.PlayerHasPermissions(playerController, AdminFlags.Standard);
        }

        public static CsTeam GetCsTeam(this CCSPlayerController playerController)
        {
            if (playerController == null) { return CsTeam.None; }
            if (!playerController.IsValid) { return CsTeam.None; }
            return (CsTeam)playerController.TeamNum;
        }

        public static Position? GetCurrentPosition(this CCSPlayerController playerController)
        {
            if (playerController == null) { return null; }
            if (!playerController.IsValid) { return null; }
            return new Position(playerController.PlayerPawn.Value!.CBodyComponent!.SceneNode!.AbsOrigin, playerController.PlayerPawn.Value.EyeAngles);
        }

        public static JsonSpawnPoint? GetCurrentPositionAsJsonSpawnPoint(this CCSPlayerController playerController)
        {
            if (playerController == null) { return null; }
            if (!playerController.IsValid) { return null; }
            return new JsonSpawnPoint(playerController.PlayerPawn.Value!.CBodyComponent!.SceneNode!.AbsOrigin.ToVector3(), playerController.PlayerPawn.Value!.EyeAngles.ToVector3(),"");
        }

        public static void TeleportToJsonSpawnPoint(this CCSPlayerController playerController,JsonSpawnPoint? jsonSpawnPoint)
        {
            if (playerController == null) { return; }
            if (!playerController.IsValid) { return; }
            if (jsonSpawnPoint == null)
            {
                return;
            }
            playerController.PlayerPawn.Value!.Teleport(jsonSpawnPoint.Position.ToCSVector(), jsonSpawnPoint.QAngle.ToCSQAngle(), new Vector(0, 0, 0));
        }

        public static void TeleportToPosition(this CCSPlayerController playerController, Position? position)
        {
            if (playerController == null) { return; }
            if (!playerController.IsValid) { return; }
            if (position == null)
            {
                return;
            }
            playerController.PlayerPawn.Value!.Teleport(position.PlayerPosition,position.PlayerAngle, new Vector(0, 0, 0));
        }

        public static void HtmlMessage(this  CCSPlayerController playerController,string message,int timetodisplay = 5)
        {
            if(playerController == null) { return; }
            if(!playerController.IsValid) { return; }
            if(message == null) { return; }
            HtmlMessage htmlMessage = new HtmlMessage(message, timetodisplay);
            GuiManager.Instance!.ShowHtmlMessage(htmlMessage, playerController);
        }
    }
}
