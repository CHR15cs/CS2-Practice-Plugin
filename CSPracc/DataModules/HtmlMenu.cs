using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSPracc.DataModules
{
    public class HtmlMenu
    {
        public int Page = 0;
        public bool CloseOnSelect { get; init; }
        public string Title { get; init; }
        public List<KeyValuePair<string, Task>> Options { get; init; }
        public HtmlMenu(string title,List<KeyValuePair<string,Task>> options, bool closeOnSelect = true) 
        {
            Title = title;
            Options = options;
            CloseOnSelect = closeOnSelect;
        }
    }
}
