using CounterStrikeSharp.API.Core;
using CSPracc.DataModules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSPracc.Managers.BaseManagers.ModeSwitchFolder
{
    public class ModeMenuGenerator
    {
        public static HtmlMenu GetModeMenu(CSPraccPlugin plugin) 
        {
            List<KeyValuePair<string, Action>> list = new List<KeyValuePair<string, Action>>();
            list.Add(new KeyValuePair<string, Action>("Standard", new Action(() => plugin.SwitchMode(Enums.PluginMode.Base))));
            list.Add(new KeyValuePair<string, Action>("Pracc", new Action(() => plugin.SwitchMode(Enums.PluginMode.Pracc))));
            list.Add(new KeyValuePair<string, Action>("Match", new Action(() => plugin.SwitchMode(Enums.PluginMode.Match))));
            list.Add(new KeyValuePair<string, Action>("Dryrun", new Action(() => plugin.SwitchMode(Enums.PluginMode.DryRun))));
            //list.Add(new KeyValuePair<string, Action>("Retake", new Action(() => CSPraccPlugin.SwitchMode(Enums.PluginMode.Retake))));
            //list.Add(new KeyValuePair<string, Action>("Prefire", new Action(() => CSPraccPlugin.SwitchMode(Enums.PluginMode.Prefire))));
            return new HtmlMenu("Select Mode", list);
        }
    }
}
