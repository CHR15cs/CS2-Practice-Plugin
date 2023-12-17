using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Menu;
using CounterStrikeSharp.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CounterStrikeSharp.API.Modules.Utils;
using CSPracc.DataModules;
using CSPracc.CommandHandler;
using CounterStrikeSharp.API.Modules.Entities;
using System.Xml.Linq;
using CSPracc.Extensions;
using CSPracc.DataModules.Constants;
using CSPracc.Modes;
using CSPracc.DataStorages.JsonStorages;
using System.Reflection;

namespace CSPracc
{
    public class ProjectileManager
    {
        private static ProjectileManager _instance;
        public static ProjectileManager Instance 
        { 
            get
            {
                if(_instance == null )
                {
                    _instance = new ProjectileManager();
                }
                return _instance;
            }
                
         }
        public Dictionary<CCSPlayerController, ProjectileSnapshot> LastThrownGrenade = new Dictionary<CCSPlayerController, ProjectileSnapshot>();
        public Dictionary<int, DateTime> LastThrownSmoke = new Dictionary<int, DateTime>();


        private ChatMenu _nadeMenu = null;
        /// <summary>
        /// Grenade chat menu
        /// </summary>
        public ChatMenu NadeMenu
        {
            get
            {
                var NadesMenu = new ChatMenu("Nade Menu");
                var handleGive = (CCSPlayerController player, ChatMenuOption option) => RestoreSnapshot(player, option.Text);

                NadesMenu.AddMenuOption($" {ChatColors.Green}Global saved nades:", handleGive, true);
                foreach (KeyValuePair<int, ProjectileSnapshot> entry in CurrentProjectileStorage.GetAll())
                {
                    NadesMenu.AddMenuOption($" {ChatColors.Green}{entry.Value.Title} ID:{entry.Key}", handleGive);
                }
                return NadesMenu;
            }
        }
        /// <summary>
        /// Stored nades
        /// </summary>
        protected Dictionary<string, ProjectileStorage> projectileStorages { get; init; } = new Dictionary<string, ProjectileStorage>();
        /// <summary>
        /// Gets Projectile Storage for current map
        /// </summary>
        protected ProjectileStorage CurrentProjectileStorage
        {
            get
            {
                return GetOrAddProjectileStorage(Server.MapName);
            }
        }
        private ProjectileManager(){}
        /// <summary>
        /// Gets or Adds Projectile Storage for given map
        /// </summary>
        /// <param name="mapName">Map name</param>
        /// <returns>Projectile Storage for given map</returns>
        protected ProjectileStorage GetOrAddProjectileStorage(string mapName)
        {
            if (!projectileStorages.ContainsKey(mapName))
            {
                //TODO: Get Directory from config.
                projectileStorages.Add(mapName, new ProjectileStorage(new DirectoryInfo(Path.Combine(CSPraccPlugin.ModuleDir.FullName, "Projectiles"))));
            }
            return projectileStorages[mapName];
        }

        public ChatMenu GetNadeMenu(CCSPlayerController player)
        {
            ChatMenu menu = NadeMenu;
            
            if (LastThrownGrenade.TryGetValue(player,out ProjectileSnapshot savedNade))
            {
                var handleGive = (CCSPlayerController player, ChatMenuOption option) => RestoreSnapshot(player, option.Text);
                menu.AddMenuOption($" {ChatColors.Red}Personal nades:", handleGive, true);
                
                menu.AddMenuOption($" {ChatColors.Red}Last thrown projectile", handleGive);
            }
            return menu;
        }

        /// <summary>
        /// Teleport player to grenade position
        /// </summary>
        /// <param name="player">player to teleport</param>
        /// <param name="grenadeName">grenade destination</param>
        private void RestoreSnapshot(CCSPlayerController player, string grenadeName)
        {
            int index = grenadeName.IndexOf(":");
            if(index == -1)
            {
                //: not found in string
                if (LastThrownGrenade.TryGetValue(player, out ProjectileSnapshot snapshot))
                {
                    snapshot.Restore(player);
                    return;
                }
                player.PrintToCenter($"Could not find id in grenade name {grenadeName}");
                return;
            }
            string idofNade = grenadeName.Substring(index + 1);
            if (!int.TryParse(idofNade, out int snapshotId))
            {
                player.PrintToCenter($"Failed to parse protectile id from {idofNade}");
                return;
            }
            RestoreSnapshot(player, snapshotId);
        }
        internal void RestoreSnapshot(CCSPlayerController player, int snapshotId)
        {
            if (CurrentProjectileStorage.Get(snapshotId, out ProjectileSnapshot snapshot))
            {
                snapshot.Restore(player);
                return;
            }
            else
            {
                player.PrintToCenter($"No projectile found with id {snapshotId}");
                return;
            }
        }
        /// <summary>
        /// Add grenade to the list
        /// </summary>
        /// <param name="player">palyer who issued the command</param>
        /// <param name="args">Arguments shall look like <Name> <Description></param>
        public void SaveSnapshot(CCSPlayerController player,string args)
        {
            if (player == null) return;
            if (args == String.Empty) return;
            Vector playerPosition = player.PlayerPawn.Value.CBodyComponent!.SceneNode!.AbsOrigin;
            //TODO provide actual projectile Position
            Vector projectilePosition = new Vector();
            QAngle playerAngle = player.PlayerPawn.Value.EyeAngles;
            string name = args;
            //TODO parse actual description if provided
            string description = "";
            CurrentProjectileStorage.Add(playerPosition, projectilePosition, playerAngle, name, description, Server.MapName);
            player.PrintToCenter($"Successfully added grenade {name}");
        }

        /// <summary>
        /// Add grenade to the list
        /// </summary>
        /// <param name="player">palyer who issued the command</param>
        /// <param name="args">Arguments shall look like <Name> <Description></param>
        public void RemoveSnapshot(CCSPlayerController player, string args)
        {
            if (player == null) return;
            if (args == String.Empty) return;
            args = args.Trim();
            int id = -1;
            try
            {
                id = Convert.ToInt32(args);
            }
            catch
            {
                player.PrintToCenter("invalid argument, needs to be a number");
                return;
            }
            if(!CurrentProjectileStorage.ContainsKey(id))
            {
                player.PrintToCenter($"Projectile with id {id} does not exist on current map");
                return;
            }
            if (CurrentProjectileStorage.RemoveKey(id))
            {
                player.PrintToCenter($"Successfully removed projectile with id {id}");
            }
            else
            {
                player.PrintToCenter($"Failed to remove projectile with id {id}");
            }

        }

        public void OnEntitySpawned(CEntityInstance entity)
        { 
            var designerName = entity.DesignerName;
            PracticeMode test = null;
            try
            {
                test = (PracticeMode)CSPraccPlugin.PluginMode;
            }
            catch (Exception e)
            {
                return;
            }
            if (test == null) return;

            if (!entity.IsProjectile())
            {
                return;
            }
            CBaseCSGrenadeProjectile projectile;

            switch (entity.Entity!.DesignerName){
                case (DesignerNames.ProjectileSmoke):
                    {
                        projectile = new CSmokeGrenadeProjectile(entity.Handle);
                        break;
                    }
                default:
                    {
                        projectile = new CBaseCSGrenadeProjectile(entity.Handle);
                        break;
                    }
            };

                Server.NextFrame(() =>
                {
                    CCSPlayerController player = new CCSPlayerController(projectile.Thrower.Value.Controller.Value.Handle);
                    Vector playerPosition = player.PlayerPawn.Value.CBodyComponent!.SceneNode!.AbsOrigin;
                    //TODO provide actual projectile Position
                    Vector projectilePosition = new Vector();
                    QAngle playerAngle = player.PlayerPawn.Value.EyeAngles;
                    string name = "LastThrown";
                    //TODO parse actual description if provided
                    string description = "";
                    
                    ProjectileSnapshot tmpSnapshot = new ProjectileSnapshot(playerPosition.ToVector3(), projectilePosition.ToVector3(), playerAngle.ToVector3(), name, description);
                    LastThrownGrenade.SetOrAdd(player, tmpSnapshot);
                });

            if (!PracticeCommandHandler.PraccSmokeColorEnabled) return;
            if (projectile is CSmokeGrenadeProjectile)
            {
                Server.NextFrame(() =>
                {
                    CSmokeGrenadeProjectile smokeProjectile = (CSmokeGrenadeProjectile)projectile;
                    CCSPlayerController player = new CCSPlayerController(projectile.Thrower.Value.Controller.Value.Handle);
                    smokeProjectile.SmokeColor.X = (float)Utils.GetTeamColor(player).R;
                    smokeProjectile.SmokeColor.Y = (float)Utils.GetTeamColor(player).G;
                    if(LastThrownSmoke.ContainsKey(((int)projectile.Index)))
                    {
                        LastThrownSmoke[(int)projectile.Index] = DateTime.Now;
                    }
                    else
                    {
                        LastThrownSmoke.Add((int)projectile.Index, DateTime.Now);
                    }                    
                    Logging.LogMessage($"smoke color {smokeProjectile.SmokeColor}");
                });
            }           
        }

        public HookResult OnSmokeDetonate(EventSmokegrenadeDetonate @event, GameEventInfo info)
        {
            if(LastThrownSmoke.TryGetValue(@event.Entityid, out var result)) 
            {
                Utils.ServerMessage($"Smoke thrown by {@event.Userid.PlayerName} took {(DateTime.Now - result).TotalSeconds.ToString("0.00")}s to detonate");
            }
            return HookResult.Continue;
        }


        public void SaveLastGrenade(CCSPlayerController playerController, string name)
        {
            if(!LastThrownGrenade.TryGetValue(playerController, out ProjectileSnapshot? projectile))
            {
                return;
            }
            projectile.Title = name;
            CurrentProjectileStorage.Add(projectile);
            playerController.PrintToCenter($"Successfully added grenade {name}");
            LastThrownGrenade.Remove(playerController);
        }
    }
}
