using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using ScannerBackgrdServer.Common;
using static ScannerBackgrdServer.Common.MsgStruct;

namespace ScannerBackgrdServer.ApController
{
    class RecvDeviceStruct
    {
        public AsyncUserToken token;
        public byte[] buff;

        public RecvDeviceStruct()
        {
            this.token = new AsyncUserToken();
        }
    }

    class DeviceManager
    {
        /// <summary>
        /// 是否为测试模式
        /// </summary>
        public const bool DebugMode = false ;

        /// <summary>
        /// 接收到设备消息条数
        /// </summary>
        public static uint recvDeciveMsgNum = 0;
        /// <summary>
        /// 已处理设备消息条数
        /// </summary>
        public static uint handleDeciveMsgNum = 0;

        /// <summary>
        /// 设备名称（LTE/WCDMA/TD-SCDMA/GSM/APP_WINDOWS/APP_WEB/APP_ANDROID）
        /// </summary>
        protected string DeviceType { get; set; }

        /// <summary>
        /// 设备消息处理锁
        /// </summary>
        private  readonly object mutex_Device_Msg = new object();
        /// <summary>
        /// 设备消息存贮队列
        /// </summary>
        private Queue<RecvDeviceStruct> rDeviceMsgQueue = new Queue<RecvDeviceStruct>();

        /// <summary>
        /// 设备连接Socket
        /// </summary>
        private SocketManager m_socket;
        internal SocketManager MySocket { get => m_socket; set => m_socket = value; }

        /// <summary>
        /// 已连接设备列表
        /// </summary>
        private DeviceList deviceList;
        internal DeviceList MyDeviceList { get => deviceList; set => deviceList = value; }
        
        /// <summary>
        /// 构造函数
        /// </summary>
        public DeviceManager()
        {
            this.deviceList = new DeviceList();
            string type = DeviceType;
			
            //启动设备消息处理线程
            Thread t = new Thread(new ThreadStart(ReceiveDeviceMsgThread));
            t.Start();
            t.IsBackground = true;
        }

        /// <summary>
        /// 日志打印。可以在继承类里重载该函数
        /// </summary>
        /// <param name="str"></param>
        public virtual void OnOutputLog(LogInfoType type, string str, 
                                LogCategory category = LogCategory.I,
                                [CallerMemberName] string memberName = "",
                                [CallerFilePath] string filePath = "",
                                [CallerLineNumber] int lineNumber = 0)
        {
            if (type < DataController.LogOutputLevel) return;

            string outStr = string.Format("{0}", str);
            Console.WriteLine(outStr);

            Xml_codec.StaticOutputLog(type, outStr, DeviceType,category, memberName,filePath,lineNumber);

            outStr = null;
        }

        #region 设备消息收/发处理    

        /// <summary>
        /// 报告指定的 System.Byte[] 在此实例中的第一个匹配项的索引。
        /// </summary>
        /// <param name="srcBytes">被执行查找的 System.Byte[]。</param>
        /// <param name="searchBytes">要查找的 System.Byte[]。</param>
        /// <returns>如果找到该字节数组，则为 searchBytes 的索引位置；如果未找到该字节数组，则为 -1。如果 searchBytes 为 null 或者长度为0，则返回值为 -1。</returns>
        protected int ByteIndexOf(byte[] srcBytes, byte[] searchBytes)
        {
            if (srcBytes == null) { return -1; }
            if (searchBytes == null) { return -1; }
            if (srcBytes.Length == 0) { return -1; }
            if (searchBytes.Length == 0) { return -1; }
            if (srcBytes.Length < searchBytes.Length) { return -1; }
            for (int i = 0; i < srcBytes.Length - searchBytes.Length + 1; i++)
            {
                if (srcBytes[i] == searchBytes[0])
                {
                    if (searchBytes.Length == 1) { return i; }
                    bool flag = true;
                    for (int j = 1; j < searchBytes.Length; j++)
                    {
                        if (srcBytes[i + j] != searchBytes[j])
                        {
                            flag = false;
                            break;
                        }
                    }
                    if (flag) { return i; }
                }
            }
            return -1;
        }

        /// <summary>
        /// 收到设备端消息处理函数，需要继承类里重载该函数
        /// </summary>
        /// <param name="token">发送消息的设备信息</param>
        /// <param name="buff">接收到的消息内容</param>
        public virtual void OnReceiveDeviceMsg(AsyncUserToken token, byte[] buff)
        {
            OnOutputLog(LogInfoType.WARN, "收到设备消息，需要重写处理函数！！!");
        }

        /// <summary>
        /// 设备消息处理线程
        /// </summary>
        private void ReceiveDeviceMsgThread()
        {
            bool noMsg = false;
            int hNum = 0;
            int count = 0;

            RecvDeviceStruct recvDeviceStruct = null;
            while (true)
            {
                try
                {
                    if (noMsg)
                    {
                        Thread.Sleep(100);
                    }
                    else
                    {
                        if (hNum >= 100)
                        {
                            hNum = 0;
                            Thread.Sleep(10);
                        }
                    }

                    lock (mutex_Device_Msg)
                    {
                        //if (DeviceType !=null && DeviceType.Equals("LTE"))
                        //    OnOutputLog(LogInfoType.EROR, "当前剩余消息条数：" + rApMsgQueue.Count);

                        if (rDeviceMsgQueue.Count <= 0)
                        {
                            noMsg = true;
                            hNum = 0;
                            continue;
                        }
                        else
                        {
                            recvDeviceStruct = rDeviceMsgQueue.Dequeue();
                            noMsg = false;
                            hNum++;
                        }

                        count = rDeviceMsgQueue.Count;
                    }

                    if (handleDeciveMsgNum == System.UInt32.MaxValue)
                        handleDeciveMsgNum = 0;
                    else
                        handleDeciveMsgNum++;

                    if (!MyDeviceList.AddMsgBuff(recvDeviceStruct.token, recvDeviceStruct.buff))
                    {
                        MyDeviceList.add(recvDeviceStruct.token);
                        if (!MyDeviceList.AddMsgBuff(recvDeviceStruct.token, recvDeviceStruct.buff))
                        {
                            OnOutputLog(LogInfoType.WARN, string.Format("设备列表中未找到该设备[{0}:{1}]信息！",
                                        recvDeviceStruct.token.IPAddress.ToString(), recvDeviceStruct.token.Port.ToString()));
                            OnOutputLog(LogInfoType.INFO, string.Format("共处理设备消息条数:{0}，当前队列消息条数：{1}!",
                                        handleDeciveMsgNum, count));
                            continue;
                        }

                    }

                    //MyDeviceList.DelMsgBuff(recvDeviceStruct.token, 0,int.MaxValue);
                    OnReceiveDeviceMsg(recvDeviceStruct.token, recvDeviceStruct.buff);

                    recvDeviceStruct = null;

                    OnOutputLog(LogInfoType.DEBG, string.Format("共处理设备消息条数:{0}，当前队列消息条数：{1}!",
                        handleDeciveMsgNum, count));
                }
                catch (Exception e)
                {
                    OnOutputLog(LogInfoType.EROR, string.Format("线程[ReceiveDeviceMsgThread]出错。错误码：{0}", e.Message));
                }
            }
        }

        /// <summary>
        /// 接收到设备发过来的消息，将其存到消息队列中
        /// </summary>
        /// <param name="token">设备信息</param>
        /// <param name="buff">消息内容</param>
        private void OnReceiveData(AsyncUserToken token, byte[] buff)
        {
            int count = 0;
            if (-1 == ByteIndexOf(buff, System.Text.Encoding.Default.GetBytes(ApMsgType.status_response)) &&
                (-1 == ByteIndexOf(buff, System.Text.Encoding.Default.GetBytes(AppMsgType.app_heartbeat_request))))
            {
                string str = string.Format("当前在线设备数[{0}]。收到设备[{1}:{2}]消息！",
                    deviceList.GetCount().ToString(), token.IPAddress.ToString(), token.Port.ToString());
                OnOutputLog(LogInfoType.INFO, str);
                str = string.Format("收到{0}设备消息内容为\n{1}", DeviceType,
                    System.Text.Encoding.Default.GetString(buff));
                OnOutputLog(LogInfoType.DEBG, str, LogCategory.R);
                str = null;
            }
            else
            {
                string str = string.Format("当前在线设备数[{0}]。收到设备[{1}:{2}]心跳消息！",
                    deviceList.GetCount().ToString(), token.IPAddress.ToString(), token.Port.ToString());
                OnOutputLog(LogInfoType.INFO, str);
                str = null;
            }
            RecvDeviceStruct recvDeviceStruct = new RecvDeviceStruct();
            recvDeviceStruct.token = token;
            recvDeviceStruct.buff = buff;

            lock (mutex_Device_Msg)
            {
                rDeviceMsgQueue.Enqueue(recvDeviceStruct);

                if (recvDeciveMsgNum == System.UInt32.MaxValue)
                    recvDeciveMsgNum = 0;
                else
                    recvDeciveMsgNum++;

                count = rDeviceMsgQueue.Count;


            }

            OnOutputLog(LogInfoType.DEBG, string.Format("共收到设备消息条数:{0}，当前队列消息条数：{1}!",
                    recvDeciveMsgNum, count));

            recvDeviceStruct = null;
        }

        #endregion

        #region 设备上/下线处理

        /// <summary>
        /// 收到设备上、下线消息处理，需要继承类里重载该函数
        /// </summary>
        /// <param name="token">发送消息的设备信息</param>
        /// <param name="buff">接收到的消息内容</param>
        public virtual void OnDeviceNumberChange(int num, AsyncUserToken token)
        {
            if (num > 0)
                OnOutputLog(LogInfoType.WARN, "收到TCP连接事件，需重写事件处理函数！！!"); 
            else
                OnOutputLog(LogInfoType.WARN, "收到TCP断开事件，需重写事件处理函数！！!");
        }

        /// <summary>
        /// 收到AP上、下线消息处理
        /// </summary>
        /// <param name="num">AP上线或下线。num>0为AP上线，否则为AP下线 </param>
        /// <param name="token">上线或下线AP信息</param>
        public void OnClientNumberChange(int num, AsyncUserToken token)
        {
            string str = string.Empty;
            if (num > 0)  //AP上线，向设备列表中增加AP信息
            {
                str = string.Format("设备[{0}:{1}]上线了！！！", token.IPAddress, token.Port.ToString());
            }
            else  //AP上线，删除设备列表中的AP信息
            {
                str = string.Format("设备[{0}:{1}]下线了！！！", token.IPAddress, token.Port.ToString());
            }

            OnDeviceNumberChange(num, token);
            OnOutputLog(LogInfoType.INFO, string.Format("{0}当前在线设备数：{1}", str, deviceList.GetCount().ToString()));
        }

        #endregion

        /// <summary>
        /// 开始进行设备管理
        /// </summary>
        /// <param name="port">TCP服务器端口号</param>
        public void Start(int port)
        {
            m_socket = new SocketManager(1000, 1024);
            m_socket.OutputLog += OnOutputLog;

            m_socket.ClientNumberChange += OnClientNumberChange;
            m_socket.ReceiveClientData += OnReceiveData;

            m_socket.Init();
            m_socket.Start(new IPEndPoint(IPAddress.Any, port));
        }

        protected void CloseToken(AsyncUserToken token)
        {
            try
            {
                m_socket.CloseClientSocket(token);
            }
            catch (Exception) { }
        }
    }
}
