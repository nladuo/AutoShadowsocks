using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Shadowsocks.Controller;
using Shadowsocks.Model;
using System.Windows.Forms;
using System.Net;
using System.IO;
using HtmlAgilityPack;

namespace Shadowsocks.Nladuo
{
    class ServerCrawler
    {
        public static readonly string CRAWLER_REMARKS = "从网络中获取的ss账号";
        public static readonly string ishadowsocks_url = "http://www.ishadowsocks.com/";
        private static readonly int interval = 10000; // 10 seconds
        private Timer timer = null;
        private ShadowsocksController controller;
        private bool isStart;

        public List<Server> serverList { get; set; }

        public ServerCrawler(ShadowsocksController controller)
        {
            this.controller = controller;
            this.isStart = false;
            timer = new Timer();
            timer.Interval = interval;
            timer.Tick += updateServers;
            this.serverList = new List<Server>();
        }

        private void updateServers(object sender, EventArgs e)
        {
            List<Server> servers = new List<Server>();
            
            try
            {
                HttpWebResponse response = HttpWebResponseUtility.CreateGetHttpResponse(ishadowsocks_url, null, null, null);
                Stream dataStream = response.GetResponseStream();
                StreamReader reader = new StreamReader(dataStream, Encoding.GetEncoding("utf-8"));
                string responseFromServer = reader.ReadToEnd();
                dataStream.Close();
                reader.Close();
                response.Close();
                HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
                doc.LoadHtml(responseFromServer);
                HtmlNode node = doc.DocumentNode;
                HtmlNodeCollection datas = node.SelectNodes("//div[@class='col-lg-4 text-center']");
                string resultText = "";
                int count = 0;
                foreach (var data in datas)
                {
                    if (count >= 3)
                    {
                        break;
                    }
                    count++;
                    //MessageBox.Show(count + "" + data.InnerText);
                    string[] strs = data.InnerText.Split('\n');
                    Server server = new Server();
                    server.remarks = CRAWLER_REMARKS;
                    server.server = strs[1].Trim().Split(':')[1];
                    server.server_port = int.Parse(strs[2].Trim().Split(':')[1]);
                    server.method = strs[3].Trim().Split(':')[1];
                    servers.Add(server);
                        
                    
                }
                if (!this.isServersEqual(servers, this.serverList))
                {
                    this.serverList = servers;
                    MessageBox.Show("true");
                }
                else
                {
                    MessageBox.Show("false");
                }
                
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + ex.StackTrace);
                Console.WriteLine("出现错误:" + ex.Message);
            }
        }

        private bool isServersEqual(List<Server> s1, List<Server> s2)
        {
            if(s1.Count != s2.Count)
            {
                return false;
            }
            for (int i = 0; i < s1.Count; i++)
            {
                if (s1[i].password != s2[i].password)
                {
                    return false;
                }
            }
            return true;
        }


        public void Stop()
        {
            if (this.isStart)
            {
                this.timer.Stop();
                this.isStart = false;
            }
        }

        public void Start()
        {
            if (!this.isStart)
            {
                timer.Start();
                this.isStart = true;
            }
            //updateServers(null, null);
            
            
        }
    }
}
