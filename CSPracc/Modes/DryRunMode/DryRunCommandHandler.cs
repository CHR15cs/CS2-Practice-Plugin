using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
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
                case ".refill":
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
                case ".ak":
                    {
                        player.GiveNamedItem("weapon_ak47");
                        break;
                    }
                case ".awp":
                    {
                        player.GiveNamedItem("weapon_awp");
                        break;
                    }
                case ".m4a1":
                    {
                        player.GiveNamedItem("weapon_m4a1_silencer");
                        break;
                    }
                case ".m4":
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
    }
}
