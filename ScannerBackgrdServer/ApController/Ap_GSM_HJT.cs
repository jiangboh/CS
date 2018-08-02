using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;
using ScannerBackgrdServer.Common;
using static ScannerBackgrdServer.Common.MsgStruct;
using static ScannerBackgrdServer.Common.Xml_codec;

namespace ScannerBackgrdServer.ApController
{
    class Ap_GSM_HJT : ApBase
    {
        #region 消息类型及结据结构定义

        enum Gsm_Device_Sys
        {
            Sys1 = 0,
            Sys2
        }
        /// <summary>
        /// 控制端发送给设备的消息类型
        /// </summary>
        enum Gsm_Send_Msg_Type
        {
            RECV_SYS_PARA = 0x01,     //系统参数，在基本参数配置界面中，包括MCC、MNC等
            RECV_SYS_OPTION,                    //系统选项，在可选配置界面中，如是否上报IMEI等
            RECV_DLRX_PARA,                 //下行接收参数，在测试界面中，用于获取网络信息
            RECV_RF_PARA,                   //射频参数，在基本参数配置界面中，如频点、功率等
            RECV_QUERY_VER = 0x05,        //查询版本，在测试界面中（暂不用）
            RECV_TEST_CMD,                  //测试命令，在测试界面中
            RECV_SMS_RPOA,                  //短消息中心号码，在短信配置界面中
            RECV_SMS_TPOA,                  //短消息原叫号码，在短信配置界面中
            RECV_SMS_SCTS,                  //短消息发送时间，在短信配置界面中
            RECV_SMS_DATA = 10,         //短消息编辑内容，在短信配置界面中
            RECV_BS_SEAR,                   //基站搜索，在基本参数配置界面中
            RECV_SMS_SEND,                  //发送短信（暂不用）
            RECV_SMS_STOP,                  //停止短信（暂不用）
            RECV_QUERY_RSP,                 //查询响应（暂不用）
            RECV_LIBRARY_REG_ADD = 15,          //登录控制库中添加IMSI
            RECV_LIBRARY_REG_DELALL,            //登录控制库清空
            RECV_LIBRARY_REG_QUERY,         //登录控制库中查询，返回所有库中IMSI
            RECV_LIBRARY_SMS_ADD,           //短信控制库中添加IMSI
            RECV_LIBRARY_SMS_DELALL,            //短信控制库清空
            RECV_LIBRARY_SMS_QUERY = 20,        //短信控制库中查询，返回所有库中IMSI
            RECV_NCELLTAB_SET,              //配置邻区表
            RECV_NCELLTAB_DEL,                //清空邻区表
            RECV_CHNUM,                 //配置进程数（暂不用）
            RECV_REREG,                     //重新获取
            RECV_BS_CALL_END = 25,         //BS挂机
            RECV_BS_CALL_CON,              //BS接听
            RECV_MT_CALL,                   //设备电话主叫，需要被呼叫的IMSI号，原叫号码
            RECV_MT_SMS,                    //设备短信主叫，需要被呼叫的IMSI号，其他在短信界面配置
            RECV_MT_CALL_SILENCE,               //设备连续安全电话主叫，需要被呼叫的IMSI号，呼叫周期
            RECV_MT_SMS_SILENCE = 30,           //设备连续安全短信主叫，需要被呼叫的IMSI号，呼叫周期（暂不用）
            RECV_ANT_DIREC,                 //使用开关控制4个方向性定位天线
            RECV_TALK_PARA,                 //设置通话参数，包括频点和手机发射功率
            RECV_REG_MODE,                   //手机注册时的工作模式
            RECV_SMS_DATA_SN = 35,               //长短信内容分段序号  (文档中用的34，实际中要用35)
            RECV_SMS_OPTION,                   //未在文档中，CS软件内部定义
            SEND_MAX
        };
        /// <summary>
        /// 设备发送给控制端的消息类型
        /// </summary>
        enum Gsm_Recv_Msg_Type
        {
            SEND_REQ_CNF = 0x01,      //确认接收到的请求，在状态栏显示
            SEND_OM_INFO,                   //设备OM信息，在状态栏显示，每60s发送一次
            SEND_VER_INFO,                  //设备版本信息，在状态栏显示
            SEND_UE_INFO,                   //用户设备信息，在上报信息界面显示，包括IMSI、IMEI等
            SEND_TEST_INFO = 0x05	,	//返回测试信息，在测试界面显示
            SEND_BS_INFO,                   //返回基站信息，在配置页显示
            SEND_QUERY_REQ,             //查询请求
            SEND_LIBRARY_REG,                //登录库返回信息
            SEND_LIBRARY_SMS,                //SMS库返回信息
            SEND_OBJECT_POWER = 0x0A ,   //目标功率
            SEND_BS_SEAR_INFO,              //基站搜索到的信息
            SEND_MS_CALL_SETUP,              //手机主动发起呼叫，包括其IMSI，被叫号码
            SEND_MS_SMS_SEND,              //手机主动发起短信，包括其IMSI，被叫号码，短信内容
            SEND_MS_CALL_OPERATE,          //手机在被呼叫时的操作，可以是挂机、摘机或未操作而超时。
            SEND_MAX
        };

        /// <summary>
        /// 发送给设备的消息结构
        /// </summary>
        private struct gsm_msg_send
        {
            public string head;            //头部标识 0xAAAA
            public string addr;            //地址，1表示设备收
            public Gsm_Device_Sys sys;     //系统号，0表示系统1或通道1或射频1，1表示系统2或通道2或射频2
            public Gsm_Send_Msg_Type type; //消息类型
            public Byte data_length;      //消息数据长度   [数据长度是消息ID和消息数据长度的总和]
            public UInt16 message_id;     //消息ID         [是上位机每发一个消息的递增序号，可以总是填充全0]
            public string data;	        //消息数据

            public gsm_msg_send(Gsm_Send_Msg_Type type, Gsm_Device_Sys sys,string data)
            {
                this.head = "AAAA";
                this.addr = "01";
                this.sys = sys;
                this.type = type;
                this.message_id = ApMsgIdClass.addNormalMsgId();
                this.data_length = (byte)(2 + (data.Replace(" ", "").Length / 2));
                this.data = data;
            }
            public gsm_msg_send(Gsm_Send_Msg_Type type, Gsm_Device_Sys sys,UInt16 id,string data)
            {
                this.head = "AAAA";
                this.addr = "01";
                this.sys = sys;
                this.type = type;
                this.message_id = id;
                this.data_length = (byte)(2 + (data.Replace(" ","").Length/2));
                this.data = data;
            }
        }

        /// <summary>
        /// 接收设备消息结构
        /// </summary>
        private struct gsm_msg_recv
        {
            public string head;          //头部标识 0xAAAA
            public Byte addr;         //地址，0表示设备发
            public Gsm_Device_Sys sys;              //系统号，0表示系统1或通道1或射频1，1表示系统2或通道2或射频2
            public Gsm_Recv_Msg_Type type;         //消息类型
            public Byte data_length;      //消息数据长度  [数据长度是硬件ID、消息ID和消息数据的总和]
            public UInt32 hardware_id;   //硬件ID    　[硬件ID是每个硬件的固有编号]
            public UInt16 message_id;    //消息ID   　  [它是设备接收到消息ID的返回值，与接收到的值一样]
            public string data;		//消息数据
        }
        // <summary>
        /// 接收设备消息结构
        /// </summary>
        private struct ack_msg_recv
        {
            public string head;          //头部标识 0xAAAA
            public Byte addr;         //地址，0表示设备发
            public Gsm_Device_Sys sys;              //系统号，0表示系统1或通道1或射频1，1表示系统2或通道2或射频2
            public Gsm_Send_Msg_Type type;         //消息类型
            public Byte data_length;      //消息数据长度  [数据长度是硬件ID、消息ID和消息数据的总和]
            public UInt32 hardware_id;   //硬件ID    　[硬件ID是每个硬件的固有编号]
            public UInt16 message_id;    //消息ID   　  [它是设备接收到消息ID的返回值，与接收到的值一样]
            public string data;		//消息数据
        }
        // <summary>
        /// 接收设备消息结构
        /// </summary>
        private struct ack_msg_ms_recv
        {
            public string head;          //头部标识 0xAAAA
            public Byte addr;         //地址，0表示设备发
            public Gsm_Device_Sys sys;              //系统号，0表示系统1或通道1或射频1，1表示系统2或通道2或射频2
            public Gsm_Send_Msg_Type type;         //消息类型
            public Byte data_length;      //消息数据长度  [数据长度是硬件ID、消息ID和消息数据的总和]
            public UInt32 hardware_id;   //硬件ID    　[硬件ID是每个硬件的固有编号]
            public UInt16 message_id;    //消息ID   　  [它是设备接收到消息ID的返回值，与接收到的值一样]
            public string gSmsRpoa;		//消息数据（短消息中心号码）
            public string gSmsTpoa;		//消息数据（短消息原叫号码）
            public string gSmsScts;		//消息数据（短消息发送时间）
            public string gSmsData;		//消息数据（短消息内容）
            public string enType;		//消息数据（短消息编码方式）
        }
        /// <summary>
        /// 系统参数数据结构
        /// </summary>
        private struct RecvSysPara                  
        {
            public UInt16 paraMcc;
            public UInt16 paraMnc;
            public Byte paraBsic;
            public UInt16 paraLac;
            public UInt16 paraCellId;
            public SByte paraC2;
            public Byte paraPeri;
            public Byte paraAccPwr;
            public Byte paraMsPwr;
            public Byte paraRejCau;
        }
        /// <summary>
        /// 系统选项数据结构
        /// </summary>
        private struct RecvSysOption
        {
            public Byte opLuSms;
            public Byte opLuImei;
            public Byte opCallEn;
            public Byte opDebug;
            public Byte opLuType;
            public Byte opSmsType;
        }
        /// <summary>
        /// 短信设置数据结构
        /// </summary>
        private struct RecvSmsOption
        {
            public string gSmsRpoa;
            public string gSmsTpoa;
            public string gSmsScts;
            public string gSmsData;
            public Byte autoSendtiny;
            public Byte autoFilterSMStiny;
            public UInt16 delayTime;
            public Byte smsCodingtiny;
        }
        /// <summary>
        /// 射频参数数据结构
        /// </summary>
        private struct RecvRfOption
        {
            public Byte rfEnable;
            public UInt16 rfFreq;
            public Byte rfPwr;
        }

        // <summary>
        /// 接收设备消息结构
        /// </summary>
        private struct para_rsp_msg_recv
        {
            public string sys_para_0;		
            public string sys_para_1;		
            public string sys_option_0;		
            public string sys_option_1;
            public string sys_workMode_0;
            public string sys_workMode_1;
            public string sys_rf_0;
            public string sys_rf_1;
        }

        private string GetValueByString_String(int len,ref string data)
        {
            if (data.Length < len)
            {
                data = String.Empty;
                return String.Empty;
            }
            string value = data.Substring(0,len);
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

        #endregion

        #region 类参数定义及构造函数
        public static uint heartbeatMsgNum = 0;
        public static uint imsiMsgNum = 0;

        private string MODE_NAME = ApInnerType.GSM.ToString();

        public Ap_GSM_HJT()
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
        private bool DecodeGsmMsg(ref gsm_msg_recv recv, string msg_data)
        {
            return DecodeGsmMsg(false,ref recv,msg_data);
        }
        /// <summary>
        /// 解析收到的GSM消息
        /// </summary>
        /// <param name="recvFlag">设备发消息类型</param>
        /// <param name="recv">解析后的消息内容</param>
        /// <param name="msg_data">收到的消息</param>
        /// <returns>解析是否成功</returns>
        private bool DecodeGsmMsg(bool recvFlag,ref gsm_msg_recv recv, string msg_data)
        {
            //string head = msg_data.Substring(0,4);
            recv.head = GetValueByString_String(4, ref msg_data);
            if (!recv.head.Equals("AAAA"))
            {
                OnOutputLog(LogInfoType.EROR, "解析GSM消息格式错误，head字段错误！");
                return false;
            }

            //Byte addr = Convert.ToByte(msg_data.Substring(4, 2),16);
            recv.addr = GetValueByString_Byte(ref msg_data);
            if (recv.addr != 0)
            {
                OnOutputLog(LogInfoType.EROR, "解析GSM消息格式错误，addr字段不为0(设备发)！");
                return false;
            }
            //Byte sys = Convert.ToByte(msg_data.Substring(6, 2),16);
            Byte sys = GetValueByString_Byte(ref msg_data);
            if ((sys != 0) && (sys != 1))
            {
                OnOutputLog(LogInfoType.EROR, "解析GSM消息格式错误，sys字段不为0或1(0表示系统1或通道1或射频1，1表示系统2或通道2或射频2)！");
                return false;
            }
            recv.sys = (Gsm_Device_Sys)sys;

            //Byte type = Convert.ToByte(msg_data.Substring(8, 2),16);
            Byte type = GetValueByString_Byte(ref msg_data);
            if (recvFlag)
            {
                if (type < (Byte)Gsm_Recv_Msg_Type.SEND_REQ_CNF || type >= (Byte)Gsm_Recv_Msg_Type.SEND_MAX)
                {
                    OnOutputLog(LogInfoType.EROR, "解析GSM消息格式错误，type字段错误！");
                    return false;
                }
            }
            else
            {
                if (type < (Byte)Gsm_Send_Msg_Type.RECV_SYS_PARA || type >= (Byte)Gsm_Send_Msg_Type.SEND_MAX)
                {
                    OnOutputLog(LogInfoType.EROR, "解析GSM消息格式错误，type字段错误！");
                    return false;
                }
            }
            recv.type = (Gsm_Recv_Msg_Type)type;

            //int data_length = Convert.ToInt16(msg_data.Substring(10, 2))-6;
            recv.data_length = GetValueByString_Byte(ref msg_data);
            recv.data_length -= 6;//-6为去掉hardware_id和message_id后的净数据长度
            if (recv.data_length < 0)
            {
                OnOutputLog(LogInfoType.EROR, "解析GSM消息格式错误，data_length字段错误！");
                return false;
            }


            //UInt32 hardware_id = Convert.ToUInt32(msg_data.Substring(12, 8),16);
            recv.hardware_id = GetValueByString_U32(ref msg_data);
            if (recv.hardware_id <= 0)
            {
                OnOutputLog(LogInfoType.EROR, "解析GSM消息格式错误，hardware_id字段错误！");
                return false;
            }

            //UInt16 message_id = Convert.ToUInt16(msg_data.Substring(20, 4),16);
            recv.message_id = GetValueByString_U16(ref msg_data);
            if (recv.message_id < 0)
            {
                OnOutputLog(LogInfoType.EROR, "解析GSM消息格式错误，message_id字段错误！");
                return false;
            }

            recv.data = string.Empty;
            if (recv.data_length > 0)
            {
                //data = msg_data.Substring(24, data_length*2);
                recv.data = GetValueByString_String(recv.data_length * 2, ref msg_data);
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

                //UInt32 oldDetail = token.Detail;

                //UInt32 detail = 0;
                //string sDetail = GetMsgStringValueInList("detail", msgBody);
                //if (!string.IsNullOrEmpty(sDetail))
                //    detail = Convert.ToUInt32(sDetail, 16);

                apToKen.Mode = GetMsgStringValueInList("mode", msgBody);
                apToKen.Sn = GetMsgStringValueInList("sn", msgBody);
                apToKen.FullName = GetMsgStringValueInList("fullname", msgBody);
                apToKen.Id = GetMsgStringValueInList("id", msgBody);
                //token.Detail = detail;

                int i = MyDeviceList.add(apToKen);
                Send2main_OnOffLine("OnLine", i, apToKen);

                //判断是周期心跳，还是上线心跳
                //if ((detail & (int)AP_STATUS.OnLine) > 0) //上线
                //{
                //    OnOutputLog(LogInfoType.DEBG, "上线消息");
                //    Send2ap_status_request(token);
                //}
                //else //周期心跳
                //{
                //    OnOutputLog(LogInfoType.DEBG, "周期心跳消息");
                //}
                ////发送状态改变
                //Send2ap_ApStatusChange(token, oldDetail);
            }
            else if (msgBody.type == ApMsgType.agent_transmit_gsm_msg )
            {
                gsm_msg_recv recv = new gsm_msg_recv();
                string msg_data = GetMsgStringValueInList("data", msgBody);
                msg_data = msg_data.Replace(" ", "");
                if (string.IsNullOrEmpty(msg_data))
                {
                    OnOutputLog(LogInfoType.EROR, "收到XML消息格式错误，XML中data字段为空！");
                    return;
                }
                if (msg_data.Length < 24)
                {
                    OnOutputLog(LogInfoType.EROR, "收到XML消息格式错误，XML中data字段长度过短！");
                    return;
                }

                if (!DecodeGsmMsg(true,ref recv, msg_data))
                {
                    OnOutputLog(LogInfoType.EROR, "收到XML消息格式错误！");
                    return;
                }

                HandleGsmMsg(apToKen, recv);
              
            }
            else if (msgBody.type == ApMsgType.agent_transmit_ack_msg)
            {
                ack_msg_recv recv = new ack_msg_recv();
                string msg_data = GetMsgStringValueInList("data", msgBody);
                msg_data = msg_data.Replace(" ", "");
                if (string.IsNullOrEmpty(msg_data))
                {
                    OnOutputLog(LogInfoType.EROR, "收到XML消息格式错误，XML中data字段为空！");
                    return;
                }
                if (msg_data.Length < 24)
                {
                    OnOutputLog(LogInfoType.EROR, "收到XML消息格式错误，XML中data字段长度过短！");
                    return;
                }
                //string head = msg_data.Substring(0,4);
                recv.head = GetValueByString_String(4, ref msg_data);
                if (!recv.head.Equals("AAAA"))
                {
                    OnOutputLog(LogInfoType.EROR, "解析GSM消息格式错误，head字段错误！");
                    return;
                }

                //Byte addr = Convert.ToByte(msg_data.Substring(4, 2),16);
                recv.addr = GetValueByString_Byte(ref msg_data);
                if (recv.addr != 0 && recv.addr != 1)
                {
                    OnOutputLog(LogInfoType.EROR, "解析GSM消息格式错误，addr字段不为0(设备发)！");
                    return;
                }
                //Byte sys = Convert.ToByte(msg_data.Substring(6, 2),16);
                Byte sys = GetValueByString_Byte(ref msg_data);
                if ((sys != 0) && (sys != 1))
                {
                    OnOutputLog(LogInfoType.EROR, "解析GSM消息格式错误，sys字段不为0或1(0表示系统1或通道1或射频1，1表示系统2或通道2或射频2)！");
                    return;
                }
                recv.sys = (Gsm_Device_Sys)sys;

                //Byte type = Convert.ToByte(msg_data.Substring(8, 2),16);
                Byte type = GetValueByString_Byte(ref msg_data);
                if (type < (Byte)Gsm_Send_Msg_Type.RECV_SYS_PARA || type >= (Byte)Gsm_Send_Msg_Type.SEND_MAX)
                {
                    OnOutputLog(LogInfoType.EROR, "解析GSM消息格式错误，type字段错误！");
                    return;
                }
                recv.type = (Gsm_Send_Msg_Type)type;

                //int data_length = Convert.ToInt16(msg_data.Substring(10, 2))-6;
                recv.data_length = GetValueByString_Byte(ref msg_data);
                recv.data_length -= 6;//-6为去掉hardware_id和message_id后的净数据长度
                if (recv.data_length < 0)
                {
                    OnOutputLog(LogInfoType.EROR, "解析GSM消息格式错误，data_length字段错误！");
                    return;
                }


                //UInt32 hardware_id = Convert.ToUInt32(msg_data.Substring(12, 8),16);
                recv.hardware_id = GetValueByString_U32(ref msg_data);
                if (recv.hardware_id <= 0)
                {
                    OnOutputLog(LogInfoType.EROR, "解析GSM消息格式错误，hardware_id字段错误！");
                    return;
                }

                //UInt16 message_id = Convert.ToUInt16(msg_data.Substring(20, 4),16);
                recv.message_id = GetValueByString_U16(ref msg_data);
                if (recv.message_id < 0)
                {
                    OnOutputLog(LogInfoType.EROR, "解析GSM消息格式错误，message_id字段错误！");
                    return;
                }

                recv.data = string.Empty;
                if (recv.data_length > 0)
                {
                    //data = msg_data.Substring(24, data_length*2);
                    recv.data = GetValueByString_String(recv.data_length * 2, ref msg_data);
                    if (string.IsNullOrEmpty(recv.data))
                    {
                        OnOutputLog(LogInfoType.EROR, "解析GSM消息格式错误，data字段错误！");
                        return;
                    }
                }

                HandleAckMsg(apToKen, recv);

            }
            else if (msgBody.type == ApMsgType.agent_transmit_ack_msg_ms)
            {
                ack_msg_ms_recv recv = new ack_msg_ms_recv();
                //内容格式为设备收的格式。而不是设备发的格式
                string gSmsData = GetMsgStringValueInList("gSmsData", msgBody);
                gSmsData = gSmsData.Replace(" ", "");
                if (string.IsNullOrEmpty(gSmsData))
                {
                    OnOutputLog(LogInfoType.EROR, "收到XML消息格式错误，XML中gSmsData字段为空！");
                    return;
                }
                if (gSmsData.Length < 24)
                {
                    OnOutputLog(LogInfoType.EROR, "收到XML消息格式错误，XML中gSmsData字段长度过短！");
                    return;
                }
                //string head = msg_data.Substring(0,4);
                recv.head = GetValueByString_String(4, ref gSmsData);
                if (!recv.head.Equals("AAAA"))
                {
                    OnOutputLog(LogInfoType.EROR, "解析GSM消息格式错误，head字段错误！");
                    return;
                }

                //Byte addr = Convert.ToByte(msg_data.Substring(4, 2),16);
                recv.addr = GetValueByString_Byte(ref gSmsData);
                if (recv.addr != 0 && recv.addr != 1)  //兼容AGENT此处回复错误
                {
                    OnOutputLog(LogInfoType.EROR, "解析GSM消息格式错误，addr字段不为0(设备发)！");
                    return;
                }
                //Byte sys = Convert.ToByte(msg_data.Substring(6, 2),16);
                Byte sys = GetValueByString_Byte(ref gSmsData);
                if ((sys != 0) && (sys != 1))
                {
                    OnOutputLog(LogInfoType.EROR, "解析GSM消息格式错误，sys字段不为0或1(0表示系统1或通道1或射频1，1表示系统2或通道2或射频2)！");
                    return;
                }
                recv.sys = (Gsm_Device_Sys)sys;

                //Byte type = Convert.ToByte(msg_data.Substring(8, 2),16);
                Byte type = GetValueByString_Byte(ref gSmsData);
                if (type < (Byte)Gsm_Send_Msg_Type.RECV_SYS_PARA || type >= (Byte)Gsm_Send_Msg_Type.SEND_MAX)
                {
                    OnOutputLog(LogInfoType.EROR, "解析GSM消息格式错误，type字段错误！");
                    return;
                }
                recv.type = (Gsm_Send_Msg_Type)type;

                //int data_length = Convert.ToInt16(msg_data.Substring(10, 2))-6;
                recv.data_length = GetValueByString_Byte(ref gSmsData);
                recv.data_length -= 6;//-6为去掉message_id后的净数据长度
                if (recv.data_length < 0)
                {
                    OnOutputLog(LogInfoType.EROR, "解析GSM消息格式错误，data_length字段错误！");
                    return;
                }

                //recv.hardware_id = 0; //该消息未返回硬件id
                recv.hardware_id = GetValueByString_U32(ref gSmsData);
                if (recv.hardware_id <= 0)
                {
                    OnOutputLog(LogInfoType.EROR, "解析GSM消息格式错误，hardware_id字段错误！");
                    return;
                }

                recv.message_id = GetValueByString_U16(ref gSmsData);
                if (recv.message_id < 0)
                {
                    OnOutputLog(LogInfoType.EROR, "解析GSM消息格式错误，message_id字段错误！");
                    return;
                }

                recv.gSmsData = string.Empty;
                if (recv.data_length > 0)
                {
                    recv.gSmsData = GetValueByString_String(recv.data_length * 2, ref gSmsData);
                    if (string.IsNullOrEmpty(recv.gSmsData))
                    {
                        OnOutputLog(LogInfoType.EROR, "解析GSM消息格式错误，gSmsData字段错误！");
                        return;
                    }
                }

                string gSmsRpoa = GetMsgStringValueInList("gSmsRpoa", msgBody);
                gSmsRpoa = gSmsRpoa.Replace(" ", "");
                if (string.IsNullOrEmpty(gSmsRpoa))
                {
                    OnOutputLog(LogInfoType.EROR, "收到XML消息格式错误，XML中gSmsRpoa字段为空！");
                    return;
                }
                if (gSmsRpoa.Length < 24)
                {
                    OnOutputLog(LogInfoType.EROR, "收到XML消息格式错误，XML中gSmsRpoa字段长度过短！");
                    return;
                }
                GetValueByString_String(10, ref gSmsRpoa);
                recv.data_length = GetValueByString_Byte(ref gSmsRpoa);
                recv.data_length -= 6;//-6为去掉message_id后的净数据长度
                GetValueByString_String(12, ref gSmsRpoa);
                recv.gSmsRpoa  = string.Empty;
                if (recv.data_length > 0)
                {
                    recv.gSmsRpoa = GetValueByString_String(recv.data_length * 2, ref gSmsRpoa);
                    if (string.IsNullOrEmpty(recv.gSmsRpoa))
                    {
                        OnOutputLog(LogInfoType.EROR, "解析GSM消息格式错误，gSmsRpoa字段错误！");
                        return;
                    }
                }

                string gSmsTpoa = GetMsgStringValueInList("gSmsTpoa", msgBody);
                gSmsTpoa = gSmsTpoa.Replace(" ", "");
                if (string.IsNullOrEmpty(gSmsTpoa))
                {
                    OnOutputLog(LogInfoType.EROR, "收到XML消息格式错误，XML中gSmsTpoa字段为空！");
                    return;
                }
                if (gSmsTpoa.Length < 24)
                {
                    OnOutputLog(LogInfoType.EROR, "收到XML消息格式错误，XML中gSmsTpoa字段长度过短！");
                    return;
                }
                GetValueByString_String(10, ref gSmsTpoa);
                recv.data_length = GetValueByString_Byte(ref gSmsTpoa);
                recv.data_length -= 6;//-6为去掉message_id后的净数据长度
                GetValueByString_String(12, ref gSmsTpoa);
                recv.gSmsTpoa = string.Empty;
                if (recv.data_length > 0)
                {
                    recv.gSmsTpoa = GetValueByString_String(recv.data_length * 2, ref gSmsTpoa);
                    if (string.IsNullOrEmpty(recv.gSmsTpoa))
                    {
                        OnOutputLog(LogInfoType.EROR, "解析GSM消息格式错误，gSmsTpoa字段错误！");
                        return;
                    }
                }

                string gSmsScts = GetMsgStringValueInList("gSmsScts", msgBody);
                gSmsScts = gSmsScts.Replace(" ", "");
                if (string.IsNullOrEmpty(gSmsScts))
                {
                    OnOutputLog(LogInfoType.EROR, "收到XML消息格式错误，XML中gSmsScts字段为空！");
                    return;
                }
                if (gSmsScts.Length < 24)
                {
                    OnOutputLog(LogInfoType.EROR, "收到XML消息格式错误，XML中gSmsScts字段长度过短！");
                    return;
                }
                GetValueByString_String(10, ref gSmsScts);
                recv.data_length = GetValueByString_Byte(ref gSmsScts);
                recv.data_length -= 6;//-6为去掉message_id后的净数据长度
                GetValueByString_String(12, ref gSmsScts);
                recv.gSmsScts = string.Empty;
                if (recv.data_length > 0)
                {
                    recv.gSmsScts = GetValueByString_String(recv.data_length * 2, ref gSmsScts);
                    if (string.IsNullOrEmpty(recv.gSmsScts))
                    {
                        OnOutputLog(LogInfoType.EROR, "解析GSM消息格式错误，gSmsScts字段错误！");
                        return;
                    }
                }

                string gTestCmd = GetMsgStringValueInList("gTestCmd", msgBody);
                gTestCmd = gTestCmd.Replace(" ", "");
                if (string.IsNullOrEmpty(gTestCmd))
                {
                    OnOutputLog(LogInfoType.EROR, "收到XML消息格式错误，XML中gTestCmd字段为空！");
                    return;
                }
                if (gTestCmd.Length < 24)
                {
                    OnOutputLog(LogInfoType.EROR, "收到XML消息格式错误，XML中gTestCmd字段长度过短！");
                    return;
                }
                GetValueByString_String(10, ref gTestCmd);
                recv.data_length = GetValueByString_Byte(ref gTestCmd);
                recv.data_length -= 6;//-6为去掉message_id后的净数据长度
                GetValueByString_String(12, ref gTestCmd);
                recv.enType = string.Empty;
                if (recv.data_length > 0)
                {
                    recv.enType = GetValueByString_String(recv.data_length * 2, ref gTestCmd);
                    if (string.IsNullOrEmpty(recv.enType))
                    {
                        OnOutputLog(LogInfoType.EROR, "解析GSM消息格式错误，gTestCmd字段错误！");
                        return;
                    }
                }

                HandleAckMsMsg(apToKen, recv);

            }
            else if (msgBody.type == ApMsgType.get_general_para_response)
            {
                para_rsp_msg_recv recv = new para_rsp_msg_recv();

                string sys_para_0 = GetMsgStringValueInList("sys_para_0", msgBody);
                sys_para_0 = sys_para_0.Replace(" ", "");
                if (string.IsNullOrEmpty(sys_para_0))
                {
                    OnOutputLog(LogInfoType.EROR, "收到XML消息格式错误，XML中sys_para_0字段为空！");
                    return;
                }
                if (sys_para_0.Length < 24)
                {
                    OnOutputLog(LogInfoType.EROR, "收到XML消息格式错误，XML中sys_para_0字段长度过短！");
                    return;
                }
                recv.sys_para_0 = sys_para_0;

                string sys_para_1 = GetMsgStringValueInList("sys_para_1", msgBody);
                sys_para_1 = sys_para_1.Replace(" ", "");
                if (string.IsNullOrEmpty(sys_para_1))
                {
                    OnOutputLog(LogInfoType.EROR, "收到XML消息格式错误，XML中sys_para_1字段为空！");
                    return;
                }
                if (sys_para_1.Length < 24)
                {
                    OnOutputLog(LogInfoType.EROR, "收到XML消息格式错误，XML中sys_para_1字段长度过短！");
                    return;
                }
                recv.sys_para_1 = sys_para_1;

                string sys_option_0 = GetMsgStringValueInList("sys_option_0", msgBody);
                sys_option_0 = sys_option_0.Replace(" ", "");
                if (string.IsNullOrEmpty(sys_option_0))
                {
                    OnOutputLog(LogInfoType.EROR, "收到XML消息格式错误，XML中sys_option_0字段为空！");
                    return;
                }
                if (sys_option_0.Length < 24)
                {
                    OnOutputLog(LogInfoType.EROR, "收到XML消息格式错误，XML中sys_option_0字段长度过短！");
                    return;
                }
                recv.sys_option_0 = sys_option_0;

                string sys_option_1 = GetMsgStringValueInList("sys_option_1", msgBody);
                sys_option_1 = sys_option_1.Replace(" ", "");
                if (string.IsNullOrEmpty(sys_option_1))
                {
                    OnOutputLog(LogInfoType.EROR, "收到XML消息格式错误，XML中sys_option_1字段为空！");
                    return;
                }
                if (sys_option_1.Length < 24)
                {
                    OnOutputLog(LogInfoType.EROR, "收到XML消息格式错误，XML中sys_option_1字段长度过短！");
                    return;
                }
                recv.sys_option_1 = sys_option_1;

                string sys_rf_0 = GetMsgStringValueInList("sys_rf_0", msgBody);
                sys_rf_0 = sys_rf_0.Replace(" ", "");
                if (string.IsNullOrEmpty(sys_rf_0))
                {
                    OnOutputLog(LogInfoType.EROR, "收到XML消息格式错误，XML中sys_rf_0字段为空！");
                    return;
                }
                if (sys_rf_0.Length < 24)
                {
                    OnOutputLog(LogInfoType.EROR, "收到XML消息格式错误，XML中sys_rf_0字段长度过短！");
                    return;
                }
                recv.sys_rf_0 = sys_rf_0;

                string sys_rf_1 = GetMsgStringValueInList("sys_rf_1", msgBody);
                sys_rf_1 = sys_rf_1.Replace(" ", "");
                if (string.IsNullOrEmpty(sys_rf_1))
                {
                    OnOutputLog(LogInfoType.EROR, "收到XML消息格式错误，XML中ssys_rf_1字段为空！");
                    return;
                }
                if (sys_rf_1.Length < 24)
                {
                    OnOutputLog(LogInfoType.EROR, "收到XML消息格式错误，XML中sys_rf_1字段长度过短！");
                    return;
                }
                recv.sys_rf_1 = sys_rf_1;

                string sys_workMode_0 = GetMsgStringValueInList("sys_workMode_0", msgBody);
                sys_workMode_0 = sys_workMode_0.Replace(" ", "");
                if (string.IsNullOrEmpty(sys_workMode_0))
                {
                    OnOutputLog(LogInfoType.EROR, "收到XML消息格式错误，XML中sys_workMode_0字段为空！");
                    return;
                }
                if (sys_workMode_0.Length < 24)
                {
                    OnOutputLog(LogInfoType.EROR, "收到XML消息格式错误，XML中sys_workMode_0字段长度过短！");
                    return;
                }
                recv.sys_workMode_0 = sys_workMode_0;

                string sys_workMode_1 = GetMsgStringValueInList("sys_workMode_1", msgBody);
                sys_workMode_1 = sys_workMode_1.Replace(" ", "");
                if (string.IsNullOrEmpty(sys_workMode_1))
                {
                    OnOutputLog(LogInfoType.EROR, "收到XML消息格式错误，XML中sys_workMode_1字段为空！");
                    return;
                }
                if (sys_workMode_1.Length < 24)
                {
                    OnOutputLog(LogInfoType.EROR, "收到XML消息格式错误，XML中sys_workMode_1字段长度过短！");
                    return;
                }
                recv.sys_workMode_1 = sys_workMode_1;

                HandleParaRspMsg(apToKen, recv);
            }
            else
            {
                Msg_Body_Struct body = new Msg_Body_Struct(msgBody.type, msgBody.dic);
                OnSendMsg2Main(msgId, MsgStruct.MsgType.CONFIG, apToKen, body);
            }

            msgBody = null;
        }

        private void HandleAckMsg(AsyncUserToken apToKen, ack_msg_recv recv)
        {
            String data = recv.data;
            if (recv.type == Gsm_Send_Msg_Type.RECV_SYS_PARA) 
            {
                Send2Main_ParaChange_RECV_SYS_PARA(apToKen,recv);
            }
            else if (recv.type == Gsm_Send_Msg_Type.RECV_SYS_OPTION)
            {
                Send2Main_ParaChange_RECV_SYS_OPTION(apToKen, recv);
            }
            else if (recv.type == Gsm_Send_Msg_Type.RECV_RF_PARA)
            {
                Send2Main_ParaChange_RECV_RF_PARA(apToKen, recv);
            }
            else if (recv.type == Gsm_Send_Msg_Type.RECV_REG_MODE)
            {
                Send2Main_ParaChange_RECV_REG_MODE(apToKen, recv);
            }
        }

        private void HandleAckMsMsg(AsyncUserToken apToKen, ack_msg_ms_recv recv)
        {
            ack_msg_ms_recv msg = new ack_msg_ms_recv();
            msg.enType = Convert.ToInt32(recv.enType.Substring(2, 2)).ToString();

            string gSmsTpoa = recv.gSmsTpoa.Substring(2);
            if ("f".Equals(gSmsTpoa.Substring(gSmsTpoa.Length-1)) || "F".Equals(gSmsTpoa.Substring(gSmsTpoa.Length-1)))
            {
                gSmsTpoa = gSmsTpoa.Substring(0, gSmsTpoa.Length -1);
            }
            if ("19".Equals(recv.gSmsTpoa.Substring(0,2)))
            {
                gSmsTpoa = "+" + gSmsTpoa;
            }
            msg.gSmsTpoa = gSmsTpoa;

            string Rphone = recv.gSmsRpoa.Substring(2);
            if ("f".Equals(Rphone.Substring(Rphone.Length-1)) || "F".Equals(Rphone.Substring(Rphone.Length-1)))
            {
                Rphone = Rphone.Substring(0, Rphone.Length - 1);
            }
            if ("19".Equals(recv.gSmsRpoa.Substring(0, 2)))
            {
                Rphone = "+" + Rphone;
            }
            msg.gSmsRpoa = Rphone;

            msg.gSmsScts = recv.gSmsScts;

            if ("0".Equals(msg.enType))
            {
                msg.gSmsData = CodeConver.Unicode2String(recv.gSmsData);
            }
            else if ("1".Equals(msg.enType))
            {
                msg.gSmsData = CodeConver.Decode7Bit(recv.gSmsData);
            }
            else
            {
                msg.gSmsData = string.Empty;
            }

            Send2Main_ParaChange_RECV_SMS_OPTION(apToKen,msg);
        }

        private void HandleGsmMsg(AsyncUserToken apToKen, gsm_msg_recv recv)
        {
            String data = recv.data;
            if (recv.type == Gsm_Recv_Msg_Type.SEND_REQ_CNF) //确认消息
            {
                Send2Main_SEND_REQ_CNF(apToKen,recv);
            }
            else if (recv.type == Gsm_Recv_Msg_Type.SEND_OM_INFO)
            {
                Send2Main_SEND_OM_INFO(apToKen, recv);
            }
            else if (recv.type == Gsm_Recv_Msg_Type.SEND_VER_INFO)
            {
                Send2Main_SEND_VER_INFO(apToKen, recv);
            }
            else if (recv.type == Gsm_Recv_Msg_Type.SEND_UE_INFO)
            {
                Send2Main_SEND_UE_INFO(apToKen, recv);
            }
            else if (recv.type == Gsm_Recv_Msg_Type.SEND_TEST_INFO)
            {
                Send2Main_SEND_TEST_INFO(apToKen, recv);
            }
            else if (recv.type == Gsm_Recv_Msg_Type.SEND_BS_INFO)
            {
                Send2Main_SEND_BS_INFO(apToKen, recv);
            }
            else if (recv.type == Gsm_Recv_Msg_Type.SEND_QUERY_REQ)
            {
                Send2Main_SEND_QUERY_REQ(apToKen, recv);
            }
            else if (recv.type == Gsm_Recv_Msg_Type.SEND_LIBRARY_REG)
            {
                Send2Main_SEND_LIBRARY_REG(apToKen, recv);
            }
            else if (recv.type == Gsm_Recv_Msg_Type.SEND_LIBRARY_SMS)
            {
                Send2Main_SEND_LIBRARY_SMS(apToKen, recv);
            }
            else if (recv.type == Gsm_Recv_Msg_Type.SEND_OBJECT_POWER)
            {
                Send2Main_SEND_OBJECT_POWER(apToKen, recv);
            }
            else if (recv.type == Gsm_Recv_Msg_Type.SEND_BS_SEAR_INFO)
            {
               
            }
            else if (recv.type == Gsm_Recv_Msg_Type.SEND_MS_CALL_SETUP)
            {
                Send2Main_SEND_MS_CALL_SETUP(apToKen, recv);
            }
            else if (recv.type == Gsm_Recv_Msg_Type.SEND_MS_SMS_SEND)
            {
                Send2Main_SEND_MS_SMS_SEND(apToKen, recv);
            }
            else if (recv.type == Gsm_Recv_Msg_Type.SEND_MS_CALL_OPERATE)
            {
                Send2Main_SEND_MS_CALL_OPERATE(apToKen, recv);
            }
        }

        private void HandleParaRspMsg(AsyncUserToken apToKen, para_rsp_msg_recv recv)
        {
            //String data = string.Empty;
            gsm_msg_recv sys_para_0 = new gsm_msg_recv();
            if (!DecodeGsmMsg(ref sys_para_0, recv.sys_para_0))
            {
                OnOutputLog(LogInfoType.EROR, "收到XML消息sys_para_0格式错误！");
                return;
            }

            gsm_msg_recv sys_option_0 = new gsm_msg_recv();
            if (!DecodeGsmMsg(ref sys_option_0, recv.sys_option_0))
            {
                OnOutputLog(LogInfoType.EROR, "收到XML消息sys_option_0格式错误！");
                return;
            }

            gsm_msg_recv sys_rf_0 = new gsm_msg_recv();
            if (!DecodeGsmMsg(ref sys_rf_0, recv.sys_rf_0))
            {
                OnOutputLog(LogInfoType.EROR, "收到XML消息sys_rf_0格式错误！");
                return;
            }

            gsm_msg_recv sys_workMode_0 = new gsm_msg_recv();
            if (!DecodeGsmMsg(ref sys_workMode_0, recv.sys_workMode_0))
            {
                OnOutputLog(LogInfoType.EROR, "收到XML消息sys_workMode_0格式错误！");
                return;
            }

            Send2Main_SEND_GET_PARA_RSP(apToKen, sys_para_0, sys_option_0, sys_rf_0, sys_workMode_0);


            gsm_msg_recv sys_para_1 = new gsm_msg_recv();
            if (!DecodeGsmMsg(ref sys_para_1, recv.sys_para_1))
            {
                OnOutputLog(LogInfoType.EROR, "收到XML消息sys_para_1格式错误！");
                return;
            }

            gsm_msg_recv sys_option_1 = new gsm_msg_recv();
            if (!DecodeGsmMsg(ref sys_option_1, recv.sys_option_1))
            {
                OnOutputLog(LogInfoType.EROR, "收到XML消息sys_option_1格式错误！");
                return;
            }

            gsm_msg_recv sys_rf_1 = new gsm_msg_recv();
            if (!DecodeGsmMsg(ref sys_rf_1, recv.sys_rf_1))
            {
                OnOutputLog(LogInfoType.EROR, "收到XML消息sys_rf_1格式错误！");
                return;
            }

            gsm_msg_recv sys_workMode_1 = new gsm_msg_recv();
            if (!DecodeGsmMsg(ref sys_workMode_1, recv.sys_workMode_1))
            {
                OnOutputLog(LogInfoType.EROR, "收到XML消息sys_workMode_1格式错误！");
                return;
            }

            Send2Main_SEND_GET_PARA_RSP(apToKen,sys_para_1,sys_option_1,sys_rf_1,sys_workMode_1);
        }

        #region 封装回复AP的消息

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
                    msgBody.ApInfo.IP, msgBody.ApInfo.Port.ToString(), msgBody.ApInfo.Fullname);
                OnOutputLog(LogInfoType.WARN, str);
                Send2APP_GeneralError(msgBody.ApInfo, msgBody.AppInfo, msgBody.Body.type, str);
                return;
            }
            MsgId2App msgId2App = new MsgId2App();
            msgId2App.id = ApMsgIdClass.addNormalMsgId();
            msgId2App.AppInfo = msgBody.AppInfo;

            if (MyDeviceList.AddMsgId2App(apToKen, msgId2App))
            {
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

            UInt16 msgId = ApMsgIdClass.addTransparentMsgId();

            MsgId2App msgId2App = new MsgId2App();
            msgId2App.id = msgId;
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
                OnOutputLog(LogInfoType.EROR, string.Format("封装XML消息(Send2ap_TransparentMsg)出错！"));
                Send2APP_GeneralError(msgBody.ApInfo, msgBody.AppInfo, msgBody.Body.type,
                    string.Format("封装向AP发送的XML消息出错！"));
                return;
            }
            sendMsg = sendMsg.Replace(" ","");
            sendMsg = sendMsg.Remove(12, 4);
            sendMsg = sendMsg.Insert(12, string.Format("{0}", msgId.ToString("X").PadLeft(4, '0')));
            sendMsg = Regex.Replace(sendMsg, @".{2}", "$0 ");

            Msg_Body_Struct TypeKeyValue =
                new Msg_Body_Struct(ApMsgType.agent_transmit_gsm_msg,
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
        /// <param name="ApToken">AP信息</param
        /// <param name="msg">GSM文档格式的消息内容，十六进制“AA AA 01 00 15 0E 55 66 00 4B 00 54 02 2A 00 00 00 00 00 00”</param>
        private void Send2ap_GSM(AsyncUserToken ApToken, App_Info_Struct AppInfo, gsm_msg_send sendMsg)
        {
            string data = string.Format("{0}{1}{2}{3}{4}{5}{6}",
                sendMsg.head, sendMsg.addr, ((byte)sendMsg.sys).ToString("X").PadLeft(2, '0'),
                ((byte)sendMsg.type).ToString("X").PadLeft(2, '0'), sendMsg.data_length.ToString("X").PadLeft(2, '0'),
                sendMsg.message_id.ToString("X").PadLeft(4, '0'), sendMsg.data);

            //在两个字符间加上空格
            string sendData = Regex.Replace(data, @".{2}", "$0 ");

            MsgId2App msgId2App = new MsgId2App();
            msgId2App.id = sendMsg.message_id;
            msgId2App.AppInfo = AppInfo;

            if (!MyDeviceList.AddMsgId2App(ApToken, msgId2App))
            {
                OnOutputLog(LogInfoType.EROR, string.Format("添加消息Id到设备列表出错！"));
                Send2APP_GeneralError(ApToken, AppInfo, sendMsg.type.ToString(),
                    string.Format("添加消息Id到设备列表出错！"));
                return;
            }

            Msg_Body_Struct TypeKeyValue =
                new Msg_Body_Struct(ApMsgType.agent_transmit_gsm_msg,
                "data", sendData.Trim());

            byte[] bMsg = EncodeApXmlMessage(sendMsg.message_id, TypeKeyValue);
            if (bMsg == null)
            {
                OnOutputLog(LogInfoType.EROR, string.Format("封装XML消息(Send2ap_GSM)出错！"));
                return;
            }
            SendMsg2Ap(ApToken, bMsg);
        }

        private void Send2ap_RECV_SYS_PARA(AsyncUserToken ApToken, App_Info_Struct AppInfo, Gsm_Device_Sys sys, RecvSysPara para)
        {
            string paraMnc = string.Empty;
            if (para.paraMnc < 0xFF)
            {
                paraMnc = string.Format("0F{0}", para.paraMnc.ToString("X").PadLeft(2, '0'));
            }
            else
            {
                paraMnc = string.Format("{0}", para.paraMnc.ToString("X").PadLeft(4, '0'));
            }

            string data = string.Format("{0}{1}{2}{3}{4}{5}{6}{7}{8}{9}",
                para.paraMcc.ToString("X").PadLeft(4, '0'),
                paraMnc,
                para.paraBsic.ToString("X").PadLeft(2, '0'),
                para.paraLac.ToString("X").PadLeft(4, '0'),
                para.paraCellId.ToString("X").PadLeft(4, '0'),
                para.paraC2.ToString("X").PadLeft(2, '0'),
                para.paraPeri.ToString("X").PadLeft(2, '0'),
                para.paraAccPwr.ToString("X").PadLeft(2, '0'),
                para.paraMsPwr.ToString("X").PadLeft(2, '0'),
                para.paraRejCau.ToString("X").PadLeft(2, '0'));
            Send2ap_GSM(ApToken, AppInfo, new gsm_msg_send(Gsm_Send_Msg_Type.RECV_SYS_PARA, sys, data));
        }

        private void Send2ap_RECV_SYS_OPTION(AsyncUserToken apToken, App_Info_Struct AppInfo, Gsm_Device_Sys sys, RecvSysOption option)
        {
            string data = string.Format("{0}{1}{2}{3}{4}{5}",
                option.opLuSms.ToString("X").PadLeft(2, '0'),
                option.opLuImei.ToString("X").PadLeft(2, '0'),
                option.opCallEn.ToString("X").PadLeft(2, '0'),
                option.opDebug.ToString("X").PadLeft(2, '0'),
                option.opLuType.ToString("X").PadLeft(2, '0'),
                option.opSmsType.ToString("X").PadLeft(2, '0'));
            Send2ap_GSM(apToken, AppInfo, new gsm_msg_send(Gsm_Send_Msg_Type.RECV_SYS_OPTION, sys, data));
        }

        private void Send2ap_RECV_RF_PARA(AsyncUserToken apToken, App_Info_Struct AppInfo, Gsm_Device_Sys sys, RecvRfOption rf)
        {
            string data = string.Format("{0}{1}{2}",
                rf.rfEnable.ToString("X").PadLeft(2, '0'),
                rf.rfFreq.ToString("X").PadLeft(4, '0'),
                rf.rfPwr.ToString("X").PadLeft(2, '0'));
            Send2ap_GSM(apToken, AppInfo, new gsm_msg_send(Gsm_Send_Msg_Type.RECV_RF_PARA, sys, data));
        }

        private void Send2ap_RECV_REG_MODE(AsyncUserToken apToken, App_Info_Struct AppInfo, Gsm_Device_Sys sys, byte mode)
        {
            string data = string.Format("{0}",
                mode.ToString("X").PadLeft(2, '0'));
            Send2ap_GSM(apToken, AppInfo, new gsm_msg_send(Gsm_Send_Msg_Type.RECV_REG_MODE, sys, data));
        }

        private void Send2ap_RECV_LIBRARY_REG_ADD(AsyncUserToken apToken, App_Info_Struct AppInfo, Gsm_Device_Sys sys, string type,string gLibrary)
        {
            string str = gLibrary;
            if (type.Equals("IMSI"))
            {
                str = "809" + str;
            }
            else if(type.Equals("IMEI"))
            {
                str = "80A" + str;
            }
            else if (type.Equals("TMSI"))
            {
                str = "05F4" + str + "00000000";
            }
            string data = string.Format("{0}",str);
            Send2ap_GSM(apToken, AppInfo, new gsm_msg_send(Gsm_Send_Msg_Type.RECV_LIBRARY_REG_ADD, sys, data));
        }

        private void Send2ap_RECV_LIBRARY_REG_DELALL(AsyncUserToken apToken, App_Info_Struct AppInfo, Gsm_Device_Sys sys)
        {
            string data = string.Empty;
            Send2ap_GSM(apToken, AppInfo, new gsm_msg_send(Gsm_Send_Msg_Type.RECV_LIBRARY_REG_DELALL, sys, data));
        }

        private void Send2ap_RECV_LIBRARY_REG_QUERY(AsyncUserToken apToken, App_Info_Struct AppInfo, Gsm_Device_Sys sys)
        {
            string data = string.Empty;
            Send2ap_GSM(apToken, AppInfo, new gsm_msg_send(Gsm_Send_Msg_Type.RECV_LIBRARY_REG_QUERY, sys, data));
        }

        /// <summary>
        /// 发送短消息中心号码
        /// </summary>
        /// <param name="apToken">Ap信息</param>
        /// <param name="sys">系统号</param>
        /// <param name="phone">电话号码</param>
        private void Send2ap_RECV_SMS_RPOA(AsyncUserToken apToken, App_Info_Struct AppInfo, Gsm_Device_Sys sys,UInt16 msgId, string phone)
        {
            //说明
            //    该消息以16进制的方式发送给设备；
            //如果短消息中心号码第一个字符为“+”号，则发送的第一字节为“0x19”，否则发送的第一个字节为“0x18”；
            //不包括“+”号在内的短消息中心号码如果是奇数位，则最后一个字节后4比特(1111)填充“0x * F”；
            //该消息长度应包括第一个字节“0x19”或“0x18”在内；
            //如短消息中心号码为“+8613800270500”，则发送的消息应为16进制的“AA AA 01 00 07 0A 55 66 19 86 13 80 02 70 50 0F”，
            //如短消息中心号码为“13800270500”，则发送的消息应为16进制的“AA AA 01 00 07 09 55 66 18 13 80 02 70 50 0F”；
            //该界面的默认值为“+8613800270500”。

            string puls = "18";
            phone = phone.Replace(" ", "");
            if ("+".Equals(phone.Substring(0, 1)))
            {
                if ((phone.Length % 2) == 0) phone = phone + "F";
                puls = "19";
                phone = phone.Substring(1);
            }
            else
            {
                if ((phone.Length % 2) != 0) phone = phone + "F";
            }

            string data = puls + phone;
            Send2ap_GSM(apToken, AppInfo, new gsm_msg_send(Gsm_Send_Msg_Type.RECV_SMS_RPOA, sys, msgId, data));
        }

        /// <summary>
        /// 发送短消息原叫号码
        /// </summary>
        /// <param name="apToken">Ap信息</param>
        /// <param name="sys">系统号</param>
        /// <param name="phone">电话号码</param>
        private void Send2ap_RECV_SMS_TPOA(AsyncUserToken apToken, App_Info_Struct AppInfo, Gsm_Device_Sys sys,UInt16 msgId, string phone)
        {
            //说明
            //    该消息以16进制的方式发送给设备；
            //如果短消息中心号码第一个字符为“+”号，则发送的第一字节为“0x19”，否则发送的第一个字节为“0x18”；
            //不包括“+”号在内的短消息中心号码如果是奇数位，则最后一个字节后4比特(1111)填充“0x * F”；
            //该消息长度应包括第一个字节“0x19”或“0x18”在内；
            //如短消息中心号码为“+8613800270500”，则发送的消息应为16进制的“AA AA 01 00 07 0A 55 66 19 86 13 80 02 70 50 0F”，
            //如短消息中心号码为“13800270500”，则发送的消息应为16进制的“AA AA 01 00 07 09 55 66 18 13 80 02 70 50 0F”；
            //该界面的默认值为“+8613800270500”。

            string puls = "18";
            phone = phone.Replace(" ", "");
            if ("+".Equals(phone.Substring(0, 1)))
            {
                if ((phone.Length % 2) == 0) phone = phone + "F";
                puls = "19";
                phone = phone.Substring(1);
            }
            else
            {
                if ((phone.Length % 2) != 0) phone = phone + "F";
            }

            string data = puls + phone;
            Send2ap_GSM(apToken, AppInfo, new gsm_msg_send(Gsm_Send_Msg_Type.RECV_SMS_TPOA, sys, msgId, data));
        }

        private void Send2ap_RECV_SMS_OPTION(AsyncUserToken apToken, App_Info_Struct AppInfo, Gsm_Device_Sys sys, RecvSmsOption option)
        {
            int SbuMaxLen = 134;
            Byte EncodeType = option.smsCodingtiny;
            string SMS_DATA_HEX = string.Empty;
            string SMS_DATA = option.gSmsData;

            if (EncodeType == 0)      //Unicode编码
            {
                SMS_DATA_HEX = CodeConver.String2Unicode(SMS_DATA);
            }
            else if (EncodeType == 1) //GSM 7Bit编码
            {
                SMS_DATA_HEX = CodeConver.Encode7Bit(Encoding.Default.GetBytes(SMS_DATA), 0);
            }
            //else if (EncodeType == 2)   //UCS2编码
            //{
            //    for (int i = 0; i < SMS_DATA.Trim().Length; i++)
            //    {
            //        string data = EncodeUCS2(SMS_DATA[i].ToString());
            //        string datainvert = data[2].ToString() + data[3].ToString() + data[0].ToString() + data[1].ToString();
            //        SMS_DATA_HEX += datainvert;
            //    }
            //}

            byte[] smsBody = CodeConver.strToToHexByte(SMS_DATA_HEX);
            int bodyLen = smsBody.Length;
            if (bodyLen <= 0 || bodyLen >= SbuMaxLen * 6)
            {
                string str = string.Format("发送给GSM设备消息({0})错误。消息长度不在(1-{1})范围内。", "RECV_SMS_OPTION", SbuMaxLen * 6);
                OnOutputLog(LogInfoType.WARN, str);
                Send2APP_GeneralError(apToken, AppInfo, "RECV_SMS_OPTION", str);
                return;
            }

            UInt16 msgId = ApMsgIdClass.addNormalMsgId();

            //发送编码方式
            Send2ap_GSM(apToken, AppInfo, new gsm_msg_send(Gsm_Send_Msg_Type.RECV_TEST_CMD, sys, msgId,
                "12" +string.Format("{0}", EncodeType.ToString("X").PadLeft(2, '0'))));

            //如果短信内容经编码后小于等于140个字节，则只分为一段；
            //如果大于140个字节，则需要分段，每段不大于134个字节（67个汉字）。目前设备只支持最大6个分段
            if (bodyLen <= 140)
            {
                //发送短消息分段序号
                Send2ap_GSM(apToken, AppInfo, new gsm_msg_send(Gsm_Send_Msg_Type.RECV_SMS_DATA_SN, sys, msgId, "0101"));
                //发送短消息中心号码
                Send2ap_RECV_SMS_RPOA(apToken,AppInfo,sys, msgId, option.gSmsRpoa);
                //发送短消息原叫号码
                Send2ap_RECV_SMS_TPOA(apToken, AppInfo, sys, msgId, option.gSmsTpoa);
                //短消息发送时间
                Send2ap_GSM(apToken, AppInfo, new gsm_msg_send(Gsm_Send_Msg_Type.RECV_SMS_SCTS, sys, msgId, option.gSmsScts));
                //短消息内容
                Send2ap_GSM(apToken, AppInfo, new gsm_msg_send(Gsm_Send_Msg_Type.RECV_SMS_DATA, sys, msgId, 
                    CodeConver.byteToHexStr(smsBody)));
            }
            else
            {
                int sub =(int)Math.Ceiling((double)bodyLen / SbuMaxLen);
                int i = 1;
                while (i <= sub)
                {
                    //发送短消息分段序号
                    Send2ap_GSM(apToken, AppInfo, new gsm_msg_send(Gsm_Send_Msg_Type.RECV_SMS_DATA_SN, sys, msgId,
                        string.Format("{0}{1}", sub.ToString("X").PadLeft(2, '0'), i.ToString("X").PadLeft(2, '0')) ));
  
                    //发送短消息中心号码
                    Send2ap_RECV_SMS_RPOA(apToken, AppInfo, sys, msgId, option.gSmsRpoa);
                    //发送短消息原叫号码
                    Send2ap_RECV_SMS_TPOA(apToken, AppInfo, sys, msgId, option.gSmsTpoa);
                    //短消息发送时间
                    Send2ap_GSM(apToken, AppInfo, new gsm_msg_send(Gsm_Send_Msg_Type.RECV_SMS_SCTS, sys, msgId, option.gSmsScts));

                    string sms = string.Empty;
                    if (i == sub)
                    {
                        //短消息内容
                        Send2ap_GSM(apToken, AppInfo, new gsm_msg_send(Gsm_Send_Msg_Type.RECV_SMS_DATA, sys, msgId,
                            CodeConver.byteToHexStr(smsBody.Skip((i-1) * SbuMaxLen).ToArray())));
                    }
                    else
                    {
                        //短消息内容
                        Send2ap_GSM(apToken, AppInfo, new gsm_msg_send(Gsm_Send_Msg_Type.RECV_SMS_DATA, sys, msgId,
                            CodeConver.byteToHexStr(smsBody.Skip((i-1) * SbuMaxLen).Take(SbuMaxLen).ToArray())));
                    }
                    i++;
                }
            }
        }

        private void Send2ap_SET_PARA_REQ(AsyncUserToken ApToken, App_Info_Struct AppInfo, Gsm_Device_Sys sys, int Flag,
            RecvSysPara para, RecvSysOption option, RecvRfOption rf,byte mode)
        {
            UInt16 msgId = ApMsgIdClass.addNormalMsgId();

            Msg_Body_Struct TypeKeyValue = new Msg_Body_Struct(ApMsgType.set_general_para_request);

            if ((Flag & 0x01) > 0 )
            {
                string paraMnc = string.Empty;
                if (para.paraMnc < 0xFF)
                {
                    paraMnc = string.Format("0F{0}", para.paraMnc.ToString("X").PadLeft(2, '0'));
                }
                else
                {
                    paraMnc = string.Format("{0}", para.paraMnc.ToString("X").PadLeft(4, '0'));
                }

                string SysParaData = string.Format("{0}{1}{2}{3}{4}{5}{6}{7}{8}{9}",
                    para.paraMcc.ToString("X").PadLeft(4, '0'),
                    paraMnc,
                    para.paraBsic.ToString("X").PadLeft(2, '0'),
                    para.paraLac.ToString("X").PadLeft(4, '0'),
                    para.paraCellId.ToString("X").PadLeft(4, '0'),
                    para.paraC2.ToString("X").PadLeft(2, '0'),
                    para.paraPeri.ToString("X").PadLeft(2, '0'),
                    para.paraAccPwr.ToString("X").PadLeft(2, '0'),
                    para.paraMsPwr.ToString("X").PadLeft(2, '0'),
                    para.paraRejCau.ToString("X").PadLeft(2, '0'));

                gsm_msg_send SysParaSendMsg = new gsm_msg_send(Gsm_Send_Msg_Type.RECV_SYS_PARA, sys, SysParaData);

                string SysPara = string.Format("{0}{1}{2}{3}{4}{5}{6}",
                    SysParaSendMsg.head, SysParaSendMsg.addr, ((byte)SysParaSendMsg.sys).ToString("X").PadLeft(2, '0'),
                    ((byte)SysParaSendMsg.type).ToString("X").PadLeft(2, '0'), SysParaSendMsg.data_length.ToString("X").PadLeft(2, '0'),
                    SysParaSendMsg.message_id.ToString("X").PadLeft(4, '0'), SysParaSendMsg.data);

                //在两个字符间加上空格
                SysPara = Regex.Replace(SysPara, @".{2}", "$0 ");

                TypeKeyValue.dic.Add(string.Format("sys_para_{0}", sys), SysPara.Trim());
            }

            if ((Flag & 0x02) > 0)
            {
                string optionData = string.Format("{0}{1}{2}{3}{4}{5}",
                option.opLuSms.ToString("X").PadLeft(2, '0'),
                option.opLuImei.ToString("X").PadLeft(2, '0'),
                option.opCallEn.ToString("X").PadLeft(2, '0'),
                option.opDebug.ToString("X").PadLeft(2, '0'),
                option.opLuType.ToString("X").PadLeft(2, '0'),
                option.opSmsType.ToString("X").PadLeft(2, '0'));

                gsm_msg_send OptionSendMsg = new gsm_msg_send(Gsm_Send_Msg_Type.RECV_SYS_OPTION, sys, optionData);

                string SysOption = string.Format("{0}{1}{2}{3}{4}{5}{6}",
                    OptionSendMsg.head, OptionSendMsg.addr, ((byte)OptionSendMsg.sys).ToString("X").PadLeft(2, '0'),
                    ((byte)OptionSendMsg.type).ToString("X").PadLeft(2, '0'), OptionSendMsg.data_length.ToString("X").PadLeft(2, '0'),
                    OptionSendMsg.message_id.ToString("X").PadLeft(4, '0'), OptionSendMsg.data);

                //在两个字符间加上空格
                SysOption = Regex.Replace(SysOption, @".{2}", "$0 ");

                TypeKeyValue.dic.Add(string.Format("sys_option_{0}", sys), SysOption.Trim());
            }

            if ((Flag & 0x04) > 0)
            {
                string rfData = string.Format("{0}{1}{2}",
               rf.rfEnable.ToString("X").PadLeft(2, '0'),
               rf.rfFreq.ToString("X").PadLeft(4, '0'),
               rf.rfPwr.ToString("X").PadLeft(2, '0'));

                gsm_msg_send RfSendMsg = new gsm_msg_send(Gsm_Send_Msg_Type.RECV_RF_PARA, sys, rfData);

                string SysRf = string.Format("{0}{1}{2}{3}{4}{5}{6}",
                    RfSendMsg.head, RfSendMsg.addr, ((byte)RfSendMsg.sys).ToString("X").PadLeft(2, '0'),
                    ((byte)RfSendMsg.type).ToString("X").PadLeft(2, '0'), RfSendMsg.data_length.ToString("X").PadLeft(2, '0'),
                    RfSendMsg.message_id.ToString("X").PadLeft(4, '0'), RfSendMsg.data);

                //在两个字符间加上空格
                SysRf = Regex.Replace(SysRf, @".{2}", "$0 ");

                TypeKeyValue.dic.Add(string.Format("sys_rf_{0}", sys), SysRf.Trim());
            }

            if ((Flag & 0x08) > 0)
            {
                string ModeData = string.Format("{0}",
                mode.ToString("X").PadLeft(2, '0'));

                gsm_msg_send ModeSendMsg = new gsm_msg_send(Gsm_Send_Msg_Type.RECV_REG_MODE, sys, ModeData);

                string SysMode = string.Format("{0}{1}{2}{3}{4}{5}{6}",
                    ModeSendMsg.head, ModeSendMsg.addr, ((byte)ModeSendMsg.sys).ToString("X").PadLeft(2, '0'),
                    ((byte)ModeSendMsg.type).ToString("X").PadLeft(2, '0'), ModeSendMsg.data_length.ToString("X").PadLeft(2, '0'),
                    ModeSendMsg.message_id.ToString("X").PadLeft(4, '0'), ModeSendMsg.data);

                //在两个字符间加上空格   
                SysMode = Regex.Replace(SysMode, @".{2}", "$0 ");

                TypeKeyValue.dic.Add(string.Format("sys_workMode_{0}", sys), SysMode.Trim());
            }


            MsgId2App msgId2App = new MsgId2App();
            msgId2App.id = msgId;
            msgId2App.AppInfo = AppInfo;

            if (!MyDeviceList.AddMsgId2App(ApToken, msgId2App))
            {
                OnOutputLog(LogInfoType.EROR, string.Format("添加消息Id到设备列表出错！"));
                Send2APP_GeneralError(ApToken, AppInfo, ApMsgType.set_general_para_request.ToString(),
                    string.Format("添加消息Id到设备列表出错！"));
                return;
            }

            byte[] bMsg = EncodeApXmlMessage(msgId, TypeKeyValue);
            if (bMsg == null)
            {
                OnOutputLog(LogInfoType.EROR, string.Format("封装XML消息(Send2ap_SET_PARA_REQ)出错！"));
                return;
            }
            SendMsg2Ap(ApToken, bMsg);
        }

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
                OnOutputLog(LogInfoType.EROR, "收到MainController模块消息ApInfo内容错误!",LogCategory.R);
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
                //所有在线AP数与数据库不一至，回复所有在线AP
                byte sys = GetMsgByteValueInList("sys", MainMsg.Body.dic,Byte.MaxValue);
                if ((sys != 0) && (sys!=1))
                {
                    OnOutputLog(LogInfoType.EROR,"发送给GSM设备消息错误。消息中系统号不为0或1。" );
                    Send2APP_GeneralError(MainMsg.ApInfo, MainMsg.AppInfo, MainMsg.Body.type,
                       string.Format("发送给GSM设备消息错误。消息中系统号不为0或1。"));
                    return;
                }
                if (MainMsg.Body.n_dic == null)
                {
                    OnOutputLog(LogInfoType.EROR, "发送给GSM设备消息错误。消息中没有可设置的参数(n_dic项为NULL)。");
                    Send2APP_GeneralError(MainMsg.ApInfo, MainMsg.AppInfo, MainMsg.Body.type,
                        "发送给GSM设备消息错误。消息中没有可设置的参数(n_dic项为NULL)。");
                    return;
                }

                foreach (Name_DIC_Struct x in MainMsg.Body.n_dic)
                {
                    EncodeMainMsg(MainMsg.ApInfo,MainMsg.AppInfo, (Gsm_Device_Sys)sys, x);
                }
                return;
            }
            else if (MainMsg.Body.type == Main2ApControllerMsgType.SetGenParaReq)
            {
                //所有在线AP数与数据库不一至，回复所有在线AP
                byte sys = GetMsgByteValueInList("sys", MainMsg.Body.dic, Byte.MaxValue);
                if ((sys != 0) && (sys != 1))
                {
                    OnOutputLog(LogInfoType.EROR, "发送给GSM设备消息错误。消息中系统号不为0或1。");
                    Send2APP_GeneralError(MainMsg.ApInfo, MainMsg.AppInfo, MainMsg.Body.type,
                       string.Format("发送给GSM设备消息错误。消息中系统号不为0或1。"));
                    return;
                }
                if (MainMsg.Body.n_dic == null)
                {
                    OnOutputLog(LogInfoType.EROR, "发送给GSM设备消息错误。消息中没有可设置的参数(n_dic项为NULL)。");
                    Send2APP_GeneralError(MainMsg.ApInfo, MainMsg.AppInfo, MainMsg.Body.type,
                        "发送给GSM设备消息错误。消息中没有可设置的参数(n_dic项为NULL)。");
                    return;
                }

                EncodeSetParaMsg(MainMsg.ApInfo, MainMsg.AppInfo, (Gsm_Device_Sys)sys, MainMsg.Body.n_dic);
                
                return;
            }
            else //其它消息
            {
                string str = string.Format("发送给设备({0}:{1})消息类型{2}错误！",
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
        private void EncodeMainMsg(Ap_Info_Struct ApInfo, App_Info_Struct AppInfo, Gsm_Device_Sys sys, Name_DIC_Struct n_dic)
        {
            AsyncUserToken ApToKen = MyDeviceList.FindByIpPort(ApInfo.IP, ApInfo.Port);
            if (ApToKen == null)
            {
                string str = string.Format("在线AP列表中找不到Ap[{0}:{1}]设备，通过FullName重新查询设备！",
                    ApInfo.IP, ApInfo.Port.ToString());
                OnOutputLog(LogInfoType.WARN, str);
                ApToKen = MyDeviceList.FindByFullname(ApInfo.Fullname);
            }

            if (ApToKen == null)
            {
                string str = string.Format("在线AP列表中找不到Ap[{0}:{1}],FullName:{2}。无法向AP发送消息！",
                    ApInfo.IP, ApInfo.Port.ToString(), ApInfo.Fullname);
                OnOutputLog(LogInfoType.WARN, str);
                Send2APP_GeneralError(ApInfo, AppInfo, Main2ApControllerMsgType.gsm_msg_send, str);
                return;
            }
            
            if (n_dic.name.Equals(Gsm_Send_Msg_Type.RECV_SYS_PARA.ToString()))
            {
                RecvSysPara para = new RecvSysPara();
                para.paraMnc = GetMsgU16ValueInList("paraMnc", n_dic.dic, UInt16.MaxValue);
                if (para.paraMnc == UInt16.MaxValue)
                {
                    SendMainMsgParaVlaueError(ApInfo, AppInfo, Gsm_Send_Msg_Type.RECV_SYS_PARA.ToString(), "paraMnc");
                    return;
                }
                para.paraMcc = GetMsgU16ValueInList("paraMcc", n_dic.dic, UInt16.MaxValue);
                if (para.paraMcc == UInt16.MaxValue)
                {
                    SendMainMsgParaVlaueError(ApInfo, AppInfo, Gsm_Send_Msg_Type.RECV_SYS_PARA.ToString(), "paraMcc");
                    return;
                }
                para.paraBsic = GetMsgByteValueInList("paraBsic", n_dic.dic);
                if (para.paraBsic == 0)
                {
                    SendMainMsgParaVlaueError(ApInfo, AppInfo, Gsm_Send_Msg_Type.RECV_SYS_PARA.ToString(), "paraBsic");
                    return;
                }
                para.paraLac = GetMsgU16ValueInList("paraLac", n_dic.dic);
                if (para.paraLac == 0)
                {
                    SendMainMsgParaVlaueError(ApInfo, AppInfo, Gsm_Send_Msg_Type.RECV_SYS_PARA.ToString(), "paraLac");
                    return;
                }
                para.paraCellId = GetMsgU16ValueInList("paraCellId", n_dic.dic);
                //if (para.paraCellId == 0)
                //{
                //    SendMainMsgParaVlaueError(ApInfo,AppInfo, Gsm_Send_Msg_Type.RECV_SYS_PARA.ToString(), "paraCellId");
                //    return;
                //}
                para.paraC2 = GetMsgSByteValueInList("paraC2", n_dic.dic, SByte.MaxValue);
                if (para.paraC2 == SByte.MaxValue)
                {
                    SendMainMsgParaVlaueError(ApInfo, AppInfo, Gsm_Send_Msg_Type.RECV_SYS_PARA.ToString(), "paraC2");
                    return;
                }
                para.paraPeri = GetMsgByteValueInList("paraPeri", n_dic.dic, Byte.MaxValue);
                //if (para.paraPeri == 0)
                //{
                //    SendMainMsgParaVlaueError(ApInfo,AppInfo, Gsm_Send_Msg_Type.RECV_SYS_PARA.ToString(), "paraPeri");
                //    return;
                //}
                para.paraAccPwr = GetMsgByteValueInList("paraAccPwr", n_dic.dic, Byte.MaxValue);
                if (para.paraAccPwr == Byte.MaxValue)
                {
                    SendMainMsgParaVlaueError(ApInfo, AppInfo, Gsm_Send_Msg_Type.RECV_SYS_PARA.ToString(), "paraAccPwr");
                    return;
                }
                para.paraMsPwr = GetMsgByteValueInList("paraMsPwr", n_dic.dic, Byte.MaxValue);
                if (para.paraMsPwr == Byte.MaxValue)
                {
                    SendMainMsgParaVlaueError(ApInfo, AppInfo, Gsm_Send_Msg_Type.RECV_SYS_PARA.ToString(), "paraMsPwr");
                    return;
                }
                para.paraRejCau = GetMsgByteValueInList("paraRejCau", n_dic.dic, Byte.MaxValue);
                if (para.paraRejCau == Byte.MaxValue)
                {
                    SendMainMsgParaVlaueError(ApInfo, AppInfo, Gsm_Send_Msg_Type.RECV_SYS_PARA.ToString(), "paraRejCau");
                    return;
                }
                Send2ap_RECV_SYS_PARA(ApToKen, AppInfo, sys, para);
            }
            else if (n_dic.name.Equals(Gsm_Send_Msg_Type.RECV_SYS_OPTION.ToString()))
            {
                RecvSysOption option = new RecvSysOption();
                option.opLuSms = GetMsgByteValueInList("opLuSms", n_dic.dic, Byte.MaxValue);
                if (option.opLuSms != 0 && option.opLuSms != 1)
                {
                    SendMainMsgParaVlaueError(ApInfo, AppInfo, Gsm_Send_Msg_Type.RECV_SYS_OPTION.ToString(), "opLuSms");
                    return;
                }
                option.opLuImei = GetMsgByteValueInList("opLuImei", n_dic.dic, Byte.MaxValue);
                if (option.opLuImei != 0 && option.opLuImei != 1)
                {
                    SendMainMsgParaVlaueError(ApInfo, AppInfo, Gsm_Send_Msg_Type.RECV_SYS_OPTION.ToString(), "opLuImei");
                    return;
                }
                option.opCallEn = GetMsgByteValueInList("opCallEn", n_dic.dic, Byte.MaxValue);
                if (option.opCallEn != 0 && option.opCallEn != 1)
                {
                    SendMainMsgParaVlaueError(ApInfo, AppInfo, Gsm_Send_Msg_Type.RECV_SYS_OPTION.ToString(), "opCallEn");
                    return;
                }
                option.opDebug = GetMsgByteValueInList("opDebug", n_dic.dic, Byte.MaxValue);
                if (option.opDebug != 0 && option.opDebug != 1)
                {
                    SendMainMsgParaVlaueError(ApInfo, AppInfo, Gsm_Send_Msg_Type.RECV_SYS_OPTION.ToString(), "opDebug");
                    return;
                }
                option.opLuType = GetMsgByteValueInList("opLuType", n_dic.dic, Byte.MaxValue);
                if (option.opLuType != 1 && option.opLuType != 2 && option.opLuType != 3)
                {
                    SendMainMsgParaVlaueError(ApInfo, AppInfo, Gsm_Send_Msg_Type.RECV_SYS_OPTION.ToString(), "opLuType");
                    return;
                }
                option.opSmsType = GetMsgByteValueInList("opSmsType", n_dic.dic, Byte.MaxValue);
                if (option.opSmsType != 1 && option.opSmsType != 2 && option.opSmsType != 3)
                {
                    SendMainMsgParaVlaueError(ApInfo, AppInfo, Gsm_Send_Msg_Type.RECV_SYS_OPTION.ToString(), "opSmsType");
                    return;
                }
                Send2ap_RECV_SYS_OPTION(ApToKen, AppInfo, sys, option);
            }
            else if (n_dic.name.Equals(Gsm_Send_Msg_Type.RECV_RF_PARA.ToString()))
            {
                RecvRfOption option = new RecvRfOption();
                option.rfEnable = GetMsgByteValueInList("rfEnable", n_dic.dic,Byte.MaxValue);
                if (option.rfEnable != 0 && option.rfEnable != 1)
                {
                    SendMainMsgParaVlaueError(ApInfo, AppInfo, Gsm_Send_Msg_Type.RECV_RF_PARA.ToString(), "rfEnable");
                    return;
                }
                option.rfFreq = GetMsgU16ValueInList("rfFreq", n_dic.dic, UInt16.MaxValue);
                if (option.rfFreq == UInt16.MaxValue)
                {
                    SendMainMsgParaVlaueError(ApInfo, AppInfo, Gsm_Send_Msg_Type.RECV_RF_PARA.ToString(), "rfFreq");
                    return;
                }
                option.rfPwr = GetMsgByteValueInList("rfPwr", n_dic.dic, Byte.MaxValue);
                if (option.rfPwr == Byte.MaxValue)
                {
                    SendMainMsgParaVlaueError(ApInfo, AppInfo, Gsm_Send_Msg_Type.RECV_RF_PARA.ToString(), "rfPwr");
                    return;
                }
                Send2ap_RECV_RF_PARA(ApToKen, AppInfo, sys, option);
            }
            else if (n_dic.name.Equals(Gsm_Send_Msg_Type.RECV_SMS_OPTION.ToString()))
            {
                RecvSmsOption sms = new RecvSmsOption();
                sms.gSmsRpoa = GetMsgStringValueInList("gSmsRpoa", n_dic.dic);
                if (string.IsNullOrEmpty(sms.gSmsRpoa))
                {
                    SendMainMsgParaVlaueError(ApInfo, AppInfo, Gsm_Send_Msg_Type.RECV_SMS_OPTION.ToString(), "gSmsRpoa");
                    return;
                }
                sms.gSmsTpoa = GetMsgStringValueInList("gSmsTpoa", n_dic.dic);
                if (string.IsNullOrEmpty(sms.gSmsTpoa))
                {
                    SendMainMsgParaVlaueError(ApInfo, AppInfo, Gsm_Send_Msg_Type.RECV_SMS_OPTION.ToString(), "gSmsTpoa");
                    return;
                }
                sms.gSmsScts = GetMsgStringValueInList("gSmsScts", n_dic.dic);
                if (string.IsNullOrEmpty(sms.gSmsScts))
                {
                    SendMainMsgParaVlaueError(ApInfo, AppInfo, Gsm_Send_Msg_Type.RECV_SMS_OPTION.ToString(), "gSmsScts");
                    return;
                }
                sms.gSmsData = GetMsgStringValueInList("gSmsData", n_dic.dic);
                if (string.IsNullOrEmpty(sms.gSmsData))
                {
                    SendMainMsgParaVlaueError(ApInfo, AppInfo, Gsm_Send_Msg_Type.RECV_SMS_OPTION.ToString(), "gSmsData");
                    return;
                }
                sms.autoSendtiny = GetMsgByteValueInList("autoSendtiny", n_dic.dic);
                if (sms.autoSendtiny != 0 && sms.autoSendtiny != 1)
                {
                    SendMainMsgParaVlaueError(ApInfo, AppInfo, Gsm_Send_Msg_Type.RECV_SMS_OPTION.ToString(), "autoSendtiny");
                    return;
                }
                sms.autoFilterSMStiny = GetMsgByteValueInList("autoFilterSMStiny", n_dic.dic);
                if (sms.autoFilterSMStiny != 0 && sms.autoFilterSMStiny != 1)
                {
                    SendMainMsgParaVlaueError(ApInfo, AppInfo, Gsm_Send_Msg_Type.RECV_SMS_OPTION.ToString(), "autoFilterSMStiny");
                    return;
                }
                sms.delayTime = GetMsgU16ValueInList("delayTime", n_dic.dic, UInt16.MaxValue);
                if (sms.delayTime == UInt16.MaxValue)
                {
                    SendMainMsgParaVlaueError(ApInfo, AppInfo, Gsm_Send_Msg_Type.RECV_SYS_PARA.ToString(), "delayTime");
                    return;
                }
                sms.smsCodingtiny = GetMsgByteValueInList("smsCodingtiny", n_dic.dic);
                if (sms.smsCodingtiny != 0 && sms.smsCodingtiny != 1)
                {
                    SendMainMsgParaVlaueError(ApInfo, AppInfo, Gsm_Send_Msg_Type.RECV_SMS_OPTION.ToString(), "smsCodingtiny");
                    return;
                }

                Send2ap_RECV_SMS_OPTION(ApToKen, AppInfo, sys, sms);
            }
            else if (n_dic.name.Equals(Gsm_Send_Msg_Type.RECV_REG_MODE.ToString()))
            {
                byte regMode = GetMsgByteValueInList("regMode", n_dic.dic, Byte.MaxValue);
                if (regMode != 0 && regMode != 1)
                {
                    SendMainMsgParaVlaueError(ApInfo, AppInfo, Gsm_Send_Msg_Type.RECV_REG_MODE.ToString(), "regMode");
                    return;
                }

                Send2ap_RECV_REG_MODE(ApToKen, AppInfo, sys, regMode);
            }
            else if (n_dic.name.Equals(Gsm_Send_Msg_Type.RECV_LIBRARY_REG_ADD.ToString()))
            {
                string type = GetMsgStringValueInList("type", n_dic.dic);
                if (!type.Equals("IMSI") && !type.Equals("IMEI") && !type.Equals("TMSI"))
                {
                    SendMainMsgParaVlaueError(ApInfo, AppInfo, Gsm_Send_Msg_Type.RECV_LIBRARY_REG_ADD.ToString(), "type");
                    return;
                }

                string gLibrary = GetMsgStringValueInList("gLibrary", n_dic.dic);
                if (string.IsNullOrEmpty(gLibrary))
                {
                    SendMainMsgParaVlaueError(ApInfo, AppInfo, Gsm_Send_Msg_Type.RECV_LIBRARY_REG_ADD.ToString(), "gLibrary");
                    return;
                }
                Send2ap_RECV_LIBRARY_REG_ADD(ApToKen, AppInfo, sys, type, gLibrary);
            }
            else if (n_dic.name.Equals(Gsm_Send_Msg_Type.RECV_LIBRARY_REG_DELALL.ToString()))
            {
                Send2ap_RECV_LIBRARY_REG_DELALL(ApToKen, AppInfo, sys);
            }
            else if (n_dic.name.Equals(Gsm_Send_Msg_Type.RECV_LIBRARY_REG_QUERY.ToString()))
            {
                Send2ap_RECV_LIBRARY_REG_QUERY(ApToKen, AppInfo, sys);
            }
            else
            {
                string str = string.Format("发送给GSM设备消息({0})错误。暂不支持该消息类型)。", n_dic.name);
                OnOutputLog(LogInfoType.EROR, str);
                Send2APP_GeneralError(ApInfo, AppInfo, n_dic.name, str);
            }
        }

        private void EncodeSetParaMsg(Ap_Info_Struct ApInfo, App_Info_Struct AppInfo, Gsm_Device_Sys sys, List<Name_DIC_Struct> n_dic_List)
        {
            int Flag = 0;
            RecvSysPara para = new RecvSysPara();
            RecvSysOption option = new RecvSysOption();
            RecvRfOption rf = new RecvRfOption();
            byte regMode = 0;

            AsyncUserToken ApToKen = MyDeviceList.FindByIpPort(ApInfo.IP, ApInfo.Port);
            if (ApToKen == null)
            {
                string str = string.Format("在线AP列表中找不到Ap[{0}:{1}]设备，通过FullName重新查询设备！",
                    ApInfo.IP, ApInfo.Port.ToString());
                OnOutputLog(LogInfoType.WARN, str);
                ApToKen = MyDeviceList.FindByFullname(ApInfo.Fullname);
            }

            if (ApToKen == null)
            {
                string str = string.Format("在线AP列表中找不到Ap[{0}:{1}],FullName:{2}。无法向AP发送消息！",
                    ApInfo.IP, ApInfo.Port.ToString(), ApInfo.Fullname);
                OnOutputLog(LogInfoType.WARN, str);
                Send2APP_GeneralError(ApInfo, AppInfo, Main2ApControllerMsgType.gsm_msg_send, str);
                return;
            }

            foreach (Name_DIC_Struct n_dic in n_dic_List)
            {
                if (n_dic.name.Equals(Gsm_Send_Msg_Type.RECV_SYS_PARA.ToString()))
                {
                    para.paraMnc = GetMsgU16ValueInList("paraMnc", n_dic.dic, UInt16.MaxValue);
                    if (para.paraMnc == UInt16.MaxValue)
                    {
                        SendMainMsgParaVlaueError(ApInfo, AppInfo, Gsm_Send_Msg_Type.RECV_SYS_PARA.ToString(), "paraMnc");
                        return;
                    }
                    para.paraMcc = GetMsgU16ValueInList("paraMcc", n_dic.dic, UInt16.MaxValue);
                    if (para.paraMcc == UInt16.MaxValue)
                    {
                        SendMainMsgParaVlaueError(ApInfo, AppInfo, Gsm_Send_Msg_Type.RECV_SYS_PARA.ToString(), "paraMcc");
                        return;
                    }
                    para.paraBsic = GetMsgByteValueInList("paraBsic", n_dic.dic);
                    if (para.paraBsic == 0)
                    {
                        SendMainMsgParaVlaueError(ApInfo, AppInfo, Gsm_Send_Msg_Type.RECV_SYS_PARA.ToString(), "paraBsic");
                        return;
                    }
                    para.paraLac = GetMsgU16ValueInList("paraLac", n_dic.dic);
                    if (para.paraLac == 0)
                    {
                        SendMainMsgParaVlaueError(ApInfo, AppInfo, Gsm_Send_Msg_Type.RECV_SYS_PARA.ToString(), "paraLac");
                        return;
                    }
                    para.paraCellId = GetMsgU16ValueInList("paraCellId", n_dic.dic);
                    //if (para.paraCellId == 0)
                    //{
                    //    SendMainMsgParaVlaueError(ApInfo,AppInfo, Gsm_Send_Msg_Type.RECV_SYS_PARA.ToString(), "paraCellId");
                    //    return;
                    //}
                    para.paraC2 = GetMsgSByteValueInList("paraC2", n_dic.dic, SByte.MaxValue);
                    if (para.paraC2 == SByte.MaxValue)
                    {
                        SendMainMsgParaVlaueError(ApInfo, AppInfo, Gsm_Send_Msg_Type.RECV_SYS_PARA.ToString(), "paraC2");
                        return;
                    }
                    para.paraPeri = GetMsgByteValueInList("paraPeri", n_dic.dic, Byte.MaxValue);
                    //if (para.paraPeri == 0)
                    //{
                    //    SendMainMsgParaVlaueError(ApInfo,AppInfo, Gsm_Send_Msg_Type.RECV_SYS_PARA.ToString(), "paraPeri");
                    //    return;
                    //}
                    para.paraAccPwr = GetMsgByteValueInList("paraAccPwr", n_dic.dic, Byte.MaxValue);
                    if (para.paraAccPwr == Byte.MaxValue)
                    {
                        SendMainMsgParaVlaueError(ApInfo, AppInfo, Gsm_Send_Msg_Type.RECV_SYS_PARA.ToString(), "paraAccPwr");
                        return;
                    }
                    para.paraMsPwr = GetMsgByteValueInList("paraMsPwr", n_dic.dic, Byte.MaxValue);
                    if (para.paraMsPwr == Byte.MaxValue)
                    {
                        SendMainMsgParaVlaueError(ApInfo, AppInfo, Gsm_Send_Msg_Type.RECV_SYS_PARA.ToString(), "paraMsPwr");
                        return;
                    }
                    para.paraRejCau = GetMsgByteValueInList("paraRejCau", n_dic.dic, Byte.MaxValue);
                    if (para.paraRejCau == Byte.MaxValue)
                    {
                        SendMainMsgParaVlaueError(ApInfo, AppInfo, Gsm_Send_Msg_Type.RECV_SYS_PARA.ToString(), "paraRejCau");
                        return;
                    }
                    Flag |= 0x1;
                }
                else if (n_dic.name.Equals(Gsm_Send_Msg_Type.RECV_SYS_OPTION.ToString()))
                {
                    option.opLuSms = GetMsgByteValueInList("opLuSms", n_dic.dic, Byte.MaxValue);
                    if (option.opLuSms != 0 && option.opLuSms != 1)
                    {
                        SendMainMsgParaVlaueError(ApInfo, AppInfo, Gsm_Send_Msg_Type.RECV_SYS_OPTION.ToString(), "opLuSms");
                        return;
                    }
                    option.opLuImei = GetMsgByteValueInList("opLuImei", n_dic.dic, Byte.MaxValue);
                    if (option.opLuImei != 0 && option.opLuImei != 1)
                    {
                        SendMainMsgParaVlaueError(ApInfo, AppInfo, Gsm_Send_Msg_Type.RECV_SYS_OPTION.ToString(), "opLuImei");
                        return;
                    }
                    option.opCallEn = GetMsgByteValueInList("opCallEn", n_dic.dic, Byte.MaxValue);
                    if (option.opCallEn != 0 && option.opCallEn != 1)
                    {
                        SendMainMsgParaVlaueError(ApInfo, AppInfo, Gsm_Send_Msg_Type.RECV_SYS_OPTION.ToString(), "opCallEn");
                        return;
                    }
                    option.opDebug = GetMsgByteValueInList("opDebug", n_dic.dic, Byte.MaxValue);
                    if (option.opDebug != 0 && option.opDebug != 1)
                    {
                        SendMainMsgParaVlaueError(ApInfo, AppInfo, Gsm_Send_Msg_Type.RECV_SYS_OPTION.ToString(), "opDebug");
                        return;
                    }
                    option.opLuType = GetMsgByteValueInList("opLuType", n_dic.dic, Byte.MaxValue);
                    if (option.opLuType != 1 && option.opLuType != 2 && option.opLuType != 3)
                    {
                        SendMainMsgParaVlaueError(ApInfo, AppInfo, Gsm_Send_Msg_Type.RECV_SYS_OPTION.ToString(), "opLuType");
                        return;
                    }
                    option.opSmsType = GetMsgByteValueInList("opSmsType", n_dic.dic, Byte.MaxValue);
                    if (option.opSmsType != 1 && option.opSmsType != 2 && option.opSmsType != 3)
                    {
                        SendMainMsgParaVlaueError(ApInfo, AppInfo, Gsm_Send_Msg_Type.RECV_SYS_OPTION.ToString(), "opSmsType");
                        return;
                    }
                    Flag |= 0x02;
                }
                else if (n_dic.name.Equals(Gsm_Send_Msg_Type.RECV_RF_PARA.ToString()))
                {
                    rf.rfEnable = GetMsgByteValueInList("rfEnable", n_dic.dic, Byte.MaxValue);
                    if (rf.rfEnable != 0 && rf.rfEnable != 1)
                    {
                        SendMainMsgParaVlaueError(ApInfo, AppInfo, Gsm_Send_Msg_Type.RECV_RF_PARA.ToString(), "rfEnable");
                        return;
                    }
                    rf.rfFreq = GetMsgU16ValueInList("rfFreq", n_dic.dic, UInt16.MaxValue);
                    if (rf.rfFreq == UInt16.MaxValue)
                    {
                        SendMainMsgParaVlaueError(ApInfo, AppInfo, Gsm_Send_Msg_Type.RECV_RF_PARA.ToString(), "rfFreq");
                        return;
                    }
                    rf.rfPwr = GetMsgByteValueInList("rfPwr", n_dic.dic, Byte.MaxValue);
                    if (rf.rfPwr == Byte.MaxValue)
                    {
                        SendMainMsgParaVlaueError(ApInfo, AppInfo, Gsm_Send_Msg_Type.RECV_RF_PARA.ToString(), "rfPwr");
                        return;
                    }
                    Flag |= 0x04;
                }
                else if (n_dic.name.Equals(Gsm_Send_Msg_Type.RECV_REG_MODE.ToString()))
                {
                    regMode = GetMsgByteValueInList("regMode", n_dic.dic, Byte.MaxValue);
                    if (regMode != 0 && regMode != 1)
                    {
                        SendMainMsgParaVlaueError(ApInfo, AppInfo, Gsm_Send_Msg_Type.RECV_REG_MODE.ToString(), "regMode");
                        return;
                    }

                    Flag |= 0x08;
                }
            }

            if (Flag == 0)
            {
                string str = string.Format("发送给GSM设备消息({0})错误。消息中没有n_dic项。", Main2ApControllerMsgType.SetGenParaReq);
                OnOutputLog(LogInfoType.WARN, str);
                return;
            }

            Send2ap_SET_PARA_REQ(ApToKen,AppInfo,sys,Flag,para,option,rf,regMode);
        }

        #region 封装发送给Main模块消息
        /// <summary>
        /// 向Main模块发送系统参数设备更改通知
        /// </summary>
        /// <param name="apToken">AP设备信息</param>
        /// <param name="recv">收到的参数</param>
        private void Send2Main_ParaChange_RECV_SYS_PARA(AsyncUserToken apToken, ack_msg_recv recv)
        {
            String data = recv.data;
            Msg_Body_Struct TypeKeyValue = new Msg_Body_Struct(Main2ApControllerMsgType.ReportGenPara);
            TypeKeyValue.dic.Add("reportType", "change");
            TypeKeyValue.dic.Add("sys", (Byte)recv.sys);
            TypeKeyValue.dic.Add("hardware_id", recv.hardware_id);

            Name_DIC_Struct nDic = new Name_DIC_Struct();
            nDic.name = Gsm_Send_Msg_Type.RECV_SYS_PARA.ToString();

            UInt16 paraMcc = GetValueByString_U16(ref data);
            nDic.dic.Add("paraMcc", paraMcc.ToString());
            string paraMnc = GetValueByString_String(4,ref data);
            string mnc = paraMnc.Substring(0,2);
            if (mnc.Equals("0F") || mnc.Equals("0f"))
            {
                paraMnc = paraMnc.Substring(2);
            }
            else
            {
                paraMnc = paraMnc.Substring(1);
            }
            nDic.dic.Add("paraMnc", paraMnc.ToString());
            Byte paraBsic = GetValueByString_Byte(ref data);
            nDic.dic.Add("paraBsic", paraBsic.ToString());
            UInt16 paraLac = GetValueByString_U16(ref data);
            nDic.dic.Add("paraLac", paraLac.ToString());
            UInt16 paraCellId = GetValueByString_U16(ref data);
            nDic.dic.Add("paraCellId", paraCellId.ToString());
            SByte paraC2 = GetValueByString_SByte(ref data);
            nDic.dic.Add("paraC2", paraC2.ToString());
            Byte paraPeri = GetValueByString_Byte(ref data);
            nDic.dic.Add("paraPeri", paraPeri.ToString());
            Byte paraAccPwr = GetValueByString_Byte(ref data);
            nDic.dic.Add("paraAccPwr", paraAccPwr.ToString());
            Byte paraMsPwr = GetValueByString_Byte(ref data);
            nDic.dic.Add("paraMsPwr", paraMsPwr.ToString());
            Byte paraRejCau = GetValueByString_Byte(ref data);
            nDic.dic.Add("paraRejCau", paraRejCau.ToString());

            TypeKeyValue.n_dic.Add(nDic);

            OnSendMsg2Main(recv.message_id, MsgType.CONFIG, apToken, TypeKeyValue);
        }

        private void Send2Main_ParaChange_RECV_SYS_OPTION(AsyncUserToken apToken, ack_msg_recv recv)
        {
            String data = recv.data;
            Msg_Body_Struct TypeKeyValue = new Msg_Body_Struct(Main2ApControllerMsgType.ReportGenPara);
            TypeKeyValue.dic.Add("reportType", "change");
            TypeKeyValue.dic.Add("sys", (Byte)recv.sys);
            TypeKeyValue.dic.Add("hardware_id", recv.hardware_id);

            Name_DIC_Struct nDic = new Name_DIC_Struct();
            nDic.name = Gsm_Send_Msg_Type.RECV_SYS_OPTION.ToString();

            Byte opLuSms = GetValueByString_Byte(ref data);
            nDic.dic.Add("opLuSms", opLuSms.ToString());
            Byte opLuImei = GetValueByString_Byte(ref data);
            nDic.dic.Add("opLuImei", opLuImei.ToString());
            Byte opCallEn = GetValueByString_Byte(ref data);
            nDic.dic.Add("opCallEn", opCallEn.ToString());
            Byte opDebug = GetValueByString_Byte(ref data);
            nDic.dic.Add("opDebug", opDebug.ToString());
            Byte opLuType = GetValueByString_Byte(ref data);
            nDic.dic.Add("opLuType", opLuType.ToString());
            Byte opSmsType = GetValueByString_Byte(ref data);
            nDic.dic.Add("opSmsType", opSmsType.ToString());

            TypeKeyValue.n_dic.Add(nDic);

            OnSendMsg2Main(recv.message_id, MsgType.CONFIG, apToken, TypeKeyValue);
        }

        private void Send2Main_ParaChange_RECV_SMS_OPTION(AsyncUserToken apToken, ack_msg_ms_recv recv)
        {
            Msg_Body_Struct TypeKeyValue = new Msg_Body_Struct(Main2ApControllerMsgType.ReportGenPara);
            TypeKeyValue.dic.Add("reportType", "change");
            TypeKeyValue.dic.Add("sys", (Byte)recv.sys);
            TypeKeyValue.dic.Add("hardware_id", recv.hardware_id);

            Name_DIC_Struct nDic = new Name_DIC_Struct();
            nDic.name = Gsm_Send_Msg_Type.RECV_SMS_OPTION.ToString();

            nDic.dic.Add("gSmsRpoa", recv.gSmsRpoa);
            nDic.dic.Add("gSmsTpoa", recv.gSmsTpoa);
            nDic.dic.Add("gSmsScts", recv.gSmsScts);
            nDic.dic.Add("gSmsData", recv.gSmsData);
            nDic.dic.Add("smsCodingtiny", recv.enType);

            TypeKeyValue.n_dic.Add(nDic);

            OnSendMsg2Main(recv.message_id, MsgType.CONFIG, apToken, TypeKeyValue);
        }

        private void Send2Main_ParaChange_RECV_RF_PARA(AsyncUserToken apToken, ack_msg_recv recv)
        {
            String data = recv.data;
            Msg_Body_Struct TypeKeyValue = new Msg_Body_Struct(Main2ApControllerMsgType.ReportGenPara);
            TypeKeyValue.dic.Add("reportType", "change");
            TypeKeyValue.dic.Add("sys", (Byte)recv.sys);
            TypeKeyValue.dic.Add("hardware_id", recv.hardware_id);

            Name_DIC_Struct nDic = new Name_DIC_Struct();
            nDic.name = Gsm_Send_Msg_Type.RECV_RF_PARA.ToString();

            Byte rfEnable = GetValueByString_Byte(ref data);
            nDic.dic.Add("rfEnable", rfEnable.ToString());
            UInt16 rfFreq = GetValueByString_U16(ref data);
            nDic.dic.Add("rfFreq", rfFreq.ToString());
            Byte rfPwr = GetValueByString_Byte(ref data);
            nDic.dic.Add("rfPwr", rfPwr.ToString());
           
            TypeKeyValue.n_dic.Add(nDic);

            OnSendMsg2Main(recv.message_id, MsgType.CONFIG, apToken, TypeKeyValue);
        }

        private void Send2Main_ParaChange_RECV_REG_MODE(AsyncUserToken apToken, ack_msg_recv recv)
        {
            String data = recv.data;
            Msg_Body_Struct TypeKeyValue = new Msg_Body_Struct(Main2ApControllerMsgType.ReportGenPara);
            TypeKeyValue.dic.Add("reportType", "change");
            TypeKeyValue.dic.Add("sys", (Byte)recv.sys);
            TypeKeyValue.dic.Add("hardware_id", recv.hardware_id);

            Name_DIC_Struct nDic = new Name_DIC_Struct();
            nDic.name = Gsm_Send_Msg_Type.RECV_REG_MODE.ToString();

            Byte regMode = GetValueByString_Byte(ref data);
            nDic.dic.Add("regMode", regMode.ToString());
          
            TypeKeyValue.n_dic.Add(nDic);

            OnSendMsg2Main(recv.message_id, MsgType.CONFIG, apToken, TypeKeyValue);
        }


        private void Send2Main_SEND_REQ_CNF(AsyncUserToken apToKen, gsm_msg_recv recv)
        {
            String data = recv.data;
            Msg_Body_Struct TypeKeyValue = new Msg_Body_Struct(Main2ApControllerMsgType.gsm_msg_recv);
            TypeKeyValue.dic.Add("sys", recv.sys);
            TypeKeyValue.dic.Add("hardware_id", recv.hardware_id);

            Name_DIC_Struct nDic = new Name_DIC_Struct();
            nDic.name = Gsm_Recv_Msg_Type.SEND_REQ_CNF.ToString();

            Byte cnfType = GetValueByString_Byte(ref data);
            nDic.dic.Add("cnfType", cnfType.ToString());
            Byte cnfInd = GetValueByString_Byte(ref data);
            nDic.dic.Add("cnfInd", cnfInd);
           
            TypeKeyValue.n_dic.Add(nDic);

            OnSendMsg2Main(recv.message_id, MsgType.CONFIG, apToKen, TypeKeyValue);
        }

        private void Send2Main_SEND_OM_INFO(AsyncUserToken apToKen, gsm_msg_recv recv)
        {
            String data = recv.data;
            Msg_Body_Struct TypeKeyValue = new Msg_Body_Struct(Main2ApControllerMsgType.gsm_msg_recv);
            TypeKeyValue.dic.Add("sys", recv.sys);
            TypeKeyValue.dic.Add("hardware_id", recv.hardware_id);

            Name_DIC_Struct nDic = new Name_DIC_Struct();
            nDic.name = Gsm_Recv_Msg_Type.SEND_OM_INFO.ToString();

            Byte gOmInfo = GetValueByString_Byte(ref data);
            nDic.dic.Add("gOmInfo", gOmInfo.ToString());
          
            TypeKeyValue.n_dic.Add(nDic);

            OnSendMsg2Main(recv.message_id, MsgType.CONFIG, apToKen, TypeKeyValue);
        }

        private void Send2Main_SEND_VER_INFO(AsyncUserToken apToKen, gsm_msg_recv recv)
        {
            String data = recv.data;
            Msg_Body_Struct TypeKeyValue = new Msg_Body_Struct(Main2ApControllerMsgType.gsm_msg_recv);
            TypeKeyValue.dic.Add("sys", recv.sys);
            TypeKeyValue.dic.Add("hardware_id", recv.hardware_id);

            Name_DIC_Struct nDic = new Name_DIC_Struct();
            nDic.name = Gsm_Recv_Msg_Type.SEND_VER_INFO.ToString();

            string verApp = GetValueByString_String(6,ref data);
            nDic.dic.Add("verApp", verApp.ToString());
            string verPhy = GetValueByString_String(6, ref data);
            nDic.dic.Add("verPhy", verPhy.ToString());

            TypeKeyValue.n_dic.Add(nDic);

            OnSendMsg2Main(recv.message_id, MsgType.CONFIG, apToKen, TypeKeyValue);
        }

        private void Send2Main_SEND_UE_INFO(AsyncUserToken apToKen, gsm_msg_recv recv)
        {
            String data = recv.data;
            Msg_Body_Struct TypeKeyValue = new Msg_Body_Struct(Main2ApControllerMsgType.gsm_msg_recv);
            TypeKeyValue.dic.Add("sys", recv.sys);
            TypeKeyValue.dic.Add("hardware_id", recv.hardware_id);

            Name_DIC_Struct nDic = new Name_DIC_Struct();
            nDic.name = Gsm_Recv_Msg_Type.SEND_UE_INFO.ToString();

            string ueImsi = GetValueByString_String(16,ref data);
            nDic.dic.Add("ueImsi", ueImsi.Substring(0,ueImsi.Length-1));
            string ueImei = GetValueByString_String(16,ref data);
            nDic.dic.Add("ueImei", ueImei.Substring(0, ueImsi.Length - 1));
            string ueMsisdn = GetValueByString_String(8,ref data);
            nDic.dic.Add("ueMsisdn", ueMsisdn);
            Byte uePwr = GetValueByString_Byte(ref data);
            nDic.dic.Add("uePwr", uePwr-111);
            Byte UeRegtype = GetValueByString_Byte(ref data);
            nDic.dic.Add("UeRegtype", UeRegtype);
            Byte ueQueryResult = GetValueByString_Byte(ref data);
            nDic.dic.Add("ueQueryResult", ueQueryResult);
            UInt32 ueTmsi = GetValueByString_U32(ref data);
            nDic.dic.Add("ueTmsi", ueTmsi);
            UInt16 ueLlac = GetValueByString_U16(ref data);
            nDic.dic.Add("ueLlac", ueLlac);
            UInt16 ueSlac = GetValueByString_U16(ref data);
            nDic.dic.Add("ueSlac", ueSlac);

            TypeKeyValue.n_dic.Add(nDic);


            OnSendMsg2Main(recv.message_id, MsgType.NOTICE, apToKen, TypeKeyValue);
        }

        private void Send2Main_SEND_TEST_INFO(AsyncUserToken apToKen, gsm_msg_recv recv)
        {
            String data = recv.data;
            Msg_Body_Struct TypeKeyValue = new Msg_Body_Struct(Main2ApControllerMsgType.gsm_msg_recv);
            TypeKeyValue.dic.Add("sys", recv.sys);
            TypeKeyValue.dic.Add("hardware_id", recv.hardware_id);

            Name_DIC_Struct nDic = new Name_DIC_Struct();
            nDic.name = Gsm_Recv_Msg_Type.SEND_TEST_INFO.ToString();

            nDic.dic.Add("gTestInfo", data.ToString());
         
            TypeKeyValue.n_dic.Add(nDic);

            OnSendMsg2Main(recv.message_id, MsgType.CONFIG, apToKen, TypeKeyValue);
        }

        private void Send2Main_SEND_BS_INFO(AsyncUserToken apToKen, gsm_msg_recv recv)
        {
            String data = recv.data;
            Msg_Body_Struct TypeKeyValue = new Msg_Body_Struct(Main2ApControllerMsgType.gsm_msg_recv);
            TypeKeyValue.dic.Add("sys", recv.sys);
            TypeKeyValue.dic.Add("hardware_id", recv.hardware_id);

            Name_DIC_Struct nDic = new Name_DIC_Struct();
            nDic.name = Gsm_Recv_Msg_Type.SEND_BS_INFO.ToString();

            nDic.dic.Add("gBsInfo", data.ToString());

            TypeKeyValue.n_dic.Add(nDic);

            OnSendMsg2Main(recv.message_id, MsgType.CONFIG, apToKen, TypeKeyValue);
        }

        private void Send2Main_SEND_QUERY_REQ(AsyncUserToken apToKen, gsm_msg_recv recv)
        {
            String data = recv.data;
            Msg_Body_Struct TypeKeyValue = new Msg_Body_Struct(Main2ApControllerMsgType.gsm_msg_recv);
            TypeKeyValue.dic.Add("sys", recv.sys);
            TypeKeyValue.dic.Add("hardware_id", recv.hardware_id);

            Name_DIC_Struct nDic = new Name_DIC_Struct();
            nDic.name = Gsm_Recv_Msg_Type.SEND_QUERY_REQ.ToString();

            Byte queryChno = GetValueByString_Byte(ref data);
            nDic.dic.Add("queryChno", queryChno.ToString());

            nDic.dic.Add("queryImsi", data.ToString());

            TypeKeyValue.n_dic.Add(nDic);

            OnSendMsg2Main(recv.message_id, MsgType.CONFIG, apToKen, TypeKeyValue);
        }

        private void Send2Main_SEND_LIBRARY_REG(AsyncUserToken apToKen, gsm_msg_recv recv)
        {
            String data = String.Empty;
            Msg_Body_Struct TypeKeyValue = new Msg_Body_Struct(Main2ApControllerMsgType.gsm_msg_recv);
            TypeKeyValue.dic.Add("sys", recv.sys);
            TypeKeyValue.dic.Add("hardware_id", recv.hardware_id);

            Name_DIC_Struct nDic = new Name_DIC_Struct();
            nDic.name = Gsm_Recv_Msg_Type.SEND_LIBRARY_REG.ToString();

            data = recv.data;
            string type = GetValueByString_String(3,ref data);
            if (type.ToUpper().Equals("809"))
            {
                nDic.dic.Add("type", "IMSI");
                nDic.dic.Add("gLibrary", data.ToString());
            }
            else if (type.ToUpper().Equals("80A"))
            {
                nDic.dic.Add("type", "IMEI");
                nDic.dic.Add("gLibrary", data.ToString());
            }
            else
            {
                data = recv.data;
                type = GetValueByString_String(4, ref data);
                if (type.ToUpper().Equals("05F4"))
                {
                    nDic.dic.Add("type", "TMSI");
                    nDic.dic.Add("gLibrary", data.ToString());
                }
                else
                {
                    OnOutputLog(LogInfoType.WARN, "收到返回读出的登录库内容错误!");
                    return;
                }
            }

            TypeKeyValue.n_dic.Add(nDic);

            OnSendMsg2Main(recv.message_id, MsgType.CONFIG, apToKen, TypeKeyValue);
        }

        private void Send2Main_SEND_LIBRARY_SMS(AsyncUserToken apToKen, gsm_msg_recv recv)
        {
            String data = String.Empty;
            Msg_Body_Struct TypeKeyValue = new Msg_Body_Struct(Main2ApControllerMsgType.gsm_msg_recv);
            TypeKeyValue.dic.Add("sys", recv.sys);
            TypeKeyValue.dic.Add("hardware_id", recv.hardware_id);

            Name_DIC_Struct nDic = new Name_DIC_Struct();
            nDic.name = Gsm_Recv_Msg_Type.SEND_LIBRARY_SMS.ToString();

            data = recv.data;
            string type = GetValueByString_String(3, ref data);
            if (type.ToUpper().Equals("809"))
            {
                nDic.dic.Add("type", "IMSI");
                nDic.dic.Add("gLibrary", data.ToString());
            }
            else if (type.ToUpper().Equals("80A"))
            {
                nDic.dic.Add("type", "IMEI");
                nDic.dic.Add("gLibrary", data.ToString());
            }
            else
            {
                data = recv.data;
                type = GetValueByString_String(4, ref data);
                if (type.ToUpper().Equals("05F4"))
                {
                    nDic.dic.Add("type", "TMSI");
                    nDic.dic.Add("gLibrary", data.ToString());
                }
                else
                {
                    OnOutputLog(LogInfoType.WARN, "收到返回读出的登录库内容错误!");
                    return;
                }
            }

            TypeKeyValue.n_dic.Add(nDic);

            OnSendMsg2Main(recv.message_id, MsgType.CONFIG, apToKen, TypeKeyValue);
        }

        private void Send2Main_SEND_OBJECT_POWER(AsyncUserToken apToKen, gsm_msg_recv recv)
        {
            String data = recv.data;
            Msg_Body_Struct TypeKeyValue = new Msg_Body_Struct(Main2ApControllerMsgType.gsm_msg_recv);
            TypeKeyValue.dic.Add("sys", recv.sys);
            TypeKeyValue.dic.Add("hardware_id", recv.hardware_id);

            Name_DIC_Struct nDic = new Name_DIC_Struct();
            nDic.name = Gsm_Recv_Msg_Type.SEND_OBJECT_POWER.ToString();

            Byte power1 = GetValueByString_Byte(ref data);
            nDic.dic.Add("power1",(power1-109).ToString());
            Byte power2 = GetValueByString_Byte(ref data);
            nDic.dic.Add("power2", (power2 - 109).ToString());
            Byte power3 = GetValueByString_Byte(ref data);
            nDic.dic.Add("power3", (power3 - 109).ToString());
            Byte power4 = GetValueByString_Byte(ref data);
            nDic.dic.Add("power4", (power4 - 109).ToString());
            Byte bs = GetValueByString_Byte(ref data);
            nDic.dic.Add("bs", (bs - 111).ToString());

            TypeKeyValue.n_dic.Add(nDic);

            OnSendMsg2Main(recv.message_id, MsgType.CONFIG, apToKen, TypeKeyValue);
        }

        private void Send2Main_SEND_MS_CALL_SETUP(AsyncUserToken apToKen, gsm_msg_recv recv)
        {
            String data = recv.data;
            Msg_Body_Struct TypeKeyValue = new Msg_Body_Struct(Main2ApControllerMsgType.gsm_msg_recv);
            TypeKeyValue.dic.Add("sys", recv.sys);
            TypeKeyValue.dic.Add("hardware_id", recv.hardware_id);

            Name_DIC_Struct nDic = new Name_DIC_Struct();
            nDic.name = Gsm_Recv_Msg_Type.SEND_MS_CALL_SETUP.ToString();

            string imsi = GetValueByString_String(9, ref data);
            nDic.dic.Add("imsi", imsi.Substring(3));
            string number = GetValueByString_String(9, ref data);
            nDic.dic.Add("number", imsi.TrimStart('0').TrimEnd('F'));

            TypeKeyValue.n_dic.Add(nDic);

            OnSendMsg2Main(recv.message_id, MsgType.CONFIG, apToKen, TypeKeyValue);
        }

        private void Send2Main_SEND_MS_SMS_SEND(AsyncUserToken apToKen, gsm_msg_recv recv)
        {
            String data = recv.data;
            Msg_Body_Struct TypeKeyValue = new Msg_Body_Struct(Main2ApControllerMsgType.gsm_msg_recv);
            TypeKeyValue.dic.Add("sys", recv.sys);
            TypeKeyValue.dic.Add("hardware_id", recv.hardware_id);

            Name_DIC_Struct nDic = new Name_DIC_Struct();
            nDic.name = Gsm_Recv_Msg_Type.SEND_MS_SMS_SEND.ToString();

            string imsi = GetValueByString_String(9, ref data);
            nDic.dic.Add("imsi", imsi.Substring(3));
            string number = GetValueByString_String(9, ref data);
            nDic.dic.Add("number", imsi.TrimStart('0').TrimEnd('F'));
            Byte codetype = GetValueByString_Byte(ref data);
            nDic.dic.Add("codetype", codetype.ToString());
            Byte len = GetValueByString_Byte(ref data);
            //nDic.dic.Add("codetype", codetype.ToString());

            if ("1".Equals(codetype))
            {
                nDic.dic.Add("codetype", CodeConver.Unicode2String(data));
            }
            else if ("0".Equals(codetype))
            {
                nDic.dic.Add("codetype", CodeConver.Decode7Bit(data));
            }
            else
            {
                OnOutputLog(LogInfoType.WARN, "收到手机主动发起短信编码方式错误!");
                return;
            }

            TypeKeyValue.n_dic.Add(nDic);

            OnSendMsg2Main(recv.message_id, MsgType.CONFIG, apToKen, TypeKeyValue);
        }

        private void Send2Main_SEND_MS_CALL_OPERATE(AsyncUserToken apToKen, gsm_msg_recv recv)
        {
            String data = recv.data;
            Msg_Body_Struct TypeKeyValue = new Msg_Body_Struct(Main2ApControllerMsgType.gsm_msg_recv);
            TypeKeyValue.dic.Add("sys", recv.sys);
            TypeKeyValue.dic.Add("hardware_id", recv.hardware_id);

            Name_DIC_Struct nDic = new Name_DIC_Struct();
            nDic.name = Gsm_Recv_Msg_Type.SEND_MS_CALL_OPERATE.ToString();

            nDic.dic.Add("call_operate", data);

            TypeKeyValue.n_dic.Add(nDic);

            OnSendMsg2Main(recv.message_id, MsgType.CONFIG, apToKen, TypeKeyValue);
        }

        private void Send2Main_SEND_GET_PARA_RSP(AsyncUserToken apToKen, gsm_msg_recv para, gsm_msg_recv option, gsm_msg_recv rf, gsm_msg_recv work)
        {
            String data = string.Empty;
            Msg_Body_Struct TypeKeyValue = new Msg_Body_Struct(Main2ApControllerMsgType.ReportGenPara);

            TypeKeyValue.dic.Add("reportType", "report");
            TypeKeyValue.dic.Add("whiteimsi_md5", "");
            TypeKeyValue.dic.Add("blackimsi_md5", "");
            TypeKeyValue.dic.Add("sys", (Byte)para.sys);
            TypeKeyValue.dic.Add("hardware_id", para.hardware_id);

            data = para.data;
            Name_DIC_Struct nDic = new Name_DIC_Struct();
            nDic.name = Gsm_Send_Msg_Type.RECV_SYS_PARA.ToString();

            UInt16 paraMcc = GetValueByString_U16(ref data);
            nDic.dic.Add("paraMcc", paraMcc.ToString());
            string paraMnc = GetValueByString_String(4, ref data);
            string mnc = paraMnc.Substring(0, 2);
            if (mnc.Equals("0F") || mnc.Equals("0f"))
            {
                paraMnc = paraMnc.Substring(2);
            }
            else
            {
                paraMnc = paraMnc.Substring(1);
            }
            nDic.dic.Add("paraMnc", paraMnc.ToString());
            Byte paraBsic = GetValueByString_Byte(ref data);
            nDic.dic.Add("paraBsic", paraBsic.ToString());
            UInt16 paraLac = GetValueByString_U16(ref data);
            nDic.dic.Add("paraLac", paraLac.ToString());
            UInt16 paraCellId = GetValueByString_U16(ref data);
            nDic.dic.Add("paraCellId", paraCellId.ToString());
            SByte paraC2 = GetValueByString_SByte(ref data);
            nDic.dic.Add("paraC2", paraC2.ToString());
            Byte paraPeri = GetValueByString_Byte(ref data);
            nDic.dic.Add("paraPeri", paraPeri.ToString());
            Byte paraAccPwr = GetValueByString_Byte(ref data);
            nDic.dic.Add("paraAccPwr", paraAccPwr.ToString());
            Byte paraMsPwr = GetValueByString_Byte(ref data);
            nDic.dic.Add("paraMsPwr", paraMsPwr.ToString());
            Byte paraRejCau = GetValueByString_Byte(ref data);
            nDic.dic.Add("paraRejCau", paraRejCau.ToString());

            TypeKeyValue.n_dic.Add(nDic);

            data = option.data;
            Name_DIC_Struct nDic1 = new Name_DIC_Struct();
            nDic1.name = Gsm_Send_Msg_Type.RECV_SYS_OPTION.ToString();

            Byte opLuSms = GetValueByString_Byte(ref data);
            nDic1.dic.Add("opLuSms", opLuSms.ToString());
            Byte opLuImei = GetValueByString_Byte(ref data);
            nDic1.dic.Add("opLuImei", opLuImei.ToString());
            Byte opCallEn = GetValueByString_Byte(ref data);
            nDic1.dic.Add("opCallEn", opCallEn.ToString());
            Byte opDebug = GetValueByString_Byte(ref data);
            nDic1.dic.Add("opDebug", opDebug.ToString());
            Byte opLuType = GetValueByString_Byte(ref data);
            nDic1.dic.Add("opLuType", opLuType.ToString());
            Byte opSmsType = GetValueByString_Byte(ref data);
            nDic1.dic.Add("opSmsType", opSmsType.ToString());

            TypeKeyValue.n_dic.Add(nDic1);


            data = rf.data;
            Name_DIC_Struct nDic2 = new Name_DIC_Struct();
            nDic2.name = Gsm_Send_Msg_Type.RECV_RF_PARA.ToString();

            Byte rfEnable = GetValueByString_Byte(ref data);
            nDic2.dic.Add("rfEnable", rfEnable.ToString());
            UInt16 rfFreq = GetValueByString_U16(ref data);
            nDic2.dic.Add("rfFreq", rfFreq.ToString());
            Byte rfPwr = GetValueByString_Byte(ref data);
            nDic2.dic.Add("rfPwr", rfPwr.ToString());

            TypeKeyValue.n_dic.Add(nDic2);

            data = work.data;
            Name_DIC_Struct nDic3 = new Name_DIC_Struct();
            nDic3.name = Gsm_Send_Msg_Type.RECV_REG_MODE.ToString();

            Byte regMode = GetValueByString_Byte(ref data);
            nDic3.dic.Add("regMode", regMode.ToString());

            TypeKeyValue.n_dic.Add(nDic3);

            OnSendMsg2Main(0, MsgType.CONFIG, apToKen, TypeKeyValue);
        }

        #endregion

        #endregion

    }
}
