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
using CSPracc.Modes;
using static CSPracc.DataModules.Enums;
using CounterStrikeSharp.API.Modules.Cvars;

namespace CSPracc
{
    /// <summary>
    /// Class for handling dryrun mode
    /// </summary>
    public  class DryRunMode : MatchMode
    {
        /// <summary>
        /// Constructor for the dryrun mode
        /// </summary>
        public DryRunMode() : base() { }  
    }
}
