using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Menu;
using CounterStrikeSharp.API.Modules.Utils;
using CSPracc.DataModules;
using CSPracc.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;

namespace CSPracc.Managers
{
    public class GuiManager : IDisposable
    {
        private static GuiManager? _instance = null;
        public static GuiManager Instance 
        { 
            get
            {
                if(_instance == null)
                {
                    _instance = new GuiManager();
                }
                return _instance;
            }
        
        }

        Dictionary<ulong,DateTime> Timers = new Dictionary<ulong,DateTime>();
        Dictionary<ulong, DateTime> Countdown = new Dictionary<ulong, DateTime>();
        Dictionary<ulong,HtmlMenu> htmlMenus = new Dictionary<ulong, HtmlMenu> ();
        Dictionary<ulong,HtmlMessage> htmlMessage = new Dictionary<ulong, HtmlMessage> ();  

        private GuiManager() 
        {
            CSPraccPlugin.Instance!.RegisterListener<Listeners.OnTick>(OnTick);
            CSPraccPlugin.Instance.AddCommand("css_1", "sel 1", Selection); 
            CSPraccPlugin.Instance.AddCommand("css_2", "sel 1", Selection);
            CSPraccPlugin.Instance.AddCommand("css_3", "sel 1", Selection);
            CSPraccPlugin.Instance.AddCommand("css_4", "sel 1", Selection);
            CSPraccPlugin.Instance.AddCommand("css_5", "sel 1", Selection);
            CSPraccPlugin.Instance.AddCommand("css_6", "sel 1", Selection);
            CSPraccPlugin.Instance.AddCommand("css_7", "sel 1", Selection);
            CSPraccPlugin.Instance.AddCommand("css_8", "sel 1", Selection);
            CSPraccPlugin.Instance.AddCommand("css_9", "sel 1", Selection);
        }

        /// <summary>
        /// Print HTML Message to player
        /// </summary>
        /// <param name="message">Html Message to print</param>
        /// <param name="player">player to print the message for</param>
        public void ShowHtmlMessage(HtmlMessage message,CCSPlayerController player)
        {
            if (message == null) return;
            if (player == null) return;
            if (!player.IsValid) return;

            Server.NextFrame(() =>
            {
                if (htmlMessage.ContainsKey(player.SteamID))
                {
                    htmlMessage[player.SteamID] = message;
                }
                else
                {
                    htmlMessage.Add(player.SteamID, message);
                }
            });
        }

        /// <summary>
        /// Start / Stop Timer
        /// </summary>
        /// <param name="playerController">player who issued the command</param>
        public void StartTimer(CCSPlayerController playerController) 
        {
            if(Timers.ContainsKey(playerController.SteamID))
            {
                Utils.ClientChatMessage($"Timer stopped after {(DateTime.Now - Timers[playerController.SteamID]).TotalSeconds.ToString("0.00")}s", playerController);
                Timers.Remove(playerController.SteamID);               
            }
            else
            {
                Utils.ClientChatMessage($"Timer started", playerController);
                Timers.Add(playerController.SteamID, DateTime.Now);
            }
        }

        /// <summary>
        /// Print countdown to player
        /// </summary>
        /// <param name="playerController">player who issued the command</param>
        /// <param name="time">time in seconds</param>
        public void StartCountdown(CCSPlayerController playerController, int time)
        {
            Utils.ClientChatMessage($"Countdown started", playerController);
            Countdown.Add(playerController.SteamID, DateTime.Now.AddSeconds(time));
        }

        public void AddMenu(ulong id, HtmlMenu menu)
        {
            Server.NextFrame(() =>
            {
                htmlMenus.SetOrAdd(id, menu);
            });

        }

        public void Dispose()
        {
            Listeners.OnTick onTick = new Listeners.OnTick(OnTick);
            CSPraccPlugin.Instance!.RemoveListener("OnTick", onTick);
            CSPraccPlugin.Instance!.RemoveCommand("css_1", Selection);
            CSPraccPlugin.Instance!.RemoveCommand("css_2", Selection);
            CSPraccPlugin.Instance!.RemoveCommand("css_3", Selection);
            CSPraccPlugin.Instance!.RemoveCommand("css_4",  Selection);
            CSPraccPlugin.Instance!.RemoveCommand("css_5",  Selection);
            CSPraccPlugin.Instance!.RemoveCommand("css_6", Selection);
            CSPraccPlugin.Instance!.RemoveCommand("css_7",  Selection);
            CSPraccPlugin.Instance!.RemoveCommand("css_8",  Selection);
            CSPraccPlugin.Instance!.RemoveCommand("css_9", Selection);
        }

        /// <summary>
        /// Draw menu and other stuff
        /// </summary>
        public void OnTick() 
        {
            foreach(var key in htmlMenus.Keys) 
            {
                CCSPlayerController? player = Utilities.GetPlayerFromSteamId(key);
                if(player == null) { htmlMenus.Remove(key); continue; }

                if(!htmlMenus.TryGetValue(key,out HtmlMenu? menu))
                {
                    htmlMenus.Remove (key);
                    continue;
                }
                string menuText = $"<font color=\"green\">{menu.Title}</font><br>";
                if (menu.Options == null) continue;

                if(menu.MenuPages.Count > menu.Page)
                {
                    for (int option = 0; option < menu.MenuPages[menu.Page].Count; option++)
                    {
                        menuText += $"<font color=\"green\">!{option + 1}</font>) {menu.MenuPages[menu.Page][option].Key}";
                        if (option + 1 < menu.MenuPages[menu.Page].Count)
                        {
                            menuText += "<br>";
                        }
                    }
                }              
                
                menuText += "<br>";
                if (menu.Page > 0)
                {
                    menuText += $"<font color=\"green\"> !7</font>) Prev&nbsp;&nbsp;&nbsp;";
                }
                else
                {
                    menuText += "&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;";
                }
                if (menu.MenuPages.Count > menu.Page + 1)
                {
                    menuText += $"<font color=\"green\">!8</font>) Next&nbsp;&nbsp;&nbsp;";
                }
                else
                {
                    menuText += "&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;";
                }
                menuText += $"<font color=\"green\">!9</font>) Close";
                player.PrintToCenterHtml(menuText);
            }

            foreach(var key in htmlMessage.Keys)
            {
                CCSPlayerController? player = Utilities.GetPlayerFromSteamId(key);
                if (player == null) { htmlMessage.Remove(key); continue; }
                if (!htmlMessage.TryGetValue(key, out HtmlMessage? message))
                {
                    htmlMessage.Remove(key);
                    continue;
                }

                if(message.DateTimeEnd > DateTime.Now)
                {
                    player.PrintToCenterHtml(message.Message);
                }
                else
                {
                    htmlMessage.Remove(player.SteamID);
                }               
            }

            foreach(var key in Timers.Keys)
            {
                CCSPlayerController? player = Utilities.GetPlayerFromSteamId(key);
                if (player == null) continue;

                player.PrintToCenter($" {ChatColors.Green} Timer {ChatColors.White} {(DateTime.Now - Timers[key]).TotalSeconds.ToString("0.00")}s");
            }


            foreach (var key in Countdown.Keys)
            {
                CCSPlayerController? player = Utilities.GetPlayerFromSteamId(key);
                if (player == null) continue;

                player.PrintToCenterHtml($"Countdown <font color=\"green\">{(Countdown[key] - DateTime.Now).TotalSeconds.ToString("0.00")}s</font> ");
                if(DateTime.Now >= Countdown[key])
                {
                    Countdown.Remove(key);
                    Utils.ClientChatMessage("Timer finished", player);
                }
            }
        }

        /// <summary>
        /// Menu Selection
        /// </summary>
        /// <param name="player"></param>
        /// <param name="command"></param>
        public void Selection(CCSPlayerController? player, CommandInfo command)
        {
            if (player == null) return;
            if (!player.IsValid) return;

            if (!htmlMenus.TryGetValue(player.SteamID, out HtmlMenu? menu))
            {
                return;
            }
            if (menu == null) return;
            string selectedOption = command.GetCommandString;
            if (selectedOption.StartsWith("css_"))
            {
                selectedOption = selectedOption.Substring(4);
            }
            if (!int.TryParse(selectedOption, out int index))
            {
                return;
            }
            int maxItems = 6;
            if(menu.MenuPages.Count > menu.Page)
            {
                maxItems = menu.MenuPages[menu.Page].Count;
            }
            if(index == 7)
            {
                if(menu.Page > 0)
                {
                    menu.Page--;                  
                }
                return;
            }
            if (index == 8)
            {
                if(menu.Page + 1 < menu.MenuPages.Count)
                {
                    menu.Page++;
                }               
                return;
            }
            if(index == 9)
            {
                htmlMenus.Remove(player.SteamID);
                return;
            }
            if (menu.MenuPages[menu.Page].Count >= index)
            {
                Server.NextFrame(menu.MenuPages[menu.Page][index-1].Value);
                if (menu.CloseOnSelect)
                {
                    htmlMenus.Remove(player.SteamID);
                }
            }
            
        }
    }
}
