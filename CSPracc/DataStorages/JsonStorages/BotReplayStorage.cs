using CSPracc.DataModules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSPracc.DataStorages.JsonStorages
{
    public class BotReplayStorage : JsonStorage<int,ReplaySet>
    {
        public BotReplayStorage(FileInfo jsonFile) : base(jsonFile)
        {
            if(!jsonFile!.Directory!.Exists)
            {
                jsonFile.Directory.Create();
            }
        }

        public int Add(ReplaySet replaySet)
        {
            int id = GetUnusedKey();
            Add(id, replaySet);
            Save();
            return id;
        }

        public int GetUnusedKey()
        {
            int id = Storage.Count + 1;
            while (Storage.ContainsKey(id))
            {
                id++;
            }
            return id;
        }

        public override bool Get(int key, out ReplaySet value)
        {
            if (!Storage.TryGetValue(key, out value))
            {
                return false;
            }
            return true;

        }
    }
}
