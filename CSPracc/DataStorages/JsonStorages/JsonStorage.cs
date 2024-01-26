using CSPracc.DataModules;
using CSPracc.Extensions;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSPracc.DataStorages.JsonStorages
{
    public abstract class JsonStorage<TKey, TValue> : IDataStorage<TKey, TValue> where TKey : notnull
    {
        protected Dictionary<TKey, TValue> Storage { get; } = new Dictionary<TKey, TValue>();
        FileInfo JsonFile { get; init; }
        public JsonStorage(FileInfo jsonFile)
        {
            JsonFile = jsonFile;
            if (JsonFile.Exists)
            {
                string jsonString = File.ReadAllText(JsonFile.FullName);
                try
                {
                    Storage = JsonConvert.DeserializeObject<Dictionary<TKey, TValue>>(jsonString)!;
                }
                catch(Exception ex) 
                {
                    CSPraccPlugin.Instance!.Logger.LogError(ex.Message, ex);
                    CSPraccPlugin.Instance!.Logger.LogWarning($"Could not read {JsonFile.Name}. Creating new dictonary.");
                    Storage = new Dictionary<TKey, TValue>();
                }
            }
        }
        public bool Add(TKey key, TValue value)
        {
            if(Storage.ContainsKey(key))
            {
                return false;
            }
            SetOrAdd(key, value);
            return true;
        }
        public void SetOrAdd(TKey key, TValue value)
        {
            Storage.SetOrAdd(key, value);
            Save();
        }
        public bool RemoveKey(TKey key)
        {
            if (!ContainsKey(key))
            {
                return true;
            }
            if(Storage.Remove(key))
            {
                Save();
                return true;
            }
            return false;

        }
        public void Clear()
        {
            Storage.Clear();
        }
        public bool ContainsKey(TKey key)
        {
            return Storage.ContainsKey(key);
        }
        public void Save()
        {
            FileInfo backupFile = new FileInfo($"{JsonFile.FullName}.backup");
            if (!JsonFile.Directory!.Exists)
            {
                JsonFile.Directory.Create();
            }
            if (JsonFile.Exists)
            {
                JsonFile.CopyTo($"{JsonFile.FullName}.backup", true);
            }
            File.WriteAllText(JsonFile.FullName, JsonConvert.SerializeObject(Storage, formatting: Formatting.Indented));
        }
        public List<KeyValuePair<TKey, TValue>> GetAll()
        {
            return Storage.ToList();
        }
        public abstract bool Get(TKey key, out TValue value);
    }
}
