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
using System.Threading;

namespace Shadowsocks.Nladuo
{
    class ServerCrawler
    {
        public static readonly string CRAWLER_REMARKS = "从网络中获取的ss账号";//免费账号的标签

        public static readonly string hishadowsocks_url = "http://www.hishadowsocks.com/";
        public static readonly string ezlink_url = "https://www.ezlink.hk/free.php"; //暂时好像用不了了
        public static readonly string ishadowsocks_url = "http://a.ishadow.co//";
        

        private ShadowsocksController controller;
        private Configuration configuration;

        public List<Server> serverList { get; set; }

        public ServerCrawler(ShadowsocksController controller)
        {
            this.controller = controller;
            configuration = controller.GetConfigurationCopy();

            this.serverList = new List<Server>();
            foreach (var server in configuration.configs)
            {
                if (server.remarks == CRAWLER_REMARKS)
                {
                    this.serverList.Add(server);
                }
            }
        }

        /// <summary>
        /// 保存免费的服务器
        /// </summary>
        public void saveServers()
        {
            foreach (var server in this.serverList)
            {
                bool isSaved = false;
                for (int i = 0; i < configuration.configs.Count; i++)
                {
                    if (server.server == configuration.configs[i].server) //爬到的server名称不可能相同
                    {
                        configuration.configs[i] = server;
                        isSaved = true;
                    }
                }
                if (!isSaved)
                {
                    configuration.configs.Add(server);
                }
            }
            controller.SaveServers(configuration.configs, configuration.localPort);
        }

        /// <summary>
        /// 开启一个线程来异步爬取
        /// </summary>
        public void asyncRequest()
        {
            Thread th = new Thread(new ThreadStart(updateServers));
            th.Start();
        }

        /// <summary>
        /// 获取并更新免费ss服务器列表
        /// </summary>
        public void updateServers()
        {
            List<Server> servers = new List<Server>();
            //Server server1 = crawlEzlink();
            //if (server1 != null)
            //{
            //    servers.Add(server1);
            //}

            //爬取hishadowsocks的免费账号
            //Server server = crawlHiShadowsocks();
            //if (server != null)
            //{
            //    servers.Add(server);
            //}

            //爬取ishadowsocks的免费账号
            List<Server> temp_servers = crawIShadowsocks();
            if (temp_servers != null)
            {
                foreach (var s in temp_servers)
                {
                    servers.Add(s);
                }
            }

            //显示爬取结果
            if (servers.Count == 0)
            {
                MessageBox.Show("请求失败，尝试关闭代理后重试");
            }
            else if (isServersEqual(this.serverList, servers))
            {
                MessageBox.Show("已经是最新了，不需要更新");
            }
            else
            {
                this.serverList = servers;
                this.saveServers();
                MessageBox.Show("更新成功");
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
                for (int j = 0; j < s2.Count; j++)
                {
                    if ( (s1[i].server == s2[j].server) &&
                        (s1[i].password != s2[j].password) )
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// 爬取http://www.hishadowsocks.com/的免费账号
        /// </summary>
        /// <returns></returns>
        private Server crawlHiShadowsocks()
        {
            Server server = new Server();
            server.remarks = CRAWLER_REMARKS;
            try
            {
                HttpWebResponse response = HttpWebResponseUtility.CreateGetHttpResponse(hishadowsocks_url, null, null, null);
                Stream dataStream = response.GetResponseStream();
                StreamReader reader = new StreamReader(dataStream, Encoding.GetEncoding("utf-8"));
                string responseFromServer = reader.ReadToEnd();
                dataStream.Close();
                reader.Close();
                response.Close();
                HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
                doc.LoadHtml(responseFromServer);
                HtmlNode node = doc.DocumentNode;
                HtmlNodeCollection datas = node.SelectNodes("//div[@class='col-md-6']/p");
                int count = 0;
                foreach (var data in datas)
                {
                    string[] texts = data.InnerText.Split(':');
                    switch (count)
                    {
                        case 1: server.server = texts[1]; break;
                        case 2: server.server_port = int.Parse(texts[1]); break;
                        case 3: server.password = texts[1]; break;
                        case 4: server.method = texts[1]; break;
                    }
                    count++;
                }
                return server;

            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// 爬取http://www.ishadowsocks.com/的免费账号
        /// </summary>
        /// <returns></returns>
        private List<Server> crawIShadowsocks()
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
                 HtmlNodeCollection datas = node.SelectNodes("//div[@class='col-sm-4 text-center']");

                 int count = 0;
                 foreach (var data in datas)
                 {
                     MessageBox.Show(data.InnerText);
                     if (count >= 3)
                     {
                         break;
                     }
                     count++;
                     string[] strs = data.InnerText.Split('\n');
                     MessageBox.Show(strs[1].Trim().Split(':')[1]);
                     Server server = new Server();
                     server.remarks = CRAWLER_REMARKS;
                     server.server = strs[1].Trim().Split(':')[1];
                     server.server_port = int.Parse(strs[2].Trim().Split(':')[1]);
                     server.password = strs[3].Trim().Split(':')[1];
                     server.method = strs[4].Trim().Split(':')[1];
                     if (server.password.Length > 0)
                     {
                         servers.Add(server);
                     }
                 }
                 return servers;   
             }
             catch (Exception)
             {
                 return null;
             }
        }

        /// <summary>
        /// 爬取https://www.ezlink.hk/的免费账号
        /// </summary>
        /// <returns></returns>
        private Server crawlEzlink()
        {
            Server server = new Server();
            server.remarks = CRAWLER_REMARKS;
            try
            {
                HttpWebResponse response = HttpWebResponseUtility.CreateGetHttpResponse(ezlink_url, null, null, null);
                Stream dataStream = response.GetResponseStream();
                StreamReader reader = new StreamReader(dataStream, Encoding.GetEncoding("utf-8"));
                string responseFromServer = reader.ReadToEnd();
                dataStream.Close();
                reader.Close();
                response.Close();
                HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
                doc.LoadHtml(responseFromServer);
                HtmlNode node = doc.DocumentNode;
                HtmlNodeCollection datas = node.SelectNodes("//tr/td");
                int count = 0;
                foreach (var data in datas)
                {

                    switch (count)
                    {
                        case 13: server.server = data.InnerText; break;
                        case 15: server.server_port = int.Parse(data.InnerText); break;
                        case 17: server.password = data.InnerText; break;
                        case 19: server.method = data.InnerText; break;
                    }

                    count++;
                }
                return server;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
