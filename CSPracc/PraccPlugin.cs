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
using CounterStrikeSharp.API.Modules.Menu;
using CounterStrikeSharp.API.Modules.Utils;
using CSPracc;
using CSPracc.DataModules;
using CSPracc.DataModules.Constants;
using System.Xml;
using System.Xml.Serialization;
using CSPracc.Managers;
using System.Drawing;
using CSPracc.Modes;
using static CSPracc.DataModules.Enums;


[MinimumApiVersion(80)]
public class CSPraccPlugin : BasePlugin, IPluginConfig<CSPraccConfig>
{
    public static CSPraccPlugin? Instance { get; private set; }

    #region properties
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
            return "0.9.1.1";
        }
    }

    public override string ModuleAuthor => "CHR15 & Grükan";   


    private static DirectoryInfo? _moduleDir;
    public static DirectoryInfo ModuleDir => _moduleDir!;

    private static DirectoryInfo? _csgoDir = null;
    public static DirectoryInfo Cs2Dir
    {
        get
        {
            if (_csgoDir == null)
            {
                _csgoDir = new DirectoryInfo(Path.Combine(Server.GameDirectory, "csgo"));
            }
            return _csgoDir;
        }
    }


    private static FileInfo? configManagerFile = null;

    public CSPraccConfig Config { get; set; }

    public static BaseMode PluginMode { get; set; }
    #endregion

    public override void Load(bool hotReload)
    {
        base.Load(hotReload);
        _moduleDir = new DirectoryInfo(ModuleDirectory);   
        RegisterListener<Listeners.OnMapStart>((mapName) =>
        {
            Reset();
        });
        Instance = this;
        SwitchMode(Enums.PluginMode.Standard);
    }

    public static void WriteConfig()
    {
       
    }

    /// <summary>
    /// Resetting plugin settings
    /// </summary>
    private void Reset()
    {
        SwitchMode(Enums.PluginMode.Standard);
    }

    public static void SwitchMode(PluginMode pluginMode)
    {
        PluginMode?.Dispose();
        switch (pluginMode)
        {
            case Enums.PluginMode.Standard:
                {

                    PluginMode = new BaseMode();
                    break;
                }
            case Enums.PluginMode.Pracc:
                {
                    PluginMode = new PracticeMode();
                    break;
                }
            case Enums.PluginMode.Match:
                {
                    PluginMode = new MatchMode();
                    break;
                }
            case Enums.PluginMode.DryRun:
                {
                    PluginMode = new DryRunMode();
                    break;
                }
            case Enums.PluginMode.Retake:
                {
                    PluginMode = new RetakeMode();
                    break;
                }
            default:
                {
                    PluginMode = new BaseMode();
                    break;
                }               
        }
        PluginMode?.ConfigureEnvironment();
    }

    public void OnConfigParsed(CSPraccConfig config)
    {
        if(config == null)
        {
            return;
        }
        Config = config;
        DemoManager.DemoManagerSettings = config.DemoManagerSettings;
    }
}


