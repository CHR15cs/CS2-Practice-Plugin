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

namespace CSPracc
{
    public static class NadeManager
    {
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

                foreach (var nade in Nades)
                {
                    if (nade.Map == Server.MapName)
                    {
                        NadesMenu.AddMenuOption($"{nade.Title} ID:{nade.ID}", handleGive);
                    }
                }
                return NadesMenu;
            }
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
    }
}
