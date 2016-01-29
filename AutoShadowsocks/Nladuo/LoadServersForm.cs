using Shadowsocks.Controller;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace Shadowsocks.Nladuo
{
    public partial class LoadServersForm : Form
    {
        string[] waitTexts = { "加载中.", "加载中..", "加载中...", "加载中...." };
        int times = 0;
        int index = 1;
        private ServerCrawler crawler;


        public LoadServersForm(ShadowsocksController controller)
        {
            InitializeComponent();
            //取消跨线程检查
            Control.CheckForIllegalCrossThreadCalls = false;
            crawler = new ServerCrawler(controller);
        }

        private void LoadServersForm_Load(object sender, EventArgs e)
        {
            timer.Interval = 20;
            timer.Start();

            Thread th = new Thread(new ThreadStart(requestForServers)); 
            th.Start();
            
        }

        void requestForServers()
        {
            crawler.updateServers();
            if (!crawler.isErrOcurred)
            {
                if (!crawler.isChange)
                {
                    MessageBox.Show("已经是最新的ss账号了");
                }
                else
                {
                    crawler.saveServers();
                    MessageBox.Show("更新成功");
                }
                
            }
            this.Close();

        }

        private void timer_Tick(object sender, EventArgs e)
        {
            times++;
            if (times == 30)
            {
                times = 0;
                loadLabel.Text = waitTexts[index];
                index++;
                if (index == waitTexts.Length)
                {
                    index = 0;
                }
            }
        }

    }
}
