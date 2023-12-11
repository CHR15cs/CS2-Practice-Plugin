using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Config;
using CounterStrikeSharp.API.Modules.Menu;
using CSPracc.DataModules;
using CSPracc.DataModules.Constants;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace CSPracc.Managers
{
    public class DemoManager
    {
        public static DemoManagerSettings DemoManagerSettings { get; set; } = new DemoManagerSettings();
        public static void StartRecording()
        {
            if(!IsGoTvOnServer())
            {
                AddGoTv();
                return;
            }
            if(String.IsNullOrEmpty(DemoManagerSettings.DemoName))
            {
                //Setting Default Demo Name
                DemoManagerSettings.DemoName = "{Map}_{yyyy}_{MM}_{dd}_{mm}_{HH}_{ss}.dem";
            }
            DemoManagerSettings.DemoName = DemoManagerSettings.DemoName.Replace("{Map}", Server.MapName);
            DemoManagerSettings.DemoName = DemoManagerSettings.DemoName.Replace("{yyyy}", DateTime.Now.ToString("yyyy"));
            DemoManagerSettings.DemoName = DemoManagerSettings.DemoName.Replace("{MM}", DateTime.Now.ToString("MM"));
            DemoManagerSettings.DemoName = DemoManagerSettings.DemoName.Replace("{dd}", DateTime.Now.ToString("dd"));
            DemoManagerSettings.DemoName = DemoManagerSettings.DemoName.Replace("{HH}", DateTime.Now.ToString("HH"));
            DemoManagerSettings.DemoName = DemoManagerSettings.DemoName.Replace("{mm}", DateTime.Now.ToString("mm"));
            DemoManagerSettings.DemoName = DemoManagerSettings.DemoName.Replace("{ss}", DateTime.Now.ToString("ss"));
            Server.ExecuteCommand($"tv_record {DemoManagerSettings.DemoName}");
            DemoManagerSettings.LastDemoFile = new FileInfo(Path.Combine(CSPraccPlugin.Cs2Dir.FullName, DemoManagerSettings.DemoName));
            DemoManagerSettings.isRecording = true;
            return;
        }

        public static void StopRecording()
        {
            if (!IsGoTvOnServer())
            {
                return;
            }
            Server.ExecuteCommand("tv_stoprecord");
            DemoManagerSettings.isRecording = false;
            if(DemoManagerSettings.AutomaticUpload)
            {
                Logging.LogMessage("Starting Demo Upload");
                CSPraccPlugin.Instance.AddTimer(4.0f,()=>UploadDemo(DemoManagerSettings.LastDemoFile));                
            }
        }

        public static bool IsGoTvOnServer()
        {
            bool isOnServer = false;
            var playerEntities = Utilities.FindAllEntitiesByDesignerName<CCSPlayerController>("cs_player_controller");
            foreach(var entity in playerEntities)
            {
                if(entity == null) continue;
                if (!entity.IsValid) continue;
                if(entity.IsHLTV)
                {
                    isOnServer = true;
                    Logging.LogMessage($"Found TV Server");
                    break;
                }
            }
            return isOnServer;
        }

        private static void AddGoTv()
        {
            Server.ExecuteCommand("tv_enable 1");
            Methods.MsgToServer("Adding GOTV, map needs to be reloaded for it to be active.");
            return;
        }

        public static void OpenDemoManagerMenu(CCSPlayerController player)
        {
            if (player == null) return;
            if (!player.PlayerPawn.IsValid) return;
            if (!player.IsAdmin())
            {
                player.PrintToCenter("Only admins can execute this command!");
                return;
            }
            var varDemoMenu = new ChatMenu("Demo Manager Menu");
            var switchDemoMode = (CCSPlayerController player, ChatMenuOption option) => SwitchDemoMode();
            var switchAutomaticUpload = (CCSPlayerController player, ChatMenuOption option) => SwitchAutomaticUpload();
            var startDemoRecordingOption = (CCSPlayerController player, ChatMenuOption option) => StartRecording();
            var stopDemoRecordingOption = (CCSPlayerController player, ChatMenuOption option) => StopRecording();
            varDemoMenu.AddMenuOption($"Recording Mode: {DemoManagerSettings.RecordingMode}", switchDemoMode);
            //string uploadOption = DemoManagerSettings.AutomaticUpload ? "yes" : "no";
            //varDemoMenu.AddMenuOption($"Automatic upload: {uploadOption}", switchAutomaticUpload);
            Logging.LogMessage("record");
            if (DemoManagerSettings.isRecording)
            {
                Logging.LogMessage("stop record");
                varDemoMenu.AddMenuOption("Stop recording", stopDemoRecordingOption);                
            }
            else
            {
                Logging.LogMessage("start record");
                varDemoMenu.AddMenuOption("Start recording", startDemoRecordingOption);
            }          
            ChatMenus.OpenMenu(player, varDemoMenu);
        }

        private static void SwitchDemoMode()
        {
            if (DemoManagerSettings.RecordingMode == Enums.RecordingMode.Manual)
            {
                DemoManagerSettings.RecordingMode = Enums.RecordingMode.Automatic;
            }
            else if (DemoManagerSettings.RecordingMode == Enums.RecordingMode.Automatic)
            {
                DemoManagerSettings.RecordingMode = Enums.RecordingMode.Manual;
            }
            CSPraccPlugin.Instance.Config.DemoManagerSettings = DemoManagerSettings;
        }

        private static void SwitchAutomaticUpload()
        {
            DemoManagerSettings.AutomaticUpload = !DemoManagerSettings.AutomaticUpload;
            CSPraccPlugin.Instance.Config.DemoManagerSettings = DemoManagerSettings;
        }

        public static void UploadDemo(FileInfo demoFile)
        {

        }
    }
}
