using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes;
using CSPracc;
using CSPracc.DataModules;
using static CSPracc.DataModules.Enums;
using CSPracc.Managers;
using CSPracc.Modes;
using Microsoft.Extensions.Logging;

[MinimumApiVersion(216)]
public class CSPraccPlugin : BasePlugin, IPluginConfig<CSPraccConfig>
{
    public static CSPraccPlugin? Instance { get; private set; }

    #region properties
    public override string ModuleName
    {
        get
        {
            return "Practice Plugin";
        }
    }
    public override string ModuleVersion
    {
        get
        {
            return "1.0.0.3";
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

    public CSPraccConfig? Config { get; set; }

    public static BaseMode? PluginMode { get; set; }
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

    public static void SwitchMode(PluginMode pluginMode)
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

    public void OnConfigParsed(CSPraccConfig config)
    {
        if(config == null)
        {
            return;
        }
        if(config.Version == null || config.Version == 1)
        {
            config.AdminRequirement = true;
            config.ModeToLoad = Enums.PluginMode.Base;
        }
        Config = config;
        DemoManager.DemoManagerSettings = config.DemoManagerSettings;
    }
}


