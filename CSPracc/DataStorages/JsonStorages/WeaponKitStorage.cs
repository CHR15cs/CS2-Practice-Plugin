using CSPracc.DataModules;
using CSPracc.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSPracc.DataStorages.JsonStorages
{
    public class WeaponKitStorage : JsonStorage<int, WeaponKit>
    {
        public WeaponKitStorage(FileInfo jsonFile) : base(jsonFile)
        {
        }

        public override bool Get(int key, out WeaponKit weapKit)
        {
            if (!Storage.TryGetValue(key, out weapKit))
            {
                weapKit = new WeaponKit();
                return false;
            }
            return true;
        }
    }
}
