using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace CSPracc.DataModules
{
    /// <summary>
    /// This Class shall represent a grenade saved by a player in the pracitce mode
    /// </summary>
    public class SavedNade
    {

        [XmlIgnore]
        public CounterStrikeSharp.API.Modules.Utils.Vector PlayerPosition { 
            get { return new CounterStrikeSharp.API.Modules.Utils.Vector(PlayerPositionX, PlayerPositionY, PlayerPositionZ); } 
            set { } }

        private float _playerPositionX = 0.0f;
        [XmlAttribute("PlayerPositionX")]
        public float PlayerPositionX {
            get { return _playerPositionX; }
            set { _playerPositionX = value; }

        }
        private float _playerPositionY = 0.0f;
        [XmlAttribute("PlayerPositionY")]
        public float PlayerPositionY
        {
            get { return _playerPositionY; }
            set { _playerPositionY = value; }
        }
        private float _playerPositionZ = 0.0f;
        [XmlAttribute("PlayerPositionZ")]
        public float PlayerPositionZ
        {
            get { return _playerPositionZ; }
            set { _playerPositionZ = value; }
        }

        [XmlIgnore]
        public CounterStrikeSharp.API.Modules.Utils.Vector Velocity { get { return new CounterStrikeSharp.API.Modules.Utils.Vector(0, 0, 0);  } set { } }

        [XmlIgnore]
        public CounterStrikeSharp.API.Modules.Utils.Vector GrenadeCordinates { get; set; }

        [XmlIgnore]
        public CounterStrikeSharp.API.Modules.Utils.Vector PlayerAngle
        {
            get { return new CounterStrikeSharp.API.Modules.Utils.Vector(PlayerAngleX, PlayerAngleY, PlayerAngleZ); }
            set { }
        }

        private float _playerAngleX = 0.0f;
        [XmlAttribute("PlayerAngleX")]
        public float PlayerAngleX
        {
            get { return _playerAngleX; }
            set { _playerAngleX = value; }
        }
        private float _playerAngleY = 0.0f;
        [XmlAttribute("PlayerAngleY")]
        public float PlayerAngleY
        {
            get { return _playerAngleY; }
            set { _playerAngleY = value; }
        }
        private float _playerAngleZ = 0.0f;
        [XmlAttribute("PlayerAngleZ")]
        public float PlayerAngleZ
        {
            get { return _playerAngleZ; }
            set { _playerAngleZ = value; }
        }


        [XmlAttribute("Title")]
        public string Title {  get; set; }

        [XmlAttribute("Description")]
        public string Description { get; set; }

        [XmlAttribute("Map")]
        public string Map { get; set; }

        public SavedNade(CounterStrikeSharp.API.Modules.Utils.Vector palyerPos, CounterStrikeSharp.API.Modules.Utils.QAngle playerangle, CounterStrikeSharp.API.Modules.Utils.Vector grenadeCord,  string title, string description, string map) 
        {
            PlayerPositionX = palyerPos.X;
            PlayerPositionY = palyerPos.Y;
            PlayerPositionZ = palyerPos.Z+4;
            PlayerAngleX = playerangle.X;
            PlayerAngleY = playerangle.Y;
            PlayerAngleZ = playerangle.Z;
            Velocity = new CounterStrikeSharp.API.Modules.Utils.Vector(0,0,0);
            GrenadeCordinates = grenadeCord;
            Title = title;
            if(!String.IsNullOrEmpty(description))
            {
                Description = description;
            }
            else
            {
                Description = "No Description available!";
            }
            
            Map = map;
        }

        public SavedNade()
        {

        }


        public override string ToString()
        {
            return $"{PlayerPosition.X};{PlayerPosition.Y};{PlayerPosition.Z};{PlayerAngle.X};{PlayerAngle.Y};{PlayerAngle.Z};{Title},{Description},{Map}";
        }
    }
}
