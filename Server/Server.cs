using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Server
{
    public class Server
    {
        public string UserId;
        private Panel panel { get; set; }
        private readonly Socket Listener;

        public Server(Panel panel, string ip, int port,string userid)
        {
            this.panel = panel;
            this.UserId = userid;

            IPHostEntry iPHostEntry = Dns.Resolve(ip);
            IPAddress iPAddress = iPHostEntry.AddressList[0];
            IPEndPoint iPEndPoint = new IPEndPoint(iPAddress, port);

            Listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            Listener.Bind(iPEndPoint);//Bind ip
            Listener.Listen(100);//Start Listen

            Thread getmessagethread = new Thread(ShowOldMessage);
            getmessagethread.Start();
        }

        private void ShowOldMessage()
        {
            Data data = new Data();
            List<MessageModel> messageModels = data.NoParameterQueryMessages();
            if (messageModels.Count == 0)
            {
                Label nomessagelabel = new Label();
                nomessagelabel.Text = "No Old Message";
                nomessagelabel.AutoSize = true;
                nomessagelabel.Location = new Point((panel.Width - nomessagelabel.Width) / 2, nomessagelabel.Height);
                AddControlToPanel(nomessagelabel);
                return;
            }

            foreach (var obj in messageModels)
            {
                Label messagelabel = new Label();
                messagelabel.Text = string.Format(obj.Message + "     :" + obj.CreateTime);
                messagelabel.AutoSize = true;
                messagelabel.Name = obj.Id;

                if (obj.UserId == this.UserId)
                {
                    messagelabel.Location = new Point(panel.Width - messagelabel.Width * 2, messagelabel.Height);
                }
                AddControlToPanel(messagelabel);
            }
        }

        /// <summary>
        /// 开始连接 监听
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port">端口</param>
        public void StartListening()
        {
            byte[] bytes = new Byte[1024];//Data buffer for incoming data
            try
            {
                Listener.BeginAccept(new AsyncCallback(AcceptCallBack), Listener);
            }
            catch (Exception e)
            {
                Label exceptionlabel = new Label();
                exceptionlabel.Text = "some error:" + e;
                exceptionlabel.AutoSize = true;
                AddControlToPanel(exceptionlabel);
            }
        }

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
        private delegate void ConreolDelegete(Control control);

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

        public void AcceptCallBack(IAsyncResult ar)
        {
            Socket listener = (Socket)ar.AsyncState;
            Socket handler = listener.EndAccept(ar);

            StateObject stateObject = new StateObject();
            stateObject.workSocket = handler;
            handler.BeginReceive(stateObject.buffer,0,StateObject.bufferSize,0,new AsyncCallback(ReadCallBack),stateObject);

            Listener.BeginAccept(new AsyncCallback(AcceptCallBack), Listener);
        }

        public void ReadCallBack(IAsyncResult ar)
        {
            string content = string.Empty;

            StateObject stateObject = (StateObject)ar.AsyncState;
            Socket handler = stateObject.workSocket;
            Data data = new Data();
            MessageModel messageModel = new MessageModel();

            int bytesRead = handler.EndReceive(ar);
            if (bytesRead > 0)
            {
                stateObject.sb.Append(Encoding.UTF8.GetString(stateObject.buffer, 0, bytesRead));

                content = stateObject.sb.ToString();
                if (content.IndexOf("<END>") > -1)
                {
                    Label receivedata = new Label();                 

                    string[] sArray = content.Split(new string[] { "<UserId>","<END>" }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (string e in sArray)
                    {
                        messageModel.Message = sArray[0];
                        messageModel.CreateTime = DateTime.Now;
                        receivedata.Text = sArray[0] + "     :" + DateTime.Now;
                        messageModel.UserId = sArray[1];
                    }
                    receivedata.AutoSize = true;
                    AddControlToPanel(receivedata);

                    bool result = data.InsertMessage(messageModel);
                    if (!result)
                    {
                        Console.WriteLine("Insert Message Error");
                    }

                    Send(handler, receivedata.Text);//返回信息
                }
                else
                {
                    handler.BeginReceive(stateObject.buffer, 0, StateObject.bufferSize, 0, new AsyncCallback(ReadCallBack), stateObject);
                }
            }
        }

        public void Send(Socket handler,string data)
        {
            byte[] byteData = Encoding.UTF8.GetBytes(data);

            handler.BeginSend(byteData,0,byteData.Length,0,new AsyncCallback(SendCallBack),handler);
        }

        public void SendCallBack(IAsyncResult ar)
        {
            try
            {
                Socket handler = (Socket)ar.AsyncState;

                int bytesSend = handler.EndSend(ar);
                handler.Shutdown(SocketShutdown.Both);
                handler.Close();

                Listener.BeginAccept(new AsyncCallback(AcceptCallBack), Listener);
            }
            catch (Exception e)
            {
                Label exception = new Label();               
                exception.Text = "some error:" + e;
                exception.AutoSize = true;
                AddControlToPanel(exception);
            }
        }
    }

    public class StateObject
    {
        public Socket workSocket { get; set; }

        public const int bufferSize = 1024;

        public byte[] buffer = new byte[bufferSize];

        public StringBuilder sb = new StringBuilder();
    }
}
