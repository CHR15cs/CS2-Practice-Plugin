using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace CSPracc.DataModules
{
    public class DemoManagerSettings
    {
        [JsonIgnore]
        public  bool isRecording = false;
        [XmlIgnore]
        public  FileInfo LastDemoFile;

        [JsonPropertyName("DemoName")]
        public string? DemoName { get; set; } = "{Map}_{yyyy}_{MM}_{dd}_{mm}_{HH}_{ss}.dem";

        [JsonPropertyName("RecordingMode")]
        public  Enums.RecordingMode RecordingMode
        {
            get;set;
        }

        [JsonPropertyName("AutomaticUpload")]
        public  bool AutomaticUpload { get; set; } = false;
    }
}
