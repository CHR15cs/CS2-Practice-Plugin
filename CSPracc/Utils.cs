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

namespace CSPracc
{
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
                    CCSPlayerController thrower = new CCSPlayerController(entity.Thrower.Value.Controller.Value.Handle);
                    if(thrower.Handle == player.Handle)
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

        public static void RemoveNoClip(CCSPlayerController player)
        {
            if (player == null || !player.IsValid) return;

            if (player.PlayerPawn.Value!.MoveType == MoveType_t.MOVETYPE_NOCLIP)
            {
                player.PlayerPawn.Value.MoveType = MoveType_t.MOVETYPE_WALK;
            }
        }

        public static void ServerMessage(string message)
        {
            Server.PrintToChatAll($"{CSPraccPlugin.Instance!.Config.ChatPrefix} {message}");
        }

        public static void ClientChatMessage(string message, CCSPlayerController player)
        {
            player.PrintToChat($"{CSPraccPlugin.Instance!.Config.ChatPrefix} {message}");
        }

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

        public static string acceptInputWindowsSig = "\\x48\\x89\\x5C\\x24\\x10\\x48\\x89\\x74\\x24\\x18\\x57\\x48\\x83\\xEC\\x40\\x49\\x8B\\xF0";
        public static string acceptInputLinuxSig = "\\x55\\x48\\x89\\xE5\\x41\\x57\\x49\\x89\\xFF\\x41\\x56\\x48\\x8D\\x7D\\xC0";

        public static MemoryFunctionVoid<IntPtr, string, IntPtr, IntPtr, string, int> AcceptEntityInputFunc = new(RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? acceptInputLinuxSig : acceptInputWindowsSig);

        public static Action<IntPtr, string, IntPtr, IntPtr, string, int> AcceptEntityInput = AcceptEntityInputFunc.Invoke;

        //Thanks to WD- for the AcceptInput implementation
        public static void AcceptInput(IntPtr handle, string inputName, IntPtr activator, IntPtr caller, string value)
        {
            AcceptEntityInput(handle, inputName, activator, caller, value, 0);
        }

        public static void BreakProps(CCSPlayerController player)
        {
            if (player == null) return;
            var entities = Utilities.FindAllEntitiesByDesignerName<CBreakable>("prop_dynamic");
            foreach (var entity in entities)
            {
                AcceptInput(entity.Handle, "Break", player.Handle, player.Handle, "");
            }
            entities = Utilities.FindAllEntitiesByDesignerName<CBreakable>("func_breakable");
            foreach (var entity in entities)
            {
                AcceptInput(entity.Handle, "Break", player.Handle, player.Handle, "");
            }
        }
    }
}
