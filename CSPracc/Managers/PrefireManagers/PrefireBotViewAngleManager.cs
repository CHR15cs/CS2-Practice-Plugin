using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSPracc.Managers.PrefireManagers
{
    public class PrefireBotViewAngleManager : IDisposable
    {
        CSPraccPlugin Plugin;
        CCSPlayerController PlayerToShoot;
        public PrefireBotViewAngleManager(ref CSPraccPlugin plugin,CCSPlayerController playerController)
        {
            Plugin = plugin;
            PlayerToShoot = playerController;
        }

        public void Dispose()
        {
            Listeners.OnTick onTick = new Listeners.OnTick(OnTick);
            Plugin.RemoveListener("on_tick", onTick);
        }

        private void OnTick()
        {
            var bots = Utilities.GetPlayers().Where(x => x.IsBot && x.IsValid && !x.IsHLTV);
            foreach (var bot in bots)
            {
                bot.PlayerPawn.Value!.MoveType = MoveType_t.MOVETYPE_NONE;
                if (PlayerToShoot != null)
                {
                    bot.PlayerPawn.Value.EyeAngles.Y = PlayerToShoot.PlayerPawn.Value!.EyeAngles.Y - 180.0f;
                    bot.PlayerPawn.Value.EyeAngles.X = PlayerToShoot.PlayerPawn.Value.EyeAngles.X + 20;
                }
            }
        }
    }
}
