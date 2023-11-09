using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace CSPracc.DataModules
{
    public class CommandAlias
    {
        [XmlAttribute("Alias")]
        public string Alias { get; set; }

        [XmlAttribute("Command")]
        public string Command { get; set; }

        public CommandAlias() 
        {
            Alias = string.Empty;
            Command = string.Empty;
        }

        public CommandAlias(string alias,string command)
        {
            Alias = alias;
            Command = command;
        }
    }
}
