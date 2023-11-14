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
using CSPracc.DataModules.consts;
using System.Xml;
using System.Xml.Serialization;
using CSPracc.Managers;
using System.Drawing;

public class CSPraccPlugin : BasePlugin
{
    public static CSPraccPlugin? Instance { get; private set; }

    private static List<SteamID>? _adminList = null;
    public static List<SteamID> AdminList
    {
        get
        {
            if (_adminList == null)
            {
                _adminList = new List<SteamID>();
                if (Config != null)
                {
                    foreach (string admin in Config.Admins)
                    {
                        try
                        {
                            _adminList.Add(new SteamID(admin));
                        }
                        catch
                        {
                            Server.PrintToConsole("ignored admin: " + admin);
                        }

                    }
                }
            }
            return _adminList;

        }
    }

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
            return "0.9.0.1";
        }
    }


    private static DirectoryInfo? _moduleDir;
    public static DirectoryInfo ModuleDir => _moduleDir!;

    private string _rconPassword = String.Empty;
    private string RconPassword
    {
        get
        {
            if (_rconPassword == String.Empty)
            {
                if (Config != null)
                {
                    _rconPassword = Config.RconPassword;
                }
            }
            return _rconPassword;
        }
    }
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

    public static ConfigManager? Config;
    #endregion

    public override void Load(bool hotReload)
    {
        base.Load(hotReload);
        _moduleDir = new DirectoryInfo(ModuleDirectory);
        Logging logging = new Logging(new FileInfo(Path.Combine(ModuleDir.FullName, "Logging.txt")));
        configManagerFile = new FileInfo(Path.Combine(ModuleDir.FullName, "configmanager.xml"));
        XmlSerializer serializer = new XmlSerializer(typeof(ConfigManager));
        if (configManagerFile.Exists)
        {
            Config = serializer.Deserialize(File.OpenRead(configManagerFile.FullName)) as ConfigManager;
            DemoManager.DemoManagerSettings = Config!.DemoManagerSettings;
        }
        else
        {
            Config = new ConfigManager();
            Config.RconPassword = "geheim";
            Config.LoggingFile = "Logging.txt";
            Config.Admins = new List<string>();
            Config.SavedNades = new List<SavedNade>();
            Config.Admins.Add("steamid1234");
            Config.SavedNades.Add(new SavedNade(new Vector(0, 0, 0), new QAngle(0, 0, 0), new Vector(0, 0, 0), "test nade", "test", "de_test", 1));
            WriteConfig(Config);
        }
        RegisterListener<Listeners.OnMapStart>((mapName) =>
        {
            Reset();
        });
        Instance = this;
    }

    public static void WriteConfig(ConfigManager config)
    {
        XmlSerializer serializer = new XmlSerializer(typeof(ConfigManager));
        if (configManagerFile!.Exists)
        {
            configManagerFile.Delete();
        }
        serializer.Serialize(File.OpenWrite(configManagerFile.FullName), config);
    }

    [ConsoleCommand("css_rcon_password", "allow temporary admin control")]
    public void OnRconPassword(CCSPlayerController? player, CommandInfo command)
    {
        if (player == null) return;
        if (!player.PlayerPawn.IsValid) return;
        rconPassword(player, command.ArgString);
    }

    #region commands
    private void rconPassword(CCSPlayerController? player, string password)
    {
        password = password.Trim();
        if (RconPassword != password)
        {
            player!.PrintToCenter("Invalid Password");
            return;
        }
        CSPraccPlugin.AdminList.Add(new SteamID(player!.SteamID));
    }
    #endregion


    /// <summary>
    /// Resetting plugin settings
    /// </summary>
    private void Reset()
    {
        Match.SwitchTo(Match.CurrentMode, true);
    }
}


