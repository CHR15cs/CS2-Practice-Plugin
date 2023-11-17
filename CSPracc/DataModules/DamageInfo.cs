using CounterStrikeSharp.API.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSPracc.DataModules
{
    public struct Damage
    {
        public int DmgGiven;
        public int HitsGiven;
        public int DmgTaken;
        public int HitsTaken;
    }
    public class DamageInfo
    {
        CCSPlayerController DamageIssuer;

        public Dictionary<CCSPlayerController, Damage> DamageGiven;

        public DamageInfo(CCSPlayerController DamageIssuer) 
        {
            DamageGiven = new Dictionary<CCSPlayerController, Damage> ();
        }

        public void AddDamage(CCSPlayerController victim,int damage)
        {
            if(DamageGiven.ContainsKey(victim))
            {
                if(!DamageGiven.TryGetValue(victim,out Damage damageTaken))
                {
                    Damage dmg = new Damage();
                    dmg.DmgGiven = damage;
                    dmg.HitsGiven = 1;
                    DamageGiven.Add(victim, dmg);
                    return;
                }
                
                damageTaken.DmgGiven += damage;
                damageTaken.HitsGiven += 1;
                DamageGiven[victim] = damageTaken;
            }
            else
            {
                Damage dmg = new Damage();
                dmg.DmgGiven = damage;
                dmg.HitsGiven = 1;
                DamageGiven.Add(victim, dmg);
            }
        }

        public void TakeDamage(CCSPlayerController Issuer,int damage)
        {
            if (DamageGiven.ContainsKey(Issuer))
            {
                if (!DamageGiven.TryGetValue(Issuer, out Damage damageTaken))
                {
                    Damage dmg = new Damage();
                    dmg.DmgTaken = damage;
                    dmg.HitsTaken = 1;
                    DamageGiven.Add(Issuer, dmg);
                    return;
                }
                damageTaken.DmgTaken += damage;
                damageTaken.HitsTaken += 1;
                DamageGiven[Issuer] = damageTaken;
            }
            else
            {
                Damage dmg = new Damage();
                dmg.DmgTaken = damage;
                dmg.HitsTaken = 1;
                DamageGiven.Add(Issuer, dmg);
            }
        }
    }
}
