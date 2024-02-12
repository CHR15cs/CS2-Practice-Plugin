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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using static CounterStrikeSharp.API.Core.BasePlugin;
using static CSPracc.DataModules.Enums;

namespace CSPracc.Modes
{
    public class PracticeMode : BaseMode
    {
        BotReplayManager? BotReplayManager;    
        ProjectileManager? projectileManager;
        PracticeBotManager? PracticeBotManager;
        PracticeSpawnManager? SpawnManager;
        PlayerBlindManager? PlayerBlindManager;
        PlayerHurtManager? PlayerHurtManager;
        ToggleImpactManager? ToggleImpactManager;
        BreakEntitiesManager? BreakEntitiesManager;
        public PracticeMode(CSPraccPlugin plugin) : base(plugin)
        {     
        }
        public override void ConfigureEnvironment()
        {
            SetupManagers();
            DataModules.Constants.Methods.MsgToServer("Loading practice mode.");
            Server.ExecuteCommand("exec CSPRACC\\pracc.cfg");         
        }

        private void SetupManagers()
        {
            projectileManager = new ProjectileManager(ref CommandManager,ref GuiManager, ref Plugin);
            PracticeBotManager = new PracticeBotManager(ref CommandManager,ref Plugin);
            SpawnManager = new PracticeSpawnManager(ref base.CommandManager);
            BotReplayManager = new BotReplayManager(ref PracticeBotManager, ref projectileManager, ref CommandManager,ref GuiManager);
            PlayerBlindManager = new PlayerBlindManager(ref Plugin, ref projectileManager);
            PlayerHurtManager = new PlayerHurtManager(ref Plugin);
            ToggleImpactManager = new ToggleImpactManager(ref CommandManager);
            BreakEntitiesManager = new BreakEntitiesManager(ref CommandManager);
        }
        public override void Dispose()
        {
            projectileManager!.Dispose();
            PracticeBotManager!.Dispose();
            SpawnManager!.Dispose();
            BotReplayManager!.Dispose();
            PlayerBlindManager!.Dispose();
            PlayerHurtManager!.Dispose();
            Server.ExecuteCommand("exec CSPRACC\\undo_pracc.cfg");
            projectileManager.Dispose();          
            base.Dispose();
        }
    }
}
