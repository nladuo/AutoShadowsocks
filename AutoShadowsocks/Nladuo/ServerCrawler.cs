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
        public static readonly string hishadowsocks_url = "http://www.hishadowsocks.com/";
        public static readonly string ezlink_url = "https://www.ezlink.hk/free.php";
        

        private ShadowsocksController controller;
        private Configuration configuration;
        public bool isChange { get; set; }

        public bool isErrOcurred { get; set; }

        public List<Server> serverList { get; set; }

        public ServerCrawler(ShadowsocksController controller)
        {
            this.controller = controller;
            configuration = controller.GetConfigurationCopy();
            
            this.isChange = false;
            this.isErrOcurred = false;

            this.serverList = new List<Server>();
            foreach (var server in configuration.configs)
            {
                if (server.remarks == CRAWLER_REMARKS)
                {
                    this.serverList.Add(server);
                }
            }
        }

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

        

        public void updateServers()
        {
            List<Server> servers = new List<Server>();
            
           
            
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


        private Server crawlhishadowsocks()
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
    }
}
