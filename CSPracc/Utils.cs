using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API;
using CSPracc.DataModules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using CounterStrikeSharp.API.Modules.Entities;
using CounterStrikeSharp.API.Modules.Memory.DynamicFunctions;
using System.Runtime.InteropServices;
using CounterStrikeSharp.API.Modules.Utils;
using System.Reflection.Metadata;

namespace CSPracc
{
    /// <summary>
    /// Util class, providing utility functions
    /// </summary>
    public class Utils
    {
        /// <summary>
        /// Deleting Grenades from server
        /// </summary>
        public static void RemoveGrenadeEntitiesFromPlayer(CCSPlayerController player)
        {
            var smokes = Utilities.FindAllEntitiesByDesignerName<CSmokeGrenadeProjectile>("smokegrenade_projectile");
            foreach (var entity in smokes)
            {
                if (entity != null)
                {
                    CCSPlayerController? thrower = new CCSPlayerController(entity.Thrower.Value.Controller.Value.Handle);
                    if(thrower == null)
                    {                      
                        return;
                    }
                    if (thrower.Handle == player.Handle)
                    {
                        entity.Remove();
                    }

                }
            }
            var mollys = Utilities.FindAllEntitiesByDesignerName<CSmokeGrenadeProjectile>("molotov_projectile");
            foreach (var entity in mollys)
            {
                if (entity != null)
                {
                    CCSPlayerController thrower = new CCSPlayerController(entity.Thrower.Value.Controller.Value.Handle);
                    if (thrower.Handle == player.Handle)
                    {
                        entity.Remove();
                    }
                }
            }
            var inferno = Utilities.FindAllEntitiesByDesignerName<CSmokeGrenadeProjectile>("inferno");
            foreach (var entity in inferno)
            {
                if (entity != null)
                {
                    if (entity.Thrower.Index == player.Index)
                    {
                        entity.Remove();
                    }
                }
            }
        }

        /// <summary>
        /// Deleting Grenades from server
        /// </summary>
        public static void RemoveGrenadeEntities()
        {
            var smokes = Utilities.FindAllEntitiesByDesignerName<CSmokeGrenadeProjectile>("smokegrenade_projectile");
            foreach (var entity in smokes)
            {
                if (entity != null)
                {
                    entity.Remove();
                }
            }
            var mollys = Utilities.FindAllEntitiesByDesignerName<CSmokeGrenadeProjectile>("molotov_projectile");
            foreach (var entity in mollys)
            {
                if (entity != null)
                {
                    entity.Remove();
                }
            }
            var inferno = Utilities.FindAllEntitiesByDesignerName<CSmokeGrenadeProjectile>("inferno");
            foreach (var entity in inferno)
            {
                if (entity != null)
                {
                    entity.Remove();
                }
            }
        }

        /// <summary>
        /// Removing noclip from player
        /// </summary>
        /// <param name="player">player to remove noclip from</param>
        public static void RemoveNoClip(CCSPlayerController player)
        {
            if (player == null || !player.IsValid) return;

            if (player.PlayerPawn.Value!.MoveType == MoveType_t.MOVETYPE_NOCLIP)
            {
                player.PlayerPawn.Value.MoveType = MoveType_t.MOVETYPE_WALK;
            }
        }

        /// <summary>
        /// Message to all players on server
        /// </summary>
        /// <param name="message">message to be sent</param>
        public static void ServerMessage(string message)
        {
            Server.PrintToChatAll($"{CSPraccPlugin.Instance!.Config!.ChatPrefix} {message}");
        }

        /// <summary>
        /// Message to specific player
        /// </summary>
        /// <param name="message">message to be sent</param>
        /// <param name="player">player to be messaged</param>
        public static void ClientChatMessage(string message, CCSPlayerController player)
        {
            player.PrintToChat($"{CSPraccPlugin.Instance!.Config!.ChatPrefix} {message}");
        }

        /// <summary>
        /// Message to specific player
        /// </summary>
        /// <param name="message">message to be sent</param>
        /// <param name="player">player to be messaged</param>
        public static void ClientChatMessage(string message, ulong steamid)
        {
            CCSPlayerController? player = Utilities.GetPlayerFromSteamId(steamid);
            if (player != null && player.IsValid)
            {
                player.PrintToChat($"{CSPraccPlugin.Instance!.Config!.ChatPrefix} {message}");
            }
        }

        /// <summary>
        /// Get the team color of a player
        /// </summary>
        /// <param name="playerController">player</param>
        /// <returns>Color</returns>
        public static Color GetTeamColor(CCSPlayerController playerController)
        {
            Logging.LogMessage($"Getting Color of player {playerController.CompTeammateColor}");
            switch (playerController.CompTeammateColor)
            {
                case 1:
                    return Color.FromArgb(50, 255, 0);
                case 2:
                    return Color.FromArgb(255, 255, 0);
                case 3:
                    return Color.FromArgb(255, 132, 0);
                case 4:
                    return Color.FromArgb(255, 0, 255);
                case 0:
                    return Color.FromArgb(0, 187, 255);
                default:
                    return Color.Red;
            }
        }

        /// <summary>
        /// Break all breakable props on the map
        /// </summary>
        public static void BreakAll()
        {
            var props = Utilities.FindAllEntitiesByDesignerName<CBreakable>("prop_dynamic")
                .Concat(Utilities.FindAllEntitiesByDesignerName<CBreakable>("func_breakable"));
            foreach(var prop in props)
            {
                if(prop != null && prop.IsValid)
                {
                    prop.AcceptInput("break");
                }
            }
        }

        /// <summary>
        /// Execute a config
        /// </summary>
        /// <param name="configName">config to be executed</param>
        /// <returns>true if successfull</returns>
        public static bool ExecuteConfig(string configName)
        {
            ///TODO: Implement a check wether config exists
            Server.ExecuteCommand($"exec {configName}");
            return true;
        }   


        /// <summary>
        /// Draw a rectangle around a position
        /// </summary>
        /// <param name="position">position</param>
        /// <param name="color">color of rectangle</param>
        public static void DrawRectanlgeAroundPosition(Vector position, Color color)
        {
            var laserLeftSide = Utilities.CreateEntityByName<CBeam>("beam");
            if (laserLeftSide == null)
            {
                Server.PrintToChatAll("laser null");
                return;
            }
            laserLeftSide.Glow.GlowColor.X = 255;
            laserLeftSide.Glow.GlowColor.Y = 255;
            laserLeftSide.Glow.GlowColor.Z = 255;
            laserLeftSide.Glow.GlowStartTime = 0;
            laserLeftSide.Glow.GlowTime = 0xffffffff;
            laserLeftSide.Glow.Glowing = true;
            laserLeftSide.EndPos.X = position.X-20;
            laserLeftSide.EndPos.Y = position.Y - 20;
            laserLeftSide.EndPos.Z = position.Z;
            Vector posStart = new Vector(position.X - 20, position.Y + 20, position.Z);
            laserLeftSide.Width = 1.0f;
            laserLeftSide.Render = color;
            laserLeftSide.DispatchSpawn();
            laserLeftSide.Teleport(posStart, new QAngle(0, 0, 0), new Vector(0, 0, 0));
            var laserTopSide = Utilities.CreateEntityByName<CBeam>("beam");
            if (laserTopSide == null)
            {
                Server.PrintToChatAll("laser null");
                return;
            }
            laserTopSide.EndPos.X = position.X - 20;
            laserTopSide.EndPos.Y = position.Y + 20;
            laserTopSide.EndPos.Z = position.Z;
            posStart = new Vector(position.X + 20, position.Y + 20, position.Z);
            laserTopSide.Width = 1.0f;
            laserTopSide.Render = color;
            laserTopSide.DispatchSpawn();
            laserTopSide.Teleport(posStart, new QAngle(0, 0, 0), new Vector(0, 0, 0));
            var laserRightSide = Utilities.CreateEntityByName<CBeam>("beam");
            if (laserRightSide == null)
            {
                Server.PrintToChatAll("laser null");
                return;
            }
            laserRightSide.EndPos.X = position.X + 20;
            laserRightSide.EndPos.Y = position.Y + 20;
            laserRightSide.EndPos.Z = position.Z;
            posStart = new Vector(position.X + 20, position.Y - 20, position.Z);
            laserRightSide.Width = 1.0f;
            laserRightSide.Render = color;
            laserRightSide.DispatchSpawn();
            laserRightSide.Teleport(posStart, new QAngle(0, 0, 0), new Vector(0, 0, 0));
            var laserBottomSide = Utilities.CreateEntityByName<CBeam>("beam");
            if (laserBottomSide == null)
            {
                Server.PrintToChatAll("laser null");
                return;
            }
            laserBottomSide.EndPos.X = position.X + 20;
            laserBottomSide.EndPos.Y = position.Y - 20;
            laserBottomSide.EndPos.Z = position.Z;
            posStart = new Vector(position.X - 20, position.Y - 20, position.Z);
            laserBottomSide.Width = 1.0f;
            laserBottomSide.Render = color;
            laserBottomSide.DispatchSpawn();
            laserBottomSide.Teleport(posStart, new QAngle(0, 0, 0), new Vector(0, 0, 0));
        }
    }

}
