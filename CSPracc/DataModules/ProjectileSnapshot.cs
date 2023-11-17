using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using CSPracc.Extensions;
using System.Numerics;
using Vector = CounterStrikeSharp.API.Modules.Utils.Vector;

namespace CSPracc.DataModules
{
    public class ProjectileSnapshot
    {        
        public System.Numerics.Vector3 PlayerPosition { get; init; } = new Vector3(0, 0, 0);
        public System.Numerics.Vector3 PlayerVelocity { get; init; } = new Vector3(0,0,0);
        public System.Numerics.Vector3 ProjectilePosition { get; init; } = new Vector3(0, 0, 0);
        public System.Numerics.Vector3 PlayerAngle { get; init; } = new Vector3(0, 0, 0);
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public string Map { get; init; } = "";
        public int Id { get; init; }
        public ProjectileSnapshot() { }
        public ProjectileSnapshot(int id, Vector3 playerPosition, Vector3 projectilePosition, Vector3 playerAngle, string title, string description, string map)
        {
            PlayerPosition = playerPosition;
            ProjectilePosition = projectilePosition;
            PlayerAngle = playerAngle;
            Title = title;
            Description = description;
            Map = map;
            Id = id;
        }
        public void Restore(CCSPlayerController player)
        {
            player.PlayerPawn.Value.Teleport(PlayerPosition.ToCSVector(), PlayerAngle.ToCSQAngle(), PlayerVelocity.ToCSVector());
        }
    }
}
