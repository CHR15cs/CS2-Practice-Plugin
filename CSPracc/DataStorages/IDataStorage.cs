using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSPracc.DataStorages
{
    internal interface IDataStorage<TKey, TValue>
    {
        public bool Add(TKey key, TValue value);
        public void SetOrAdd(TKey key, TValue value);
        public bool RemoveKey(TKey key);
        public void Clear();
        public bool ContainsKey(TKey key);
        public bool Get(TKey key, out TValue value);
        public List<KeyValuePair<TKey, TValue>> GetAll();
        public void Save();
    }
}
