using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSPracc.DataStorages.JsonStorages
{
    public class CommandAliasStorage : JsonStorage<string, string>
    {
        public CommandAliasStorage(DirectoryInfo commandAliasSaveDirectory, CCSPlayerController player) : base(new FileInfo(Path.Combine(commandAliasSaveDirectory.FullName, $"{player.PlayerName}_aliases.json")))
        {
        }
        public CommandAliasStorage(DirectoryInfo commandAliasSaveDirectory): base(new FileInfo(Path.Combine(commandAliasSaveDirectory.FullName, "global_aliases.json")))
        {

        }
        public override bool Get(string key, out string value)
        {
            if (!Storage.TryGetValue(key, out value))
            {
                value = "";
                return false;
            }
            return true;
        }
    }
}
