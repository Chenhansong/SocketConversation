using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Server;

namespace WebAListener
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            this.client = new Client(textBox1, panel1, this.UserId, this.IP, this.SendPort);
        }

        private readonly string IP = "127.0.0.1";
        private readonly int ReceivePort = 11000;
        private readonly int SendPort = 12000;
        private readonly string UserId = "8610e81fd73c4bd4894cdf41a7a0e88b";
        private readonly long fileSize = 8388608;
        private readonly Client client;

        private void send_Click(object sender, EventArgs e)
        {
            SendMessage();
        }

        public void SendMessage()
        {
            client.Send(textBox1.Text);
        }

        private void StartServer()
        {
            Server.Server serverListen = new Server.Server(panel1, this.IP, this.ReceivePort,this.UserId);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Control.CheckForIllegalCrossThreadCalls = false;

            StartServer();
        }

        /// <summary>
        /// panel控件的事件：在向该控件添加控件时发生
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void panel1_ControlAdded(object sender, ControlEventArgs e)
        {
            this.panel1.VerticalScroll.Enabled = true;
            this.panel1.VerticalScroll.Visible = true;
            this.panel1.Scroll += panel1_Scroll;

            Point newPoint = new Point(0, this.panel1.Height - panel1.AutoScrollPosition.Y);
            panel1.AutoScrollPosition = newPoint;
        }

        /// <summary>
        /// panel控件的事件：用户或代码滚动工作时发生  
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void panel1_Scroll(object sender, ScrollEventArgs e)
        {
            if (e.NewValue <= MinimumSize.Width && e.NewValue >= MaximumSize.Width)
            {
                this.panel1.VerticalScroll.Value = e.NewValue;
            }

        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                SendMessage();
                textBox1.Text = string.Empty;
            }
        }

        private void filebutton_ControlAdded(object sender, ControlEventArgs e)
        {
            
        }

        private void filebutton_Click(object sender, EventArgs e)
        {
            //初始化一个OpenFileDialog类
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.Multiselect = true;//允许选多个文件
            fileDialog.Filter = "(*.jpg, *.gif, *.png) | *.jpg; *.gif; *.png";

            //判断用户是否正确的选择了文件
            if (fileDialog.ShowDialog() == DialogResult.OK)
            {
                List<string> filelist = new List<string>();
                if (fileDialog.FileNames.Length > 3)//一次只能发三张
                {
                    MessageBox.Show("soory,Only three pictures can be sent at a time");
                    return;
                }
                foreach (var file in fileDialog.FileNames)
                {
                    //获取用户选择的文件，并判断文件大小不能超过20K，fileInfo.Length是以字节为单位的
                    FileInfo fileInfo = new FileInfo(file);
                    if (fileInfo.Length > this.fileSize)
                    {
                        MessageBox.Show("The picture can't be bigger than 8m");
                    }
                    else
                    {
                        filelist.Add(file);
                    }
                }
                this.client.SendFile(filelist, textBox1.Text);
            }
        }
    }
}
