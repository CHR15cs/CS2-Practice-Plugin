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
    /// <summary>
    /// Extension class of CCSPlayerController
    /// </summary>
    public static class CCSPlayerControllerExtensions
    {
        /// <summary>
        /// Check if player is admin
        /// </summary>
        /// <param name="playerController">player to be checked</param>
        /// <returns>True if admin</returns>
        public static bool IsAdmin(this CCSPlayerController playerController)
        {     
            if(!CSPraccPlugin.Instance!.Config!.AdminRequirement)
            {
                return true;
            }
            AdminData? adminData = AdminManager.GetPlayerAdminData(new SteamID(playerController.SteamID));
            string[] flags = new string[] { "admin" };
            if(!adminData!.DomainHasFlags("CSPracc", flags, true))
            {
                Server.PrintToChatAll("did not find flag");
            }
            else
            {
                Server.PrintToChatAll("found flag");
            }
            return AdminManager.PlayerHasPermissions(new SteamID(playerController.SteamID), AdminFlags.Standard);
        }

        /// <summary>
        /// Get the team of the player
        /// </summary>
        /// <param name="playerController">player to be checked</param>
        /// <returns>Team of the player</returns>
        public static CsTeam GetCsTeam(this CCSPlayerController playerController)
        {
            if (playerController == null) { return CsTeam.None; }
            if (!playerController.IsValid) { return CsTeam.None; }
            return (CsTeam)playerController.TeamNum;
        }

        /// <summary>
        /// Gets the current position of the player
        /// </summary>
        /// <param name="playerController">player to retrieve the position from</param>
        /// <returns>Position of the player, can be null</returns>
        public static Position? GetCurrentPosition(this CCSPlayerController playerController)
        {
            if (playerController == null) { return null; }
            if (!playerController.IsValid) { return null; }
            return new Position(playerController.PlayerPawn.Value!.CBodyComponent!.SceneNode!.AbsOrigin, playerController.PlayerPawn.Value.EyeAngles);
        }

        /// <summary>
        /// Gets the current position of the player as a JsonSpawnPoint
        /// </summary>
        /// <param name="playerController">player to get the position from</param>
        /// <returns>Position of the player as JsonSpawnPoint, can be null</returns>
        public static JsonSpawnPoint? GetCurrentPositionAsJsonSpawnPoint(this CCSPlayerController playerController)
        {
            if (playerController == null) { return null; }
            if (!playerController.IsValid) { return null; }
            return new JsonSpawnPoint(playerController.PlayerPawn.Value!.CBodyComponent!.SceneNode!.AbsOrigin.ToVector3(), playerController.PlayerPawn.Value!.EyeAngles.ToVector3(),"");
        }

        /// <summary>
        /// Teleport the player to a JsonSpawnPoint
        /// </summary>
        /// <param name="playerController">player to be teleported</param>
        /// <param name="jsonSpawnPoint">target position</param>
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

        /// <summary>
        /// Teleport the player to a position
        /// </summary>
        /// <param name="playerController">player to be teleported</param>
        /// <param name="position">target position</param>
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

        /// <summary>
        /// Show a html mesage to a player
        /// </summary>
        /// <param name="playerController">target player</param>
        /// <param name="message">message to be displayed</param>
        /// <param name="timeToDisplay">time to display the message</param>
        public static void HtmlMessage(this  CCSPlayerController playerController,string message,int timeToDisplay = 5)
        {
            if(playerController == null) { return; }
            if(!playerController.IsValid) { return; }
            if(message == null) { return; }
            HtmlMessage htmlMessage = new HtmlMessage(message, timeToDisplay);
            GuiManager.Instance!.ShowHtmlMessage(htmlMessage, playerController);
        }

        /// <summary>
        /// Get the value of a cookie
        /// </summary>
        /// <param name="playerController">player to get the cookie from</param>
        /// <param name="Cookie">cookie to read out</param>
        /// <param name="value">value of the cookie</param>
        /// <returns>true if successfull</returns>
        public static bool GetValueOfCookie(this CCSPlayerController playerController,string Cookie,out string? value)
        {
            value = null;
            if (playerController == null || !playerController.IsValid) { return false; }
            return CookieManager.GetValueOfCookie(playerController, Cookie, out value);
        }

        /// <summary>
        /// Set or add a value to a cookie
        /// </summary>
        /// <param name="playerController">set or add the value of a cookie</param>
        /// <param name="Cookie">cookie to be set</param>
        /// <param name="value">value of the cookie</param>
        /// <returns>True if successfull</returns>
        public static bool SetOrAddValueOfCookie(this CCSPlayerController playerController, string Cookie, string value)
        {
            if (playerController == null || !playerController.IsValid) { return false; }
            return CookieManager.AddOrSetValueOfCookie(playerController, Cookie, value);
        }

        /// <summary>
        /// Write a message to the chat of a player
        /// </summary>
        /// <param name="playerController">target player</param>
        /// <param name="message">message to be sent</param>
        public static void ChatMessage(this CCSPlayerController playerController, string message)
        {
            if (playerController == null || !playerController.IsValid) { return; }
            playerController.PrintToChat($"{CSPraccPlugin.Instance!.Config!.ChatPrefix} {message}");
        }
    }
}
