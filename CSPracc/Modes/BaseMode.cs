using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSPracc.EventHandler;
using CSPracc.CommandHandler;

namespace CSPracc.Modes
{
    public class BaseMode : IDisposable
    {
        public BaseMode() 
        {            
        }
        protected  BaseEventHandler EventHandler { get; set; }
        public  virtual void ConfigureEnvironment()
        {
            DataModules.Constants.Methods.MsgToServer("Restoring default config.");
            Server.ExecuteCommand("exec CSPRACC\\undo_pracc.cfg");
            Server.ExecuteCommand("exec server.cfg");
            EventHandler = new BaseEventHandler(CSPraccPlugin.Instance!, new BaseCommandHandler());
        }
        public static void ChangeMap(CCSPlayerController player, string mapName)
        {
            if (player == null) return;
            if (!player.PlayerPawn.IsValid) { return; }
            if (!player.IsAdmin()) { player.PrintToCenter("Only admins can execute this command!"); return; }
            if (mapName == null) return;
            if (mapName.Length == 0) return;
            if (!mapName.StartsWith("de_"))
            {
                mapName = "de_" + mapName;
            }
            Server.ExecuteCommand($"say Changing map to {mapName}");
            Server.ExecuteCommand($"changelevel {mapName}");

        }
        public virtual void Dispose()
        {        
            EventHandler?.Dispose();
        }

    }


}
