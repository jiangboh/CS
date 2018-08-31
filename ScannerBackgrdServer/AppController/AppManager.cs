using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using ScannerBackgrdServer.ApController;
using ScannerBackgrdServer.Common;
using static ScannerBackgrdServer.Common.MsgStruct;

namespace ScannerBackgrdServer.AppController
{
    #region MainController消息处理类

    class AppManager
    {
        /// <summary>
        /// MainController消息处理锁
        /// </summary>
        public static readonly object mutex_Main2App_Msg = new object();
        /// <summary>
        /// MainController消息接收队列
        /// </summary>
        public static Queue<string> rMain2AppMsgQueue = new Queue<string>();

        /// <summary>
        /// 已接收MainController到AppController消息条数
        /// </summary>
        public static uint recvMain2AppContrMsgNum = 0;
        /// <summary>
        /// 已处理MainController到AppController消息条数
        /// </summary>
        public static uint handleMain2AppContrMsgNum = 0;

        //声明用于发送信息给MainController的代理
        public static MessageDelegate sendMsg_2_MainController = new MessageDelegate(FrmMainController.MessageDelegate_For_AppController);

        /// <summary>
        /// 接收MainController发来的消息，存贮到消息队列中
        /// </summary>
        /// <param name="mt"></param>
        /// <param name="mb"></param>
        public static void MessageDelegate_For_MainController(MessageType mt, MessageBody mb)
        {
            int count = 0;

            Xml_codec.StaticOutputLog(LogInfoType.INFO, "AppContr收到Main侧消息。", "APPContr", LogCategory.R);
            Xml_codec.StaticOutputLog(LogInfoType.DEBG, string.Format("消息内容:\n{0}", mb.bJson), "APPContr", LogCategory.R);

            lock (mutex_Main2App_Msg)
            {
                rMain2AppMsgQueue.Enqueue(mb.bJson);

                if (recvMain2AppContrMsgNum == System.UInt32.MaxValue)
                    recvMain2AppContrMsgNum = 0;
                else
                    recvMain2AppContrMsgNum++;

                count = rMain2AppMsgQueue.Count;
            }

            string outStr = string.Format("ApContr共收到设备消息条数:{0},当前队列消息条数：{1}！", recvMain2AppContrMsgNum,count);
            Xml_codec.StaticOutputLog(LogInfoType.INFO, outStr, "APContr", LogCategory.R);
        }
    }

    #endregion

    class AppBase : DeviceManager
    {
        private const byte jsonStartFlag = (int)'{';
        private const byte jsonEndFlag = (int)'}';

        /// <summary>
        /// 接收到Main模块的数据委托
        /// </summary>
        /// <param name="MainMsg">消息内容</param>
        public delegate void OnReceiveMainData(InterModuleMsgStruct MainMsg);
        /// <summary>  
        /// 接收到Main模块的数据事件  
        /// </summary>  
        public static event OnReceiveMainData ReceiveMainData;

        public AppBase()
        {
            //启动AP状态检测线程
            Thread t = new Thread(new ThreadStart(CheckAppStatusThread));
            t.Start();
            t.IsBackground = true;

            // 启动处理MainController模块消息线程
            Thread t2 = new Thread(new ParameterizedThreadStart(ReceiveMainMsgThread));
            t2.Start(string.Empty);
            t2.IsBackground = true;
        }

        /// <summary>
        /// 检测App在线状态
        /// </summary>
        private void CheckAppStatusThread()
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
                        //if (timeSpan.TotalSeconds > new ModuleConfig().AppOnLineTime) //大于180秒认为设备下线
                        if (timeSpan.TotalSeconds > DataController.AppOnLineTime) //大于180秒认为设备下线
                        {
                            RemovList.Add(x);
                        }
                    }
                    //删除已下线的AP
                    foreach (AsyncUserToken x in RemovList)
                    {
                        int i = MyDeviceList.remov(x);
                        if (i != -1)
                        {
                            CloseToken(x);
                            OnOutputLog(LogInfoType.INFO, string.Format("App[{0}:{1}]下线了！！！", x.IPAddress, x.Port.ToString()));
                            //只发送AP的上下线消息，不发送APP的上下线消息
                            //Send2main_OnOffLine(OffLine, x); 
                        }
                    }
                    //OnOutputLog(LogInfoType.DEBG, "当前在线App数量为：" + MyDeviceList.GetCount().ToString() + "台 ！");

                    toKenList.Clear();
                    toKenList.TrimExcess();
                    RemovList.Clear();
                    RemovList.TrimExcess();

                    Thread.Sleep(3000);
                }
                catch (Exception e)
                {
                    OnOutputLog(LogInfoType.EROR, string.Format("线程[CheckAppStatusThread]出错。错误码：{0}", e.Message));
                }
            }
        }

        #region 与MainController模块收发消息处理

        /// <summary>
        /// 从消息中找到一条完整的Json消息。返回Json起始、结束索引号
        /// </summary>
        /// <param name="buff">消息</param>
        /// <param name="startIndex">起始索引</param>
        /// <param name="endIndex">结束索引</param>
        /// <returns>是否有完整Json消息</returns>
        private bool GetFlag(byte[] buff, ref int startIndex, ref int endIndex)
        {
            startIndex = -1;
            endIndex = -1;
            //起始标志数
            int sFlagNum = 0;
            for (int i = 0; i < buff.Length; i++)
            {
                if (buff[i] == jsonStartFlag)
                {
                    int j = i;
                    for (j = i+1;j<buff.Length;j++)
                    {
                        if (buff[j] != '\n' && buff[j] != '\r' && buff[j] != '\t' && buff[j] != ' ')
                        {
                            break;
                        }
                    }

                    if (buff[j] == '"'
                        && (buff[j + 1] == 'A' || buff[j + 1] == 'a')
                        && (buff[j + 2] == 'P' || buff[j + 2] == 'p'))
                    {
                        startIndex = i;
                        sFlagNum=1;
                    }
                    else
                    {
                        sFlagNum++;
                    }
                }

                if (buff[i] == jsonEndFlag)
                {
                    if (sFlagNum > 0)
                        sFlagNum--;
                    if (sFlagNum == 0)
                    {
                        endIndex = i + 1; //1为sFlag的长度
                        break;
                    }
                }
            }

            if (endIndex != -1 && startIndex != -1)
                return true;

            return false;
        }

        /// <summary>
        /// 防止粘包，检查Json消息的起始标志，并从消息中返回一条完整的Json消息
        /// </summary>
        /// <param name="appToKen">设备信息</param>
        /// <returns></returns>
        protected string GetDeviceMsg_JSON(AsyncUserToken appToKen)
        {
            int startIndex = -1;
            int endIndex = -1;

            byte[] allMsgByte = MyDeviceList.GetAllMsgBuff(appToKen);
            GetFlag(allMsgByte, ref startIndex, ref endIndex);

            if (endIndex != -1)
            {
                if (startIndex == -1)
                {
                    OnOutputLog(LogInfoType.WARN, "未找到Json消息起始标志！");
                    MyDeviceList.DelMsgBuff(appToKen, 0, endIndex);
                    return string.Empty;
                }
        
                if (startIndex >= endIndex)
                {
                    OnOutputLog(LogInfoType.WARN, "Json消息起始标志错误！");
                    MyDeviceList.DelMsgBuff(appToKen, 0, startIndex);
                    return string.Empty;
                }

                string msgStr = MyDeviceList.GetMsgBuff(appToKen, startIndex, endIndex-startIndex); 
                MyDeviceList.DelMsgBuff(appToKen, 0, endIndex);

                //OnOutputLog(LogInfoType.INFO, "收到消息：————————————————————");
                //OnOutputLog(LogInfoType.INFO, msgStr);
                //OnOutputLog(LogInfoType.INFO, "————————————————————");
                return msgStr;
            }

            return string.Empty;
        }

        /// <summary>
        /// 发送消息到MainContrller模块
        /// </summary>
        /// <param name="appToKen">App连接信息</param>
        /// <param name="ApInfo">Ap信息</param>
        /// <param name="TypeKeyValue">发送内容</param>
        protected void OnSendMsg2Main(AsyncUserToken appToKen, Ap_Info_Struct ApInfo, Msg_Body_Struct TypeKeyValue)
        {
            MessageType mt = MessageType.MSG_JSON;
            MessageBody mb = new MessageBody();

            InterModuleMsgStruct msg = new InterModuleMsgStruct();

            msg.Version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            msg.MsgType = MsgType.CONFIG.ToString();

            msg.AppInfo.User = appToKen.User;
            msg.AppInfo.Group = appToKen.Group;
            msg.AppInfo.Domain = appToKen.Domain;
            msg.AppInfo.Ip = appToKen.IPAddress.ToString();
            msg.AppInfo.Port = appToKen.Port;
            msg.AppInfo.Type = DeviceType;

            msg.ApInfo = ApInfo;

            msg.Body = TypeKeyValue;
            mb.bJson = JsonConvert.SerializeObject(msg);

            OnOutputLog(LogInfoType.INFO, string.Format("发送消息给MainController模块！"), LogCategory.S);
            OnOutputLog(LogInfoType.DEBG, string.Format("消息内容:\n{0}", mb.bJson), LogCategory.S);

            AppManager.sendMsg_2_MainController(mt, mb);
        }

        /// <summary>
        /// 向AP发送透传消息
        /// </summary>
        /// <param name="appToKen"></param>
        /// <param name="ApInfo"></param>
        /// <param name="TypeKeyValue"></param>
        protected void OnSendTransparentMsg2Main(AsyncUserToken appToKen, Ap_Info_Struct ApInfo, Msg_Body_Struct TypeKeyValue)
        {
            MessageType mt = MessageType.MSG_JSON;
            MessageBody mb = new MessageBody();

            InterModuleMsgStruct msg = new InterModuleMsgStruct();

            msg.Version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            msg.MsgType = MsgType.TRANSPARENT.ToString();

            msg.AppInfo.User = appToKen.User;
            msg.AppInfo.Group = appToKen.Group;
            msg.AppInfo.Domain = appToKen.Domain;
            msg.AppInfo.Ip = appToKen.IPAddress.ToString();
            msg.AppInfo.Port = appToKen.Port;
            msg.AppInfo.Type = DeviceType;

            msg.ApInfo = ApInfo;

            string sData = GetMsgStringValueInList("transparent_msg", TypeKeyValue);
            //sData = Regex.Replace(sData, "<\\s*id\\s*>.+<\\s*/\\s*id\\s*>", string.Format("<id>{0}</id>", ApMsgIdClass.addTransparentMsgId()));
            Dictionary<string, object> dic = new Dictionary<string, object>();
            dic.Add("transparent_msg", sData);

            TypeKeyValue.dic = dic;
            msg.Body = TypeKeyValue;
            mb.bJson = JsonConvert.SerializeObject(msg);

            OnOutputLog(LogInfoType.INFO, string.Format("发送消息给MainController模块！"), LogCategory.S);
            OnOutputLog(LogInfoType.DEBG, string.Format("消息内容:\n{0}", mb.bJson), LogCategory.S);

            AppManager.sendMsg_2_MainController(mt, mb);
        }

        /// <summary>
        /// 已封装好的Json消息发送给MainContrller模块。一般用于消息转发
        /// </summary>
        /// <param name="buff"></param>
        protected void OnSendMsg2Main(byte[] buff)
        {
            MessageType mt;
            MessageBody mb = new MessageBody(); ;

            mt = MessageType.MSG_JSON;
            mb.bJson = System.Text.Encoding.Default.GetString(buff);

            OnOutputLog(LogInfoType.INFO, string.Format("发送消息给MainController模块！"), LogCategory.S);
            OnOutputLog(LogInfoType.DEBG, string.Format("消息内容:\n{0}", mb.bJson), LogCategory.S);

            AppManager.sendMsg_2_MainController(mt, mb);
        }

        /// <summary>
        /// 接收到MainController模块消息处理线程
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

                    lock (AppManager.mutex_Main2App_Msg)
                    {
                        if (AppManager.rMain2AppMsgQueue.Count <= 0)
                        {
                            noMsg = true;
                            hNum = 0;
                            continue;
                        }
                        else
                        {
                            noMsg = false;
                            hNum++;
                            str = AppManager.rMain2AppMsgQueue.Dequeue();
                        }
                        count = AppManager.rMain2AppMsgQueue.Count;
                    }

                    if (AppManager.handleMain2AppContrMsgNum == System.UInt32.MaxValue)
                        AppManager.handleMain2AppContrMsgNum = 0;
                    else
                        AppManager.handleMain2AppContrMsgNum++;

                    //解析收到的消息
                    InterModuleMsgStruct MainMsg = null;
                    try
                    {
                        MainMsg = JsonConvert.DeserializeObject<InterModuleMsgStruct>(str);
                    }
                    catch (Exception)
                    {
                        OnOutputLog(LogInfoType.EROR, "解析收到的Main模块消息出错！", LogCategory.I);
                        OnOutputLog(LogInfoType.INFO, string.Format("共处理Main2Ap消息条数:{0}，当前队列消息条数：{1}!",
                                AppManager.handleMain2AppContrMsgNum, count));
                        continue;
                    }

                    if ((MainMsg.AppInfo.Ip.Equals(MsgStruct.AllDevice)) || (MainMsg.AppInfo.Type.Equals(DeviceType)))
                    {
                        OnOutputLog(LogInfoType.INFO, "接收到MainController消息。");
                        OnOutputLog(LogInfoType.DEBG, string.Format("消息内容:\n{0}", str));
                    }

                    AppInnerType flag;
                    if ((!MainMsg.AppInfo.Ip.Equals(MsgStruct.AllDevice)) 
                        && (!(Enum.TryParse<AppInnerType>(MainMsg.AppInfo.Type, true, out flag))))
                    {
                        if (string.IsNullOrEmpty(MainMsg.AppInfo.Type)) MainMsg.AppInfo.Type = "空";
                        OnOutputLog(LogInfoType.EROR, "收到MainController模块消息中APP类型错误!收到类型为:" + MainMsg.AppInfo.Type);
                        continue;
                    }

                    if (ReceiveMainData != null && MainMsg != null)
                        ReceiveMainData(MainMsg);

                    OnOutputLog(LogInfoType.INFO, string.Format("共处理Main2Ap消息条数:{0}，当前队列消息条数：{1}!",
                                AppManager.handleMain2AppContrMsgNum, count));
                }
                catch (Exception e)
                {
                    OnOutputLog(LogInfoType.EROR, string.Format("线程[ReceiveMainMsgThread]出错。错误码：{0}", e.Message));
                }
            }
        }

        #endregion

        /// <summary>
        /// 重写设备更改事件
        /// </summary>
        /// <param name="num">num大于0表示设备上线，否则设备下线</param>
        /// <param name="appToKen">设备信息</param>
        public override void OnDeviceNumberChange(int num, AsyncUserToken appToKen)
        {
            if (num > 0)
            {
                //OnOutputLog(LogInfoType.EROR, string.Format("上上上线APP：[{0}:{1}]：", appToKen.IPAddress.ToString(),appToKen.Port));
                MyDeviceList.add(appToKen);
                //在收到心跳消息时上报
                //send2main_OnOffLine(OnLine,token);
            }
            else  //AP下线，删除设备列表中的AP信息
            {
                //OnOutputLog(LogInfoType.EROR, string.Format("下下下线APP：[{0}:{1}]：", appToKen.IPAddress.ToString(), appToKen.Port));
                MyDeviceList.remov(appToKen);
            }
        }

        /// <summary>
        /// 发送消息到App
        /// </summary>
        /// <param name="appInfo">Ap信息</param>
        /// <param name="stdeviceServerMsgStructr">消息内容</param>
        protected void SendMsg2App(App_Info_Struct appInfo, DeviceServerMsgStruct deviceServerMsgStruct)
        {
            try
            {
                if ((string.IsNullOrEmpty(appInfo.Ip)) || (appInfo.Ip == MsgStruct.NullDevice))
                {
                    OnOutputLog(LogInfoType.INFO, string.Format("目的设备为Null，不向App发送信息！"));
                    return;
                }

                string strJosn = JsonConvert.SerializeObject(deviceServerMsgStruct);
                byte[] buff = System.Text.Encoding.Default.GetBytes(strJosn);

                if (appInfo.Ip == MsgStruct.AllDevice)
                {
                    OnOutputLog(LogInfoType.INFO, string.Format("目的设备为All，向所有App发送信息！"));
                    //HashSet<AsyncUserToken>  toKenList = MyDeviceList.GetConnList();
                    AsyncUserToken[] toKenList = MyDeviceList.GetConnListToArray();
                    if (toKenList.Length > 0)
                    {
                        foreach (AsyncUserToken appToKen in toKenList)
                        {
                            OnOutputLog(LogInfoType.INFO, string.Format("发送消息{0}给APP[{1}:{2}]！",
                                deviceServerMsgStruct.Body.type, appToKen.IPAddress, appToKen.Port), LogCategory.S);
                            OnOutputLog(LogInfoType.DEBG, string.Format("消息内容:\n{0}", strJosn), LogCategory.S);
                            MySocket.SendMessage(appToKen, buff);
                        }
                    }
                }
                else
                {
                    AsyncUserToken appToKen = MyDeviceList.FindByIpPort(appInfo.Ip, appInfo.Port);
                    if (appToKen == null)
                    {
                        OnOutputLog(LogInfoType.WARN, string.Format("设备列表中未找到该App设备[{0}:{1}]信息！", appInfo.Ip, appInfo.Port));
                        return;
                    }

                    OnOutputLog(LogInfoType.INFO, string.Format("发送消息{0}给APP[{1}:{2}]！",
                                deviceServerMsgStruct.Body.type, appToKen.IPAddress, appToKen.Port), LogCategory.S);
                    OnOutputLog(LogInfoType.DEBG, string.Format("消息内容:\n{0}", strJosn), LogCategory.S);
                    MySocket.SendMessage(appToKen, buff);
                }
            }
            catch (Exception ee)
            {
                OnOutputLog(LogInfoType.EROR, "发送消息到App出错。出错原因："+ee.Message, LogCategory.I);
            }

            return;
        }

        /// <summary>
        /// 发送消息到App
        /// </summary>
        /// <param name="appToKen">App信息</param>
        /// <param name="TypeKeyValue">消息内容</param>
        protected void SendMsg2App(AsyncUserToken appToKen, Msg_Body_Struct TypeKeyValue)
        {
            if (appToKen == null)
            {
                OnOutputLog(LogInfoType.WARN, string.Format("目的设备信息为NULL！"));
                return;
            }
            DeviceServerMsgStruct msgStruct = new DeviceServerMsgStruct();
            msgStruct.Version = Assembly.GetExecutingAssembly().GetName().Version.ToString();

            msgStruct.ApInfo.IP = MsgStruct.NullDevice;

            msgStruct.Body = TypeKeyValue;
            string strJosn = JsonConvert.SerializeObject(msgStruct);
            if (-1 == strJosn.IndexOf(AppMsgType.app_heartbeat_response))
            {
                OnOutputLog(LogInfoType.INFO, string.Format("发送消息{0}给APP[{1}:{2}]！",
                            TypeKeyValue.type, appToKen.IPAddress, appToKen.Port), LogCategory.S);
                OnOutputLog(LogInfoType.DEBG, string.Format("消息内容:\n{0}", strJosn), LogCategory.S);
            }

            byte[] buff = System.Text.Encoding.Default.GetBytes(strJosn);

            MySocket.SendMessage(appToKen, buff);
        }

        /// <summary>
        /// 发送通用出错消息给App
        /// </summary>
        /// <param name="appToKen">App信息</param>
        /// <param name="type">接收到的消息类型</param>
        /// <param name="str">出错信息描述</param>
        protected void SendErrorMsg2App(AsyncUserToken appToKen, AppMsgType type, String str)
        {
            string sType = string.Empty;
            if (type != null)
            {
                sType = type.ToString();
            }

            Msg_Body_Struct TypeKeyValue =
                    new Msg_Body_Struct(AppMsgType.general_error_result,
                    "ErrStr", str,
                    "RecvType", sType);
            SendMsg2App(appToKen, TypeKeyValue);
        }
    }
}
