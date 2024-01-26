using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSPracc.DataModules
{
    public class HtmlMenu
    {
        public int ItemsPerSite { get; set; } = 6;
        public int Page = 0;
        public bool CloseOnSelect { get; init; }
        public string Title { get; init; }
        public List<KeyValuePair<string, Action>> Options { get; init; }
        public HtmlMenu(string title,List<KeyValuePair<string,Action>> options, bool closeOnSelect = true) 
        {
            Title = title;
            Options = options;
            CloseOnSelect = closeOnSelect;
            MenuPages = new List<List<KeyValuePair<string, Action>>>();
            initPages();
        }

        private void initPages()
        {
            ItemsPerSite = 6;
            int maxCharPerNadeLine = 30;
            int pagecount = 0;
            List<KeyValuePair<string,Action>> page = new List<KeyValuePair<string, Action>>();
            for (int option = 0; option < Options.Count; option++)
            {
                if (Options[option].Value != null)
                {
                    int length = Options[option].Key.Length;
                    while ( length > maxCharPerNadeLine)
                    {
                        ItemsPerSite--; 
                        length -= maxCharPerNadeLine;
                    }
                    if(ItemsPerSite > 0)
                    {
                        page.Add(Options[option]);
                        ItemsPerSite--;
                        if(option +1 >= Options.Count)
                        {
                            MenuPages.Add(page);
                            page = new List<KeyValuePair<string, Action>>();
                        }
                    }
                    else
                    {
                        MenuPages.Add(page);
                        page = new List<KeyValuePair<string,Action>>();
                        ItemsPerSite = 6;
                        option--;
                    }
                }
            }
        }

        public List<List<KeyValuePair<string,Action>>> MenuPages { get; init; } = new List<List<KeyValuePair<string, Action>>> ();
    }
}
