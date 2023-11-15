using CounterStrikeSharp.API;
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
    public class PracticeMode : BaseMode
    {
        public PracticeMode() : base() 
        {
        }
        public override void ConfigureEnvironment()
        {
            DataModules.consts.Methods.MsgToServer("Starting practice mode.");
            Server.ExecuteCommand("exec CSPRACC\\pracc.cfg");
            EventHandler?.Dispose();
            EventHandler = new PracticeEventHandler(CSPraccPlugin.Instance!, new PracticeCommandHandler());
        }
    }
}
