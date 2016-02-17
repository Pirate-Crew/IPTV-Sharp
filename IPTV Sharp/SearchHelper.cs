using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace IPTV_Sharp
{
    public class SearchHelper
    {
        private string search_engine;

        public SearchHelper()
        {
            search_engine = "https://duckduckgo.com/html/?q=";
        }

        public List<string> DoSearch(string search_string)
        {
            List<string> results = new List<string>();

            HtmlWeb crawler = new HtmlWeb();
            HtmlDocument doc = crawler.Load(search_engine + search_string);

            foreach (HtmlNode link in doc.DocumentNode.SelectNodes("//a[@href]"))
            {
                HtmlAttribute att = link.Attributes["href"];

                if (att.Value.Contains("http://"))
                {
                    string[] temp = att.Value.Split('/');
                    string tempx = temp[0] + "//" + temp[2];
                    if (!results.Any(tempx.Contains) && temp[2].Contains(":"))
                    {
                        results.Add(tempx);
                    }
                }
            }

            return results;
        }
    }
}
