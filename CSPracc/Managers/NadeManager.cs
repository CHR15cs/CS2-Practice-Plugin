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

namespace CSPracc
{
    public static class NadeManager
    {
        public static Dictionary<CCSPlayerController, SavedNade> LastThrownGrenade = new Dictionary<CCSPlayerController, SavedNade>();

        private static ChatMenu _nadeMenu = null;
        /// <summary>
        /// Grenade chat menu
        /// </summary>
        public static ChatMenu NadeMenu
        {
            get
            {
                var NadesMenu = new ChatMenu("Nade Menu");
                var handleGive = (CCSPlayerController player, ChatMenuOption option) => TeleportPlayer(player, option.Text);

                NadesMenu.AddMenuOption($" {ChatColors.Green}Global saved nades:", handleGive, true);
                foreach (var nade in Nades)
                {
                    if (nade.Map == Server.MapName)
                    {
                        NadesMenu.AddMenuOption($" {ChatColors.Green}{nade.Title} ID:{nade.ID}", handleGive);
                    }
                }
                return NadesMenu;
            }
        }

        public static ChatMenu GetNadeMenu(CCSPlayerController player)
        {
            ChatMenu menu = NadeMenu;
            
            if (LastThrownGrenade.TryGetValue(player,out SavedNade savedNade))
            {
                var handleGive = (CCSPlayerController player, ChatMenuOption option) => TeleportPlayer(player, option.Text);
                menu.AddMenuOption($" {ChatColors.Red}Personal nades:", handleGive, true);
                
                menu.AddMenuOption($" {ChatColors.Red}unsaved nade ID:-1", handleGive);
            }
            return menu;
        }

        private static List<CSPracc.DataModules.SavedNade>? _nades = null;
        /// <summary>
        /// Stored nades
        /// </summary>
        public static List<CSPracc.DataModules.SavedNade>? Nades
        {
            get
            {
                if(_nades == null)
                {
                    _nades = new List<SavedNade> {};
                }
                return _nades;
            }
            set { _nades = value; }
        }
        /// <summary>
        /// Teleport player to grenade position
        /// </summary>
        /// <param name="player">player to teleport</param>
        /// <param name="grenadeName">grenade destination</param>
        private static void TeleportPlayer(CCSPlayerController player, string grenadeName)
        {
            foreach (var nade in Nades)
            {
                if (nade.Map == Server.MapName)
                {
                    string idofNade = grenadeName.Substring(grenadeName.IndexOf(":")+1);
                    if(! int.TryParse(idofNade,out int id))
                     {
                        player.PrintToCenter($"Could not find nade {idofNade}");
                        return;
                    }
                    if(id == -1)
                    {
                        player.PlayerPawn.Value.Teleport(LastThrownGrenade[player].PlayerPosition, new QAngle(LastThrownGrenade[player].PlayerAngle.X, LastThrownGrenade[player].PlayerAngle.Y, LastThrownGrenade[player].PlayerAngle.Z), LastThrownGrenade[player].Velocity);
                    }
                    if (nade.ID == id)
                    {
                        player.PlayerPawn.Value.Teleport(nade.PlayerPosition, new QAngle(nade.PlayerAngle.X, nade.PlayerAngle.Y, nade.PlayerAngle.Z), nade.Velocity);
                    }
                }
            }
        }

        /// <summary>
        /// Add grenade to the list
        /// </summary>
        /// <param name="player">palyer who issued the command</param>
        /// <param name="args">Arguments shall look like <Name> <Description></param>
        public static void AddGrenade(CCSPlayerController player,string args)
        {
            if (player == null) return;
            if (args == String.Empty) return;
            var absOrigin = player.PlayerPawn.Value.CBodyComponent!.SceneNode!.AbsOrigin;
            string name = args;
            Nades?.Add(new SavedNade(absOrigin, player.PlayerPawn.Value.EyeAngles, null, name, "", Server.MapName,Nades.Count +1));
            player.PrintToCenter($"Successfully added grenade {name}");
            CSPraccPlugin.WriteConfig(CSPraccPlugin.Config);
        }

        /// <summary>
        /// Add grenade to the list
        /// </summary>
        /// <param name="player">palyer who issued the command</param>
        /// <param name="args">Arguments shall look like <Name> <Description></param>
        public static void RemoveGrenade(CCSPlayerController player, string args)
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
            bool foundnade = false; 
            foreach(var nade in Nades)
            {
                if(nade.ID == id)
                {
                    Nades.Remove(nade);
                    player.PrintToCenter("Successfully removed nade");
                    foundnade = true;
                    break;
                }
            }
            if(!foundnade)
            {
                player.PrintToCenter($"Could not find nade with id {id}");
            }
            CSPraccPlugin.WriteConfig(CSPraccPlugin.Config);
        }

        public static void OnEntitySpawned(CEntityInstance entity)
        {
            var designerName = entity.DesignerName;
            if (Match.CurrentMode != Enums.PluginMode.Pracc) return;

            if (designerName.Contains("_projectile"))
            {
                Logging.LogMessage("Its a projectile!!!!!!!");
                CBaseCSGrenadeProjectile projectile = new CBaseCSGrenadeProjectile(entity.Handle);
                Logging.LogMessage($"Its a {projectile.DesignerName}");

                Server.NextFrame(() =>
                {
                    CCSPlayerController player = new CCSPlayerController(projectile.Thrower.Value.Controller.Value.Handle);
                    Logging.LogMessage($"thrower : {player.PlayerName}");
                    SavedNade last = new SavedNade(player.PlayerPawn.Value.CBodyComponent!.SceneNode!.AbsOrigin, player.PlayerPawn.Value.EyeAngles, null, "temp", "", Server.MapName, -1);
                    Logging.LogMessage("generated temp nade");
                    if (LastThrownGrenade.ContainsKey(player))
                    {
                        LastThrownGrenade[player] = last;
                    }
                    else
                    {
                        LastThrownGrenade.Add(player, last);
                    }

                });

            }

            if (designerName == "smokegrenade_projectile")
            {
                var projectile = new CSmokeGrenadeProjectile(entity.Handle);

                Server.NextFrame(() =>
                {
                    CCSPlayerController player = new CCSPlayerController(projectile.Thrower.Value.Controller.Value.Handle);
                    projectile.SmokeColor.X = (float)Utils.GetTeamColor(player).R;
                    projectile.SmokeColor.Y = (float)Utils.GetTeamColor(player).G;
                    projectile.SmokeColor.Z = (float)Utils.GetTeamColor(player).B;
                    Logging.LogMessage($"smoke color {projectile.SmokeColor}");
                });
            }
        }

        public static void SaveLastGrenade(CCSPlayerController playerController,string newName)
        {
            if(!LastThrownGrenade.TryGetValue(playerController, out SavedNade nade))
            {
                return;
            }
            nade.Title = newName;
            nade.ID = Nades.Count + 1;
            Nades.Add(nade);
            playerController.PrintToCenter($"Successfully added grenade {newName}");
            LastThrownGrenade.Remove(playerController);
            CSPraccPlugin.WriteConfig(CSPraccPlugin.Config);
        }
    }
}
