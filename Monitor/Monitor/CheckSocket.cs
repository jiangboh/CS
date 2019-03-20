using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Monitor
{
    class CheckSocket
    {
        //最大失败次数
        private int MaxFaildNum = 3;
        private String ip = "127.0.0.1";
        private int[] devicePortList; //= { 14783, 14784, 14785, 14786, 14788, 14789 };
        private int[] windowsPortList;
        private string toWindowsMsg = "";
        private string toDeviceMsg = "";
        private static Dictionary<int, int> PortConnect = null;
        private static UInt32 checkNum = 0;
        private static readonly object locker1 = new object();

        public CheckSocket(int[] devicePortList, int[] windowsPortList)
        {
            lock (locker1)
            {
                this.devicePortList = devicePortList;
                this.windowsPortList = windowsPortList;

                if (PortConnect == null)
                {
                    PortConnect = new Dictionary<int, int>();
                    foreach (int port in devicePortList)
                    {
                        PortConnect.Add(port, 0);
                    }
                    foreach (int port in windowsPortList)
                    {
                        PortConnect.Add(port, 0);
                    }
                }
            }
            toDeviceMsg = toDeviceMsg + "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\r\n\t";
            toDeviceMsg = toDeviceMsg + "<message_content>\r\n\t\t";
            toDeviceMsg = toDeviceMsg + "<id>0</id>\r\n\t\t";
            toDeviceMsg = toDeviceMsg + "<device_test_request>\r\n\t\t\t";
            toDeviceMsg = toDeviceMsg + "<timeout>5</timeout>\r\n\t\t\t";
            toDeviceMsg = toDeviceMsg + "<timestamp>" + DateTime.Now.ToLocalTime().ToString() + "</timestamp>\r\n\t\t";
            toDeviceMsg = toDeviceMsg + "</device_test_request>\r\n";
            toDeviceMsg = toDeviceMsg + "</message_content>";

            toWindowsMsg = toWindowsMsg + "{ \"ApInfo\":{ \"SN\":\"\",\"Fullname\":\"\",\"IP\":\"NULL_DEVICE\",\"Port\":0,\"Type\":\"\"},";
            toWindowsMsg = toWindowsMsg + "\"Body\":{ \"type\":\"device_test_request\",\"dic\":{ },";
            toWindowsMsg = toWindowsMsg + "\"n_dic\":null},\"Version\":\"1.0.0\"}";
        }

        public void SetPortList(int[] devicePortList, int[] windowsPortList)
        {
            lock (locker1)
            {
                log("开始设置端口列表。。。");

                this.devicePortList = devicePortList;
                this.windowsPortList = windowsPortList;

                if (PortConnect == null)
                {
                    PortConnect = new Dictionary<int, int>();
                }
                else
                {
                    PortConnect.Clear();
                }

                foreach (int port in devicePortList)
                {
                    PortConnect.Add(port, 0);
                }
                foreach (int port in windowsPortList)
                {
                    PortConnect.Add(port, 0);
                }
            }
        }

        /// <summary>
        /// 检测所有端口
        /// </summary>
        /// <returns>0：可以正常连接；其它值为不能正常连接的端口号</returns>
        public int start()
        {
            int ret = 0;

            checkNum++;
            ret = CheckTcpConnectStatus();
            if (0 == ret)
            {
                log(string.Format("\n第【{0}】次连接服务端成功!!!!\r\n\n\n", checkNum));
            }
            else
            {
                log(string.Format("\n第【{0}】次连接服务端({1})失败!!!!\r\n\n\n", checkNum,ret));
                PortConnect.Clear();
                PortConnect = null;
            }

            return ret;
        }

        /// <summary>
        /// 检测TCP是否可能连接
        /// </summary>
        /// <returns>0：可以正常连接；其它值为不能正常连接的端口号</returns>
        private int CheckTcpConnectStatus()
        {
            int ret = 0;

            lock (locker1)
            {
                foreach (int port in devicePortList)
                {
                    try
                    {
                        if (checkPort(port, toDeviceMsg))
                        {
                            PortConnect[port] = 0;
                            //log(string.Format("测试服务端[{0}:{1}]成功。", ip, port));
                        }
                        else
                        {
                            PortConnect[port]++;
                            if (PortConnect[port] >= MaxFaildNum)
                            {
                                ret = port;
                            }
                            //log(string.Format("测试服务端[{0}:{1}]失败({2}/{3})。",ip, port, PortConnect[port], MaxFaildNum));
                        }
                    }
                    catch (Exception e)
                    {
                        log(string.Format("测试服务端[{0}：{1}]失败。失败原因={1}", ip, port, e.Message));
                    }
                }

                foreach (int port in windowsPortList)
                {
                    try
                    {
                        if (checkPort(port, toWindowsMsg))
                        {
                            PortConnect[port] = 0;
                            log(string.Format("测试服务端[{0}:{1}]成功。", ip, port));
                        }
                        else
                        {
                            PortConnect[port]++;
                            if (PortConnect[port] >= MaxFaildNum)
                            {
                                ret = port;
                            }
                            log(string.Format("测试服务端[{0}:{1}]失败({2}/{3})。",ip, port, PortConnect[port], MaxFaildNum));
                        }
                    }
                    catch (Exception e)
                    {
                        log(string.Format("测试服务端[{0}：{1}]失败。失败原因={1}", ip, port, e.Message));
                    }
                }
            }

            return ret;
        }

        /// <summary>
        /// 检测端口状态
        /// </summary>
        /// <param name="port">端口号</param>
        /// <param name="sendMessage">测试消息</param>
        /// <returns>端口是否正常</returns>
        private bool checkPort(int port,string sendMessage)
        {
            bool ret = true;

            byte[] result = new byte[1024];


            IPAddress address = IPAddress.Parse(ip);

            IPEndPoint sendpoint = new IPEndPoint(address, port);
            Socket socketClient = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socketClient.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, 5000);
            //IPEndPoint localEP = new IPEndPoint(IPAddress.Any, 24789);
            //socketClient.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            //socketClient.Bind(localEP);
            try
            {
                socketClient.Connect(sendpoint);
                //log(string.Format("连接服务端[{0}:{1}]成功", ip, port));
            }
            catch (Exception e)
            {
                //log(string.Format("连接服务端[{0}:{1}]失败。",ip, port, e.Message.ToString()));
                ret = false;
            }

            if (ret)
            {
                //通过 clientSocket 发送数据  
                try
                {
                    socketClient.Send(Encoding.UTF8.GetBytes(sendMessage));
                    //log(string.Format("向服务器[{0}:{1}]发送消息：\n", ip, port, sendMessage));

                    //通过clientSocket接收数据  
                    int receiveLength = socketClient.Receive(result);
                    //log(string.Format("接收服务器[{0}:{1}]消息：\n{2}", ip, port, Encoding.ASCII.GetString(result, 0, receiveLength)));
                }
                catch
                {
                    ret = false;
                    log(string.Format("未收到服务器[{0}:{1}]消息。", ip, port));
                }
            }

            if (socketClient.Connected)
                socketClient.Shutdown(SocketShutdown.Both);

            socketClient.Close();

            return ret;
        }

        private void log(string str)
        {
            //Console.WriteLine(str);
            Logger.Trace(LogInfoType.INFO, str, "CheckSocket", LogCategory.I);
        }
    }
}
