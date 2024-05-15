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
using CounterStrikeSharp.API.Modules.Entities;
using CSPracc.Managers.MatchManagers.ReadyUpManagerFolder;
using CSPracc.Managers.MatchManagers;

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
            SetupWarmup();

        }

        private void SetupLiveMode()
        {
            CoachManager coachManager = new CoachManager();
            matchManagers.Add(coachManager);
            state = match_state.live;
            Utils.ExecuteConfig("5on5.cfg");
        }

        private void StopLiveMode()
        {
            foreach (IManager manager in matchManagers)
            {
                manager.Dispose();
            }
        }

        private void SetupWarmup()
        {
            ReadyUpManager readyUpManager = new ReadyUpManager();
            readyUpManager.TeamsReady += HandleAllPlayerReady;
            warmupManagers.Add(readyUpManager);
            state = match_state.warmup;
        }

        private void StopWarmup()
        {
            foreach (IManager manager in warmupManagers)
            {
                manager.Dispose();
            }
        }

        /// <inheritdoc/>
        public override void Dispose()
        {
            
        }
       
        /// <summary>
        /// Event to be called when all players are ready
        /// </summary>
        /// <param name="sender">readyup manager</param>
        /// <param name="event">info</param>
        public void HandleAllPlayerReady(object? sender, EventArgs @event)
        {
            StopWarmup();
            Utils.ServerMessage("All players are ready, starting match");
            SetupLiveMode();
        }

    }
}
