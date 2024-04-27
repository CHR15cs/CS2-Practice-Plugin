using CounterStrikeSharp.API.Core;
using CSPracc.Managers.BaseManagers.CommandManagerFolder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSPracc.Managers.MatchManagers.DamagePrintManagerFolder
{
    /// <summary>
    /// Manager to print damage done to players after the round
    /// </summary>
    public class DamagePrintManager : BaseManagers.BaseManager
    {
        private bool _damagePrintEnabled { get; set; } = false;

        /// <summary>
        /// Constructor registering the command
        /// </summary>
        public DamagePrintManager() : base()
        {
            Commands.Add("damageprint", new DataModules.PlayerCommand("damageprint", "Toggle damage print", DamagePrintCommandHandler, null, null));
        }

        /// <summary>
        /// Toggle Damageprint
        /// </summary>
        /// <param name="controller">Player who issued the command</param>
        /// <param name="argument">arguments passed</param>
        /// <returns>True if successfull</returns>
        private bool DamagePrintCommandHandler(CCSPlayerController controller, PlayerCommandArgument argument)
        {
            _damagePrintEnabled = !_damagePrintEnabled;
            controller.ChatMessage($"Damage print is now {_damagePrintEnabled}");
            return true;
        }
    }
}
