using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.ComponentModel;

namespace IPTV_Sharp
{
    public partial class Form1 : Form
    {
        private SearchHelper searcher;
        private DictionaryManager dictionary;
        private XtreamCrawler crawler;
        private BackgroundWorker crawler_worker;

        public int num_found;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

            StatusBoxWrite("Initialising IPTV# - Please wait." + Environment.NewLine);

            StatusBoxWrite("Searching IPTVs on the web...");
            searcher = new SearchHelper();    
            List<string> results = searcher.DoSearch("Xtream+Codes+v1.0.59.5&kl=it-it");
            comboBox1.DataSource = results;
            StatusBoxWrite("Done. Showing" + results.Count + " Results." + Environment.NewLine);
            
            StatusBoxWrite("Loading word dictionary...");
            dictionary = new DictionaryManager("part_list");
            dictionary.LoadDictionaries();

            if (dictionary.entries.Count > 0)
            {
                StatusBoxWrite(dictionary.entries.Count + " dictionary entries loaded. Ready." + Environment.NewLine);
                button1.Enabled = true;
            }
            else
            {
                StatusBoxWrite("No dictionary entries loaded. Not Ready.");
            }

            StatusBoxWrite("\"Force Crawl\" disables site validity check.");
            StatusBoxWrite("\"Try Uppercase First Letter\" changes the first letter of your dictionary entries to uppercase when sending requests. (Example: mario -> Mario)");
            StatusBoxWrite("Only for demo purposes. Be warned as crawl time may increase drastically depending on target site and dictionary size.");
            num_found = 0;
            
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (comboBox1.Text == "")
            {
                MessageBox.Show("You must select a target.", "No target", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            else
            {
                string server = comboBox1.Text;
                StatusBoxWrite("Initiating Attack. Check Progress Bar.");

                if (force_checkbox.Checked)
                {
                    StatusBoxWrite("Force Crawl selected. Bypassing site validity check.");
                }

                crawler = new XtreamCrawler(dictionary, server, 10, force_checkbox.Checked, uppercase_check.Checked);
                crawler_worker = new BackgroundWorker();
                crawler_worker.DoWork += new DoWorkEventHandler(crawler.DoCrawl);
                crawler_worker.RunWorkerAsync();

                progressBar1.Maximum = dictionary.entries.Count;

                button1.Enabled = false;
                timer1.Interval = 100;
                timer1.Start();
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            progressBar1.Value = crawler.progress;

            if (num_found < crawler.num_results)
            {
                num_found = crawler.num_results;
                StatusBoxWrite("Found " + num_found + " channels so far.");
            }

            switch (crawler.status)
            {
                case CrawlerStatus.Aborted:
                    StatusBoxWrite("Attack aborted. Something went wrong.");
                    timer1.Stop();
                    button1.Enabled = true;
                    break;

                case CrawlerStatus.InvalidSite:
                    StatusBoxWrite("Target is not an IPTV site or something went wrong. Try forcing crawling anyway by selecting \"Force Crawl\"");
                    timer1.Stop();
                    button1.Enabled = true;
                    break;

                case CrawlerStatus.CompletedWithoutResults:
                    StatusBoxWrite("Attack completed. No Results.");
                    timer1.Stop();
                    button1.Enabled = true;
                    break;

                case CrawlerStatus.CompletedWithResults:
                    StatusBoxWrite("Attack completed. Check output directory for results.");
                    timer1.Stop();
                    button1.Enabled = true;
                    break;
            }


        }

        private void StatusBoxWrite(string message)
        {
            statusBox.Text += message + Environment.NewLine;
            statusBox.SelectionStart = statusBox.Text.Length;
            statusBox.ScrollToCaret();
        }
    }
}
