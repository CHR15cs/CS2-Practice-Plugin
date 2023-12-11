using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API;
using CSPracc.DataModules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

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
                    entity.Remove();
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
    }
}
