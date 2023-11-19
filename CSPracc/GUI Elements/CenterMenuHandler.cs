using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSPracc.GUI_Elements
{
    public class CenterMenuHandler
    {
        private static CenterMenuHandler _instance;
        public static CenterMenuHandler Instance
        {
            get
            { 
                if(_instance == null)
                {
                    _instance = new CenterMenuHandler();
                }
                return _instance;
            }
        }

        Dictionary<CCSPlayerController,CenterMenu> CenterMenuDictionary = new Dictionary<CCSPlayerController, CenterMenu>();

        private CenterMenuHandler() { }

        public void Add(CCSPlayerController player, CenterMenu menu) 
        { 
            if(CenterMenuDictionary.ContainsKey(player))
            {
                CenterMenuDictionary[player] = menu;
            }
            else
            {
                CenterMenuDictionary.Add(player, menu);
            }
        }

        public void Command(CCSPlayerController player,string args)
        {
            Server.PrintToConsole($"Reached Command with {args}");
            if (!CenterMenuDictionary.ContainsKey(player)) 
            {
                return;
            }
            if(!int.TryParse(args, out int id))
            {
                return;
            }
            Server.PrintToConsole($"Reached CenterMenuDictionary[player].SelectedIndex = id;");
            Server.PrintToConsole($"id = {id}, {CenterMenuDictionary[player].ToString()}");
            CenterMenuDictionary[player].SelectedIndex = id;
            CSPraccPlugin.Instance.AddTimer(0.5f, () => CenterMenuDictionary.Remove(player));
        }
    }
}
