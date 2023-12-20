using CSPracc.DataModules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSPracc.DataStorages.JsonStorages
{
    public class UserPlayerEquipStorage : JsonStorage<ulong, UserSelectedEquipment>
    {
        public UserPlayerEquipStorage(FileInfo jsonFile) : base(jsonFile)
        {
        }

        public override bool Get(ulong key, out UserSelectedEquipment userSelectedEquipment)
        {
            if (!Storage.TryGetValue(key, out userSelectedEquipment))
            {
                userSelectedEquipment = new UserSelectedEquipment();
                return false;
            }
            return true;
        }
    }
}
