using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;
using ScannerBackgrdServer.Common;
using static ScannerBackgrdServer.Common.MsgStruct;
using static ScannerBackgrdServer.Common.Xml_codec;

namespace ScannerBackgrdServer.ApController
{

   
    class Ap_LTE: ApBase
    {
        public static uint heartbeatMsgNum = 0;
        public static uint imsiMsgNum = 0;

        private const string MODE_NAME = "LTE";

        public Ap_LTE()
        {
            DeviceType = MODE_NAME;
            ApBase.ReceiveMainData += OnReceiveMainMsg;
        }

        /// <summary>
        /// 重载LOG函数
        /// </summary>
        /// <param name="str"></param>
        //public override void OnOutputLog(LogInfoType type, string str)
        //{
        //    Console.WriteLine("[" + MODE_NAME + "] " + str);
        //}


        /// <summary>
        /// 重载收到Ap侧消息处理
        /// </summary>
        /// <param name="apToken">Ap信息</param>
        /// <param name="buff">消息内容</param>
        public override void OnReceiveDeviceMsg(AsyncUserToken apToken, byte[] buff)
        {
            while (true)
            {
                string msg = GetDeviceMsg_XML(apToken);
                if (string.IsNullOrEmpty(msg))
                {
                    //OnOutputLog(LogInfoType.DEBG, "消息已解析完。缓存里没有完整消息了！");
                    return;
                }
                HandleApMsg(apToken, msg);
            }
        }

        /// <summary>
        /// 处理收到的AP消息
        /// </summary>
        /// <param name="apToken"></param>
        /// <param name="msg"></param>
        private void HandleApMsg(AsyncUserToken apToken, string msg)
        { 
            //解析AP发过来的消息
            UInt16 msgId = 0;
            Msg_Body_Struct msgBody = null;
            try
            {
                msgBody = Xml_codec.DecodeApXmlMessage(msg, ref msgId);
            }
            catch (Exception)
            {
                OnOutputLog(LogInfoType.EROR, string.Format("解析收到的Ap[{0}:{1}]消息出错！",
                    apToken.IPAddress.ToString(),apToken.Port.ToString()));
                return;
            }

            if (msgBody == null)
            {
                OnOutputLog(LogInfoType.EROR, "收到消息格式错误！");
                return;
            }

            //处理透传消息
            if (msgId == APP_TRANSPARENT_MSG)
            {
                Msg_Body_Struct body = new Msg_Body_Struct(Main2ApControllerMsgType.transparent_msg_response, "transparent_msg", msg);
                OnSendMsg2Main(msgId, MsgStruct.MsgType.TRANSPARENT, apToken, body);
                return;
            }

            //心跳消息处理
            if (msgBody.type == ApMsgType.status_response)
            {
                OnOutputLog(LogInfoType.INFO, "收到心跳消息");
                if (heartbeatMsgNum == System.UInt32.MaxValue)
                    heartbeatMsgNum = 0;
                else
                    heartbeatMsgNum++;

                UInt32 oldDetail = apToken.Detail;

                UInt32 detail = 0;
                string sDetail = GetMsgStringValueInList("detail", msgBody);
                if (!string.IsNullOrEmpty(sDetail))
                    detail = Convert.ToUInt32(sDetail, 16);

                apToken.Mode = GetMsgStringValueInList("mode", msgBody);
                apToken.Sn = GetMsgStringValueInList("sn", msgBody);
                apToken.FullName = GetMsgStringValueInList("fullname", msgBody);
                if (string.IsNullOrEmpty(apToken.FullName))   //兼容老Agent拼写错误
                {
                    apToken.FullName = GetMsgStringValueInList("fullaname", msgBody);
                }
                apToken.Id = GetMsgStringValueInList("id", msgBody);
                apToken.Detail = detail;

                int i = MyDeviceList.add(apToken);
                Send2main_OnOffLine("OnLine", i, apToken);

                //判断是周期心跳，还是上线心跳
                if ((detail & (int)AP_STATUS.OnLine) > 0) //上线
                {
                    //OnOutputLog(LogInfoType.DEBG, "上线消息");
                    if ("OnLine".Equals(MyDeviceList.GetMainControllerStatus(apToken)))
                    {
                        Send2ap_status_request(apToken);
                        Thread.Sleep(1000);
                        Send2ap_get_general_para_request(apToken);
                    }
                    else
                    {
                        OnOutputLog(LogInfoType.DEBG, "MainController未回复上线成功消息！");
                    }
                }
                else //周期心跳
                {
                    //OnOutputLog(LogInfoType.DEBG, "周期心跳消息");
                }
                //发送状态改变
                Send2ap_ApStatusChange(apToken, oldDetail);
            }
            else if (msgBody.type == ApMsgType.get_general_para_response)
            {
                Msg_Body_Struct SendMsgBody = msgBody;
                SendMsgBody.type = Main2ApControllerMsgType.ReportGenPara;
                // 向Main模块发消息
                OnSendMsg2Main(0, MsgStruct.MsgType.NOTICE, apToken, SendMsgBody);
            }
            else if (msgBody.type == ApMsgType.set_general_para_response)
            {
                Msg_Body_Struct SendMsgBody = msgBody;
                SendMsgBody.type = Main2ApControllerMsgType.SetGenParaRsp;
                // 向Main模块发消息
                OnSendMsg2Main(0, MsgStruct.MsgType.NOTICE, apToken, SendMsgBody);
            }
            else if (msgBody.type == ApMsgType.imsi_list_config_result)
            {
                Msg_Body_Struct SendMsgBody = msgBody;
                SendMsgBody.type = Main2ApControllerMsgType.app_add_bwlist_response;
                // 向Main模块发消息
                OnSendMsg2Main(0, MsgStruct.MsgType.NOTICE, apToken, SendMsgBody);
            }
            else if (msgBody.type == ApMsgType.imsi_list_delconfig_result)
            {
                Msg_Body_Struct SendMsgBody = msgBody;
                SendMsgBody.type = Main2ApControllerMsgType.app_del_bwlist_response;
                // 向Main模块发消息
                OnSendMsg2Main(0, MsgStruct.MsgType.NOTICE, apToken, SendMsgBody);
            }
            else if (msgBody.type == ApMsgType.scanner)
            {
                if (imsiMsgNum == System.UInt32.MaxValue)
                    imsiMsgNum = 0;
                else
                    imsiMsgNum++;

                string imsi = GetMsgStringValueInList("imsi", msgBody);
                if (!string.IsNullOrEmpty(imsi))
                {
                    //IMSI去重处理
                    if (DataController.RemoveDupMode == 1)
                    {
                        //有该IMSI,去重，不用上报
                        if (ImsiRemoveDup.isExist(imsi))
                        {
                            OnOutputLog(LogInfoType.INFO,
                                string.Format("Imsi[{0}]在{1}时被抓到过，不再上报!",
                                imsi, ImsiRemoveDup.GetTime(imsi)));
                            return;
                        }
                        ImsiRemoveDup.add(imsi);
                    }

                    Msg_Body_Struct body = new Msg_Body_Struct(msgBody.type, msgBody.dic);

                    OnOutputLog(LogInfoType.DEBG, string.Format("上报imsi:{0}", imsi));
                    OnSendMsg2Main(0, MsgStruct.MsgType.NOTICE, apToken, body);
                }
            }
            else
            {
                Msg_Body_Struct body = new Msg_Body_Struct(msgBody.type, msgBody.dic);
                OnSendMsg2Main(msgId, MsgStruct.MsgType.CONFIG, apToken, body);
            }

            msgBody = null;
        }


        /// <summary>
        /// 重载收到Main模块消息处理
        /// </summary>
        /// <param name="msg">消息内容</param>
        public void OnReceiveMainMsg(MsgStruct.InterModuleMsgStruct MainMsg)
        {
            if (MainMsg == null || MainMsg.Body == null)
            {
                OnOutputLog(LogInfoType.EROR, "收到MainController模块消息内容为空!");
                return;
            }

            if (MainMsg.ApInfo.IP == null || MainMsg.ApInfo.Type == null)
            {
                OnOutputLog(LogInfoType.EROR, "收到MainController模块消息ApInfo内容错误!");
                return;
            }

            if ((!MainMsg.ApInfo.IP.Equals(MsgStruct.AllDevice)) && (!MainMsg.ApInfo.Type.Equals(MODE_NAME)))
            {
                //OnOutputLog(LogInfoType.DEBG, "收到MainController模块消息，不是本模块消息！");
                return;
            }

            //处理透传消息
            if (MainMsg.MsgType == MsgType.TRANSPARENT.ToString())
            {
                Send2ap_TransparentMsg(MainMsg);
                return;
            }

            HandleMainMsg(MainMsg);
            return;
        }

        /// <summary>
        /// 处理收到的Main模块的消息
        /// </summary>
        /// <param name="MainMsg">消息内容</param>
        private void HandleMainMsg(MsgStruct.InterModuleMsgStruct MainMsg)
        {
            OnOutputLog(LogInfoType.INFO, string.Format("处理MainController消息。消息类型:{0}。", MainMsg.Body.type));

            //上、下线消息回复
            if (MainMsg.Body.type == Main2ApControllerMsgType.OnOffLine_Ack)
            {
                //所有在线AP数与数据库不一至，回复所有在线AP
                if (GetMsgIntValueInList("ReturnCode", MainMsg.Body) != 0)
                {
                    OnOutputLog(LogInfoType.EROR,
                        "[OnOffLine_Ack]Main模块返回错误:" + GetMsgStringValueInList("ReturnStr", MainMsg.Body));
                    //暂时不发送，等待后续定义
                    //Send2main_OnLineList();
                }
                else
                {
                    string status = GetMsgStringValueInList("Status", MainMsg.Body);
                    if (status.Equals("OnLine") || status.Equals("OffLine"))
                    {
                        //修改状态
                        MyDeviceList.SetMainControllerStatus(status, MainMsg.ApInfo.IP, MainMsg.ApInfo.Port);
                    }
                    else
                    {
                        OnOutputLog(LogInfoType.EROR, "Main模块返回消息中，Status字段错误!");
                    }
                }
            }
            //状态改变回复
            else if (MainMsg.Body.type == Main2ApControllerMsgType.ApStatusChange_Ack)
            {
                //返回错误
                if (GetMsgIntValueInList("ReturnCode", MainMsg.Body) != 0)
                {
                    OnOutputLog(LogInfoType.EROR,
                        "[ApStatusChange_Ack]Main模块返回错误:" + GetMsgStringValueInList("ReturnStr", MainMsg.Body));
                }
                return;
            }
            else if (MainMsg.Body.type == Main2ApControllerMsgType.ReportGenParaAck)
            {
                //返回错误
                if (GetMsgIntValueInList("ReturnCode", MainMsg.Body) != 0)
                {
                    OnOutputLog(LogInfoType.EROR,
                        "[ReportGenParaAck]Main模块返回错误:" + GetMsgStringValueInList("ReturnStr", MainMsg.Body));
                }
                return;
            }
            else if (MainMsg.Body.type == Main2ApControllerMsgType.app_add_bwlist_request)
            {
                //添加黑白名单
                Send2ap_imsi_list_setconfig(MainMsg);
                return;
            }
            else if (MainMsg.Body.type == Main2ApControllerMsgType.app_del_bwlist_request)
            {
                //删除黑白名单
                Send2ap_imsi_list_delconfig(MainMsg);
                return;
            }
            else if (MainMsg.Body.type == Main2ApControllerMsgType.SetGenParaReq)
            {
                MainMsg.Body.type = ApMsgType.set_general_para_request;
                Send2ap_RecvMainMsg(MainMsg);
            }
            else //其它消息
            {
                if ((string.IsNullOrEmpty(MainMsg.ApInfo.IP)) || (MainMsg.ApInfo.IP == MsgStruct.NullDevice))
                {
                    OnOutputLog(LogInfoType.INFO, string.Format("目的设备为Null，不向Ap发送信息！"));
                    Send2APP_GeneralError(MainMsg.ApInfo,MainMsg.AppInfo,MainMsg.Body.type, 
                        string.Format("目的设备为Null，不向Ap发送信息！"));
                }
                else
                {
                    Send2ap_RecvMainMsg(MainMsg);
                }
            }
            return;
        }

        #region 封装回复AP的消息

        /// <summary>
        /// 向Ap发送Main模块过来的消息
        /// </summary>
        /// <param name="msgBody"></param>
        private void Send2ap_RecvMainMsg(InterModuleMsgStruct msgBody)
        {
            AsyncUserToken apToKen = MyDeviceList.FindByIpPort(msgBody.ApInfo.IP, msgBody.ApInfo.Port);
            if (apToKen == null)
            {
                string str = string.Format("在线AP列表中找不到Ap[{0}:{1}]设备，通过FullName重新查询设备！",
                    msgBody.ApInfo.IP, msgBody.ApInfo.Port.ToString());
                OnOutputLog(LogInfoType.WARN, str);
                apToKen = MyDeviceList.FindByFullname(msgBody.ApInfo.Fullname);
            }
            
            if (apToKen == null)
            {
                string str = string.Format("在线AP列表中找不到Ap[{0}:{1}],FullName:{2}。无法向AP发送消息！",
                    msgBody.ApInfo.IP, msgBody.ApInfo.Port.ToString(),msgBody.ApInfo.Fullname);
                OnOutputLog(LogInfoType.WARN, str);
                Send2APP_GeneralError(msgBody.ApInfo, msgBody.AppInfo, msgBody.Body.type,str);
                return;
            }

            MsgId2App msgId2App = new MsgId2App();
            msgId2App.id = addMsgId();
            msgId2App.AppInfo = msgBody.AppInfo;

            if (!MyDeviceList.AddMsgId2App(apToKen, msgId2App))
            {
                OnOutputLog(LogInfoType.EROR, string.Format("添加消息Id到设备列表出错！"));
                Send2APP_GeneralError(msgBody.ApInfo, msgBody.AppInfo, msgBody.Body.type,
                    string.Format("添加消息Id到设备列表出错！"));
                return;
            }

            byte[] sendMsg = EncodeApXmlMessage(msgId2App.id, msgBody.Body);
            if (sendMsg == null)
            {
                OnOutputLog(LogInfoType.EROR, string.Format("封装XML消息(RecvMainMsg)出错！"));
                Send2APP_GeneralError(msgBody.ApInfo, msgBody.AppInfo, msgBody.Body.type,
                    string.Format("封装向AP发送的XML消息出错！"));
                return;
            }
            SendMsg2Ap(apToKen, sendMsg);
            
        }

        /// <summary>
        /// 透传MainController模块过来的消息给设备
        /// </summary>
        /// <param name="msgBody"></param>
        private void Send2ap_TransparentMsg(InterModuleMsgStruct msgBody)
        {
            AsyncUserToken apToKen = MyDeviceList.FindByIpPort(msgBody.ApInfo.IP, msgBody.ApInfo.Port);
            if (apToKen == null)
            {
                string str = string.Format("在线AP列表中找不到Ap[{0}:{1}]设备，通过FullName重新查询设备！",
                    msgBody.ApInfo.IP, msgBody.ApInfo.Port.ToString());
                OnOutputLog(LogInfoType.WARN, str);
                apToKen = MyDeviceList.FindByFullname(msgBody.ApInfo.Fullname);
            }

            if (apToKen == null)
            {
                string str = string.Format("在线AP列表中找不到Ap[{0}:{1}],FullName:{2}。无法向AP发送消息！",
                    msgBody.ApInfo.IP, msgBody.ApInfo.Port.ToString(), msgBody.ApInfo.Fullname);
                OnOutputLog(LogInfoType.WARN, str);
                Send2APP_GeneralError(msgBody.ApInfo, msgBody.AppInfo, msgBody.Body.type, str);
                return;
            }

            MsgId2App msgId2App = new MsgId2App();
            msgId2App.id = APP_TRANSPARENT_MSG;
            msgId2App.AppInfo = msgBody.AppInfo;

            if (!MyDeviceList.AddMsgId2App(apToKen, msgId2App))
            {
                OnOutputLog(LogInfoType.EROR, string.Format("添加消息Id到设备列表出错！"));
                Send2APP_GeneralError(msgBody.ApInfo, msgBody.AppInfo, msgBody.Body.type,
                    string.Format("添加消息Id到设备列表出错！"));
                return;
            }

            string sendMsg = GetMsgStringValueInList("transparent_msg", msgBody.Body);
            if (string.IsNullOrEmpty(sendMsg))
            {
                OnOutputLog(LogInfoType.EROR, string.Format("封装XML消息(RecvMainMsg)出错！"));
                Send2APP_GeneralError(msgBody.ApInfo, msgBody.AppInfo, msgBody.Body.type,
                    string.Format("封装向AP发送的XML消息出错！"));
                return;
            }
            SendMsg2Ap(apToKen, sendMsg);

        }

        /// <summary>
        /// 向AP回应心跳消息
        /// </summary>
        /// <param name="apToken">AP信息</param>
        private void Send2ap_status_request(AsyncUserToken apToken)
        {
            //向AP回复心跳
            Msg_Body_Struct TypeKeyValue =
                new Msg_Body_Struct(ApMsgType.status_request,
                "timeout", 5,
                "timestamp", DateTime.Now.ToLocalTime().ToString());

            byte[] sendMsg = EncodeApXmlMessage(0, TypeKeyValue);
            if (sendMsg == null)
            {
                OnOutputLog(LogInfoType.EROR, string.Format("封装XML消息(status_request)出错！"));
                return;
            }
            SendMsg2Ap(apToken, sendMsg);
        }

        /// <summary>
        /// 获取AP的配置信息
        /// </summary>
        /// <param name="apToken">AP信息</param>
        private void Send2ap_get_general_para_request(AsyncUserToken apToken)
        {
            Msg_Body_Struct TypeKeyValue =
                new Msg_Body_Struct(ApMsgType.get_general_para_request,
                "timeout", 5,
                "timestamp", DateTime.Now.ToLocalTime().ToString());

            byte[] sendMsg = EncodeApXmlMessage(0, TypeKeyValue);
            if (sendMsg == null)
            {
                OnOutputLog(LogInfoType.EROR, string.Format("封装XML消息(get_general_para_request)出错！"));
                return;
            }
            SendMsg2Ap(apToken, sendMsg);
        }

        /// <summary>
        /// 向AP添加黑白名单
        /// </summary>
        /// <param name="token">AP信息</param>
        private void Send2ap_imsi_list_setconfig(InterModuleMsgStruct msgBody)
        {
            AsyncUserToken apToken = MyDeviceList.FindByIpPort(msgBody.ApInfo.IP, msgBody.ApInfo.Port);
            if (apToken == null)
            {
                string str = string.Format("在线AP列表中找不到Ap[{0}:{1}]设备，通过FullName重新查询设备！",
                    msgBody.ApInfo.IP, msgBody.ApInfo.Port.ToString());
                OnOutputLog(LogInfoType.WARN, str);
                apToken = MyDeviceList.FindByFullname(msgBody.ApInfo.Fullname);
            }

            if (apToken == null)
            {
                string str = string.Format("在线AP列表中找不到Ap[{0}:{1}],FullName:{2}。无法向AP发送消息！",
                    msgBody.ApInfo.IP, msgBody.ApInfo.Port.ToString(), msgBody.ApInfo.Fullname);
                OnOutputLog(LogInfoType.WARN, str);
                Send2APP_GeneralError(msgBody.ApInfo, msgBody.AppInfo, msgBody.Body.type, str);
                return;
            }

            Msg_Body_Struct TypeKeyValue = new Msg_Body_Struct(ApMsgType.imsi_list_setconfig);
            int i = 0;
            foreach (Name_DIC_Struct n_dicList in msgBody.Body.n_dic)
            {
                Dictionary<string, object> dic = n_dicList.dic;

                String bwFlag = GetMsgStringValueInList("bwFlag",dic);
                if (!bwFlag.Equals("black") && !bwFlag.Equals("white"))
                {
                    OnOutputLog(LogInfoType.WARN, "添加黑白名单参数错误。bwFlag字段错误!");
                    Send2APP_GeneralError(msgBody.ApInfo, msgBody.AppInfo, msgBody.Body.type,
                        "添加黑白名单参数错误。bwFlag字段错误!");
                    continue;
                }

                if (bwFlag.Equals("white"))
                {
                    String imsi = GetMsgStringValueInList("imsi", dic);
                    if (imsi.Equals(String.Empty))
                    {
                        OnOutputLog(LogInfoType.WARN, "添加白名单参数错误。imsi字段错误!");
                        Send2APP_GeneralError(msgBody.ApInfo, msgBody.AppInfo, msgBody.Body.type,
                            "添加白名单参数错误。imsi字段错误!");
                        continue;
                    }

                    Name_DIC_Struct n_dic = new Name_DIC_Struct("name" + i++);
                    n_dic.dic.Add("whiteimsi/imsi", imsi);
                    TypeKeyValue.n_dic.Add(n_dic);
                }
                if (bwFlag.Equals("black"))
                {
                    String imsi = GetMsgStringValueInList("imsi", dic);
                    if (imsi.Equals(String.Empty))
                    {
                        OnOutputLog(LogInfoType.WARN, "添加黑名单参数错误。imsi字段错误!");
                        Send2APP_GeneralError(msgBody.ApInfo, msgBody.AppInfo, msgBody.Body.type,
                            "添加黑名单参数错误。imsi字段错误!");
                        continue;
                    }
                    String rbStart = GetMsgStringValueInList("rbStart", dic);
                    if (rbStart.Equals(String.Empty))
                    {
                        string str = string.Format("添加黑名单参数错误。imsi({0})rbStart字段错误!",imsi);
                        OnOutputLog(LogInfoType.WARN, str);
                        Send2APP_GeneralError(msgBody.ApInfo, msgBody.AppInfo, msgBody.Body.type,str);
                        continue;
                    }
                    String rbEnd = GetMsgStringValueInList("rbEnd", dic);
                    if (rbEnd.Equals(String.Empty))
                    {
                        string str = string.Format("添加黑名单参数错误。imsi({0})rbEnd字段错误!", imsi);
                        OnOutputLog(LogInfoType.WARN, str);
                        Send2APP_GeneralError(msgBody.ApInfo, msgBody.AppInfo, msgBody.Body.type, str);
                        continue;
                    }
                    Name_DIC_Struct n_dic = new Name_DIC_Struct("name" + i++);
                    n_dic.dic.Add("blackimsi/imsi", imsi);
                    n_dic.dic.Add("blackimsi/dedicatedRB_start", rbStart);
                    n_dic.dic.Add("blackimsi/dedicatedRB_end", rbEnd);
                    TypeKeyValue.n_dic.Add(n_dic);
                }
            }

            if (i == 0)
            {
                OnOutputLog(LogInfoType.EROR, "添加黑白名单参数错误。没有要发送的名单!");
                Send2APP_GeneralError(msgBody.ApInfo, msgBody.AppInfo, msgBody.Body.type,
                            "添加黑白名单参数错误。没有要发送的名单!");
                return;
            }
            byte[] sendMsg = EncodeApXmlMessage(0, TypeKeyValue);
            if (sendMsg == null)
            {
                OnOutputLog(LogInfoType.EROR, "封装XML消息(imsi_list_setconfig)出错！");
                Send2APP_GeneralError(msgBody.ApInfo, msgBody.AppInfo, msgBody.Body.type,
                            "封装XML消息(imsi_list_setconfig)出错！");
                return;
            }
            SendMsg2Ap(apToken, sendMsg);
        }

        /// <summary>
        /// 向AP删除黑白名单
        /// </summary>
        /// <param name="token">AP信息</param>
        private void Send2ap_imsi_list_delconfig(InterModuleMsgStruct msgBody)
        {
            AsyncUserToken apToken = MyDeviceList.FindByIpPort(msgBody.ApInfo.IP, msgBody.ApInfo.Port);
            if (apToken == null)
            {
                string str = string.Format("在线AP列表中找不到Ap[{0}:{1}]设备，通过FullName重新查询设备！",
                    msgBody.ApInfo.IP, msgBody.ApInfo.Port.ToString());
                OnOutputLog(LogInfoType.WARN, str);
                apToken = MyDeviceList.FindByFullname(msgBody.ApInfo.Fullname);
            }

            if (apToken == null)
            {
                string str = string.Format("在线AP列表中找不到Ap[{0}:{1}],FullName:{2}。无法向AP发送消息！",
                    msgBody.ApInfo.IP, msgBody.ApInfo.Port.ToString(), msgBody.ApInfo.Fullname);
                OnOutputLog(LogInfoType.WARN, str);
                Send2APP_GeneralError(msgBody.ApInfo, msgBody.AppInfo, msgBody.Body.type, str);
                return;
            }

            Msg_Body_Struct TypeKeyValue = new Msg_Body_Struct(ApMsgType.imsi_list_delconfig);
            int i = 0;
            foreach (Name_DIC_Struct n_dicList in msgBody.Body.n_dic)
            {
                Dictionary<string, object> dic = n_dicList.dic;

                String bwFlag = GetMsgStringValueInList("bwFlag", dic);
                if (!bwFlag.Equals("black") && !bwFlag.Equals("white"))
                {
                    OnOutputLog(LogInfoType.WARN, "删除黑白名单参数错误。bwFlag字段错误!");
                    Send2APP_GeneralError(msgBody.ApInfo, msgBody.AppInfo, msgBody.Body.type,
                            "删除黑白名单参数错误。bwFlag字段错误！");
                    continue;
                }

                if (bwFlag.Equals("white"))
                {
                    String imsi = GetMsgStringValueInList("imsi", dic);
                    if (imsi.Equals(String.Empty))
                    {
                        OnOutputLog(LogInfoType.WARN, "删除白名单参数错误。imsi字段错误!");
                        Send2APP_GeneralError(msgBody.ApInfo, msgBody.AppInfo, msgBody.Body.type,
                            "删除白名单参数错误。imsi字段错误！");
                        continue;
                    }

                    Name_DIC_Struct n_dic = new Name_DIC_Struct("name" + i++);
                    n_dic.dic.Add("whiteimsi/imsi", imsi);
                    TypeKeyValue.n_dic.Add(n_dic);
                }
                if (bwFlag.Equals("black"))
                {
                    String imsi = GetMsgStringValueInList("imsi", dic);
                    if (imsi.Equals(String.Empty))
                    {
                        OnOutputLog(LogInfoType.WARN, "删除黑名单参数错误。imsi字段错误!");
                        Send2APP_GeneralError(msgBody.ApInfo, msgBody.AppInfo, msgBody.Body.type,
                            "删除黑名单参数错误。imsi字段错误！");
                        continue;
                    }
                
                    Name_DIC_Struct n_dic = new Name_DIC_Struct("name" + i++);
                    n_dic.dic.Add("blackimsi/imsi", imsi);
                    TypeKeyValue.n_dic.Add(n_dic);
                }
            }

            if (i == 0)
            {
                OnOutputLog(LogInfoType.EROR, "删除黑名单参数错误。没有要发送的名单!");
                Send2APP_GeneralError(msgBody.ApInfo, msgBody.AppInfo, msgBody.Body.type,
                            "删除黑名单参数错误。没有要发送的名单！");
                return;
            }
            byte[] sendMsg = EncodeApXmlMessage(0, TypeKeyValue);
            if (sendMsg == null)
            {
                OnOutputLog(LogInfoType.EROR, string.Format("封装XML消息(imsi_list_delconfig)出错！"));
                Send2APP_GeneralError(msgBody.ApInfo, msgBody.AppInfo, msgBody.Body.type,
                            "封装XML消息(imsi_list_delconfig)出错！");
                return;
            }
            SendMsg2Ap(apToken, sendMsg);
        }
        #endregion
    }
}
