using System.Drawing;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Memory;

namespace CSPracc
{
    public class Utils
    {
        // https://github.com/alliedmodders/hl2sdk/blob/cs2/game/shared/shareddefs.h#L509
        private const uint EFL_NOCLIP_ACTIVE = (((uint)1) << 2);
        
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
        
        public static void ToggleNoClip(CCSPlayerController? player)
        {
            if (player == null || !player.IsValid || player.PlayerPawn.Value == null)
            {
                return;
            }
            if (player.PlayerPawn.Value.MoveType == MoveType_t.MOVETYPE_NOCLIP)
            {
                RemoveNoClip(player);
                return;
            }

            player.PlayerPawn.Value.Flags |= EFL_NOCLIP_ACTIVE;
            player.PlayerPawn.Value.MoveType = MoveType_t.MOVETYPE_NOCLIP;
            
            Schema.SetSchemaValue(player.PlayerPawn.Value.Handle, "CBaseEntity", "m_nActualMoveType", 8); // 7?
            
            Utilities.SetStateChanged(player.PlayerPawn.Value, "CBaseEntity", "m_MoveType");
        }

        public static void RemoveNoClip(CCSPlayerController? player)
        {
            if (player == null || !player.IsValid || player.PlayerPawn.Value == null)
            {
                return;
            }
            if (player.PlayerPawn.Value.MoveType == MoveType_t.MOVETYPE_WALK)
            {
                return;
            }
            
            player.PlayerPawn.Value.Flags &= ~EFL_NOCLIP_ACTIVE;
            player.PlayerPawn.Value.MoveType = MoveType_t.MOVETYPE_WALK;
            
            Schema.SetSchemaValue(player.PlayerPawn.Value.Handle, "CBaseEntity", "m_nActualMoveType", 2);
            
            Utilities.SetStateChanged(player.PlayerPawn.Value, "CBaseEntity", "m_MoveType");
            
            // maybe needs calling?:
            // NoClipStateChanged
            // https://github.com/alliedmodders/hl2sdk/blob/cs2/game/server/player.h#L406
            // see:
            // https://github.com/alliedmodders/hl2sdk/blob/cs2/game/server/client.cpp#L1207-L1286
        }

        public static void ServerMessage(string message)
        {
            Server.PrintToChatAll($"{CSPraccPlugin.Instance!.Config.ChatPrefix} {message}");
        }

        public static void ClientChatMessage(string message, CCSPlayerController player)
        {
            player.PrintToChat($"{CSPraccPlugin.Instance!.Config.ChatPrefix} {message}");
        }


        public static void ClientChatMessage(string message, ulong steamid)
        {
            CCSPlayerController? player = Utilities.GetPlayerFromSteamId(steamid);
            if (player != null && player.IsValid)
            {
                player.PrintToChat($"{CSPraccPlugin.Instance!.Config!.ChatPrefix} {message}");
            }
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
    }

}
