using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Server
{
    public class Server
    {
        public string UserId;
        private Panel panel { get; set; }
        private readonly Socket Listener;
        private readonly string saveFilePath = @"E:\IT\Exercise\SocketConversationOne\Server\ReceiveImg\";//写入的文件路径

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

            Thread getmessagethread = new Thread(QueryOldMessage);
            getmessagethread.Start();

            StartListening();
        }

        private void QueryOldMessage()
        {
            Data data = new Data();
            List<MessageModel> messageModels = data.NoParameterQueryMessages();
            if (messageModels.Count == 0)
            {
                Label nomessagelabel = CreateLabel("No Old Message");
                nomessagelabel.Location = new Point((this.panel.Width - nomessagelabel.Width) / 2, nomessagelabel.Height);
                AddControlToPanel(nomessagelabel);
                return;
            }
            else
            {
                ShowOldMessage(messageModels);
            }
        }

        private void ShowOldMessage(List<MessageModel> messageModels)
        {
            foreach (var obj in messageModels)
            {
                if (obj.MessageText != "" && obj.MessageText != null)
                {
                    string text = string.Format(obj.MessageText + "     :" + obj.CreateTime);
                    Label messagelabel = CreateLabel(text);
                    messagelabel.Name = obj.Guid;
                    messagelabel.Location = CompareUserIdConfirmLocationInPanel(obj.UserId, messagelabel);
                    AddControlToPanel(messagelabel);
                    continue;
                }

                if (obj.MessageFile != null)
                {
                    string path = this.saveFilePath + obj.MessageFile + ".png";
                    if (File.Exists(path))
                    {
                        PictureBox pictureBox = CreatePicture(path);
                        pictureBox.Name = obj.Guid;
                        pictureBox.Location = CompareUserIdConfirmLocationInPanel(obj.UserId, pictureBox);

                        AddControlToPanel(pictureBox);  
                    }
                }
            }
        }

        /// <summary>
        /// 根据UserId来确定在Panel容器中的位置 相同在右 不同在左
        /// </summary>
        /// <param name="userid"></param>
        /// <param name="comparecontrol">要改位置的control</param>
        /// <returns></returns>
        private Point CompareUserIdConfirmLocationInPanel(string userid,Control comparecontrol)
        {
           return userid == this.UserId ? new Point(this.panel.Width - comparecontrol.Width * 2, comparecontrol.Height) : comparecontrol.Location;
        }

        /// <summary>
        /// 开始连接 监听
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port">端口</param>
        public void StartListening()
        {
            try
            {
                Listener.BeginAccept(new AsyncCallback(AcceptCallBack), Listener);
            }
            catch (Exception e)
            {
                Label exceptionlabel = CreateLabel("some error:" + e);
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
                newcontrol.Location = CountMessagePointOnPanel(newcontrol);
                panel.Controls.Add(newcontrol);
            }

        }
        private delegate void ConreolDelegete(Control control);

        /// <summary>
        /// 得到容器里控件的最新排列坐标
        /// </summary>
        /// <param name="editpointcontrol">要修改坐标的控件</param>
        /// <returns></returns>
        public Point CountMessagePointOnPanel(Control editpointcontrol)
        {
            int count = panel.Controls.Count;
            int blank = 20;
            Control control = new Control();
            if (count != 0)
            {
                control = panel.Controls[count - 1];//得到容器里的最后一个控件(因为所有控件都从上而下排列 所有最后的控件就是Height值最大的控件)
                return new Point(editpointcontrol.Location.X, control.Location.Y + control.Height + blank);
            }

            //第一个控件
            return editpointcontrol.Location;
        }

        /// <summary>
        /// 图片转字节数组
        /// </summary>
        /// <param name="bitmap"></param>
        /// <returns></returns>
        public byte[] ImgToBytes(Bitmap bitmap)
        {
            MemoryStream memory = new MemoryStream();
            bitmap.Save(memory, ImageFormat.Png);
            int len = (int)memory.Position;

            byte[] ret = new byte[memory.Position];
            memory.Seek(0, SeekOrigin.Begin);
            memory.Read(ret, 0, len);

            return ret;
        }

        /// <summary>
        /// 字节数组转图片
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public Image BytesToImage(byte[] bytes)
        {
            return Image.FromStream(new MemoryStream(bytes));
        }

        public void AcceptCallBack(IAsyncResult ar)
        {
            Socket listener = (Socket)ar.AsyncState;
            Socket handler = listener.EndAccept(ar);

            StateObject stateObject = new StateObject();
            stateObject.workSocket = handler;
            handler.BeginReceive(stateObject.buffer,0,StateObject.bufferSize,0,new AsyncCallback(ReadCallBack),stateObject);

            listener.BeginAccept(new AsyncCallback(AcceptCallBack), listener);
        }

        public void ReadCallBack(IAsyncResult ar)
        {
            string content = string.Empty;

            StateObject stateObject = (StateObject)ar.AsyncState;
            Socket handler = stateObject.workSocket;
            Data data = new Data();

            int bytesRead = handler.EndReceive(ar);
            if (bytesRead > 0)
            {
                content = Encoding.UTF8.GetString(stateObject.buffer, 0, bytesRead);
                try
                {
                    MessageModel messageModel = JsonConvert.DeserializeObject<MessageModel>(content);                    
                    if (messageModel.MessageFile != null)
                    {
                        byte[] filebyte= Convert.FromBase64String(messageModel.MessageFile);
                        
                        string fileName = Guid.NewGuid().ToString();
                        try
                        {
                            File.WriteAllBytes(this.saveFilePath + fileName+".png", filebyte);//往指定的路径写入文件
                            messageModel.MessageFile = fileName;//把文件名插入数据库
                        }
                        catch (Exception e)
                        {
                            MessageBox.Show("尝试往指定路径写入文件时出错 路径为:" + this.saveFilePath);
                        }
                        PictureBox pictureBox = CreatePicture(filebyte);

                        AddControlToPanel(pictureBox);
                        
                    }
                    if (messageModel.MessageText != null && messageModel.MessageText != "")
                    {
                        Label receivedata = new Label();
                        receivedata.Text = messageModel.MessageText + "     :" + messageModel.CreateTime;
                        receivedata.AutoSize = true;
                        AddControlToPanel(receivedata);
                    }
                    messageModel.Guid = Guid.NewGuid().ToString();

                    bool result = data.InsertMessage(messageModel);
                    Send(handler,messageModel);//返回信息
                    if (!result)
                    {
                        MessageBox.Show("Insert Message Error");
                    }

                }
                catch (Exception e)
                {
                    MessageBox.Show("receive error" + e.ToString());
                }

            }
        }

        /// <summary>
        /// 把label放在picture的右边
        /// </summary>
        /// <param name="pic"></param>
        /// <param name="label"></param>
        /// <returns></returns>
        private Point labelOnPictureSideLocation(PictureBox pic,Label label)
        {
            Point labelpoint = new Point(pic.Width, pic.Location.Y);
            return labelpoint;
        }

        private Label CreateLabel(string text)
        {
            Label createlabel = new Label();
            createlabel.Text = text;
            createlabel.AutoSize = true;

            return createlabel;
        }

        private PictureBox CreatePicture(byte[] picbyte)
        {
            PictureBox pictureBox = new PictureBox();
            pictureBox.BackgroundImage = BytesToImage(picbyte);
            pictureBox.BackgroundImageLayout = ImageLayout.Zoom;
            pictureBox.Size = new Size { Height = 100, Width = 100 };

            return pictureBox;
        }

        private PictureBox CreatePicture(string picpath)
        {
            PictureBox pictureBox = new PictureBox();
            Bitmap bitmap = new Bitmap(picpath);
            pictureBox.BackgroundImage = bitmap;
            pictureBox.BackgroundImageLayout = ImageLayout.Zoom;
            pictureBox.Size = new Size { Height = 100, Width = 100 };

            return pictureBox;
        }

        public void Send(Socket handler,MessageModel messageModel)
        {
            string data = JsonConvert.SerializeObject(messageModel);
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

                StartListening();
            }
            catch (Exception e)
            {
                Label exception = CreateLabel("some error:" + e);
                AddControlToPanel(exception);
            }
        }
    }

    public class StateObject
    {
        public Socket workSocket { get; set; }

        public const int bufferSize = 100000000;

        public byte[] buffer = new byte[bufferSize];

        public StringBuilder sb = new StringBuilder();
    }
}
