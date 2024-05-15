using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSPracc.Managers.MatchManagers.DamagePrintManagerFolder
{
    internal class DamageMatrix
    {
        CCSPlayerController _dmgDealer;
        CCSPlayerController _dmgTaker;
        Dictionary<ulong, Dictionary<ulong, DamageData>> _allDamageDealt;
        public DamageMatrix(CCSPlayerController attackingPlayer,CCSPlayerController receivingPlayer, Dictionary<ulong, Dictionary<ulong, DamageData>> allDamageDealt)
        {
            _dmgDealer = attackingPlayer;
            _dmgTaker = receivingPlayer;
            _allDamageDealt = allDamageDealt;
        }

        public override string ToString()
        {
            int hitsGiven = 0;
            int damageGiven = 0;
            int hitsTaken = 0;
            int damageTaken = 0;
            if (!_allDamageDealt.ContainsKey(_dmgDealer.SteamID))
            {
                if (_allDamageDealt[_dmgDealer.SteamID].ContainsKey(_dmgTaker.SteamID))
                {
                    DamageData damageData = _allDamageDealt[_dmgDealer.SteamID][_dmgTaker.SteamID];  
                    hitsGiven = damageData.hits;
                    damageGiven = damageData.damage;
                }
            }
            if (!_allDamageDealt.ContainsKey(_dmgTaker.SteamID))
            {
                if (_allDamageDealt[_dmgTaker.SteamID].ContainsKey(_dmgDealer.SteamID))
                {
                    DamageData damageData = _allDamageDealt[_dmgTaker.SteamID][_dmgDealer.SteamID];
                    hitsTaken = damageData.hits;
                    damageTaken = damageData.damage;
                }
            }
            return $"To: [{damageGiven}/{hitsGiven}] From [{damageTaken}/{hitsTaken}] - {_dmgTaker.PlayerName}";           
        }

    }
}
