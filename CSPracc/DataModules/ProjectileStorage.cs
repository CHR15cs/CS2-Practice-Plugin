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
using CSPracc.DataStorages.JsonStorages;
using CounterStrikeSharp.API;
using CSPracc.DataStorages;
using Newtonsoft.Json.Linq;

namespace CSPracc.DataModules
{
    public class ProjectileStorage : JsonStorage<int, ProjectileSnapshot>
    {
        public ProjectileStorage(DirectoryInfo projectileSaveDirectory, string mapName) : base(new FileInfo(Path.Combine(projectileSaveDirectory.FullName, $"{Server.MapName}_projectiles.json"))){}
        public int GetUnusedKey()
        {
            int id = Storage.Count + 1;
            while (Storage.ContainsKey(id))
            {
                id++;
            }
            return id;
        }
        public void Add(Vector playerPosition, Vector projectilePosition, QAngle playerAngle, string title, string description, string map)
        {
            ProjectileSnapshot snapshot = new ProjectileSnapshot(playerPosition.ToVector3(), projectilePosition.ToVector3(), playerAngle.ToVector3(), title, description);
            Add(snapshot);
        }
        public void Add(Vector3 playerPosition, Vector3 projectilePosition, Vector3 playerAngle, string title, string description)
        {
            ProjectileSnapshot snapshot = new ProjectileSnapshot(playerPosition, projectilePosition, playerAngle, title, description);
            Add(snapshot);
        }

        public void Add(ProjectileSnapshot snapshot)
        {
            SetOrAdd(GetUnusedKey(), snapshot);
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
