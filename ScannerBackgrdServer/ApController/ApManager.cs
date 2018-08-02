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

            string outStr = string.Format("ApContr收到Main侧消息。消息内容:\n{0}\n", mb.bJson);
            Xml_codec.StaticOutputLog(LogInfoType.INFO,outStr, "APContr", LogCategory.R);

            lock (mutex_Main2Ap_Msg)
            {
                rMain2ApMsgQueue.Enqueue(mb.bJson);

                if (recvMain2ApContrMsgNum == System.UInt32.MaxValue)
                    recvMain2ApContrMsgNum = 0;
                else
                    recvMain2ApContrMsgNum++;

                count = rMain2ApMsgQueue.Count;
            }

            outStr = string.Format("ApContr共收到设备消息条数:{0},当前队列消息条数：{0}！", recvMain2ApContrMsgNum,count);
            Xml_codec.StaticOutputLog(LogInfoType.INFO, outStr, "APContr", LogCategory.R);
        }
    }

    #endregion

    class ApBase : DeviceManager
    {
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
            HashSet<AsyncUserToken> toKenList = new HashSet<AsyncUserToken>();
            HashSet<AsyncUserToken> RemovList = new HashSet<AsyncUserToken>();

            while (true)
            {
                try
                {
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
                            Send2main_OnOffLine("OffLine", i, x, MainControllerStatus);
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
                //send2main_OnOffLine("OnLine",token);
            }
            else  //AP下线，删除设备列表中的AP信息
            {
                string MainControllerStatus = MyDeviceList.GetMainControllerStatus(apToKen);
                if (string.IsNullOrEmpty(MainControllerStatus)) MainControllerStatus = "unknown";

                int i = MyDeviceList.remov(apToKen);
                if (i != -1)
                {
                    Send2main_OnOffLine("OffLine", i, apToKen, MainControllerStatus);
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
                        OnOutputLog(LogInfoType.INFO, "接收到MainController消息。");
                        OnOutputLog(LogInfoType.DEBG, string.Format("消息内容:\n{0}\n", str));
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
        /// <param name="apToken">设备信息</param>
        /// <returns></returns>
        protected string GetDeviceMsg_XML(AsyncUserToken apToken)
        {
            byte[] allMsgByte = MyDeviceList.GetAllMsgBuff(apToken);

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
                    MyDeviceList.DelMsgBuff(apToken, 0, endIndex);
                    return string.Empty;
                }
                if (startIndex >= endIndex)
                {
                    allMsgByte = null;
                    OnOutputLog(LogInfoType.WARN, "Xml消息起始标志错误！");
                    MyDeviceList.DelMsgBuff(apToken, 0, startIndex);
                    return string.Empty;
                }

                string msgStr = MyDeviceList.GetMsgBuff(apToken,startIndex, endIndex-startIndex);
                MyDeviceList.DelMsgBuff(apToken, 0, endIndex);

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
        protected void SendMsg2Ap(AsyncUserToken apToKen, byte[] buff)
        {
            OnOutputLog(LogInfoType.INFO, string.Format("发送消息给AP[{0}:{1}]！",apToKen.IPAddress.ToString(),apToKen.Port));
            OnOutputLog(LogInfoType.DEBG, string.Format("消息内容为:\n{0}\n\n", System.Text.Encoding.UTF8.GetString(buff)));
            MySocket.SendMessage(apToKen, buff);
        }

        protected void SendMsg2Ap(AsyncUserToken apToKen, string buff)
        {
            OnOutputLog(LogInfoType.INFO, string.Format("发送消息给AP[{0}:{1}]！", apToKen.IPAddress.ToString(), apToKen.Port));
            OnOutputLog(LogInfoType.DEBG, string.Format("消息内容为:\n{0}\n\n", buff));
            MySocket.SendMessage(apToKen, System.Text.Encoding.Default.GetBytes(buff));
        }

        #endregion

        #region 向MainController模块发送消息

        /// <summary>
        /// 发送消息给MainController模块
        /// </summary>
        /// <param name="msgId">Ap返回的消息Id</param>
        /// <param name="msgType">Main模块消息类型</param>
        /// <param name="apToken">Ap信息</param>
        /// <param name="TypeKeyValue">消息内容</param>
        public void OnSendMsg2Main(UInt16 msgId, MsgStruct.MsgType msgType, AsyncUserToken apToken, Msg_Body_Struct TypeKeyValue)
        {
            MessageType mt = MessageType.MSG_JSON;
            MessageBody mb = new MessageBody();

            InterModuleMsgStruct msg = new InterModuleMsgStruct();
            msg.Version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            msg.MsgType = msgType.ToString();

            msg.ApInfo.SN = apToken.Sn;
            msg.ApInfo.Fullname = apToken.FullName;
            msg.ApInfo.IP = apToken.IPAddress.ToString();
            msg.ApInfo.Port = apToken.Port;
            msg.ApInfo.Type = DeviceType;

            App_Info_Struct appInfo = new App_Info_Struct();
            if ((msgType == MsgType.NOTICE) || (msgId == 0))
            {
                appInfo.Ip = AllDevice;
            }
            else
            {
                //通过消息id查找AppInfo
                AsyncUserToken ListToken = MyDeviceList.FindByIpPort(apToken.IPAddress.ToString(), apToken.Port);
                if (ListToken == null)
                {
                    String str = string.Format(
                        "未找到设备[{0}:{1}]消息ID({2})对应的APP信息，不发送消息给MainController模块！",
                        apToken.IPAddress.ToString(), apToken.Port, msgId);
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
                //MyDeviceList.RemoveMsgId2App(apToken, msgId);
            }

            if (string.IsNullOrEmpty(appInfo.Ip))
            {
                String str = string.Format(
                    "未找到设备[{0}:{1}]消息ID({2})对应的APP信息，不发送消息给MainController模块！",
                    apToken.IPAddress.ToString(), apToken.Port, msgId);
                OnOutputLog(LogInfoType.EROR, str);

                return;
            }

            msg.AppInfo = appInfo;
            msg.Body = TypeKeyValue;
            mb.bJson = JsonConvert.SerializeObject(msg);

            //当设备状态为在线时再发送消息给Main模块
            if ((String.Compare(MyDeviceList.GetMainControllerStatus(apToken), "OnLine", true) != 0) &&
                (!TypeKeyValue.type.Equals(Main2ApControllerMsgType.OnOffLine)))
            {
                    OnOutputLog(LogInfoType.WARN,
                    string.Format("设备[{0}:{1}]在线状态为：{2}，OnLine状态才向Main模块发送消息！",
                    apToken.IPAddress.ToString(), apToken.Port.ToString(), MyDeviceList.GetMainControllerStatus(apToken)));
                return;
            }

            OnOutputLog(LogInfoType.INFO, string.Format("发送消息{0}给MainController模块！", TypeKeyValue.type), LogCategory.S);
            OnOutputLog(LogInfoType.DEBG, string.Format("消息内容:\n{0}\n", mb.bJson),LogCategory.S);

            ApManager.sendMsg_2_MainController(mt, mb);
        }

        protected void Send2main_OnOffLine(string status, int allNum, AsyncUserToken apToken)
        {
            //保存的状态有更改时才上报
            string MainControllerStatus = MyDeviceList.GetMainControllerStatus(apToken);
            if (string.IsNullOrEmpty(MainControllerStatus)) MainControllerStatus = "unknown";

            this.Send2main_OnOffLine(status,allNum,apToken,MainControllerStatus);
        }
        /// <summary>
        /// 发送AP上下线消息
        /// </summary>
        /// <param name="status">状态（OnLine：上线；OffLine:下线）</param>
        /// <param name="allNum">当前在线的AP总数</param>
        /// <param name="apToken">Ap信息</param>
        /// <param name="MainControllerStatus">发送给数据库上、下线状态</param>
        protected void Send2main_OnOffLine(string status, int allNum,AsyncUserToken apToken,string MainControllerStatus)
        {
            //保存的状态有更改时才上报
            //string MainControllerStatus = MyDeviceList.GetMainControllerStatus(token);
            //if (string.IsNullOrEmpty(MainControllerStatus)) MainControllerStatus = "unknown";

            OnOutputLog(LogInfoType.DEBG, "旧状态为：" + MainControllerStatus + "新状态为：" + status);

            if ((String.Compare(status, "OffLine", true) == 0) && (MainControllerStatus.Equals("unknown")))
            {
                OnOutputLog(LogInfoType.WARN, "未向MainController模块上报过上线消息，该下线消息不上报！");
                return;
            }

            if ((String.Compare(status, "OffLine", true) == 0) && (null != MyDeviceList.FindByFullname(apToken.FullName)))
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
                    "mode", apToken.Mode,
                    "timestamp", DateTime.Now.ToLocalTime().ToString());

                //向Main模块发消息
                OnSendMsg2Main(0,MsgStruct.MsgType.NOTICE,apToken, TypeKeyValue);

                //修改状态----测试版本中使用，正式版本中在收到Ack后更改状态。
                //if (DebugMode)
                //{
                //    MyDeviceList.SetMainControllerStatus(status, token);
                //}
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
        /// <param name="apToken">AP信息，包含改变后的状态</param>
        /// <param name="OldDetail">改变前的状态</param>
        protected void Send2ap_ApStatusChange_LTE(AsyncUserToken apToken, UInt32 OldDetail)
        {
            UInt32 detail = apToken.Detail;

            //状态改变时才发送消息
            //需去掉上下线状态，再比较
            if ((detail | AP_STATUS_LTE.OnLine) == (OldDetail | AP_STATUS_LTE.OnLine))
                return;

            Msg_Body_Struct TypeKeyValue =
                new Msg_Body_Struct(Main2ApControllerMsgType.ApStatusChange,
                "SCTP", ((detail & AP_STATUS_LTE.SCTP) > 0) ? 1 : 0,
                "S1", ((detail & AP_STATUS_LTE.S1) > 0) ? 1 : 0,
                "GPS", ((detail & AP_STATUS_LTE.GPS) > 0) ? 1 : 0,
                "CELL", ((detail & AP_STATUS_LTE.CELL) > 0) ? 1 : 0,
                "SYNC", ((detail & AP_STATUS_LTE.SYNC) > 0) ? 1 : 0,
                "LICENSE", ((detail & AP_STATUS_LTE.LICENSE) > 0) ? 1 : 0,
                "RADIO", ((detail & AP_STATUS_LTE.RADIO) > 0) ? 1 : 0,
                "wSelfStudy", ((detail & AP_STATUS_LTE.wSelfStudy) > 0) ? 1 : 0,
                "timestamp", DateTime.Now.ToLocalTime().ToString());

            //向Main模块发消息
            OnSendMsg2Main(0, MsgStruct.MsgType.NOTICE, apToken, TypeKeyValue);
        }

        /// <summary>
        /// 向MainController模块发送状态改变消息(GSM_ZYF/CDMA_ZYF状态)
        /// </summary>
        /// <param name="apToken">AP信息，包含改变后的状态</param>
        /// <param name="OldDetail">改变前的状态</param>
        protected void Send2ap_ApStatusChange_GSM_ZYF(AsyncUserToken apToken, UInt32 OldDetail)
        {
            this.Send2ap_ApStatusChange_LTE(apToken,OldDetail);
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

            OnOutputLog(LogInfoType.INFO, string.Format("发送消息{0}给MainController模块！", TypeKeyValue.type), LogCategory.S);
            OnOutputLog(LogInfoType.DEBG, string.Format("消息内容:\n{0}\n", mb.bJson), LogCategory.S);

            ApManager.sendMsg_2_MainController(mt, mb);
        }

        /// <summary>
        /// 向APP返回通用错误消息
        /// </summary>
        /// <param name="apToken">ap信息</param>
        /// <param name="appToken">app信息</param>
        /// <param name="type">app发送过来的消息类型</param>
        /// <param name="str">错误描述</param>
        protected void Send2APP_GeneralError(AsyncUserToken apToken, AsyncUserToken appToken,string type,string str)
        {
            MessageType mt = MessageType.MSG_JSON;
            MessageBody mb = new MessageBody();

            InterModuleMsgStruct msg = new InterModuleMsgStruct();
            msg.Version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            msg.MsgType = MsgStruct.MsgType.CONFIG.ToString();

            if (apToken == null)
            {
                msg.ApInfo = null;
            }
            else
            {
                msg.ApInfo.Type = this.DeviceType;
                msg.ApInfo.IP = apToken.IPAddress.ToString();
                msg.ApInfo.Port = apToken.Port;
                msg.ApInfo.SN = apToken.Sn;
                msg.ApInfo.Fullname = apToken.FullName;
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

            OnOutputLog(LogInfoType.INFO, string.Format("发送消息{0}给MainController模块！", TypeKeyValue.type), LogCategory.S);
            OnOutputLog(LogInfoType.DEBG, string.Format("消息内容:\n{0}\n", mb.bJson), LogCategory.S);

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

            OnOutputLog(LogInfoType.INFO, string.Format("发送消息{0}给MainController模块！", TypeKeyValue.type), LogCategory.S);
            OnOutputLog(LogInfoType.DEBG, string.Format("消息内容:\n{0}\n", mb.bJson), LogCategory.S);

            ApManager.sendMsg_2_MainController(mt, mb);
            //OnReceiveMainMsg(mt, mb);
        }

        protected void Send2APP_GeneralError(AsyncUserToken apToken, App_Info_Struct appInfo, string type, string str)
        {
            MessageType mt = MessageType.MSG_JSON;
            MessageBody mb = new MessageBody();

            InterModuleMsgStruct msg = new InterModuleMsgStruct();
            msg.Version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            msg.MsgType = MsgStruct.MsgType.CONFIG.ToString();

            if (apToken == null)
            {
                msg.ApInfo = null;
            }
            else
            {
                msg.ApInfo.Type = this.DeviceType;
                msg.ApInfo.IP = apToken.IPAddress.ToString();
                msg.ApInfo.Port = apToken.Port;
                msg.ApInfo.SN = apToken.Sn;
                msg.ApInfo.Fullname = apToken.FullName;
            }

            msg.AppInfo = appInfo;

            Msg_Body_Struct TypeKeyValue =
                    new Msg_Body_Struct(AppMsgType.general_error_result,
                    "ErrStr", str,
                    "RecvType", type);

            msg.Body = TypeKeyValue;
            mb.bJson = JsonConvert.SerializeObject(msg);

            OnOutputLog(LogInfoType.INFO, string.Format("发送消息{0}给MainController模块！", TypeKeyValue.type), LogCategory.S);
            OnOutputLog(LogInfoType.DEBG, string.Format("消息内容:\n{0}\n", mb.bJson), LogCategory.S);

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

            OnOutputLog(LogInfoType.INFO, string.Format("发送消息{0}给MainController模块！", TypeKeyValue.type), LogCategory.S);
            OnOutputLog(LogInfoType.DEBG, string.Format("消息内容:\n{0}\n", mb.bJson), LogCategory.S);

            ApManager.sendMsg_2_MainController(mt, mb);
        }

        #endregion
    }
}
