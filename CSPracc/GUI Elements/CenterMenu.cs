using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Memory;
using CounterStrikeSharp.API.Modules.Utils;
using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace CSPracc.GUI_Elements
{
    public class CenterMenu
    {
        public int SelectedIndex = -1;
        public string Title { get; init; }
        List<KeyValuePair<int,CenterMenuOption>> Options;
        public CenterMenu(string title) 
        { 
            Title = title;
            Options = new List<KeyValuePair<int,CenterMenuOption>>();
        }

        public void AddCenterMenuOption(CenterMenuOption option) 
        {
            int index = GetNewIndex();
            option.SetIndex(index);
            Options.Add(new KeyValuePair<int, CenterMenuOption>(index,option));
        }

        private int GetNewIndex()
        {
            return Options.Count + 1;
        }

        public void ShowMenu(CCSPlayerController player)
        {
            Task.Run(() =>
            {
                string menuString = "";
                menuString += $"{Title}\u2029";
                menuString += $"-----------\u2029";
                //foreach (KeyValuePair<int, CenterMenuOption> option in Options)
                //{
                //    menuString += $".{option.Key} {option.Value.Text}";
                //} 
               
                for (int i = 0; i < Options.Count; i++)
                {
                    menuString += $".{Options[i].Key} \t{Options[i].Value.Text}";
                    if (i + 1 < Options.Count)
                    {
                        menuString += "\u2029";
                    }
                }
                //player.PrintToCenterHtml(menuString);

            
                while (SelectedIndex == -1)
                {
                    int optionIndex = 0;
                    player.PrintToCenterHtml(menuString);
                    Thread.Sleep(250);
                }

                Server.PrintToConsole("out of loop");
                foreach(KeyValuePair<int, CenterMenuOption> option in Options)
                {
                    if(option.Key == SelectedIndex)
                    {
                        Server.PrintToConsole("found key");
                        option.Value.RunTask();
                    }
                }
            });
        }
    }
}
