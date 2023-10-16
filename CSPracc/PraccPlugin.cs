 using System;
 using System.Collections.Generic;
 using System.IO;
 using System.Linq;
 using System.Net.Http;
 using System.Threading;
 using System.Threading.Tasks;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Entities;
using CounterStrikeSharp.API.Modules.Events;
using CounterStrikeSharp.API.Modules.Memory;

public class CSPraccPlugin : BasePlugin
{
    List<Player> Players;
    enum Mode
    {
        Standard,
        Pracc,
        Match
    }
    Mode CurrentMode { get; set; }

    public override string ModuleName
    {
        get
        {
            return "CSPraccPlugin";
        }
    }

    public override string ModuleVersion
    {
        get
        {
            return "1.0.0.0";
        }
    }

    public override void Load(bool hotReload)
    {
        Players = new List<Player>();
        base.Load(hotReload);
        CurrentMode = Mode.Standard;
        //Register Handler that adds new player to Players and if tey disconnect, they shall be removed.

    }


}
