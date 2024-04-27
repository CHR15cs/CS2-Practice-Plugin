using CounterStrikeSharp.API.Core;
using CSPracc.DataModules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSPracc.Managers.BaseManagers.ModeSwitchFolder
{
    /// <summary>
    /// Class to generate the mode menu
    /// </summary>
    public class ModeMenuGenerator
    {
        /// <summary>
        /// Get the mode menu
        /// </summary>
        /// <returns>HtmlMenu to display all available modes</returns>
        public static HtmlMenu GetModeMenu() 
        {
            List<KeyValuePair<string, Action>> list = new List<KeyValuePair<string, Action>>();
            list.Add(new KeyValuePair<string, Action>("Standard", new Action(() => CSPraccPlugin.Instance.SwitchMode(Enums.PluginMode.Base))));
            list.Add(new KeyValuePair<string, Action>("Pracc", new Action(() => CSPraccPlugin.Instance.SwitchMode(Enums.PluginMode.Pracc))));
            list.Add(new KeyValuePair<string, Action>("Match", new Action(() => CSPraccPlugin.Instance.SwitchMode(Enums.PluginMode.Match))));
            list.Add(new KeyValuePair<string, Action>("Dryrun", new Action(() => CSPraccPlugin.Instance.SwitchMode(Enums.PluginMode.DryRun))));
            //list.Add(new KeyValuePair<string, Action>("Retake", new Action(() => CSPraccPlugin.SwitchMode(Enums.PluginMode.Retake))));
            //list.Add(new KeyValuePair<string, Action>("Prefire", new Action(() => CSPraccPlugin.SwitchMode(Enums.PluginMode.Prefire))));
            return new HtmlMenu("Select Mode", list);
        }
    }
}
