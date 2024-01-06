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
using System.Numerics;
using Vector = CounterStrikeSharp.API.Modules.Utils.Vector;
using System.Globalization;
using CounterStrikeSharp.API.Modules.Entities.Constants;
using Microsoft.Extensions.Logging;
using static System.Formats.Asn1.AsnWriter;
using CounterStrikeSharp.API.Modules.Memory;
using System.ComponentModel;

namespace CSPracc
{
    public class ProjectileManager : IDisposable
    {
        public Dictionary<ulong, ProjectileSnapshot> LastThrownGrenade = new Dictionary<ulong, ProjectileSnapshot>();
        public List<CBaseCSGrenadeProjectile?> SelfThrownGrenade = new List<CBaseCSGrenadeProjectile?>();
        public Dictionary<int, DateTime> LastThrownSmoke = new Dictionary<int, DateTime>();
        Dictionary<ulong,Position> FlashPosition = new Dictionary<ulong, Position>();
        public List<ulong> NoFlashList = new List<ulong>();
        int nadeToSpectate = 0;
        


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
        public ProjectileManager()
        {
            CSPraccPlugin.Instance.RegisterListener<Listeners.OnTick>(OnTick);
        }


        void IDisposable.Dispose()
        {
            CSPraccPlugin.Instance!.RemoveListener("OnTick", OnTick);
        }
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

        public HtmlMenu GetNadeMenu(CCSPlayerController player)
        {
            List<KeyValuePair<string, Action>> nadeOptions = new List<KeyValuePair<string, Action>>();



            //NadesMenu.AddMenuOption($" {ChatColors.Green}Global saved nades:", handleGive, true);
            foreach (KeyValuePair<int, ProjectileSnapshot> entry in CurrentProjectileStorage.GetAll())
            {
                nadeOptions.Add(new KeyValuePair<string, Action>($"{entry.Value.Title} ID:{entry.Key}", new Action(() => RestoreSnapshot(player, entry.Key))));              
            }
           HtmlMenu htmlNadeMenu =  new HtmlMenu("Nade Menu", nadeOptions, false); ;
            
            //if (LastThrownGrenade.TryGetValue(player,out ProjectileSnapshot savedNade))
            //{
            //    var handleGive = (CCSPlayerController player, ChatMenuOption option) => RestoreSnapshot(player, option.Text);
            //    menu.AddMenuOption($" {ChatColors.Red}Personal nades:", handleGive, true);
            //   // nadeOptions.Add(new KeyValuePair<string, Task>($"Personal nades:", new Task()));

            //    menu.AddMenuOption($" {ChatColors.Red}Last thrown projectile", handleGive);
            //}
            return htmlNadeMenu;
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
                if (LastThrownGrenade.TryGetValue(player.SteamID, out ProjectileSnapshot snapshot))
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
            CounterStrikeSharp.API.Modules.Utils.Vector playerPosition = player.PlayerPawn.Value.CBodyComponent!.SceneNode!.AbsOrigin;
            //TODO provide actual projectile Position
            CounterStrikeSharp.API.Modules.Utils.Vector projectilePosition = new CounterStrikeSharp.API.Modules.Utils.Vector();
            QAngle playerAngle = player.PlayerPawn.Value.EyeAngles;
            string name = args;
            //TODO parse actual description if provided
            string description = "";
            CurrentProjectileStorage.Add(playerPosition, projectilePosition, playerAngle, new Vector(0,0,0),name, description, Server.MapName);
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
                    CounterStrikeSharp.API.Modules.Utils.Vector playerPosition = player.PlayerPawn.Value.CBodyComponent!.SceneNode!.AbsOrigin;
                    //TODO provide actual projectile Position
                    CounterStrikeSharp.API.Modules.Utils.Vector projectilePosition = new CounterStrikeSharp.API.Modules.Utils.Vector();
                    QAngle playerAngle = player.PlayerPawn.Value.EyeAngles;
                    string name = "LastThrown";
                    //TODO parse actual description if provided
                    string description = "";
                    GrenadeType_t type = GrenadeType_t.GRENADE_TYPE_SMOKE;
                    switch(projectile.DesignerName)
                    {
                        case DesignerNames.ProjectileSmoke:
                            {
                                type = GrenadeType_t.GRENADE_TYPE_SMOKE;
                                break;
                            }
                        case DesignerNames.ProjectileFlashbang:
                            {
                                type = GrenadeType_t.GRENADE_TYPE_FLASH;
                                TpToFlashPos(player);
                                break;
                            }
                        case DesignerNames.ProjectileHE:
                            {
                                type = GrenadeType_t.GRENADE_TYPE_EXPLOSIVE;
                                break;
                            }
                        case DesignerNames.ProjectileMolotov:
                            {
                                type = GrenadeType_t.GRENADE_TYPE_FIRE;
                                break;
                            }
                        case DesignerNames.ProjectileDecoy:
                            {
                                type = GrenadeType_t.GRENADE_TYPE_DECOY;
                                break;
                            }
                        default :
                            {
                                type = GrenadeType_t.GRENADE_TYPE_SMOKE;
                                break;
                            }
                            
                    }
                    if ( projectile.Globalname != "custom")
                    {
                        if(type == GrenadeType_t.GRENADE_TYPE_SMOKE)
                        {
                            CSmokeGrenadeProjectile smoke = new CSmokeGrenadeProjectile(projectile.Handle);

                            Utils.ServerMessage($"IsLive {smoke.IsLive}");
                            Utils.ServerMessage($"SmokeDetonationPos {smoke.SmokeDetonationPos}");
                            Utils.ServerMessage($"Spawnflags {smoke.Spawnflags}");
                            Utils.ServerMessage($"CurrentThinkContext {smoke.CurrentThinkContext}");
                            Utils.ServerMessage($"DetonateTime {smoke.DetonateTime}");
                            Utils.ServerMessage($"Effects {smoke.Effects}");
                            Utils.ServerMessage($"ChoreoServices {smoke.ChoreoServices}");
                            Utils.ServerMessage($"CRenderComponent {smoke.CRenderComponent}");
                            Utils.ServerMessage($"CurrentThinkContext {smoke.CurrentThinkContext}");
                            Utils.ServerMessage($"DecalPosition {smoke.DecalPosition}");
                            Utils.ServerMessage($"ExplodeEffectTickBegin {smoke.ExplodeEffectTickBegin}");
                            Utils.ServerMessage($"LifeState {smoke.LifeState}");


                        }

                        ProjectileSnapshot tmpSnapshot = new ProjectileSnapshot(playerPosition.ToVector3(), projectile.InitialPosition.ToVector3(), playerAngle.ToVector3(), projectile.InitialVelocity.ToVector3(), name, description, type);
                        LastThrownGrenade.SetOrAdd(player.SteamID, tmpSnapshot);
                    }
                    
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
                
               CSmokeGrenadeProjectile projectile =  Utilities.GetEntityFromIndex<CSmokeGrenadeProjectile>(@event.Entityid);
                if(projectile != null)
                {
                    Utils.ServerMessage($"Smoke thrown by {ChatColors.Blue}{@event.Userid.PlayerName}{ChatColors.White} took {ChatColors.Green}{(DateTime.Now - result).TotalSeconds.ToString("0.00")}{ChatColors.White}s and {ChatColors.Green}{projectile.Bounces}{ChatColors.White} bounces to detonate.");
                    Utils.ServerMessage($"IsLive {projectile.IsLive}");
                    Utils.ServerMessage($"SmokeDetonationPos {projectile.SmokeDetonationPos}");
                    Utils.ServerMessage($"AbsOrigin {projectile.AbsOrigin}");
                    Utils.ServerMessage($"Spawnflags {projectile.Spawnflags}");
                    Utils.ServerMessage($"CurrentThinkContext {projectile.CurrentThinkContext}");
                    Utils.ServerMessage($"DetonateTime {projectile.DetonateTime}");
                    Utils.ServerMessage($"Effects {projectile.Effects}");
                    Utils.ServerMessage($"ChoreoServices {projectile.ChoreoServices}");
                    Utils.ServerMessage($"CRenderComponent {projectile.CRenderComponent}");
                    Utils.ServerMessage($"DecalPosition {projectile.DecalPosition}");
                    Utils.ServerMessage($"ExplodeEffectTickBegin {projectile.ExplodeEffectTickBegin}");
                    Utils.ServerMessage($"LifeState {projectile.LifeState}");
                    nadeToSpectate = 0;
                }
                else
                {
                    Utils.ServerMessage($"Smoke thrown by {@event.Userid.PlayerName} took {(DateTime.Now - result).TotalSeconds.ToString("0.00")}s to detonate");
                }
                
            }
            return HookResult.Continue;
        }


        public void SaveLastGrenade(CCSPlayerController playerController, string name)
        {
            if(!LastThrownGrenade.TryGetValue(playerController.SteamID, out ProjectileSnapshot? projectile))
            {
                return;
            }
            projectile.Title = name;
            CurrentProjectileStorage.Add(projectile);
            playerController.PrintToCenter($"Successfully added grenade {name}");
            LastThrownGrenade.Remove(playerController.SteamID);
        }

        public void ReThrow(CCSPlayerController player)
        {
            Utils.ServerMessage($"player: {player.SteamID}");
            Utils.ServerMessage($"keycount: {LastThrownGrenade.Keys.Count}");
            foreach (var key in LastThrownGrenade.Keys)
            {
                Utils.ServerMessage($"key: {key}");
            }
            if(!LastThrownGrenade.ContainsKey(player.SteamID))
            {
                Utils.ServerMessage("Could not get last thrown nade");
                return;
            }
            if(!LastThrownGrenade.TryGetValue(player.SteamID, out var grenade))
            {
                Utils.ServerMessage("Could not get last thrown nade1");
                return;
            }
            if(grenade == null)
            {
                Utils.ServerMessage("Could not get last thrown nade2");
                return;
            }

            CBaseCSGrenadeProjectile? cGrenade = null;

            switch(grenade.GrenadeType_T)
            {
                case GrenadeType_t.GRENADE_TYPE_EXPLOSIVE: 
                    {
                        cGrenade = Utilities.CreateEntityByName<CHEGrenadeProjectile>(DesignerNames.ProjectileHE);
                        break;
                    }
                case GrenadeType_t.GRENADE_TYPE_FLASH:
                    {
                        cGrenade = Utilities.CreateEntityByName<CFlashbangProjectile>(DesignerNames.ProjectileFlashbang);
                        break;
                    }
                case GrenadeType_t.GRENADE_TYPE_SMOKE:
                    {
                        cGrenade = Utilities.CreateEntityByName<CSmokeGrenadeProjectile>(DesignerNames.ProjectileSmoke);
                        cGrenade!.IsSmokeGrenade = true;
                        break;
                    }
                case GrenadeType_t.GRENADE_TYPE_FIRE:
                    {
                        cGrenade = Utilities.CreateEntityByName<CMolotovProjectile>(DesignerNames.ProjectileMolotov);
                        cGrenade.SetModel("weapons/models/grenade/incendiary/weapon_incendiarygrenade.vmdl");
                        break;
                    }
                case GrenadeType_t.GRENADE_TYPE_DECOY:
                    {
                        cGrenade = Utilities.CreateEntityByName<CDecoyProjectile>(DesignerNames.ProjectileDecoy);
                        break;
                    }
                    default: 
                    {
                        cGrenade = Utilities.CreateEntityByName<CSmokeGrenadeProjectile>(DesignerNames.ProjectileSmoke);
                        break;
                    }
            }
            if (cGrenade == null)
            {
                Server.PrintToConsole("grenade entity is  null");
                return;
            }
            cGrenade.Elasticity = 0.33f;
            cGrenade.IsLive = false;
            cGrenade.DmgRadius = 350.0f;
            cGrenade.Teleport(grenade.ProjectilePosition.ToCSVector(), grenade.PlayerAngle.ToCSQAngle(), grenade.Velocity.ToCSVector());

            cGrenade.DispatchSpawn();
            cGrenade.Globalname = "custom";
            cGrenade.AcceptInput("FireUser1", player, player, "");
            cGrenade.AcceptInput("InitializeSpawnFromWorld", null, null, "");
            cGrenade.TeamNum = player.TeamNum;
            cGrenade.Thrower.Raw = player.PlayerPawn.Raw;
            cGrenade.OwnerEntity.Raw = player.PlayerPawn.Raw;
            SelfThrownGrenade.Add(cGrenade);
            Utils.ClientChatMessage("Rethrowing your last grenade.", player);
        }

        public void OnTick()
        {
            for (int i =0;i<SelfThrownGrenade.Count;i++) 
            {
                CBaseCSGrenadeProjectile? projectile = SelfThrownGrenade[i];
                if (projectile == null || !projectile.IsValid) 
                {
                    SelfThrownGrenade.RemoveAt(i);
                    i--;
                    continue;
                }              
                //Smoke projectiles are somewhat special since they need some extra manipulation
                if(projectile.IsSmokeGrenade) 
                {
                    CSmokeGrenadeProjectile? cSmoke = new CSmokeGrenadeProjectile(projectile.Handle);
                    if (cSmoke == null)
                    {
                        SelfThrownGrenade.RemoveAt(i);
                        i--;
                        continue;
                    }
                    if (cSmoke.AbsVelocity.X == 0.0f && cSmoke.AbsVelocity.Y == 0.0f && cSmoke.AbsVelocity.Z == 0.0f)
                    {                       
                        cSmoke.SmokeEffectTickBegin = Server.TickCount + 1;
                        cSmoke.SmokeDetonationPos.X = cSmoke.AbsOrigin.X;
                        cSmoke.SmokeDetonationPos.Y = cSmoke.AbsOrigin.Y;
                        cSmoke.SmokeDetonationPos.Z = cSmoke.AbsOrigin.Z;
                        cSmoke.DidSmokeEffect = true;
                        Utilities.SetStateChanged(cSmoke, "CSmokeGrenadeProjectile", "m_nSmokeEffectTickBegin");
                        Utilities.SetStateChanged(cSmoke, "CSmokeGrenadeProjectile", "m_vSmokeDetonationPos");
                        Utilities.SetStateChanged(cSmoke, "CSmokeGrenadeProjectile", "m_bDidSmokeEffect");
                        CSPraccPlugin.Instance!.AddTimer(17.0f, () => cSmoke.Remove());                        
                        SelfThrownGrenade.RemoveAt(i);
                        i--;
                        continue;
                    }
                }
                else 
                {            
                    //Non Smoke projectiles like HE, Flash or Molotov can be removed, does not need extra attention
                    SelfThrownGrenade.RemoveAt(i);
                    i--;
                }
            }
        }

        private void DetonateSmoke(int smokeid)
        {
            CSmokeGrenadeProjectile? smokeGrenadeProjectile = Utilities.GetEntityFromIndex<CSmokeGrenadeProjectile>(nadeToSpectate);
            if (nadeToSpectate == null)
            {
                return;
            }
            smokeGrenadeProjectile.SmokeDetonationPos.X = smokeGrenadeProjectile.AbsOrigin.X;
            smokeGrenadeProjectile.SmokeDetonationPos.Y = smokeGrenadeProjectile.AbsOrigin.Y;
            smokeGrenadeProjectile.SmokeDetonationPos.Z = smokeGrenadeProjectile.AbsOrigin.Z;
            smokeGrenadeProjectile.DidSmokeEffect = true;
        }

        public void Flash(CCSPlayerController? player)
        {
            if (player == null || !player.IsValid)
            {
                return;
            }

            if (!FlashPosition.ContainsKey(player.SteamID))
            {
                FlashPosition.Add(player.SteamID,new Position(player.PlayerPawn.Value.CBodyComponent.SceneNode.AbsOrigin.Copy(),player.PlayerPawn.Value.EyeAngles.Copy()));
            }
            else
            {
                FlashPosition[player.SteamID] = new Position(player.PlayerPawn.Value.CBodyComponent.SceneNode.AbsOrigin.Copy(), player.PlayerPawn.Value.EyeAngles.Copy());
            }

        }

        public void Stop(CCSPlayerController? player)
        {
            if (player == null || !player.IsValid)
            {
                return;
            }
            if (FlashPosition.ContainsKey(player.SteamID)) 
            { 
                FlashPosition.Remove(player.SteamID);
            }
        }

        private void TpToFlashPos(CCSPlayerController? player)
        {
            if (player == null || !player.IsValid)
            {
                return;
            }

            if(!FlashPosition.TryGetValue(player.SteamID,out Position? pos))
            {
                return;
            }
            if (pos == null) return;

            player.PlayerPawn.Value!.Teleport(pos.PlayerPosition, pos.PlayerAngle, new Vector(0,0,0));
        }

        public void NoFlash(CCSPlayerController? player)
        {
            if (player == null || !player.IsValid)
            {
                return;
            }
            if(!NoFlashList.Contains(player.SteamID))
            {
                NoFlashList.Add(player.SteamID);
                Server.NextFrame(() => player.PlayerPawn.Value.FlashMaxAlpha = 0.5f);
                player.HtmlMessage($"No flash: <font color='#008000'>enabled</font>", 2);
                
            }
            else
            {
                NoFlashList.Remove(player.SteamID);
                player.HtmlMessage("No flash: <font color='#008000'>disabled</font>", 2);
            }
        }


    }
}
