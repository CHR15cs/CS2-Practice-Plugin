using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CounterStrikeSharp.API.Modules.Utils;

namespace CSPracc.Extensions
{
    internal static class VectorExtensions
    {
        internal static Vector Copy(this Vector @this)
        {
            return new Vector(@this.X, @this.Y, @this.Z);
        }
        internal static QAngle Copy(this QAngle @this)
        {
            return new QAngle(@this.X, @this.Y, @this.Z);
        }

        internal static System.Numerics.Vector3 ToVector3(this Vector @this)
        {
            return new System.Numerics.Vector3(@this.X,@this.Y,@this.Z);
        }

        internal static System.Numerics.Vector3 ToVector3(this QAngle @this)
        {
            return new System.Numerics.Vector3(@this.X, @this.Y, @this.Z);
        }

        internal static Vector ToCSVector(this System.Numerics.Vector3 @this)
        {
            return new Vector(@this.X, @this.Y, @this.Z);
        }

        internal static QAngle ToCSQAngle(this System.Numerics.Vector3 @this)
        {
            return new QAngle(@this.X, @this.Y, @this.Z);
        }

    }
}
