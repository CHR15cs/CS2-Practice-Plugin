using CSPracc.DataModules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace CSPracc
{
    [XmlRoot("CSPraccConfig")]
    public class ConfigManager
    {
        [XmlElement]
        public string RconPassword { get; set; }
        [XmlElement]
        public string LoggingFile { get; set; }

        [XmlArray("PluginAdmins")]
        public List<string> Admins;

        [XmlElement]
        public List<SavedNade> SavedNades
        {
            get
            {
                return NadeManager.Nades;
            }
            set
            {
                NadeManager.Nades = value;
            }
        }

        public ConfigManager()
        {
            SavedNades = NadeManager.Nades!;
        }
    }
}
