using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Cvars;
using CounterStrikeSharp.API.Modules.Entities;
using CounterStrikeSharp.API.Modules.Utils;
using CSPracc.CommandHandler;
using CSPracc.DataModules;
using CSPracc.DataModules.Constants;
using CSPracc.EventHandler;
using CSPracc.Extensions;
using CSPracc.Managers;
using CSPracc.Managers.PracticeManagers;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static CounterStrikeSharp.API.Core.BasePlugin;
using static CSPracc.DataModules.Enums;

namespace CSPracc.Modes
{
    public class PracticeMode : BaseMode
    {
        List<IManager> _managers;
        BotReplayManager? BotReplayManager;    
        ProjectileManager? projectileManager;
        PracticeBotManager? PracticeBotManager;
        PracticeSpawnManager? SpawnManager;
        PlayerBlindManager? PlayerBlindManager;
        PlayerHurtManager? PlayerHurtManager;
        ToggleImpactManager? ToggleImpactManager;
        BreakEntitiesManager? BreakEntitiesManager;
        TimerManager? TimerManager;
        CountdownManager? CountdownManager;
        public PracticeMode(CSPraccPlugin plugin) : base(plugin)
        {
            _managers = new List<IManager>();
        }
        public override void ConfigureEnvironment()
        {
            SetupManagers();
            _managers.ForEach(m => m.RegisterCommands());
            DataModules.Constants.Methods.MsgToServer("Loading practice mode.");
            Server.ExecuteCommand("exec CSPRACC\\pracc.cfg");         
        }

        private void SetupManagers()
        {
            projectileManager = new ProjectileManager();            
            PracticeBotManager = new PracticeBotManager();
            SpawnManager = new PracticeSpawnManager();
            BotReplayManager = new BotReplayManager(ref PracticeBotManager, ref projectileManager);
            PlayerBlindManager = new PlayerBlindManager(ref Plugin, ref projectileManager, ref CommandManager);
            PlayerHurtManager = new PlayerHurtManager(ref Plugin, ref CommandManager);
            ToggleImpactManager = new ToggleImpactManager(ref CommandManager);
            BreakEntitiesManager = new BreakEntitiesManager(ref CommandManager);
            TimerManager = new TimerManager(ref CommandManager, ref GuiManager);
            CountdownManager = new CountdownManager(ref CommandManager, ref GuiManager);

            _managers.Add(projectileManager);
            _managers.Add(PracticeBotManager);
            _managers.Add(SpawnManager);
            _managers.Add(BotReplayManager);
            _managers.Add(PlayerBlindManager);
            _managers.Add(PlayerHurtManager);
            _managers.Add(ToggleImpactManager);
            _managers.Add(BreakEntitiesManager);
            _managers.Add(TimerManager);
            _managers.Add(CountdownManager);
        }

        public override void Dispose()
        {
            _managers.ForEach(m => m.Dispose());
            Server.ExecuteCommand("exec CSPRACC\\undo_pracc.cfg");         
            base.Dispose();
        }
    }
}
