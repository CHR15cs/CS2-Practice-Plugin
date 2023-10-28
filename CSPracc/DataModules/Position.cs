using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSPracc.DataModules
{
    public class Position
    {

        public CounterStrikeSharp.API.Modules.Utils.Vector PlayerPosition { get; private set; }
        public CounterStrikeSharp.API.Modules.Utils.QAngle PlayerAngle { get; private set; }
        public Position(CounterStrikeSharp.API.Modules.Utils.Vector playerPosition, CounterStrikeSharp.API.Modules.Utils.QAngle playerAngle)
        {
            PlayerPosition = playerPosition;
            PlayerAngle = playerAngle;
        }
    }
}
