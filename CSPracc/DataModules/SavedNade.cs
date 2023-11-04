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

        [XmlAttribute("PlayerPosition")]
        public CounterStrikeSharp.API.Modules.Utils.Vector PlayerPosition { get; set; }

        [XmlAttribute("Velocity")]
        public CounterStrikeSharp.API.Modules.Utils.Vector Velocity {  get; set; }

        [XmlAttribute("GrenadeCoordinates")]
        public CounterStrikeSharp.API.Modules.Utils.Vector GrenadeCordinates { get; set; }

        [XmlAttribute("PlayerAngle")]
        public CounterStrikeSharp.API.Modules.Utils.Vector PlayerAngle { get;set; }

        [XmlAttribute("Title")]
        public string Title {  get; set; }

        [XmlAttribute("Description")]
        public string Description { get; set; }

        [XmlAttribute("Map")]
        public string Map { get; set; }

        public SavedNade(string configFileLine)
        {
            PlayerPosition = new CounterStrikeSharp.API.Modules.Utils.Vector();
            PlayerAngle = new CounterStrikeSharp.API.Modules.Utils.Vector();
            Velocity = new CounterStrikeSharp.API.Modules.Utils.Vector(0, 0, 0);
            List<string> lines = configFileLine.Split(';').ToList();
            if(lines.Count == 9)
            {
                PlayerPosition.X = (float)Convert.ToDouble(lines[0]);
                PlayerPosition.Y = (float)Convert.ToDouble(lines[1]);
                PlayerPosition.Z = (float)Convert.ToDouble(lines[2]);

                PlayerAngle.X = (float)Convert.ToDouble(lines[3]);
                PlayerAngle.Y = (float)Convert.ToDouble(lines[4]);
                PlayerAngle.Z = (float)Convert.ToDouble(lines[5]);

                Title = lines[6];
                Description = lines[7];
                Map = lines[8];
            }

        }

        public SavedNade(CounterStrikeSharp.API.Modules.Utils.Vector palyerPos, CounterStrikeSharp.API.Modules.Utils.QAngle playerangle, CounterStrikeSharp.API.Modules.Utils.Vector grenadeCord,  string title, string description, string map) 
        {
            PlayerPosition = new CounterStrikeSharp.API.Modules.Utils.Vector();
            PlayerPosition.X = palyerPos.X;
            PlayerPosition.Y = palyerPos.Y;
            PlayerPosition.Z = palyerPos.Z+4;
            PlayerAngle = new CounterStrikeSharp.API.Modules.Utils.Vector();
            PlayerAngle.X = playerangle.X;
            PlayerAngle.Y = playerangle.Y;
            PlayerAngle.Z = playerangle.Z;
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
