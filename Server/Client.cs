using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Server
{
    public class Client
    {
        private static string response = string.Empty;

        private TextBox textbox { get; set; }
        private Panel panel { get; set; }
        private readonly string UserId;

        public Client(TextBox textbox, Panel panel,string userid)
        {
            this.textbox = textbox;
            this.panel = panel;
            this.UserId = userid;
        }

        public void StartClient(string ip, int port)
        {
            try
            {
                byte[] receiveBytes = new byte[1024];
                IPHostEntry iPHost = Dns.Resolve(ip);
                IPAddress iPAddress = iPHost.AddressList[0];
                IPEndPoint iPEnd = new IPEndPoint(iPAddress, port);

                Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                client.BeginConnect(iPEnd, new AsyncCallback(ConnectCallBack), client);

                Send(client, textbox.Text);

                Receive(client);
            }
            catch (Exception e)
            {
                Label error = new Label();
                error.Text = "some error:" + e;
                error.AutoSize = true;
                AddControlToPanel(error);
            }
        }

        private delegate void ConreolDelegete(Control control);

        /// <summary>
        /// 添加控件到Panel容器
        /// </summary>
        /// <param name="newcontrol"></param>
        public void AddControlToPanel(Control newcontrol)
        {
            if (panel.InvokeRequired)
            {
                ConreolDelegete objSet = new ConreolDelegete(AddControlToPanel);
                panel.Invoke(objSet, new object[] { newcontrol });
            }
            else
            {
                newcontrol.Location = CountPoint(newcontrol);
                panel.Controls.Add(newcontrol);
            }
        }

        /// <summary>
        /// 得到容器里控件的最新排列坐标
        /// </summary>
        /// <param name="editpointcontrol">要修改坐标的控件</param>
        /// <returns></returns>
        public Point CountPoint(Control editpointcontrol)
        {
            int count = panel.Controls.Count;
            Control control = new Control();
            if (count != 0)
            {
                control = panel.Controls[count - 1];//得到容器里的最后一个控件(因为所有控件都从上而下排列 所有最后的控件就是Height值最大的控件)
            }

            Point point = new Point(editpointcontrol.Location.X, control.Location.Y + editpointcontrol.Height);
            return point;
        }

        public void ConnectCallBack(IAsyncResult ar)
        {
            try
            {
                Socket client = (Socket)ar.AsyncState;
                client.EndConnect(ar);
            }
            catch (Exception e)
            {
                Label error = new Label();
                error.Text = "some error:" + e;
                error.AutoSize = true;
                AddControlToPanel(error);
            }
        }

        public void Send(Socket client, string data)
        {
            byte[] byteData = Encoding.UTF8.GetBytes(data + "<UserId>" + this.UserId + "<END>");
            client.BeginSend(byteData, 0, byteData.Length, 0, new AsyncCallback(SendBackCall), client);
        }

        public void SendBackCall(IAsyncResult ar)
        {
            try
            {
                Socket client = (Socket)ar.AsyncState;
                int bytesSent = client.EndSend(ar);
            }
            catch (Exception e)
            {
                Label error = new Label();
                error.Text = "some error:" + e;
                error.AutoSize = true;
                AddControlToPanel(error);
            }
        }

        public void Receive(Socket client)
        {
            try
            {
                StateObject stateObject = new StateObject();
                stateObject.workSocket = client;

                client.BeginReceive(stateObject.buffer, 0, StateObject.bufferSize, 0, new AsyncCallback(ReceiveBackCall), stateObject);
            }
            catch (Exception e)
            {
                Label error = new Label();
                error.Text = "some error:" + e;
                error.AutoSize = true;
                AddControlToPanel(error);
            }
        }

        public void ReceiveBackCall(IAsyncResult ar)
        {
            try
            {
                StateObject state = (StateObject)ar.AsyncState;
                Socket client = state.workSocket;

                int bytesRead = client.EndReceive(ar);
                if (bytesRead > 0)
                {
                    state.sb.Append(Encoding.UTF8.GetString(state.buffer, 0, bytesRead));

                    client.BeginReceive(state.buffer, 0, StateObject.bufferSize, 0, new AsyncCallback(ReceiveBackCall), state);//检查有没接收完
                }
                else
                {
                    if (state.sb.Length > 1)
                    {
                        response = state.sb.ToString();

                        Label sendmessage = new Label();
                        sendmessage.Text = string.Format(response + "     :" + DateTime.Now);
                        sendmessage.AutoSize = true;
                        sendmessage.Location = new Point(panel.Width - sendmessage.Width * 2, sendmessage.Height);
                        AddControlToPanel(sendmessage);

                        client.Shutdown(SocketShutdown.Both);
                        client.Close();
                    }
                }
            }
            catch (Exception e)
            {
                Label error = new Label();
                error.Text = "some error:" + e;
                error.AutoSize = true;
                AddControlToPanel(error);
            }
        }
    }
}
