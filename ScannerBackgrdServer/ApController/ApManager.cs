using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using ScannerBackgrdServer.Common;
using static ScannerBackgrdServer.Common.MsgStruct;
using static ScannerBackgrdServer.Common.Xml_codec;

namespace ScannerBackgrdServer.ApController
{
    #region MainController消息处理类

    class ApManager
    {
        /// <summary>
        /// MainController消息处理锁
        /// </summary>
        public static readonly object mutex_Main2Ap_Msg = new object();
        /// <summary>
        /// MainController消息接收队列
        /// </summary>
        public static Queue<string> rMain2ApMsgQueue = new Queue<string>();

        /// <summary>
        /// 已接收MainController到ApController消息条数
        /// </summary>
        public static uint recvMain2ApContrMsgNum = 0;
        /// <summary>
        /// 已处理MainController到ApController消息条数
        /// </summary>
        public static uint handleMain2ApContrMsgNum = 0;

        //声明用于发送信息给MainController的代理
        public static MessageDelegate sendMsg_2_MainController = new MessageDelegate(FrmMainController.MessageDelegate_For_ApController);

        /// <summary>
        /// 接收MainController发来的消息，存贮到消息队列中
        /// </summary>
        /// <param name="mt"></param>
        /// <param name="mb"></param>
        public static void MessageDelegate_For_MainController(MessageType mt, MessageBody mb)
        {
            int count = 0;

            Xml_codec.StaticOutputLog(LogInfoType.INFO, "ApContr收到Main侧消息。", "APContr", LogCategory.R);
            Xml_codec.StaticOutputLog(LogInfoType.DEBG, string.Format("消息内容:\n{0}", mb.bJson), "APContr", LogCategory.R);

            lock (mutex_Main2Ap_Msg)
            {
                rMain2ApMsgQueue.Enqueue(mb.bJson);

                if (recvMain2ApContrMsgNum == System.UInt32.MaxValue)
                    recvMain2ApContrMsgNum = 0;
                else
                    recvMain2ApContrMsgNum++;

                count = rMain2ApMsgQueue.Count;
            }

            string outStr = string.Format("ApContr共收到设备消息条数:{0},当前队列消息条数：{0}！", recvMain2ApContrMsgNum,count);
            Xml_codec.StaticOutputLog(LogInfoType.DEBG, outStr, "APContr", LogCategory.R);
        }
    }

    #endregion

    class ApBase : DeviceManager
    {
        protected string OffLine = "OffLine";
        protected string OnLine = "OnLine";

        /// <summary>
        /// 发送到Main模块的Imsi条数
        /// </summary>
        public static UInt64 sendMainImsiMsgNum = 0;
        /// <summary>
        /// 接收到Ap发过来的IMSI条数
        /// </summary>
        public static UInt64 recvApImsiMsgNum = 0;
        /// <summary>
        /// 心跳不正常，未上报的imsi数量
        /// </summary>
        public static UInt64 noSendMainImsiNum = 0;

        protected enum ApReadyStEnum : Byte
        {
            XML_Not_Ready = 0,                    
            XML_Ready,
            Sniffering,
            Ready_For_Cell,
            Cell_Setuping,
            Cell_Ready,
            Cell_Failure
        }

        protected class AP_STATUS_LTE
        {
            public static UInt32 SCTP = 0x80000000;
            public static UInt32 S1 = 0x40000000;
            public static UInt32 GPS = 0x20000000;
            public static UInt32 CELL = 0x10000000;
            public static UInt32 SYNC = 0x8000000;
            public static UInt32 LICENSE = 0x4000000;
            public static UInt32 RADIO = 0x2000000;
            public static UInt32 OnLine = 0x1000000;
            public static UInt32 wSelfStudy = 0x800000;

            public static UInt32 RADIO2 = 0x1;
        }

        /// <summary>
        /// 接收到Main模块的数据委托
        /// </summary>
        /// <param name="MainMsg">消息内容</param>
        public delegate void OnReceiveMainData(InterModuleMsgStruct MainMsg);
        /// <summary>  
        /// 接收到Main模块的数据事件  
        /// </summary>  
        public static event OnReceiveMainData ReceiveMainData;

        public ApBase()
        {
            //启动AP状态检测线程
            Thread t = new Thread(new ThreadStart(CheckApStatusThread));
            t.Start();
            t.IsBackground = true;

            //启动处理MainController模块消息线程
            Thread t2 = new Thread(new ParameterizedThreadStart(ReceiveMainMsgThread));
            t2.Start(string.Empty);
            t2.IsBackground = true;
        }

        /// <summary>
        /// 检测Ap在线状态
        /// </summary>
        private void CheckApStatusThread()
        {
            long upTime = 0;
            HashSet<AsyncUserToken> toKenList = new HashSet<AsyncUserToken>();
            HashSet<AsyncUserToken> RemovList = new HashSet<AsyncUserToken>();

            while (true)
            {
                try
                {
                    if (DeviceType != null && ((DateTime.Now.Ticks - upTime) / 10000000) > 60)
                    {
                        upTime = DateTime.Now.Ticks;
                        //报到线程状态
                        FrmMainController.write_monitor_status(DeviceType + "_STATUS");
                    }

                    MyDeviceList.CopyConnList(ref toKenList);
                    foreach (AsyncUserToken x in toKenList)
                    {
                        TimeSpan timeSpan = DateTime.Now - x.EndMsgTime;
                        if (timeSpan.TotalSeconds > DataController.ApOnLineTime) //大于180秒认为设备下线
                        {
                            RemovList.Add(x);
                        }
                    }
                    //删除已下线的AP
                    foreach (AsyncUserToken x in RemovList)
                    {
                        string MainControllerStatus = MyDeviceList.GetMainControllerStatus(x);
                        if (string.IsNullOrEmpty(MainControllerStatus)) MainControllerStatus = "unknown";

                        int i = MyDeviceList.remov(x);
                        if (i != -1)
                        {
                            OnOutputLog(LogInfoType.INFO, string.Format("Ap[{0}:{1}]下线了！！！", x.IPAddress, x.Port.ToString()));
                            Send2main_OnOffLine(OffLine, i, x, MainControllerStatus);
                        }
                    }
                    //OnOutputLog(LogInfoType.DEBG, "当前在线Ap数量为：" + MyDeviceList.GetCount().ToString() + "台 ！");

                    toKenList.Clear();
                    toKenList.TrimExcess();
                    RemovList.Clear();
                    RemovList.TrimExcess();

                    Thread.Sleep(3000);
                }
                catch (Exception e)
                {
                    OnOutputLog(LogInfoType.EROR, string.Format("线程[CheckApStatusThread]出错。错误码：{0}", e.Message));
                }
            }
        }

        /// <summary>
        /// 重写设备连接更改事件
        /// </summary>
        /// <param name="num">num大于0表示设备上线，否则设备下线</param>
        /// <param name="apToKen">设备信息</param>
        public override void OnDeviceNumberChange(int num, AsyncUserToken apToKen)
        {
            if (num > 0)
            {
                MyDeviceList.add(apToKen);
                //在收到心跳消息时上报
                //send2main_OnOffLine(OnLine,token);
            }
            else  //AP下线，删除设备列表中的AP信息
            {
                string MainControllerStatus = MyDeviceList.GetMainControllerStatus(apToKen);
                if (string.IsNullOrEmpty(MainControllerStatus)) MainControllerStatus = "unknown";

                int i = MyDeviceList.remov(apToKen);
                if (i != -1)
                {
                    Send2main_OnOffLine(OffLine, i, apToKen, MainControllerStatus);
                }
            }
        }

        /// <summary>
        /// MainController模块消息处理线程
        /// </summary>
        /// <param name="o"></param>
        public void ReceiveMainMsgThread(object o)
        {
            bool noMsg = false;
            int hNum = 0;
            int count = 0;
            string str = string.Empty;
            long upTime = 0;
            while (true)
            {
                try
                {
                    if (DeviceType != null && ((DateTime.Now.Ticks - upTime) / 10000000) > 60)
                    {
                        upTime = DateTime.Now.Ticks;
                        //报到线程状态
                        FrmMainController.write_monitor_status("MAIN_2_" + DeviceType + "_HANDLE");
                    }

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

                    lock (ApManager.mutex_Main2Ap_Msg)
                    {
                        if (ApManager.rMain2ApMsgQueue.Count <= 0)
                        {
                            noMsg = true;
                            hNum = 0;
                            continue;
                        }
                        else
                        {
                            noMsg = false;
                            hNum++;
                            str = ApManager.rMain2ApMsgQueue.Dequeue();
                        }
                        count = ApManager.rMain2ApMsgQueue.Count;
                    }

                    if (ApManager.handleMain2ApContrMsgNum == System.UInt32.MaxValue)
                        ApManager.handleMain2ApContrMsgNum = 0;
                    else
                        ApManager.handleMain2ApContrMsgNum++;

                    //解析收到的消息
                    InterModuleMsgStruct MainMsg = null;
                    try
                    {
                        MainMsg = JsonConvert.DeserializeObject<InterModuleMsgStruct>(str);
                        MainMsg.ApInfo.Type = MainMsg.ApInfo.Type.Replace("-","_");
                    }
                    catch (Exception)
                    {
                        OnOutputLog(LogInfoType.EROR, "解析收到的Main模块消息出错！");
                        OnOutputLog(LogInfoType.INFO, string.Format("共处理Main2Ap消息条数:{0}，当前队列消息条数：{1}!",
                            ApManager.handleMain2ApContrMsgNum, count));
                        continue;
                    }

                    if ((MainMsg.ApInfo.IP.Equals(MsgStruct.AllDevice)) || (MainMsg.ApInfo.Type.Equals(DeviceType)))
                    {
                        OnOutputLog(LogInfoType.INFO, 
                            string.Format("接收到MainController消息({0})。",MainMsg.Body.type));
                        OnOutputLog(LogInfoType.DEBG, string.Format("消息内容:\n{0}", str));
                    }

                    ApInnerType flag;
                    if ((!MainMsg.ApInfo.IP.Equals(MsgStruct.AllDevice))
                        && (!(Enum.TryParse<ApInnerType>(MainMsg.ApInfo.Type, true, out flag))))
                    {
                        if (string.IsNullOrEmpty(MainMsg.ApInfo.Type)) MainMsg.ApInfo.Type = "空";
                        OnOutputLog(LogInfoType.EROR, "收到MainController模块消息中AP类型错误!收到类型为:" + MainMsg.ApInfo.Type);
                        continue;
                    }

                    if (ReceiveMainData != null && MainMsg != null)
                        ReceiveMainData(MainMsg);

                    MainMsg = null;
                    str = null;

                    OnOutputLog(LogInfoType.INFO, string.Format("共处理Main2Ap消息条数:{0}，当前队列消息条数：{1}!",
                        ApManager.handleMain2ApContrMsgNum, count));
                }
                catch (Exception e)
                {
                    OnOutputLog(LogInfoType.EROR, string.Format("线程[ReceiveMainMsgThread]出错。错误码：{0}", e.Message));
                }
            }
        }

        #region 防粘包处理

        private const string xmlStartFlag = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>";
        private const string xmlEndFlag = "</message_content>";

        /// <summary>
        /// 防止粘包，检查XML消息的起始标志，并从消息中返回一条完整的XML消息
        /// </summary>
        /// <param name="apToKen">设备信息</param>
        /// <returns></returns>
        protected string GetDeviceMsg_XML(AsyncUserToken apToKen)
        {
            byte[] allMsgByte = MyDeviceList.GetAllMsgBuff(apToKen);

            int endIndex = ByteIndexOf(allMsgByte,System.Text.Encoding.Default.GetBytes(xmlEndFlag));
            if (endIndex >= 0)
            {
                endIndex = endIndex + System.Text.Encoding.Default.GetBytes(xmlEndFlag).Length;
                if (endIndex > allMsgByte.Length)
                {
                    allMsgByte = null;
                    OnOutputLog(LogInfoType.WARN, "未找到Xml消息结束标志！");
                    return string.Empty;
                }

                int startIndex = ByteIndexOf(allMsgByte, System.Text.Encoding.Default.GetBytes(xmlStartFlag));
                if (startIndex < 0)
                {
                    allMsgByte = null;
                    OnOutputLog(LogInfoType.WARN, "未找到Xml消息起始标志！");
                    MyDeviceList.DelMsgBuff(apToKen, 0, endIndex);
                    return string.Empty;
                }
                if (startIndex >= endIndex)
                {
                    allMsgByte = null;
                    OnOutputLog(LogInfoType.WARN, "Xml消息起始标志错误！");
                    MyDeviceList.DelMsgBuff(apToKen, 0, startIndex);
                    return string.Empty;
                }

                string msgStr = MyDeviceList.GetMsgBuff(apToKen,startIndex, endIndex-startIndex);
                MyDeviceList.DelMsgBuff(apToKen, 0, endIndex);

                //OnOutputLog(LogInfoType.DEBG, "收到AP消息：————————————————————");
                //OnOutputLog(LogInfoType.DEBG, msgStr);
                //OnOutputLog(LogInfoType.DEBG, "————————————————————\n\n");
                return msgStr;
            }

            allMsgByte = null;

            return string.Empty;
        }

        #endregion

        #region 向AP发送消息
        /// <summary>
        /// 发送消息到AP
        /// </summary>
        /// <param name="apToKen">Ap信息</param>
        /// <param name="buff">消息内容</param>
        protected void SendMsg2Ap(AsyncUserToken apToKen,string type, byte[] buff)
        {
            OnOutputLog(LogInfoType.INFO, string.Format("发送消息({0})给AP[{1}:{2}]！",
                type,apToKen.IPAddress.ToString(),apToKen.Port));
            OnOutputLog(LogInfoType.DEBG, string.Format("消息内容为:\n{0}", System.Text.Encoding.UTF8.GetString(buff)));
            MySocket.SendMessage(apToKen, buff);
        }

        protected void SendMsg2Ap(AsyncUserToken apToKen,string type, string buff)
        {
            OnOutputLog(LogInfoType.INFO, string.Format("发送消息({0})给AP[{1}:{2}]！",
                type,apToKen.IPAddress.ToString(), apToKen.Port));
            OnOutputLog(LogInfoType.DEBG, string.Format("消息内容为:\n{0}", buff));
            MySocket.SendMessage(apToKen, System.Text.Encoding.Default.GetBytes(buff));
        }

        /// <summary>
        /// 向AP发送消息
        /// </summary>
        /// <param name="apToKen">Ap信息</param>
        /// <param name="id">消息id</param>
        /// <param name="body">消息内容</param>
        /// <returns>消息封装是否成功</returns>
        protected bool SendMsg2Ap(AsyncUserToken apToKen,ushort id, Msg_Body_Struct body)
        {
            byte[] sendMsg = EncodeApXmlMessage(id, body);
            if (sendMsg == null)
            {
                OnOutputLog(LogInfoType.EROR, string.Format("封装XML消息({0})出错！",body.type));
                return false;
            }
            SendMsg2Ap(apToKen, body.type,sendMsg);
            return true;
        }

        /// <summary>
        /// 向AP回应心跳消息
        /// </summary>
        /// <param name="apToKen">AP信息</param>
        protected void Send2ap_status_request(AsyncUserToken apToKen)
        {
            //向AP回复心跳
            Msg_Body_Struct TypeKeyValue =
                new Msg_Body_Struct(ApMsgType.status_request,
                "timeout", 5,
                "timestamp", DateTime.Now.ToLocalTime().ToString());

            SendMsg2Ap(apToKen, 0, TypeKeyValue);
        }

        #endregion

        #region 向MainController模块发送消息

        /// <summary>
        /// 发送消息给MainController模块
        /// </summary>
        /// <param name="msgId">Ap返回的消息Id</param>
        /// <param name="msgType">Main模块消息类型</param>
        /// <param name="apToKen">Ap信息</param>
        /// <param name="TypeKeyValue">消息内容</param>
        public void OnSendMsg2Main(UInt16 msgId, MsgStruct.MsgType msgType, AsyncUserToken apToKen, Msg_Body_Struct TypeKeyValue)
        {
            MessageType mt = MessageType.MSG_JSON;
            MessageBody mb = new MessageBody();

            InterModuleMsgStruct msg = new InterModuleMsgStruct();
            msg.Version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            msg.MsgType = msgType.ToString();

            msg.ApInfo.SN = apToKen.Sn;
            msg.ApInfo.Fullname = apToKen.FullName;
            msg.ApInfo.IP = apToKen.IPAddress.ToString();
            msg.ApInfo.Port = apToKen.Port;
            msg.ApInfo.Type = DeviceType;

            App_Info_Struct appInfo = new App_Info_Struct();
            if ((msgType == MsgType.NOTICE) || (msgId == 0))
            {
                appInfo.Ip = AllDevice;
            }
            else
            {
                //通过消息id查找AppInfo
                AsyncUserToken ListToken = MyDeviceList.FindByIpPort(apToKen.IPAddress.ToString(), apToKen.Port);
                if (ListToken == null)
                {
                    String str = string.Format(
                        "未找到设备[{0}:{1}]消息ID({2})对应的APP信息，不发送消息给MainController模块！",
                        apToKen.IPAddress.ToString(), apToKen.Port, msgId);
                    OnOutputLog(LogInfoType.EROR, str);

                    return;
                }
                HashSet<MsgId2App> msgId2App = new HashSet<MsgId2App>();
                msgId2App = ListToken.msgId2App;
                foreach (MsgId2App x in msgId2App)
                {
                    if (x.id == msgId)
                    {
                        appInfo = x.AppInfo;
                        break;
                    }
                }

                //删除保存的消息Id列表(目前实现的方法是120秒后自动删除)
                //MyDeviceList.RemoveMsgId2App(apToKen, msgId);
            }

            if (string.IsNullOrEmpty(appInfo.Ip))
            {
                String str = string.Format(
                    "未找到设备[{0}:{1}]消息ID({2})对应的APP信息，不发送消息给MainController模块！",
                    apToKen.IPAddress.ToString(), apToKen.Port, msgId);
                OnOutputLog(LogInfoType.EROR, str);

                return;
            }

            msg.AppInfo = appInfo;
            msg.Body = TypeKeyValue;
            mb.bJson = JsonConvert.SerializeObject(msg);

            //当设备状态为在线时再发送消息给Main模块
            if ((String.Compare(MyDeviceList.GetMainControllerStatus(apToKen), OnLine, true) != 0) &&
                (!TypeKeyValue.type.Equals(Main2ApControllerMsgType.OnOffLine)))
            {
                OnOutputLog(LogInfoType.WARN,
                    string.Format("设备[{0}:{1}]在线状态为：{2}，OnLine状态才向Main模块发送消息{3}！",
                    apToKen.IPAddress.ToString(), apToKen.Port.ToString(), 
                    MyDeviceList.GetMainControllerStatus(apToKen), TypeKeyValue.type));

                if (TypeKeyValue.type == ApMsgType.scanner)
                {
                    noSendMainImsiNum++;
                }
                return;
            }

            if (TypeKeyValue.type == ApMsgType.scanner)
            {
                 sendMainImsiMsgNum++;
            }

            OnOutputLog(LogInfoType.INFO, string.Format("发送消息({0})给MainController模块！", TypeKeyValue.type), LogCategory.S);
            OnOutputLog(LogInfoType.DEBG, string.Format("消息内容:\n{0}", mb.bJson),LogCategory.S);

            ApManager.sendMsg_2_MainController(mt, mb);
        }

        protected void Send2main_OnOffLine(string status, int allNum, AsyncUserToken apToKen)
        {
            //保存的状态有更改时才上报
            string MainControllerStatus = MyDeviceList.GetMainControllerStatus(apToKen);
            if (string.IsNullOrEmpty(MainControllerStatus)) MainControllerStatus = "unknown";

            this.Send2main_OnOffLine(status,allNum,apToKen,MainControllerStatus);
        }
        /// <summary>
        /// 发送AP上下线消息
        /// </summary>
        /// <param name="status">状态（OnLine：上线；OffLine:下线）</param>
        /// <param name="allNum">当前在线的AP总数</param>
        /// <param name="apToKen">Ap信息</param>
        /// <param name="MainControllerStatus">发送给数据库上、下线状态</param>
        protected void Send2main_OnOffLine(string status, int allNum,AsyncUserToken apToKen,string MainControllerStatus)
        {
            //保存的状态有更改时才上报
            //string MainControllerStatus = MyDeviceList.GetMainControllerStatus(token);
            //if (string.IsNullOrEmpty(MainControllerStatus)) MainControllerStatus = "unknown";

            OnOutputLog(LogInfoType.DEBG, "旧状态为：" + MainControllerStatus + "新状态为：" + status);

            if ((String.Compare(status, OffLine, true) == 0) && (MainControllerStatus.Equals("unknown")))
            {
                OnOutputLog(LogInfoType.WARN, "未向MainController模块上报过上线消息，该下线消息不上报！");
                return;
            }

            if ((String.Compare(status, OffLine, true) == 0) && (null != MyDeviceList.FindByFullname(apToKen.FullName)))
            {
                OnOutputLog(LogInfoType.WARN, "该设备有另外一个TCP连接在线，不向MainController模块上报下线消息！");
                return;
            }

            if (String.Compare(MainControllerStatus, status, true) != 0)
            {
                Msg_Body_Struct TypeKeyValue =
                    new Msg_Body_Struct(Main2ApControllerMsgType.OnOffLine,
                    "AllOnLineNum", allNum.ToString(),
                    "Status", status,
                    "mode", apToKen.Mode.Replace("-","_"),
                    "version", apToKen.version,
                    "timestamp", DateTime.Now.ToLocalTime().ToString());

                //向Main模块发消息
                OnSendMsg2Main(0,MsgStruct.MsgType.NOTICE,apToKen, TypeKeyValue);

                //修改状态----测试版本中使用，正式版本中在收到Ack后更改状态。
                //if (DebugMode)
                //{
                //    MyDeviceList.SetMainControllerStatus(status, token);
                //}
            }
        }

        /// <summary>
        /// 检测数所库保存的Ap上下线状态是否正确，若不正确，向其发送正状态
        /// </summary>
        /// <param name="status">数据库保存状态</param>
        /// <param name="apToKen">ap信息</param>
        protected void Send2main_OnOffLineCheck(string status, Ap_Info_Struct ApInfo)
        {
            string MainControllerStatus = OffLine;
            AsyncUserToken apToKen = MyDeviceList.FindByIpPort(ApInfo.IP, ApInfo.Port);
            if (apToKen == null)
            {
                apToKen = MyDeviceList.FindByFullname(ApInfo.Fullname);
            }
            if (apToKen == null)
            {
                MainControllerStatus = OffLine;
            }
            else
            {
                MainControllerStatus = apToKen.MainControllerStatus;
            }

            OnOutputLog(LogInfoType.DEBG, "保存的状态为：" + MainControllerStatus + ";接收到状态为：" + status);

            if (String.Compare(MainControllerStatus, status, true) != 0)
            {
                Msg_Body_Struct TypeKeyValue =
                    new Msg_Body_Struct(Main2ApControllerMsgType.OnOffLine,
                    "AllOnLineNum", MyDeviceList.GetCount().ToString(),
                    "Status", MainControllerStatus,
                    "mode", apToKen.Mode,
                    "timestamp", DateTime.Now.ToLocalTime().ToString());

                //向Main模块发消息
                MessageType mt = MessageType.MSG_JSON;
                MessageBody mb = new MessageBody();

                InterModuleMsgStruct msg = new InterModuleMsgStruct();
                msg.Version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
                msg.MsgType = MsgStruct.MsgType.NOTICE.ToString();

                msg.ApInfo = ApInfo;
               
                App_Info_Struct appInfo = new App_Info_Struct();
                appInfo.Ip = AllDevice;
             
                msg.AppInfo = appInfo;
                msg.Body = TypeKeyValue;
                mb.bJson = JsonConvert.SerializeObject(msg);

                OnOutputLog(LogInfoType.INFO, string.Format("发送消息({0})给MainController模块！", TypeKeyValue.type), LogCategory.S);
                OnOutputLog(LogInfoType.DEBG, string.Format("消息内容:\n{0}", mb.bJson), LogCategory.S);

                ApManager.sendMsg_2_MainController(mt, mb);
            }
        }

        /// <summary>
        /// 发送所有在线AP的列表给 MainContorller模块
        /// </summary>
        protected void Send2main_OnLineList()
        {
            OnOutputLog(LogInfoType.DEBG, "发送所有在线AP列表给MainController模块!");

            AsyncUserToken token = new AsyncUserToken();

            Msg_Body_Struct TypeKeyValue =
                    new Msg_Body_Struct(Main2ApControllerMsgType.OnLineAPList);

            AsyncUserToken[] dList = MyDeviceList.GetConnListToArray();
            int i = 0;
            foreach (AsyncUserToken x in dList)
            {
                Name_DIC_Struct n_dic = new Name_DIC_Struct("Ap" + i++);
                n_dic.dic.Add("Sn", x.Sn);
                n_dic.dic.Add("FullName", x.FullName);
                n_dic.dic.Add("Ip", x.IPAddress.ToString());
                n_dic.dic.Add("Port", x.Port.ToString());
                n_dic.dic.Add("Type", DeviceType);
                TypeKeyValue.n_dic.Add(n_dic);
            }

            //向Main模块发消息
            OnSendMsg2Main(0, MsgStruct.MsgType.NOTICE, token, TypeKeyValue);

        }

        /// <summary>
        /// 向MainController模块发送状态改变消息(LTE/WCDMA状态)
        /// </summary>
        /// <param name="apToKen">AP信息，包含改变后的状态</param>
        /// <param name="OldDetail">改变前的状态</param>
        protected void Send2ap_ApStatusChange_LTE(AsyncUserToken apToKen, UInt32 detail,Byte ApReadySt)
        {
            UInt32 oldDetail = apToKen.Detail;
            byte oldApReadySt = apToKen.ApReadySt;
            //OnOutputLog(LogInfoType.EROR, "oldDetail=" + oldDetail + ";detail=" + detail);
            //状态改变时才发送消息
            //需去掉上下线状态，再比较
            if (((detail | AP_STATUS_LTE.OnLine) == (oldDetail | AP_STATUS_LTE.OnLine)) && (oldApReadySt == ApReadySt))
                return;

            string st = Enum.GetName(typeof(ApReadyStEnum), ApReadySt);
            if (string.IsNullOrEmpty(st))
            {
                OnOutputLog(LogInfoType.EROR, string.Format("收到设备[{0}:{1}]的addStatu为{2}错误。",
                    apToKen.IPAddress.ToString(),apToKen.Port,ApReadySt));
                return;
            }

            st = st.Replace("_", "-");

            Msg_Body_Struct TypeKeyValue =
                new Msg_Body_Struct(Main2ApControllerMsgType.ApStatusChange,
                "carry", 0,
                "detail", string.Format("0x{0:X}", detail),
                "SCTP", ((detail & AP_STATUS_LTE.SCTP) > 0) ? 1 : 0,
                "S1", ((detail & AP_STATUS_LTE.S1) > 0) ? 1 : 0,
                "GPS", ((detail & AP_STATUS_LTE.GPS) > 0) ? 1 : 0,
                "CELL", ((detail & AP_STATUS_LTE.CELL) > 0) ? 1 : 0,
                "SYNC", ((detail & AP_STATUS_LTE.SYNC) > 0) ? 1 : 0,
                "LICENSE", ((detail & AP_STATUS_LTE.LICENSE) > 0) ? 1 : 0,
                "RADIO", ((detail & AP_STATUS_LTE.RADIO) > 0) ? 1 : 0,
                "wSelfStudy", ((detail & AP_STATUS_LTE.wSelfStudy) > 0) ? 1 : 0,
                "ApReadySt", st,
                "timestamp", DateTime.Now.ToLocalTime().ToString());

            //向Main模块发消息
            OnSendMsg2Main(0, MsgStruct.MsgType.NOTICE, apToKen, TypeKeyValue);
        }

        /// <summary>
        /// 向MainController模块发送状态改变消息(GSM_ZYF/CDMA_ZYF状态)
        /// </summary>
        /// <param name="apToKen">AP信息，包含改变后的状态</param>
        /// <param name="OldDetail">改变前的状态</param>
        protected void Send2ap_ApStatusChange_GSM_ZYF(AsyncUserToken apToKen, UInt32 detail,Byte ApReadySt)
        {
            UInt32 oldDetail = apToKen.Detail;
            //byte oldApReadySt = apToKen.ApReadySt;

            //状态改变时才发送消息
            //需去掉上下线状态，再比较
            if (((detail | AP_STATUS_LTE.OnLine) == (oldDetail | AP_STATUS_LTE.OnLine)))
                return;

            string st = ApReadyStEnum.Cell_Ready.ToString();

            if (((detail & AP_STATUS_LTE.RADIO2) != (oldDetail & AP_STATUS_LTE.RADIO2)))
            {
                Msg_Body_Struct TypeKeyValue2 =
                    new Msg_Body_Struct(Main2ApControllerMsgType.ApStatusChange,
                    "carry", 1,
                    "detail", string.Format("0x{0:X}", detail),
                    "SCTP", ((detail & AP_STATUS_LTE.SCTP) > 0) ? 1 : 0,
                    "S1", ((detail & AP_STATUS_LTE.S1) > 0) ? 1 : 0,
                    "GPS", ((detail & AP_STATUS_LTE.GPS) > 0) ? 1 : 0,
                    "CELL", ((detail & AP_STATUS_LTE.CELL) > 0) ? 1 : 0,
                    "SYNC", ((detail & AP_STATUS_LTE.SYNC) > 0) ? 1 : 0,
                    "LICENSE", ((detail & AP_STATUS_LTE.LICENSE) > 0) ? 1 : 0,
                    "RADIO", ((detail & AP_STATUS_LTE.RADIO2) > 0) ? 1 : 0,
                    "wSelfStudy", ((detail & AP_STATUS_LTE.wSelfStudy) > 0) ? 1 : 0,
                    "ApReadySt", st,
                    "timestamp", DateTime.Now.ToLocalTime().ToString());

                //向Main模块发消息
                OnSendMsg2Main(0, MsgStruct.MsgType.NOTICE, apToKen, TypeKeyValue2);
            }
         
            Msg_Body_Struct TypeKeyValue =
                new Msg_Body_Struct(Main2ApControllerMsgType.ApStatusChange,
                "carry", 0,
                "detail", string.Format("0x{0:X}", detail),
                "SCTP", ((detail & AP_STATUS_LTE.SCTP) > 0) ? 1 : 0,
                "S1", ((detail & AP_STATUS_LTE.S1) > 0) ? 1 : 0,
                "GPS", ((detail & AP_STATUS_LTE.GPS) > 0) ? 1 : 0,
                "CELL", ((detail & AP_STATUS_LTE.CELL) > 0) ? 1 : 0,
                "SYNC", ((detail & AP_STATUS_LTE.SYNC) > 0) ? 1 : 0,
                "LICENSE", ((detail & AP_STATUS_LTE.LICENSE) > 0) ? 1 : 0,
                "RADIO", ((detail & AP_STATUS_LTE.RADIO) > 0) ? 1 : 0,
                "wSelfStudy", ((detail & AP_STATUS_LTE.wSelfStudy) > 0) ? 1 : 0,
                "ApReadySt", st,
                "timestamp", DateTime.Now.ToLocalTime().ToString());

            //向Main模块发消息
            OnSendMsg2Main(0, MsgStruct.MsgType.NOTICE, apToKen, TypeKeyValue);
            
        }

        protected void Send2ap_ApStatusChange_CDMA_ZYF(AsyncUserToken apToKen, UInt32 Detail, Byte ApReadySt)
        {
            this.Send2ap_ApStatusChange_LTE(apToKen, Detail, ApReadySt);
        }

        protected void Send2ap_ApStatusChange_GSM_HJT(AsyncUserToken apToKen, UInt32 Detail, Byte ApReadySt)
        {
            this.Send2ap_ApStatusChange_LTE(apToKen, Detail, ApReadySt);
        }

        /// <summary>
        /// 收到ApStatusChange_Ack后，将ap状态保存到设备列表
        /// </summary>
        /// <param name="MainMsg">收到Main模块发过来的原始消息</param>
        protected void RecvAckSaveApStatus(MsgStruct.InterModuleMsgStruct MainMsg)
        {
            if (GetMsgIntValueInList("ReturnCode", MainMsg.Body) != 0)
            {
                OnOutputLog(LogInfoType.EROR,
                    "[ApStatus_Ack]Main模块返回错误:" + GetMsgStringValueInList("ReturnStr", MainMsg.Body));
                return;
            }

            int carry = GetMsgIntValueInList("carry", MainMsg.Body);
            if (carry != 0 && carry != 1)
            {
                OnOutputLog(LogInfoType.EROR,
                    "[ApStatus_Ack]Main模块返回错误。carry不为0或1!");
                return;
            }

            UInt32 detail = 0;
            string sDetail = GetMsgStringValueInList("detail", MainMsg.Body);
            if (!string.IsNullOrEmpty(sDetail))
            {
                detail = Convert.ToUInt32(sDetail, 16);
                //UInt32 oldDetail = MyDeviceList.GetDetail(MainMsg.ApInfo.IP, MainMsg.ApInfo.Port);
                ////OnOutputLog(LogInfoType.EROR, "oldDetail=" + oldDetail);
                ////还原以前的射频状态
                //if (carry == 0)
                //{
                //    UInt32 oleRadio = oldDetail & AP_STATUS_LTE.RADIO2;
                //    detail |= oleRadio;
                //}
                //else
                //{
                //    UInt32 oleRadio = oldDetail & AP_STATUS_LTE.RADIO;
                //    detail |= oleRadio;
                //}
                //OnOutputLog(LogInfoType.EROR, "detail=" + detail);
                //修改状态
                MyDeviceList.SetDetail(detail, MainMsg.ApInfo.IP, MainMsg.ApInfo.Port);

            }
            else
            {
                OnOutputLog(LogInfoType.EROR, "Main模块返回消息中，detail字段错误!");
            }

            Byte ApReadySt = 0;
            string sApReadySt = GetMsgStringValueInList("ApReadySt", MainMsg.Body);
            if (!string.IsNullOrEmpty(sApReadySt))
            {
                sApReadySt = sApReadySt.Replace("-","_");
                ApReadySt = Convert.ToByte(Enum.Parse(typeof(ApReadyStEnum), sApReadySt)); ;
                //修改状态
                MyDeviceList.SetApReadySt(ApReadySt, MainMsg.ApInfo.IP, MainMsg.ApInfo.Port);
            }
            else
            {
                OnOutputLog(LogInfoType.EROR, "Main模块返回消息中，ApReadySt字段错误!");
            }
        }
        /// <summary>
        /// 向APP返回通用错误消息
        /// </summary>
        /// <param name="appToken">app信息</param>
        /// <param name="type">app发送过来的消息类型</param>
        /// <param name="str">错误描述</param>
        protected void Send2APP_GeneralError(AsyncUserToken appToken, string type, string str)
        {
            MessageType mt = MessageType.MSG_JSON;
            MessageBody mb = new MessageBody();

            InterModuleMsgStruct msg = new InterModuleMsgStruct();
            msg.Version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            msg.MsgType = MsgStruct.MsgType.CONFIG.ToString();

            msg.ApInfo = null;
           
            msg.AppInfo.Type = string.Empty;
            msg.AppInfo.Ip = appToken.IPAddress.ToString();
            msg.AppInfo.Port = appToken.Port;
            msg.AppInfo.Group = appToken.Group;
            msg.AppInfo.User = appToken.User;
            msg.AppInfo.Domain = appToken.Domain;

            Msg_Body_Struct TypeKeyValue =
                    new Msg_Body_Struct(AppMsgType.general_error_result,
                    "ErrStr", str,
                    "RecvType", type);

            msg.Body = TypeKeyValue;
            mb.bJson = JsonConvert.SerializeObject(msg);

            OnOutputLog(LogInfoType.INFO, string.Format("发送消息({0})给MainController模块！", TypeKeyValue.type), LogCategory.S);
            OnOutputLog(LogInfoType.DEBG, string.Format("消息内容:\n{0}", mb.bJson), LogCategory.S);

            ApManager.sendMsg_2_MainController(mt, mb);
        }

        /// <summary>
        /// 向APP返回通用错误消息
        /// </summary>
        /// <param name="apToKen">ap信息</param>
        /// <param name="appToken">app信息</param>
        /// <param name="type">app发送过来的消息类型</param>
        /// <param name="str">错误描述</param>
        protected void Send2APP_GeneralError(AsyncUserToken apToKen, AsyncUserToken appToken,string type,string str)
        {
            MessageType mt = MessageType.MSG_JSON;
            MessageBody mb = new MessageBody();

            InterModuleMsgStruct msg = new InterModuleMsgStruct();
            msg.Version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            msg.MsgType = MsgStruct.MsgType.CONFIG.ToString();

            if (apToKen == null)
            {
                msg.ApInfo = null;
            }
            else
            {
                msg.ApInfo.Type = this.DeviceType;
                msg.ApInfo.IP = apToKen.IPAddress.ToString();
                msg.ApInfo.Port = apToKen.Port;
                msg.ApInfo.SN = apToKen.Sn;
                msg.ApInfo.Fullname = apToKen.FullName;
            }
   
            msg.AppInfo.Type = string.Empty;
            msg.AppInfo.Ip = appToken.IPAddress.ToString();
            msg.AppInfo.Port = appToken.Port;
            msg.AppInfo.Group = appToken.Group;
            msg.AppInfo.User = appToken.User;
            msg.AppInfo.Domain = appToken.Domain;

            Msg_Body_Struct TypeKeyValue =
                    new Msg_Body_Struct(AppMsgType.general_error_result,
                    "ErrStr", str,
                    "RecvType",type);

            msg.Body = TypeKeyValue;
            mb.bJson = JsonConvert.SerializeObject(msg);

            OnOutputLog(LogInfoType.INFO, string.Format("发送消息({0})给MainController模块！", TypeKeyValue.type), LogCategory.S);
            OnOutputLog(LogInfoType.DEBG, string.Format("消息内容:\n{0}", mb.bJson), LogCategory.S);

            ApManager.sendMsg_2_MainController(mt, mb);
            //OnReceiveMainMsg(mt, mb);
        }

        protected void Send2APP_GeneralError(Ap_Info_Struct apInfo, App_Info_Struct appInfo, string type, string str)
        {
            MessageType mt = MessageType.MSG_JSON;
            MessageBody mb = new MessageBody();

            InterModuleMsgStruct msg = new InterModuleMsgStruct();
            msg.Version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            msg.MsgType = MsgStruct.MsgType.CONFIG.ToString();

            msg.ApInfo = apInfo;
    
            msg.AppInfo = appInfo;
           
            Msg_Body_Struct TypeKeyValue =
                    new Msg_Body_Struct(AppMsgType.general_error_result,
                    "ErrStr", str,
                    "RecvType", type);

            msg.Body = TypeKeyValue;
            mb.bJson = JsonConvert.SerializeObject(msg);

            OnOutputLog(LogInfoType.INFO, string.Format("发送消息({0})给MainController模块！", TypeKeyValue.type), LogCategory.S);
            OnOutputLog(LogInfoType.DEBG, string.Format("消息内容:\n{0}", mb.bJson), LogCategory.S);

            ApManager.sendMsg_2_MainController(mt, mb);
            //OnReceiveMainMsg(mt, mb);
        }

        protected void Send2APP_GeneralError(AsyncUserToken apToKen, App_Info_Struct appInfo, string type, string str)
        {
            MessageType mt = MessageType.MSG_JSON;
            MessageBody mb = new MessageBody();

            InterModuleMsgStruct msg = new InterModuleMsgStruct();
            msg.Version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            msg.MsgType = MsgStruct.MsgType.CONFIG.ToString();

            if (apToKen == null)
            {
                msg.ApInfo = null;
            }
            else
            {
                msg.ApInfo.Type = this.DeviceType;
                msg.ApInfo.IP = apToKen.IPAddress.ToString();
                msg.ApInfo.Port = apToKen.Port;
                msg.ApInfo.SN = apToKen.Sn;
                msg.ApInfo.Fullname = apToKen.FullName;
            }

            msg.AppInfo = appInfo;

            Msg_Body_Struct TypeKeyValue =
                    new Msg_Body_Struct(AppMsgType.general_error_result,
                    "ErrStr", str,
                    "RecvType", type);

            msg.Body = TypeKeyValue;
            mb.bJson = JsonConvert.SerializeObject(msg);

            OnOutputLog(LogInfoType.INFO, string.Format("发送消息({0})给MainController模块！", TypeKeyValue.type), LogCategory.S);
            OnOutputLog(LogInfoType.DEBG, string.Format("消息内容:\n{0}", mb.bJson), LogCategory.S);

            ApManager.sendMsg_2_MainController(mt, mb);
            //OnReceiveMainMsg(mt, mb);
        }

        protected void Send2APP_GeneralError(Ap_Info_Struct apInfo, AsyncUserToken appToken, string type, string str)
        {
            MessageType mt = MessageType.MSG_JSON;
            MessageBody mb = new MessageBody();

            InterModuleMsgStruct msg = new InterModuleMsgStruct();
            msg.Version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            msg.MsgType = MsgStruct.MsgType.CONFIG.ToString();

            msg.ApInfo = apInfo;

            msg.AppInfo.Type = string.Empty;
            msg.AppInfo.Ip = appToken.IPAddress.ToString();
            msg.AppInfo.Port = appToken.Port;
            msg.AppInfo.Group = appToken.Group;
            msg.AppInfo.User = appToken.User;
            msg.AppInfo.Domain = appToken.Domain;

            Msg_Body_Struct TypeKeyValue =
                    new Msg_Body_Struct(AppMsgType.general_error_result,
                    "ErrStr", str,
                    "RecvType", type);

            msg.Body = TypeKeyValue;
            mb.bJson = JsonConvert.SerializeObject(msg);

            OnOutputLog(LogInfoType.INFO, string.Format("发送消息({0})给MainController模块！", TypeKeyValue.type), LogCategory.S);
            OnOutputLog(LogInfoType.DEBG, string.Format("消息内容:\n{0}", mb.bJson), LogCategory.S);

            ApManager.sendMsg_2_MainController(mt, mb);
        }

        #endregion
    }
}
