using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Entities;
using CounterStrikeSharp.API.Modules.Memory;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSPracc.DataModules
{
    /// <summary>
    /// Player Class, shall be used for any interaction with this player
    /// </summary>
    public class Player
    {
        public int clientindex = -1;
        private ulong _steamid = 0xff;


        public CCSPlayerController? CCSPlayerController
        {
            get
            {
                if (clientindex == 0) return null;
                return new CCSPlayerController(NativeAPI.GetEntityFromIndex(clientindex + 1));
            }
        }

        public CBasePlayerPawn? CBasePlayerPawn
        {
            get
            {
                if (clientindex == 0) return null;
                return this.CCSPlayerController?.Pawn.Value;
            }
        }
        /// <summary>
        /// Get Player SteamID
        /// </summary>
        public ulong SteamID
        {
            get
            {
                if(_steamid == 0xff)
                {
                    _steamid = CCSPlayerController.SteamID;
                }
                return _steamid;
            }
        }

        /// <summary>
        /// Get Player Name
        /// </summary>
        public string Name
        {
            get
            {
                return this.CCSPlayerController.PlayerName;
            }
        }

        /// <summary>
        /// Get Player health
        /// </summary>
        public int Health
        {
            get 
            {              
                return CCSPlayerController.Health;
            }
            set 
            {
                CCSPlayerController.Health = value;
            }
        }

        /// <summary>
        /// Get Player Money
        /// </summary>
        public int Money
        {
            get
            {
                return this.CCSPlayerController.InGameMoneyServices.Account;
            }
            set
            {
                this.CCSPlayerController.InGameMoneyServices.Account = value;
            }
        }

        /// <summary>
        /// Player Color, f.e shall be used to draw smokes in the same color
        /// </summary>
        public Color PlayerColor
        {
            get
            {
                switch(this.CCSPlayerController.CompTeammateColor)
                {
                    case 1:
                        return Color.Green;
                    case 2:
                        return Color.Yellow;
                    case 3:
                        return Color.Orange; 
                    case 4:
                        return Color.Pink;
                    case 5:
                        return Color.LightBlue;
                    default :
                        return Color.Red;
                }
            }
        }
        private List<SavedNade> _savedNade = new List<SavedNade>();
        public List<SavedNade> SavedNades
        {
            get { return _savedNade; }
        }

        /// <summary>
        /// Initializes Object with required informations
        /// </summary>
        /// <param name="userid">id of user</param>
        public Player(int handle)
        {
            //ToDo initialize player with current values such as SteamID, Name etc.
            clientindex = handle;
        }

        public void AddSavedNade(SavedNade savedNade)
        {
            _savedNade.Add(savedNade);
        }

        /// <summary>
        /// Kick Player form Server
        /// </summary>
        public void Kick()
        {
            Server.ExecuteCommand($"kickid {clientindex}");
        }

        /// <summary>
        /// slay player
        /// </summary>
        public void Kill()
        {
            CCSPlayerController.PlayerPawn.Value.CommitSuicide(true, true);          
        }

        private void IssueCommand(string command)
        {
            NativeAPI.IssueClientCommand(clientindex, command);
        }
    }


}
