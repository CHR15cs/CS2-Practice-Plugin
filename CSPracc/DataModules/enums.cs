using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSPracc.DataModules
{
    public static class Enums
    {
        public enum GRENADE_TYPE
        {
            decoy,
            smokegrenade,
            highexplosivegrenade,
            flashbang,
            molotov,
            incendiary
        }

        public enum RecordingMode
        {
            Automatic,
            Manual
        }

        public enum match_state
        {
            warmup,
            live
        }
        public enum PluginMode
        {
            Standard,
            Pracc,
            Match,
            DryRun,
            Retake
        }
    }
}
