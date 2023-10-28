using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace CSPracc.DataModules
{
    /// <summary>
    /// This Class shall represent a grenade saved by a player in the pracitce mode
    /// </summary>
    public class SavedNade
    {

        private CounterStrikeSharp.API.Modules.Utils.Vector _playerPosition = null;
        public CounterStrikeSharp.API.Modules.Utils.Vector PlayerPosition => _playerPosition;

        private CounterStrikeSharp.API.Modules.Utils.Vector _playerVelocity = null;
        public CounterStrikeSharp.API.Modules.Utils.Vector Velocity => new CounterStrikeSharp.API.Modules.Utils.Vector(0,0,0);

        private CounterStrikeSharp.API.Modules.Utils.Vector _grenadeCord = null;
        public CounterStrikeSharp.API.Modules.Utils.Vector GrenadeCordinates => _grenadeCord;

        CounterStrikeSharp.API.Modules.Utils.Vector _playerAngle;
        public CounterStrikeSharp.API.Modules.Utils.Vector PlayerAngle => _playerAngle;

        private string _title = string.Empty;
        public string Title => _title;


        private string _description = string.Empty;
        public string Description => _description;

        private string _map = string.Empty;
        public string Map => _map;

        public SavedNade(CounterStrikeSharp.API.Modules.Utils.Vector palyerPos, CounterStrikeSharp.API.Modules.Utils.QAngle playerangle, CounterStrikeSharp.API.Modules.Utils.Vector grenadeCord,  string title, string description, string map) 
        {
            _playerPosition = new CounterStrikeSharp.API.Modules.Utils.Vector();
            _playerPosition.X = palyerPos.X; 
            _playerPosition.Y = palyerPos.Y;
            _playerPosition.Z = palyerPos.Z+4;
            _playerAngle = new CounterStrikeSharp.API.Modules.Utils.Vector();
            _playerAngle.X = playerangle.X;
            _playerAngle.Y = playerangle.Y;
            _playerAngle.Z = playerangle.Z;
            _playerVelocity = new CounterStrikeSharp.API.Modules.Utils.Vector();
            _playerVelocity.X = 0.0f; _playerVelocity.Y = 0.0f; _playerVelocity.Z = 0.0f;
            _grenadeCord = grenadeCord;
            _title = title;
            _description = description;
            _map = map;
        }
    }
}
