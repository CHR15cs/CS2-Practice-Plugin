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

        private Coordinate _playerPosition = null;
        public Coordinate PlayerPosition => _playerPosition;

        private Coordinate _grenadeCord = null;
        public Coordinate GrenadeCordinates => _grenadeCord;


        private string _title = string.Empty;
        public string Title => _title;


        private string _description = string.Empty;
        public string Description => _description;

        private string _map = string.Empty;
        public string Map => _map;

        public SavedNade(Coordinate playerCord, Coordinate grenadeCord, string title, string description, string map) 
        { 
            _playerPosition = playerCord;
            _grenadeCord = grenadeCord;
            _title = title;
            _description = description;
            _map = map;
        }
    }
}
