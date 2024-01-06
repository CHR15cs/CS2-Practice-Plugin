using CSPracc.DataModules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSPracc.DataStorages.JsonStorages
{
    public class PrefireRouteStorage : JsonStorage<int, PrefireRoute>
    {
        public PrefireRouteStorage(FileInfo jsonFile) : base(jsonFile)
        {
        }

        public override bool Get(int key, out PrefireRoute value)
        {
            if (!Storage.TryGetValue(key, out value))
            {
                value = new PrefireRoute("");
                return false;
            }
            return true;
        }
    }
}
