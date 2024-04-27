using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CounterStrikeSharp.API.Modules.Utils;
using System.Text.RegularExpressions;
using CSPracc.DataModules;
using System.IO;
using CSPracc.Managers;
using CSPracc.DataModules.Constants;
using CSPracc.EventHandler;
using CSPracc.CommandHandler;
using CSPracc.Modes;
using static CSPracc.DataModules.Enums;
using CounterStrikeSharp.API.Modules.Cvars;
using CounterStrikeSharp.API.Modules.Entities;

namespace CSPracc
{
    /// <summary>
    /// Class for handling match mode
    /// </summary>
    public  class MatchMode : BaseMode
    {        
        protected  DataModules.Enums.match_state state = DataModules.Enums.match_state.warmup;
        private List<IManager> warmupManagers { get; set; } = new List<IManager>();
        private List<IManager> matchManagers { get; set; } = new List<IManager>();

        /// <summary>
        /// Constructor for the match mode
        /// </summary>
        public MatchMode() : base()
        {

        }

/// <inheritdoc/>
        public override void Dispose()
        {
            
        }
       
    }
}
