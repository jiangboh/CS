using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using ScannerBackgrdServer.Common;
using static ScannerBackgrdServer.Common.MsgStruct;
using static ScannerBackgrdServer.Common.Xml_codec;

namespace ScannerBackgrdServer.ApController
{
    class Ap_CDMA_ZYF : ApBase
    {
        #region 消息类型及结据结构定义
        private enum Device_Sys
        {
            Sys1 = 0,
            Sys2
        }
        private enum Protocol_Sap
        {
            GSM = 241,
            CDMA = 242
        }
        private enum Msg_Type
        {
            INITIAL_MSG = 0,
            SUCC_OUTCOME = 1,
            UNSUCC_OUTCOME = 2
        }

        /// <summary>
        /// 控制端发送给设备的消息类型
        /// </summary>
        private enum Send_Msg_Id
        {
            QUERY_NB_CELL_INFO_MSG = 202,  //	GUI查询邻区信息
            CONFIG_FAP_MSG = 204,  //GUI配置FAP的启动参数
            CONTROL_FAP_REBOOT_MSG = 205,  //GUI控制FAP的重启。
            CONTROL_FAP_RF_MSG = 206,  //控制FAP射频的消息
            CONFIG_SMS_CONTENT_MSG =192,	    //配置下发短信号码和内容（下发，界面报成功、报数据库）
            CONTROL_FAP_RADIO_ON_MSG = 193,  //	GUI 控制FAP开启射频
            CONTROL_FAP_RADIO_OFF_MSG = 194,  //GUI 控制FAP关闭射频
            CONTROL_FAP_RESET_MSG = 195,  //GUI 控制FAP的软复位
            CONFIG_CDMA_CARRIER_MSG = 196,  //GUI 配置CDMA多载波参数
            QUERY_FAP_PARAM_MSG = 197,  //GUI 查询FAP运行参数
            CONFIG_IMSI_MSG_V3_ID = 245,  //大数量imsi名单，用于配置不同的目标IMSI不同的行为
        };
        /// <summary>
        /// 设备发送给控制端的消息类型
        /// </summary>
        private enum Recv_Msg_Id
        {
            QUERY_NB_CELL_INFO_MSG = 202,  //	GUI查询邻区信息
            FAP_NB_CELL_INFO_MSG = 203,  //FAP上报邻区信息
            CONFIG_FAP_MSG = 204,  //GUI配置FAP的启动参数
            CONTROL_FAP_REBOOT_MSG = 205,  //GUI控制FAP的重启。
            CONTROL_FAP_RF_MSG = 206,  //控制FAP射频的消息
            FAP_HEARTBEAT_MSG = 208,  //FAP心跳消息，FAP启动成功后，每10秒发送一次该消息给GUI。心跳消息包含基站的一些状态信息，比如工作模式、同步状态等
            FAP_TRACE_MSG = 210,  //FAP上报一些事件和状态给GUI，GUI程序需要显示给操作者看。
            UE_STATUS_REPORT_MSG = 212,  //FAP上报UE相关状态. 
            UE_ORM_REPORT_MSG = 219,  //FAP上报UE主叫信息，只用于GSM和CDMA
            CONFIG_SMS_CONTENT_MSG_ID = 192,  //FAP 配置下发短信号码和内容
            FAP_PARAM_REPORT_MSG = 198,  //FAP上报FAP运行参数. 
            CONTROL_FAP_RADIO_ON_MSG = 193,  //	GUI 控制FAP开启射频
            CONTROL_FAP_RADIO_OFF_MSG = 194,  //GUI 控制FAP关闭射频
            CONTROL_FAP_RESET_MSG = 195,  //GUI 控制FAP的软复位
            CONFIG_CDMA_CARRIER_MSG = 196,  //GUI 配置CDMA多载波参数
            CONFIG_IMSI_MSG_V3_ID = 245,  //大数量imsi名单，用于配置不同的目标IMSI不同的行为
        };

        /// <summary>
        /// 发送给设备的消息结构
        /// </summary>
        private struct MsgSendStruct
        {
            public Protocol_Sap bProtocolSap;            //头部标识 0xAAAA
            public Send_Msg_Id bMsgId; //消息Id
            public Msg_Type bMsgType; //消息类型
            public Device_Sys bCellIdx; //系统号，0表示系统1或通道1或射频1，1表示系统2或通道2或射频2
            public UInt16 wMsgLen;      //消息数据长度   [数据长度是消息ID和消息数据长度的总和]
            public UInt16 wSqn;     //消息的序列号，每发送完一个消息FAP将此序列号加1，GUI可以用该字段简单查看FAP到GUI有没有丢包发生。
            public UInt32 dwTimeStamp;	        //消息时间
            public string data;

            public MsgSendStruct(Send_Msg_Id MsgId, Device_Sys sys, string data)
            {
                this.bProtocolSap = Protocol_Sap.CDMA;
                this.bMsgId = MsgId;
                this.bMsgType = Msg_Type.INITIAL_MSG;
                this.bCellIdx = sys;
                this.wMsgLen = (UInt16)(MsgHadeLen + (data.Replace(" ", "").Length / 2));
                this.wSqn = ApMsgIdClass.addNormalMsgId();
                this.dwTimeStamp = (UInt32)DateTime.Now.Subtract(DateTime.Parse("1970-1-1")).TotalSeconds;
                this.data = data;
            }
            public MsgSendStruct(Send_Msg_Id MsgId, Device_Sys sys, UInt16 sqn, string data)
            {
                this.bProtocolSap = Protocol_Sap.CDMA;
                this.bMsgId = MsgId;
                this.bMsgType = Msg_Type.INITIAL_MSG;
                this.bCellIdx = sys;
                this.wMsgLen = (UInt16)(12 + (data.Replace(" ", "").Length / 2));
                this.wSqn = sqn;
                this.dwTimeStamp = (UInt32)DateTime.Now.Subtract(DateTime.Parse("1970-1-1")).TotalSeconds;
                this.data = data;
            }
        }

        private struct MsgRecvStruct
        {
            public Protocol_Sap bProtocolSap;            //头部标识 0xAAAA
            public Recv_Msg_Id bMsgId; //消息Id
            public Msg_Type bMsgType; //消息类型
            public Device_Sys bCellIdx; //系统号，0表示系统1或通道1或射频1，1表示系统2或通道2或射频2
            public UInt16 wMsgLen;      //消息数据长度   [数据长度是消息ID和消息数据长度的总和]
            public UInt16 wSqn;     //消息的序列号，每发送完一个消息FAP将此序列号加1，GUI可以用该字段简单查看FAP到GUI有没有丢包发生。
            public UInt32 dwTimeStamp;	        //消息时间
            public string data;
        }

        private struct STRUCT_CONFIG_FAP_MSG
        {
            public byte bWorkingMode;         //工作模式:1 为侦码模式 ;3驻留模式.
            public byte bC;                   //是否自动切换模式。保留
            public UInt16 wRedirectCellUarfcn;  //CDMA黑名单频点
            public UInt32 dwDateTime;           //当前时间	
            public string bPLMNId;              //PLMN标志
            public byte bTxPower;             //实际发射功率.设置发射功率衰减寄存器, 0输出最大功率, 每增加1, 衰减1DB
            public byte bRxGain;              //接收信号衰减寄存器. 每增加1增加1DB的增益
            public UInt16 wPhyCellId;           //物理小区ID.
            public UInt16 wLAC;                 //追踪区域码。GSM：LAC;CDMA：REG_ZONE
            public UInt16 wUARFCN;              //小区频点. CDMA 制式为BSID
            public UInt32 dwCellId;             //小区ID。注意在CDMA制式没有小区ID，高位WORD 是SID ， 低位WORD 是NID
        }

        private struct STRUCT_CONFIG_CDMA_CARRIER_MSG
        {
            public UInt16 wARFCN1;          // 工作频点
            public Byte bARFCN1Mode;      //工作频点模式。0表示扫描，1表示常开,2表示关闭。
            public UInt16 wARFCN1Duration;  //工作频点扫描时长
            public UInt16 wARFCN1Period;    //工作频点扫描间隔
            public UInt16 wARFCN2;          // 工作频点
            public Byte bARFCN2Mode;        //工作频点模式。0表示扫描，1表示常开,2表示关闭。
            public UInt16 wARFCN2Duration;  //工作频点扫描时长
            public UInt16 wARFCN2Period;    //工作频点扫描间隔
            public UInt16 wARFCN3;          // 工作频点
            public Byte bARFCN3Mode;        //工作频点模式。0表示扫描，1表示常开,2表示关闭。
            public UInt16 wARFCN3Duration;  //工作频点扫描时长
            public UInt16 wARFCN3Period;    //工作频点扫描间隔
            public UInt16 wARFCN4;          // 工作频点
            public Byte bARFCN4Mode;        //工作频点模式。0表示扫描，1表示常开,2表示关闭。
            public UInt16 wARFCN4Duration;  //工作频点扫描时长
            public UInt16 wARFCN4Period;    //工作频点扫描间隔
        }

        private struct STRUCT_CONFIG_IMSI_MSG_V3
        {
            public UInt16 wTotalImsi;
            public Byte bIMSINum;
            public Byte bSegmentType;
            public Byte bSegmentID;
            public Byte bActionType;
            public string[] bIMSI;
            public Byte[] bUeActionFlag;

            public STRUCT_CONFIG_IMSI_MSG_V3(int arrayNum)
            {
                this.wTotalImsi = 0;
                this.bIMSINum = 0;
                this.bSegmentType = 0;
                this.bSegmentID = 0;
                this.bActionType = 0;
                this.bIMSI = new string[arrayNum];
                this.bUeActionFlag = new byte[arrayNum];
            }
        }

        private string GetValueByString_String(int len, ref string data)
        {
            if (data.Length < len)
            {
                data = String.Empty;
                return String.Empty;
            }
            string value = data.Substring(0, len);
            data = data.Substring(len);
            return value;
        }
        private Byte GetValueByString_Byte(ref string data)
        {
            int len = 2;
            if (data.Length < len)
            {
                data = String.Empty;
                return 0;
            }
            Byte value = Convert.ToByte(data.Substring(0, len), 16);
            data = data.Substring(len);
            return value;
        }
        private SByte GetValueByString_SByte(ref string data)
        {
            int len = 2;
            if (data.Length < len)
            {
                data = String.Empty;
                return 0;
            }
            SByte value = Convert.ToSByte(data.Substring(0, len), 16);
            data = data.Substring(len);
            return value;
        }
        private UInt16 GetValueByString_U16(ref string data)
        {
            int len = 4;
            if (data.Length < len)
            {
                data = String.Empty;
                return 0;
            }
            UInt16 value = Convert.ToUInt16(data.Substring(0, len), 16);
            data = data.Substring(len);
            return value;
        }
        private UInt32 GetValueByString_U32(ref string data)
        {
            int len = 8;
            if (data.Length < len)
            {
                data = String.Empty;
                return 0;
            }
            UInt32 value = Convert.ToUInt32(data.Substring(0, len), 16);
            data = data.Substring(len);
            return value;
        }
        private void GetValue_Reserved(int len,ref string data)
        {
            len = len * 2;
            if (data.Length < len)
            {
                data = String.Empty;
                return ;
            }
            string value = data.Substring(0, len);
            data = data.Substring(len);
            return;
        }
        #endregion

        #region 类参数定义及构造函数
        public static uint heartbeatMsgNum = 0;
        public static uint imsiMsgNum = 0;

        private string MODE_NAME = ApInnerType.CDMA.ToString();

        private const byte MsgHadeLen = 12;

        public Ap_CDMA_ZYF()
        {
            DeviceType = MODE_NAME;
            ApBase.ReceiveMainData += OnReceiveMainMsg;
        }

        #endregion

        #region AP侧消息处理

        /// <summary>
        /// 重载收到Ap侧消息处理
        /// </summary>
        /// <param name="apToKen">Ap信息</param>
        /// <param name="buff">消息内容</param>
        public override void OnReceiveDeviceMsg(AsyncUserToken apToKen, byte[] buff)
        {
            while (true)
            {
                string msg = GetDeviceMsg_XML(apToKen);
                if (string.IsNullOrEmpty(msg))
                {
                    //OnOutputLog(LogInfoType.DEBG, "消息已解析完。缓存里没有完整消息了！");
                    return;
                }
                HandleApMsg(apToKen, msg);
            }
        }

        /// <summary>
        /// 解析收到的GSM消息
        /// </summary>
        /// <param name="recv">解析后的消息内容</param>
        /// <param name="msg_data">收到的消息</param>
        /// <returns>解析是否成功</returns>
        private bool DecodeGsmMsg(ref MsgRecvStruct recv, string msg_data)
        {
            return DecodeGsmMsg(false, ref recv, msg_data);
        }

        /// <summary>
        /// 解析收到的GSM消息
        /// </summary>
        /// <param name="recvFlag">设备发消息类型</param>
        /// <param name="recv">解析后的消息内容</param>
        /// <param name="msg_data">收到的消息</param>
        /// <returns>解析是否成功</returns>
        private bool DecodeGsmMsg(bool recvFlag, ref MsgRecvStruct recv, string msg_data)
        {
            //string head = msg_data.Substring(0,4);
            byte bProtocolSap = GetValueByString_Byte(ref msg_data);
            if ( bProtocolSap != (byte)Protocol_Sap.CDMA)
            {
                OnOutputLog(LogInfoType.EROR, string.Format("解析CDMA消息格式错误，bProtocolSap为{0},字段错误！",bProtocolSap));
                return false;
            }
            recv.bProtocolSap = (Protocol_Sap)bProtocolSap;

            byte bMsgId = GetValueByString_Byte(ref msg_data);
            if (!Enum.IsDefined(typeof(Recv_Msg_Id),(System.Int32)bMsgId))
            {
                OnOutputLog(LogInfoType.EROR, string.Format("解析CDMA消息格式错误，bMsgId为{0}，不在定义中！",bMsgId));
                return false;
            }
            recv.bMsgId = (Recv_Msg_Id)bMsgId;

            byte bMsgType = GetValueByString_Byte(ref msg_data);
            if (!Enum.IsDefined(typeof(Msg_Type), (System.Int32)bMsgType))
            {
                OnOutputLog(LogInfoType.EROR, string.Format("解析CDMA消息格式错误，bMsgType{0}，不在定义中！", bMsgType));
                return false;
            }
            recv.bMsgType = (Msg_Type)bMsgType;

            byte bCellIdx = GetValueByString_Byte(ref msg_data);
            if (bCellIdx != (byte)Device_Sys.Sys1)
            {
                OnOutputLog(LogInfoType.EROR, string.Format("解析CDMA消息格式错误，bCellIdx为{0},字段错误！", bCellIdx));
                return false;
            }
            recv.bCellIdx = (Device_Sys)bCellIdx;

            recv.wMsgLen = GetValueByString_U16(ref msg_data);
            recv.wMsgLen -= MsgHadeLen;//去掉消息头后的净数据长度
            if (recv.wMsgLen < 0)
            {
                OnOutputLog(LogInfoType.EROR, "解析GSM消息格式错误，wMsgLen字段错误！");
                return false;
            }

            recv.wSqn = GetValueByString_U16(ref msg_data);
            if (recv.wSqn < 0)
            {
                OnOutputLog(LogInfoType.EROR, "解析GSM消息格式错误，wSqn字段错误！");
                return false;
            }

            recv.dwTimeStamp = GetValueByString_U32(ref msg_data);

            recv.data = string.Empty;
            if (recv.wMsgLen > 0)
            {
                //data = msg_data.Substring(24, data_length*2);
                recv.data = GetValueByString_String(recv.wMsgLen * 2, ref msg_data);
                if (string.IsNullOrEmpty(recv.data))
                {
                    OnOutputLog(LogInfoType.EROR, "解析GSM消息格式错误，data字段错误！");
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 处理收到的AP消息
        /// </summary>
        /// <param name="apToKen"></param>
        /// <param name="msg"></param>
        private void HandleApMsg(AsyncUserToken apToKen, string msg)
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
                    apToKen.IPAddress.ToString(), apToKen.Port.ToString()));
                return;
            }

            if (msgBody == null)
            {
                OnOutputLog(LogInfoType.EROR, "收到消息格式错误！");
                return;
            }

            //处理透传消息
            if (msgId >= ApMsgIdClass.MIN_TRANSPARENT_MSG_ID && msgId <= ApMsgIdClass.MAX_TRANSPARENT_MSG_ID)
            {
                if (msgBody.type != ApMsgType.agent_transmit_ack_msg) //参数上报消息不透传
                {
                    Msg_Body_Struct body = new Msg_Body_Struct(Main2ApControllerMsgType.transparent_msg_response, "transparent_msg", msg);
                    OnSendMsg2Main(msgId, MsgStruct.MsgType.TRANSPARENT, apToKen, body);
                    return;
                }
            }

            //心跳消息处理
            if (msgBody.type == ApMsgType.status_response)
            {
                //OnOutputLog(LogInfoType.INFO, "收到心跳消息");
                if (heartbeatMsgNum == System.UInt32.MaxValue)
                    heartbeatMsgNum = 0;
                else
                    heartbeatMsgNum++;

                //UInt32 oldDetail = apToKen.Detail;
                UInt32 detail = 0;
                string sDetail = GetMsgStringValueInList("detail", msgBody);
                if (!string.IsNullOrEmpty(sDetail))
                    detail = Convert.ToUInt32(sDetail, 16);

                //Byte oldApReadySt = apToKen.ApReadySt;
                Byte ApReadySt = 5;
                string sApReadySt = GetMsgStringValueInList("addStatus", msgBody);
                if (!string.IsNullOrEmpty(sApReadySt))
                    ApReadySt = Convert.ToByte(sApReadySt);

                apToKen.version = GetMsgStringValueInList("version", msgBody);
                apToKen.Mode = GetMsgStringValueInList("mode", msgBody);
                apToKen.Sn = GetMsgStringValueInList("sn", msgBody);
                apToKen.FullName = GetMsgStringValueInList("fullname", msgBody);
                apToKen.Id = GetMsgStringValueInList("id", msgBody);
                //apToKen.Detail = detail;

                int i = MyDeviceList.add(apToKen);
                Send2main_OnOffLine(OnLine, i, apToKen);

                //判断是周期心跳，还是上线心跳
                if ((detail & (int)AP_STATUS_LTE.OnLine) > 0) //上线
                {
                    //OnOutputLog(LogInfoType.DEBG, "上线消息");
                    if (OnLine.Equals(MyDeviceList.GetMainControllerStatus(apToKen)))
                    {
                        Send2ap_status_request(apToKen);
                        //Thread.Sleep(1000);
                        //Send2ap_get_general_para_request(apToKen);
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
                Send2ap_ApStatusChange_CDMA_ZYF(apToKen, detail, ApReadySt);
            }
            else if (msgBody.type == ApMsgType.agent_straight_msg)
            {
                MsgRecvStruct recv = new MsgRecvStruct();
                string msg_data = GetMsgStringValueInList("data", msgBody);
                msg_data = msg_data.Replace(" ", "");
                if (string.IsNullOrEmpty(msg_data))
                {
                    OnOutputLog(LogInfoType.EROR, "收到XML消息格式错误，XML中data字段为空！");
                    return;
                }
                if (msg_data.Length < MsgHadeLen)
                {
                    OnOutputLog(LogInfoType.EROR, "收到XML消息格式错误，XML中data字段长度过短！");
                    return;
                }

                if (!DecodeGsmMsg(true, ref recv, msg_data))
                {
                    OnOutputLog(LogInfoType.EROR, "收到XML消息格式错误！");
                    return;
                }

                HandleGsmMsg(apToKen, recv);

            }
            else if (msgBody.type == ApMsgType.agent_transmit_ack_msg)
            {
                MsgRecvStruct recv = new MsgRecvStruct();
                string msg_data = GetMsgStringValueInList("data", msgBody);
                msg_data = msg_data.Replace(" ", "");
                if (string.IsNullOrEmpty(msg_data))
                {
                    OnOutputLog(LogInfoType.EROR, "收到XML消息格式错误，XML中data字段为空！");
                    return;
                }
                if (msg_data.Length < MsgHadeLen)
                {
                    OnOutputLog(LogInfoType.EROR, "收到XML消息格式错误，XML中data字段长度过短！");
                    return;
                }

                if (!DecodeGsmMsg(true, ref recv, msg_data))
                {
                    OnOutputLog(LogInfoType.EROR, "收到XML消息格式错误！");
                    return;
                }

                HandleGsmAckMsg(apToKen, recv);

            }
            else if (msgBody.type == ApMsgType.set_param_response)
            {
                Msg_Body_Struct body = new Msg_Body_Struct(msgBody.type, msgBody.dic);
                OnSendMsg2Main(msgId, MsgStruct.MsgType.CONFIG, apToKen, body);
            }
            else if (msgBody.type == ApMsgType.get_general_para_response)
            {
            //    STRUCT_CONFIG_FAP_MSG recv = new STRUCT_CONFIG_FAP_MSG();

            //    string sys_para_0 = GetMsgStringValueInList("sys_para_0", msgBody);
            //    sys_para_0 = sys_para_0.Replace(" ", "");
            //    if (string.IsNullOrEmpty(sys_para_0))
            //    {
            //        OnOutputLog(LogInfoType.EROR, "收到XML消息格式错误，XML中sys_para_0字段为空！");
            //        return;
            //    }
            //    if (sys_para_0.Length < 24)
            //    {
            //        OnOutputLog(LogInfoType.EROR, "收到XML消息格式错误，XML中sys_para_0字段长度过短！");
            //        return;
            //    }
            //    recv.sys_para_0 = sys_para_0;

            //    string sys_para_1 = GetMsgStringValueInList("sys_para_1", msgBody);
            //    sys_para_1 = sys_para_1.Replace(" ", "");
            //    if (string.IsNullOrEmpty(sys_para_1))
            //    {
            //        OnOutputLog(LogInfoType.EROR, "收到XML消息格式错误，XML中sys_para_1字段为空！");
            //        return;
            //    }
            //    if (sys_para_1.Length < 24)
            //    {
            //        OnOutputLog(LogInfoType.EROR, "收到XML消息格式错误，XML中sys_para_1字段长度过短！");
            //        return;
            //    }
            //    recv.sys_para_1 = sys_para_1;

            //    string sys_option_0 = GetMsgStringValueInList("sys_option_0", msgBody);
            //    sys_option_0 = sys_option_0.Replace(" ", "");
            //    if (string.IsNullOrEmpty(sys_option_0))
            //    {
            //        OnOutputLog(LogInfoType.EROR, "收到XML消息格式错误，XML中sys_option_0字段为空！");
            //        return;
            //    }
            //    if (sys_option_0.Length < 24)
            //    {
            //        OnOutputLog(LogInfoType.EROR, "收到XML消息格式错误，XML中sys_option_0字段长度过短！");
            //        return;
            //    }
            //    recv.sys_option_0 = sys_option_0;

            //    string sys_option_1 = GetMsgStringValueInList("sys_option_1", msgBody);
            //    sys_option_1 = sys_option_1.Replace(" ", "");
            //    if (string.IsNullOrEmpty(sys_option_1))
            //    {
            //        OnOutputLog(LogInfoType.EROR, "收到XML消息格式错误，XML中sys_option_1字段为空！");
            //        return;
            //    }
            //    if (sys_option_1.Length < 24)
            //    {
            //        OnOutputLog(LogInfoType.EROR, "收到XML消息格式错误，XML中sys_option_1字段长度过短！");
            //        return;
            //    }
            //    recv.sys_option_1 = sys_option_1;

            //    string sys_rf_0 = GetMsgStringValueInList("sys_rf_0", msgBody);
            //    sys_rf_0 = sys_rf_0.Replace(" ", "");
            //    if (string.IsNullOrEmpty(sys_rf_0))
            //    {
            //        OnOutputLog(LogInfoType.EROR, "收到XML消息格式错误，XML中sys_rf_0字段为空！");
            //        return;
            //    }
            //    if (sys_rf_0.Length < 24)
            //    {
            //        OnOutputLog(LogInfoType.EROR, "收到XML消息格式错误，XML中sys_rf_0字段长度过短！");
            //        return;
            //    }
            //    recv.sys_rf_0 = sys_rf_0;

            //    string sys_rf_1 = GetMsgStringValueInList("sys_rf_1", msgBody);
            //    sys_rf_1 = sys_rf_1.Replace(" ", "");
            //    if (string.IsNullOrEmpty(sys_rf_1))
            //    {
            //        OnOutputLog(LogInfoType.EROR, "收到XML消息格式错误，XML中ssys_rf_1字段为空！");
            //        return;
            //    }
            //    if (sys_rf_1.Length < 24)
            //    {
            //        OnOutputLog(LogInfoType.EROR, "收到XML消息格式错误，XML中sys_rf_1字段长度过短！");
            //        return;
            //    }
            //    recv.sys_rf_1 = sys_rf_1;

            //    string sys_workMode_0 = GetMsgStringValueInList("sys_workMode_0", msgBody);
            //    sys_workMode_0 = sys_workMode_0.Replace(" ", "");
            //    if (string.IsNullOrEmpty(sys_workMode_0))
            //    {
            //        OnOutputLog(LogInfoType.EROR, "收到XML消息格式错误，XML中sys_workMode_0字段为空！");
            //        return;
            //    }
            //    if (sys_workMode_0.Length < 24)
            //    {
            //        OnOutputLog(LogInfoType.EROR, "收到XML消息格式错误，XML中sys_workMode_0字段长度过短！");
            //        return;
            //    }
            //    recv.sys_workMode_0 = sys_workMode_0;

            //    string sys_workMode_1 = GetMsgStringValueInList("sys_workMode_1", msgBody);
            //    sys_workMode_1 = sys_workMode_1.Replace(" ", "");
            //    if (string.IsNullOrEmpty(sys_workMode_1))
            //    {
            //        OnOutputLog(LogInfoType.EROR, "收到XML消息格式错误，XML中sys_workMode_1字段为空！");
            //        return;
            //    }
            //    if (sys_workMode_1.Length < 24)
            //    {
            //        OnOutputLog(LogInfoType.EROR, "收到XML消息格式错误，XML中sys_workMode_1字段长度过短！");
            //        return;
            //    }
            //    recv.sys_workMode_1 = sys_workMode_1;

            //    //HandleParaRspMsg(apToKen, recv);
            }
            else
            {
                Msg_Body_Struct body = new Msg_Body_Struct(msgBody.type, msgBody.dic);
                OnSendMsg2Main(msgId, MsgStruct.MsgType.CONFIG, apToKen, body);
            }

            msgBody = null;
        }

        private void HandleGsmMsg(AsyncUserToken apToKen, MsgRecvStruct recv)
        {
            String data = recv.data;
            if (recv.bMsgId == Recv_Msg_Id.QUERY_NB_CELL_INFO_MSG ||
                recv.bMsgId == Recv_Msg_Id.CONFIG_FAP_MSG ||
                recv.bMsgId == Recv_Msg_Id.CONTROL_FAP_REBOOT_MSG ||
                recv.bMsgId == Recv_Msg_Id.CONFIG_SMS_CONTENT_MSG_ID ||
                recv.bMsgId == Recv_Msg_Id.CONTROL_FAP_RADIO_ON_MSG ||
                recv.bMsgId == Recv_Msg_Id.CONTROL_FAP_RADIO_OFF_MSG ||
                recv.bMsgId == Recv_Msg_Id.CONTROL_FAP_RESET_MSG ||
                recv.bMsgId == Recv_Msg_Id.CONFIG_CDMA_CARRIER_MSG ||
                recv.bMsgId == Recv_Msg_Id.CONFIG_IMSI_MSG_V3_ID)
            {
                Send2Main_SEND_REQ_CNF(apToKen, recv);
            }
            else if (recv.bMsgId == Recv_Msg_Id.FAP_NB_CELL_INFO_MSG)
            {
                Send2Main_FAP_NB_CELL_INFO_MSG(apToKen, recv);
            }
            else if (recv.bMsgId == Recv_Msg_Id.FAP_TRACE_MSG)
            {
                Send2Main_FAP_TRACE_MSG(apToKen, recv,Main2ApControllerMsgType.gsm_msg_recv);
            }
            else if (recv.bMsgId == Recv_Msg_Id.UE_STATUS_REPORT_MSG)
            {
                Send2Main_UE_STATUS_REPORT_MSG(apToKen, recv, Main2ApControllerMsgType.gsm_msg_recv);
            }
            else if (recv.bMsgId == Recv_Msg_Id.UE_ORM_REPORT_MSG)
            {
                Send2Main_UE_ORM_REPORT_MSG(apToKen, recv, Main2ApControllerMsgType.gsm_msg_recv);
            }
            else if (recv.bMsgId == Recv_Msg_Id.FAP_PARAM_REPORT_MSG)
            {
                Send2Main_FAP_PARAM_REPORT_MSG(apToKen, recv);
            }
            else
            {
                OnOutputLog(LogInfoType.WARN, "HandleGsmMsg收到的Ap消息类型错误！");
            }
        }

        private void HandleGsmAckMsg(AsyncUserToken apToKen, MsgRecvStruct recv)
        {
            String data = recv.data;
            if (recv.bMsgId == Recv_Msg_Id.CONFIG_FAP_MSG)
            {
                Send2Main_CONFIG_FAP_MSG(apToKen, recv);
            }
            else if (recv.bMsgId == Recv_Msg_Id.FAP_TRACE_MSG)
            {
                Send2Main_FAP_TRACE_MSG(apToKen, recv, Main2ApControllerMsgType.ReportGenPara);
            }
            else if (recv.bMsgId == Recv_Msg_Id.UE_STATUS_REPORT_MSG)
            {
                //Send2Main_UE_STATUS_REPORT_MSG(apToKen, recv, Main2ApControllerMsgType.ReportGenPara);
            }
            else if (recv.bMsgId == Recv_Msg_Id.UE_ORM_REPORT_MSG)
            {
                Send2Main_UE_ORM_REPORT_MSG(apToKen, recv, Main2ApControllerMsgType.ReportGenPara);
            }
            else if (recv.bMsgId == Recv_Msg_Id.CONFIG_SMS_CONTENT_MSG_ID)
            {
                Send2Main_CONFIG_SMS_CONTENT_MSG_ID(apToKen, recv);
            }
            else if (recv.bMsgId == Recv_Msg_Id.CONFIG_CDMA_CARRIER_MSG)
            {
                Send2Main_CONFIG_CDMA_CARRIER_MSG(apToKen, recv);
            }
            else if (recv.bMsgId == Recv_Msg_Id.CONFIG_IMSI_MSG_V3_ID)
            {
                Send2Main_CONFIG_IMSI_MSG_V3_ID(apToKen, recv);
            }
            else
            {
                OnOutputLog(LogInfoType.WARN, "HandleGsmAckMsg收到的Ap消息类型错误！");
            }
        }

        #region 封装回复AP的消息

        /// <summary>
        /// 获取保留字段
        /// </summary>
        /// <param name="len">保留字段长度</param>
        /// <returns></returns>
        private string getReservedString(byte len)
        {
            return "0".PadLeft(len * 2, '0');
        }

        private string fullString(byte len,string str)
        {
            if (str.Length >= len)
                return str;
            else
                return str + "0".PadLeft(len - str.Length, '0');
        }

        private string StringAddZero(string str)
        {
            string data = string.Empty;
            for (int i=0;i<str.Length;i++)
            {
                data = string.Format("{0}{1}",data,str[i].ToString().PadLeft(2,'0'));
            }
            return data;
        }

        private void SendMainMsgParaVlaueError(Ap_Info_Struct ApInfo, App_Info_Struct AppInfo, string type, string name)
        {
            string str = string.Format("发送给GSM设备消息({0})错误。消息中参数项({1})缺失或值错误。", type.ToString(), name);
            OnOutputLog(LogInfoType.WARN, str);
            Send2APP_GeneralError(ApInfo, AppInfo, type, str);
            return;
        }

        /// <summary>
        /// 向Ap发送Main模块过来的消息
        /// </summary>
        /// <param name="MainMsg"></param>
        private void Send2ap_RecvMainMsg(InterModuleMsgStruct MainMsg)
        {
            AsyncUserToken apToKen = MyDeviceList.FindByApInfo(MainMsg.ApInfo);
            if (apToKen == null)
            {
                OnOutputLog(LogInfoType.WARN, string.Format("在线AP列表中找不到Ap[{0}:{1}]设备({2})!",
                    MainMsg.ApInfo.IP, MainMsg.ApInfo.Port.ToString(), MainMsg.ApInfo.Fullname));
                return;
            }
            MsgId2App msgId2App = new MsgId2App();
            msgId2App.id = ApMsgIdClass.addNormalMsgId();
            msgId2App.AppInfo = MainMsg.AppInfo;

            if (MyDeviceList.AddMsgId2App(apToKen, msgId2App))
            {
                byte[] sendMsg = EncodeApXmlMessage(msgId2App.id, MainMsg.Body);
                if (sendMsg == null)
                {
                    OnOutputLog(LogInfoType.EROR, string.Format("封装XML消息(RecvMainMsg)出错！"));
                    Send2APP_GeneralError(MainMsg.ApInfo, MainMsg.AppInfo, MainMsg.Body.type,
                        string.Format("封装向AP发送的XML消息出错！"));
                    return;
                }
                SendMsg2Ap(apToKen, sendMsg);
            }
        }

        /// <summary>
        /// 透传MainController模块过来的消息给设备
        /// </summary>
        /// <param name="MainMsg"></param>
        private void Send2ap_TransparentMsg(InterModuleMsgStruct MainMsg)
        {
            AsyncUserToken apToKen = MyDeviceList.FindByApInfo(MainMsg.ApInfo);
            if (apToKen == null)
            {
                OnOutputLog(LogInfoType.WARN, string.Format("在线AP列表中找不到Ap[{0}:{1}]设备({2})!",
                    MainMsg.ApInfo.IP, MainMsg.ApInfo.Port.ToString(), MainMsg.ApInfo.Fullname));
                return;
            }

            UInt16 msgId = ApMsgIdClass.addTransparentMsgId();

            MsgId2App msgId2App = new MsgId2App();
            msgId2App.id = msgId;
            msgId2App.AppInfo = MainMsg.AppInfo;

            if (!MyDeviceList.AddMsgId2App(apToKen, msgId2App))
            {
                OnOutputLog(LogInfoType.EROR, string.Format("添加消息Id到设备列表出错！"));
                Send2APP_GeneralError(MainMsg.ApInfo, MainMsg.AppInfo, MainMsg.Body.type,
                    string.Format("添加消息Id到设备列表出错！"));
                return;
            }

            string sendMsg = GetMsgStringValueInList("transparent_msg", MainMsg.Body);
            if (string.IsNullOrEmpty(sendMsg))
            {
                OnOutputLog(LogInfoType.EROR, string.Format("封装XML消息(Send2ap_TransparentMsg)出错！"));
                Send2APP_GeneralError(MainMsg.ApInfo, MainMsg.AppInfo, MainMsg.Body.type,
                    string.Format("封装向AP发送的XML消息出错！"));
                return;
            }
            sendMsg = sendMsg.Replace(" ", "");
            sendMsg = sendMsg.Remove(12, 4);
            sendMsg = sendMsg.Insert(12, string.Format("{0}", msgId.ToString("X").PadLeft(4, '0')));
            sendMsg = Regex.Replace(sendMsg, @".{2}", "$0 ");

            Msg_Body_Struct TypeKeyValue =
                new Msg_Body_Struct(ApMsgType.agent_straight_msg,
                "data", sendMsg.Trim());

            byte[] bMsg = EncodeApXmlMessage(msgId2App.id, TypeKeyValue);
            if (bMsg == null)
            {
                OnOutputLog(LogInfoType.EROR, string.Format("封装XML消息(Send2ap_TransparentMsg)出错！"));
                return;
            }
            SendMsg2Ap(apToKen, bMsg);
        }

        /// <summary>
        /// 向AP发送消息
        /// </summary>
        /// <param name="apToKen">AP信息</param
        /// <param name="msg">GSM文档格式的消息内容，十六进制“AA AA 01 00 15 0E 55 66 00 4B 00 54 02 2A 00 00 00 00 00 00”</param>
        private void Send2ap_CDMA(AsyncUserToken apToKen, App_Info_Struct AppInfo, MsgSendStruct sendMsg)
        {
            string data = string.Format("{0}{1}{2}{3}{4}{5}{6}{7}",
                ((byte)sendMsg.bProtocolSap).ToString("X").PadLeft(2, '0'),
                ((byte)sendMsg.bMsgId).ToString("X").PadLeft(2, '0'),
                ((byte)sendMsg.bMsgType).ToString("X").PadLeft(2, '0'),
                ((byte)sendMsg.bCellIdx).ToString("X").PadLeft(2, '0'),
                ((UInt16)sendMsg.wMsgLen).ToString("X").PadLeft(4, '0'),
                ((UInt16)sendMsg.wSqn).ToString("X").PadLeft(4, '0'),
                ((UInt32)sendMsg.dwTimeStamp).ToString("X").PadLeft(8, '0'),
                sendMsg.data);

            //在两个字符间加上空格
            string sendData = Regex.Replace(data, @".{2}", "$0 ");

            MsgId2App msgId2App = new MsgId2App();
            msgId2App.id = sendMsg.wSqn;
            msgId2App.AppInfo = AppInfo;

            if (!MyDeviceList.AddMsgId2App(apToKen, msgId2App))
            {
                OnOutputLog(LogInfoType.EROR, string.Format("添加消息Id到设备列表出错！"));
                Send2APP_GeneralError(apToKen, AppInfo, sendMsg.bMsgId.ToString(),
                    string.Format("添加消息Id到设备列表出错！"));
                return;
            }

            Msg_Body_Struct TypeKeyValue =
                new Msg_Body_Struct(ApMsgType.agent_straight_msg,
                "data", sendData.Trim());

            byte[] bMsg = EncodeApXmlMessage(sendMsg.wSqn, TypeKeyValue);
            if (bMsg == null)
            {
                OnOutputLog(LogInfoType.EROR, string.Format("封装XML消息(Send2ap_CDMA)出错！"));
                return;
            }
            SendMsg2Ap(apToKen, bMsg);
        }

        private void Send2ap_QUERY_NB_CELL_INFO_MSG(AsyncUserToken apToKen, App_Info_Struct AppInfo , Device_Sys sys)
        {
            Send2ap_CDMA(apToKen, AppInfo, new MsgSendStruct(Send_Msg_Id.QUERY_NB_CELL_INFO_MSG,sys, getReservedString(32)));
        }

        private void Send2ap_CONFIG_FAP_MSG(AsyncUserToken apToKen, App_Info_Struct AppInfo, Device_Sys sys, STRUCT_CONFIG_FAP_MSG para)
        {
            string paraMnc = string.Empty;

            //string plmn = string.Empty;
            //for (int i = 0; i < 5; i++)
            //{
            //    plmn = string.Format("{0}{1}",plmn, GetValueByString_String(1,ref para.bPLMNId).PadLeft(2, '0'));
            //}

            string data = string.Format("{0}{1}{2}{3}{4}{5}{6}{7}{8}{9}{10}{11}{12}{13}{14}",
                para.bWorkingMode.ToString("X").PadLeft(2, '0'),
                para.bC.ToString("X").PadLeft(2, '0'),
                para.wRedirectCellUarfcn.ToString("X").PadLeft(4, '0'),
                getReservedString(4),
                para.dwDateTime.ToString("X").PadLeft(8, '0'),
                CodeConver.String2HexString(para.bPLMNId).PadLeft(10, '0'),
                para.bTxPower.ToString("X").PadLeft(2, '0'),
                getReservedString(1),
                para.bRxGain.ToString("X").PadLeft(2, '0'),
                para.wPhyCellId.ToString("X").PadLeft(4, '0'),
                para.wLAC.ToString("X").PadLeft(4, '0'),
                para.wUARFCN.ToString("X").PadLeft(4, '0'),
                getReservedString(2),
                para.dwCellId.ToString("X").PadLeft(8, '0'),
                getReservedString(16));
            Send2ap_CDMA(apToKen, AppInfo, new MsgSendStruct(Send_Msg_Id.CONFIG_FAP_MSG,sys, data));
        }

        private void Send2ap_CONFIG_SMS_CONTENT_MSG(AsyncUserToken apToKen, App_Info_Struct AppInfo, Device_Sys sys, string num,string text)
        {
            int len = num.Length;
            string phoneNum  = CodeConver.String2HexString(num);
            string phoneText = CodeConver.String2Unicode(text,false);
            if(phoneText.Length >40 || phoneText.Length<=0)
            {
                Send2APP_GeneralError(apToKen,AppInfo,Send_Msg_Id.CONFIG_SMS_CONTENT_MSG.ToString(),
                    "编码后的消息内容长度错误!");
                return;
            }

            string data = string.Format("{0}{1}{2}{3}",
                 len.ToString("X").PadLeft(2, '0'),
                 fullString(36,phoneNum),
                 text.Length.ToString("X").PadLeft(2, '0'),
                 phoneText);
            Send2ap_CDMA(apToKen, AppInfo, new MsgSendStruct(Send_Msg_Id.CONFIG_SMS_CONTENT_MSG, sys, data));
        }

        private void Send2ap_CONTROL_FAP_REBOOT_MSG(AsyncUserToken apToKen, App_Info_Struct AppInfo, Device_Sys sys,byte flag)
        {
            string data = string.Format("{0}{1}",
                 flag.ToString("X").PadLeft(2, '0'),
                 getReservedString(3));
            Send2ap_CDMA(apToKen, AppInfo, new MsgSendStruct(Send_Msg_Id.CONTROL_FAP_REBOOT_MSG, sys,data));
        }

        private void Send2ap_CONTROL_FAP_RADIO_ON_MSG(AsyncUserToken apToKen, App_Info_Struct AppInfo, Device_Sys sys)
        {
            Send2ap_CDMA(apToKen, AppInfo, new MsgSendStruct(Send_Msg_Id.CONTROL_FAP_RADIO_ON_MSG, sys, string.Empty));
        }

        private void Send2ap_CONTROL_FAP_RADIO_OFF_MSG(AsyncUserToken apToKen, App_Info_Struct AppInfo, Device_Sys sys)
        {
            Send2ap_CDMA(apToKen, AppInfo, new MsgSendStruct(Send_Msg_Id.CONTROL_FAP_RADIO_OFF_MSG, sys, string.Empty));
        }

        private void Send2ap_CONTROL_FAP_RESET_MSG(AsyncUserToken apToKen, App_Info_Struct AppInfo, Device_Sys sys)
        {
            Send2ap_CDMA(apToKen, AppInfo, new MsgSendStruct(Send_Msg_Id.CONTROL_FAP_RESET_MSG, sys, string.Empty));
        }

        private void Send2ap_CONFIG_CDMA_CARRIER_MSG(AsyncUserToken apToKen, App_Info_Struct AppInfo, Device_Sys sys, STRUCT_CONFIG_CDMA_CARRIER_MSG para)
        {
            string data = string.Format("{0}{1}{2}{3}{4}{5}{6}{7}{8}{9}{10}{11}{12}{13}{14}{15}{16}{17}{18}{19}",
                para.wARFCN1.ToString("X").PadLeft(4, '0'),
                para.bARFCN1Mode.ToString("X").PadLeft(2, '0'),
                getReservedString(1),
                para.wARFCN1Duration.ToString("X").PadLeft(4, '0'),
                para.wARFCN1Period.ToString("X").PadLeft(4, '0'),
                para.wARFCN2.ToString("X").PadLeft(4, '0'),
                para.bARFCN2Mode.ToString("X").PadLeft(2, '0'),
                getReservedString(1),
                para.wARFCN2Duration.ToString("X").PadLeft(4, '0'),
                para.wARFCN2Period.ToString("X").PadLeft(4, '0'),
                para.wARFCN3.ToString("X").PadLeft(4, '0'),
                para.bARFCN3Mode.ToString("X").PadLeft(2, '0'),
                getReservedString(1),
                para.wARFCN3Duration.ToString("X").PadLeft(4, '0'),
                para.wARFCN3Period.ToString("X").PadLeft(4, '0'),
                para.wARFCN4.ToString("X").PadLeft(4, '0'),
                para.bARFCN4Mode.ToString("X").PadLeft(2, '0'),
                getReservedString(1),
                para.wARFCN4Duration.ToString("X").PadLeft(4, '0'),
                para.wARFCN4Period.ToString("X").PadLeft(4, '0'));
            Send2ap_CDMA(apToKen, AppInfo, new MsgSendStruct(Send_Msg_Id.CONFIG_CDMA_CARRIER_MSG, sys, data));
        }

        private void Send2ap_QUERY_FAP_PARAM_MSG(AsyncUserToken apToKen, App_Info_Struct AppInfo, Device_Sys sys)
        {
            Send2ap_CDMA(apToKen, AppInfo, new MsgSendStruct(Send_Msg_Id.QUERY_FAP_PARAM_MSG, sys, string.Empty));
        }

        private void Send2ap_CONFIG_IMSI_MSG_V3(AsyncUserToken apToKen, App_Info_Struct AppInfo, Device_Sys sys, STRUCT_CONFIG_IMSI_MSG_V3 para)
        {  
            if (para.wTotalImsi <= 50 || para.bActionType == 1)
            {
                string data = string.Empty;
                data = string.Format("{0}{1}{2}{3}{4}{5}",
                   para.wTotalImsi.ToString("X").PadLeft(4, '0'),
                   para.wTotalImsi.ToString("X").PadLeft(2, '0'),
                   4.ToString("X").PadLeft(2, '0'), //Complete
                   0.ToString("X").PadLeft(2, '0'),
                   para.bActionType.ToString("X").PadLeft(2, '0'),
                   getReservedString(2));

                for (int i = 0; i < para.wTotalImsi; i++)
                {
                    data = data + StringAddZero(para.bIMSI[i]);
                }
                for (int i = para.wTotalImsi; i < 50; i++)
                {
                    data = data + getReservedString(15);
                }
                for (int i = 0; i < para.wTotalImsi; i++)
                {
                    data = data + para.bUeActionFlag[i].ToString("X").PadLeft(2, '0');
                }
                for (int i = para.wTotalImsi; i < 50; i++)
                {
                    data = data + getReservedString(1);
                }

                Send2ap_CDMA(apToKen, AppInfo, new MsgSendStruct(Send_Msg_Id.CONFIG_IMSI_MSG_V3_ID, sys, data));
            }
            else
            {
                string data = string.Empty;
                byte bSegmentType = 0;
                byte bSegmentID = 0;
                for (int j = 0; j < para.wTotalImsi; j += 50)
                {
                    Thread.Sleep(300);

                    int lastId = j + 50;
                    if (j == 0)
                        bSegmentType = 1;
                    else if (lastId >= para.wTotalImsi)
                        bSegmentType = 1;
                    else
                        bSegmentType = 3;

                    data = string.Format("{0}{1}{2}{3}{4}{5}",
                       para.wTotalImsi.ToString("X").PadLeft(4, '0'),
                       para.wTotalImsi.ToString("X").PadLeft(2, '0'),
                       bSegmentType.ToString("X").PadLeft(2, '0'), //Complete
                       bSegmentID.ToString("X").PadLeft(2, '0'),
                       para.bActionType.ToString("X").PadLeft(2, '0'),
                       getReservedString(2));

                    bSegmentID++;

                    if (lastId < para.wTotalImsi)
                    {
                        for (int i = j; i < lastId; i++)
                        {
                            data = data + StringAddZero(para.bIMSI[i]);
                        }
                        for (int i = j; i < lastId; i++)
                        {
                            data = data + para.bUeActionFlag[i].ToString("X").PadLeft(2, '0');
                        }
                    }
                    else
                    {
                        for (int i = j; i < para.wTotalImsi; i++)
                        {
                            data = data + StringAddZero(para.bIMSI[i]);
                        }
                        for (int i = para.wTotalImsi; i < lastId; i++)
                        {
                            data = data + getReservedString(15);
                        }
                        for (int i = j; i < para.wTotalImsi; i++)
                        {
                            data = data + para.bUeActionFlag[i].ToString("X").PadLeft(2, '0');
                        }
                        for (int i = para.wTotalImsi; i < lastId; i++)
                        {
                            data = data + getReservedString(1);
                        }
                    }

                    Send2ap_CDMA(apToKen, AppInfo, new MsgSendStruct(Send_Msg_Id.CONFIG_IMSI_MSG_V3_ID, sys, data));
                }
            }
        }

        //private void Send2ap_SET_PARA_REQ(AsyncUserToken apToKen, App_Info_Struct AppInfo, Gsm_Device_Sys sys, int Flag,
        //    RecvSysPara para, RecvSysOption option, RecvRfOption rf, byte mode)
        //{
        //    UInt16 msgId = addMsgId();

        //    Msg_Body_Struct TypeKeyValue = new Msg_Body_Struct(ApMsgType.set_general_para_request);

        //    if ((Flag & 0x01) > 0)
        //    {
        //        string paraMnc = string.Empty;
        //        if (para.paraMnc < 0xFF)
        //        {
        //            paraMnc = string.Format("0F{0}", para.paraMnc.ToString("X").PadLeft(2, '0'));
        //        }
        //        else
        //        {
        //            paraMnc = string.Format("{0}", para.paraMnc.ToString("X").PadLeft(4, '0'));
        //        }

        //        string SysParaData = string.Format("{0}{1}{2}{3}{4}{5}{6}{7}{8}{9}",
        //            para.paraMcc.ToString("X").PadLeft(4, '0'),
        //            paraMnc,
        //            para.paraBsic.ToString("X").PadLeft(2, '0'),
        //            para.paraLac.ToString("X").PadLeft(4, '0'),
        //            para.paraCellId.ToString("X").PadLeft(4, '0'),
        //            para.paraC2.ToString("X").PadLeft(2, '0'),
        //            para.paraPeri.ToString("X").PadLeft(2, '0'),
        //            para.paraAccPwr.ToString("X").PadLeft(2, '0'),
        //            para.paraMsPwr.ToString("X").PadLeft(2, '0'),
        //            para.paraRejCau.ToString("X").PadLeft(2, '0'));

        //        gsm_msg_send SysParaSendMsg = new gsm_msg_send(Send_Msg_Id.RECV_SYS_PARA, sys, SysParaData);

        //        string SysPara = string.Format("{0}{1}{2}{3}{4}{5}{6}",
        //            SysParaSendMsg.head, SysParaSendMsg.addr, ((byte)SysParaSendMsg.sys).ToString("X").PadLeft(2, '0'),
        //            ((byte)SysParaSendMsg.type).ToString("X").PadLeft(2, '0'), SysParaSendMsg.data_length.ToString("X").PadLeft(2, '0'),
        //            SysParaSendMsg.message_id.ToString("X").PadLeft(4, '0'), SysParaSendMsg.data);

        //        //在两个字符间加上空格
        //        SysPara = Regex.Replace(SysPara, @".{2}", "$0 ");

        //        TypeKeyValue.dic.Add(string.Format("sys_para_{0}", sys), SysPara.Trim());
        //    }

        //    if ((Flag & 0x02) > 0)
        //    {
        //        string optionData = string.Format("{0}{1}{2}{3}{4}{5}",
        //        option.opLuSms.ToString("X").PadLeft(2, '0'),
        //        option.opLuImei.ToString("X").PadLeft(2, '0'),
        //        option.opCallEn.ToString("X").PadLeft(2, '0'),
        //        option.opDebug.ToString("X").PadLeft(2, '0'),
        //        option.opLuType.ToString("X").PadLeft(2, '0'),
        //        option.opSmsType.ToString("X").PadLeft(2, '0'));

        //        gsm_msg_send OptionSendMsg = new gsm_msg_send(Send_Msg_Id.RECV_SYS_OPTION, sys, optionData);

        //        string SysOption = string.Format("{0}{1}{2}{3}{4}{5}{6}",
        //            OptionSendMsg.head, OptionSendMsg.addr, ((byte)OptionSendMsg.sys).ToString("X").PadLeft(2, '0'),
        //            ((byte)OptionSendMsg.type).ToString("X").PadLeft(2, '0'), OptionSendMsg.data_length.ToString("X").PadLeft(2, '0'),
        //            OptionSendMsg.message_id.ToString("X").PadLeft(4, '0'), OptionSendMsg.data);

        //        //在两个字符间加上空格
        //        SysOption = Regex.Replace(SysOption, @".{2}", "$0 ");

        //        TypeKeyValue.dic.Add(string.Format("sys_option_{0}", sys), SysOption.Trim());
        //    }

        //    if ((Flag & 0x04) > 0)
        //    {
        //        string rfData = string.Format("{0}{1}{2}",
        //       rf.rfEnable.ToString("X").PadLeft(2, '0'),
        //       rf.rfFreq.ToString("X").PadLeft(4, '0'),
        //       rf.rfPwr.ToString("X").PadLeft(2, '0'));

        //        gsm_msg_send RfSendMsg = new gsm_msg_send(Send_Msg_Id.RECV_RF_PARA, sys, rfData);

        //        string SysRf = string.Format("{0}{1}{2}{3}{4}{5}{6}",
        //            RfSendMsg.head, RfSendMsg.addr, ((byte)RfSendMsg.sys).ToString("X").PadLeft(2, '0'),
        //            ((byte)RfSendMsg.type).ToString("X").PadLeft(2, '0'), RfSendMsg.data_length.ToString("X").PadLeft(2, '0'),
        //            RfSendMsg.message_id.ToString("X").PadLeft(4, '0'), RfSendMsg.data);

        //        //在两个字符间加上空格
        //        SysRf = Regex.Replace(SysRf, @".{2}", "$0 ");

        //        TypeKeyValue.dic.Add(string.Format("sys_rf_{0}", sys), SysRf.Trim());
        //    }

        //    if ((Flag & 0x08) > 0)
        //    {
        //        string ModeData = string.Format("{0}",
        //        mode.ToString("X").PadLeft(2, '0'));

        //        gsm_msg_send ModeSendMsg = new gsm_msg_send(Send_Msg_Id.RECV_REG_MODE, sys, ModeData);

        //        string SysMode = string.Format("{0}{1}{2}{3}{4}{5}{6}",
        //            ModeSendMsg.head, ModeSendMsg.addr, ((byte)ModeSendMsg.sys).ToString("X").PadLeft(2, '0'),
        //            ((byte)ModeSendMsg.type).ToString("X").PadLeft(2, '0'), ModeSendMsg.data_length.ToString("X").PadLeft(2, '0'),
        //            ModeSendMsg.message_id.ToString("X").PadLeft(4, '0'), ModeSendMsg.data);

        //        //在两个字符间加上空格   
        //        SysMode = Regex.Replace(SysMode, @".{2}", "$0 ");

        //        TypeKeyValue.dic.Add(string.Format("sys_workMode_{0}", sys), SysMode.Trim());
        //    }


        //    MsgId2App msgId2App = new MsgId2App();
        //    msgId2App.id = msgId;
        //    msgId2App.AppInfo = AppInfo;

        //    if (!MyDeviceList.AddMsgId2App(apToKen, msgId2App))
        //    {
        //        OnOutputLog(LogInfoType.EROR, string.Format("添加消息Id到设备列表出错！"));
        //        Send2APP_GeneralError(apToKen, AppInfo, ApMsgType.set_general_para_request.ToString(),
        //            string.Format("添加消息Id到设备列表出错！"));
        //        return;
        //    }

        //    byte[] bMsg = EncodeApXmlMessage(msgId, TypeKeyValue);
        //    if (bMsg == null)
        //    {
        //        OnOutputLog(LogInfoType.EROR, string.Format("封装XML消息(Send2ap_SET_PARA_REQ)出错！"));
        //        return;
        //    }
        //    SendMsg2Ap(apToKen, bMsg);
        //}

        #endregion

        #endregion

        #region MainController侧消息处理

        /// <summary>
        /// 重载收到Main模块消息处理
        /// </summary>
        /// <param name="msg">消息内容</param>
        public void OnReceiveMainMsg(MsgStruct.InterModuleMsgStruct MainMsg)
        {
            if (MainMsg == null || MainMsg.Body == null)
            {
                OnOutputLog(LogInfoType.EROR, "收到MainController模块消息内容为空!", LogCategory.R);
                return;
            }

            if (MainMsg.ApInfo.IP == null || MainMsg.ApInfo.Type == null)
            {
                OnOutputLog(LogInfoType.EROR, "收到MainController模块消息ApInfo内容错误!", LogCategory.R);
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
                    if (status.Equals(OnLine) || status.Equals(OffLine))
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
            else if (MainMsg.Body.type == Main2ApControllerMsgType.ApDelete)
            {
                //修改状态为下线状态
                MyDeviceList.SetMainControllerStatus(OffLine, MainMsg.ApInfo.IP, MainMsg.ApInfo.Port);
            }
            else if (MainMsg.Body.type == Main2ApControllerMsgType.ApSetRadio)
            {
                AsyncUserToken apToKen = MyDeviceList.FindByApInfo(MainMsg.ApInfo);
                if (apToKen == null)
                {
                    OnOutputLog(LogInfoType.WARN, string.Format("在线AP列表中找不到Ap[{0}:{1}]设备({2})!",
                        MainMsg.ApInfo.IP, MainMsg.ApInfo.Port.ToString(),MainMsg.ApInfo.Fullname));
                    return;
                }

                byte carry = GetMsgByteValueInList("carry", MainMsg.Body.dic, Byte.MaxValue);
                if (carry != 0)
                {
                    OnOutputLog(LogInfoType.EROR, string.Format("Main模块发送消息[{0}]中，carry字段非法!",
                        Main2ApControllerMsgType.ApSetRadio));
                    return;
                }

                Byte RADIO = GetMsgByteValueInList("RADIO", MainMsg.Body.dic, Byte.MaxValue);
                if (RADIO == 1)
                {
                    Send2ap_CONTROL_FAP_RADIO_ON_MSG(apToKen, MainMsg.AppInfo, (Device_Sys)carry);
                }
                else if (RADIO == 0)
                {
                    Send2ap_CONTROL_FAP_RADIO_OFF_MSG(apToKen, MainMsg.AppInfo, (Device_Sys)carry);
                }
                else
                {
                    OnOutputLog(LogInfoType.EROR, string.Format("Main模块发送消息[{0}]中，RADIO字段非法!",
                        Main2ApControllerMsgType.ApSetRadio));
                    return;
                }

            }
            //状态改变回复
            else if (MainMsg.Body.type == Main2ApControllerMsgType.ApStatusChange_Ack)
            {
                RecvAckSaveApStatus(MainMsg);
            }
            else if (MainMsg.Body.type == Main2ApControllerMsgType.OnOffLineCheck)
            {
                string status = GetMsgStringValueInList("Status", MainMsg.Body);
                Send2main_OnOffLineCheck(status, MainMsg.ApInfo);
            }
            else if (MainMsg.Body.type == Main2ApControllerMsgType.ReportGenParaAck)
            {
                if (GetMsgIntValueInList("ReturnCode", MainMsg.Body) != 0)
                {
                    OnOutputLog(LogInfoType.EROR,
                        "[ReportGenParaAck]Main模块返回错误:" + GetMsgStringValueInList("ReturnStr", MainMsg.Body));
                }
                return;
            }
            else if (MainMsg.Body.type == Main2ApControllerMsgType.gsm_msg_send)
            {
                //string Protocol = GetMsgStringValueInList("Protocol", MainMsg.Body.dic, string.Empty);
                //if (!Protocol.Equals(Protocol_Sap.CDMA.ToString()))
                //{
                //    OnOutputLog(LogInfoType.EROR, "发送给CDMA设备消息错误。消息中协议类型不为" + Protocol_Sap.CDMA + ".");
                //    Send2APP_GeneralError(MainMsg.ApInfo, MainMsg.AppInfo, MainMsg.Body.type,
                //       string.Format("发送给CDMA设备消息错误。消息中系统号不为" + Protocol_Sap.CDMA + "."));
                //    return;
                //}

                byte sys = GetMsgByteValueInList("sys", MainMsg.Body.dic, Byte.MaxValue);
                if (sys != (byte)Device_Sys.Sys1)
                {
                    OnOutputLog(LogInfoType.EROR, "发送给CDMA设备消息错误。消息中系统号不为0。");
                    Send2APP_GeneralError(MainMsg.ApInfo, MainMsg.AppInfo, MainMsg.Body.type,
                       string.Format("发送给CDMA设备消息错误。消息中系统号不为0。"));
                    return;
                }
                if (MainMsg.Body.n_dic == null)
                {
                    OnOutputLog(LogInfoType.EROR, "发送给CDMA设备消息错误。消息中没有可设置的参数(n_dic项为NULL)。");
                    Send2APP_GeneralError(MainMsg.ApInfo, MainMsg.AppInfo, MainMsg.Body.type,
                        "发送给CDMA设备消息错误。消息中没有可设置的参数(n_dic项为NULL)。");
                    return;
                }

                foreach (Name_DIC_Struct x in MainMsg.Body.n_dic)
                {
                    EncodeMainMsg(MainMsg.ApInfo, MainMsg.AppInfo, (Device_Sys)sys, x);
                }
                return;
            }
            else if (MainMsg.Body.type == Main2ApControllerMsgType.SetGenParaReq)  //数据对齐部分
            {
                byte Protocol = GetMsgByteValueInList("Protocol", MainMsg.Body.dic, Byte.MaxValue);
                if (Protocol != (byte)Protocol_Sap.CDMA)
                {
                    OnOutputLog(LogInfoType.EROR, "发送给CDMA设备消息错误。消息中协议类型不为" + Protocol_Sap.CDMA + ".");
                    Send2APP_GeneralError(MainMsg.ApInfo, MainMsg.AppInfo, MainMsg.Body.type,
                       string.Format("发送给CDMA设备消息错误。消息中系统号不为" + Protocol_Sap.CDMA + "."));
                    return;
                }
                byte sys = GetMsgByteValueInList("sys", MainMsg.Body.dic, Byte.MaxValue);
                if (sys != (byte)Device_Sys.Sys1)
                {
                    OnOutputLog(LogInfoType.EROR, "发送给CDMA设备消息错误。消息中系统号不为0。");
                    Send2APP_GeneralError(MainMsg.ApInfo, MainMsg.AppInfo, MainMsg.Body.type,
                       string.Format("发送给CDMA设备消息错误。消息中系统号不为0。"));
                    return;
                }
                if (MainMsg.Body.n_dic == null)
                {
                    OnOutputLog(LogInfoType.EROR, "发送给CDMA设备消息错误。消息中没有可设置的参数(n_dic项为NULL)。");
                    Send2APP_GeneralError(MainMsg.ApInfo, MainMsg.AppInfo, MainMsg.Body.type,
                        "发送给CDMA设备消息错误。消息中没有可设置的参数(n_dic项为NULL)。");
                    return;
                }

                //EncodeSetParaMsg(MainMsg.ApInfo, MainMsg.AppInfo, (Device_Sys)sys, MainMsg.Body.n_dic);

                return;
            }
            else if (MainMsg.Body.type == Main2ApControllerMsgType.set_parameter_request)
            {
                if ((string.IsNullOrEmpty(MainMsg.ApInfo.IP)) || (MainMsg.ApInfo.IP == MsgStruct.NullDevice))
                {
                    OnOutputLog(LogInfoType.INFO, string.Format("目的设备为Null，不向Ap发送信息！"));
                    Send2APP_GeneralError(MainMsg.ApInfo, MainMsg.AppInfo, MainMsg.Body.type,
                        string.Format("目的设备为Null，不向Ap发送信息！"));
                }
                else
                {
                    Send2ap_RecvMainMsg(MainMsg);
                }
            }
            else //其它消息
            {
                string str = string.Format("发送给CDMA设备({0}:{1})消息类型{2}错误！",
                    MainMsg.ApInfo.IP, MainMsg.ApInfo.Port.ToString(), MainMsg.Body.type.ToString());
                OnOutputLog(LogInfoType.WARN, str);
                Send2APP_GeneralError(MainMsg.ApInfo, MainMsg.AppInfo, Main2ApControllerMsgType.gsm_msg_send, str);
            }
            return;
        }

        /// <summary>
        /// 解析Main模块发过来的消息
        /// </summary>
        /// <param name="msgBody">消息内容</param>
        /// <param name="sys"></param>
        /// <param name="n_dic"></param>
        private void EncodeMainMsg(Ap_Info_Struct ApInfo, App_Info_Struct AppInfo, Device_Sys sys, Name_DIC_Struct n_dic)
        {
            AsyncUserToken apToKen = MyDeviceList.FindByApInfo(ApInfo);
            if (apToKen == null)
            {
                OnOutputLog(LogInfoType.WARN, string.Format("在线AP列表中找不到Ap[{0}:{1}]设备({2})!",
                    ApInfo.IP, ApInfo.Port.ToString(), ApInfo.Fullname));
                return;
            }

            if (n_dic.name.Equals(Send_Msg_Id.QUERY_NB_CELL_INFO_MSG.ToString()))
            {
                Send2ap_QUERY_NB_CELL_INFO_MSG(apToKen, AppInfo, sys);
            }
            else if (n_dic.name.Equals(Send_Msg_Id.CONFIG_FAP_MSG.ToString()))
            {
                STRUCT_CONFIG_FAP_MSG para = new STRUCT_CONFIG_FAP_MSG();
                para.bWorkingMode = GetMsgByteValueInList("bWorkingMode", n_dic.dic, Byte.MaxValue);
                if (para.bWorkingMode != 1 && para.bWorkingMode != 3)
                {
                    SendMainMsgParaVlaueError(ApInfo, AppInfo, Send_Msg_Id.CONFIG_FAP_MSG.ToString(), "bWorkingMode");
                    return;
                }

                para.bC = GetMsgByteValueInList("bC", n_dic.dic, Byte.MaxValue);
                if (para.bC == Byte.MaxValue)
                {
                    SendMainMsgParaVlaueError(ApInfo, AppInfo, Send_Msg_Id.CONFIG_FAP_MSG.ToString(), "bC");
                    return;
                }

                para.wRedirectCellUarfcn = GetMsgU16ValueInList("wRedirectCellUarfcn", n_dic.dic, UInt16.MaxValue);
                if (para.wRedirectCellUarfcn == UInt16.MaxValue)
                {
                    SendMainMsgParaVlaueError(ApInfo, AppInfo, Send_Msg_Id.CONFIG_FAP_MSG.ToString(), "wRedirectCellUarfcn");
                    return;
                }

                para.dwDateTime = (UInt32)DateTime.Now.Subtract(DateTime.Parse("1970-1-1")).TotalSeconds;
                //para.dwDateTime = GetMsgU32ValueInList("dwDateTime", n_dic.dic, UInt32.MaxValue);
                //if (para.dwDateTime == UInt32.MaxValue)
                //{
                //    SendMainMsgParaVlaueError(ApInfo, AppInfo, Send_Msg_Id.CONFIG_FAP_MSG.ToString(), "dwDateTime");
                //    return;
                //}

                para.bPLMNId = GetMsgStringValueInList("bPLMNId", n_dic.dic, string.Empty);
                if (para.bPLMNId == string.Empty)
                {
                    SendMainMsgParaVlaueError(ApInfo, AppInfo, Send_Msg_Id.CONFIG_FAP_MSG.ToString(), "bPLMNId");
                    return;
                }

                para.bTxPower = GetMsgByteValueInList("bTxPower", n_dic.dic, Byte.MaxValue);
                if (para.bTxPower == Byte.MaxValue)
                {
                    SendMainMsgParaVlaueError(ApInfo, AppInfo, Send_Msg_Id.CONFIG_FAP_MSG.ToString(), "bTxPower");
                    return;
                }

                para.bRxGain = GetMsgByteValueInList("bRxGain", n_dic.dic, Byte.MaxValue);
                if (para.bRxGain == Byte.MaxValue)
                {
                    SendMainMsgParaVlaueError(ApInfo, AppInfo, Send_Msg_Id.CONFIG_FAP_MSG.ToString(), "bRxGain");
                    return;
                }

                para.wPhyCellId = GetMsgU16ValueInList("wPhyCellId", n_dic.dic, UInt16.MaxValue);
                if (para.wPhyCellId == UInt16.MaxValue)
                {
                    SendMainMsgParaVlaueError(ApInfo, AppInfo, Send_Msg_Id.CONFIG_FAP_MSG.ToString(), "wPhyCellId");
                    return;
                }

                para.wLAC = GetMsgU16ValueInList("wLAC", n_dic.dic, UInt16.MaxValue);
                if (para.wLAC == UInt16.MaxValue)
                {
                    SendMainMsgParaVlaueError(ApInfo, AppInfo, Send_Msg_Id.CONFIG_FAP_MSG.ToString(), "wLAC");
                    return;
                }

                para.wUARFCN = GetMsgU16ValueInList("wUARFCN", n_dic.dic, UInt16.MaxValue);
                if (para.wUARFCN == UInt16.MaxValue)
                {
                    SendMainMsgParaVlaueError(ApInfo, AppInfo, Send_Msg_Id.CONFIG_FAP_MSG.ToString(), "wUARFCN");
                    return;
                }

                para.dwCellId = GetMsgU32ValueInList("dwCellId", n_dic.dic, UInt32.MaxValue);
                if (para.dwCellId == UInt32.MaxValue)
                {
                    SendMainMsgParaVlaueError(ApInfo, AppInfo, Send_Msg_Id.CONFIG_FAP_MSG.ToString(), "dwCellId");
                    return;
                }

                Send2ap_CONFIG_FAP_MSG(apToKen, AppInfo, sys, para);
            }
            else if (n_dic.name.Equals(Send_Msg_Id.CONTROL_FAP_REBOOT_MSG.ToString()))
            {
                byte bRebootFlag = GetMsgByteValueInList("bRebootFlag", n_dic.dic, Byte.MaxValue);
                if (bRebootFlag != 1 && bRebootFlag != 3)
                {
                    SendMainMsgParaVlaueError(ApInfo, AppInfo, Send_Msg_Id.CONFIG_FAP_MSG.ToString(), "bRebootFlag");
                    return;
                }

                Send2ap_CONTROL_FAP_REBOOT_MSG(apToKen, AppInfo, sys, bRebootFlag);
            }
            else if (n_dic.name.Equals(Send_Msg_Id.CONFIG_SMS_CONTENT_MSG.ToString()))
            {
                string bSMSOriginalNum = GetMsgStringValueInList("bSMSOriginalNum", n_dic.dic, String.Empty);
                string bSMSContent = GetMsgStringValueInList("bSMSContent", n_dic.dic, String.Empty);
                if (String.IsNullOrEmpty(bSMSOriginalNum) || bSMSOriginalNum.Length <= 0)
                {
                    SendMainMsgParaVlaueError(ApInfo, AppInfo, Send_Msg_Id.CONFIG_SMS_CONTENT_MSG.ToString(), "bSMSOriginalNum");
                    return;
                }
                if (String.IsNullOrEmpty(bSMSContent))
                {
                    SendMainMsgParaVlaueError(ApInfo, AppInfo, Send_Msg_Id.CONFIG_SMS_CONTENT_MSG.ToString(), "bSMSContent");
                    return;
                }
                Send2ap_CONFIG_SMS_CONTENT_MSG(apToKen, AppInfo, sys, bSMSOriginalNum, bSMSContent);
            }
            else if (n_dic.name.Equals(Send_Msg_Id.CONTROL_FAP_RADIO_ON_MSG.ToString()))
            {
                Send2ap_CONTROL_FAP_RADIO_ON_MSG(apToKen, AppInfo, sys);
            }
            else if (n_dic.name.Equals(Send_Msg_Id.CONTROL_FAP_RADIO_OFF_MSG.ToString()))
            {
                Send2ap_CONTROL_FAP_RADIO_OFF_MSG(apToKen, AppInfo, sys);
            }
            else if (n_dic.name.Equals(Send_Msg_Id.CONTROL_FAP_RESET_MSG.ToString()))
            {
                Send2ap_CONTROL_FAP_RESET_MSG(apToKen, AppInfo, sys);
            }
            else if (n_dic.name.Equals(Send_Msg_Id.CONFIG_CDMA_CARRIER_MSG.ToString()))
            {
                STRUCT_CONFIG_CDMA_CARRIER_MSG para = new STRUCT_CONFIG_CDMA_CARRIER_MSG();
                para.wARFCN1 = GetMsgU16ValueInList("wARFCN1", n_dic.dic, UInt16.MaxValue);
                if (para.wARFCN1 == UInt16.MaxValue)
                {
                    SendMainMsgParaVlaueError(ApInfo, AppInfo, Send_Msg_Id.CONFIG_CDMA_CARRIER_MSG.ToString(), "wARFCN1");
                    return;
                }

                para.bARFCN1Mode = GetMsgByteValueInList("bARFCN1Mode", n_dic.dic, Byte.MaxValue);
                if (para.bARFCN1Mode != 0 && para.bARFCN1Mode != 1 && para.bARFCN1Mode != 2)
                {
                    SendMainMsgParaVlaueError(ApInfo, AppInfo, Send_Msg_Id.CONFIG_CDMA_CARRIER_MSG.ToString(), "bARFCN1Mode");
                    return;
                }

                para.wARFCN1Duration = GetMsgU16ValueInList("wARFCN1Duration", n_dic.dic, UInt16.MaxValue);
                if (para.wARFCN1Duration == UInt16.MaxValue)
                {
                    SendMainMsgParaVlaueError(ApInfo, AppInfo, Send_Msg_Id.CONFIG_CDMA_CARRIER_MSG.ToString(), "wARFCN1Duration");
                    return;
                }

                para.wARFCN1Period = GetMsgU16ValueInList("wARFCN1Period", n_dic.dic, UInt16.MaxValue);
                if (para.wARFCN1Period == UInt16.MaxValue)
                {
                    SendMainMsgParaVlaueError(ApInfo, AppInfo, Send_Msg_Id.CONFIG_CDMA_CARRIER_MSG.ToString(), "wARFCN1Period");
                    return;
                }

                para.wARFCN2 = GetMsgU16ValueInList("wARFCN2", n_dic.dic, UInt16.MaxValue);
                if (para.wARFCN2 == UInt16.MaxValue)
                {
                    SendMainMsgParaVlaueError(ApInfo, AppInfo, Send_Msg_Id.CONFIG_CDMA_CARRIER_MSG.ToString(), "wARFCN2");
                    return;
                }

                para.bARFCN2Mode = GetMsgByteValueInList("bARFCN2Mode", n_dic.dic, Byte.MaxValue);
                if (para.bARFCN2Mode != 0 && para.bARFCN2Mode != 1 && para.bARFCN2Mode != 2)
                {
                    SendMainMsgParaVlaueError(ApInfo, AppInfo, Send_Msg_Id.CONFIG_CDMA_CARRIER_MSG.ToString(), "bARFCN2Mode");
                    return;
                }

                para.wARFCN2Duration = GetMsgU16ValueInList("wARFCN2Duration", n_dic.dic, UInt16.MaxValue);
                if (para.wARFCN2Duration == UInt16.MaxValue)
                {
                    SendMainMsgParaVlaueError(ApInfo, AppInfo, Send_Msg_Id.CONFIG_CDMA_CARRIER_MSG.ToString(), "wARFCN2Duration");
                    return;
                }

                para.wARFCN2Period = GetMsgU16ValueInList("wARFCN2Period", n_dic.dic, UInt16.MaxValue);
                if (para.wARFCN2Period == UInt16.MaxValue)
                {
                    SendMainMsgParaVlaueError(ApInfo, AppInfo, Send_Msg_Id.CONFIG_CDMA_CARRIER_MSG.ToString(), "wARFCN2Period");
                    return;
                }

                para.wARFCN3 = GetMsgU16ValueInList("wARFCN3", n_dic.dic, UInt16.MaxValue);
                if (para.wARFCN3 == UInt16.MaxValue)
                {
                    SendMainMsgParaVlaueError(ApInfo, AppInfo, Send_Msg_Id.CONFIG_CDMA_CARRIER_MSG.ToString(), "wARFCN3");
                    return;
                }

                para.bARFCN3Mode = GetMsgByteValueInList("bARFCN3Mode", n_dic.dic, Byte.MaxValue);
                if (para.bARFCN3Mode != 0 && para.bARFCN3Mode != 1 && para.bARFCN3Mode != 2)
                {
                    SendMainMsgParaVlaueError(ApInfo, AppInfo, Send_Msg_Id.CONFIG_CDMA_CARRIER_MSG.ToString(), "bARFCN3Mode");
                    return;
                }

                para.wARFCN3Duration = GetMsgU16ValueInList("wARFCN3Duration", n_dic.dic, UInt16.MaxValue);
                if (para.wARFCN3Duration == UInt16.MaxValue)
                {
                    SendMainMsgParaVlaueError(ApInfo, AppInfo, Send_Msg_Id.CONFIG_CDMA_CARRIER_MSG.ToString(), "wARFCN3Duration");
                    return;
                }

                para.wARFCN3Period = GetMsgU16ValueInList("wARFCN3Period", n_dic.dic, UInt16.MaxValue);
                if (para.wARFCN3Period == UInt16.MaxValue)
                {
                    SendMainMsgParaVlaueError(ApInfo, AppInfo, Send_Msg_Id.CONFIG_CDMA_CARRIER_MSG.ToString(), "wARFCN3Period");
                    return;
                }

                para.wARFCN4 = GetMsgU16ValueInList("wARFCN4", n_dic.dic, UInt16.MaxValue);
                if (para.wARFCN4 == UInt16.MaxValue)
                {
                    SendMainMsgParaVlaueError(ApInfo, AppInfo, Send_Msg_Id.CONFIG_CDMA_CARRIER_MSG.ToString(), "wARFCN4");
                    return;
                }

                para.bARFCN4Mode = GetMsgByteValueInList("bARFCN4Mode", n_dic.dic, Byte.MaxValue);
                if (para.bARFCN4Mode != 0 && para.bARFCN4Mode != 1 && para.bARFCN4Mode != 2)
                {
                    SendMainMsgParaVlaueError(ApInfo, AppInfo, Send_Msg_Id.CONFIG_CDMA_CARRIER_MSG.ToString(), "bARFCN4Mode");
                    return;
                }

                para.wARFCN4Duration = GetMsgU16ValueInList("wARFCN4Duration", n_dic.dic, UInt16.MaxValue);
                if (para.wARFCN4Duration == UInt16.MaxValue)
                {
                    SendMainMsgParaVlaueError(ApInfo, AppInfo, Send_Msg_Id.CONFIG_CDMA_CARRIER_MSG.ToString(), "wARFCN4Duration");
                    return;
                }

                para.wARFCN4Period = GetMsgU16ValueInList("wARFCN4Period", n_dic.dic, UInt16.MaxValue);
                if (para.wARFCN4Period == UInt16.MaxValue)
                {
                    SendMainMsgParaVlaueError(ApInfo, AppInfo, Send_Msg_Id.CONFIG_CDMA_CARRIER_MSG.ToString(), "wARFCN4Period");
                    return;
                }

                Send2ap_CONFIG_CDMA_CARRIER_MSG(apToKen, AppInfo, sys, para);
            }
            else if (n_dic.name.Equals(Send_Msg_Id.QUERY_FAP_PARAM_MSG.ToString()))
            {
                Send2ap_QUERY_FAP_PARAM_MSG(apToKen, AppInfo, sys);
            }
            else if (n_dic.name.Equals(Send_Msg_Id.CONFIG_IMSI_MSG_V3_ID.ToString()))
            {
                STRUCT_CONFIG_IMSI_MSG_V3 para = new STRUCT_CONFIG_IMSI_MSG_V3(1000);
                para.wTotalImsi = GetMsgU16ValueInList("wTotalImsi", n_dic.dic, UInt16.MaxValue);
                if (para.wTotalImsi > 1000)
                {
                    SendMainMsgParaVlaueError(ApInfo, AppInfo, Send_Msg_Id.CONFIG_IMSI_MSG_V3_ID.ToString(), "wTotalImsi");
                    return;
                }

                //para.bIMSINum = GetMsgByteValueInList("bIMSINum", n_dic.dic, Byte.MaxValue);
                //if (para.bIMSINum <= 0 || para.bIMSINum > 50)
                //{
                //    SendMainMsgParaVlaueError(ApInfo, AppInfo, Send_Msg_Id.CONFIG_IMSI_MSG_V3.ToString(), "bIMSINum");
                //    return;
                //}

                //para.bSegmentType = GetMsgByteValueInList("bSegmentType", n_dic.dic, Byte.MaxValue);
                //if (para.bSegmentType != 1 && para.bSegmentType != 2 && para.bSegmentType != 3 && para.bSegmentType != 4)
                //{
                //    SendMainMsgParaVlaueError(ApInfo, AppInfo, Send_Msg_Id.CONFIG_IMSI_MSG_V3.ToString(), "bSegmentType");
                //    return;
                //}

                //para.bSegmentID = GetMsgByteValueInList("bSegmentID", n_dic.dic, Byte.MaxValue);
                //if (para.bSegmentID == Byte.MaxValue)
                //{
                //    SendMainMsgParaVlaueError(ApInfo, AppInfo, Send_Msg_Id.CONFIG_IMSI_MSG_V3.ToString(), "bSegmentID");
                //    return;
                //}

                para.bActionType = GetMsgByteValueInList("bActionType", n_dic.dic, Byte.MaxValue);
                if (para.bActionType != 1 && para.bActionType != 2 && para.bActionType != 3 && para.bActionType != 4)
                {
                    SendMainMsgParaVlaueError(ApInfo, AppInfo, Send_Msg_Id.CONFIG_IMSI_MSG_V3_ID.ToString(), "bActionType");
                    return;
                }

                for (int i = 0; i < para.wTotalImsi; i++)
                {
                    para.bIMSI[i] = GetMsgStringValueInList(string.Format("bIMSI_#{0}#", i), n_dic.dic, string.Empty);
                    if (String.IsNullOrEmpty(para.bIMSI[i]))
                    {
                        SendMainMsgParaVlaueError(ApInfo, AppInfo, Send_Msg_Id.CONFIG_IMSI_MSG_V3_ID.ToString(), string.Format("bIMSI_#{0}#", i));
                        return;
                    }
                }

                for (int i = 0; i < para.wTotalImsi; i++)
                {
                    para.bUeActionFlag[i] = GetMsgByteValueInList(string.Format("bUeActionFlag_#{0}#", i), n_dic.dic, Byte.MaxValue);
                    if (para.bUeActionFlag[i] != 1 && para.bUeActionFlag[i] != 5)
                    {
                        SendMainMsgParaVlaueError(ApInfo, AppInfo, Send_Msg_Id.CONFIG_IMSI_MSG_V3_ID.ToString(), string.Format("bUeActionFlag#{0}#", i));
                        return;
                    }
                }
                Send2ap_CONFIG_IMSI_MSG_V3(apToKen, AppInfo, sys, para);
            }
            else
            {
                string str = string.Format("发送给CDMA设备消息({0})错误。暂不支持该消息类型)。", n_dic.name);
                OnOutputLog(LogInfoType.EROR, str);
                Send2APP_GeneralError(ApInfo, AppInfo, n_dic.name, str);
            }
        }

        //    private void EncodeSetParaMsg(Ap_Info_Struct ApInfo, App_Info_Struct AppInfo, Device_Sys sys, List<Name_DIC_Struct> n_dic_List)
        //    {
        //        int Flag = 0;
        //        RecvSysPara para = new RecvSysPara();
        //        RecvSysOption option = new RecvSysOption();
        //        RecvRfOption rf = new RecvRfOption();
        //        byte regMode = 0;

        //        AsyncUserToken apToKen = MyDeviceList.FindByIpPort(ApInfo.IP, ApInfo.Port);
        //        if (apToKen == null)
        //        {
        //            string str = string.Format("在线AP列表中找不到Ap[{0}:{1}]设备，通过FullName重新查询设备！",
        //                ApInfo.IP, ApInfo.Port.ToString());
        //            OnOutputLog(LogInfoType.WARN, str);
        //            apToKen = MyDeviceList.FindByFullname(ApInfo.Fullname);
        //        }

        //        if (apToKen == null)
        //        {
        //            string str = string.Format("在线AP列表中找不到Ap[{0}:{1}],FullName:{2}。无法向AP发送消息！",
        //                ApInfo.IP, ApInfo.Port.ToString(), ApInfo.Fullname);
        //            OnOutputLog(LogInfoType.WARN, str);
        //            Send2APP_GeneralError(ApInfo, AppInfo, Main2ApControllerMsgType.gsm_msg_send, str);
        //            return;
        //        }

        //        foreach (Name_DIC_Struct n_dic in n_dic_List)
        //        {
        //        //    if (n_dic.name.Equals(Send_Msg_Id.RECV_SYS_PARA.ToString()))
        //        //    {
        //        //        para.paraMnc = GetMsgU16ValueInList("paraMnc", n_dic.dic, UInt16.MaxValue);
        //        //        if (para.paraMnc == UInt16.MaxValue)
        //        //        {
        //        //            SendMainMsgParaVlaueError(ApInfo, AppInfo, Send_Msg_Id.RECV_SYS_PARA.ToString(), "paraMnc");
        //        //            return;
        //        //        }
        //        //        para.paraMcc = GetMsgU16ValueInList("paraMcc", n_dic.dic, UInt16.MaxValue);
        //        //        if (para.paraMcc == UInt16.MaxValue)
        //        //        {
        //        //            SendMainMsgParaVlaueError(ApInfo, AppInfo, Send_Msg_Id.RECV_SYS_PARA.ToString(), "paraMcc");
        //        //            return;
        //        //        }
        //        //        para.paraBsic = GetMsgByteValueInList("paraBsic", n_dic.dic);
        //        //        if (para.paraBsic == 0)
        //        //        {
        //        //            SendMainMsgParaVlaueError(ApInfo, AppInfo, Send_Msg_Id.RECV_SYS_PARA.ToString(), "paraBsic");
        //        //            return;
        //        //        }
        //        //        para.paraLac = GetMsgU16ValueInList("paraLac", n_dic.dic);
        //        //        if (para.paraLac == 0)
        //        //        {
        //        //            SendMainMsgParaVlaueError(ApInfo, AppInfo, Send_Msg_Id.RECV_SYS_PARA.ToString(), "paraLac");
        //        //            return;
        //        //        }
        //        //        para.paraCellId = GetMsgU16ValueInList("paraCellId", n_dic.dic);
        //        //        //if (para.paraCellId == 0)
        //        //        //{
        //        //        //    SendMainMsgParaVlaueError(ApInfo,AppInfo, Send_Msg_Id.RECV_SYS_PARA.ToString(), "paraCellId");
        //        //        //    return;
        //        //        //}
        //        //        para.paraC2 = GetMsgSByteValueInList("paraC2", n_dic.dic, SByte.MaxValue);
        //        //        if (para.paraC2 == SByte.MaxValue)
        //        //        {
        //        //            SendMainMsgParaVlaueError(ApInfo, AppInfo, Send_Msg_Id.RECV_SYS_PARA.ToString(), "paraC2");
        //        //            return;
        //        //        }
        //        //        para.paraPeri = GetMsgByteValueInList("paraPeri", n_dic.dic, Byte.MaxValue);
        //        //        //if (para.paraPeri == 0)
        //        //        //{
        //        //        //    SendMainMsgParaVlaueError(ApInfo,AppInfo, Send_Msg_Id.RECV_SYS_PARA.ToString(), "paraPeri");
        //        //        //    return;
        //        //        //}
        //        //        para.paraAccPwr = GetMsgByteValueInList("paraAccPwr", n_dic.dic, Byte.MaxValue);
        //        //        if (para.paraAccPwr == Byte.MaxValue)
        //        //        {
        //        //            SendMainMsgParaVlaueError(ApInfo, AppInfo, Send_Msg_Id.RECV_SYS_PARA.ToString(), "paraAccPwr");
        //        //            return;
        //        //        }
        //        //        para.paraMsPwr = GetMsgByteValueInList("paraMsPwr", n_dic.dic, Byte.MaxValue);
        //        //        if (para.paraMsPwr == Byte.MaxValue)
        //        //        {
        //        //            SendMainMsgParaVlaueError(ApInfo, AppInfo, Send_Msg_Id.RECV_SYS_PARA.ToString(), "paraMsPwr");
        //        //            return;
        //        //        }
        //        //        para.paraRejCau = GetMsgByteValueInList("paraRejCau", n_dic.dic, Byte.MaxValue);
        //        //        if (para.paraRejCau == Byte.MaxValue)
        //        //        {
        //        //            SendMainMsgParaVlaueError(ApInfo, AppInfo, Send_Msg_Id.RECV_SYS_PARA.ToString(), "paraRejCau");
        //        //            return;
        //        //        }
        //        //        Flag |= 0x1;
        //        //    }
        //        //    else if (n_dic.name.Equals(Send_Msg_Id.RECV_SYS_OPTION.ToString()))
        //        //    {
        //        //        option.opLuSms = GetMsgByteValueInList("opLuSms", n_dic.dic, Byte.MaxValue);
        //        //        if (option.opLuSms != 0 && option.opLuSms != 1)
        //        //        {
        //        //            SendMainMsgParaVlaueError(ApInfo, AppInfo, Send_Msg_Id.RECV_SYS_OPTION.ToString(), "opLuSms");
        //        //            return;
        //        //        }
        //        //        option.opLuImei = GetMsgByteValueInList("opLuImei", n_dic.dic, Byte.MaxValue);
        //        //        if (option.opLuImei != 0 && option.opLuImei != 1)
        //        //        {
        //        //            SendMainMsgParaVlaueError(ApInfo, AppInfo, Send_Msg_Id.RECV_SYS_OPTION.ToString(), "opLuImei");
        //        //            return;
        //        //        }
        //        //        option.opCallEn = GetMsgByteValueInList("opCallEn", n_dic.dic, Byte.MaxValue);
        //        //        if (option.opCallEn != 0 && option.opCallEn != 1)
        //        //        {
        //        //            SendMainMsgParaVlaueError(ApInfo, AppInfo, Send_Msg_Id.RECV_SYS_OPTION.ToString(), "opCallEn");
        //        //            return;
        //        //        }
        //        //        option.opDebug = GetMsgByteValueInList("opDebug", n_dic.dic, Byte.MaxValue);
        //        //        if (option.opDebug != 0 && option.opDebug != 1)
        //        //        {
        //        //            SendMainMsgParaVlaueError(ApInfo, AppInfo, Send_Msg_Id.RECV_SYS_OPTION.ToString(), "opDebug");
        //        //            return;
        //        //        }
        //        //        option.opLuType = GetMsgByteValueInList("opLuType", n_dic.dic, Byte.MaxValue);
        //        //        if (option.opLuType != 1 && option.opLuType != 2 && option.opLuType != 3)
        //        //        {
        //        //            SendMainMsgParaVlaueError(ApInfo, AppInfo, Send_Msg_Id.RECV_SYS_OPTION.ToString(), "opLuType");
        //        //            return;
        //        //        }
        //        //        option.opSmsType = GetMsgByteValueInList("opSmsType", n_dic.dic, Byte.MaxValue);
        //        //        if (option.opSmsType != 1 && option.opSmsType != 2 && option.opSmsType != 3)
        //        //        {
        //        //            SendMainMsgParaVlaueError(ApInfo, AppInfo, Send_Msg_Id.RECV_SYS_OPTION.ToString(), "opSmsType");
        //        //            return;
        //        //        }
        //        //        Flag |= 0x02;
        //        //    }
        //        //    else if (n_dic.name.Equals(Send_Msg_Id.RECV_RF_PARA.ToString()))
        //        //    {
        //        //        rf.rfEnable = GetMsgByteValueInList("rfEnable", n_dic.dic, Byte.MaxValue);
        //        //        if (rf.rfEnable != 0 && rf.rfEnable != 1)
        //        //        {
        //        //            SendMainMsgParaVlaueError(ApInfo, AppInfo, Send_Msg_Id.RECV_RF_PARA.ToString(), "rfEnable");
        //        //            return;
        //        //        }
        //        //        rf.rfFreq = GetMsgU16ValueInList("rfFreq", n_dic.dic, UInt16.MaxValue);
        //        //        if (rf.rfFreq == UInt16.MaxValue)
        //        //        {
        //        //            SendMainMsgParaVlaueError(ApInfo, AppInfo, Send_Msg_Id.RECV_RF_PARA.ToString(), "rfFreq");
        //        //            return;
        //        //        }
        //        //        rf.rfPwr = GetMsgByteValueInList("rfPwr", n_dic.dic, Byte.MaxValue);
        //        //        if (rf.rfPwr == Byte.MaxValue)
        //        //        {
        //        //            SendMainMsgParaVlaueError(ApInfo, AppInfo, Send_Msg_Id.RECV_RF_PARA.ToString(), "rfPwr");
        //        //            return;
        //        //        }
        //        //        Flag |= 0x04;
        //        //    }
        //        //    else if (n_dic.name.Equals(Send_Msg_Id.RECV_REG_MODE.ToString()))
        //        //    {
        //        //        regMode = GetMsgByteValueInList("regMode", n_dic.dic, Byte.MaxValue);
        //        //        if (regMode != 0 && regMode != 1)
        //        //        {
        //        //            SendMainMsgParaVlaueError(ApInfo, AppInfo, Send_Msg_Id.RECV_REG_MODE.ToString(), "regMode");
        //        //            return;
        //        //        }

        //        //        Flag |= 0x08;
        //        //    }
        //        //}

        //        if (Flag == 0)
        //        {
        //            string str = string.Format("发送给GSM设备消息({0})错误。消息中没有n_dic项。", Main2ApControllerMsgType.SetGenParaReq);
        //            OnOutputLog(LogInfoType.WARN, str);
        //            return;
        //        }

        //        Send2ap_SET_PARA_REQ(apToKen, AppInfo, sys, Flag, para, option, rf, regMode);
        //    }

        #region 封装发送给Main模块消息

        private string StringDelZero(string str)
        {
            string data = string.Empty;
            for (int i = 0; i < str.Length; i+=2)
            {
                data = string.Format("{0}{1}", data, str[i+1].ToString());
            }
            return data;
        }

        /// <summary>
        /// 向Main模块发送系统参数设备更改通知
        /// </summary>
        /// <param name="apToKen">AP设备信息</param>
        /// <param name="recv">收到的参数</param>
        private void Send2Main_SEND_REQ_CNF(AsyncUserToken apToKen, MsgRecvStruct recv)
        {
            String data = recv.data;
            Msg_Body_Struct TypeKeyValue = new Msg_Body_Struct(Main2ApControllerMsgType.gsm_msg_recv);
            TypeKeyValue.dic.Add("sys", recv.bCellIdx);
            TypeKeyValue.dic.Add("hardware_id", 0);

            Name_DIC_Struct nDic = new Name_DIC_Struct();
            nDic.name = "SEND_REQ_CNF".ToString();

            GetValue_Reserved(1, ref data);
            nDic.dic.Add("cnfType", Enum.GetName(typeof(Recv_Msg_Id), recv.bMsgId));
            if (recv.bMsgType == Msg_Type.SUCC_OUTCOME)
                nDic.dic.Add("cnfInd",  0);
            else
                nDic.dic.Add("cnfInd", 1);

            TypeKeyValue.n_dic.Add(nDic);

            OnSendMsg2Main(recv.wSqn, MsgType.CONFIG, apToKen, TypeKeyValue);
        }

        private void Send2Main_FAP_NB_CELL_INFO_MSG(AsyncUserToken apToKen, MsgRecvStruct recv)
        {
            String data = recv.data;
            Msg_Body_Struct TypeKeyValue = new Msg_Body_Struct(Main2ApControllerMsgType.gsm_msg_recv);
            TypeKeyValue.dic.Add("sys", recv.bCellIdx);
            TypeKeyValue.dic.Add("hardware_id", 0);

            Name_DIC_Struct nDic = new Name_DIC_Struct();
            nDic.name = Recv_Msg_Id.FAP_NB_CELL_INFO_MSG.ToString();

            Byte bFapNbCellNum = GetValueByString_Byte(ref data);
            if (bFapNbCellNum<0 || bFapNbCellNum>16)
            {
                OnOutputLog(LogInfoType.WARN, string.Format("邻小区的个数错误，上报个数为{0}。",bFapNbCellNum));
                return;
            }
            nDic.dic.Add("bFapNbCellNum", bFapNbCellNum.ToString());

            GetValue_Reserved(3,ref data);

            for (int i = 0; i < bFapNbCellNum; i++)
            {
                UInt32 bGlobalCellId = GetValueByString_U32(ref data);
                nDic.dic.Add(string.Format("Cell_#{0}#/bGlobalCellId", i), bGlobalCellId.ToString());
                string bPLMNId = StringDelZero(GetValueByString_String(10, ref data));
                nDic.dic.Add(string.Format("Cell_#{0}#/bPLMNId", i), bPLMNId.ToString());
                SByte cRSRP = GetValueByString_SByte(ref data);
                nDic.dic.Add(string.Format("Cell_#{0}#/cRSRP", i), cRSRP.ToString());
                UInt16 wTac = GetValueByString_U16(ref data);
                nDic.dic.Add(string.Format("Cell_#{0}#/wTac", i), wTac.ToString());
                UInt16 wPhyCellId = GetValueByString_U16(ref data);
                nDic.dic.Add(string.Format("Cell_#{0}#/wPhyCellId", i), wPhyCellId.ToString());
                UInt16 wUARFCN = GetValueByString_U16(ref data);
                nDic.dic.Add(string.Format("Cell_#{0}#/wUARFCN", i), wUARFCN.ToString());
                SByte cRefTxPower = GetValueByString_SByte(ref data);
                nDic.dic.Add(string.Format("Cell_#{0}#/cRefTxPower", i), cRefTxPower.ToString());
                Byte bNbCellNum = GetValueByString_Byte(ref data);
                if (bNbCellNum < 0 || bNbCellNum > 32)
                {
                    OnOutputLog(LogInfoType.WARN, string.Format("邻小区{0}的个数错误，上报个数为{1}。", i, bNbCellNum));
                    return;
                }
                nDic.dic.Add(string.Format("Cell_#{0}#/bNbCellNum", i), bNbCellNum.ToString());
                nDic.dic.Add(string.Format("Cell_#{0}#/bReserved", i), GetValueByString_String(2,ref data));
                Byte bC2 = GetValueByString_Byte(ref data);
                nDic.dic.Add(string.Format("Cell_#{0}#/bC2", i), bC2.ToString());
                nDic.dic.Add(string.Format("Cell_#{0}#/bReserved1", i), GetValueByString_String(8, ref data));

                for (int j = 0; j < bNbCellNum; j++)
                {
                    UInt16 wUarfcn = GetValueByString_U16(ref data);
                    nDic.dic.Add(string.Format("Cell_#{0}#/NeighCell_#{1}#/wUarfcn", i, j), wUarfcn.ToString());
                    UInt16 wPhyCellId1 = GetValueByString_U16(ref data);
                    nDic.dic.Add(string.Format("Cell_#{0}#/NeighCell_#{1}#/wPhyCellId", i, j), wPhyCellId1.ToString());
                    SByte cRSRP1 = GetValueByString_SByte(ref data);
                    nDic.dic.Add(string.Format("Cell_#{0}#/NeighCell_#{1}#/cRSRP", i, j), cRSRP1.ToString());
                    GetValue_Reserved(1, ref data);
                    SByte cC1 = GetValueByString_SByte(ref data);
                    nDic.dic.Add(string.Format("Cell_#{0}#/NeighCell_#{1}#/cC1", i, j), cC1.ToString());
                    Byte bC21 = GetValueByString_Byte(ref data);
                    nDic.dic.Add(string.Format("Cell_#{0}#/NeighCell_#{1}#/bC2", i, j), bC21.ToString());
                }

                for (int j = bNbCellNum; j < 32; j++)
                {
                    GetValue_Reserved(8, ref data);
                }
            }

            TypeKeyValue.n_dic.Add(nDic);

            OnSendMsg2Main(recv.wSqn, MsgType.CONFIG, apToKen, TypeKeyValue);
        }

        private void Send2Main_CONFIG_FAP_MSG(AsyncUserToken apToKen, MsgRecvStruct recv)
        {
            String data = recv.data;
            Msg_Body_Struct TypeKeyValue = new Msg_Body_Struct(Main2ApControllerMsgType.ReportGenPara);
            TypeKeyValue.dic.Add("sys", recv.bCellIdx);
            TypeKeyValue.dic.Add("hardware_id", 0);

            Name_DIC_Struct nDic = new Name_DIC_Struct();
            nDic.name = Recv_Msg_Id.CONFIG_FAP_MSG.ToString();

            nDic.dic.Add("bWorkingMode", GetValueByString_Byte(ref data).ToString());
            nDic.dic.Add("bC", GetValueByString_Byte(ref data).ToString());
            nDic.dic.Add("wRedirectCellUarfcn", GetValueByString_U16(ref data).ToString());
            GetValue_Reserved(4, ref data);
            GetValue_Reserved(4, ref data);
            //nDic.dic.Add("dwDateTime", GetValueByString_U32(ref data).ToString());
            string plmn = CodeConver.AscStr2str(GetValueByString_String(10,ref data).ToString());
            //for (int i= 0;i<5;i++)
            //{
            //    plmn = string.Format("{0}{1}",plmn, GetValueByString_Byte(ref data).ToString());
            //}
            
            nDic.dic.Add("bPLMNId", plmn);
            nDic.dic.Add("bTxPower", GetValueByString_Byte(ref data).ToString());
            nDic.dic.Add("cReserved", GetValueByString_SByte(ref data).ToString());
            nDic.dic.Add("bRxGain", GetValueByString_Byte(ref data).ToString());
            nDic.dic.Add("wPhyCellId", GetValueByString_U16(ref data).ToString());
            nDic.dic.Add("wLAC", GetValueByString_U16(ref data).ToString());
            nDic.dic.Add("wUARFCN", GetValueByString_U16(ref data).ToString());
            GetValue_Reserved(2, ref data);
            nDic.dic.Add("dwCellId", GetValueByString_U32(ref data).ToString());
            GetValue_Reserved(32, ref data);

            TypeKeyValue.n_dic.Add(nDic);

            OnSendMsg2Main(recv.wSqn, MsgType.CONFIG, apToKen, TypeKeyValue);
        }

        private void Send2Main_CONTROL_FAP_REBOOT_MSG(AsyncUserToken apToKen, MsgRecvStruct recv)
        {
            String data = recv.data;
            Msg_Body_Struct TypeKeyValue = new Msg_Body_Struct(Main2ApControllerMsgType.gsm_msg_recv);
            TypeKeyValue.dic.Add("sys", recv.bCellIdx);
            TypeKeyValue.dic.Add("hardware_id", 0);

            Name_DIC_Struct nDic = new Name_DIC_Struct();
            nDic.name = Recv_Msg_Id.CONTROL_FAP_REBOOT_MSG.ToString();

            nDic.dic.Add("bRebootFlag", GetValueByString_Byte(ref data).ToString());
            GetValue_Reserved(3, ref data);

            TypeKeyValue.n_dic.Add(nDic);

            OnSendMsg2Main(recv.wSqn, MsgType.CONFIG, apToKen, TypeKeyValue);
        }

        private void Send2Main_FAP_TRACE_MSG(AsyncUserToken apToKen, MsgRecvStruct recv,string msgType)
        {
            String data = recv.data;
            Msg_Body_Struct TypeKeyValue = new Msg_Body_Struct(msgType);
            TypeKeyValue.dic.Add("sys", recv.bCellIdx);
            TypeKeyValue.dic.Add("hardware_id", 0);

            Name_DIC_Struct nDic = new Name_DIC_Struct();
            nDic.name = Recv_Msg_Id.FAP_TRACE_MSG.ToString();

            nDic.dic.Add("wTraceLen", GetValueByString_U16(ref data).ToString());
            nDic.dic.Add("cTrace", GetValueByString_String(1024,ref data).ToString());

            TypeKeyValue.n_dic.Add(nDic);

            OnSendMsg2Main(recv.wSqn, MsgType.CONFIG, apToKen, TypeKeyValue);
        }

        private void Send2Main_UE_STATUS_REPORT_MSG(AsyncUserToken apToKen, MsgRecvStruct recv, string msgType)
        {
            String data = recv.data;
            Msg_Body_Struct TypeKeyValue = new Msg_Body_Struct(msgType);
            TypeKeyValue.dic.Add("sys", recv.bCellIdx);
            TypeKeyValue.dic.Add("hardware_id", 0);

            Name_DIC_Struct nDic = new Name_DIC_Struct();
            nDic.name = Recv_Msg_Id.UE_STATUS_REPORT_MSG.ToString();

            byte addFlag = 0;
            byte type1 = GetValueByString_Byte(ref data);

            string msg = GetValueByString_String(30, ref data).ToString();
            sbyte rsrp = GetValueByString_SByte(ref data);
            byte len = GetValueByString_Byte(ref data);

            if (type1 == 1) 
            {
                string imsi = string.Empty;
                for (int i = 0; i < len; i++)
                {
                    imsi = string.Format("{0}{1}", imsi, GetValueByString_Byte(ref msg).ToString());
                }
                nDic.dic.Add("imsi", imsi.ToString());
                addFlag |= 0x1;
            }
            else if (type1 == 2)
            {
                string imsi = string.Empty;
                for (int i = 0; i < len; i++)
                {
                    imsi = string.Format("{0}{1}", imsi, GetValueByString_Byte(ref msg).ToString("X"));
                }
                nDic.dic.Add("tmsi", "0x" + imsi.ToString());
                addFlag |= 0x2;
            }
            else if (type1 == 3)
            {
                string imsi = string.Empty;
                for (int i = 0; i < len; i++)
                {
                    imsi = string.Format("{0}{1}", imsi, GetValueByString_Byte(ref msg).ToString());
                }
                nDic.dic.Add("imei", imsi.ToString());
                addFlag |= 0x4;
            }


            byte type2 = GetValueByString_Byte(ref data);
            msg = GetValueByString_String(30, ref data).ToString();
            len = GetValueByString_Byte(ref data);
         
            if (type2 == 1)
            {
                string imsi = string.Empty;
                for (int i = 0; i < len; i++)
                {
                    imsi = string.Format("{0}{1}", imsi, GetValueByString_Byte(ref msg).ToString());
                }
                nDic.dic.Add("imsi", imsi.ToString());
                addFlag |= 0x1;
            }
            else if (type2 == 2)
            {
                string imsi = string.Empty;
                for (int i = 0; i < len; i++)
                {
                    imsi = string.Format("{0}{1}", imsi, GetValueByString_Byte(ref msg).ToString("X"));
                }
                nDic.dic.Add("tmsi", "0x" + imsi.ToString());
                addFlag |= 0x2;
            }
            else if (type2 == 3)
            {
                string imsi = string.Empty;
                for (int i = 0; i < len; i++)
                {
                    imsi = string.Format("{0}{1}", imsi, GetValueByString_Byte(ref msg).ToString());
                }
                nDic.dic.Add("imei", imsi.ToString());
                addFlag |= 0x4;
            }


            byte type3 = GetValueByString_Byte(ref data);
            msg = GetValueByString_String(30, ref data).ToString();
            len = GetValueByString_Byte(ref data);
            
            if (type3 == 1)
            {
                string imsi = string.Empty;
                for (int i = 0; i < len; i++)
                {
                    imsi = string.Format("{0}{1}", imsi, GetValueByString_Byte(ref msg).ToString());
                }
                nDic.dic.Add("imsi", imsi.ToString());
                addFlag |= 0x1;
            }
            else if (type3 == 2)
            {
                string imsi = string.Empty;
                for (int i = 0; i < len; i++)
                {
                    imsi = string.Format("{0}{1}", imsi, GetValueByString_Byte(ref msg).ToString("X"));
                }
                nDic.dic.Add("tmsi", "0x" + imsi.ToString());
                addFlag |= 0x2;
            }
            else if (type3 == 3)
            {
                string imsi = string.Empty;
                for (int i = 0; i < len; i++)
                {
                    imsi = string.Format("{0}{1}", imsi, GetValueByString_Byte(ref msg).ToString());
                }
                nDic.dic.Add("imei", imsi.ToString());
                addFlag |= 0x4;
            }


            if ((addFlag & 0X1) <= 0)
            {
                nDic.dic.Add("imsi", "");
            }
            if ((addFlag & 0X2) <= 0)
            {
                nDic.dic.Add("tmsi", "");
            }
            if ((addFlag & 0X4) <= 0)
            {
                nDic.dic.Add("imei", "");
            }

            nDic.dic.Add("rsrp", rsrp.ToString());
            nDic.dic.Add("userType", "");
            nDic.dic.Add("sn", apToKen.Sn);

            TypeKeyValue.n_dic.Add(nDic);

            OnSendMsg2Main(recv.wSqn, MsgType.NOTICE, apToKen, TypeKeyValue);
        }

        private void Send2Main_UE_ORM_REPORT_MSG(AsyncUserToken apToKen, MsgRecvStruct recv, string msgType)
        {
            String data = recv.data;
            Msg_Body_Struct TypeKeyValue = new Msg_Body_Struct(msgType);
            TypeKeyValue.dic.Add("sys", recv.bCellIdx);
            TypeKeyValue.dic.Add("hardware_id", 0);

            Name_DIC_Struct nDic = new Name_DIC_Struct();
            nDic.name = Recv_Msg_Id.UE_ORM_REPORT_MSG.ToString();

            nDic.dic.Add("bOrmType", GetValueByString_Byte(ref data).ToString());
            nDic.dic.Add("bUeId", GetValueByString_String(30, ref data).ToString());
            nDic.dic.Add("cRSRP", GetValueByString_SByte(ref data).ToString());
            Byte bUeContentLen = GetValueByString_Byte(ref data);
            nDic.dic.Add("bUeContentLen", bUeContentLen.ToString());

            nDic.dic.Add("bUeContent", GetValueByString_String(bUeContentLen * 2, ref data).ToString());
           
            TypeKeyValue.n_dic.Add(nDic);

            OnSendMsg2Main(recv.wSqn, MsgType.CONFIG, apToKen, TypeKeyValue);
        }

        private void Send2Main_CONFIG_SMS_CONTENT_MSG_ID(AsyncUserToken apToKen, MsgRecvStruct recv)
        {
            String data = recv.data;
            Msg_Body_Struct TypeKeyValue = new Msg_Body_Struct(Main2ApControllerMsgType.ReportGenPara);
            TypeKeyValue.dic.Add("sys", recv.bCellIdx);
            TypeKeyValue.dic.Add("hardware_id", 0);

            Name_DIC_Struct nDic = new Name_DIC_Struct();
            nDic.name = Recv_Msg_Id.CONFIG_SMS_CONTENT_MSG_ID.ToString();

            nDic.dic.Add("bSMSOriginalNumLen", GetValueByString_Byte(ref data).ToString());
            nDic.dic.Add("bSMSOriginalNum", CodeConver.strToToHexByte(GetValueByString_String(36, ref data).ToString()));
 
            Byte bSMSContentLen = GetValueByString_Byte(ref data);
            nDic.dic.Add("bSMSContentLen", bSMSContentLen.ToString());
            string bSMSContent = GetValueByString_String(bSMSContentLen* 2, ref data);
            nDic.dic.Add("bSMSContent", CodeConver.Unicode2String(bSMSContent));

            TypeKeyValue.n_dic.Add(nDic);

            OnSendMsg2Main(recv.wSqn, MsgType.CONFIG, apToKen, TypeKeyValue);
        }

        private void Send2Main_FAP_PARAM_REPORT_MSG(AsyncUserToken apToKen, MsgRecvStruct recv)
        {
            String data = recv.data;
            Msg_Body_Struct TypeKeyValue = new Msg_Body_Struct(Main2ApControllerMsgType.gsm_msg_recv);
            TypeKeyValue.dic.Add("sys", recv.bCellIdx);
            TypeKeyValue.dic.Add("hardware_id", 0);

            Name_DIC_Struct nDic = new Name_DIC_Struct();
            nDic.name = Recv_Msg_Id.FAP_PARAM_REPORT_MSG.ToString();

            nDic.dic.Add("bWorkingMode", GetValueByString_Byte(ref data).ToString());
            GetValue_Reserved(1, ref data);
            nDic.dic.Add("wCDMAUarfcn", GetValueByString_U16(ref data).ToString());
            nDic.dic.Add("bPLMNId", StringDelZero( GetValueByString_String(10, ref data).ToString()));
            nDic.dic.Add("bDlAtt", GetValueByString_Byte(ref data).ToString());
            GetValue_Reserved(1, ref data);
            nDic.dic.Add("bRxGain", GetValueByString_Byte(ref data).ToString());
            nDic.dic.Add("wPhyCellId", GetValueByString_U16(ref data).ToString());
            nDic.dic.Add("wUARFCN", GetValueByString_U16(ref data).ToString());
            GetValue_Reserved(2, ref data);
            nDic.dic.Add("dwCellId", GetValueByString_U32(ref data).ToString());

            nDic.dic.Add("wARFCN1", GetValueByString_U16(ref data).ToString());
            nDic.dic.Add("bARFCN1Mode", GetValueByString_Byte(ref data).ToString());
            GetValue_Reserved(1, ref data);
            nDic.dic.Add("wARFCN1Duration", GetValueByString_U16(ref data).ToString());
            nDic.dic.Add("wARFCN1Period", GetValueByString_U16(ref data).ToString());

            nDic.dic.Add("wARFCN2", GetValueByString_U16(ref data).ToString());
            nDic.dic.Add("bARFCN2Mode", GetValueByString_Byte(ref data).ToString());
            GetValue_Reserved(1, ref data);
            nDic.dic.Add("wARFCN2Duration", GetValueByString_U16(ref data).ToString());
            nDic.dic.Add("wARFCN2Period", GetValueByString_U16(ref data).ToString());

            nDic.dic.Add("wARFCN3", GetValueByString_U16(ref data).ToString());
            nDic.dic.Add("bARFCN3Mode", GetValueByString_Byte(ref data).ToString());
            GetValue_Reserved(1, ref data);
            nDic.dic.Add("wARFCN3Duration", GetValueByString_U16(ref data).ToString());
            nDic.dic.Add("wARFCN3Period", GetValueByString_U16(ref data).ToString());

            nDic.dic.Add("wARFCN4", GetValueByString_U16(ref data).ToString());
            nDic.dic.Add("bARFCN4Mode", GetValueByString_Byte(ref data).ToString());
            GetValue_Reserved(1, ref data);
            nDic.dic.Add("wARFCN4Duration", GetValueByString_U16(ref data).ToString());
            nDic.dic.Add("wARFCN4Period", GetValueByString_U16(ref data).ToString());

            TypeKeyValue.n_dic.Add(nDic);

            OnSendMsg2Main(recv.wSqn, MsgType.CONFIG, apToKen, TypeKeyValue);
        }

        private void Send2Main_CONTROL_FAP_RADIO_ON_MSG(AsyncUserToken apToKen, MsgRecvStruct recv)
        {
            String data = recv.data;
            Msg_Body_Struct TypeKeyValue = new Msg_Body_Struct(Main2ApControllerMsgType.gsm_msg_recv);
            TypeKeyValue.dic.Add("sys", recv.bCellIdx);
            TypeKeyValue.dic.Add("hardware_id", 0);

            Name_DIC_Struct nDic = new Name_DIC_Struct();
            nDic.name = Recv_Msg_Id.CONTROL_FAP_RADIO_ON_MSG.ToString();

            nDic.dic.Add("rfStatus", GetValueByString_Byte(ref data).ToString());
            
            TypeKeyValue.n_dic.Add(nDic);

            OnSendMsg2Main(recv.wSqn, MsgType.CONFIG, apToKen, TypeKeyValue);
        }

        private void Send2Main_CONTROL_FAP_RADIO_OFF_MSG(AsyncUserToken apToKen, MsgRecvStruct recv)
        {
            String data = recv.data;
            Msg_Body_Struct TypeKeyValue = new Msg_Body_Struct(Main2ApControllerMsgType.gsm_msg_recv);
            TypeKeyValue.dic.Add("sys", recv.bCellIdx);
            TypeKeyValue.dic.Add("hardware_id", 0);

            Name_DIC_Struct nDic = new Name_DIC_Struct();
            nDic.name = Recv_Msg_Id.CONTROL_FAP_RADIO_OFF_MSG.ToString();

            nDic.dic.Add("rfStatus", GetValueByString_Byte(ref data).ToString());

            TypeKeyValue.n_dic.Add(nDic);

            OnSendMsg2Main(recv.wSqn, MsgType.CONFIG, apToKen, TypeKeyValue);
        }

        private void Send2Main_CONTROL_FAP_RESET_MSG(AsyncUserToken apToKen, MsgRecvStruct recv)
        {
            String data = recv.data;
            Msg_Body_Struct TypeKeyValue = new Msg_Body_Struct(Main2ApControllerMsgType.gsm_msg_recv);
            TypeKeyValue.dic.Add("sys", recv.bCellIdx);
            TypeKeyValue.dic.Add("hardware_id", 0);

            Name_DIC_Struct nDic = new Name_DIC_Struct();
            nDic.name = Recv_Msg_Id.CONTROL_FAP_RESET_MSG.ToString();

            //nDic.dic.Add("rfStatus", GetValueByString_Byte(ref data).ToString());

            TypeKeyValue.n_dic.Add(nDic);

            OnSendMsg2Main(recv.wSqn, MsgType.CONFIG, apToKen, TypeKeyValue);
        }

        private void Send2Main_CONFIG_CDMA_CARRIER_MSG(AsyncUserToken apToKen, MsgRecvStruct recv)
        {
            String data = recv.data;
            Msg_Body_Struct TypeKeyValue = new Msg_Body_Struct(Main2ApControllerMsgType.ReportGenPara);
            TypeKeyValue.dic.Add("sys", recv.bCellIdx);
            TypeKeyValue.dic.Add("hardware_id", 0);

            Name_DIC_Struct nDic = new Name_DIC_Struct();
            nDic.name = Recv_Msg_Id.CONFIG_CDMA_CARRIER_MSG.ToString();

            nDic.dic.Add("wARFCN1", GetValueByString_U16(ref data).ToString());
            nDic.dic.Add("bARFCN1Mode", GetValueByString_Byte(ref data).ToString());
            GetValue_Reserved(1, ref data);
            nDic.dic.Add("wARFCN1Duration", GetValueByString_U16(ref data).ToString());
            nDic.dic.Add("wARFCN1Period", GetValueByString_U16(ref data).ToString());

            nDic.dic.Add("wARFCN2", GetValueByString_U16(ref data).ToString());
            nDic.dic.Add("bARFCN2Mode", GetValueByString_Byte(ref data).ToString());
            GetValue_Reserved(1, ref data);
            nDic.dic.Add("wARFCN2Duration", GetValueByString_U16(ref data).ToString());
            nDic.dic.Add("wARFCN2Period", GetValueByString_U16(ref data).ToString());

            nDic.dic.Add("wARFCN3", GetValueByString_U16(ref data).ToString());
            nDic.dic.Add("bARFCN3Mode", GetValueByString_Byte(ref data).ToString());
            GetValue_Reserved(1, ref data);
            nDic.dic.Add("wARFCN3Duration", GetValueByString_U16(ref data).ToString());
            nDic.dic.Add("wARFCN3Period", GetValueByString_U16(ref data).ToString());

            nDic.dic.Add("wARFCN4", GetValueByString_U16(ref data).ToString());
            nDic.dic.Add("bARFCN4Mode", GetValueByString_Byte(ref data).ToString());
            GetValue_Reserved(1, ref data);
            nDic.dic.Add("wARFCN4Duration", GetValueByString_U16(ref data).ToString());
            nDic.dic.Add("wARFCN4Period", GetValueByString_U16(ref data).ToString());


            TypeKeyValue.n_dic.Add(nDic);

            OnSendMsg2Main(recv.wSqn, MsgType.CONFIG, apToKen, TypeKeyValue);
        }

        private void Send2Main_CONFIG_IMSI_MSG_V3_ID(AsyncUserToken apToKen, MsgRecvStruct recv)
        {
            String data = recv.data;
            Msg_Body_Struct TypeKeyValue = new Msg_Body_Struct(Main2ApControllerMsgType.ReportGenPara);
            TypeKeyValue.dic.Add("sys", recv.bCellIdx);
            TypeKeyValue.dic.Add("hardware_id", 0);

            Name_DIC_Struct nDic = new Name_DIC_Struct();
            nDic.name = Recv_Msg_Id.CONFIG_IMSI_MSG_V3_ID.ToString();

            nDic.dic.Add("wTotalImsi", GetValueByString_U16(ref data).ToString());
            Byte bIMSINum = GetValueByString_Byte(ref data);
            nDic.dic.Add("bIMSINum", bIMSINum.ToString());
            if (bIMSINum < 0 || bIMSINum > 50)
            {
                OnOutputLog(LogInfoType.WARN, string.Format("IMSI个数错误，上报个数为{0}。", bIMSINum));
                return;
            }
            nDic.dic.Add("bSegmentType", GetValueByString_Byte(ref data).ToString());
            nDic.dic.Add("bSegmentID", GetValueByString_Byte(ref data).ToString());
            nDic.dic.Add("bActionType", GetValueByString_Byte(ref data).ToString());
            GetValue_Reserved(2, ref data);

            for (int i = 0; i < bIMSINum; i++)
            {
                nDic.dic.Add(string.Format("bIMSI_#{0}#",i), 
                    StringDelZero(GetValueByString_String(30,ref data).ToString()));
                //nDic.dic.Add(string.Format("bUeActionFlag_#{0}#", i), GetValueByString_Byte(ref data).ToString());
            }

            for (int i = bIMSINum; i < 50; i++)
            {
                GetValue_Reserved(15, ref data);
            }

            for (int i = 0; i < bIMSINum; i++)
            {
                //nDic.dic.Add(string.Format("bIMSI_#{0}#", i), GetValueByString_String(30, ref data).ToString());
                nDic.dic.Add(string.Format("bUeActionFlag_#{0}#", i), GetValueByString_Byte(ref data).ToString());
            }

            for (int i = bIMSINum; i < 50; i++)
            {
                GetValue_Reserved(1, ref data);
            }

            TypeKeyValue.n_dic.Add(nDic);

            OnSendMsg2Main(recv.wSqn, MsgType.CONFIG, apToKen, TypeKeyValue);
        }

        #endregion

        #endregion

    }
}
