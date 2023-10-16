using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
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
        private nint PawnHandle = -1;
        private ulong _steamid = 0xff;

        /// <summary>
        /// Get Player SteamID
        /// </summary>
        public ulong SteamID
        {
            get
            {
                if(_steamid == 0xff)
                {
                    _steamid = NativeAPI.GetSchemaValueByName<ulong>(PawnHandle,(int)DataType.DATA_TYPE_ULONG_LONG, "CBasePlayerController", "m_steamID");

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
                return  NativeAPI.GetSchemaValueByName<string>(PawnHandle, (int)DataType.DATA_TYPE_STRING, "CBasePlayerController", "m_iszPlayerName"); ;
            }
            set
            {
                NativeAPI.SetSchemaValueByName<string>(PawnHandle, (int)DataType.DATA_TYPE_INT, "CBasePlayerController", "m_iszPlayerName", value);
            }
        }

        /// <summary>
        /// Get Player health
        /// </summary>
        public int Health
        {
            get 
            { 
                return NativeAPI.GetSchemaValueByName<int>(PawnHandle,(int) DataType.DATA_TYPE_INT, "CBaseEntity", "m_iHealth");; 
            }
            set 
            {
                NativeAPI.SetSchemaValueByName<int>(PawnHandle, (int)DataType.DATA_TYPE_INT, "CBaseEntity", "m_iHealth", value);
            }
        }

        /// <summary>
        /// Player Color, f.e shall be used to draw smokes in the same color
        /// </summary>
        public Color PlayerColor
        {
            get
            {
                return Color.White;
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
        public Player(int userid)
        {
            //ToDo initialize player with current values such as SteamID, Name etc.
            PawnHandle = userid;
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
            Server.ExecuteCommand($"kickid {PawnHandle}");
        }

        /// <summary>
        /// slay player
        /// </summary>
        public void Kill()
        {
            Server.ExecuteCommand($"slay {PawnHandle}");
        }
    }


}
