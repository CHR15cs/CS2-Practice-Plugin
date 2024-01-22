using CounterStrikeSharp.API.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSPracc.DataModules
{
    public class PlayerReplay
    {
        string ReplayName {  get; set; }
        public List<PlayerFrame> frames;
        public PlayerReplay(string replayName) 
        { 
            frames = new List<PlayerFrame>();
            ReplayName = replayName;
        }

        public void RecordFrame(CCSPlayerController player)
        {
            frames.Add(new PlayerFrame(player));
        }

        public void AddFrame(PlayerFrame frame)
        {
            frames.Add(frame);
        }

        public PlayerFrame? GetNextFrame()
        {
            if(frames.Count > 0)
            {
                PlayerFrame? frame = frames.FirstOrDefault();
                if(frame != null)
                {
                    frames.Remove(frame);
                }
                return frame;
            }
            return null;
        }
    }
}
