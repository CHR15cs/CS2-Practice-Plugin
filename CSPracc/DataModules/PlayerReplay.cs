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
        public string ReplayName {  get; set; }
        public List<PlayerFrame> frames;
        public int frameCount = 0;
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
            if (frames.Count > frameCount)
            {
                PlayerFrame? frame = frames[frameCount];
                if(frame != null)
                {
                    frameCount++;
                }
                return frame;
            }
            return null;
        }
    }
}
