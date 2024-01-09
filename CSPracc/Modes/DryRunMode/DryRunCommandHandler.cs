using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using CSPracc.DataModules.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSPracc.CommandHandler
{
    public class DryRunCommandHandler : MatchCommandHandler
    {
        public DryRunCommandHandler(DryRunMode mode):base(mode) 
        { 
            
        }

        public override bool PlayerChat(EventPlayerChat @event, GameEventInfo info)
        {
            if (!CheckAndGetCommand(@event.Userid, @event.Text, out string command, out string args, out CCSPlayerController player))
            {
                return false;
            }
            switch (command)
            {
                case DRYRUN_COMMAND.refill:
                    {
                        int index = 0;
                        bool molotov = false;
                        bool flash = false;
                        bool smoke = false;
                        bool he = false;
                        foreach(var weapon in player.PlayerPawn.Value.WeaponServices.MyWeapons)
                        {
                            if (weapon.Value.DesignerName.Trim() == "weapon_molotov")
                            {
                               molotov = true;
                            }
                            if (weapon.Value.DesignerName.Trim() == "weapon_flashbang")
                            {
                                flash = true;
                            }
                            if (weapon.Value.DesignerName.Trim() == "weapon_smokegrenade")
                            {
                                smoke = true;
                            }
                            if (weapon.Value.DesignerName.Trim() == "weapon_hegrenade")
                            {
                               he = true;
                            }
                            index++;
                        }
                        if (!molotov)
                        {
                            player.GiveNamedItem("weapon_molotov");
                        }
                        if (!flash)
                        {
                            player.GiveNamedItem("weapon_flashbang");
                        }
                        if (!smoke)
                        {
                            player.GiveNamedItem("weapon_smokegrenade");
                        }
                        if(!he)
                        {
                            player.GiveNamedItem("weapon_hegrenade");
                        }

                        break;
                    }
                case DRYRUN_COMMAND.ak:
                    {
                        player.GiveNamedItem("weapon_ak47");
                        break;
                    }
                case DRYRUN_COMMAND.awp:
                    {
                        player.GiveNamedItem("weapon_awp");
                        break;
                    }
                case DRYRUN_COMMAND.m4a1:
                    {
                        player.GiveNamedItem("weapon_m4a1_silencer");
                        break;
                    }
                case DRYRUN_COMMAND.m4:
                    {
                        player.GiveNamedItem("weapon_m4a1");
                        break;
                    }
                default:
                    {
                        base.PlayerChat(@event, info);
                        return false;
                    }
            }
            return true;
        }


        public override void PrintHelp(CCSPlayerController? player)
        {
            base.PrintHelp(player);
            List<string> message = new List<string>();
            message.Add($" {ChatColors.Green}{DRYRUN_COMMAND.refill}{ChatColors.White} Refills your utility.");
            message.Add($" {ChatColors.Green}{DRYRUN_COMMAND.ak}{ChatColors.White} Drop yourself an ak.");
            message.Add($" {ChatColors.Green}{DRYRUN_COMMAND.awp}{ChatColors.White} Drop yourself an awp.");
            message.Add($" {ChatColors.Green}{DRYRUN_COMMAND.m4a1}{ChatColors.White} Drop yourself an m4a1-s.");
            message.Add($" {ChatColors.Green}{DRYRUN_COMMAND.m4}{ChatColors.White} Drop yourself an m4a4.");
            foreach (string s in message)
            {
                player?.PrintToChat(s);
            }
        }
    }
}
