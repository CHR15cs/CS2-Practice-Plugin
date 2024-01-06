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
using CounterStrikeSharp.API.Modules.Entities;
using CounterStrikeSharp.API;

namespace CSPracc.DataModules
{
    public class ProjectileSnapshot
    {        
        public GrenadeType_t GrenadeType_T { get; set; }
        public System.Numerics.Vector3 PlayerPosition { get; init; } = new Vector3(0, 0, 0);
        public System.Numerics.Vector3 PlayerVelocity { get; init; } = new Vector3(0,0,0);
        public System.Numerics.Vector3 ProjectilePosition { get; init; } = new Vector3(0, 0, 0);
        public System.Numerics.Vector3 PlayerAngle { get; init; } = new Vector3(0, 0, 0);
        public System.Numerics.Vector3 Velocity { get; init; } = new Vector3(0, 0, 0);
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public ProjectileSnapshot() { }
        public ProjectileSnapshot(Vector3 playerPosition, Vector3 projectilePosition, Vector3 playerAngle, Vector3 velocity,string title, string description, GrenadeType_t type)
        {
            PlayerPosition = playerPosition;
            ProjectilePosition = projectilePosition;
            PlayerAngle = playerAngle;
            Title = title;
            Description = description;
            Velocity = velocity;
            GrenadeType_T = type;
        }
        public void Restore(CCSPlayerController player)
        {
            Utils.RemoveNoClip(player);
            player.PlayerPawn.Value.Teleport(PlayerPosition.ToCSVector(), PlayerAngle.ToCSQAngle(), PlayerVelocity.ToCSVector());            
        }
    }
}
