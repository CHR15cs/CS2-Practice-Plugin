using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSPracc.DataModules
{
    public class HtmlMessage
    {
        public string Message { get; init; }

        public int SecondsToDisplay { get; init; }

        public DateTime DateTimeEnd { get; set; }

        public HtmlMessage(string message,int secondsToDisplay = 5)
        {
            Message = message;
            SecondsToDisplay = secondsToDisplay;
            DateTimeEnd = DateTime.Now.AddSeconds(secondsToDisplay);
        }
    }
}
