using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Entities;
using CounterStrikeSharp.API.Modules.Entities.Constants;
using CounterStrikeSharp.API.Modules.Memory;
using CounterStrikeSharp.API.Modules.Utils;
using CSPracc.DataModules;
using CSPracc.DataStorages.JsonStorages;
using CSPracc.Extensions;
using CSPracc.Managers;
using CSPracc.Managers.PrefireManagers;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CSPracc.DataModules.Enums;

namespace CSPracc.Modes
{
    public class PrefireMode : BaseMode
    {        
        PrefireRouteManager PrefireRouteManager { get; set; }

        GunManager GunManager { get; set; }
        PrefireRouteEditor PrefireRouteEditor { get; set; }

        public PrefireMode() : base()
        {
            GunManager = new GunManager();
            //PrefireRouteManager = new PrefireRouteManager(ref CSPraccPlugin.Instance);
        }

        public override void ConfigureEnvironment()
        {
            DataModules.Constants.Methods.MsgToServer("Loading prefire mode.");
            Server.ExecuteCommand("exec CSPRACC\\undo_pracc.cfg");
            Server.ExecuteCommand("exec CSPRACC\\prefire.cfg");
            Utils.ServerMessage($"Use {ChatColors.Green}.routes{ChatColors.White} to show menu of routes.");
            Utils.ServerMessage($"Use {ChatColors.Green}.addroute (routename){ChatColors.White} to add a empty route.");
            Utils.ServerMessage($"Use {ChatColors.Green}.editroute (routename){ChatColors.White} to edit given route.");
            Utils.ServerMessage($"Use {ChatColors.Green}.addspawn{ChatColors.White} to add spawn to current route.");
            Utils.ServerMessage($"Use {ChatColors.Green}.startpoint{ChatColors.White} to set startpoint of current route.");
            Utils.ServerMessage($"Use {ChatColors.Green}.next{ChatColors.White} to select next route of current map.");
            Utils.ServerMessage($"Use {ChatColors.Green}.back{ChatColors.White} to go back to last route.");
        }
    }
}
