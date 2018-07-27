using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
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
        }

        private readonly string IP = "127.0.0.1";
        private readonly int ReceivePort = 11000;
        private readonly int SendPort = 12000;
        private readonly string UserId = "8610e81fd73c4bd4894cdf41a7a0e88b";

        private void send_Click(object sender, EventArgs e)
        {
            SendMessage();
            textBox1.Text = string.Empty;
        }

        public void SendMessage()
        {
            Client client = new Client(textBox1,panel1,this.UserId);
            client.StartClient(this.IP, this.SendPort);
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
        }

        /// <summary>
        /// panel控件的事件：用户或代码滚动工作时发生  
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void panel1_Scroll(object sender, ScrollEventArgs e)
        {
            if (e.NewValue <=MinimumSize.Width && e.NewValue >=MaximumSize.Width)
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
    }
}
