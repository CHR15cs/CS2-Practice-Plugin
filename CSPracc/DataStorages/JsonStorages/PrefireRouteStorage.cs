using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
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
        public PrefireRouteStorage() : base(new FileInfo(Path.Combine(CSPraccPlugin.Instance!.ModuleDirectory, "Prefire", $"{Server.MapName}.json")))
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

        public PrefireRoute? GetRouteByNameOrDefault(string name)
        {
            List<KeyValuePair<int, PrefireRoute>> routes = GetAll();
            foreach(KeyValuePair<int, PrefireRoute> route in routes) 
            { 
                if(route.Value.Name == name)
                {
                    return route.Value;
                }
            }
            return null;
        }
    }
}
