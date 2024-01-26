using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSPracc.DataModules
{
    public class ReplaySet
    {
        public string SetName { get; set; } = "";
        public List<PlayerReplay> Replays { get; set; } = new List<PlayerReplay>();

        public ReplaySet(List<PlayerReplay> replays, string Name) 
        { 
            SetName = Name;
            Replays = replays;
        }

        public ReplaySet() { }

    }
}
