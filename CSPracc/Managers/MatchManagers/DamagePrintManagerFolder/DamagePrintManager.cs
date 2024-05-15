using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using CSPracc.DataModules.Constants;
using CSPracc.Managers.BaseManagers.CommandManagerFolder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CounterStrikeSharp.API.Core.BasePlugin;

namespace CSPracc.Managers.MatchManagers.DamagePrintManagerFolder
{

    struct DamageData
    {
        public int hits;
        public int damage;
    }
    /// <summary>
    /// Manager to print damage done to players after the round
    /// </summary>
    public class DamagePrintManager : BaseManagers.BaseManager
    {
        private bool _damagePrintEnabled { get; set; } = false;

        private Dictionary<ulong,Dictionary<ulong, DamageData>> _playerDamageDictionary = new();


        /// <summary>
        /// Constructor registering the command
        /// </summary>
        public DamagePrintManager() : base()
        {
            Commands.Add("damageprint", new DataModules.PlayerCommand("damageprint", "Toggle damage print", DamagePrintCommandHandler, null, null));
            CSPraccPlugin.Instance.RegisterEventHandler<EventPlayerHurt>(OnPlayerHurt, HookMode.Post);
            CSPraccPlugin.Instance.RegisterEventHandler<EventRoundEnd>(OnRoundEnd, HookMode.Post);
        }

        private HookResult OnPlayerHurt(EventPlayerHurt @event, GameEventInfo info)
        {
            if (@event.Userid != null && @event.Attacker != null)
            {
                if(!_playerDamageDictionary.ContainsKey(@event.Attacker.SteamID))
                {
                    _playerDamageDictionary.Add(@event.Attacker.SteamID, new Dictionary<ulong, DamageData>());
                }
                if (!_playerDamageDictionary[@event.Attacker.SteamID].ContainsKey(@event.Userid.SteamID))
                {
                    _playerDamageDictionary[@event.Attacker.SteamID].Add(@event.Userid.SteamID, new DamageData() { hits = 0, damage = 0 });
                }
                DamageData damageData = _playerDamageDictionary[@event.Attacker.SteamID][@event.Userid.SteamID];
                damageData.hits++;
                damageData.damage += @event.DmgHealth;
                _playerDamageDictionary[@event.Attacker.SteamID][@event.Userid.SteamID] = damageData;
            }
            return HookResult.Continue;
        }

        private HookResult OnRoundEnd(EventRoundEnd @event, GameEventInfo info)
        {
            if (_damagePrintEnabled)
            {
                IEnumerable<CCSPlayerController> terrorists = Utilities.GetPlayers().Where(x => x.GetCsTeam() == CsTeam.Terrorist);
                IEnumerable<CCSPlayerController> counterTerrorists = Utilities.GetPlayers().Where(x => x.GetCsTeam() == CsTeam.CounterTerrorist);
                //Print dmg for Terrorists
                for (int currentTerrorirst = 0; currentTerrorirst < terrorists.Count(); currentTerrorirst++)
                {
                    for(int currentCounterTerrorist = 0; currentCounterTerrorist< counterTerrorists.Count(); currentCounterTerrorist++ )
                    {
                        DamageMatrix damageMatrix = new DamageMatrix(terrorists.ElementAt(currentTerrorirst), counterTerrorists.ElementAt(currentCounterTerrorist), _playerDamageDictionary);
                        terrorists.ElementAt(currentTerrorirst).PrintToChat($" {ChatColors.Green}{damageMatrix}");
                    }                
                }
                //Print dmg for CounterTerrorists
                for (int currentCounterTerrorist = 0; currentCounterTerrorist < counterTerrorists.Count(); currentCounterTerrorist++)
                {
                    for (int currentTerrorirst = 0; currentTerrorirst < terrorists.Count(); currentTerrorirst++)
                    {
                        DamageMatrix damageMatrix = new DamageMatrix(counterTerrorists.ElementAt(currentCounterTerrorist), terrorists.ElementAt(currentTerrorirst), _playerDamageDictionary);
                        terrorists.ElementAt(currentTerrorirst).PrintToChat($" {ChatColors.Green}{damageMatrix}");
                    }
                }
            }
            _playerDamageDictionary.Clear();
            return HookResult.Continue;
        }

        /// <summary>
        /// Toggle Damageprint
        /// </summary>
        /// <param name="controller">Player who issued the command</param>
        /// <param name="argument">arguments passed</param>
        /// <returns>True if successfull</returns>
        private bool DamagePrintCommandHandler(CCSPlayerController controller, PlayerCommandArgument argument)
        {
            _damagePrintEnabled = !_damagePrintEnabled;
            controller.ChatMessage($"Damage print is now {_damagePrintEnabled}");
            return true;
        }

        /// <summary>
        /// Disposing the event handler and command
        /// </summary>
        public new void Dispose()
        {
            GameEventHandler<EventPlayerHurt> playerHurt = OnPlayerHurt;
            CSPraccPlugin.Instance.DeregisterEventHandler("player_hurt", playerHurt, false);
            GameEventHandler<EventRoundEnd> roundEnd = OnRoundEnd;
            CSPraccPlugin.Instance.DeregisterEventHandler("round_end", roundEnd, false);
            Commands.Remove("damageprint");
        }
    }
}
