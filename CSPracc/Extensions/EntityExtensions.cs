using CounterStrikeSharp.API.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSPracc.DataModules.Constants;

namespace CSPracc.Extensions
{
    internal static class EntityExtensions
    {
        internal static bool IsProjectile(this CEntityInstance @this)
        {
            if(@this == null)
            {
                return false;
            }
            if(@this.Entity == null)
            {
                return false;
            }
            return @this.Entity.DesignerName.EndsWith("_projectile");
        }
        internal static bool IsSmokeProjectile(this CEntityInstance @this)
        {
            return @this.IsProjectile() && @this.Entity!.DesignerName.Equals(DesignerNames.ProjectileSmoke);
        }
    }
}
