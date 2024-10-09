using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSPracc.EventHandler;
using CSPracc.CommandHandler;
using CSPracc.Managers;
using CSPracc.DataModules;

namespace CSPracc.Modes
{
    public class BaseMode : IDisposable
    {
        HtmlMenu ModeMenu { get; set; }
        public BaseMode() 
        {
            GuiManager = new GuiManager();
            List<KeyValuePair<string,Action>> list = new List<KeyValuePair<string, Action>>();
            list.Add(new KeyValuePair<string, Action>("Standard", new Action(() => CSPraccPlugin.SwitchMode(Enums.PluginMode.Base))));
            list.Add(new KeyValuePair<string, Action>("Pracc", new Action(() =>CSPraccPlugin.SwitchMode(Enums.PluginMode.Pracc))));
            list.Add(new KeyValuePair<string, Action>("Match", new Action(() => CSPraccPlugin.SwitchMode(Enums.PluginMode.Match))));
            list.Add(new KeyValuePair<string, Action>("Dryrun", new Action(() => CSPraccPlugin.SwitchMode(Enums.PluginMode.DryRun))));
            //list.Add(new KeyValuePair<string, Action>("Retake", new Action(() => CSPraccPlugin.SwitchMode(Enums.PluginMode.Retake))));
            //list.Add(new KeyValuePair<string, Action>("Prefire", new Action(() => CSPraccPlugin.SwitchMode(Enums.PluginMode.Prefire))));
            ModeMenu = new HtmlMenu("Select Mode", list);
        }

        protected GuiManager GuiManager { get; set; }

        protected  BaseEventHandler EventHandler { get; set; }
        public  virtual void ConfigureEnvironment(bool hotReload = true)
        {
            if(hotReload)
            {
                DataModules.Constants.Methods.MsgToServer("Restoring default config.");
                Server.ExecuteCommand("exec CSPRACC\\undo_pracc.cfg");
                Server.ExecuteCommand("exec server.cfg");
            }
            EventHandler = new BaseEventHandler(CSPraccPlugin.Instance!, new BaseCommandHandler(this));           
        }
        public static void ChangeMap(CCSPlayerController player, string mapName)
        {
            if (player == null) return;
            if (!player.PlayerPawn.IsValid) { return; }
            if (!player.IsAdmin()) { player.PrintToCenter("Only admins can execute this command!"); return; }
            if (mapName == null) return;
            if (mapName.Length == 0) return;
            if (!mapName.StartsWith("de_"))
            {
                mapName = "de_" + mapName;
            }
            Utils.ServerMessage($"Changing map in {CSPraccPlugin.Instance!.Config!.DelayMapChange}s to {mapName}");
            CSPraccPlugin.Instance!.AddTimer(CSPraccPlugin.Instance!.Config!.DelayMapChange, () => Server.ExecuteCommand($"changelevel {mapName}"));

        }

        public void ShowModeMenu(CCSPlayerController playerController)
        {
            if(playerController == null) return;
            if (!playerController.IsValid) return;
            if(!playerController.IsAdmin()) {  return; }

            GuiManager.AddMenu(playerController.SteamID, ModeMenu);
        }

        public virtual void Dispose()
        {
            GuiManager.Dispose();
            EventHandler?.Dispose();
        }

    }


}
