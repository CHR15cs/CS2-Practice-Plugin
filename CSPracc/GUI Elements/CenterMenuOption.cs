using CounterStrikeSharp.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSPracc.GUI_Elements
{
    public class CenterMenuOption
    {
        private int _index = -1;
        public string Text { get; init; }
        Task TaskToExecute { get; set; }
        public CenterMenuOption(string optionText,Task taskToExecute) 
        { 
               Text = optionText;
            TaskToExecute = taskToExecute;
        }

        public void SetIndex (int index) 
        { 
            _index = index;
        }
        public void RunTask()
        {
            Server.PrintToConsole($"Execute Task {Text}");
            TaskToExecute.Start();
        }
    }
}
