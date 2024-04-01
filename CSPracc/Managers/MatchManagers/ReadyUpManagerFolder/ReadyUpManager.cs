using CounterStrikeSharp.API.Core;
using CSPracc.Managers.BaseManagers;
using CSPracc.Managers.BaseManagers.CommandManagerFolder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSPracc.Managers.MatchManagers.ReadyUpManagerFolder
{
    public class ReadyUpManager : BaseManager
    {
        public ReadyUpManager() : base()
        {
        }

        private bool ReadyCommandHandler(CCSPlayerController player,PlayerCommandArgument commands)
        {
            return true;
        }
    }
}
