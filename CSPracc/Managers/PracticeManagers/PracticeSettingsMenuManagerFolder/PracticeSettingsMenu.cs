using CounterStrikeSharp.API.Core;
using CSPracc.DataModules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSPracc.Managers.PracticeManagers.PracticeSettingsMenuManagerFolder
{
    public class PracticeSettingsMenu
    {
        public static HtmlMenu GetPracticeSettingsMenu(CCSPlayerController ccsplayerController, CSPraccPlugin Plugin)
        {
            HtmlMenu practiceMenu;
            List<KeyValuePair<string, Action>> menuOptions = new List<KeyValuePair<string, Action>>();
            if (!ccsplayerController.GetValueOfCookie("PersonalizedNadeMenu", out string? setting))
            {
                if (Plugin.Config!.UsePersonalNadeMenu)
                {
                    setting = "yes";
                    ccsplayerController.SetOrAddValueOfCookie("PersonalizedNadeMenu", "yes");
                }
                else
                {
                    setting = "no";
                    ccsplayerController.SetOrAddValueOfCookie("PersonalizedNadeMenu", "no");
                }
            }
            if (setting == null)
            {
                setting = Plugin.Config!.UsePersonalNadeMenu ? "yes" : "no";
                ccsplayerController.SetOrAddValueOfCookie("PersonalizedNadeMenu", "yes");
            }
            string MenuText = "";
            if (setting == "yes")
            {
                MenuText = "Show Global Nade Menu";
            }
            else
            {
                MenuText = "Show Personalized Nade Menu";
            }

            KeyValuePair<string, Action> personalizedNadeMenu = new KeyValuePair<string, Action>(MenuText, () =>
            {
                SwitchPersonalizedNadeMenuOption(ccsplayerController);
            });
            menuOptions.Add(personalizedNadeMenu);
            return practiceMenu = new HtmlMenu("Practice Settings", menuOptions);
        }

        /// <summary>
        /// Get settings
        /// </summary>
        /// <param name="ccsplayerController">player who issued the command</param>
        /// <returns>return setting menu</returns>
        private static void  SwitchPersonalizedNadeMenuOption(CCSPlayerController player)
        {
            if (!player.GetValueOfCookie("PersonalizedNadeMenu", out string? setting))
            {
                setting = "yes";
                player.SetOrAddValueOfCookie("PersonalizedNadeMenu", "yes");
            }
            switch (setting)
            {
                case "yes":
                    player.SetOrAddValueOfCookie("PersonalizedNadeMenu", "no");
                    break;
                case "no":
                    player.SetOrAddValueOfCookie("PersonalizedNadeMenu", "yes");
                    break;
                default:
                    player.SetOrAddValueOfCookie("PersonalizedNadeMenu", "yes");
                    break;
            }
        }

    }
}
