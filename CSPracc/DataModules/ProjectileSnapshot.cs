using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using CSPracc.Extensions;

namespace CSPracc.DataModules
{
    public class ProjectileSnapshot
    {
        public Vector PlayerPosition { get; init; } = new Vector(0, 0, 0);
        public Vector PlayerVelocity { get; init; } = new Vector(0,0,0);
        public Vector ProjectilePosition { get; init; } = new Vector(0, 0, 0);
        public QAngle PlayerAngle { get; init; } = new QAngle(0, 0, 0);
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public string Map { get; init; } = "";
        public int Id { get; init; }
        public ProjectileSnapshot() { }
        public ProjectileSnapshot(int id, Vector playerPosition, Vector projectilePosition, QAngle playerAngle, string title, string description, string map)
        {
            PlayerPosition = playerPosition.Copy();
            ProjectilePosition = projectilePosition.Copy();
            PlayerAngle = playerAngle.Copy();
            Title = title;
            Description = description;
            Map = map;
            Id = id;
        }
        public void Restore(CCSPlayerController player)
        {
            player.PlayerPawn.Value.Teleport(PlayerPosition, PlayerAngle, PlayerVelocity);
        }
    }
}
