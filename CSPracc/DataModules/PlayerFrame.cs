using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSPracc.DataModules
{
    public class PlayerFrame
    {
        public Position Position {  get; set; }
        public PlayerButtons PlayerButtons { get; set; }

        public CBasePlayerWeapon ActivePlayerWeapon { get; set; }

        public bool shoot = false;

        public PlayerFrame(CCSPlayerController cCSPlayerController,ProjectileSnapshot? projectileSnapshot = null) 
        {
            Position = cCSPlayerController.GetCurrentPosition()!;
            PlayerButtons = cCSPlayerController.Buttons;
            ProjectileSnapshot = projectileSnapshot;
            ActivePlayerWeapon = cCSPlayerController.PlayerPawn.Value.WeaponServices.ActiveWeapon.Value;
        }

        public ProjectileSnapshot? ProjectileSnapshot { get; set; }
    }
}
