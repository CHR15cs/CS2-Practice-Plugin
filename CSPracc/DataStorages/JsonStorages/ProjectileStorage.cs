using CounterStrikeSharp.API.Modules.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using CSPracc.Extensions;
using System.Numerics;
using Vector = CounterStrikeSharp.API.Modules.Utils.Vector;
using CounterStrikeSharp.API;
using CSPracc.DataStorages;
using Newtonsoft.Json.Linq;
using CSPracc.DataModules;
using CounterStrikeSharp.API.Core;

namespace CSPracc.DataStorages.JsonStorages
{
    public class ProjectileStorage : JsonStorage<int, ProjectileSnapshot>
    {
        public ProjectileStorage(DirectoryInfo projectileSaveDirectory) : base(new FileInfo(Path.Combine(projectileSaveDirectory.FullName, $"{Server.MapName}_projectiles.json"))) { }
        public int GetUnusedKey()
        {
            int id = Storage.Count + 1;
            while (Storage.ContainsKey(id))
            {
                id++;
            }
            return id;
        }

        public int Add(CCSPlayerController player, CBaseCSGrenadeProjectile? projectile, string title, string map)
        {
            ProjectileSnapshot? snapshot = null;
            if (projectile != null)
            {
                snapshot = new ProjectileSnapshot(player, projectile, title, "", GrenadeType_t.GRENADE_TYPE_SMOKE);
            }
            else
            {
                snapshot = new ProjectileSnapshot(player.PlayerPawn.Value!.CBodyComponent!.SceneNode!.AbsOrigin.ToVector3(), player.PlayerPawn.Value!.CBodyComponent!.SceneNode!.AbsOrigin.ToVector3(), player.PlayerPawn.Value!.EyeAngles.ToVector3(),new Vector3(0,0,0), title, "", GrenadeType_t.GRENADE_TYPE_SMOKE,player.SteamID);
            }
            
            return Add(snapshot);
        }

        public void Add(Vector playerPosition, Vector projectilePosition, QAngle playerAngle, Vector velocity,string title, string description, string map)
        {
            ProjectileSnapshot snapshot = new ProjectileSnapshot(playerPosition.ToVector3(), projectilePosition.ToVector3(), playerAngle.ToVector3(), velocity.ToVector3(), title, description,GrenadeType_t.GRENADE_TYPE_SMOKE);
            Add(snapshot);
        }
        public void Add(Vector3 playerPosition, Vector3 projectilePosition, Vector3 playerAngle, Vector3 velocity,string title, string description)
        {
            ProjectileSnapshot snapshot = new ProjectileSnapshot(playerPosition, projectilePosition, playerAngle, velocity, title, description, GrenadeType_t.GRENADE_TYPE_SMOKE);
            Add(snapshot);
        }

        public int Add(ProjectileSnapshot snapshot)
        {
            int newkey = GetUnusedKey();
            SetOrAdd(newkey, snapshot);
            return newkey;
        }
        public override bool Get(int id, out ProjectileSnapshot snapshot)
        {
            if (!Storage.TryGetValue(id, out snapshot))
            {
                snapshot = new ProjectileSnapshot();
                return false;
            }
            return true;
        }
    }
}
