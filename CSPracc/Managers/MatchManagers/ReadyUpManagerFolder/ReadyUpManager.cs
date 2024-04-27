using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using CSPracc.Extensions;
using CSPracc.Managers.BaseManagers;
using CSPracc.Managers.BaseManagers.CommandManagerFolder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSPracc.Managers.MatchManagers.ReadyUpManagerFolder
{
    /// <summary>
    /// Enum to define the ready up mode
    /// </summary>
    public enum ReadyUpMode
    {
        /// <summary>
        /// A player can ready up as a team
        /// </summary>
        Team,
        /// <summary>
        /// A player only ready up for himself
        /// </summary>
        SinglePlayer
    }

    /// <summary>
    /// Manager to handle the ready up phase of the match
    /// </summary>
    public class ReadyUpManager : BaseManager
    {
        ReadyUpMode _readyUpMode { get; set; } = ReadyUpMode.Team;
        Dictionary<CsTeam, List<CCSPlayerController>> _readyUpDictionary { get; set; } = new Dictionary<CsTeam, List<CCSPlayerController>>();
        Dictionary<CsTeam, bool> _readyUpTeamDictionary { get; set; } = new Dictionary<CsTeam, bool>();

        /// <summary>
        /// Constructor registering the commands
        /// </summary>
        public ReadyUpManager() : base()
        {
            Commands.Add("ready", new DataModules.PlayerCommand("ready", "Ready up for the match", ReadyCommandHandler, null, null));
            Commands.Add("unready", new DataModules.PlayerCommand("unready", "Unready for the match", UnreadyCommandHandler, null, null));
            _readyUpDictionary.Add(CsTeam.Terrorist, new List<CCSPlayerController>());
            _readyUpDictionary.Add(CsTeam.CounterTerrorist, new List<CCSPlayerController>());
            _readyUpTeamDictionary.Add(CsTeam.Terrorist, false);
            _readyUpTeamDictionary.Add(CsTeam.CounterTerrorist, false);
        }

        /// <summary>
        /// Commandhandler for the player to readyup as team or as single player
        /// </summary>
        /// <param name="player">player who executed the command</param>
        /// <param name="commands">argument passed from the player</param>
        /// <returns>True if successfull</returns>
        private bool ReadyCommandHandler(CCSPlayerController player,PlayerCommandArgument commands)
        {
            if(!_readyUpDictionary.TryGetValue(player.GetCsTeam(),out List<CCSPlayerController>? playerList))
            {
                player.ChatMessage("Oops! Something went wrong, make sure you`re in the T or CT Team.");
                return false;
            }
            if(playerList == null)
            {
                player.ChatMessage("Oops! Something went wrong, make sure you`re in the T or CT Team.");
                return false;
            }
            if(playerList.Contains(player))
            {
                player.ChatMessage("You`re already ready.");
                return true;
            }
            //if readyupmode is team, only one player needs to ready up
            if (_readyUpMode == ReadyUpMode.Team)
            {
                Utils.ServerMessage($"{player.GetCsTeam()} is ready up for the match.");
                return true;
            }
            else
            {
                playerList.Add(player);
                player.ChatMessage("You`re now ready.");
                List<CCSPlayerController> players = Utilities.GetPlayers().Where(x => x.GetCsTeam() == player.GetCsTeam()).ToList();
                if(playerList.Count == players.Count)
                {
                    Utils.ServerMessage($"{player.GetCsTeam()} is ready for the match.");
                    _readyUpTeamDictionary.SetOrAdd(player.GetCsTeam(), true);
                }
            }
            //Check if both teams are ready
            if (_readyUpTeamDictionary[CsTeam.Terrorist] && _readyUpTeamDictionary[CsTeam.Terrorist])
            {
                Utils.ServerMessage($"Both Teams are ready for the match.");
                //ToDo fire Teams ready event
            }

            return true;
        }

        /// <summary>
        /// Commandhandler to unready the player or team
        /// </summary>
        /// <param name="player">player who issued the command</param>
        /// <param name="args">argument passed from the player</param>
        /// <returns>True if successfull</returns>
        private bool UnreadyCommandHandler(CCSPlayerController player, PlayerCommandArgument args)
        {
            //Get all players which are ready in the players team
            if (!_readyUpDictionary.TryGetValue(player.GetCsTeam(), out List<CCSPlayerController>? playerList))
            {
                player.ChatMessage("Oops! Something went wrong, make sure you`re in the T or CT Team.");
                return false;
            }
            //playerList is null when the team is not t or ct
            if (playerList == null)
            {
                player.ChatMessage("Oops! Something went wrong, make sure you`re in the T or CT Team.");
                return false;
            }
            //if readyupmode is team, only one player needs to unready
            if (_readyUpMode == ReadyUpMode.Team)
            {
                Utils.ServerMessage($"{player.GetCsTeam()} is no longer ready for the match.");
                _readyUpTeamDictionary.SetOrAdd(player.GetCsTeam(), false);
                return true;
            }
            //if readyupmode is singleplayer, the player needs to unready for themselves
            if (playerList.Contains(player))
            {
                _readyUpTeamDictionary.SetOrAdd(player.GetCsTeam(), false);
                player.ChatMessage("You`re not ready.");
                playerList.Remove(player);
                return true;
            }           
            return false;
        }
    }
}
