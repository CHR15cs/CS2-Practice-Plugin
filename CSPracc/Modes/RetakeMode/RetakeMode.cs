using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CSPracc.CommandHandler;
using CSPracc.EventHandler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CSPracc.DataModules.Enums;

namespace CSPracc.Modes
{
    public class RetakeMode : BaseMode
    {
        enum RetakeModeStatus
        {
            edit,
            live
        }


        private RetakeModeStatus status;
        public RetakeMode() : base() 
        {
            status = RetakeModeStatus.live;
        }


        public void LoadRetakeMode(CCSPlayerController player)
        {
            if (!player.IsAdmin()) player.PrintToCenter("Only Admins can execute this command!");

            Utils.ServerMessage("Admin loaded retake mode.");
            status = RetakeModeStatus.live;
            Server.ExecuteCommand("exec CSPRACC\\undo_pracc.cfg");
            Server.ExecuteCommand("exec CSPRACC\\retake.cfg");
            //ToDo Look for players, split up the teams
            loadSpawnForBombsite("A");
        }

        private void loadSpawnForBombsite(string Bombsite)
        {
            SpawnManager.DeleteAllSpawnPoints();
            SpawnManager.LoadSpawnsForBombsite(Bombsite);
        }

        public void LoadEditMode(CCSPlayerController player)
        {
            if (!player.IsAdmin()) player.PrintToCenter("Only Admins can execute this command!");

            Utils.ServerMessage("Admin loaded edit mode.");
            status = RetakeModeStatus.edit;
            Server.ExecuteCommand("exec CSPRACC\\pracc.cfg");
        }

        public void AddSpawn(CCSPlayerController player, string bombsite)
        {
            if (!player.IsAdmin()) player.PrintToCenter("Only Admins can execute this command!");
            SpawnManager.AddCurrentPositionAsSpawnPoint(player, bombsite);
        }

        public override void ConfigureEnvironment()
        {
            DataModules.Constants.Methods.MsgToServer("Loading retakes mode.");
            Server.ExecuteCommand("exec CSPRACC\\pracc.cfg");
            EventHandler?.Dispose();
            EventHandler = new RetakeEventHandler(CSPraccPlugin.Instance!, new RetakeCommandHandler(this));
        }
    }
}
