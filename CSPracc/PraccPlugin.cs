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
using System.Resources;
using Microsoft.Extensions.Logging;

[MinimumApiVersion(80)]
public class CSPraccPlugin : BasePlugin, IPluginConfig<CSPraccConfig>
{
    private static CSPraccPlugin? _instance = null;
    /// <summary>
    /// Plugin instance
    /// </summary>
    public static CSPraccPlugin Instance 
    {
        get
        {
            if(_instance == null)
            {
                throw new Exception("Plugin instance is null.");
            }
            return _instance;
        }
    }

    #region properties
    /// <summary>
    /// Module Name
    /// </summary>
    public override string ModuleName
    {
        get
        {
            return "Practice Plugin";
        }
    }
    /// <summary>
    /// Module Version
    /// </summary>
    public override string ModuleVersion
    {
        get
        {
            return "1.0.0.1";
        }
    }

    /// <summary>
    /// Author of the module
    /// </summary>
    public override string ModuleAuthor => "CHR15";   


    private static DirectoryInfo? _moduleDir;
    /// <summary>
    /// Module Directory
    /// </summary>
    public static DirectoryInfo ModuleDir => _moduleDir!;

    private static DirectoryInfo? _csgoDir = null;

    /// <summary>
    /// CS2 Directory
    /// </summary>
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

    /// <summary>
    /// Current config
    /// </summary>
    public CSPraccConfig? Config { get; set; }

    /// <summary>
    /// Current plugin mode
    /// </summary>
    public static BaseMode? PluginMode { get; set; }
    #endregion

    /// <summary>
    /// Called on plugin load
    /// </summary>
    /// <param name="hotReload">is hotreload</param>
    public override void Load(bool hotReload)
    {
        base.Load(hotReload);
        _moduleDir = new DirectoryInfo(ModuleDirectory);   
        RegisterListener<Listeners.OnMapStart>((mapName) =>
        {
            Reset();
        });
        _instance = this;
        SwitchMode(Config!.ModeToLoad);
        Logger.LogInformation("Pracitce Plugin loaded.");
    }

    /// <summary>
    /// Resetting plugin settings
    /// </summary>
    private void Reset()
    {
        SwitchMode(Config!.ModeToLoad);
    }

    /// <summary>
    /// Switching the plugin mode
    /// </summary>
    /// <param name="pluginMode">Mode in which shall be switched</param>
    public void SwitchMode(PluginMode pluginMode)
    {
        PluginMode?.Dispose();
        switch (pluginMode)
        {
            case Enums.PluginMode.Base:
                {
                    PluginMode = new BaseMode();
                    break;
                }
            case Enums.PluginMode.Pracc:
                {
                    PluginMode = new PracticeMode();
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
            case Enums.PluginMode.Prefire:
                {
                    PluginMode = new PrefireMode();
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

    /// <summary>
    /// Called when the plugin configuration is parsed
    /// </summary>
    /// <param name="config"></param>
    public void OnConfigParsed(CSPraccConfig config)
    {
        if(config == null)
        {
            return;
        }
        if(config.Version == 1)
        {
            config.AdminRequirement = true;
            config.ModeToLoad = Enums.PluginMode.Base;
        }
        Config = config;
        DemoManager.DemoManagerSettings = config.DemoManagerSettings;
    }
}


