using CSPracc.Extensions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace CSPracc.DataModules
{
    public class Position
    {

        public Vector3 PlayerPos 
        { 
            get
            {
                return PlayerPosition.ToVector3();
            } 
            set { PlayerPosition = value.ToCSVector(); }
        }

        public Vector3 PlayerQAngle
        {
            get
            {
                return PlayerAngle.ToVector3();
            }
            set { PlayerAngle = value.ToCSQAngle(); }
        }

        [JsonIgnore]
        public CounterStrikeSharp.API.Modules.Utils.Vector PlayerPosition { get; private set; }
        [JsonIgnore]
        public CounterStrikeSharp.API.Modules.Utils.QAngle PlayerAngle { get; private set; }
        public Position(CounterStrikeSharp.API.Modules.Utils.Vector playerPosition, CounterStrikeSharp.API.Modules.Utils.QAngle playerAngle)
        {
            PlayerPosition = new CounterStrikeSharp.API.Modules.Utils.Vector(playerPosition.X, playerPosition.Y, playerPosition.Z);
            PlayerAngle = new CounterStrikeSharp.API.Modules.Utils.QAngle(playerAngle.X, playerAngle.Y, playerAngle.Z);
        }
    }
}
