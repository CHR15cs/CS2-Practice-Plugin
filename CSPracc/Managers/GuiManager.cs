using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Menu;
using CounterStrikeSharp.API.Modules.Utils;
using CSPracc.DataModules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSPracc.Managers
{
    public class GuiManager : IDisposable
    {

        Dictionary<ulong,DateTime> Timers = new Dictionary<ulong,DateTime>();
        Dictionary<ulong,HtmlMenu> htmlMenus = new Dictionary<ulong, HtmlMenu> ();
        Dictionary<ulong,HtmlMessage> htmlMessage = new Dictionary<ulong, HtmlMessage> ();  

        public GuiManager() 
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

        public void AddMenu(ulong id, HtmlMenu menu)
        {
            Server.NextFrame(() =>
            {
                if (htmlMenus.ContainsKey(id))
                {
                    htmlMenus[id] = menu;
                }
                else
                {
                    htmlMenus.Add(id, menu);
                }
            });

        }

        public void Dispose()
        {
            CSPraccPlugin.Instance!.RemoveListener("OnTick", OnTick);
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
                menuText += $"--------------<br>";
                if (menu.Options == null) continue;

                int maxItemsPerSite = 4;
                for(int i = 0;i< maxItemsPerSite; i++)
                {
                    if(menu.Options.Count > i + (menu.Page * maxItemsPerSite))
                    {
                        menuText += $"<font color=\"green\">!{i+1}</font> - {menu.Options[i + (menu.Page * maxItemsPerSite)].Key}";
                        if (i +1 < maxItemsPerSite)
                        {
                            menuText += "<br>";
                        }
                    }
                }
                menuText += "<br>";
                menuText += "_____________<br>";
                if(menu.Page > 0) menuText += $"<font color=\"green\">!7</font> - Prev";
                if((menu.Page +1) * maxItemsPerSite  <  menu.Options.Count) menuText += $"<font color=\"green\">!8</font> - Next";
                menuText += $"<font color=\"green\">!9</font> - Close";
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
        }

        public void Selection(CCSPlayerController? player, CommandInfo command)
        {
            if(player == null) return;
            if (!player.IsValid) return;

            if (!htmlMenus.TryGetValue(player.SteamID, out HtmlMenu? menu))
            {
                return;
            }
            if(menu == null) return;
            string selectedOption = command.GetCommandString;
            if (selectedOption.StartsWith("css_"))
            {
               selectedOption = selectedOption.Substring(4);
            }
            if(!int.TryParse(selectedOption, out int index))
            {
                return;
            }
            if(index == 7)
            {
                if(menu.Page > 0)
                {
                    menu.Page--;
                    return;
                }
            }
            if(index == 8 && menu.Page +1 <= menu.Options.Count / 4 )
            {
                menu.Page++;
                return;
            }
            if(index == 9)
            {
                htmlMenus.Remove(player.SteamID);
                return;
            }
            if (menu.Options.Count >= index) 
            {
                Server.NextFrame(menu.Options[menu.Page * 4 + index - 1].Value);
                if (menu.CloseOnSelect)
                {
                    htmlMenus.Remove(player.SteamID);
                }
            }
            
        }
    }
}
