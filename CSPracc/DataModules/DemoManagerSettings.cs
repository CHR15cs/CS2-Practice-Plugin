using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace CSPracc.DataModules
{
    public class DemoManagerSettings
    {
        [XmlIgnore]
        public  bool isRecording = false;
        [XmlIgnore]
        public  FileInfo LastDemoFile;

        [XmlElement("DemoName")]
        public string? DemoName { get; set; } = "{Map}_{yyyy}_{MM}_{dd}_{mm}_{HH}_{ss}.dem";
        [XmlElement("MegaUsername")]
        public string? MegaUsername { get; set; } = "MegaName";
        [XmlElement("MegaPassword")]
        public string? MegaPassword { get; set; } = "MegaPassword";

        [XmlElement("RecordingMode")]
        public  Enums.RecordingMode RecordingMode
        {
            get;set;
        }

        [XmlElement("AutomaticUpload")]
        public  bool AutomaticUpload { get; set; } = false;
    }
}
