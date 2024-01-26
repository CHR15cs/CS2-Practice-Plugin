using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace CSPracc.DataModules
{
    public class PlayerFrame
    {
        public JsonSpawnPoint Position {  get; set; }
        public PlayerButtons PlayerButtons { get; set; }

        public bool shoot { get; set; } = false;

        public PlayerFrame(CCSPlayerController cCSPlayerController, ProjectileSnapshot? projectileSnapshot = null)
        {
            Position = cCSPlayerController.GetCurrentPositionAsJsonSpawnPoint()!;
            PlayerButtons = cCSPlayerController.Buttons;
            ProjectileSnapshot = projectileSnapshot;
        }

        public PlayerFrame() { }


        public ProjectileSnapshot? ProjectileSnapshot { get; set; } = null;
    }
}
