using CounterStrikeSharp.API.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSPracc.DataModules
{
    public class DamageInfo
    {
        public CCSPlayerController? Player1;
        protected int HitsPlayer1 { get; set; } = 0;
        protected int DamageTakenP1 { get; set; } = 0;

        public DamageInfo(CCSPlayerController player1, CCSPlayerController player2) 
        {
            Player1 = player1;
        }
        public void Update(CCSPlayerController damageTaker, int damage)
        {
            if(damageTaker == null) { return; }
            HitsPlayer1++;
            DamageTakenP1 += damage;
        }
    }
}
