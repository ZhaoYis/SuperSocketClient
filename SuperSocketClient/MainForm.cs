using SuperSocket.ClientEngine;
using SuperSocketClient.AppBase;
using SuperSocketClient.Common;
using System;
using System.Linq;
using System.Net;
using System.Threading;
using System.Timers;
using System.Windows.Forms;

namespace SuperSocketClient
{
    public partial class MainForm : Form
    {
        private EasyClient<CustomPackageInfo> client;
        private System.Timers.Timer timer1;

        #region 解决跨线程调用UI组件问题

        private void DelegateAction(Action action)
        {
            if (InvokeRequired)
            {
                Invoke(action);
            }
            else
            {
                action();
            }
        }

        #endregion 解决跨线程调用UI组件问题

        public MainForm()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            var timer = new System.Timers.Timer(1000);
            timer.Elapsed += (s, x) =>
            {
                DelegateAction(() =>
                {
                    txtAll.Text = LogHelper.SetOnLog();
                    txtAll.Select(txtAll.TextLength, 0);
                    txtAll.ScrollToCaret();
                });
            };
            timer.Enabled = true;
            timer.Start();


            timer1 = new System.Timers.Timer(5000);
            timer1.Elapsed += (s, x) =>
            {
                //SendMessage(CustomCommand.Heartbeat, "&");
            };
            timer1.Enabled = true;
            timer1.Start();
        }

        private void btnConn_Click(object sender, EventArgs e)
        {
            if (client == null || !client.IsConnected)
                ConnectServer();
        }

        /// <summary>
        /// 连接服务器
        /// </summary>
        private async void ConnectServer()
        {
            client = new EasyClient<CustomPackageInfo>();
            client.Initialize(new CustomReceiveFilter());
            client.Connected += OnClientConnected;
            client.NewPackageReceived += OnPagckageReceived;
            client.Error += OnClientError;
            client.Closed += OnClientClosed;

            var webSocketUrl = System.Configuration.ConfigurationManager.AppSettings["WebSocketURL"];
            var webSocketPort = System.Configuration.ConfigurationManager.AppSettings["WebSocketPort"];
            var connected = await client.ConnectAsync(new IPEndPoint(IPAddress.Parse(webSocketUrl), int.Parse(webSocketPort)));
        }

        private void OnClientClosed(object sender, EventArgs e)
        {
            int attmpts = 5;
            //if (timer1 != null)
            //{
            //    timer1.Dispose();
            //}
            do
            {
                LogHelper.WriteLog("连接已断开...");
                LogHelper.WriteLog("等待5秒中后重新连接");
                Thread.Sleep(5000);
                ConnectServer();
                attmpts--;
            } while (!client.IsConnected && attmpts > 0);
        }

        private void OnClientError(object sender, ErrorEventArgs e)
        {
            LogHelper.WriteLog("客户端错误：" + e.Exception.Message);

        }

        private void OnPagckageReceived(object sender, PackageEventArgs<CustomPackageInfo> e)
        {
            LogHelper.WriteLog("收到消息：" + e.Package.Body);
        }

        private void OnClientConnected(object sender, EventArgs e)
        {
            LogHelper.WriteLog("已连接到服务器...");
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            LogHelper.AllLines.Clear();
            LogHelper.AllLines.Add("清空了..");
            LogHelper.DisplayLength = 0;
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            SendMessage(CustomCommand.Test, txtMsg.Text);
        }

        /// <summary>
        /// 发送命令和消息到服务器
        /// </summary>
        /// <param name="command"></param>
        /// <param name="message"></param>
        private void SendMessage(CustomCommand command, string message)
        {
            if (client == null || !client.IsConnected || message.Length <= 0) return;
            var response = BitConverter.GetBytes((ushort)command).Reverse().ToList();
            var arr = System.Text.Encoding.UTF8.GetBytes(message);
            response.AddRange(BitConverter.GetBytes((ushort)arr.Length).Reverse().ToArray());
            response.AddRange(arr);
            client.Send(response.ToArray());

            LogHelper.WriteLog($"发送{command.GetDescription()}数据：" + message);

            DelegateAction(() =>
            {
                txtMsg.Text = string.Empty;
            });
        }
    }
}