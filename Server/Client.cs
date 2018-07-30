using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
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
        private Socket socketClient;
        private readonly string ip;
        private readonly int port;
        private List<string> fileAddress { get; set; }

        /// <summary>
        /// 连接成功回调
        /// </summary>
        //private delegate void ConnectBackCall(byte[] byteData);

        public Client(TextBox textbox, Panel panel, string userid, string ip, int port)
        {
            this.textbox = textbox;
            this.panel = panel;
            this.UserId = userid;
            this.ip = ip;
            this.port = port;
        }

        public void StartClient()
        {
            try
            {
                IPHostEntry iPHost = Dns.Resolve(this.ip);
                IPAddress iPAddress = iPHost.AddressList[0];
                IPEndPoint iPEnd = new IPEndPoint(iPAddress, this.port);

                Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                this.socketClient = client;
                client.BeginConnect(iPEnd, new AsyncCallback(ConnectCallBack), client);
            }
            catch (Exception e)
            {
                Label error = new Label();
                error.Text = "some error1:" + e;
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
                error.Text = "some error:2" + e;
                error.AutoSize = true;
                AddControlToPanel(error);
            }
        }

        public void Send(string text)
        {
            StartClient();
            DateTime time = DateTime.Now;
            string messageModel = JsonConvert.SerializeObject(new MessageModel { MessageText = text, UserId = this.UserId, CreateTime = time });
            byte[] byteData = Encoding.UTF8.GetBytes(messageModel);
            this.socketClient.BeginSend(byteData, 0, byteData.Length, 0, new AsyncCallback(SendBackCall), this.socketClient);
        }

        /// <summary>
        /// 根据UserId来确定在Panel容器中的位置 相同在右 不同在左
        /// </summary>
        /// <param name="userid"></param>
        /// <param name="comparecontrol">要改位置的control</param>
        /// <returns></returns>
        private Point CompareUserIdConfirmLocationInPanel(string userid, Control comparecontrol)
        {
            return userid == this.UserId ? new Point(this.panel.Width - comparecontrol.Width * 2, comparecontrol.Height) : comparecontrol.Location;
        }

        private Label CreateLabel(string text)
        {
            Label createlabel = new Label();
            createlabel.Text = text;
            createlabel.AutoSize = true;

            return createlabel;
        }

        /// <summary>
        /// 发送文件
        /// </summary>
        /// <param name="files"></param>
        public void SendFile(List<string> fileaddress, string message)
        {
            this.fileAddress = fileaddress;
            foreach (var address in fileaddress)
            {
                StartClient();
                try
                {
                    byte[] byteimg = ImgToBytes(address);

                    string jsondata = JsonConvert.SerializeObject(new MessageModel { UserId = this.UserId, MessageText = message, CreateTime = DateTime.Now, MessageFile = Convert.ToBase64String(byteimg) });
                    byte[] byteData = Encoding.UTF8.GetBytes(jsondata);
                    while (!this.socketClient.Connected) { }//等到连接成功
                    SendFileBackCall(byteData);
                }
                catch (Exception e)
                {
                    MessageBox.Show("file error" + e);
                }

            }
        }

        public void SendFileBackCall(byte[] byteData)
        {
            this.socketClient.BeginSend(byteData, 0, byteData.Length, 0, new AsyncCallback(SendBackCall), this.socketClient);
        }

        public byte[] ImgToBytes(string address)
        {
            Image image = Image.FromFile(address);
            ImageFormat format = image.RawFormat;
            using (MemoryStream ms = new MemoryStream())
            {
                if (format.Equals(ImageFormat.Jpeg))
                {
                    image.Save(ms, ImageFormat.Jpeg);
                }
                else if (format.Equals(ImageFormat.Png))
                {
                    image.Save(ms, ImageFormat.Png);
                }
                else if (format.Equals(ImageFormat.Gif))
                {
                    image.Save(ms, ImageFormat.Gif);
                }
                byte[] buffer = new byte[ms.Length];
                //Image.Save()会改变MemoryStream的Position，需要重新Seek到Begin
                ms.Seek(0, SeekOrigin.Begin);
                ms.Read(buffer, 0, buffer.Length);
                return buffer;
            }
        }

        public Image BytesToImage(byte[] bytes)
        {
            return Image.FromStream(new MemoryStream(bytes));
        }

        public void SendBackCall(IAsyncResult ar)
        {
            try
            {
                Socket client = (Socket)ar.AsyncState;
                int bytesSent = client.EndSend(ar);

                Receive(client);//开始接收服务器返回的数据
                this.textbox.Text = string.Empty;//清除textbox框里的值
            }
            catch (Exception e)
            {
                Label error = new Label();
                error.Text = "some error:3" + e;
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
                error.Text = "some error:4" + e;
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
                MessageModel messageModel = new MessageModel();

                int bytesRead = client.EndReceive(ar);
                if (bytesRead > 0)
                {
                    messageModel = JsonConvert.DeserializeObject<MessageModel>(Encoding.UTF8.GetString(state.buffer, 0, bytesRead));

                    string threadlock = "";
                    if (this.fileAddress != null && messageModel != null)//发送过文件
                    {

                        lock (threadlock)
                        {
                            foreach (var address in this.fileAddress)
                            {
                                PictureBox pictureBox = CreatePicture(address);
                                pictureBox.Location = CompareUserIdConfirmLocationInPanel(this.UserId, pictureBox);
                                pictureBox.Name = messageModel.Guid;
                                AddControlToPanel(pictureBox);
                            }
                            this.fileAddress.Clear();
                        }

                    }
                    if (messageModel.MessageText != null && messageModel.MessageText != "" && messageModel != null)
                    {
                        lock (threadlock)
                        {
                            Label sendmessage = CreateLabel(messageModel.MessageText + "     :" + messageModel.CreateTime);
                            //sendmessage.Location = new Point(panel.Width - sendmessage.Width * 2, sendmessage.Height);
                            sendmessage.Location = CompareUserIdConfirmLocationInPanel(this.UserId, sendmessage);
                            AddControlToPanel(sendmessage);
                        }
                    }

                    client.Shutdown(SocketShutdown.Both);
                    client.Close();
                }

            }
            catch (Exception e)
            {
                Label error = new Label();
                error.Text = "some error:5" + e;
                error.AutoSize = true;
                AddControlToPanel(error);
            }
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
    }
}
