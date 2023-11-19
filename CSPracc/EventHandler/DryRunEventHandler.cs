using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using CSPracc.CommandHandler;
using CSPracc.DataModules;
using CSPracc.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSPracc.EventHandler
{
    public class DryRunEventHandler : MatchEventHandler
    {
        public DryRunEventHandler(CSPraccPlugin plugin, DryRunCommandHandler mch,DryRunMode mode) : base(plugin, mch,mode)
        {

        }

        public override void Dispose()
        {         
            base.Dispose();
        }
    }
}
