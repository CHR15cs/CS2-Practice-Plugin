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

namespace CSPracc.DataModules
{
    public class ProjectileStorage
    {
        Dictionary<int, ProjectileSnapshot> savedProjectiles = new Dictionary<int, ProjectileSnapshot>();
        FileInfo SaveFile { get; init; }
        string MapName {  get; init; }
        public ProjectileStorage(DirectoryInfo projectileSaveDirectory, string mapName)
        {
            MapName = mapName;
            SaveFile = new FileInfo(Path.Combine(projectileSaveDirectory.FullName, $"{MapName}_projectiles.json"));
            if (SaveFile.Exists)
            {
                string jsonString = File.ReadAllText(SaveFile.FullName);
                try
                {
                    savedProjectiles = JsonConvert.DeserializeObject<Dictionary<int, ProjectileSnapshot>>(jsonString);
                }
                catch
                {
                    Logging.LogMessage($"Could not read {SaveFile.Name}. Creating new dictonary.");
                    savedProjectiles = new Dictionary<int, ProjectileSnapshot>();
                }
            }
        }
        public int GetId()
        {
            int id = savedProjectiles.Count + 1;
            while (savedProjectiles.ContainsKey(id))
            {
                id++;
            }
            return id;
        }
        public void Add(Vector playerPosition, Vector projectilePosition, QAngle playerAngle, string title, string description, string map)
        {
            ProjectileSnapshot snapshot = new ProjectileSnapshot(GetId(), playerPosition.ToVector3(), projectilePosition.ToVector3(), playerAngle.ToVector3(), title, description, map);
            savedProjectiles.Add(snapshot.Id, snapshot);
            Save();
        }
        public void Add(Vector3 playerPosition, Vector3 projectilePosition, Vector3 playerAngle, string title, string description, string map)
        {
            ProjectileSnapshot snapshot = new ProjectileSnapshot(GetId(), playerPosition, projectilePosition, playerAngle, title, description, map);
            savedProjectiles.Add(snapshot.Id, snapshot);
            Save();
        }

        public void Add(ProjectileSnapshot snapshot)
        {
            Add(snapshot.PlayerPosition, snapshot.ProjectilePosition, snapshot.PlayerAngle, snapshot.Title, snapshot.Description, snapshot.Map);
        }
        public bool Get(int id, out ProjectileSnapshot snapshot)
        {
            if (savedProjectiles.ContainsKey(id))
            {
                snapshot = savedProjectiles[id];
                return true;
            }
            snapshot = new ProjectileSnapshot();
            return false;
        }
        public List<ProjectileSnapshot> GetAll()
        {
            return savedProjectiles.Values.ToList();
        }
        /// <summary>
        /// Removes snapshot from storage
        /// </summary>
        /// <param name="id">id to remove</param>
        /// <returns>true if succesfully removed or id is not present in storage. false if id is present but remove failed</returns>
        public bool Remove(int id)
        {
            if (!savedProjectiles.ContainsKey(id))
            {
                return true;
            }
            return savedProjectiles.Remove(id);
        }
        public bool IdExists(int id)
        {
            return savedProjectiles.ContainsKey(id);
        }
        public void Save()
        {
            FileInfo backupFile = new FileInfo($"{SaveFile.FullName}.backup");
            if(!SaveFile.Directory.Exists)
            {
                SaveFile.Directory.Create();  
            }
            if(SaveFile.Exists)
            {
                SaveFile.CopyTo($"{SaveFile.FullName}.backup", true);
            }            
            File.WriteAllText(SaveFile.FullName, JsonConvert.SerializeObject(savedProjectiles, formatting: Formatting.Indented));
        }
    }
}
