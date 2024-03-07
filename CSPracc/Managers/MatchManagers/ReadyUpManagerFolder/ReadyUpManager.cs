using CounterStrikeSharp.API.Core;
using CSPracc.Managers.BaseManagers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSPracc.Managers.MatchManagers.ReadyUpManagerFolder
{
    public class ReadyUpManager : BaseManager
    {
        public ReadyUpManager(ref CommandManager commandManager) : base(ref commandManager)
        {
        }

        private bool ReadyCommandHandler(CCSPlayerController player, List<string> commands)
        {

        }
    }
}
