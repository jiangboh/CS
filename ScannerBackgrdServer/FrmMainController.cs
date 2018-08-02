using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Security.Cryptography;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;
using Newtonsoft.Json;
using ScannerBackgrdServer.ApController;
using ScannerBackgrdServer.AppController;
using ScannerBackgrdServer.Common;
using static ScannerBackgrdServer.Common.MsgStruct;
using System.Runtime.InteropServices;

namespace ScannerBackgrdServer
{
    #region 消息交互相关

    public enum MessageType       //消息类型
    {
        MSG_STRING = 0,
        MSG_INT = 1,
        MSG_DOUBLE = 2,
        MSG_DATATABLE = 3,
        MSG_XML = 4,
        MSG_STATUS = 5,
        MSG_JSON = 6,
        MSG_MAX = 7
    }

    public struct MessageBody       //消息实体
    {
        public string bString; 
        public int bInt;
        public double bDouble;
        public DataTable bDataTable;
        public string bXml;
        public int bStatus;
        public string bJson;
    }

    /// <summary>
    /// 声明消息交互的delegate对象,用于ApCtrl，
    /// MainCtrl和AppCtrl之间进行消息的交互
    /// </summary>
    /// <param name="mt"></param>
    /// <param name="mb"></param>
    public delegate void MessageDelegate(MessageType mt, MessageBody mb);

    #endregion
   
    public partial class FrmMainController : Form
    {
        #region IMSI解析

        [DllImport(@"PhoneAreaInterface.dll", EntryPoint = "initGLFunc", CallingConvention = CallingConvention.Cdecl)]
        public static extern int InitGLFunc();

        [DllImport(@"PhoneAreaInterface.dll", EntryPoint = "getLocationAndOprator", CallingConvention = CallingConvention.Cdecl)]
        public static extern int GetLocationAndOprator(string imsi, ref byte location, ref byte country, ref byte opreator, ref byte isdn);

        public struct strImsiParse       
        {
            /*
             * 号码归属地 = 国家 + 位置
             */ 
            public string location;    //位置
            public string country;     //国家
            public string operators;   //运营商
            public string isdn;        //综合业务数字网
        }               

        /// <summary>
        /// 用于调试Log的互斥
        /// </summary>
        private static Object mutex_ImsiParse = new Object();

        // 2018-07-25
        private static Dictionary<string, strImsiParse> gDicImsiParse = new Dictionary<string, strImsiParse>();

        public int Location_And_Operator_Init()
        {
            return InitGLFunc();
        }

        /// <summary>
        /// 根据配置，将imsi号对应的信息获取出来
        /// </summary>
        /// <param name="imsi"></param>
        /// <param name="imsiParse"></param>
        /// <returns></returns>
        public int Location_And_Operator_Get(string imsi, ref strImsiParse imsiParse)
        {
            int rtv = -1;

            byte[] _location = new byte[128];
            byte[] _country = new byte[128];
            byte[] _operators = new byte[128];
            byte[] _isdn = new byte[64];

            if (string.IsNullOrEmpty(imsi))
            {
                return -1;
            }

            imsiParse = new strImsiParse();

            if (DataController.ImsiParseMode == 0)
            {
                #region 直接从接口获取

                rtv = GetLocationAndOprator(imsi, ref _location[0], ref _country[0], ref _operators[0], ref _isdn[0]);
                if (rtv != 0)
                {
                    imsiParse.location = Encoding.UTF8.GetString(_location, 0, _location.Length).Replace('\0', ' ').Trim();

                    if (Location.Equals(""))
                    {
                        imsiParse.location = "Null";
                    }

                    imsiParse.country = Encoding.UTF8.GetString(_country, 0, _country.Length).Replace('\0', ' ').Trim();
                    if (imsiParse.country.Equals(""))
                    {
                        imsiParse.country = "Null";
                    }

                    imsiParse.operators = Encoding.UTF8.GetString(_operators, 0, _operators.Length).Replace('\0', ' ').Trim();
                    if (imsiParse.operators.Equals(""))
                    {
                        imsiParse.operators = "Null";
                    }

                    imsiParse.isdn = Encoding.UTF8.GetString(_isdn, 0, _isdn.Length).Replace('\0', ' ').Trim();
                    if (imsiParse.isdn.Equals(""))
                    {
                        imsiParse.isdn = "Null";
                    }
                }
                else
                {
                    imsiParse.location = "Null";
                    imsiParse.country = string.Empty;
                    imsiParse.operators = "Null";
                    imsiParse.isdn = "Null";
                }

                #endregion
            }
            else
            {
                #region 从接口和字典获取

                lock (mutex_ImsiParse)
                {
                    if (gDicImsiParse.ContainsKey(imsi))
                    {
                        imsiParse = gDicImsiParse[imsi];
                    }
                    else
                    {
                        rtv = GetLocationAndOprator(imsi, ref _location[0], ref _country[0], ref _operators[0], ref _isdn[0]);
                        if (rtv != 0)
                        {
                            imsiParse.location = Encoding.UTF8.GetString(_location, 0, _location.Length).Replace('\0', ' ').Trim();

                            if (Location.Equals(""))
                            {
                                imsiParse.location = "Null";
                            }

                            imsiParse.country = Encoding.UTF8.GetString(_country, 0, _country.Length).Replace('\0', ' ').Trim();
                            if (imsiParse.country.Equals(""))
                            {
                                imsiParse.country = "Null";
                            }

                            imsiParse.operators = Encoding.UTF8.GetString(_operators, 0, _operators.Length).Replace('\0', ' ').Trim();
                            if (imsiParse.operators.Equals(""))
                            {
                                imsiParse.operators = "Null";
                            }

                            imsiParse.isdn = Encoding.UTF8.GetString(_isdn, 0, _isdn.Length).Replace('\0', ' ').Trim();
                            if (imsiParse.isdn.Equals(""))
                            {
                                imsiParse.isdn = "Null";
                            }
                        }
                        else
                        {
                            imsiParse.location = "Null";
                            imsiParse.country = string.Empty;
                            imsiParse.operators = "Null";
                            imsiParse.isdn = "Null";
                        }

                        gDicImsiParse.Add(imsi, imsiParse);
                    }
                }

                #endregion
            }

            return 0;
        }

        /// <summary>
        /// 将imsi对应的信息添加到字典中
        /// </summary>
        /// <param name="imsi"></param>
        /// <returns></returns>
        public int Location_And_Operator_Set(string imsi)
        {
            int rtv = -1;

            byte[] _location = new byte[128];
            byte[] _country = new byte[128];
            byte[] _operators = new byte[128];
            byte[] _isdn = new byte[64];

            if (string.IsNullOrEmpty(imsi))
            {
                return -1;
            }
           
            if (DataController.ImsiParseMode == 1)
            {
                #region 添加字典中的项

                lock (mutex_ImsiParse)
                {
                    if (gDicImsiParse.ContainsKey(imsi))
                    {
                        #region 已经包含

                        return 0;

                        #endregion
                    }
                    else
                    {
                        #region 尚未包含

                        strImsiParse imsiParse = new strImsiParse();
                        rtv = GetLocationAndOprator(imsi, ref _location[0], ref _country[0], ref _operators[0], ref _isdn[0]);
                        if (rtv != 0)
                        {
                            imsiParse.location = Encoding.UTF8.GetString(_location, 0, _location.Length).Replace('\0', ' ').Trim();

                            if (Location.Equals(""))
                            {
                                imsiParse.location = "Null";
                            }

                            imsiParse.country = Encoding.UTF8.GetString(_country, 0, _country.Length).Replace('\0', ' ').Trim();
                            if (imsiParse.country.Equals(""))
                            {
                                imsiParse.country = "Null";
                            }

                            imsiParse.operators = Encoding.UTF8.GetString(_operators, 0, _operators.Length).Replace('\0', ' ').Trim();
                            if (imsiParse.operators.Equals(""))
                            {
                                imsiParse.operators = "Null";
                            }

                            imsiParse.isdn = Encoding.UTF8.GetString(_isdn, 0, _isdn.Length).Replace('\0', ' ').Trim();
                            if (imsiParse.isdn.Equals(""))
                            {
                                imsiParse.isdn = "Null";
                            }
                        }
                        else
                        {
                            imsiParse.location = "Null";
                            imsiParse.country = string.Empty;
                            imsiParse.operators = "Null";
                            imsiParse.isdn = "Null";
                        }

                        gDicImsiParse.Add(imsi, imsiParse);

                        #endregion
                    }
                }

                #endregion
            }
         
            return 0;
        }

        #endregion

        #region 计时器

        public void TimerFunc(object source, EventArgs e)
        {
            TaskTimer tt = (TaskTimer)source;

            string errInfo = string.Format("Id:Name:Interval({0}:{1}:{2}) -> 超时", tt.Id, tt.Name, tt.Interval);
            add_log_info(LogInfoType.EROR, errInfo, "Main", LogCategory.I);
            Logger.Trace(LogInfoType.EROR, errInfo, "Main", LogCategory.I);

            InterModuleMsgStruct imms = tt.Imms;

            // result:"0",      // 0:SUCCESS ; 1:GENERAL FAILURE;
            //                  // 2:CONFIGURATION FAIURE OR NOT SUPPORTED
            // rebootflag:"1",	// 1—立刻reboot,2—需要reboot
            // timestamp:"xxx" 

            imms.Body.type = tt.MsgType;
            imms.Body.dic = new Dictionary<string, object>();
            imms.Body.dic.Add("ReturnCode", -1);
            imms.Body.dic.Add("ReturnStr", errInfo);
            imms.Body.dic.Add("result", "1");
            imms.Body.dic.Add("rebootflag", "2");
            imms.Body.dic.Add("timestamp", DateTime.Now.ToString());

            Send_Msg_2_AppCtrl_Upper(imms);
            tt.TimeOutFlag = true;
        }

        public class TaskTimer : System.Timers.Timer
        {
            #region <变量>

            /// <summary>
            /// 定时器id
            /// </summary>
            private int id;

            /// <summary>
            /// 定时器name
            /// </summary>
            private string name;

            private bool timeOutFlag;
            private string msgType;
            private InterModuleMsgStruct imms;

            #endregion

            #region <属性>

            /// <summary>
            /// 定时器id属性
            /// </summary>
            public int Id
            {
                set { id = value; }
                get { return id; }
            }

            /// <summary>
            /// 定时器name属性
            /// </summary>
            public string Name
            {
                set { name = value; }
                get { return name; }
            }

            public InterModuleMsgStruct Imms { get => imms; set => imms = value; }
            public string MsgType { get => msgType; set => msgType = value; }
            public bool TimeOutFlag { get => timeOutFlag; set => timeOutFlag = value; }

            public string parentFullPathName;
            public string devName;
            public string mode;
            public string ipAddr;
            public string port;

            ///// <summary>
            ///// 定时器参数属性
            ///// </summary>
            //public InterModuleMsgStruct Imms
            //{
            //    set { imms = value; }
            //    get { return imms; }
            //}


            #endregion

            #region <构造函数>

            ///<summary>
            /// 构造函数
            /// </summary>
            public TaskTimer() : base()
            {
                this.AutoReset = false;
            }

            #endregion
        }

        /// <summary>
        /// 用于设置AP全名的超时计时器
        /// </summary>
        private TaskTimer gTimerSetFullName = new TaskTimer();

        /// <summary>
        /// 用于设置AP重定向的超时计时器
        /// </summary>
        private TaskTimer gTimerSetRedirection = new TaskTimer();

        /// <summary>
        /// 用于设置黑白名单的操作
        /// </summary>
        private TaskTimer gTimerBlackWhite = new TaskTimer();

        #endregion

        #region 声明委托

        /// <summary>
        /// 声明委托类型
        /// </summary>
        /// <param name="str"></param>
        /// <param name="type"></param>
        public delegate void show_log_info_delegate(string str, LogInfoType type);

        /// <summary>
        /// 声明委托类型，用于批量数据库处理
        /// </summary>
        /// <param name="listSC"></param>
        /// <returns></returns>
        private delegate int db_batch_process_delegate(List<strCapture> listSC);

        /// <summary>
        /// 声明委托类型，用于处理历史记录搜索
        /// </summary>
        /// <param name="imms"></param>
        /// <returns></returns>
        private delegate void history_record_process_delegate(InterModuleMsgStruct imms);

        #endregion

        #region 定义类型

        /// <summary>
        /// App信息
        /// </summary>
        public class clsDataAlign
        {
            public string fileNameBlackList_Ap_Base;
            public string fileNameWhiteList_Ap_Base;

            public string fileNameBlackList_Db_Base;
            public string fileNameWhiteList_Db_Base;

            public bool whiteimsi_md5_match;
            public bool blackimsi_md5_match;

            public int devId;

            public clsDataAlign()
            {
                this.fileNameBlackList_Ap_Base = "fileNameBlackList_Ap.txt";
                this.fileNameWhiteList_Ap_Base = "fileNameWhiteList_Ap.txt";

                this.fileNameBlackList_Db_Base = "fileNameBlackList_Db.txt";
                this.fileNameWhiteList_Db_Base = "fileNameWhiteList_Db.txt";

                this.whiteimsi_md5_match = true;
                this.blackimsi_md5_match = true;
                this.devId = -1;
            }
        }

        public struct strLogInfo
        {
            public LogInfoType type;
            public string info;
        }

        public struct strMsgInfo
        {
            public MessageType mt;
            public MessageBody mb;
        }

        /// <summary>
        /// 登录的用户相关信息
        /// </summary>
        public struct strLoginUserInfo
        {
            public string userName;             //用户名
            public string affRole;              //用户所属的角色
            public List<string> affDomainList;  //用户所属的域列表
            public List<string> affFunList;     //用户所属的功能列表
        }

        public struct strUpdateInfo
        {
            public string md5;
            public string fileName;
            public string version;
            public bool needToUpdate;
            public List<int> listDevId;
            public List<string> listDevFullName;            
        }

        public struct strBwListSetInfo
        {            
            public string devId;
            public string domainId;
            public List<strBwList> listBwInfo;
            public List<int> listDevId;
            public List<string> listDevFullName;
        }

        private const string LOG_DEBG = "\r\n【DEBG】[{0}][{1}] {2}({3})";
        private const string LOG_INFO = "\r\n【INFO】[{0}][{1}] {2}({3})";
        private const string LOG_WARN = "\r\n【WARN】[{0}][{1}] {2}({3})";
        private const string LOG_EROR = "\r\n【EROR】[{0}][{1}] {2}({3})";        

        #endregion

        #region 定义变量

        private static DbHelper gDbHelper;

        /// <summary>
        /// 专门用于IMSI处理
        /// </summary>
        private static FtpHelper gFtpHelperImsi;

        /// <summary>
        /// 专门用于File处理
        /// </summary>
        private static FtpHelper gFtpHelperFile;

        private static Dictionary<string, DateTime> gDicRemoveDup = new Dictionary<string, DateTime>();

        /// <summary>
        /// 用于调试Log的互斥
        /// </summary>
        private static Object mutex_Logger = new Object();

        /// <summary>
        /// 用于接收ApController消息的互斥
        /// </summary>
        private static Object mutex_Ap_Controller = new Object();

        /// <summary>
        /// 用于接收AppController消息的互斥
        /// </summary>
        private static Object mutex_App_Controller = new Object();

        /// <summary>
        /// 用于数据库处理得互斥
        /// </summary>
        private static Object mutex_DbHelper = new object();

        /// <summary>
        /// 用于FTP处理得互斥
        /// </summary>
        private static Object mutex_FtpHelper = new object();

        private static Queue<strLogInfo> gListLog = new Queue<strLogInfo>();
        private static Queue<strMsgInfo> gMsgFor_Ap_Controller = new Queue<strMsgInfo>();
        private static Queue<strMsgInfo> gMsgFor_App_Controller = new Queue<strMsgInfo>();

        /// <summary>
        /// 所有捕到的号都先放入该队列,用于数据库
        /// </summary>
        private static Queue<strCapture> gCaptureInfoDb = new Queue<strCapture>();

        /// <summary>
        /// 所有捕到的号都先放入该队列,用于FTP
        /// </summary>
        private static Queue<strCapture> gCaptureInfoFtp = new Queue<strCapture>();

        /// <summary>
        /// 用于快速通过设备的全名找设备对应的ID
        /// 如：设备.深圳.福田.中心广场.西北监控.LTE-FDD-B3，其中
        /// 设备.深圳.福田.中心广场.西北监控为域名，LTE-FDD-B3为名称
        /// 系统启动后或设备有更改后获取该字典到内存中
        /// string = 设备.深圳.福田.中心广场.西北监控.LTE-FDD-B3
        /// int    = device的id
        /// </summary>
        //private static Dictionary<string, int> gDicDeviceId = new Dictionary<string, int>();

        // 2018-07-03
        private static Dictionary<string, strDevice> gDicDeviceId = new Dictionary<string, strDevice>();

        private static List<strLoginUserInfo> gLoginUserInfo = new List<strLoginUserInfo>();

        private static InterModuleMsgStruct gApLower;
        private static InterModuleMsgStruct gAppUpper;

        private static strBwListSetInfo gBwListSetInfo = new strBwListSetInfo();

        //private int aaa = 1;
        private bool stopFlag = false;

        private int gIMSI_Index = 1;
        private int gIMSI_Count = 1;

        private strUpdateInfo gUpdateInfo = new strUpdateInfo();

        /// <summary>
        /// 用于保存多个APP获取黑白名单的信息
        /// string : 172.17.0.123:12345
        /// strBwListQueryInfo : 查询条件 + 查询结果
        /// </summary>
        private static Dictionary<string, strBwListQueryInfo> gDicBwListQueryInfo = new Dictionary<string, strBwListQueryInfo>();

        /// <summary>
        /// 用于保存多个APP获取历史记录的信息
        /// string : 172.17.0.123:12345
        /// strCaptureQueryInfo : 查询结果
        /// </summary>
        private static Dictionary<string, strCaptureQueryInfo> gDicCaptureQueryInfo = new Dictionary<string, strCaptureQueryInfo>();

        private strRedirection gRedirectionInfo = new strRedirection();

        private clsDataAlign gClsDataAlign = new clsDataAlign();  
        private static int gCurLogInfoTypeIndex;

        #endregion

        #region 发消息给ApCtrlLower

        /// <summary>
        /// 声明用于发送信息给ApCtrlLower的代理
        /// </summary>       
        private static MessageDelegate Delegate_SendMsg_2_ApCtrl_Lower = new MessageDelegate(ApManager.MessageDelegate_For_MainController);

        #endregion

        #region 发消息给AppCtrlUpper

        /// <summary>
        ///  声明用于发送信息给AppCtrlUpper的代理
        /// </summary>
        private static MessageDelegate Delegate_SendMsg_2_AppCtrl_Upper = new MessageDelegate(AppManager.MessageDelegate_For_MainController);

        #endregion

        #region 用于Log的消息线程

        /// <summary>
        /// 添加Log消息到队列中
        /// </summary>
        /// <param name="type">Log类型</param>
        /// <param name="str">Log消息</param>
        /// <param name="filePath">文件名</param>
        /// <param name="memberName">函数名</param>
        /// <param name="lineNumber">行号</param>
        public static void add_log_info(LogInfoType type, string str,
                                        string moduleName,
                                        LogCategory cat,
                                       [CallerFilePath]   string filePath = "",
                                       [CallerMemberName] string memberName = "",
                                       [CallerLineNumber] int lineNumber = 0)
        {
            if ("0" == DataController.StrAppDebugMode)
            {
                //不是调试模式
                return;
            }

            if (type < DataController.LogOutputLevel)
            {
                return;
            }

            if (string.IsNullOrEmpty(str))
            {
                add_log_info(LogInfoType.EROR, "add_log_info字符串为空","Main",LogCategory.I);
                return;
            }

            string tmp = "";

            if (type == LogInfoType.INFO)
            {
                tmp = string.Format(LOG_INFO, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff"), lines, Path.GetFileName(filePath), lineNumber);
            }
            else if (type == LogInfoType.WARN)
            {
                tmp = string.Format(LOG_WARN, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff"), lines, Path.GetFileName(filePath), lineNumber);
            }
            else if (type == LogInfoType.EROR)
            {
                tmp = string.Format(LOG_EROR, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff"), lines, Path.GetFileName(filePath), lineNumber);
            }
            else
            {
                tmp = string.Format(LOG_DEBG, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff"), lines, Path.GetFileName(filePath), lineNumber);
            }

            if (cat == LogCategory.I)
            {
                str = tmp + string.Format("\n【{0}，I】{1}", moduleName,str);
            }
            else if (cat == LogCategory.S)
            {
                str = tmp + string.Format("\n【{0}，S】{1}", moduleName, str);
            }
            else if (cat == LogCategory.R)
            {
                str = tmp + string.Format("\n【{0}，R】{1}", moduleName, str);
            }
            else 
            {
                str = tmp + string.Format("\n【{0}，I】{1}", moduleName, str);
            }

            strLogInfo logInfo;
            logInfo.type = type;
            logInfo.info = str;

            lock (mutex_Logger)
            {
                lines++;
                gListLog.Enqueue(logInfo);
            }
        }


        public static string get_debug_info([CallerFilePath] string filePath = "",
                                          [CallerMemberName] string memberName = "",
                                          [CallerLineNumber] int lineNumber = 0)
        {            
            return string.Format("\n(Line->{0}\nFunc->{1}\nFile->{2})\n", lineNumber, memberName, Path.GetFileName(filePath));                 
        }

        /// <summary>
        /// 本例中的线程要通过这个方法来访问主线程中的控件
        /// </summary>
        /// <param name="str"></param>
        /// <param name="type"></param>
        private void show_log_info(string str, LogInfoType type)
        {
            if (stopFlag == true)
            {
                return;
            }

            if (richTextBoxLog.Text.Length >= richTextBoxLog.MaxLength)
            {
                richTextBoxLog.Text = "";
            }

            //richTextBoxLog.AppendText("\n");

            if (type == LogInfoType.INFO)
            {
                richTextBoxLog.SelectionColor = Color.Black;
            }
            else if (type == LogInfoType.WARN)
            {
                richTextBoxLog.SelectionColor = Color.Blue;
            }
            else if (type == LogInfoType.EROR)
            {
                richTextBoxLog.SelectionColor = Color.Red;
            }
            else
            {
                richTextBoxLog.SelectionColor = Color.Gray;
            }

            richTextBoxLog.AppendText(str);

            //设置光标的位置到文本尾 
            richTextBoxLog.Select(richTextBoxLog.TextLength, 10);

            //滚动到控件光标处 
            richTextBoxLog.ScrollToCaret();

            richTextBoxLog.AppendText("\n");
        }

        /// <summary>
        /// 创建有参的方法
        /// 注意：方法里面的参数类型必须是Object类型
        /// </summary>
        /// <param name="obj"></param>
        private static int lines = 1;
        private void thread_for_logger(object obj)
        {
            strLogInfo tmp = new strLogInfo();

            while (true)
            {
                lock (mutex_Logger)
                {
                    if (gListLog.Count <= 0)
                    {
                        Thread.Sleep(100);
                        continue;
                    }
                    else
                    {
                        Thread.Sleep(2);
                    }

                    tmp = gListLog.Dequeue();

                    /*
                     * 线程通过方法的委托执行show_log_info()，实现对richTextBoxLog控件的访问
                     * public object Invoke(Delegate method, params object[] args);
                     */
                    BeginInvoke(new show_log_info_delegate(show_log_info), new object[] { tmp.info, tmp.type });
                }
            }
        }

        #endregion

        #region 用于接收【设备】的消息线程

        /// <summary>
        /// 用于从ApController中收消息
        /// </summary>
        /// <param name="mt"></param>
        /// <param name="mb"></param>
        public static void MessageDelegate_For_ApController(MessageType mt, MessageBody mb)
        {
            strMsgInfo msgInfo;
            msgInfo.mt = mt;
            msgInfo.mb = mb;

            //string tmp = string.Format("【收到设备的消息:{0}】:\n{1}\n", gMsgFor_Ap_Controller.Count, mb.bJson);
            //Logger.Trace(LogInfoType.EROR, tmp, "Main", LogCategory.I);

            lock (mutex_Ap_Controller)
            {
                gMsgFor_Ap_Controller.Enqueue(msgInfo);
            }
        }

        /// <summary>
        /// 发送消息给ApController--Lower
        /// </summary>
        /// <param name="ap"></param>
        private void Send_Msg_2_ApCtrl_Lower(InterModuleMsgStruct ap)
        {
            strMsgInfo msgInfo = new strMsgInfo();

            msgInfo.mt = MessageType.MSG_JSON;
            msgInfo.mb.bJson = JsonConvert.SerializeObject(ap);

            add_log_info(LogInfoType.DEBG, "Main->ApCtrl:" + msgInfo.mb.bJson, "Main", LogCategory.S);
            Logger.Trace(LogInfoType.DEBG, "Main->ApCtrl:" + msgInfo.mb.bJson, "Main", LogCategory.S);

            #region 检查Type，2018-07-26

            ApInnerType apInnerType;
            if (!Enum.TryParse(ap.ApInfo.Type, true, out apInnerType))
            {
                //"Ap的内部类型错误"

                string errInfo = string.Format("ap.ApInfo.Type = {0},错误的类型.", ap.ApInfo.Type);
                ap.Body.type = AppMsgType.general_error_result;

                ap.Body.dic = new Dictionary<string, object>();
                ap.Body.dic.Add("RecvType", ap.Body.type);
                ap.Body.dic.Add("ErrStr", errInfo);

                add_log_info(LogInfoType.EROR, errInfo, "Main", LogCategory.S);
                Logger.Trace(LogInfoType.EROR, errInfo, "Main", LogCategory.S);
                Send_Msg_2_AppCtrl_Upper(ap);
                return;
            }            

            #endregion

            //将消息转发给ApController
            Delegate_SendMsg_2_ApCtrl_Lower(msgInfo.mt, msgInfo.mb);
        }

        /// <summary>
        /// 通过设备所属的域ID，设置所有设备的信息
        /// </summary>
        /// <param name="affDomainId"></param>
        /// <param name="nameFullPath"></param>
        /// <param name="app"></param>
        /// <returns>
        /// -1 : 失败
        ///  0 : 成功
        /// </returns>
        private int set_device_info_by_name_affdomainid(string name, int affDomainId, string nameFullPath, ref InterModuleMsgStruct app)
        {
            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(nameFullPath))
            {
                add_log_info(LogInfoType.EROR, "par is NULL", "Main", LogCategory.I);
                Logger.Trace(LogInfoType.EROR, "par is NULL", "Main", LogCategory.I);
                return -1;
            }

            if (name.Length > 64 || nameFullPath.Length > 1024)
            {
                add_log_info(LogInfoType.EROR, "par长度有误", "Main", LogCategory.I);
                Logger.Trace(LogInfoType.EROR, "par长度有误", "Main", LogCategory.I);
                return -1;
            }

            DataTable dt = new DataTable();

            int rv = gDbHelper.device_record_entity_get_by_name_affdomainid(name, affDomainId, ref dt);
            if (rv != 0)
            {
                add_log_info(LogInfoType.EROR, "device_record_entity_get_by_name_affdomainid失败", "Main", LogCategory.I);
                Logger.Trace(LogInfoType.EROR, "device_record_entity_get_by_name_affdomainid失败", "Main", LogCategory.I);
                return -1;
            }

            // id,name,sn,ipAddr
            // port,netmask,mode,online,lastOnline,isActive,affDomainId

            Name_DIC_Struct ndic = new Name_DIC_Struct();
            ndic.name = string.Format("{0}.{1}", nameFullPath, name);

            foreach (DataRow dr in dt.Rows)
            {
                if (string.IsNullOrEmpty(dr["sn"].ToString()))
                {
                    ndic.dic.Add("sn", "");
                }
                else
                {
                    ndic.dic.Add("sn", dr["sn"].ToString());
                }               

                if (string.IsNullOrEmpty(dr["ipAddr"].ToString()))
                {
                    ndic.dic.Add("ipAddr", "");
                }
                else
                {
                    ndic.dic.Add("ipAddr", dr["ipAddr"].ToString());
                }

                if (string.IsNullOrEmpty(dr["port"].ToString()))
                {
                    ndic.dic.Add("port", "");
                }
                else
                {
                    ndic.dic.Add("port", dr["port"].ToString());
                }

                if (string.IsNullOrEmpty(dr["netmask"].ToString()))
                {
                    ndic.dic.Add("netmask", "");
                }
                else
                {
                    ndic.dic.Add("netmask", dr["netmask"].ToString());
                }

                if (string.IsNullOrEmpty(dr["mode"].ToString()))
                {
                    ndic.dic.Add("mode", "");
                }
                else
                {
                    ndic.dic.Add("mode", dr["mode"].ToString());
                }

                if (string.IsNullOrEmpty(dr["online"].ToString()))
                {
                    ndic.dic.Add("online", "");
                }
                else
                {
                    ndic.dic.Add("online", dr["online"].ToString());
                }

                if (string.IsNullOrEmpty(dr["lastOnline"].ToString()))
                {
                    ndic.dic.Add("lastOnline", "");
                }
                else
                {
                    ndic.dic.Add("lastOnline", dr["lastOnline"].ToString());
                }

                if (string.IsNullOrEmpty(dr["isActive"].ToString()))
                {
                    ndic.dic.Add("isActive", "");
                }
                else
                {
                    ndic.dic.Add("isActive", dr["isActive"].ToString());
                }

                if (string.IsNullOrEmpty(dr["innerType"].ToString()))
                {
                    ndic.dic.Add("innerType", "");
                }
                else
                {
                    ndic.dic.Add("innerType", dr["innerType"].ToString());
                }
            }

            if (ndic.dic.Count > 0)
            {
                app.Body.n_dic.Add(ndic);
                return 0;
            }
            else
            {
                return -1;
            }
        }

        /// <summary>
        /// 通过DataTable，设置所有设备(未指派)的信息
        /// </summary>
        /// <param name="app"></param>
        /// <returns>
        /// -1 : 失败
        ///  0 : 成功
        /// </returns>
        private int set_device_unknown_info_by_datatable(DataTable dt, ref InterModuleMsgStruct app)
        {            
            if (dt == null || dt.Rows.Count == 0)
            {
                add_log_info(LogInfoType.WARN, "set_device_unknown_info_by_datatable,dt is null.", "Main", LogCategory.I);
                Logger.Trace(LogInfoType.WARN, "set_device_unknown_info_by_datatable,dt is null.", "Main", LogCategory.I);
                return -1;
            }

            // id,name,sn,ipAddr
            // port,netmask,mode,online,lastOnline,isActive,affDomainId            
            foreach (DataRow dr in dt.Rows)
            {
                Name_DIC_Struct ndic = new Name_DIC_Struct();
                ndic.name = "device_unknown";

                if (string.IsNullOrEmpty(dr["sn"].ToString()))
                {
                    ndic.dic.Add("sn", "");
                }
                else
                {
                    ndic.dic.Add("sn", dr["sn"].ToString());
                }                

                if (string.IsNullOrEmpty(dr["ipAddr"].ToString()))
                {
                    ndic.dic.Add("ipAddr", "");
                }
                else
                {
                    ndic.dic.Add("ipAddr", dr["ipAddr"].ToString());
                }

                if (string.IsNullOrEmpty(dr["port"].ToString()))
                {
                    ndic.dic.Add("port", "");
                }
                else
                {
                    ndic.dic.Add("port", dr["port"].ToString());
                }

                if (string.IsNullOrEmpty(dr["netmask"].ToString()))
                {
                    ndic.dic.Add("netmask", "");
                }
                else
                {
                    ndic.dic.Add("netmask", dr["netmask"].ToString());
                }

                if (string.IsNullOrEmpty(dr["mode"].ToString()))
                {
                    ndic.dic.Add("mode", "");
                }
                else
                {
                    ndic.dic.Add("mode", dr["mode"].ToString());
                }

                if (string.IsNullOrEmpty(dr["online"].ToString()))
                {
                    ndic.dic.Add("online", "");
                }
                else
                {
                    ndic.dic.Add("online", dr["online"].ToString());
                }

                if (string.IsNullOrEmpty(dr["lastOnline"].ToString()))
                {
                    ndic.dic.Add("lastOnline", "");
                }
                else
                {
                    ndic.dic.Add("lastOnline", dr["lastOnline"].ToString());
                }

                if (string.IsNullOrEmpty(dr["isActive"].ToString()))
                {
                    ndic.dic.Add("isActive", "");
                }
                else
                {
                    ndic.dic.Add("isActive", dr["isActive"].ToString());
                }

                if (string.IsNullOrEmpty(dr["innerType"].ToString()))
                {
                    ndic.dic.Add("innerType", "");
                }
                else
                {
                    ndic.dic.Add("innerType", dr["innerType"].ToString());
                }

                app.Body.n_dic.Add(ndic);
            }

            return 0;
        }

        /// <summary>
        /// 处理LTE的捕号消息
        /// </summary>
        /// <param name="ap"></param>
        private void lte_capture_info_process(InterModuleMsgStruct ap)
        {
            string wSelfStudy = "";
            string Fullname = ap.ApInfo.Fullname;
            strCapture cap = new strCapture();

            #region 获取信息
           
            if (DataController.SimuTest == 0)
            {
                // 非模拟器测试模式
                if (string.IsNullOrEmpty(Fullname))
                {
                    add_log_info(LogInfoType.EROR, "Fullname is NULL.", "Main", LogCategory.I);
                    Logger.Trace(LogInfoType.EROR, "Fullname is NULL.", "Main", LogCategory.I);
                    return;
                }

                if (!gDicDeviceId.ContainsKey(Fullname))
                {
                    add_log_info(LogInfoType.EROR, "gDicDeviceId的value找不到", "Main", LogCategory.I);
                    Logger.Trace(LogInfoType.EROR, "gDicDeviceId的value找不到", "Main", LogCategory.I);
                    return;
                }
                else
                {
                    cap.affDeviceId = gDicDeviceId[Fullname].id.ToString();
                    cap.name = gDicDeviceId[Fullname].name;
                    wSelfStudy = gDicDeviceId[Fullname].wSelfStudy;
                }
            }

            if (ap.Body.dic.ContainsKey("userType"))
            {
                /*
                 * 数据库中的枚举是从1开始算起的
                 */
                cap.bwFlag = (bwType)(int.Parse(ap.Body.dic["userType"].ToString()) + 1);
            }

            if (ap.Body.dic.ContainsKey("imsi"))
            {
                cap.imsi = ap.Body.dic["imsi"].ToString();
            }

            if (ap.Body.dic.ContainsKey("imei"))
            {
                cap.imei = ap.Body.dic["imei"].ToString();
            }

            if (ap.Body.dic.ContainsKey("isdn"))
            {
                cap.isdn = ap.Body.dic["isdn"].ToString();
            }

            if (ap.Body.dic.ContainsKey("tmsi"))
            {
                cap.tmsi = ap.Body.dic["tmsi"].ToString();
            }

            if (ap.Body.dic.ContainsKey("rsrp"))
            {
                cap.bsPwr = ap.Body.dic["rsrp"].ToString();
            }

            if (ap.Body.dic.ContainsKey("timestamp"))
            {
                cap.time = ap.Body.dic["timestamp"].ToString();
                //cap.time = DateTime.Now.ToString();
            }

            if (ap.Body.dic.ContainsKey("sn"))
            {
                cap.sn = ap.Body.dic["sn"].ToString();
            }

            #endregion

            #region 模拟器测试模式

            if (DataController.SimuTest == 1)
            {
                // 模拟器测试模式
                //cap.imsi = gIMSI_Index.ToString();

                gIMSI_Count++;
                if ((gIMSI_Count % 10) == 0)
                {
                    gIMSI_Index++;
                }

                cap.bwFlag = (bwType)((gIMSI_Index % 3) + 1);
                cap.affDeviceId = ((gIMSI_Index % 4) + 1).ToString();
            }

            #endregion

            #region 白名单自学习处理

            if (wSelfStudy.Equals("1"))
            {
                int rtv = gDbHelper.bwlist_record_insert(cap, int.Parse(cap.affDeviceId));
                if ((int)RC.SUCCESS != rtv)
                {
                    string errInfo = string.Format("白名单自学习:bwlist_record_insert失败{0}.", gDbHelper.get_rtv_str(rtv));
                    add_log_info(LogInfoType.INFO, errInfo, "Main", LogCategory.I);
                    Logger.Trace(LogInfoType.INFO, errInfo, "Main", LogCategory.I);
                }
                else
                {
                    string errInfo = string.Format("白名单自学习:bwlist_record_insert成功.");
                    add_log_info(LogInfoType.INFO, errInfo, "Main", LogCategory.I);
                    Logger.Trace(LogInfoType.INFO, errInfo, "Main", LogCategory.I);
                }
            }

            #endregion

            #region IMSI解析

            // 2018-07-25
            Location_And_Operator_Set(cap.imsi);

            #endregion

            #region 放入FTP处理队列

            if (DataController.StrFtpSwitch == "1")
            {
                lock (mutex_FtpHelper)
                {
                    gCaptureInfoFtp.Enqueue(cap);
                }
            }

            #endregion

            #region 放入数据库处理队列

            if (DataController.StrDbSwitch == "1")
            {
                lock (mutex_DbHelper)
                {
                    gCaptureInfoDb.Enqueue(cap);
                }
            }

            #endregion            
        }

        /// <summary>
        /// 处理GSM的捕号消息
        /// </summary>
        /// <param name="ap"></param>
        /// <param name="ndicInx"></param>
        private void gsm_capture_info_process(InterModuleMsgStruct ap,int ndicInx)
        {
            string Fullname = ap.ApInfo.Fullname;
            strCapture cap = new strCapture();
            Name_DIC_Struct ndic = ap.Body.n_dic[ndicInx];

            #region 获取信息

            if (DataController.SimuTest == 0)
            {
                // 非模拟器测试模式
                if (string.IsNullOrEmpty(Fullname))
                {
                    add_log_info(LogInfoType.EROR, "Fullname is NULL.", "Main", LogCategory.I);
                    Logger.Trace(LogInfoType.EROR, "Fullname is NULL.", "Main", LogCategory.I);
                    return;
                }

                if (!gDicDeviceId.ContainsKey(Fullname))
                {
                    add_log_info(LogInfoType.EROR, "gDicDeviceId的value找不到", "Main", LogCategory.I);
                    Logger.Trace(LogInfoType.EROR, "gDicDeviceId的value找不到", "Main", LogCategory.I);
                    return;
                }
                else
                {
                    cap.affDeviceId = gDicDeviceId[Fullname].id.ToString();
                    cap.name = gDicDeviceId[Fullname].name;
                }
            }          

            /*
             * GSM直接固定为白名单
             */
            cap.bwFlag = bwType.BWTYPE_WHITE;

            if (ndic.dic.ContainsKey("ueImsi"))
            {
                cap.imsi = ndic.dic["ueImsi"].ToString();
            }

            if (ndic.dic.ContainsKey("ueImei"))
            {
                cap.imei = ndic.dic["ueImei"].ToString();
            }

            if (ndic.dic.ContainsKey("ueMsisdn"))
            {
                cap.isdn = ndic.dic["ueMsisdn"].ToString();
            }

            if (ndic.dic.ContainsKey("ueTmsi"))
            {
                cap.tmsi = ndic.dic["ueTmsi"].ToString();
            }

            if (ndic.dic.ContainsKey("uePwr"))
            {
                cap.bsPwr = ndic.dic["uePwr"].ToString();
            }

            if (ndic.dic.ContainsKey("UeRegtype"))
            {
                cap.regType = ndic.dic["UeRegtype"].ToString();
            }

            if (ndic.dic.ContainsKey("ueQueryResult"))
            {
                cap.queryResult = ndic.dic["ueQueryResult"].ToString();
            }

            if (ndic.dic.ContainsKey("ueLlac"))
            {
                cap.localLAC = ndic.dic["ueLlac"].ToString();
            }

            if (ndic.dic.ContainsKey("ueSlac"))
            {
                cap.sourceLAC = ndic.dic["ueSlac"].ToString();
            }

            //取系统时间为时间戳
            cap.time = DateTime.Now.ToString();
            cap.sn = ap.ApInfo.SN;

            #endregion

            #region IMSI解析

            // 2018-07-25
            Location_And_Operator_Set(cap.imsi);

            #endregion

            #region 放入FTP处理队列

            if (DataController.StrFtpSwitch == "1")
            {
                lock (mutex_FtpHelper)
                {
                    gCaptureInfoFtp.Enqueue(cap);
                }
            }

            #endregion

            #region 放入数据库处理队列

            if (DataController.StrDbSwitch == "1")
            {
                lock (mutex_DbHelper)
                {
                    gCaptureInfoDb.Enqueue(cap);
                }
            }

            #endregion            
        }

        /// <summary>
        /// 处理GSM-V2/CDMA的捕号消息
        /// </summary>
        /// <param name="ap"></param>
        /// <param name="ndicInx"></param>
        private void gc_capture_info_process(InterModuleMsgStruct ap, int ndicInx)
        {
            string Fullname = ap.ApInfo.Fullname;
            strCapture cap = new strCapture();
            Name_DIC_Struct ndic = ap.Body.n_dic[ndicInx];

            #region 获取信息

            //       "name":"UE_STATUS_REPORT_MSG",                 //4.8  FAP上报UE相关状态
            //      {
            //					"imsi":XXX	    //上报imsi，如果没有为空
            //					"imei":XXX      //上报imsi，如果没有为空      
            //					"tmsi":XXX	    //上报imsi，如果没有为空       
            //					"rsrp":XXX	    //上报imsi，如果没有为空
            //                  "sn":XXX        //上报ap的Sn
            //                  "userType":XXX  //用户类型，该版本一直为空
            //       }

            if (DataController.SimuTest == 0)
            {
                // 非模拟器测试模式
                if (string.IsNullOrEmpty(Fullname))
                {
                    add_log_info(LogInfoType.EROR, "Fullname is NULL.", "Main", LogCategory.I);
                    Logger.Trace(LogInfoType.EROR, "Fullname is NULL.", "Main", LogCategory.I);
                    return;
                }

                if (!gDicDeviceId.ContainsKey(Fullname))
                {
                    add_log_info(LogInfoType.EROR, "gDicDeviceId的value找不到", "Main", LogCategory.I);
                    Logger.Trace(LogInfoType.EROR, "gDicDeviceId的value找不到", "Main", LogCategory.I);
                    return;
                }
                else
                {
                    cap.affDeviceId = gDicDeviceId[Fullname].id.ToString();
                    cap.name = gDicDeviceId[Fullname].name;
                }
            }

            /*
             * GSM-V2/CDMA直接固定为白名单(暂时)
             */
            cap.bwFlag = bwType.BWTYPE_WHITE;

            if (ndic.dic.ContainsKey("imsi"))
            {
                cap.imsi = ndic.dic["imsi"].ToString();
            }

            if (ndic.dic.ContainsKey("imei"))
            {
                cap.imei = ndic.dic["imei"].ToString();
            }
           
            cap.isdn = "";

            if (ndic.dic.ContainsKey("tmsi"))
            {
                cap.tmsi = ndic.dic["tmsi"].ToString();
            }

            if (ndic.dic.ContainsKey("rsrp"))
            {
                cap.bsPwr = ndic.dic["rsrp"].ToString();
            }           

            //取系统时间为时间戳
            cap.time = DateTime.Now.ToString();
            cap.sn = ap.ApInfo.SN;

            #endregion

            #region IMSI解析

            // 2018-07-25
            Location_And_Operator_Set(cap.imsi);

            #endregion

            #region 放入FTP处理队列

            if (DataController.StrFtpSwitch == "1")
            {
                lock (mutex_FtpHelper)
                {
                    gCaptureInfoFtp.Enqueue(cap);
                }
            }

            #endregion

            #region 放入数据库处理队列

            if (DataController.StrDbSwitch == "1")
            {
                lock (mutex_DbHelper)
                {
                    gCaptureInfoDb.Enqueue(cap);
                }
            }

            #endregion            
        }

        /// <summary>
        /// 处理WCDMA的捕号消息
        /// </summary>
        /// <param name="ap"></param>
        private void wcdma_capture_info_process(InterModuleMsgStruct ap)
        {

        }       

        /// <summary>
        /// 用于接收ApController的消息线程
        /// </summary>
        /// <param name="obj"></param>
        private void thread_for_ap_controller(object obj)
        {
            bool noMsg = false;
            strMsgInfo msgInfo;

            while (true)
            {
                if (noMsg)
                {
                    Thread.Sleep(100);
                }
                else
                {
                    //Thread.Sleep(1);
                }

                lock (mutex_Ap_Controller)
                {
                    if (gMsgFor_Ap_Controller.Count <= 0)
                    {
                        noMsg = true;
                        continue;
                    }

                    //string tmp = string.Format("【线程中处理设备的消息:{0}】\n", gMsgFor_Ap_Controller.Count);
                    //Logger.Trace(LogInfoType.EROR, tmp, "Main", LogCategory.I);

                    //循环处理从APController接收到的消息
                    msgInfo = gMsgFor_Ap_Controller.Dequeue();
                }

                noMsg = false;
                switch (msgInfo.mt)
                {
                    case MessageType.MSG_STRING:
                        {
                            add_log_info(LogInfoType.DEBG, "recv from ap controller，MSG_STRING", "Main", LogCategory.I);
                            Logger.Trace(LogInfoType.DEBG, "recv from ap controller，MSG_STRING", "Main", LogCategory.I);
                            break;
                        }
                    case MessageType.MSG_INT:
                        {
                            add_log_info(LogInfoType.DEBG, "recv from ap controller，MSG_INT", "Main", LogCategory.I);
                            Logger.Trace(LogInfoType.DEBG, "recv from ap controller，MSG_INT", "Main", LogCategory.I);

                            break;
                        }
                    case MessageType.MSG_DOUBLE:
                        {
                            add_log_info(LogInfoType.DEBG, "recv from ap controller，MSG_DOUBLE", "Main", LogCategory.I);
                            Logger.Trace(LogInfoType.DEBG, "recv from ap controller，MSG_DOUBLE", "Main", LogCategory.I);

                            break;
                        }
                    case MessageType.MSG_DATATABLE:
                        {
                            add_log_info(LogInfoType.DEBG, "recv from ap controller，MSG_DATATABLE", "Main", LogCategory.I);
                            Logger.Trace(LogInfoType.DEBG, "recv from ap controller，MSG_DATATABLE", "Main", LogCategory.I);

                            break;
                        }
                    case MessageType.MSG_XML:
                        {
                            add_log_info(LogInfoType.DEBG, "recv from ap controller，MSG_XML", "Main", LogCategory.I);
                            Logger.Trace(LogInfoType.DEBG, "recv from ap controller，MSG_XML", "Main", LogCategory.I);

                            //发送消息给ApController
                            //Delegate_sendMsg_2_ApCtrl_Lower(msgInfo.mt, msgInfo.mb);
                            break;
                        }
                    case MessageType.MSG_STATUS:
                        {
                            add_log_info(LogInfoType.DEBG, "recv from ap controller，MSG_STATUS", "Main", LogCategory.I);
                            Logger.Trace(LogInfoType.DEBG, "recv from ap controller，MSG_STATUS", "Main", LogCategory.I);


                            break;
                        }
                    case MessageType.MSG_JSON:
                        {
                            //add_log_info(LogInfoType.DEBG, "recv from ap controller，MSG_JSON:\n" + msgInfo.mb.bJson);

                            Logger.Trace(LogInfoType.INFO, "RecvFromLower，MSG_JSON:" + msgInfo.mb.bJson,"Main",LogCategory.R);
                            process_ap_controller_msg(msgInfo.mb.bJson);

                            break;
                        }
                    default:
                        {
                            add_log_info(LogInfoType.WARN, "recv from ap controller，MSG_ERROR", "Main", LogCategory.I);
                            Logger.Trace(LogInfoType.WARN, "recv from ap controller，MSG_ERROR", "Main", LogCategory.I);
                            break;
                        }
                }
            }
        }

        /// <summary>
        /// 用于处理未指派设备的心跳消息
        /// </summary>
        /// <param name="imms"></param>
        private void process_device_unknown(InterModuleMsgStruct imms)
        {
            int rtv = -1;
            string Status = "";
            string mode = "";

            strDevice dev = new strDevice();

            if (imms.Body.dic.ContainsKey("Status"))
            {
                Status = imms.Body.dic["Status"].ToString();
                if (Status != "OnLine" && Status != "OffLine")
                {
                    add_log_info(LogInfoType.EROR, "Status不为OnLine或OffLine", "Main", LogCategory.I);
                    Logger.Trace(LogInfoType.EROR, "Status不为OnLine或OffLine", "Main", LogCategory.R);

                    //返回出错处理
                    imms.Body.type = Main2ApControllerMsgType.OnOffLine_Ack;
                    imms.Body.dic = new Dictionary<string, object>();
                    imms.Body.dic.Add("ReturnCode", -1);
                    imms.Body.dic.Add("ReturnStr", get_debug_info() + "Status不为OnLine或OffLine");

                    Send_Msg_2_ApCtrl_Lower(imms);
                    return;
                }
            }
            else
            {
                add_log_info(LogInfoType.EROR, "不存在Key：Status", "Main", LogCategory.I);
                Logger.Trace(LogInfoType.EROR, "不存在Key：Status", "Main", LogCategory.R);

                //返回出错处理
                imms.Body.type = Main2ApControllerMsgType.OnOffLine_Ack;
                imms.Body.dic = new Dictionary<string, object>();
                imms.Body.dic.Add("ReturnCode", -1);
                imms.Body.dic.Add("ReturnStr", get_debug_info() + "不存在Key：Status");

                Send_Msg_2_ApCtrl_Lower(imms);
                return;
            }

            if (imms.Body.dic.ContainsKey("mode"))
            {
                mode = imms.Body.dic["mode"].ToString();
                devMode dm = gDbHelper.get_device_mode(mode);

                if (dm == devMode.MODE_UNKNOWN)
                {
                    string errInfo = string.Format("mode = {0},非法!", mode);
                    add_log_info(LogInfoType.EROR, errInfo, "Main", LogCategory.I);
                    Logger.Trace(LogInfoType.EROR, errInfo, "Main", LogCategory.R);

                    //返回出错处理
                    imms.Body.type = Main2ApControllerMsgType.OnOffLine_Ack;
                    imms.Body.dic = new Dictionary<string, object>();
                    imms.Body.dic.Add("ReturnCode", -1);
                    imms.Body.dic.Add("ReturnStr", get_debug_info() + "mode的值非法");

                    Send_Msg_2_ApCtrl_Lower(imms);
                    return;
                }                
            }
            else
            {
                add_log_info(LogInfoType.EROR, "不存在Key：mode", "Main", LogCategory.I);
                Logger.Trace(LogInfoType.EROR, "不存在Key：mode", "Main", LogCategory.R);

                //返回出错处理
                imms.Body.type = Main2ApControllerMsgType.OnOffLine_Ack;
                imms.Body.dic = new Dictionary<string, object>();
                imms.Body.dic.Add("ReturnCode", -1);
                imms.Body.dic.Add("ReturnStr", get_debug_info() + "不存在Key：mode");

                Send_Msg_2_ApCtrl_Lower(imms);
                return;
            }

            if (Status == "OnLine")
            {
                dev.online = "1";
            }
            else
            {
                dev.online = "0";
            }

            dev.sn = imms.ApInfo.SN;
            dev.ipAddr = imms.ApInfo.IP;
            dev.port = imms.ApInfo.Port.ToString();
            dev.innerType = imms.ApInfo.Type;
            dev.lastOnline = DateTime.Now.ToString();
            dev.mode = mode;

            if (Status == "OnLine")
            {
                #region 上线处理

                if ((int)RC.EXIST == gDbHelper.device_unknown_record_exist(dev.ipAddr, int.Parse(dev.port)))
                {
                    //设备(未指派)记录已经存在
                    string errInfo = string.Format("未指派设备[{0}:{1}]已经存在", dev.ipAddr, int.Parse(dev.port));
                    Logger.Trace(LogInfoType.DEBG, errInfo, "Main", LogCategory.I);                    

                    return;
                }

                //插入新记录
                rtv = gDbHelper.device_unknown_record_insert(dev.ipAddr, int.Parse(dev.port));
                if (rtv == 0)
                {
                    //更新新记录
                    rtv = gDbHelper.device_unknown_record_update(dev.ipAddr, int.Parse(dev.port), dev);
                    if (rtv == 0)
                    {
                        imms.Body.type = Main2ApControllerMsgType.OnOffLine_Ack;
                        imms.Body.dic = new Dictionary<string, object>();
                        imms.Body.dic.Add("ReturnCode", rtv);
                        imms.Body.dic.Add("ReturnStr", gDbHelper.get_rtv_str(rtv));
                        imms.Body.dic.Add("Status", Status);
                     
                        Send_Msg_2_ApCtrl_Lower(imms);

                        //Send_Msg_2_AppCtrl_Upper(imms);

                        #region 重新获取未指派设备

                        //2018-06-26
                        DataTable dt = new DataTable();
                        rtv = gDbHelper.device_unknown_record_entity_get(ref dt);

                        imms.Body.type = Main2ApControllerMsgType.app_all_device_response;
                        imms.Body.dic = new Dictionary<string, object>();
                        imms.Body.dic.Add("ReturnCode", rtv);
                        imms.Body.dic.Add("ReturnStr", gDbHelper.get_rtv_str(rtv));

                        if (rtv == 0)
                        {
                            if (dt.Rows.Count > 0)
                            {
                                imms.Body.n_dic = new List<Name_DIC_Struct>();
                                set_device_unknown_info_by_datatable(dt, ref imms);
                            }
                        }

                        string info = string.Format("发送app_all_device_response给AppCtrl,未指派个数{0}", dt.Rows.Count);
                        Logger.Trace(LogInfoType.DEBG, info, "Main", LogCategory.S);

                        //发送给界面去更新未指派设备信息
                        Send_Msg_2_AppCtrl_Upper(imms);

                        #endregion
                    }
                    else
                    {
                        //出错处理
                        imms.Body.type = Main2ApControllerMsgType.OnOffLine_Ack;
                        imms.Body.dic = new Dictionary<string, object>();
                        imms.Body.dic.Add("ReturnCode", rtv);
                        imms.Body.dic.Add("ReturnStr", get_debug_info() + gDbHelper.get_rtv_str(rtv));

                        Send_Msg_2_ApCtrl_Lower(imms);
                    }
                }
                else
                {
                    //出错处理
                    imms.Body.type = Main2ApControllerMsgType.OnOffLine_Ack;
                    imms.Body.dic = new Dictionary<string, object>();
                    imms.Body.dic.Add("ReturnCode", rtv);
                    imms.Body.dic.Add("ReturnStr", get_debug_info() + gDbHelper.get_rtv_str(rtv));

                    Send_Msg_2_ApCtrl_Lower(imms);
                }

                #endregion
            }
            else
            {
                #region 下线处理

                if ((int)RC.NO_EXIST == gDbHelper.device_unknown_record_exist(dev.ipAddr, int.Parse(dev.port)))
                {
                    //设备(未指派)记录不存在

                    return;
                }

                //更新新记录
                rtv = gDbHelper.device_unknown_record_delete(dev.ipAddr, int.Parse(dev.port));
                if (rtv == 0)
                {
                    imms.Body.type = Main2ApControllerMsgType.OnOffLine_Ack;
                    imms.Body.dic = new Dictionary<string, object>();
                    imms.Body.dic.Add("ReturnCode", rtv);
                    imms.Body.dic.Add("ReturnStr", gDbHelper.get_rtv_str(rtv));
                    imms.Body.dic.Add("Status", Status);

                    Send_Msg_2_ApCtrl_Lower(imms);

                    #region 重新获取未指派设备

                    //2018-06-26
                    DataTable dt = new DataTable();
                    rtv = gDbHelper.device_unknown_record_entity_get(ref dt);
                                       
                    imms.Body.type = Main2ApControllerMsgType.app_all_device_response;
                    imms.Body.dic = new Dictionary<string, object>();
                    imms.Body.dic.Add("ReturnCode", rtv);
                    imms.Body.dic.Add("ReturnStr", gDbHelper.get_rtv_str(rtv));                    

                    if (rtv == 0)
                    {
                        if (dt.Rows.Count > 0)
                        {
                            imms.Body.n_dic = new List<Name_DIC_Struct>();
                            set_device_unknown_info_by_datatable(dt, ref imms);
                        }
                    }

                    string info = string.Format("发送app_all_device_response给AppCtrl,未指派个数{0}", dt.Rows.Count);
                    Logger.Trace(LogInfoType.DEBG, info, "Main", LogCategory.S);

                    //发送给APP去更新未指派设备信息
                    Send_Msg_2_AppCtrl_Upper(imms);

                    #endregion
                }
                else
                {
                    //出错处理
                    imms.Body.type = Main2ApControllerMsgType.OnOffLine_Ack;
                    imms.Body.dic = new Dictionary<string, object>();
                    imms.Body.dic.Add("ReturnCode", rtv);
                    imms.Body.dic.Add("ReturnStr", get_debug_info() + gDbHelper.get_rtv_str(rtv));

                    Send_Msg_2_ApCtrl_Lower(imms);
                }

                #endregion
            }

            return;
        }

        /// <summary>
        /// 获取GSM相关的参数
        /// </summary>
        /// <param name="imms"></param>
        /// <param name="all"></param>
        /// <returns></returns>
        private int get_gsm_info(InterModuleMsgStruct imms, ref str_Gsm_All_Para all)
        {
            if (imms.Body.n_dic.Count <= 0)
            {
                return -1;
            }

            if (!imms.Body.dic.ContainsKey("sys"))
            {
                return -1;
            }
            else
            {
                all.sys = int.Parse(imms.Body.dic["sys"].ToString());

                if (all.sys != 0 && all.sys != 1)
                {
                    return -1;
                }
            }

            if (imms.Body.dic.ContainsKey("hardware_id"))
            {
                all.hardware_id = int.Parse(imms.Body.dic["hardware_id"].ToString());
            }           

            for (int i = 0; i < imms.Body.n_dic.Count; i++)
            {
                string name = imms.Body.n_dic[i].name;

                switch (name)
                {
                    case "RECV_SYS_PARA":
                        {
                            #region RECV_SYS_PARA

                            //					"paraMcc":移动国家码
                            //					"paraMnc":移动网号
                            //					"paraBsic":基站识别码
                            //					"paraLac":位置区号
                            //					"paraCellId":小区ID
                            //					"paraC2":C2偏移量
                            //					"paraPeri":周期性位置更新周期
                            //					"paraAccPwr":接入功率
                            //					"paraMsPwr":手机发射功率
                            //					"paraRejCau":位置更新拒绝原因

                            all.gsmSysParaFlag = true;
                            if (imms.Body.n_dic[i].dic.ContainsKey("paraMcc"))
                            {
                                if (imms.Body.n_dic[i].dic["paraMcc"].ToString() != "")
                                {
                                    all.gsmSysPara.paraMcc = imms.Body.n_dic[i].dic["paraMcc"].ToString();
                                }
                            }

                            if (imms.Body.n_dic[i].dic.ContainsKey("paraMnc"))
                            {
                                if (imms.Body.n_dic[i].dic["paraMnc"].ToString() != "")
                                {
                                    all.gsmSysPara.paraMnc = imms.Body.n_dic[i].dic["paraMnc"].ToString();
                                }
                            }

                            if (imms.Body.n_dic[i].dic.ContainsKey("paraBsic"))
                            {
                                if (imms.Body.n_dic[i].dic["paraBsic"].ToString() != "")
                                {
                                    all.gsmSysPara.paraBsic = imms.Body.n_dic[i].dic["paraBsic"].ToString();
                                }
                            }

                            if (imms.Body.n_dic[i].dic.ContainsKey("paraLac"))
                            {
                                if (imms.Body.n_dic[i].dic["paraLac"].ToString() != "")
                                {
                                    all.gsmSysPara.paraLac = imms.Body.n_dic[i].dic["paraLac"].ToString();
                                }
                            }

                            if (imms.Body.n_dic[i].dic.ContainsKey("paraCellId"))
                            {
                                if (imms.Body.n_dic[i].dic["paraCellId"].ToString() != "")
                                {
                                    all.gsmSysPara.paraCellId = imms.Body.n_dic[i].dic["paraCellId"].ToString();
                                }
                            }

                            if (imms.Body.n_dic[i].dic.ContainsKey("paraC2"))
                            {
                                if (imms.Body.n_dic[i].dic["paraC2"].ToString() != "")
                                {
                                    all.gsmSysPara.paraC2 = imms.Body.n_dic[i].dic["paraC2"].ToString();
                                }
                            }

                            if (imms.Body.n_dic[i].dic.ContainsKey("paraPeri"))
                            {
                                if (imms.Body.n_dic[i].dic["paraPeri"].ToString() != "")
                                {
                                    all.gsmSysPara.paraPeri = imms.Body.n_dic[i].dic["paraPeri"].ToString();
                                }
                            }

                            if (imms.Body.n_dic[i].dic.ContainsKey("paraAccPwr"))
                            {
                                if (imms.Body.n_dic[i].dic["paraAccPwr"].ToString() != "")
                                {
                                    all.gsmSysPara.paraAccPwr = imms.Body.n_dic[i].dic["paraAccPwr"].ToString();
                                }
                            }

                            if (imms.Body.n_dic[i].dic.ContainsKey("paraMsPwr"))
                            {
                                if (imms.Body.n_dic[i].dic["paraMsPwr"].ToString() != "")
                                {
                                    all.gsmSysPara.paraMsPwr = imms.Body.n_dic[i].dic["paraMsPwr"].ToString();
                                }
                            }

                            if (imms.Body.n_dic[i].dic.ContainsKey("paraRejCau"))
                            {
                                if (imms.Body.n_dic[i].dic["paraRejCau"].ToString() != "")
                                {
                                    all.gsmSysPara.paraRejCau = imms.Body.n_dic[i].dic["paraRejCau"].ToString();
                                }
                            }

                            break;

                            #endregion
                        }
                    case "RECV_SYS_OPTION":
                        {
                            #region RECV_SYS_OPTION

                            //                  "opLuSms":登录时发送短信
                            //					"opLuImei":登录时获取IMEI
                            //					"opCallEn":允许用户主叫
                            //					"opDebug":调试模式，上报信令
                            //					"opLuType":登录类型
                            //					"opSmsType":短信类型

                            all.gsmSysOptionFlag = true;
                            if (imms.Body.n_dic[i].dic.ContainsKey("opLuSms"))
                            {
                                if (imms.Body.n_dic[i].dic["opLuSms"].ToString() != "")
                                {
                                    all.gsmSysOption.opLuSms = imms.Body.n_dic[i].dic["opLuSms"].ToString();
                                }
                            }

                            if (imms.Body.n_dic[i].dic.ContainsKey("opLuImei"))
                            {
                                if (imms.Body.n_dic[i].dic["opLuImei"].ToString() != "")
                                {
                                    all.gsmSysOption.opLuImei = imms.Body.n_dic[i].dic["opLuImei"].ToString();
                                }
                            }

                            if (imms.Body.n_dic[i].dic.ContainsKey("opCallEn"))
                            {
                                if (imms.Body.n_dic[i].dic["opCallEn"].ToString() != "")
                                {
                                    all.gsmSysOption.opCallEn = imms.Body.n_dic[i].dic["opCallEn"].ToString();
                                }
                            }

                            if (imms.Body.n_dic[i].dic.ContainsKey("opDebug"))
                            {
                                if (imms.Body.n_dic[i].dic["opDebug"].ToString() != "")
                                {
                                    all.gsmSysOption.opDebug = imms.Body.n_dic[i].dic["opDebug"].ToString();
                                }
                            }

                            if (imms.Body.n_dic[i].dic.ContainsKey("opLuType"))
                            {
                                if (imms.Body.n_dic[i].dic["opLuType"].ToString() != "")
                                {
                                    all.gsmSysOption.opLuType = imms.Body.n_dic[i].dic["opLuType"].ToString();
                                }
                            }

                            if (imms.Body.n_dic[i].dic.ContainsKey("opSmsType"))
                            {
                                if (imms.Body.n_dic[i].dic["opSmsType"].ToString() != "")
                                {
                                    all.gsmSysOption.opSmsType = imms.Body.n_dic[i].dic["opSmsType"].ToString();
                                }
                            }

                            break;

                            #endregion
                        }
                    case "RECV_RF_PARA":
                        {
                            #region RECV_RF_PARA

                            //					"rfEnable":射频使能
                            //					"rfFreq":信道号
                            //					"rfPwr":发射功率衰减值

                            all.gsmRfParaFlag = true;
                            if (imms.Body.n_dic[i].dic.ContainsKey("rfEnable"))
                            {
                                if (imms.Body.n_dic[i].dic["rfEnable"].ToString() != "")
                                {
                                    all.gsmRfPara.rfEnable = imms.Body.n_dic[i].dic["rfEnable"].ToString();
                                }
                            }

                            if (imms.Body.n_dic[i].dic.ContainsKey("rfFreq"))
                            {
                                if (imms.Body.n_dic[i].dic["rfFreq"].ToString() != "")
                                {
                                    all.gsmRfPara.rfFreq = imms.Body.n_dic[i].dic["rfFreq"].ToString();
                                }
                            }

                            if (imms.Body.n_dic[i].dic.ContainsKey("rfPwr"))
                            {
                                if (imms.Body.n_dic[i].dic["rfPwr"].ToString() != "")
                                {
                                    all.gsmRfPara.rfPwr = imms.Body.n_dic[i].dic["rfPwr"].ToString();
                                }
                            }

                            break;

                            #endregion
                        }
                    case "RECV_SMS_OPTION":
                        {
                            #region RECV_SMS_OPTION

                            //          "gSmsRpoa":短消息中心号码
                            //          "gSmsTpoa":短消息原叫号码
                            //          "gSmsScts":短消息发送时间 （时间格式为年/月/日/时/分/秒各两位，不足两位前补0。如2014年4月22日15点46分47秒的消息内容为“140422154647”）
                            //          "gSmsData":短消息内容 （编码格式为Unicode编码）
                            //          "autoSendtiny":是否自动发送
                            //          "autoFilterSMStiny":是否自动过滤短信
                            //          "delayTime":发送延时时间
                            //          "smsCodingtiny":短信的编码格式

                            all.gsmMsgOptionFlag = true;
                            if (imms.Body.n_dic[i].dic.ContainsKey("gSmsRpoa"))
                            {
                                if (imms.Body.n_dic[i].dic["gSmsRpoa"].ToString() != "")
                                {
                                    all.gsmMsgOption.smsRPOA = imms.Body.n_dic[i].dic["gSmsRpoa"].ToString();
                                }
                            }

                            if (imms.Body.n_dic[i].dic.ContainsKey("gSmsTpoa"))
                            {
                                if (imms.Body.n_dic[i].dic["gSmsTpoa"].ToString() != "")
                                {
                                    all.gsmMsgOption.smsTPOA = imms.Body.n_dic[i].dic["gSmsTpoa"].ToString();
                                }
                            }

                            if (imms.Body.n_dic[i].dic.ContainsKey("gSmsScts"))
                            {
                                if (imms.Body.n_dic[i].dic["gSmsScts"].ToString() != "")
                                {
                                    all.gsmMsgOption.smsSCTS = imms.Body.n_dic[i].dic["gSmsScts"].ToString();
                                }
                            }

                            if (imms.Body.n_dic[i].dic.ContainsKey("gSmsData"))
                            {
                                if (imms.Body.n_dic[i].dic["gSmsData"].ToString() != "")
                                {
                                    all.gsmMsgOption.smsDATA = imms.Body.n_dic[i].dic["gSmsData"].ToString();
                                }
                            }

                            if (imms.Body.n_dic[i].dic.ContainsKey("autoSendtiny"))
                            {
                                if (imms.Body.n_dic[i].dic["autoSendtiny"].ToString() != "")
                                {
                                    all.gsmMsgOption.autoSend = imms.Body.n_dic[i].dic["autoSendtiny"].ToString();
                                }
                            }
                            else
                            {
                                all.gsmMsgOption.autoSend = "0";
                            }

                            if (imms.Body.n_dic[i].dic.ContainsKey("autoFilterSMStiny"))
                            {
                                if (imms.Body.n_dic[i].dic["autoFilterSMStiny"].ToString() != "")
                                {
                                    all.gsmMsgOption.autoFilterSMS = imms.Body.n_dic[i].dic["autoFilterSMStiny"].ToString();
                                }
                            }
                            else
                            {
                                all.gsmMsgOption.autoFilterSMS = "0";
                            }

                            if (imms.Body.n_dic[i].dic.ContainsKey("delayTime"))
                            {
                                if (imms.Body.n_dic[i].dic["delayTime"].ToString() != "")
                                {
                                    all.gsmMsgOption.delayTime = imms.Body.n_dic[i].dic["delayTime"].ToString();
                                }
                            }
                            else
                            {
                                all.gsmMsgOption.delayTime = "0";
                            }

                            if (imms.Body.n_dic[i].dic.ContainsKey("smsCodingtiny"))
                            {
                                if (imms.Body.n_dic[i].dic["smsCodingtiny"].ToString() != "")
                                {
                                    all.gsmMsgOption.smsCoding = imms.Body.n_dic[i].dic["smsCodingtiny"].ToString();
                                }
                            }

                            break;

                            #endregion
                        }
                    case "RECV_REG_MODE":
                        {
                            #region RECV_REG_MODE

                            //          "regMode":模式0时由设备自行根据系统选项决定是否允许终端入网，是否对终端发送短信；
                            //                    模式1时设备将终端标识发送给上位机，由上位机告知设备下一步的动作

                            all.gsmSysOptionFlag = true;
                            if (imms.Body.n_dic[i].dic.ContainsKey("regMode"))
                            {
                                if (imms.Body.n_dic[i].dic["regMode"].ToString() != "")
                                {
                                    all.gsmSysOption.opRegModel = imms.Body.n_dic[i].dic["regMode"].ToString();
                                }
                            }

                            break;

                            #endregion
                        }
                    default:
                        {
                            add_log_info(LogInfoType.EROR, "get_gsm_info包含非法的ndic name\n", "Main", LogCategory.I);
                            Logger.Trace(LogInfoType.EROR, "get_gsm_info包含非法的ndic name\n", "Main", LogCategory.R);
                            break;
                        }
                }                          
            }

            return 0;
        }

        /// <summary>
        /// 获取GSM-V2/CDMA相关的参数
        /// </summary>
        /// <param name="imms"></param>
        /// <param name="all"></param>
        /// <returns></returns>
        private int get_gc_info(InterModuleMsgStruct imms, ref str_GC_All_Para all,ref string errInfo)
        {
            if (imms.Body.n_dic.Count <= 0)
            {
                errInfo = string.Format("n_dic中没任何内容.");
                return -1;
            }

            if (!imms.Body.dic.ContainsKey("Protocol"))
            {
                all.Protocol = "GSM";
            }
            else
            {
                all.Protocol = imms.Body.dic["Protocol"].ToString();
                if (all.Protocol != "GSM" && all.Protocol != "CDMA")
                {
                    errInfo = string.Format("不支持的Protocol = {0}",all.Protocol);
                    return -1;
                }
            }
            
            if (!imms.Body.dic.ContainsKey("sys"))
            {
                errInfo = string.Format("没包含sys字段.");
                return -1;
            }
            else
            {
                all.sys = int.Parse(imms.Body.dic["sys"].ToString());
                if (all.sys != 0 && all.sys != 1)
                {
                    errInfo = string.Format("不支持的sys = {0}", all.sys);
                    return -1;
                }
            }

            //if (imms.Body.dic.ContainsKey("hardware_id"))
            //{
            //    all.hardware_id = int.Parse(imms.Body.dic["hardware_id"].ToString());
            //}

            for (int i = 0; i < imms.Body.n_dic.Count; i++)
            {
                string name = imms.Body.n_dic[i].name;

                switch (name)
                {
                    case "FAP_NB_CELL_INFO_MSG":
                        {
                            #region FAP_NB_CELL_INFO_MSG

                            //
                            // 该消息暂时不用了
                            //
                            //	"bFapNbCellNum":n	         邻小区个数。最多16个(n<=16)
                            //  "Cell_#n#/bGCId":XXX         小区ID。注意在CDMA制式没有小区ID，高位WORD是SID，低位WORD是NID
                            //	"Cell_#n#/bPLMNId":XXX       邻小区PLMN标志。
                            //	"Cell_#n#/cRSRP":XXX	     信号功率
                            //	"Cell_#n#/wTac":XXX	         追踪区域码。GSM：LAC；CDMA：REG_ZONE
                            //	"Cell_#n#/wPhyCellId":XXX	 物理小区ID。GSM：BSIC；CDMA：PN
                            //	"Cell_#n#/wUARFCN":XXX	     小区频点
                            //	"Cell_#n#/cRefTxPower":XXX	 参考发射功率。GSM制式时为C1测量值
                            //	"Cell_#n#/bNbCellNum":XXX	 邻小区的令小区个数
                            //	"Cell_#n#/bC2":XXX	         C2测量值。GSM,其他制式保留
                            //	"Cell_#n#/bReserved1":XXX	 只用于LTE,其它保留
                            //	"Cell_#n#/stNbCell":m		 邻小区的邻小区个数，最多32个（m<=32）
                            //	"Cell_#n#/NeighCell_#m#/wUarfcn":XXX	    小区频点
                            //	"Cell_#n#/NeighCell_#m#/wPhyCellId":XXX	    物理小区ID。GSM:BSIC；CDMA：PN
                            //	"Cell_#n#/NeighCell_#m#/cRSRP":XXX	        信号功率
                            //	"Cell_#n#/NeighCell_#m#/cC1":XXX	        C1测量值。只用于GSM制式
                            //	"Cell_#n#/NeighCell_#m#/bC2":XXX	        C2测量值。只用于GSM制式

                            int bFapNbCellNum = -1;
                            if (imms.Body.n_dic[i].dic.ContainsKey("bFapNbCellNum"))
                            {
                                if (imms.Body.n_dic[i].dic["bFapNbCellNum"].ToString() != "")
                                {
                                    bFapNbCellNum = int.Parse(imms.Body.n_dic[i].dic["bFapNbCellNum"].ToString());
                                    if (bFapNbCellNum <= 0 || bFapNbCellNum > 16)
                                    {
                                        errInfo = string.Format("bFapNbCellNum = {0},越界.", bFapNbCellNum);
                                        return -1;
                                    }
                                }
                            }
                            else
                            {
                                errInfo = string.Format("不包含bFapNbCellNum字段.");
                                return -1;
                            }

                            string field = "";
                            all.gcNbCellFlag = true;
                            all.listGcNbCell = new List<strGcNbCell>();

                            for (int j = 1; j <= bFapNbCellNum; j++)
                            {
                                strGcNbCell str = new strGcNbCell();

                                field = string.Format("Cell_#{0}#/bGCId", j);
                                if (imms.Body.n_dic[i].dic.ContainsKey(field))
                                {
                                    str.bGCId = imms.Body.n_dic[i].dic.ContainsKey(field).ToString();
                                }

                                field = string.Format("Cell_#{0}#/bPLMNId", j);
                                if (imms.Body.n_dic[i].dic.ContainsKey(field))
                                {
                                    str.bPLMNId = imms.Body.n_dic[i].dic.ContainsKey(field).ToString();
                                }

                                field = string.Format("Cell_#{0}#/cRSRP", j);
                                if (imms.Body.n_dic[i].dic.ContainsKey(field))
                                {
                                    str.cRSRP = imms.Body.n_dic[i].dic.ContainsKey(field).ToString();
                                }

                                field = string.Format("Cell_#{0}#/wTac", j);
                                if (imms.Body.n_dic[i].dic.ContainsKey(field))
                                {
                                    str.wTac = imms.Body.n_dic[i].dic.ContainsKey(field).ToString();
                                }

                                field = string.Format("Cell_#{0}#/wPhyCellId", j);
                                if (imms.Body.n_dic[i].dic.ContainsKey(field))
                                {
                                    str.wPhyCellId = imms.Body.n_dic[i].dic.ContainsKey(field).ToString();
                                }

                                field = string.Format("Cell_#{0}#/wUARFCN", j);
                                if (imms.Body.n_dic[i].dic.ContainsKey(field))
                                {
                                    str.wUARFCN = imms.Body.n_dic[i].dic.ContainsKey(field).ToString();
                                }

                                field = string.Format("Cell_#{0}#/cRefTxPower", j);
                                if (imms.Body.n_dic[i].dic.ContainsKey(field))
                                {
                                    str.cRefTxPower = imms.Body.n_dic[i].dic.ContainsKey(field).ToString();
                                }

                                field = string.Format("Cell_#{0}#/bNbCellNum", j);
                                if (imms.Body.n_dic[i].dic.ContainsKey(field))
                                {
                                    str.bNbCellNum = imms.Body.n_dic[i].dic.ContainsKey(field).ToString();
                                }

                                field = string.Format("Cell_#{0}#/bC2", j);
                                if (imms.Body.n_dic[i].dic.ContainsKey(field))
                                {
                                    str.bC2 = imms.Body.n_dic[i].dic.ContainsKey(field).ToString();
                                }

                                field = string.Format("Cell_#{0}#/bReserved1", j);
                                if (imms.Body.n_dic[i].dic.ContainsKey(field))
                                {
                                    str.bReserved1 = imms.Body.n_dic[i].dic.ContainsKey(field).ToString();
                                }

                                int m = -1;
                                field = string.Format("Cell_#{0}#/stNbCell", j);
                                if (imms.Body.n_dic[i].dic.ContainsKey(field))
                                {
                                    m = int.Parse(imms.Body.n_dic[i].dic.ContainsKey(field).ToString());
                                    if (m < 0 || m > 32)
                                    {
                                        errInfo = string.Format("stNbCell = {0},越界.", m);
                                        return -1;
                                    }
                                }

                                //					"Cell_#n#/NeighCell_#m#/wUarfcn":XXX	    小区频点
                                //					"Cell_#n#/NeighCell_#m#/wPhyCellId":XXX	    物理小区ID。GSM:BSIC；CDMA：PN
                                //					"Cell_#n#/NeighCell_#m#/cRSRP":XXX	        信号功率
                                //					"Cell_#n#/NeighCell_#m#/cC1":XXX	        C1测量值。只用于GSM制式
                                //					"Cell_#n#/NeighCell_#m#/bC2":XXX	        C2测量值。只用于GSM制式

                                str.listItem = new List<strGcNbCellItem>();
                                for (int k = 1; k <= m; k++)
                                {
                                    strGcNbCellItem item = new strGcNbCellItem();

                                    field = string.Format("Cell_#{0}#/NeighCell_#{1}#/wUarfcn", j,k);
                                    if (imms.Body.n_dic[i].dic.ContainsKey(field))
                                    {
                                        item.wUarfcn = imms.Body.n_dic[i].dic.ContainsKey(field).ToString();
                                    }

                                    field = string.Format("Cell_#{0}#/NeighCell_#{1}#/wPhyCellId", j, k);
                                    if (imms.Body.n_dic[i].dic.ContainsKey(field))
                                    {
                                        item.wPhyCellId = imms.Body.n_dic[i].dic.ContainsKey(field).ToString();
                                    }

                                    field = string.Format("Cell_#{0}#/NeighCell_#{1}#/cRSRP", j, k);
                                    if (imms.Body.n_dic[i].dic.ContainsKey(field))
                                    {
                                        item.cRSRP = imms.Body.n_dic[i].dic.ContainsKey(field).ToString();
                                    }

                                    field = string.Format("Cell_#{0}#/NeighCell_#{1}#/cC1", j, k);
                                    if (imms.Body.n_dic[i].dic.ContainsKey(field))
                                    {
                                        item.cC1 = imms.Body.n_dic[i].dic.ContainsKey(field).ToString();
                                    }

                                    field = string.Format("Cell_#{0}#/NeighCell_#{1}#/wUarfcn", j, k);
                                    if (imms.Body.n_dic[i].dic.ContainsKey(field))
                                    {
                                        item.bC2 = imms.Body.n_dic[i].dic.ContainsKey(field).ToString();
                                    }

                                    str.listItem.Add(item);
                                }

                                all.listGcNbCell.Add(str);
                            }                  

                            break;

                            #endregion
                        }                        
                    case "CONFIG_FAP_MSG":
                        {
                            #region CONFIG_FAP_MSG

                            //	"bWorkingMode":XXX		    工作模式:1 为侦码模式 ;3驻留模式.
                            //	"bC":XXX		            是否自动切换模式。保留
                            //	"wRedirectCellUarfcn":XXX	CDMA黑名单频点
                            //	"dwDateTime":XXX			当前时间	
                            //	"bPLMNId":XXX		        PLMN标志
                            //	"bTxPower":XXX			    实际发射功率.设置发射功率衰减寄存器, 0输出最大功率, 每增加1, 衰减1DB
                            //	"bRxGain":XXX			    接收信号衰减寄存器. 每增加1增加1DB的增益
                            //	"wPhyCellId":XXX		    物理小区ID.
                            //	"wLAC":XXX			        追踪区域码。GSM：LAC;CDMA：REG_ZONE
                            //	"wUARFCN":XXX			    小区频点. CDMA 制式为BSID
                            //	"dwCellId":XXX			    小区ID。注意在CDMA制式没有小区ID，高位WORD 是SID ， 低位WORD 是NID

                            all.gcParamConfigFlag = true;

                            //(1)
                            if (imms.Body.n_dic[i].dic.ContainsKey("bWorkingMode"))
                            {
                                all.gcParamConfig.bWorkingMode = imms.Body.n_dic[i].dic["bWorkingMode"].ToString();
                            }

                            //(2)
                            if (imms.Body.n_dic[i].dic.ContainsKey("bC"))
                            {
                                all.gcParamConfig.bC = imms.Body.n_dic[i].dic["bC"].ToString();
                            }

                            //(3)
                            if (imms.Body.n_dic[i].dic.ContainsKey("wRedirectCellUarfcn"))
                            {
                                all.gcParamConfig.wRedirectCellUarfcn = imms.Body.n_dic[i].dic["wRedirectCellUarfcn"].ToString();
                            }

                            //(4)
                            if (imms.Body.n_dic[i].dic.ContainsKey("dwDateTime"))
                            {
                                all.gcParamConfig.dwDateTime = imms.Body.n_dic[i].dic["dwDateTime"].ToString();
                            }

                            //(5)
                            if (imms.Body.n_dic[i].dic.ContainsKey("bPLMNId"))
                            {
                                all.gcParamConfig.bPLMNId = imms.Body.n_dic[i].dic["bPLMNId"].ToString();
                            }

                            //(6)
                            if (imms.Body.n_dic[i].dic.ContainsKey("bTxPower"))
                            {
                                all.gcParamConfig.bTxPower = imms.Body.n_dic[i].dic["bTxPower"].ToString();
                            }

                            //(7)
                            if (imms.Body.n_dic[i].dic.ContainsKey("bRxGain"))
                            {
                                all.gcParamConfig.bRxGain = imms.Body.n_dic[i].dic["bRxGain"].ToString();
                            }

                            //(8)
                            if (imms.Body.n_dic[i].dic.ContainsKey("wPhyCellId"))
                            {
                                all.gcParamConfig.wPhyCellId = imms.Body.n_dic[i].dic["wPhyCellId"].ToString();
                            }

                            //(9)
                            if (imms.Body.n_dic[i].dic.ContainsKey("wLAC"))
                            {
                                all.gcParamConfig.wLAC = imms.Body.n_dic[i].dic["wLAC"].ToString();
                            }

                            //(10)
                            if (imms.Body.n_dic[i].dic.ContainsKey("wUARFCN"))
                            {
                                all.gcParamConfig.wUARFCN = imms.Body.n_dic[i].dic["wUARFCN"].ToString();
                            }

                            //(11)
                            if (imms.Body.n_dic[i].dic.ContainsKey("dwCellId"))
                            {
                                all.gcParamConfig.dwCellId = imms.Body.n_dic[i].dic["dwCellId"].ToString();
                            }
                          
                            break;

                            #endregion
                        }
                    case "FAP_TRACE_MSG":
                        {
                            #region FAP_TRACE_MSG

                            //	"wTraceLen":XXX	      Trace长度
                            //   "cTrace":XXX          Trace内容

                            if (all.gcMiscFlag != true)
                            {
                                all.gcMiscFlag = true;
                                all.gcMisc = new strGcMisc();
                            }

                            if (imms.Body.n_dic[i].dic.ContainsKey("wTraceLen"))
                            {                             
                                all.gcMisc.wTraceLen = imms.Body.n_dic[i].dic["wTraceLen"].ToString();                               
                            }

                            if (imms.Body.n_dic[i].dic.ContainsKey("cTrace"))
                            {
                                all.gcMisc.cTrace = imms.Body.n_dic[i].dic["cTrace"].ToString();
                            }

                            break;

                            #endregion
                        }
                    case "UE_ORM_REPORT_MSG":
                        {
                            #region UE_ORM_REPORT_MSG

                            //	"bOrmType":XXX	    	主叫类型。1=呼叫号码, 2=短消息PDU,3=寻呼测量
                            //	"bUeId":XXX	     	    IMSI
                            //	"cRSRP":XXX	    	    接收信号强度。寻呼测量时，-128表示寻呼失败
                            //	"bUeContentLen":XXX	    Ue主叫内容长度
                            //	"bUeContent":XXX	    Ue主叫内容。最大249字节。

                            if (all.gcMiscFlag != true)
                            {
                                all.gcMiscFlag = true;
                                all.gcMisc = new strGcMisc();
                            }

                            if (imms.Body.n_dic[i].dic.ContainsKey("bOrmType"))
                            {
                                all.gcMisc.bOrmType = imms.Body.n_dic[i].dic["bOrmType"].ToString();
                            }

                            if (imms.Body.n_dic[i].dic.ContainsKey("bUeId"))
                            {
                                all.gcMisc.bUeId = imms.Body.n_dic[i].dic["bUeId"].ToString();
                            }

                            if (imms.Body.n_dic[i].dic.ContainsKey("cRSRP"))
                            {
                                all.gcMisc.cRSRP = imms.Body.n_dic[i].dic["cRSRP"].ToString();
                            }

                            if (imms.Body.n_dic[i].dic.ContainsKey("bUeContentLen"))
                            {
                                all.gcMisc.bUeContentLen = imms.Body.n_dic[i].dic["bUeContentLen"].ToString();
                            }

                            if (imms.Body.n_dic[i].dic.ContainsKey("bUeContent"))
                            {
                                all.gcMisc.bUeContent = imms.Body.n_dic[i].dic["bUeContent"].ToString();
                            }

                            break;

                            #endregion
                        }
                    case "CONFIG_SMS_CONTENT_MSG":
                        {
                            #region CONFIG_SMS_CONTENT_MSG

                            //	"bSMSOriginalNumLen":XXX	    主叫号码长度
                            //	"bSMSOriginalNum":XXX	    	主叫号码
                            //	"bSMSContentLen":XXX	    	短信内容字数
                            //	"bSMSContent":XXX	            短信内容.unicode编码，每个字符占2字节

                            if (all.gcMiscFlag != true)
                            {
                                all.gcMiscFlag = true;
                                all.gcMisc = new strGcMisc();
                            }

                            if (imms.Body.n_dic[i].dic.ContainsKey("bSMSOriginalNumLen"))
                            {
                                all.gcMisc.bSMSOriginalNumLen = imms.Body.n_dic[i].dic["bSMSOriginalNumLen"].ToString();
                            }

                            if (imms.Body.n_dic[i].dic.ContainsKey("bSMSOriginalNum"))
                            {
                                all.gcMisc.bSMSOriginalNum = imms.Body.n_dic[i].dic["bSMSOriginalNum"].ToString();
                            }

                            if (imms.Body.n_dic[i].dic.ContainsKey("bSMSContentLen"))
                            {
                                all.gcMisc.bSMSContentLen = imms.Body.n_dic[i].dic["bSMSContentLen"].ToString();
                            }

                            if (imms.Body.n_dic[i].dic.ContainsKey("bSMSContent"))
                            {
                                all.gcMisc.bSMSContent = imms.Body.n_dic[i].dic["bSMSContent"].ToString();
                            }                            

                            break;

                            #endregion
                        }
                    case "CONFIG_CDMA_CARRIER_MSG":
                        {
                            #region CONFIG_CDMA_CARRIER_MSG                            

                            //	"wARFCN1":XXX	        工作频点1	
                            //	"bARFCN1Mode":XXX	    工作频点1模式。0表示扫描，1表示常开,2表示关闭。
                            //	"wARFCN1Duration":XXX	工作频点1扫描时长
                            //	"wARFCN1Period":XXX	    工作频点1扫描间隔

                            //	"wARFCN2":XXX	        工作频点2
                            //	"bARFCN2Mode":XXX	    工作频点2模式。 0表示扫描，1表示常开,2表示关闭。
                            //	"wARFCN2Duration":XXX	工作频点2扫描时长
                            //	"wARFCN2Period":XXX	    工作频点2扫描间隔

                            //	"wARFCN3":XXX	        工作频点3	
                            //	"bARFCN3Mode":XXX	    工作频点3模式。 0表示扫描，1表示常开,2表示关闭。
                            //	"wARFCN3Duration":XXX	工作频点3扫描时长	
                            //	"wARFCN3Period":XXX	    工作频点3扫描间隔

                            //	"wARFCN4":XXX	        工作频点4	
                            //	"bARFCN4Mode":XXX	    工作频点4模式。	0表示扫描，1表示常开,2表示关闭。
                            //	"wARFCN4Duration":XXX	工作频点4扫描时长
                            //	"wARFCN4Period":XXX	    工作频点4扫描间隔

                            all.gcCarrierMsgFlag = true;                            
                           
                            if (imms.Body.n_dic[i].dic.ContainsKey("wARFCN1"))
                            {
                                all.gcCarrierMsg.wARFCN1 = imms.Body.n_dic[i].dic["wARFCN1"].ToString();
                            }

                            if (imms.Body.n_dic[i].dic.ContainsKey("bARFCN1Mode"))
                            {
                                all.gcCarrierMsg.bARFCN1Mode = imms.Body.n_dic[i].dic["bARFCN1Mode"].ToString();
                            }

                            if (imms.Body.n_dic[i].dic.ContainsKey("wARFCN1Duration"))
                            {
                                all.gcCarrierMsg.wARFCN1Duration = imms.Body.n_dic[i].dic["wARFCN1Duration"].ToString();
                            }

                            if (imms.Body.n_dic[i].dic.ContainsKey("wARFCN1Period"))
                            {
                                all.gcCarrierMsg.wARFCN1Period = imms.Body.n_dic[i].dic["wARFCN1Period"].ToString();
                            }



                            if (imms.Body.n_dic[i].dic.ContainsKey("wARFCN2"))
                            {
                                all.gcCarrierMsg.wARFCN2 = imms.Body.n_dic[i].dic["wARFCN2"].ToString();
                            }

                            if (imms.Body.n_dic[i].dic.ContainsKey("bARFCN2Mode"))
                            {
                                all.gcCarrierMsg.bARFCN2Mode = imms.Body.n_dic[i].dic["bARFCN2Mode"].ToString();
                            }

                            if (imms.Body.n_dic[i].dic.ContainsKey("wARFCN2Duration"))
                            {
                                all.gcCarrierMsg.wARFCN2Duration = imms.Body.n_dic[i].dic["wARFCN2Duration"].ToString();
                            }

                            if (imms.Body.n_dic[i].dic.ContainsKey("wARFCN2Period"))
                            {
                                all.gcCarrierMsg.wARFCN2Period = imms.Body.n_dic[i].dic["wARFCN2Period"].ToString();
                            }



                            if (imms.Body.n_dic[i].dic.ContainsKey("wARFCN3"))
                            {
                                all.gcCarrierMsg.wARFCN3 = imms.Body.n_dic[i].dic["wARFCN3"].ToString();
                            }

                            if (imms.Body.n_dic[i].dic.ContainsKey("bARFCN3Mode"))
                            {
                                all.gcCarrierMsg.bARFCN3Mode = imms.Body.n_dic[i].dic["bARFCN3Mode"].ToString();
                            }

                            if (imms.Body.n_dic[i].dic.ContainsKey("wARFCN3Duration"))
                            {
                                all.gcCarrierMsg.wARFCN3Duration = imms.Body.n_dic[i].dic["wARFCN3Duration"].ToString();
                            }

                            if (imms.Body.n_dic[i].dic.ContainsKey("wARFCN3Period"))
                            {
                                all.gcCarrierMsg.wARFCN3Period = imms.Body.n_dic[i].dic["wARFCN3Period"].ToString();
                            }


                            if (imms.Body.n_dic[i].dic.ContainsKey("wARFCN4"))
                            {
                                all.gcCarrierMsg.wARFCN4 = imms.Body.n_dic[i].dic["wARFCN4"].ToString();
                            }

                            if (imms.Body.n_dic[i].dic.ContainsKey("bARFCN4Mode"))
                            {
                                all.gcCarrierMsg.bARFCN4Mode = imms.Body.n_dic[i].dic["bARFCN4Mode"].ToString();
                            }

                            if (imms.Body.n_dic[i].dic.ContainsKey("wARFCN4Duration"))
                            {
                                all.gcCarrierMsg.wARFCN4Duration = imms.Body.n_dic[i].dic["wARFCN4Duration"].ToString();
                            }

                            if (imms.Body.n_dic[i].dic.ContainsKey("wARFCN4Period"))
                            {
                                all.gcCarrierMsg.wARFCN4Period = imms.Body.n_dic[i].dic["wARFCN4Period"].ToString();
                            }


                            break;

                            #endregion
                        }
                    case "CONFIG_IMSI_MSG_V3_ID":
                        {
                            #region CONFIG_IMSI_MSG_V3_ID

                            //	"bIMSINum":n		    本条消息中的IMSI(n<=50)                           
                            //	"bActionType":XXX		动作类型。1 = Delete All IMSI；2 = Delete Special IMSI；3 = Add IMSI；4 = Query IMSI
                            //	"bIMSI_#n#":XXX	        IMSI数组。0~9	配置/删除/查询的IMSI
                            //	"bUeActionFlag_#n#":XXX 目标IMSI对应的动作。1 = Reject；5 = Hold ON	

                            int bIMSINum = -1;
                            string field = "";
                            string bActionType = "";                            

                            if (imms.Body.n_dic[i].dic.ContainsKey("bIMSINum"))
                            {
                                if (imms.Body.n_dic[i].dic["bIMSINum"].ToString() != "")
                                {
                                    bIMSINum = int.Parse(imms.Body.n_dic[i].dic["bIMSINum"].ToString());
                                    if (bIMSINum <= 0 || bIMSINum > 50)
                                    {
                                        errInfo = string.Format("bIMSINum = {0},越界.", bIMSINum);
                                        return -1;
                                    }
                                }
                            }
                            else
                            {
                                errInfo = string.Format("不包含bIMSINum字段.");
                                return -1;
                            }
                            
                            if (imms.Body.n_dic[i].dic.ContainsKey("bActionType"))
                            {
                                if (imms.Body.n_dic[i].dic["bActionType"].ToString() != "")
                                {
                                    bActionType = imms.Body.n_dic[i].dic["bActionType"].ToString();
                                    if (bActionType != "1" &&
                                        bActionType != "2" &&
                                        bActionType != "3" &&
                                        bActionType != "4")
                                    {
                                        errInfo = string.Format("bActionType = {0},非法.", bActionType);
                                        return -1;
                                    }
                                }
                            }
                            else
                            {
                                errInfo = string.Format("不包含bActionType字段.");
                                return -1;
                            }

                            all.gcImsiActionFlag = true;
                            all.listGcImsiAction = new List<strGcImsiAction>();

                            for (int j = 1; j <= bIMSINum; j++)
                            {
                                strGcImsiAction str = new strGcImsiAction();

                                str.res1 = "";
                                str.res2 = "";
                                str.res3 = "";
                                str.bindingDevId = "-1";
                               
                                field = string.Format("bIMSI_#n#", j);
                                if (imms.Body.n_dic[i].dic.ContainsKey(field))
                                {
                                    str.bIMSI= imms.Body.n_dic[i].dic.ContainsKey(field).ToString();
                                }

                                field = string.Format("bUeActionFlag_#n#", j);
                                if (imms.Body.n_dic[i].dic.ContainsKey(field))
                                {
                                    str.bUeActionFlag = imms.Body.n_dic[i].dic.ContainsKey(field).ToString();
                                }

                                all.listGcImsiAction.Add(str);
                            }


                            #endregion

                            break;
                        }
                    default:
                        {
                            errInfo = string.Format("get_gc_info包含非法的ndic name:{0}.",name);
                            add_log_info(LogInfoType.EROR, errInfo, "Main", LogCategory.I);
                            Logger.Trace(LogInfoType.EROR, errInfo, "Main", LogCategory.R);
                            return -1;                           
                        }
                }
            }

            return 0;
        }

        /// <summary>
        /// 获取GSM上报相关的参数
        /// </summary>
        /// <param name="imms"></param>
        /// <param name="all"></param>
        /// <returns></returns>
        private int get_gsm_report_info(InterModuleMsgStruct imms, ref str_Gsm_All_Para all,ref string gsmReportFullString, ref string errInfo)
        {
            errInfo = "";
            gsmReportFullString = "";

            if (imms.Body.n_dic.Count <= 0)
            {
                errInfo = string.Format("n_dic的个数为0");
                return -1;
            }

            if (!imms.Body.dic.ContainsKey("sys"))
            {
                errInfo = string.Format("dic不包含sys");
                return -1;
            }
            else
            {
                all.sys = int.Parse(imms.Body.dic["sys"].ToString());
                gsmReportFullString += all.sys.ToString();

                if (all.sys != 0 && all.sys != 1)
                {
                    errInfo = string.Format("sys只能为0或1");
                    return -1;
                }
            }

            if (imms.Body.dic.ContainsKey("hardware_id"))
            {
                all.hardware_id = int.Parse(imms.Body.dic["hardware_id"].ToString());
            }
            else
            {
                errInfo = string.Format("dic不包含hardware_id");
                return -1;
            }


            for (int i = 0; i < imms.Body.n_dic.Count; i++)
            {
                string name = imms.Body.n_dic[i].name;
                switch (name)
                {
                    case "RECV_SYS_PARA":
                        {
                            #region RECV_SYS_PARA

                            //					"paraMcc":移动国家码
                            //					"paraMnc":移动网号
                            //					"paraBsic":基站识别码
                            //					"paraLac":位置区号
                            //					"paraCellId":小区ID
                            //					"paraC2":C2偏移量
                            //					"paraPeri":周期性位置更新周期
                            //					"paraAccPwr":接入功率
                            //					"paraMsPwr":手机发射功率
                            //					"paraRejCau":位置更新拒绝原因

                            all.gsmSysParaFlag = true;
                            if (imms.Body.n_dic[i].dic.ContainsKey("paraMcc"))
                            {
                                if (imms.Body.n_dic[i].dic["paraMcc"].ToString() != "")
                                {
                                    all.gsmSysPara.paraMcc = imms.Body.n_dic[i].dic["paraMcc"].ToString();
                                    gsmReportFullString += string.Format("[{0}]", all.gsmSysPara.paraMcc);
                                }
                            }
                            else
                            {
                                errInfo = string.Format("n_dic不包含:{0}", "paraMcc");
                                return -1;
                            }

                            if (imms.Body.n_dic[i].dic.ContainsKey("paraMnc"))
                            {
                                if (imms.Body.n_dic[i].dic["paraMnc"].ToString() != "")
                                {
                                    all.gsmSysPara.paraMnc = imms.Body.n_dic[i].dic["paraMnc"].ToString();
                                    gsmReportFullString += string.Format("[{0}]", all.gsmSysPara.paraMnc);
                                }
                            }
                            else
                            {
                                errInfo = string.Format("n_dic不包含:{0}", "paraMnc");
                                return -1;
                            }

                            if (imms.Body.n_dic[i].dic.ContainsKey("paraBsic"))
                            {
                                if (imms.Body.n_dic[i].dic["paraBsic"].ToString() != "")
                                {
                                    all.gsmSysPara.paraBsic = imms.Body.n_dic[i].dic["paraBsic"].ToString();
                                    gsmReportFullString += string.Format("[{0}]", all.gsmSysPara.paraBsic);
                                }
                            }
                            else
                            {
                                errInfo = string.Format("n_dic不包含:{0}", "paraBsic");
                                return -1;
                            }

                            if (imms.Body.n_dic[i].dic.ContainsKey("paraLac"))
                            {
                                if (imms.Body.n_dic[i].dic["paraLac"].ToString() != "")
                                {
                                    all.gsmSysPara.paraLac = imms.Body.n_dic[i].dic["paraLac"].ToString();
                                    gsmReportFullString += string.Format("[{0}]", all.gsmSysPara.paraLac);
                                }
                            }
                            else
                            {
                                errInfo = string.Format("n_dic不包含:{0}", "paraLac");
                                return -1;
                            }

                            if (imms.Body.n_dic[i].dic.ContainsKey("paraCellId"))
                            {
                                if (imms.Body.n_dic[i].dic["paraCellId"].ToString() != "")
                                {
                                    all.gsmSysPara.paraCellId = imms.Body.n_dic[i].dic["paraCellId"].ToString();
                                    gsmReportFullString += string.Format("[{0}]", all.gsmSysPara.paraCellId);
                                }
                            }
                            else
                            {
                                errInfo = string.Format("n_dic不包含:{0}", "paraCellId");
                                return -1;
                            }

                            if (imms.Body.n_dic[i].dic.ContainsKey("paraC2"))
                            {
                                if (imms.Body.n_dic[i].dic["paraC2"].ToString() != "")
                                {
                                    all.gsmSysPara.paraC2 = imms.Body.n_dic[i].dic["paraC2"].ToString();
                                    gsmReportFullString += string.Format("[{0}]", all.gsmSysPara.paraC2);
                                }
                            }
                            else
                            {
                                errInfo = string.Format("n_dic不包含:{0}", "paraC2");
                                return -1;
                            }


                            if (imms.Body.n_dic[i].dic.ContainsKey("paraPeri"))
                            {
                                if (imms.Body.n_dic[i].dic["paraPeri"].ToString() != "")
                                {
                                    all.gsmSysPara.paraPeri = imms.Body.n_dic[i].dic["paraPeri"].ToString();
                                    gsmReportFullString += string.Format("[{0}]", all.gsmSysPara.paraPeri);
                                }
                            }
                            else
                            {
                                errInfo = string.Format("n_dic不包含:{0}", "paraPeri");
                                return -1;
                            }

                            if (imms.Body.n_dic[i].dic.ContainsKey("paraAccPwr"))
                            {
                                if (imms.Body.n_dic[i].dic["paraAccPwr"].ToString() != "")
                                {
                                    all.gsmSysPara.paraAccPwr = imms.Body.n_dic[i].dic["paraAccPwr"].ToString();
                                    gsmReportFullString += string.Format("[{0}]", all.gsmSysPara.paraAccPwr);
                                }
                            }
                            else
                            {
                                errInfo = string.Format("n_dic不包含:{0}", "paraAccPwr");
                                return -1;
                            }

                            if (imms.Body.n_dic[i].dic.ContainsKey("paraMsPwr"))
                            {
                                if (imms.Body.n_dic[i].dic["paraMsPwr"].ToString() != "")
                                {
                                    all.gsmSysPara.paraMsPwr = imms.Body.n_dic[i].dic["paraMsPwr"].ToString();
                                    gsmReportFullString += string.Format("[{0}]", all.gsmSysPara.paraMsPwr);
                                }
                            }
                            else
                            {
                                errInfo = string.Format("n_dic不包含:{0}", "paraMsPwr");
                                return -1;
                            }

                            if (imms.Body.n_dic[i].dic.ContainsKey("paraRejCau"))
                            {
                                if (imms.Body.n_dic[i].dic["paraRejCau"].ToString() != "")
                                {
                                    all.gsmSysPara.paraRejCau = imms.Body.n_dic[i].dic["paraRejCau"].ToString();
                                    gsmReportFullString += string.Format("[{0}]", all.gsmSysPara.paraRejCau);
                                }
                            }
                            else
                            {
                                errInfo = string.Format("n_dic不包含:{0}", "paraRejCau");
                                return -1;
                            }

                            break;

                            #endregion
                        }
                    case "RECV_SYS_OPTION":
                        {
                            #region RECV_SYS_OPTION

                            //                  "opLuSms":登录时发送短信
                            //					"opLuImei":登录时获取IMEI
                            //					"opCallEn":允许用户主叫
                            //					"opDebug":调试模式，上报信令
                            //					"opLuType":登录类型
                            //					"opSmsType":短信类型

                            all.gsmSysOptionFlag = true;
                            if (imms.Body.n_dic[i].dic.ContainsKey("opLuSms"))
                            {
                                if (imms.Body.n_dic[i].dic["opLuSms"].ToString() != "")
                                {
                                    all.gsmSysOption.opLuSms = imms.Body.n_dic[i].dic["opLuSms"].ToString();
                                    gsmReportFullString += string.Format("[{0}]", all.gsmSysOption.opLuSms);
                                }
                            }
                            else
                            {
                                errInfo = string.Format("n_dic不包含:{0}", "opLuSms");
                                return -1;
                            }

                            if (imms.Body.n_dic[i].dic.ContainsKey("opLuImei"))
                            {
                                if (imms.Body.n_dic[i].dic["opLuImei"].ToString() != "")
                                {
                                    all.gsmSysOption.opLuImei = imms.Body.n_dic[i].dic["opLuImei"].ToString();
                                    gsmReportFullString += string.Format("[{0}]", all.gsmSysOption.opLuImei);
                                }
                            }
                            else
                            {
                                errInfo = string.Format("n_dic不包含:{0}", "opLuImei");
                                return -1;
                            }

                            if (imms.Body.n_dic[i].dic.ContainsKey("opCallEn"))
                            {
                                if (imms.Body.n_dic[i].dic["opCallEn"].ToString() != "")
                                {
                                    all.gsmSysOption.opCallEn = imms.Body.n_dic[i].dic["opCallEn"].ToString();
                                    gsmReportFullString += string.Format("[{0}]", all.gsmSysOption.opCallEn);
                                }
                            }
                            else
                            {
                                errInfo = string.Format("n_dic不包含:{0}", "opCallEn");
                                return -1;
                            }

                            if (imms.Body.n_dic[i].dic.ContainsKey("opDebug"))
                            {
                                if (imms.Body.n_dic[i].dic["opDebug"].ToString() != "")
                                {
                                    all.gsmSysOption.opDebug = imms.Body.n_dic[i].dic["opDebug"].ToString();
                                    gsmReportFullString += string.Format("[{0}]", all.gsmSysOption.opDebug);
                                }
                            }
                            else
                            {
                                errInfo = string.Format("n_dic不包含:{0}", "opDebug");
                                return -1;
                            }

                            if (imms.Body.n_dic[i].dic.ContainsKey("opLuType"))
                            {
                                if (imms.Body.n_dic[i].dic["opLuType"].ToString() != "")
                                {
                                    all.gsmSysOption.opLuType = imms.Body.n_dic[i].dic["opLuType"].ToString();
                                    gsmReportFullString += string.Format("[{0}]", all.gsmSysOption.opLuType);
                                }
                            }
                            else
                            {
                                errInfo = string.Format("n_dic不包含:{0}", "opLuType");
                                return -1;
                            }

                            if (imms.Body.n_dic[i].dic.ContainsKey("opSmsType"))
                            {
                                if (imms.Body.n_dic[i].dic["opSmsType"].ToString() != "")
                                {
                                    all.gsmSysOption.opSmsType = imms.Body.n_dic[i].dic["opSmsType"].ToString();
                                    gsmReportFullString += string.Format("[{0}]", all.gsmSysOption.opSmsType);
                                }
                            }
                            else
                            {
                                errInfo = string.Format("n_dic不包含:{0}", "opSmsType");
                                return -1;
                            }

                            break;

                            #endregion
                        }
                    case "RECV_RF_PARA":
                        {
                            #region RECV_RF_PARA

                            //					"rfEnable":射频使能
                            //					"rfFreq":信道号
                            //					"rfPwr":发射功率衰减值

                            all.gsmRfParaFlag = true;
                            if (imms.Body.n_dic[i].dic.ContainsKey("rfEnable"))
                            {
                                if (imms.Body.n_dic[i].dic["rfEnable"].ToString() != "")
                                {
                                    all.gsmRfPara.rfEnable = imms.Body.n_dic[i].dic["rfEnable"].ToString();
                                    gsmReportFullString += string.Format("[{0}]", all.gsmRfPara.rfEnable);
                                }
                            }
                            else
                            {
                                errInfo = string.Format("n_dic不包含:{0}", "rfEnable");
                                return -1;
                            }

                            if (imms.Body.n_dic[i].dic.ContainsKey("rfFreq"))
                            {
                                if (imms.Body.n_dic[i].dic["rfFreq"].ToString() != "")
                                {
                                    all.gsmRfPara.rfFreq = imms.Body.n_dic[i].dic["rfFreq"].ToString();
                                    gsmReportFullString += string.Format("[{0}]", all.gsmRfPara.rfFreq);
                                }
                            }
                            else
                            {
                                errInfo = string.Format("n_dic不包含:{0}", "rfFreq");
                                return -1;
                            }

                            if (imms.Body.n_dic[i].dic.ContainsKey("rfPwr"))
                            {
                                if (imms.Body.n_dic[i].dic["rfPwr"].ToString() != "")
                                {
                                    all.gsmRfPara.rfPwr = imms.Body.n_dic[i].dic["rfPwr"].ToString();
                                    gsmReportFullString += string.Format("[{0}]", all.gsmRfPara.rfPwr);
                                }
                            }
                            else
                            {
                                errInfo = string.Format("n_dic不包含:{0}", "rfPwr");
                                return -1;
                            }

                            break;

                            #endregion
                        }
                    case "RECV_SMS_OPTION":
                        {
                            // 短信部分无需处理

                            #region RECV_SMS_OPTION

                            //          "gSmsRpoa":短消息中心号码
                            //          "gSmsTpoa":短消息原叫号码
                            //          "gSmsScts":短消息发送时间 （时间格式为年/月/日/时/分/秒各两位，不足两位前补0。如2014年4月22日15点46分47秒的消息内容为“140422154647”）
                            //          "gSmsData":短消息内容 （编码格式为Unicode编码）
                            //          "autoSendtiny":是否自动发送
                            //          "autoFilterSMStiny":是否自动过滤短信
                            //          "delayTime":发送延时时间
                            //          "smsCodingtiny":短信的编码格式

                            all.gsmMsgOptionFlag = true;
                            if (imms.Body.n_dic[i].dic.ContainsKey("gSmsRpoa"))
                            {
                                if (imms.Body.n_dic[i].dic["gSmsRpoa"].ToString() != "")
                                {
                                    all.gsmMsgOption.smsRPOA = imms.Body.n_dic[i].dic["gSmsRpoa"].ToString();
                                }
                            }

                            if (imms.Body.n_dic[i].dic.ContainsKey("gSmsTpoa"))
                            {
                                if (imms.Body.n_dic[i].dic["gSmsTpoa"].ToString() != "")
                                {
                                    all.gsmMsgOption.smsTPOA = imms.Body.n_dic[i].dic["gSmsTpoa"].ToString();
                                }
                            }

                            if (imms.Body.n_dic[i].dic.ContainsKey("gSmsScts"))
                            {
                                if (imms.Body.n_dic[i].dic["gSmsScts"].ToString() != "")
                                {
                                    all.gsmMsgOption.smsSCTS = imms.Body.n_dic[i].dic["gSmsScts"].ToString();
                                }
                            }

                            if (imms.Body.n_dic[i].dic.ContainsKey("gSmsData"))
                            {
                                if (imms.Body.n_dic[i].dic["gSmsData"].ToString() != "")
                                {
                                    all.gsmMsgOption.smsDATA = imms.Body.n_dic[i].dic["gSmsData"].ToString();
                                }
                            }

                            if (imms.Body.n_dic[i].dic.ContainsKey("autoSendtiny"))
                            {
                                if (imms.Body.n_dic[i].dic["autoSendtiny"].ToString() != "")
                                {
                                    all.gsmMsgOption.autoSend = imms.Body.n_dic[i].dic["autoSendtiny"].ToString();
                                }
                            }
                            else
                            {
                                all.gsmMsgOption.autoSend = "0";
                            }

                            if (imms.Body.n_dic[i].dic.ContainsKey("autoFilterSMStiny"))
                            {
                                if (imms.Body.n_dic[i].dic["autoFilterSMStiny"].ToString() != "")
                                {
                                    all.gsmMsgOption.autoFilterSMS = imms.Body.n_dic[i].dic["autoFilterSMStiny"].ToString();
                                }
                            }
                            else
                            {
                                all.gsmMsgOption.autoFilterSMS = "0";
                            }

                            if (imms.Body.n_dic[i].dic.ContainsKey("delayTime"))
                            {
                                if (imms.Body.n_dic[i].dic["delayTime"].ToString() != "")
                                {
                                    all.gsmMsgOption.delayTime = imms.Body.n_dic[i].dic["delayTime"].ToString();
                                }
                            }
                            else
                            {
                                all.gsmMsgOption.delayTime = "0";
                            }

                            if (imms.Body.n_dic[i].dic.ContainsKey("smsCodingtiny"))
                            {
                                if (imms.Body.n_dic[i].dic["smsCodingtiny"].ToString() != "")
                                {
                                    all.gsmMsgOption.smsCoding = imms.Body.n_dic[i].dic["smsCodingtiny"].ToString();
                                }
                            }

                            break;

                            #endregion
                        }
                    case "RECV_REG_MODE":
                        {
                            #region RECV_REG_MODE

                            //          "regMode":模式0时由设备自行根据系统选项决定是否允许终端入网，是否对终端发送短信；
                            //                    模式1时设备将终端标识发送给上位机，由上位机告知设备下一步的动作

                            all.gsmSysOptionFlag = true;
                            if (imms.Body.n_dic[i].dic.ContainsKey("regMode"))
                            {
                                if (imms.Body.n_dic[i].dic["regMode"].ToString() != "")
                                {
                                    all.gsmSysOption.opRegModel = imms.Body.n_dic[i].dic["regMode"].ToString();
                                    gsmReportFullString += string.Format("[{0}]", all.gsmSysOption.opRegModel);
                                }
                            }
                            else
                            {
                                errInfo = string.Format("n_dic不包含:{0}", "regMode");
                                return -1;
                            }

                            break;

                            #endregion
                        }
                    default:
                        {
                            add_log_info(LogInfoType.EROR, "get_gsm_info包含非法的ndic name\n", "Main", LogCategory.I);
                            Logger.Trace(LogInfoType.EROR, "get_gsm_info包含非法的ndic name\n", "Main", LogCategory.I);
                            break;
                        }
                }
            }

            return 0;
        }

        private string GetMD5WithString(string sDataIn)
        {
            string str = "";
            byte[] data = Encoding.GetEncoding("utf-8").GetBytes(str);

            MD5 md5 = new MD5CryptoServiceProvider();

            byte[] bytes = md5.ComputeHash(data);

            for (int i = 0; i < bytes.Length; i++)
            {
                str += bytes[i].ToString("x2");
            }

            return str;
        }

        private string GetMD5WithFilePath(string filePath)
        {
            FileStream file = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();

            byte[] hash_byte = md5.ComputeHash(file);
            string str = System.BitConverter.ToString(hash_byte);
            str = str.Replace("-", "");

            return str;
        }

        /// <summary>
        /// 获取MD5校验和
        /// </summary>
        /// <param name="listImsi"></param>
        /// <param name="fromType">
        /// 0 ： listImsi生成字符串,再生成MD5
        /// 1 ： listImsi生成文件,再生成MD5
        /// </param>
        /// <param name="md5"></param>
        /// <returns></returns>
        private int Get_Md5_Sum(List<string> listImsi, int fromType, ref string md5)
        {
            if (listImsi == null)
            {
                return -1;
            }

            if (fromType != 0 && fromType != 1)
            {
                return -1;
            }

            md5 = "";
            if (fromType == 0)
            {
                string tmp = listImsi.ToString();
                md5 = GetMD5WithString(tmp);
            }
            else
            {
                string fileFullPath = Application.StartupPath + @"\\md5.txt";              
                if (File.Exists(fileFullPath))
                {
                    File.Delete(fileFullPath);
                }

                try
                {
                    byte[] data = null;
                    FileStream fs = new FileStream(fileFullPath, FileMode.Create);
             
                    foreach (string str in listImsi)
                    {                                           
                        data = System.Text.Encoding.Default.GetBytes(str + "\n");
                        fs.Write(data, 0, data.Length);
                    }

                    //清空缓冲区、关闭流
                    fs.Flush();
                    fs.Close();
                }
                catch (Exception e)
                {
                    add_log_info(LogInfoType.EROR, e.Message, "Main", LogCategory.I);
                    Logger.Trace(LogInfoType.EROR, e.Message, "Main", LogCategory.I);
                    return -1;
                }

                md5 = GetMD5WithFilePath(fileFullPath);
                md5 = md5.ToLower();
            }

            return 0;
        }

        private int Get_BwList_From_File(string fileFullPath, bwType bwFlag, ref List<strBwList> bwList, ref string errInfo)
        {
            if (string.IsNullOrEmpty(fileFullPath))
            {
                errInfo = string.Format("文件名为空.");
                return -1;
            }

            if (!File.Exists(fileFullPath))
            {
                errInfo = string.Format("文件:{0}不存在.",fileFullPath);
                return -1;
            }

            if (bwFlag != bwType.BWTYPE_BLACK && bwFlag != bwType.BWTYPE_WHITE)
            {
                errInfo = string.Format("黑白标识:{0}有误.", bwFlag);
                return -1;
            }


            bwList = new List<strBwList>();

            string[] lines = System.IO.File.ReadAllLines(@fileFullPath);
            foreach (string line in lines)
            {
                strBwList str = new strBwList();
                if (string.IsNullOrEmpty(line) )
                {
                    continue;
                }

                if (bwFlag == bwType.BWTYPE_BLACK)
                {                             
                    string[] s = line.Split(new char[] { ',' });
                    if (s.Length != 3)
                    {
                        continue;
                    }
                    else
                    {
                        try
                        {
                            Int16.Parse(s[1]);
                            Int16.Parse(s[2]);
                        }
                        catch (Exception ee)
                        {
                            add_log_info(LogInfoType.EROR, "RB Parse Error." + ee.Message, "Main", LogCategory.R);
                            Logger.Trace(LogInfoType.EROR, "RB Parse Error." + ee.Message, "Main", LogCategory.R);
                            continue;
                        }

                        str.bwFlag = bwType.BWTYPE_BLACK;
                        str.imsi = s[0];
                        str.rbStart = s[1];
                        str.rbEnd = s[2];
                    }
                }
                else
                {
                    str.bwFlag = bwType.BWTYPE_WHITE;
                    str.imsi = line.Trim();
                }

                bwList.Add(str);
            }

            errInfo = string.Format("成功");
            return 0;
        }

        /// <summary>
        /// 处理收到从ApController收到的消息
        /// </summary>
        /// <param name="strBody">消息体</param>
        /// <returns></returns>
        private int process_ap_controller_msg(string strBody)
        {
            int rv = 0;
            if (string.IsNullOrEmpty(strBody))
            {
                add_log_info(LogInfoType.EROR, "strBody is Error.\n", "Main", LogCategory.R);
                Logger.Trace(LogInfoType.EROR, "strBody is Error.\n", "Main", LogCategory.R);
                return -1;
            }

            try
            {
                gApLower = JsonConvert.DeserializeObject<InterModuleMsgStruct>(strBody);

                switch (gApLower.Body.type)
                {
                    case Main2ApControllerMsgType.OnOffLine:
                        {
                            #region 获取信息

                            int rtv = 0;

                            string Status = "";
                            string mode = "";

                            if (string.IsNullOrEmpty(gApLower.ApInfo.Fullname))
                            {
                                //用于处理未指派设备的心跳消息
                                process_device_unknown(gApLower);
                                break;
                            }

                            string name = "";
                            string nameFullPath = "";

                            if (gApLower.ApInfo.Fullname.Contains("."))
                            {
                                int i = gApLower.ApInfo.Fullname.LastIndexOf(".");

                                name = gApLower.ApInfo.Fullname.Substring(i + 1);
                                nameFullPath = gApLower.ApInfo.Fullname.Substring(0, i);
                            }
                            else
                            {
                                add_log_info(LogInfoType.EROR, "Fullname is invalid.", "Main", LogCategory.R);
                                Logger.Trace(LogInfoType.EROR, "Fullname is invalid.", "Main", LogCategory.R);

                                //返回出错处理
                                gApLower.Body.type = Main2ApControllerMsgType.OnOffLine_Ack;
                                gApLower.Body.dic = new Dictionary<string, object>();
                                gApLower.Body.dic.Add("ReturnCode", -1);
                                gApLower.Body.dic.Add("ReturnStr", get_debug_info() + "Fullname format error.");

                                Send_Msg_2_ApCtrl_Lower(gApLower);
                                break;
                            }

                            if (name == "" || nameFullPath == "")
                            {
                                //返回出错处理
                                gApLower.Body.type = Main2ApControllerMsgType.OnOffLine_Ack;
                                gApLower.Body.dic = new Dictionary<string, object>();
                                gApLower.Body.dic.Add("ReturnCode", -1);
                                gApLower.Body.dic.Add("ReturnStr", get_debug_info() + "get name or nameFullPath fail.");

                                Send_Msg_2_ApCtrl_Lower(gApLower);
                                break;
                            }

                            if (gApLower.Body.dic.ContainsKey("Status"))
                            {
                                Status = gApLower.Body.dic["Status"].ToString();
                            }

                            if (Status == "")
                            {
                                add_log_info(LogInfoType.EROR, "不存在Key：Status", "Main", LogCategory.R);
                                Logger.Trace(LogInfoType.EROR, "不存在Key：Status", "Main", LogCategory.R);

                                //返回出错处理
                                gApLower.Body.type = Main2ApControllerMsgType.OnOffLine_Ack;
                                gApLower.Body.dic = new Dictionary<string, object>();
                                gApLower.Body.dic.Add("ReturnCode", -1);
                                gApLower.Body.dic.Add("ReturnStr", get_debug_info() + "不存在Key：Status");

                                Send_Msg_2_ApCtrl_Lower(gApLower);
                                break;
                            }

                            if (Status != "OnLine" && Status != "OffLine")
                            {
                                add_log_info(LogInfoType.EROR, "Status不为OnLine或OffLine", "Main", LogCategory.R);
                                Logger.Trace(LogInfoType.EROR, "Status不为OnLine或OffLine", "Main", LogCategory.R);

                                //返回出错处理
                                gApLower.Body.type = Main2ApControllerMsgType.OnOffLine_Ack;
                                gApLower.Body.dic = new Dictionary<string, object>();
                                gApLower.Body.dic.Add("ReturnCode", -1);
                                gApLower.Body.dic.Add("ReturnStr", get_debug_info() + "Status不为OnLine或OffLine");

                                Send_Msg_2_ApCtrl_Lower(gApLower);
                                break;
                            }

                            if (DataController.SimuTest == 0)
                            {
                                // 非模拟器测试
                                if (gApLower.Body.dic.ContainsKey("mode"))
                                {
                                    mode = gApLower.Body.dic["mode"].ToString();
                                    devMode dm = gDbHelper.get_device_mode(mode);

                                    if (dm == devMode.MODE_UNKNOWN)
                                    {
                                        string errInfo = string.Format("mode = {0},非法！",mode);
                                        add_log_info(LogInfoType.EROR, errInfo, "Main", LogCategory.R);
                                        Logger.Trace(LogInfoType.EROR, errInfo, "Main", LogCategory.R);

                                        //返回出错处理
                                        gApLower.Body.type = Main2ApControllerMsgType.OnOffLine_Ack;
                                        gApLower.Body.dic = new Dictionary<string, object>();
                                        gApLower.Body.dic.Add("ReturnCode", -1);
                                        gApLower.Body.dic.Add("ReturnStr", get_debug_info() + "mode的值非法");

                                        Send_Msg_2_ApCtrl_Lower(gApLower);
                                        break;
                                    }
                                }
                                else
                                {
                                    add_log_info(LogInfoType.EROR, "不存在Key：mode", "Main", LogCategory.R);
                                    Logger.Trace(LogInfoType.EROR, "不存在Key：mode", "Main", LogCategory.R);

                                    //返回出错处理
                                    gApLower.Body.type = Main2ApControllerMsgType.OnOffLine_Ack;
                                    gApLower.Body.dic = new Dictionary<string, object>();
                                    gApLower.Body.dic.Add("ReturnCode", -1);
                                    gApLower.Body.dic.Add("ReturnStr", get_debug_info() + "不存在Key：mode");

                                    Send_Msg_2_ApCtrl_Lower(gApLower);
                                    break;
                                }
                            }
                            else
                            {
                                // 模拟器测试
                                Logger.Trace(LogInfoType.EROR, "进入模拟器测试模式", "Main", LogCategory.R);

                                gApLower.Body.type = Main2ApControllerMsgType.OnOffLine_Ack;
                                gApLower.Body.dic = new Dictionary<string, object>();
                                gApLower.Body.dic.Add("ReturnCode", 0);                               
                                gApLower.Body.dic.Add("ReturnStr", gDbHelper.get_rtv_str(0));
                                gApLower.Body.dic.Add("Status", Status);
                                Send_Msg_2_ApCtrl_Lower(gApLower);
                                break;
                            }

                            #endregion

                            #region 更新数据库

                            strDomian domianInfo = new strDomian();

                            //通过名称全路径获取对应记录的信息
                            rtv = gDbHelper.domain_record_get_by_nameFullPath(nameFullPath, ref domianInfo);
                            if (rtv != 0)
                            {
                                //返回出错处理                               
                                gApLower.Body.type = Main2ApControllerMsgType.OnOffLine_Ack;
                                gApLower.Body.dic = new Dictionary<string, object>();
                                gApLower.Body.dic.Add("ReturnCode", rtv);
                                if (rtv != 0)
                                {
                                    gApLower.Body.dic.Add("ReturnStr", get_debug_info() + gDbHelper.get_rtv_str(rtv));
                                }
                                else
                                {
                                    gApLower.Body.dic.Add("ReturnStr", gDbHelper.get_rtv_str(rtv));
                                }

                                Send_Msg_2_ApCtrl_Lower(gApLower);
                                break;
                            }
                            else
                            {
                                //更新数据库中的信息
                                strDevice dev = new strDevice();

                                if (Status == "OnLine")
                                {
                                    dev.online = "1";
                                }
                                else
                                {
                                    dev.online = "0";
                                }

                                dev.sn = gApLower.ApInfo.SN;
                                dev.ipAddr = gApLower.ApInfo.IP;
                                dev.port = gApLower.ApInfo.Port.ToString();
                                dev.innerType = gApLower.ApInfo.Type;

                                // 2018-06-22
                                dev.lastOnline = DateTime.Now.ToString();

                                // 2018-06-28
                                dev.mode = mode;

                                rtv = gDbHelper.device_record_update(domianInfo.id, name, dev);
                                if (rtv != 0)
                                {
                                    //返回出错处理
                                    gApLower.Body.type = Main2ApControllerMsgType.OnOffLine_Ack;
                                    gApLower.Body.dic = new Dictionary<string, object>();
                                    gApLower.Body.dic.Add("ReturnCode", rtv);
                                    gApLower.Body.dic.Add("ReturnStr", get_debug_info() + gDbHelper.get_rtv_str(rtv));

                                    Send_Msg_2_ApCtrl_Lower(gApLower);
                                    break;
                                }
                                else
                                {
                                    gApLower.Body.type = Main2ApControllerMsgType.OnOffLine_Ack;
                                    gApLower.Body.dic = new Dictionary<string, object>();
                                    gApLower.Body.dic.Add("ReturnCode", rtv);
                                    gApLower.Body.dic.Add("ReturnStr", gDbHelper.get_rtv_str(rtv));
                                    gApLower.Body.dic.Add("Status", Status);
                                }

                                Send_Msg_2_ApCtrl_Lower(gApLower);
                                Send_Msg_2_AppCtrl_Upper(gApLower);

                                break;
                            }

                            #endregion
                        }
                    case Main2ApControllerMsgType.ApStatusChange:
                        {
                            #region 获取信息

                            int rtv = 0;
                            string Fullname = "";
                            string wSelfStudy = "0";

                            strDevice devInfo = new strDevice();
                            strApStatus apSts = new strApStatus();

                            if (string.IsNullOrEmpty(gApLower.ApInfo.Fullname))
                            {
                                //返回出错处理
                                string errInfo = string.Format("{0}:Fullname is NULL.", Main2ApControllerMsgType.ApStatusChange);
                                add_log_info(LogInfoType.EROR, errInfo, "Main", LogCategory.I);
                                Logger.Trace(LogInfoType.EROR, errInfo, "Main", LogCategory.I);

                                Fill_IMMS_Info(ref gApLower, Main2ApControllerMsgType.ApStatusChange_Ack, -1, errInfo, true, null, null);
                                Send_Msg_2_ApCtrl_Lower(gApLower);
                                break;                                
                            }
                            else
                            {
                                Fullname = gApLower.ApInfo.Fullname;
                            }

                            if (!gDicDeviceId.ContainsKey(Fullname))
                            {                                
                                string errInfo = get_debug_info() + string.Format("{0}:对应的设备ID在gDicDeviceId中找不到", Fullname);
                                add_log_info(LogInfoType.EROR, errInfo, "Main", LogCategory.I);
                                Logger.Trace(LogInfoType.EROR, errInfo, "Main", LogCategory.I);

                                Fill_IMMS_Info(ref gApLower, Main2ApControllerMsgType.ApStatusChange_Ack, -1, errInfo, true, null, null);
                                Send_Msg_2_ApCtrl_Lower(gApLower);
                                break;
                            }
                            else
                            {
                                devInfo = gDicDeviceId[Fullname];                                
                            }

                            if (gApLower.Body.dic.ContainsKey("SCTP"))
                            {
                                apSts.SCTP = gApLower.Body.dic["SCTP"].ToString();
                            }

                            if (gApLower.Body.dic.ContainsKey("S1"))
                            {
                                apSts.S1 = gApLower.Body.dic["S1"].ToString();
                            }

                            if (gApLower.Body.dic.ContainsKey("GPS"))
                            {
                                apSts.GPS = gApLower.Body.dic["GPS"].ToString();
                            }

                            if (gApLower.Body.dic.ContainsKey("CELL"))
                            {
                                apSts.CELL = gApLower.Body.dic["CELL"].ToString();
                            }

                            if (gApLower.Body.dic.ContainsKey("SYNC"))
                            {
                                apSts.SYNC = gApLower.Body.dic["SYNC"].ToString();
                            }

                            if (gApLower.Body.dic.ContainsKey("LICENSE"))
                            {
                                apSts.LICENSE = gApLower.Body.dic["LICENSE"].ToString();
                            }

                            if (gApLower.Body.dic.ContainsKey("RADIO"))
                            {
                                apSts.RADIO = gApLower.Body.dic["RADIO"].ToString();
                            }

                            if (gApLower.Body.dic.ContainsKey("timestamp"))
                            {
                                try
                                {
                                    DateTime.Parse(gApLower.Body.dic["timestamp"].ToString());
                                    apSts.time = gApLower.Body.dic["timestamp"].ToString();
                                }
                                catch (Exception ee)
                                {
                                    Logger.Trace(LogInfoType.EROR, ee.Message, "Main", LogCategory.R);
                                    apSts.time = DateTime.Now.ToString();
                                }
                            }

                            #endregion

                            #region 白名单自学习处理

                            if (gApLower.Body.dic.ContainsKey("wSelfStudy"))
                            {
                                wSelfStudy = gApLower.Body.dic["wSelfStudy"].ToString().Trim();
                                if (!wSelfStudy.Equals(devInfo.wSelfStudy))
                                {
                                    //【状态不一致才需要处理】

                                    if (wSelfStudy.Equals("1"))
                                    {
                                        #region wSelfStudy : 0 -> 1

                                        //(1) 记录状态变化
                                        string errInfo = string.Format("白名单自学习:wSelfStudy : 0 -> 1");
                                        add_log_info(LogInfoType.INFO, errInfo, "Main", LogCategory.I);
                                        Logger.Trace(LogInfoType.INFO, errInfo, "Main", LogCategory.I);

                                        //(2) 删除该AP的所有白名单
                                        if (devInfo.clearWhiteList.Equals("1"))
                                        {
                                            rtv = gDbHelper.bwlist_record_bwflag_delete(bwType.BWTYPE_WHITE, devInfo.id);
                                            if ((int)RC.SUCCESS != rtv)
                                            {
                                                errInfo = string.Format("白名单自学习:bwlist_record_bwflag_delete出错,{0}", gDbHelper.get_rtv_str(rtv));
                                                add_log_info(LogInfoType.EROR, errInfo, "Main", LogCategory.I);
                                                Logger.Trace(LogInfoType.EROR, errInfo, "Main", LogCategory.I);

                                                Fill_IMMS_Info(ref gApLower, Main2ApControllerMsgType.ApStatusChange_Ack, -1, errInfo, true, null, null);
                                                Send_Msg_2_ApCtrl_Lower(gApLower);
                                                break;
                                            }
                                            else
                                            {
                                                errInfo = string.Format("白名单自学习:删除设备{0}的白名单成功.", Fullname);
                                                add_log_info(LogInfoType.INFO, errInfo, "Main", LogCategory.I);
                                                Logger.Trace(LogInfoType.INFO, errInfo, "Main", LogCategory.I);
                                            }
                                        }
                                        else
                                        {
                                            errInfo = string.Format("白名单自学习:不删除库中的白名单{0}.", devInfo.clearWhiteList);
                                            add_log_info(LogInfoType.INFO, errInfo, "Main", LogCategory.I);
                                            Logger.Trace(LogInfoType.INFO, errInfo, "Main", LogCategory.I);
                                        }

                                        //(3) 保存wSelfStudy的状态
                                        devInfo.wSelfStudy = wSelfStudy;
                                        gDicDeviceId[Fullname] = devInfo;

                                        #endregion
                                    }
                                    else
                                    {
                                        #region wSelfStudy : 1 -> 0

                                        //(1) 记录状态变化
                                        string errInfo = string.Format("白名单自学习:wSelfStudy : 1 -> 0");
                                        add_log_info(LogInfoType.INFO, errInfo, "Main", LogCategory.I);
                                        Logger.Trace(LogInfoType.INFO, errInfo, "Main", LogCategory.I);

                                        //(2) 保存wSelfStudy的状态
                                        devInfo.wSelfStudy = wSelfStudy;
                                        gDicDeviceId[Fullname] = devInfo;

                                        #endregion
                                    }
                                }
                            }

                            #endregion

                            #region 更新数据库

                            // 先转发给AppCtrl
                            gApLower.Body.type = Main2ApControllerMsgType.ApStatusChange_Ack;
                            Send_Msg_2_AppCtrl_Upper(gApLower);

                            rtv = gDbHelper.ap_status_record_update(devInfo.id, apSts);

                            string info = string.Format("ap_status_record_update:{0}", gDbHelper.get_rtv_str(rtv));
                            Fill_IMMS_Info(ref gApLower, Main2ApControllerMsgType.ApStatusChange_Ack, rtv, info, true, null, null);                           
                            Send_Msg_2_ApCtrl_Lower(gApLower);                            

                            break;

                            #endregion
                        }
                    case ApMsgType.scanner:
                        {
                            #region LTE捕号记录处理

                            lte_capture_info_process(gApLower);

                            //if (gApLower.ApInfo.Type.ToUpper().Contains("LTE"))
                            //{
                            //    //lte_capture_info_process(gApLower);
                            //}
                            //else if (gApLower.ApInfo.Type.ToUpper().Contains("GSW"))
                            //{
                            //    //gsm_capture_info_process(gApLower);
                            //}
                            //else if (gApLower.ApInfo.Type.ToUpper().Contains("WCDMA"))
                            //{
                            //    //wcdma_capture_info_process(gApLower);
                            //}
                            //else
                            //{
                            //    add_log_info(LogInfoType.EROR, "ApMsgType.scanner,不支持的类型.", "Main", LogCategory.R);
                            //    Logger.Trace(LogInfoType.EROR, "ApMsgType.scanner,不支持的类型.", "Main", LogCategory.R);
                            //    break;
                            //}

                            #endregion

                            #region 发送给AppController

                            //在此将数据送给AppCtrl_Upper
                            Send_Msg_2_AppCtrl_Upper(gApLower);

                            break;

                            #endregion
                        }
                    case AppMsgType.gsm_msg_recv:
                        {
                            #region IMSI处理

                            devMode dm = gDbHelper.get_device_mode(gApLower.ApInfo.Type);
                            if (dm == devMode.MODE_UNKNOWN)
                            {
                                string errInfo = string.Format("ApLower.ApInfo.Type = {0},错误的类型.", gApLower.ApInfo.Type);
                                add_log_info(LogInfoType.EROR, errInfo, "Main", LogCategory.I);
                                Logger.Trace(LogInfoType.EROR, errInfo, "Main", LogCategory.I);
                            }

                            switch (dm)
                            {
                                case devMode.MODE_GSM:
                                    {
                                        #region GSM

                                        if ((gApLower.Body.n_dic != null) && (gApLower.Body.n_dic.Count > 0))
                                        {
                                            for (int i = 0; i < gApLower.Body.n_dic.Count; i++)
                                            {
                                                if (gApLower.Body.n_dic[i].name == "SEND_UE_INFO")
                                                {
                                                    gsm_capture_info_process(gApLower,i);
                                                    break;
                                                }
                                            }
                                        }

                                        break;

                                        #endregion
                                    }
                                case devMode.MODE_GSM_V2:
                                case devMode.MODE_CDMA:
                                    {
                                        #region GSM-V2/CDMA                                       

                                        if ((gApLower.Body.n_dic != null) && (gApLower.Body.n_dic.Count > 0))
                                        {
                                            for (int i = 0; i < gApLower.Body.n_dic.Count; i++)
                                            {
                                                if (gApLower.Body.n_dic[i].name == "UE_STATUS_REPORT_MSG")
                                                {
                                                    gc_capture_info_process(gApLower, i);
                                                    break;
                                                }
                                            }
                                        }

                                        break;

                                        #endregion
                                    }
                                case devMode.MODE_TD_SCDMA:
                                    {
                                        break;
                                    }
                                case devMode.MODE_WCDMA:
                                case devMode.MODE_LTE_FDD:
                                case devMode.MODE_LTE_TDD:
                                    {                                                                               
                                        break;                   
                                    }
                                case devMode.MODE_UNKNOWN:
                                    {
                                        break;
                                    }
                            }

                            #endregion

                            #region 转发给AppCtrl_Upper

                            //在此将数据送给AppCtrl_Upper
                            Send_Msg_2_AppCtrl_Upper(gApLower);

                            break;

                            #endregion
                        }
                    case Main2ApControllerMsgType.ReportGenPara:
                        {
                            #region 获取信息

                            int rtv = 0;
                            string Fullname = "";
                            devMode dm;

                            //默认为设置
                            string reportType = "change";

                            string whiteimsi_md5 = "";
                            string blackimsi_md5 = "";

                            string whiteimsi_md5_db = "";
                            string blackimsi_md5_db = "";

                            //共22项
                            string genParaFullString = "";
                            string genParaFullString_db = "";
                         
                            string gsmParaFullString = "";
                            string gsmParaFullString_db = "";

                            bool whiteimsi_md5_match = false;
                            bool blackimsi_md5_match = false;

                            bool gen_para_match = false;
                            bool gsm_para_match = false;

                            strDevice devInfo = new strDevice();

                            if (string.IsNullOrEmpty(gApLower.ApInfo.Fullname))
                            {
                                //返回出错处理
                                string errInfo = string.Format("{0}:Fullname is NULL.", Main2ApControllerMsgType.ReportGenPara);

                                add_log_info(LogInfoType.EROR, errInfo, "Main", LogCategory.I);
                                Logger.Trace(LogInfoType.EROR, errInfo, "Main", LogCategory.I);

                                Fill_IMMS_Info(ref gApLower, Main2ApControllerMsgType.ReportGenParaAck, -1, errInfo, true, null, null);
                                Send_Msg_2_ApCtrl_Lower(gApLower);
                                break;
                            }
                            else
                            {
                                Fullname = gApLower.ApInfo.Fullname;
                                if (!gDicDeviceId.ContainsKey(Fullname))
                                {
                                    string errInfo = get_debug_info() + string.Format("{0}:对应的设备ID在gDicDeviceId中找不到", Fullname);
                                    add_log_info(LogInfoType.EROR, errInfo, "Main", LogCategory.R);
                                    Logger.Trace(LogInfoType.EROR, errInfo, "Main", LogCategory.R);

                                    Fill_IMMS_Info(ref gApLower, Main2ApControllerMsgType.ReportGenParaAck, -1, errInfo, true, null, null);
                                    Send_Msg_2_ApCtrl_Lower(gApLower);
                                    break;
                                }
                                else
                                {
                                    devInfo = gDicDeviceId[Fullname];
                                    gClsDataAlign.devId = devInfo.id;
                                }
                            }                                                   

                            if (string.IsNullOrEmpty(gApLower.ApInfo.Type))
                            {
                                //返回出错处理
                                string errInfo = string.Format("{0}:ApInfo.Type is NULL.", Main2ApControllerMsgType.ReportGenPara);

                                add_log_info(LogInfoType.EROR, errInfo, "Main", LogCategory.I);
                                Logger.Trace(LogInfoType.EROR, errInfo, "Main", LogCategory.I);

                                Fill_IMMS_Info(ref gApLower, Main2ApControllerMsgType.ReportGenParaAck, -1, errInfo, true, null, null);
                                Send_Msg_2_ApCtrl_Lower(gApLower);
                                break;
                            }
                            else
                            {
                                dm = gDbHelper.get_device_mode(gApLower.ApInfo.Type);
                                if (dm == devMode.MODE_UNKNOWN)
                                {
                                    string errInfo = string.Format("ap.ApInfo.Type = {0},错误的类型.", gApLower.ApInfo.Type);
                                    add_log_info(LogInfoType.EROR, errInfo, "Main", LogCategory.I);
                                    Logger.Trace(LogInfoType.EROR, errInfo, "Main", LogCategory.I);

                                    Fill_IMMS_Info(ref gApLower, Main2ApControllerMsgType.ReportGenParaAck, -1, errInfo, true, null, null);
                                    Send_Msg_2_ApCtrl_Lower(gApLower);
                                    break;
                                }                                                     
                            }

                            if (gApLower.Body.dic.ContainsKey("reportType"))
                            {
                                if (!string.IsNullOrEmpty(gApLower.Body.dic["reportType"].ToString()))
                                {
                                    reportType = gApLower.Body.dic["reportType"].ToString();
                                    if ("change" != reportType && "report" != reportType)
                                    {
                                        //返回出错处理
                                        string errInfo = string.Format("reportType = {0}出错", reportType);
                                        add_log_info(LogInfoType.EROR, errInfo, "Main", LogCategory.I);
                                        Logger.Trace(LogInfoType.EROR, errInfo, "Main", LogCategory.I);

                                        Fill_IMMS_Info(ref gApLower, Main2ApControllerMsgType.ReportGenParaAck, -1, errInfo, true, null, null);
                                        Send_Msg_2_ApCtrl_Lower(gApLower);
                                        break;
                                    }
                                }
                            }
                            //else
                            //{
                            //    //返回出错处理
                            //    string errInfo = string.Format("{0}:没包含reportType字段.", Main2ApControllerMsgType.ReportGenPara);
                            //    add_log_info(LogInfoType.EROR, errInfo, "Main", LogCategory.I);
                            //    Logger.Trace(LogInfoType.EROR, errInfo, "Main", LogCategory.I);

                            //    Fill_IMMS_Info(ref gApLower, Main2ApControllerMsgType.ReportGenParaAck, -1, errInfo, true, null, null);
                            //    Send_Msg_2_ApCtrl_Lower(gApLower);
                            //    break;
                            //}

                       
                            string info = string.Format("DataAlignMode = {0},reportType = {1}", DataController.DataAlignMode,reportType);
                            add_log_info(LogInfoType.INFO, info, "Main", LogCategory.I);
                            Logger.Trace(LogInfoType.INFO, info, "Main", LogCategory.I);

                            if (DataController.DataAlignMode == 2)
                            {
                                reportType = "change";
                            }                     

                            #endregion                  

                            #region 设置或对齐处理

                            if (reportType == "change")
                            {
                                #region 设置处理            

                                switch (dm)
                                {
                                    case devMode.MODE_GSM:
                                        {
                                            #region GSM处理

                                            str_Gsm_All_Para gsmAllPara = new str_Gsm_All_Para();

                                            if (get_gsm_info(gApLower, ref gsmAllPara) != 0)
                                            {
                                                string errInfo = string.Format("获取GSM相关的参数出错.");
                                                add_log_info(LogInfoType.EROR, errInfo, "Main", LogCategory.I);
                                                Logger.Trace(LogInfoType.EROR, errInfo, "Main", LogCategory.I);

                                                Fill_IMMS_Info(ref gApLower, Main2ApControllerMsgType.ReportGenParaAck, -1, errInfo, true, null, null);
                                                Send_Msg_2_ApCtrl_Lower(gApLower);
                                                break;
                                            }

                                            rtv = 0;
                                            int carry = gsmAllPara.sys;

                                            if (gsmAllPara.gsmSysParaFlag == true)
                                            {
                                                rtv += gDbHelper.gsm_sys_para_record_update(carry, devInfo.id, gsmAllPara.gsmSysPara);
                                            }

                                            if (gsmAllPara.gsmSysOptionFlag == true)
                                            {
                                                rtv += gDbHelper.gsm_sys_option_record_update(carry, devInfo.id, gsmAllPara.gsmSysOption);
                                            }

                                            if (gsmAllPara.gsmRfParaFlag == true)
                                            {
                                                rtv += gDbHelper.gsm_rf_para_record_update(carry, devInfo.id, gsmAllPara.gsmRfPara);
                                            }

                                            if (gsmAllPara.gsmMsgOptionFlag == true)
                                            {
                                                rtv += gDbHelper.gsm_msg_option_insert(carry, devInfo.id, gsmAllPara.gsmMsgOption);
                                            }

                                            Fill_IMMS_Info(ref gApLower, Main2ApControllerMsgType.ReportGenParaAck, rtv, gDbHelper.get_rtv_str(rtv), true, null, null);
                                            Send_Msg_2_ApCtrl_Lower(gApLower);
                                            break;

                                            #endregion                                     
                                        }
                                    case devMode.MODE_GSM_V2:
                                    case devMode.MODE_CDMA:
                                        {
                                            #region GSM-V2处理

                                            string gcErrInfo = "";
                                            str_GC_All_Para gcAllPara = new str_GC_All_Para();

                                            if (get_gc_info(gApLower, ref gcAllPara, ref gcErrInfo) != 0)
                                            {
                                                string errInfo = string.Format("获取GSM-V2/CDMA相关的参数出错:{0}.", gcErrInfo);
                                                add_log_info(LogInfoType.EROR, errInfo, "Main", LogCategory.I);
                                                Logger.Trace(LogInfoType.EROR, errInfo, "Main", LogCategory.I);

                                                Fill_IMMS_Info(ref gApLower, Main2ApControllerMsgType.ReportGenParaAck, -1, errInfo, true, null, null);
                                                Send_Msg_2_ApCtrl_Lower(gApLower);
                                                break;
                                            }

                                            rtv = 0;
                                            int carry = gcAllPara.sys;                                            

                                            if (gcAllPara.gcNbCellFlag)
                                            {
                                                rtv += gDbHelper.gc_nb_cell_record_delete(carry, devInfo.id);
                                                rtv += gDbHelper.gc_nb_cell_record_insert_batch(carry, devInfo.id, gcAllPara.listGcNbCell);
                                            }

                                            if (gcAllPara.gcParamConfigFlag)
                                            {
                                                rtv += gDbHelper.gc_param_config_record_update(carry, devInfo.id, gcAllPara.gcParamConfig);
                                            }

                                            if (gcAllPara.gcMiscFlag)
                                            {
                                                rtv += gDbHelper.gc_misc_record_update(carry, devInfo.id, gcAllPara.gcMisc);
                                            }

                                            if (gcAllPara.gcImsiActionFlag)
                                            {
                                                /*
                                                 * 1 = Delete All IMSI；
                                                 * 2 = Delete Special IMSI；
                                                 * 3 = Add IMSI；
                                                 * 4 = Query IMSI
                                                 */
                                                if (gcAllPara.actionType == "1")
                                                {
                                                    rtv += gDbHelper.gc_imsi_action_record_delete(carry, devInfo.id);
                                                }
                                                else if (gcAllPara.actionType == "2")
                                                {
                                                    for (int i = 0; i < gcAllPara.listGcImsiAction.Count; i++)
                                                    {
                                                        rtv += gDbHelper.gc_imsi_action_record_delete(carry, devInfo.id, gcAllPara.listGcImsiAction[i].bIMSI);
                                                    }
                                                }
                                                else if (gcAllPara.actionType == "3")
                                                {
                                                    for (int i = 0; i < gcAllPara.listGcImsiAction.Count; i++)
                                                    {
                                                        rtv += gDbHelper.gc_imsi_action_record_insert(carry, devInfo.id, gcAllPara.listGcImsiAction[i]);
                                                    }
                                                }
                                                else
                                                {
                                                    // Query IMSI
                                                }
                                            }                                            

                                            Fill_IMMS_Info(ref gApLower, Main2ApControllerMsgType.ReportGenParaAck, rtv, gDbHelper.get_rtv_str(rtv), true, null, null);
                                            Send_Msg_2_ApCtrl_Lower(gApLower);
                                            break;

                                            #endregion                                                                           
                                        }                                     
                                    case devMode.MODE_WCDMA:
                                    case devMode.MODE_LTE_TDD:
                                    case devMode.MODE_LTE_FDD:
                                        {
                                            #region LTE处理

                                            strApGenPara apGP = new strApGenPara();
                                            if (gApLower.Body.dic.ContainsKey("mode"))
                                            {
                                                if (!string.IsNullOrEmpty(gApLower.Body.dic["mode"].ToString()))
                                                {
                                                    apGP.mode = gApLower.Body.dic["mode"].ToString();
                                                }
                                            }

                                            if (gApLower.Body.dic.ContainsKey("primaryplmn"))
                                            {
                                                if (!string.IsNullOrEmpty(gApLower.Body.dic["primaryplmn"].ToString()))
                                                {
                                                    apGP.primaryplmn = gApLower.Body.dic["primaryplmn"].ToString();
                                                }
                                            }

                                            if (gApLower.Body.dic.ContainsKey("earfcndl"))
                                            {
                                                if (!string.IsNullOrEmpty(gApLower.Body.dic["earfcndl"].ToString()))
                                                {
                                                    apGP.earfcndl = gApLower.Body.dic["earfcndl"].ToString();
                                                }
                                            }

                                            if (gApLower.Body.dic.ContainsKey("earfcnul"))
                                            {
                                                if (!string.IsNullOrEmpty(gApLower.Body.dic["earfcnul"].ToString()))
                                                {
                                                    apGP.earfcnul = gApLower.Body.dic["earfcnul"].ToString();
                                                }
                                            }


                                            if (gApLower.Body.dic.ContainsKey("cellid"))
                                            {
                                                if (!string.IsNullOrEmpty(gApLower.Body.dic["cellid"].ToString()))
                                                {
                                                    apGP.cellid = gApLower.Body.dic["cellid"].ToString();
                                                }
                                            }

                                            if (gApLower.Body.dic.ContainsKey("pci"))
                                            {
                                                if (!string.IsNullOrEmpty(gApLower.Body.dic["pci"].ToString()))
                                                {
                                                    apGP.pci = gApLower.Body.dic["pci"].ToString();
                                                }
                                            }

                                            if (gApLower.Body.dic.ContainsKey("bandwidth"))
                                            {
                                                if (!string.IsNullOrEmpty(gApLower.Body.dic["bandwidth"].ToString()))
                                                {
                                                    apGP.bandwidth = gApLower.Body.dic["bandwidth"].ToString();
                                                }
                                            }

                                            if (gApLower.Body.dic.ContainsKey("tac"))
                                            {
                                                if (!string.IsNullOrEmpty(gApLower.Body.dic["tac"].ToString()))
                                                {
                                                    apGP.tac = gApLower.Body.dic["tac"].ToString();
                                                }
                                            }

                                            if (gApLower.Body.dic.ContainsKey("txpower"))
                                            {
                                                if (!string.IsNullOrEmpty(gApLower.Body.dic["txpower"].ToString()))
                                                {
                                                    apGP.txpower = gApLower.Body.dic["txpower"].ToString();
                                                }
                                            }

                                            if (gApLower.Body.dic.ContainsKey("periodtac"))
                                            {
                                                if (!string.IsNullOrEmpty(gApLower.Body.dic["periodtac"].ToString()))
                                                {
                                                    apGP.periodtac = gApLower.Body.dic["periodtac"].ToString();
                                                }
                                            }

                                            if (gApLower.Body.dic.ContainsKey("manualfreq"))
                                            {
                                                if (!string.IsNullOrEmpty(gApLower.Body.dic["manualfreq"].ToString()))
                                                {
                                                    apGP.manualfreq = gApLower.Body.dic["manualfreq"].ToString();
                                                }
                                            }

                                            if (gApLower.Body.dic.ContainsKey("bootMode"))
                                            {
                                                if (!string.IsNullOrEmpty(gApLower.Body.dic["bootMode"].ToString()))
                                                {
                                                    apGP.bootMode = gApLower.Body.dic["bootMode"].ToString();
                                                }
                                            }

                                            if (gApLower.Body.dic.ContainsKey("Earfcnlist"))
                                            {
                                                if (!string.IsNullOrEmpty(gApLower.Body.dic["Earfcnlist"].ToString()))
                                                {
                                                    apGP.Earfcnlist = gApLower.Body.dic["Earfcnlist"].ToString();
                                                }
                                            }

                                            if (gApLower.Body.dic.ContainsKey("Bandoffset"))
                                            {
                                                if (!string.IsNullOrEmpty(gApLower.Body.dic["Bandoffset"].ToString()))
                                                {
                                                    apGP.Bandoffset = gApLower.Body.dic["Bandoffset"].ToString();
                                                }
                                            }

                                            if (gApLower.Body.dic.ContainsKey("NTP"))
                                            {
                                                if (!string.IsNullOrEmpty(gApLower.Body.dic["NTP"].ToString()))
                                                {
                                                    apGP.NTP = gApLower.Body.dic["NTP"].ToString();
                                                }
                                            }

                                            if (gApLower.Body.dic.ContainsKey("ntppri"))
                                            {
                                                if (!string.IsNullOrEmpty(gApLower.Body.dic["ntppri"].ToString()))
                                                {
                                                    apGP.ntppri = gApLower.Body.dic["ntppri"].ToString();
                                                }
                                            }

                                            if (gApLower.Body.dic.ContainsKey("source"))
                                            {
                                                if (!string.IsNullOrEmpty(gApLower.Body.dic["source"].ToString()))
                                                {
                                                    apGP.source = gApLower.Body.dic["source"].ToString();
                                                }
                                            }

                                            if (gApLower.Body.dic.ContainsKey("ManualEnable"))
                                            {
                                                if (!string.IsNullOrEmpty(gApLower.Body.dic["ManualEnable"].ToString()))
                                                {
                                                    apGP.ManualEnable = gApLower.Body.dic["ManualEnable"].ToString();
                                                }
                                            }

                                            if (gApLower.Body.dic.ContainsKey("ManualEarfcn"))
                                            {
                                                if (!string.IsNullOrEmpty(gApLower.Body.dic["ManualEarfcn"].ToString()))
                                                {
                                                    apGP.ManualEarfcn = gApLower.Body.dic["ManualEarfcn"].ToString();
                                                }
                                            }

                                            if (gApLower.Body.dic.ContainsKey("ManualPci"))
                                            {
                                                if (!string.IsNullOrEmpty(gApLower.Body.dic["ManualPci"].ToString()))
                                                {
                                                    apGP.ManualPci = gApLower.Body.dic["ManualPci"].ToString();
                                                }
                                            }

                                            if (gApLower.Body.dic.ContainsKey("ManualBw"))
                                            {
                                                if (!string.IsNullOrEmpty(gApLower.Body.dic["ManualBw"].ToString()))
                                                {
                                                    apGP.ManualBw = gApLower.Body.dic["ManualBw"].ToString();
                                                }
                                            }

                                            if (gApLower.Body.dic.ContainsKey("gpsConfig"))
                                            {
                                                if (!string.IsNullOrEmpty(gApLower.Body.dic["gpsConfig"].ToString()))
                                                {
                                                    apGP.gpsConfig = gApLower.Body.dic["gpsConfig"].ToString();
                                                }
                                            }

                                            // 2018-07-23
                                            if (gApLower.Body.dic.ContainsKey("otherplmn"))
                                            {
                                                if (!string.IsNullOrEmpty(gApLower.Body.dic["otherplmn"].ToString()))
                                                {
                                                    apGP.otherplmn = gApLower.Body.dic["otherplmn"].ToString();
                                                }
                                            }

                                            // 2018-07-23
                                            if (gApLower.Body.dic.ContainsKey("periodFreq"))
                                            {
                                                if (!string.IsNullOrEmpty(gApLower.Body.dic["periodFreq"].ToString()))
                                                {
                                                    apGP.periodFreq = gApLower.Body.dic["periodFreq"].ToString();
                                                }
                                            }

                                            // 2018-07-23
                                            if (gApLower.Body.dic.ContainsKey("res1"))
                                            {
                                                if (!string.IsNullOrEmpty(gApLower.Body.dic["res1"].ToString()))
                                                {
                                                    apGP.res1 = gApLower.Body.dic["res1"].ToString();
                                                }
                                            }

                                            // 2018-07-23
                                            if (gApLower.Body.dic.ContainsKey("res2"))
                                            {
                                                if (!string.IsNullOrEmpty(gApLower.Body.dic["res2"].ToString()))
                                                {
                                                    apGP.res2 = gApLower.Body.dic["res2"].ToString();
                                                }
                                            }

                                            // 2018-07-23
                                            if (gApLower.Body.dic.ContainsKey("res3"))
                                            {
                                                if (!string.IsNullOrEmpty(gApLower.Body.dic["res3"].ToString()))
                                                {
                                                    apGP.res3 = gApLower.Body.dic["res3"].ToString();
                                                }
                                            }

                                            rtv = gDbHelper.ap_general_para_record_update(devInfo.id, apGP);

                                            Fill_IMMS_Info(ref gApLower, Main2ApControllerMsgType.ReportGenParaAck, rtv, gDbHelper.get_rtv_str(rtv), true, null, null);
                                            Send_Msg_2_ApCtrl_Lower(gApLower);
                                            break;

                                            #endregion                                  
                                        }                                   
                                    default:
                                        {
                                            #region 出错处理

                                            //返回出错处理
                                            string errInfo = string.Format("不支持的apInnerType:{0}.", dm);
                                            add_log_info(LogInfoType.EROR, errInfo, "Main", LogCategory.I);
                                            Logger.Trace(LogInfoType.EROR, errInfo, "Main", LogCategory.I);

                                            Fill_IMMS_Info(ref gApLower, Main2ApControllerMsgType.ReportGenParaAck, -1, errInfo, true, null, null);
                                            Send_Msg_2_ApCtrl_Lower(gApLower);
                                            break;

                                            #endregion
                                        }
                                }

                                #endregion                               
                            }
                            else
                            {
                                #region 上报处理

                                #region 黑白MD5信息获取

                                if (gApLower.Body.dic.ContainsKey("whiteimsi_md5"))
                                {
                                    if (!string.IsNullOrEmpty(gApLower.Body.dic["whiteimsi_md5"].ToString()))
                                    {
                                        whiteimsi_md5 = gApLower.Body.dic["whiteimsi_md5"].ToString();
                                    }
                                }
                                else
                                {
                                    //返回出错处理
                                    string errInfo = string.Format("{0}:没包含whiteimsi_md5字段.", Main2ApControllerMsgType.ReportGenPara);
                                    add_log_info(LogInfoType.EROR, errInfo, "Main", LogCategory.I);
                                    Logger.Trace(LogInfoType.EROR, errInfo, "Main", LogCategory.I);

                                    Fill_IMMS_Info(ref gApLower, Main2ApControllerMsgType.ReportGenParaAck, -1, errInfo, true, null, null);
                                    Send_Msg_2_ApCtrl_Lower(gApLower);
                                    break;
                                }

                                if (gApLower.Body.dic.ContainsKey("blackimsi_md5"))
                                {
                                    if (!string.IsNullOrEmpty(gApLower.Body.dic["blackimsi_md5"].ToString()))
                                    {
                                        blackimsi_md5 = gApLower.Body.dic["blackimsi_md5"].ToString();
                                    }
                                }
                                else
                                {
                                    //返回出错处理
                                    string errInfo = string.Format("{0}:没包含blackimsi_md5字段.", Main2ApControllerMsgType.ReportGenPara);
                                    add_log_info(LogInfoType.EROR, errInfo, "Main", LogCategory.I);
                                    Logger.Trace(LogInfoType.EROR, errInfo, "Main", LogCategory.I);

                                    Fill_IMMS_Info(ref gApLower, Main2ApControllerMsgType.ReportGenParaAck, -1, errInfo, true, null, null);
                                    Send_Msg_2_ApCtrl_Lower(gApLower);
                                    break;
                                }

                                if (whiteimsi_md5 == "" || blackimsi_md5 == "")
                                {
                                    //返回出错处理
                                    string errInfo = string.Format("获取whiteimsi_md5或blackimsi_md5出错.");
                                    add_log_info(LogInfoType.EROR, errInfo, "Main", LogCategory.I);
                                    Logger.Trace(LogInfoType.EROR, errInfo, "Main", LogCategory.I);

                                    Fill_IMMS_Info(ref gApLower, Main2ApControllerMsgType.ReportGenParaAck, -1, errInfo, true, null, null);
                                    Send_Msg_2_ApCtrl_Lower(gApLower);
                                    break;
                                }

                                strApGenPara apGP = new strApGenPara();                                
                                List<string> listBlackImsi = new List<string>();
                                List<string> listWhiteImsi = new List<string>();

                                #endregion

                                switch (dm)
                                {
                                    case devMode.MODE_GSM:                            
                                        {
                                            #region GSM处理

                                            #region 获取GSM所有的上报信息以及串

                                            string errInfo = "";
                                            str_Gsm_All_Para gsmAllParaReport = new str_Gsm_All_Para();
                                            if (get_gsm_report_info(gApLower, ref gsmAllParaReport, ref gsmParaFullString, ref errInfo) != 0)
                                            {
                                                add_log_info(LogInfoType.EROR, errInfo, "Main", LogCategory.I);
                                                Logger.Trace(LogInfoType.EROR, errInfo, "Main", LogCategory.I);

                                                Fill_IMMS_Info(ref gApLower, Main2ApControllerMsgType.ReportGenParaAck, -1, errInfo, true, null, null);
                                                Send_Msg_2_ApCtrl_Lower(gApLower);
                                                break;
                                            }

                                            int carry = gsmAllParaReport.sys;

                                            #endregion

                                            #region 计算DB中所有的参数串

                                            rtv = gDbHelper.gsm_all_record_get_by_devid(carry, devInfo.id, ref gsmParaFullString_db);
                                            if ((int)RC.SUCCESS != rtv)
                                            {
                                                errInfo = string.Format("gsm_all_record_get_by_devid出错.");
                                                add_log_info(LogInfoType.EROR, errInfo, "Main", LogCategory.I);
                                                Logger.Trace(LogInfoType.EROR, errInfo, "Main", LogCategory.I);

                                                Fill_IMMS_Info(ref gApLower, Main2ApControllerMsgType.ReportGenParaAck, -1, errInfo, true, null, null);
                                                Send_Msg_2_ApCtrl_Lower(gApLower);
                                                break;
                                            }

                                            if (gsmParaFullString.Equals(gsmParaFullString_db))
                                            {
                                                gsm_para_match = true;
                                            }
                                            else
                                            {
                                                errInfo = string.Format("gsmParaFullString={0}\ngsmParaFullString_db={1}", gsmParaFullString, gsmParaFullString_db);
                                                add_log_info(LogInfoType.INFO, errInfo, "Main", LogCategory.I);
                                                Logger.Trace(LogInfoType.INFO, errInfo, "Main", LogCategory.I);
                                            }

                                            #endregion

                                            #region 计算DB中黑白名单的MD5

                                            rtv = gDbHelper.bwlist_record_md5sum_get(bwType.BWTYPE_BLACK, devInfo.id, ref listBlackImsi);
                                            if ((int)RC.SUCCESS != rtv)
                                            {
                                                //返回出错处理
                                                errInfo = string.Format("bwlist_record_md5sum_get出错：{0}", gDbHelper.get_rtv_str(rtv));
                                                add_log_info(LogInfoType.EROR, errInfo, "Main", LogCategory.I);
                                                Logger.Trace(LogInfoType.EROR, errInfo, "Main", LogCategory.I);

                                                Fill_IMMS_Info(ref gApLower, Main2ApControllerMsgType.ReportGenParaAck, -1, errInfo, true, null, null);
                                                Send_Msg_2_ApCtrl_Lower(gApLower);
                                                break;
                                            }
                                            else
                                            {
                                                listBlackImsi.Sort();
                                            }

                                            rtv = gDbHelper.bwlist_record_md5sum_get(bwType.BWTYPE_WHITE, devInfo.id, ref listWhiteImsi);
                                            if ((int)RC.SUCCESS != rtv)
                                            {
                                                //返回出错处理
                                                errInfo = string.Format("bwlist_record_md5sum_get出错：{0}", gDbHelper.get_rtv_str(rtv));
                                                add_log_info(LogInfoType.EROR, errInfo, "Main", LogCategory.I);
                                                Logger.Trace(LogInfoType.EROR, errInfo, "Main", LogCategory.I);

                                                Fill_IMMS_Info(ref gApLower, Main2ApControllerMsgType.ReportGenParaAck, -1, errInfo, true, null, null);
                                                Send_Msg_2_ApCtrl_Lower(gApLower);
                                                break;
                                            }
                                            else
                                            {
                                                listWhiteImsi.Sort();
                                            }

                                            if (0 != Get_Md5_Sum(listBlackImsi, 1, ref blackimsi_md5_db))
                                            {
                                                //返回出错处理
                                                errInfo = string.Format("Get_Md5_Sum出错");
                                                add_log_info(LogInfoType.EROR, errInfo, "Main", LogCategory.I);
                                                Logger.Trace(LogInfoType.EROR, errInfo, "Main", LogCategory.I);

                                                Fill_IMMS_Info(ref gApLower, Main2ApControllerMsgType.ReportGenParaAck, -1, errInfo, true, null, null);
                                                Send_Msg_2_ApCtrl_Lower(gApLower);
                                                break;
                                            }

                                            if (0 != Get_Md5_Sum(listWhiteImsi, 1, ref whiteimsi_md5_db))
                                            {
                                                //返回出错处理
                                                errInfo = string.Format("Get_Md5_Sum出错");
                                                add_log_info(LogInfoType.EROR, errInfo, "Main", LogCategory.I);
                                                Logger.Trace(LogInfoType.EROR, errInfo, "Main", LogCategory.I);

                                                Fill_IMMS_Info(ref gApLower, Main2ApControllerMsgType.ReportGenParaAck, -1, errInfo, true, null, null);
                                                Send_Msg_2_ApCtrl_Lower(gApLower);
                                                break;
                                            }

                                            if (whiteimsi_md5.Equals(whiteimsi_md5_db))
                                            {
                                                whiteimsi_md5_match = true;                                                
                                            }
                                            else
                                            {
                                                errInfo = string.Format("whiteimsi_md5={0}\nwhiteimsi_md5_db={1}", whiteimsi_md5, whiteimsi_md5_db);
                                                add_log_info(LogInfoType.INFO, errInfo, "Main", LogCategory.I);
                                                Logger.Trace(LogInfoType.INFO, errInfo, "Main", LogCategory.I);
                                            }

                                            if (blackimsi_md5.Equals(blackimsi_md5_db))
                                            {
                                                blackimsi_md5_match = true;
                                            }
                                            else
                                            {
                                                errInfo = string.Format("blackimsi_md5={0}\nblackimsi_md5_db={1}", blackimsi_md5, blackimsi_md5_db);
                                                add_log_info(LogInfoType.INFO, errInfo, "Main", LogCategory.I);
                                                Logger.Trace(LogInfoType.INFO, errInfo, "Main", LogCategory.I);
                                            }

                                            gClsDataAlign.whiteimsi_md5_match = whiteimsi_md5_match;
                                            gClsDataAlign.blackimsi_md5_match = blackimsi_md5_match;

                                            #endregion

                                            #region 回复ReportGenParaAck

                                            Fill_IMMS_Info(ref gApLower, Main2ApControllerMsgType.ReportGenParaAck,0,"成功", true, null, null);
                                            Send_Msg_2_ApCtrl_Lower(gApLower);

                                            #endregion

                                            #region 流程处理

                                            if (gsm_para_match && blackimsi_md5_match && whiteimsi_md5_match)
                                            {
                                                //数据对齐                                        
                                                Fill_IMMS_Info(ref gApLower, Main2ApControllerMsgType.DataAlignOver, 0, "成功", true, null, null);
                                                Send_Msg_2_ApCtrl_Lower(gApLower);
                                            }
                                            else
                                            {
                                                //数据不对齐
                                                if (DataController.DataAlignMode == 0)
                                                {
                                                    #region 以DB为准

                                                    Fill_IMMS_Info(ref gApLower, Main2ApControllerMsgType.SetGenParaReq, 0, "成功", true, null, null);
                                                    gApLower.Body.dic.Add("ApIsBase", DataController.DataAlignMode.ToString());

                                                    #region 上传白名单到FTP

                                                    if (!whiteimsi_md5_match)
                                                    {                                                        
                                                        string fileName = gClsDataAlign.fileNameWhiteList_Db_Base;

                                                        try
                                                        {
                                                            byte[] data = null;
                                                            if (0 == generate_ftp_byte(ref data, listWhiteImsi))
                                                            {
                                                                rtv = gFtpHelperFile.Put(fileName, data);
                                                                if (rtv != 0)
                                                                {
                                                                    //返回出错处理
                                                                    errInfo = string.Format("上传对齐文件listWhiteImsi到FTP服务器出错.");
                                                                    add_log_info(LogInfoType.EROR, errInfo, "Main", LogCategory.I);
                                                                    Logger.Trace(LogInfoType.EROR, errInfo, "Main", LogCategory.I);

                                                                    Fill_IMMS_Info(ref gApLower, Main2ApControllerMsgType.ReportGenParaAck, -1, errInfo, true, null, null);
                                                                    Send_Msg_2_ApCtrl_Lower(gApLower);
                                                                    break;
                                                                }
                                                            }
                                                        }
                                                        catch (Exception e)
                                                        {
                                                            //返回出错处理
                                                            errInfo = string.Format("上传对齐文件listWhiteImsi到FTP服务器出错." + e.Message);
                                                            add_log_info(LogInfoType.EROR, errInfo, "Main", LogCategory.I);
                                                            Logger.Trace(LogInfoType.EROR, errInfo, "Main", LogCategory.I);

                                                            Fill_IMMS_Info(ref gApLower, Main2ApControllerMsgType.ReportGenParaAck, -1, errInfo, true, null, null);
                                                            Send_Msg_2_ApCtrl_Lower(gApLower);
                                                            break;
                                                        }                                                      

                                                        string ftpUrl = string.Format("ftp://{0}:{1}@{2}:{3}/{4}/{5}",
                                                            DataController.StrFtpUserId,
                                                            DataController.StrFtpUserPsw,
                                                            DataController.StrFtpIpAddr,
                                                            DataController.StrFtpPort,
                                                            DataController.StrFtpUpdateDir,
                                                            fileName);

                                                        gApLower.Body.dic.Add("FtpUrl_White", ftpUrl);

                                                        if (!gApLower.Body.dic.ContainsKey("FtpUser"))
                                                        {
                                                            gApLower.Body.dic.Add("FtpUser", DataController.StrFtpUserId);
                                                            gApLower.Body.dic.Add("FtpPas", DataController.StrFtpUserPsw);
                                                            gApLower.Body.dic.Add("ftpRootDir", DataController.StrFtpUpdateDir);
                                                            gApLower.Body.dic.Add("ftpServerIp", DataController.StrFtpIpAddr);
                                                            gApLower.Body.dic.Add("ftpPort", DataController.StrFtpPort);
                                                            gApLower.Body.dic.Add("sys", carry.ToString());
                                                        }
                                                    }

                                                    #endregion

                                                    #region 上传黑名单到FTP

                                                    if (!blackimsi_md5_match)
                                                    {                                                        
                                                        string fileName = gClsDataAlign.fileNameBlackList_Db_Base;

                                                        try
                                                        {
                                                            byte[] data = null;
                                                            if (0 == generate_ftp_byte(ref data, listBlackImsi))
                                                            {
                                                                rtv = gFtpHelperFile.Put(fileName, data);
                                                                if (rtv != 0)
                                                                {
                                                                    //返回出错处理
                                                                    errInfo = string.Format("上传对齐文件listBlackImsi到FTP服务器出错.");
                                                                    add_log_info(LogInfoType.EROR, errInfo, "Main", LogCategory.I);
                                                                    Logger.Trace(LogInfoType.EROR, errInfo, "Main", LogCategory.I);

                                                                    Fill_IMMS_Info(ref gApLower, Main2ApControllerMsgType.ReportGenParaAck, -1, errInfo, true, null, null);
                                                                    Send_Msg_2_ApCtrl_Lower(gApLower);
                                                                    break;
                                                                }
                                                            }
                                                        }
                                                        catch (Exception e)
                                                        {
                                                            //返回出错处理
                                                            errInfo = string.Format("上传对齐文件listBlackImsi到FTP服务器出错." + e.Message);
                                                            add_log_info(LogInfoType.EROR, errInfo, "Main", LogCategory.I);
                                                            Logger.Trace(LogInfoType.EROR, errInfo, "Main", LogCategory.I);

                                                            Fill_IMMS_Info(ref gApLower, Main2ApControllerMsgType.ReportGenParaAck, -1, errInfo, true, null, null);
                                                            Send_Msg_2_ApCtrl_Lower(gApLower);
                                                            break;
                                                        }                                                   

                                                        string ftpUrl = string.Format("ftp://{0}:{1}@{2}:{3}/{4}/{5}",
                                                            DataController.StrFtpUserId,
                                                            DataController.StrFtpUserPsw,
                                                            DataController.StrFtpIpAddr,
                                                            DataController.StrFtpPort,
                                                            DataController.StrFtpUpdateDir,
                                                            fileName);

                                                        gApLower.Body.dic.Add("FtpUrl_Black", ftpUrl);

                                                        if (!gApLower.Body.dic.ContainsKey("FtpUser"))
                                                        {
                                                            gApLower.Body.dic.Add("FtpUser", DataController.StrFtpUserId);
                                                            gApLower.Body.dic.Add("FtpPas", DataController.StrFtpUserPsw);
                                                            gApLower.Body.dic.Add("ftpRootDir", DataController.StrFtpUpdateDir);
                                                            gApLower.Body.dic.Add("ftpServerIp", DataController.StrFtpIpAddr);
                                                            gApLower.Body.dic.Add("ftpPort", DataController.StrFtpPort);
                                                            gApLower.Body.dic.Add("sys", carry.ToString());
                                                        }
                                                    }

                                                    #endregion

                                                    #region 将DB中通用参数传给AP

                                                    if (!gsm_para_match)
                                                    {
                                                        str_Gsm_All_Para allInfo = new str_Gsm_All_Para();

                                                        //(1)
                                                        rtv = gDbHelper.gsm_sys_para_record_get_by_devid(carry, devInfo.id, ref allInfo.gsmSysPara);
                                                        if (rtv != 0)
                                                        {
                                                            errInfo = string.Format("gsm_sys_para_record_get_by_devid出错:") + gDbHelper.get_rtv_str(rtv) ;
                                                            add_log_info(LogInfoType.EROR, errInfo, "Main", LogCategory.I);
                                                            Logger.Trace(LogInfoType.EROR, errInfo, "Main", LogCategory.I);

                                                            Fill_IMMS_Info(ref gApLower, Main2ApControllerMsgType.ReportGenParaAck, -1, errInfo, true, null, null);
                                                            Send_Msg_2_ApCtrl_Lower(gApLower);
                                                            break;
                                                        }

                                                        //(2)
                                                        rtv = gDbHelper.gsm_sys_option_record_get_by_devid(carry, devInfo.id, ref allInfo.gsmSysOption);
                                                        if (rtv != 0)
                                                        {
                                                            errInfo = string.Format("gsm_sys_option_record_get_by_devid出错:") + gDbHelper.get_rtv_str(rtv);
                                                            add_log_info(LogInfoType.EROR, errInfo, "Main", LogCategory.I);
                                                            Logger.Trace(LogInfoType.EROR, errInfo, "Main", LogCategory.I);

                                                            Fill_IMMS_Info(ref gApLower, Main2ApControllerMsgType.ReportGenParaAck, -1, errInfo, true, null, null);
                                                            Send_Msg_2_ApCtrl_Lower(gApLower);
                                                            break;
                                                        }

                                                        //(3)
                                                        rtv = gDbHelper.gsm_rf_para_record_get_by_devid(carry, devInfo.id, ref allInfo.gsmRfPara);
                                                        if (rtv != 0)
                                                        {
                                                            errInfo = string.Format("gsm_rf_para_record_get_by_devid出错:") + gDbHelper.get_rtv_str(rtv);
                                                            add_log_info(LogInfoType.EROR, errInfo, "Main", LogCategory.I);
                                                            Logger.Trace(LogInfoType.EROR, errInfo, "Main", LogCategory.I);

                                                            Fill_IMMS_Info(ref gApLower, Main2ApControllerMsgType.ReportGenParaAck, -1, errInfo, true, null, null);
                                                            Send_Msg_2_ApCtrl_Lower(gApLower);
                                                            break;
                                                        }

                                                        //       "name":"RECV_SYS_PARA",                 //4.1   系统参数
                                                        //      {
                                                        //					"paraMcc":移动国家码
                                                        //					"paraMnc":移动网号
                                                        //					"paraBsic":基站识别码
                                                        //					"paraLac":位置区号
                                                        //					"paraCellId":小区ID
                                                        //					"paraC2":C2偏移量
                                                        //					"paraPeri":周期性位置更新周期
                                                        //					"paraAccPwr":接入功率
                                                        //					"paraMsPwr":手机发射功率
                                                        //					"paraRejCau":位置更新拒绝原因
                                                        //       }
                                                        Name_DIC_Struct ndic = new Name_DIC_Struct();
                                                        ndic.name = "RECV_SYS_PARA";

                                                        ndic.dic.Add("paraMcc", allInfo.gsmSysPara.paraMcc);
                                                        ndic.dic.Add("paraMnc", allInfo.gsmSysPara.paraMnc);
                                                        ndic.dic.Add("paraBsic", allInfo.gsmSysPara.paraBsic);
                                                        ndic.dic.Add("paraLac", allInfo.gsmSysPara.paraLac);
                                                        ndic.dic.Add("paraCellId", allInfo.gsmSysPara.paraCellId);
                                                        ndic.dic.Add("paraC2", allInfo.gsmSysPara.paraC2);
                                                        ndic.dic.Add("paraPeri", allInfo.gsmSysPara.paraPeri);
                                                        ndic.dic.Add("paraAccPwr", allInfo.gsmSysPara.paraAccPwr);
                                                        ndic.dic.Add("paraMsPwr", allInfo.gsmSysPara.paraMsPwr);
                                                        ndic.dic.Add("paraRejCau", allInfo.gsmSysPara.paraRejCau);
                                                        gApLower.Body.n_dic.Add(ndic);

                                                        //       "name":"RECV_SYS_OPTION",                 //4.2  系统选项
                                                        //      {
                                                        //					"opLuSms":登录时发送短信
                                                        //					"opLuImei":登录时获取IMEI
                                                        //					"opCallEn":允许用户主叫
                                                        //					"opDebug":调试模式，上报信令
                                                        //					"opLuType":登录类型
                                                        //					"opSmsType":短信类型
                                                        //       }
                                                        ndic = new Name_DIC_Struct();
                                                        ndic.name = "RECV_SYS_OPTION";

                                                        ndic.dic.Add("opLuSms", allInfo.gsmSysOption.opLuSms);
                                                        ndic.dic.Add("opLuImei", allInfo.gsmSysOption.opLuImei);
                                                        ndic.dic.Add("opCallEn", allInfo.gsmSysOption.opCallEn);
                                                        ndic.dic.Add("opDebug", allInfo.gsmSysOption.opDebug);
                                                        ndic.dic.Add("opLuType", allInfo.gsmSysOption.opLuType);
                                                        ndic.dic.Add("opSmsType", allInfo.gsmSysOption.opSmsType);
                                                        //ndic.dic.Add("opRegModel", allInfo.gsmSysOption.opRegModel);
                                                        gApLower.Body.n_dic.Add(ndic);


                                                        //       "name":"RECV_RF_PARA",                 //4.4	射频参数
                                                        //      {
                                                        //					"rfEnable":射频使能
                                                        //					"rfFreq":信道号
                                                        //					"rfPwr":发射功率衰减值
                                                        //       }
                                                        ndic = new Name_DIC_Struct();
                                                        ndic.name = "RECV_RF_PARA";
                                                        ndic.dic.Add("rfEnable", allInfo.gsmRfPara.rfEnable);
                                                        ndic.dic.Add("rfFreq", allInfo.gsmRfPara.rfFreq);
                                                        ndic.dic.Add("rfPwr", allInfo.gsmRfPara.rfPwr);
                                                        gApLower.Body.n_dic.Add(ndic);

                                                        //       "name":"RECV_REG_MODE",            //4.33	注册工作模式
                                                        //       {
                                                        //          "regMode":模式0时由设备自行根据系统选项决定是否允许终端入网，是否对终端发送短信；
                                                        //                    模式1时设备将终端标识发送给上位机，由上位机告知设备下一步的动作
                                                        //       }
                                                        ndic = new Name_DIC_Struct();
                                                        ndic.name = "RECV_REG_MODE";

                                                        ndic.dic.Add("regMode", allInfo.gsmSysOption.opRegModel);
                                                        gApLower.Body.n_dic.Add(ndic);
                                                    }

                                                    #endregion

                                                    Send_Msg_2_ApCtrl_Lower(gApLower);

                                                    break;

                                                    #endregion
                                                }
                                                else
                                                {
                                                    #region 以AP为准

                                                    #region 告诉AP发白名单FTP服务器上

                                                    if (!whiteimsi_md5_match)
                                                    {
                                                        Fill_IMMS_Info(ref gApLower, Main2ApControllerMsgType.SetGenParaReq, 0, "成功", true, null, null);
                                                        gApLower.Body.dic.Add("ApIsBase", DataController.DataAlignMode.ToString());
                                                       
                                                        string fileName = gClsDataAlign.fileNameWhiteList_Ap_Base;

                                                        string ftpUrl = string.Format("ftp://{0}:{1}@{2}:{3}/{4}/{5}",
                                                            DataController.StrFtpUserId,
                                                            DataController.StrFtpUserPsw,
                                                            DataController.StrFtpIpAddr,
                                                            DataController.StrFtpPort,
                                                            DataController.StrFtpUpdateDir,
                                                            fileName);

                                                        gApLower.Body.dic.Add("FtpUrl_White", ftpUrl);

                                                        if (!gApLower.Body.dic.ContainsKey("FtpUser"))
                                                        {
                                                            gApLower.Body.dic.Add("FtpUser", DataController.StrFtpUserId);
                                                            gApLower.Body.dic.Add("FtpPas", DataController.StrFtpUserPsw);
                                                            gAppUpper.Body.dic.Add("ftpRootDir", DataController.StrFtpUpdateDir);
                                                            gAppUpper.Body.dic.Add("ftpServerIp", DataController.StrFtpIpAddr);
                                                            gAppUpper.Body.dic.Add("ftpPort", DataController.StrFtpPort);
                                                        }
                                                    }

                                                    #endregion

                                                    #region 告诉AP发黑名单到FTP服务器上

                                                    if (!blackimsi_md5_match)
                                                    {
                                                        Fill_IMMS_Info(ref gApLower, Main2ApControllerMsgType.SetGenParaReq, 0, "成功", true, null, null);
                                                        gApLower.Body.dic.Add("ApIsBase", DataController.DataAlignMode.ToString());
                                                       
                                                        string fileName = gClsDataAlign.fileNameBlackList_Ap_Base;

                                                        string ftpUrl = string.Format("ftp://{0}:{1}@{2}:{3}/{4}/{5}",
                                                            DataController.StrFtpUserId,
                                                            DataController.StrFtpUserPsw,
                                                            DataController.StrFtpIpAddr,
                                                            DataController.StrFtpPort,
                                                            DataController.StrFtpUpdateDir,
                                                            fileName);

                                                        gApLower.Body.dic.Add("FtpUrl_Black", ftpUrl);

                                                        if (!gApLower.Body.dic.ContainsKey("FtpUser"))
                                                        {
                                                            gApLower.Body.dic.Add("FtpUser", DataController.StrFtpUserId);
                                                            gApLower.Body.dic.Add("FtpPas", DataController.StrFtpUserPsw);
                                                            gAppUpper.Body.dic.Add("ftpRootDir", DataController.StrFtpUpdateDir);
                                                            gAppUpper.Body.dic.Add("ftpServerIp", DataController.StrFtpIpAddr);
                                                            gAppUpper.Body.dic.Add("ftpPort", DataController.StrFtpPort);
                                                        }
                                                    }

                                                    #endregion

                                                    #region 将通用参数更新到DB

                                                    if (!gsm_para_match)
                                                    {
                                                        rtv = 0;
                                                        if (gsmAllParaReport.gsmSysParaFlag == true)
                                                        {
                                                            rtv += gDbHelper.gsm_sys_para_record_update(carry, devInfo.id, gsmAllParaReport.gsmSysPara);
                                                        }

                                                        if (gsmAllParaReport.gsmSysOptionFlag == true)
                                                        {
                                                            rtv += gDbHelper.gsm_sys_option_record_update(carry, devInfo.id, gsmAllParaReport.gsmSysOption);
                                                        }

                                                        //if (gsmAllParaReport.gsmRfParaFlag == true)
                                                        //{
                                                        //    rtv += gDbHelper.gsm_rf_para_record_update(carry, devInfo.id, gsmAllParaReport.gsmRfPara);
                                                        //}

                                                        if (gsmAllParaReport.gsmMsgOptionFlag == true)
                                                        {
                                                            rtv += gDbHelper.gsm_msg_option_insert(carry, devInfo.id, gsmAllParaReport.gsmMsgOption);
                                                        }

                                                        if ((int)RC.SUCCESS != rtv)
                                                        {
                                                            //返回出错处理
                                                            errInfo = string.Format("对齐时gsm_sys_para_record_update等等出错.");
                                                            add_log_info(LogInfoType.EROR, errInfo, "Main", LogCategory.I);
                                                            Logger.Trace(LogInfoType.EROR, errInfo, "Main", LogCategory.I);

                                                            Fill_IMMS_Info(ref gApLower, Main2ApControllerMsgType.ReportGenParaAck, -1, errInfo, true, null, null);
                                                            Send_Msg_2_ApCtrl_Lower(gApLower);
                                                            break;
                                                        }
                                                        else
                                                        {
                                                            Fill_IMMS_Info(ref gApLower, Main2ApControllerMsgType.DataAlignOver, 0, "成功", true, null, null);
                                                            Send_Msg_2_ApCtrl_Lower(gApLower);
                                                        }
                                                    }
                                                    else
                                                    {
                                                        Fill_IMMS_Info(ref gApLower, Main2ApControllerMsgType.DataAlignOver, 0, "成功", true, null, null);
                                                        Send_Msg_2_ApCtrl_Lower(gApLower);
                                                    }

                                                    #endregion

                                                    #endregion
                                                }
                                            }

                                            #endregion

                                            break;

                                            #endregion                                            
                                        }
                                    case devMode.MODE_GSM_V2:
                                    case devMode.MODE_CDMA:
                                        {
                                            #region GSM-V2/CDMA处理

                                            #region 获取GSM所有的上报信息以及串

                                            string errInfo = "";
                                            str_Gsm_All_Para gsmAllParaReport = new str_Gsm_All_Para();
                                            if (get_gsm_report_info(gApLower, ref gsmAllParaReport, ref gsmParaFullString, ref errInfo) != 0)
                                            {
                                                add_log_info(LogInfoType.EROR, errInfo, "Main", LogCategory.I);
                                                Logger.Trace(LogInfoType.EROR, errInfo, "Main", LogCategory.I);

                                                Fill_IMMS_Info(ref gApLower, Main2ApControllerMsgType.ReportGenParaAck, -1, errInfo, true, null, null);
                                                Send_Msg_2_ApCtrl_Lower(gApLower);
                                                break;
                                            }

                                            int carry = gsmAllParaReport.sys;

                                            #endregion

                                            #region 计算DB中所有的参数串

                                            rtv = gDbHelper.gsm_all_record_get_by_devid(carry, devInfo.id, ref gsmParaFullString_db);
                                            if ((int)RC.SUCCESS != rtv)
                                            {
                                                errInfo = string.Format("gsm_all_record_get_by_devid出错.");
                                                add_log_info(LogInfoType.EROR, errInfo, "Main", LogCategory.I);
                                                Logger.Trace(LogInfoType.EROR, errInfo, "Main", LogCategory.I);

                                                Fill_IMMS_Info(ref gApLower, Main2ApControllerMsgType.ReportGenParaAck, -1, errInfo, true, null, null);
                                                Send_Msg_2_ApCtrl_Lower(gApLower);
                                                break;
                                            }

                                            if (gsmParaFullString.Equals(gsmParaFullString_db))
                                            {
                                                gsm_para_match = true;
                                            }
                                            else
                                            {
                                                errInfo = string.Format("gsmParaFullString={0}\ngsmParaFullString_db={1}", gsmParaFullString, gsmParaFullString_db);
                                                add_log_info(LogInfoType.INFO, errInfo, "Main", LogCategory.I);
                                                Logger.Trace(LogInfoType.INFO, errInfo, "Main", LogCategory.I);
                                            }

                                            #endregion

                                            #region 计算DB中黑白名单的MD5

                                            rtv = gDbHelper.bwlist_record_md5sum_get(bwType.BWTYPE_BLACK, devInfo.id, ref listBlackImsi);
                                            if ((int)RC.SUCCESS != rtv)
                                            {
                                                //返回出错处理
                                                errInfo = string.Format("bwlist_record_md5sum_get出错：{0}", gDbHelper.get_rtv_str(rtv));
                                                add_log_info(LogInfoType.EROR, errInfo, "Main", LogCategory.I);
                                                Logger.Trace(LogInfoType.EROR, errInfo, "Main", LogCategory.I);

                                                Fill_IMMS_Info(ref gApLower, Main2ApControllerMsgType.ReportGenParaAck, -1, errInfo, true, null, null);
                                                Send_Msg_2_ApCtrl_Lower(gApLower);
                                                break;
                                            }
                                            else
                                            {
                                                listBlackImsi.Sort();
                                            }

                                            rtv = gDbHelper.bwlist_record_md5sum_get(bwType.BWTYPE_WHITE, devInfo.id, ref listWhiteImsi);
                                            if ((int)RC.SUCCESS != rtv)
                                            {
                                                //返回出错处理
                                                errInfo = string.Format("bwlist_record_md5sum_get出错：{0}", gDbHelper.get_rtv_str(rtv));
                                                add_log_info(LogInfoType.EROR, errInfo, "Main", LogCategory.I);
                                                Logger.Trace(LogInfoType.EROR, errInfo, "Main", LogCategory.I);

                                                Fill_IMMS_Info(ref gApLower, Main2ApControllerMsgType.ReportGenParaAck, -1, errInfo, true, null, null);
                                                Send_Msg_2_ApCtrl_Lower(gApLower);
                                                break;
                                            }
                                            else
                                            {
                                                listWhiteImsi.Sort();
                                            }

                                            if (0 != Get_Md5_Sum(listBlackImsi, 1, ref blackimsi_md5_db))
                                            {
                                                //返回出错处理
                                                errInfo = string.Format("Get_Md5_Sum出错");
                                                add_log_info(LogInfoType.EROR, errInfo, "Main", LogCategory.I);
                                                Logger.Trace(LogInfoType.EROR, errInfo, "Main", LogCategory.I);

                                                Fill_IMMS_Info(ref gApLower, Main2ApControllerMsgType.ReportGenParaAck, -1, errInfo, true, null, null);
                                                Send_Msg_2_ApCtrl_Lower(gApLower);
                                                break;
                                            }

                                            if (0 != Get_Md5_Sum(listWhiteImsi, 1, ref whiteimsi_md5_db))
                                            {
                                                //返回出错处理
                                                errInfo = string.Format("Get_Md5_Sum出错");
                                                add_log_info(LogInfoType.EROR, errInfo, "Main", LogCategory.I);
                                                Logger.Trace(LogInfoType.EROR, errInfo, "Main", LogCategory.I);

                                                Fill_IMMS_Info(ref gApLower, Main2ApControllerMsgType.ReportGenParaAck, -1, errInfo, true, null, null);
                                                Send_Msg_2_ApCtrl_Lower(gApLower);
                                                break;
                                            }

                                            if (whiteimsi_md5.Equals(whiteimsi_md5_db))
                                            {
                                                whiteimsi_md5_match = true;
                                            }
                                            else
                                            {
                                                errInfo = string.Format("whiteimsi_md5={0}\nwhiteimsi_md5_db={1}", whiteimsi_md5, whiteimsi_md5_db);
                                                add_log_info(LogInfoType.INFO, errInfo, "Main", LogCategory.I);
                                                Logger.Trace(LogInfoType.INFO, errInfo, "Main", LogCategory.I);
                                            }

                                            if (blackimsi_md5.Equals(blackimsi_md5_db))
                                            {
                                                blackimsi_md5_match = true;
                                            }
                                            else
                                            {
                                                errInfo = string.Format("blackimsi_md5={0}\nblackimsi_md5_db={1}", blackimsi_md5, blackimsi_md5_db);
                                                add_log_info(LogInfoType.INFO, errInfo, "Main", LogCategory.I);
                                                Logger.Trace(LogInfoType.INFO, errInfo, "Main", LogCategory.I);
                                            }

                                            gClsDataAlign.whiteimsi_md5_match = whiteimsi_md5_match;
                                            gClsDataAlign.blackimsi_md5_match = blackimsi_md5_match;

                                            #endregion

                                            #region 回复ReportGenParaAck

                                            Fill_IMMS_Info(ref gApLower, Main2ApControllerMsgType.ReportGenParaAck, 0, "成功", true, null, null);
                                            Send_Msg_2_ApCtrl_Lower(gApLower);

                                            #endregion

                                            #region 流程处理

                                            if (gsm_para_match && blackimsi_md5_match && whiteimsi_md5_match)
                                            {
                                                //数据对齐                                        
                                                Fill_IMMS_Info(ref gApLower, Main2ApControllerMsgType.DataAlignOver, 0, "成功", true, null, null);
                                                Send_Msg_2_ApCtrl_Lower(gApLower);
                                            }
                                            else
                                            {
                                                //数据不对齐
                                                if (DataController.DataAlignMode == 0)
                                                {
                                                    #region 以DB为准

                                                    Fill_IMMS_Info(ref gApLower, Main2ApControllerMsgType.SetGenParaReq, 0, "成功", true, null, null);
                                                    gApLower.Body.dic.Add("ApIsBase", DataController.DataAlignMode.ToString());

                                                    #region 上传白名单到FTP

                                                    if (!whiteimsi_md5_match)
                                                    {
                                                        string fileName = gClsDataAlign.fileNameWhiteList_Db_Base;

                                                        try
                                                        {
                                                            byte[] data = null;
                                                            if (0 == generate_ftp_byte(ref data, listWhiteImsi))
                                                            {
                                                                rtv = gFtpHelperFile.Put(fileName, data);
                                                                if (rtv != 0)
                                                                {
                                                                    //返回出错处理
                                                                    errInfo = string.Format("上传对齐文件listWhiteImsi到FTP服务器出错.");
                                                                    add_log_info(LogInfoType.EROR, errInfo, "Main", LogCategory.I);
                                                                    Logger.Trace(LogInfoType.EROR, errInfo, "Main", LogCategory.I);

                                                                    Fill_IMMS_Info(ref gApLower, Main2ApControllerMsgType.ReportGenParaAck, -1, errInfo, true, null, null);
                                                                    Send_Msg_2_ApCtrl_Lower(gApLower);
                                                                    break;
                                                                }
                                                            }
                                                        }
                                                        catch (Exception e)
                                                        {
                                                            //返回出错处理
                                                            errInfo = string.Format("上传对齐文件listWhiteImsi到FTP服务器出错." + e.Message);
                                                            add_log_info(LogInfoType.EROR, errInfo, "Main", LogCategory.I);
                                                            Logger.Trace(LogInfoType.EROR, errInfo, "Main", LogCategory.I);

                                                            Fill_IMMS_Info(ref gApLower, Main2ApControllerMsgType.ReportGenParaAck, -1, errInfo, true, null, null);
                                                            Send_Msg_2_ApCtrl_Lower(gApLower);
                                                            break;
                                                        }

                                                        string ftpUrl = string.Format("ftp://{0}:{1}@{2}:{3}/{4}/{5}",
                                                            DataController.StrFtpUserId,
                                                            DataController.StrFtpUserPsw,
                                                            DataController.StrFtpIpAddr,
                                                            DataController.StrFtpPort,
                                                            DataController.StrFtpUpdateDir,
                                                            fileName);

                                                        gApLower.Body.dic.Add("FtpUrl_White", ftpUrl);

                                                        if (!gApLower.Body.dic.ContainsKey("FtpUser"))
                                                        {
                                                            gApLower.Body.dic.Add("FtpUser", DataController.StrFtpUserId);
                                                            gApLower.Body.dic.Add("FtpPas", DataController.StrFtpUserPsw);
                                                            gApLower.Body.dic.Add("ftpRootDir", DataController.StrFtpUpdateDir);
                                                            gApLower.Body.dic.Add("ftpServerIp", DataController.StrFtpIpAddr);
                                                            gApLower.Body.dic.Add("ftpPort", DataController.StrFtpPort);
                                                            gApLower.Body.dic.Add("sys", carry.ToString());
                                                        }
                                                    }

                                                    #endregion

                                                    #region 上传黑名单到FTP

                                                    if (!blackimsi_md5_match)
                                                    {
                                                        string fileName = gClsDataAlign.fileNameBlackList_Db_Base;

                                                        try
                                                        {
                                                            byte[] data = null;
                                                            if (0 == generate_ftp_byte(ref data, listBlackImsi))
                                                            {
                                                                rtv = gFtpHelperFile.Put(fileName, data);
                                                                if (rtv != 0)
                                                                {
                                                                    //返回出错处理
                                                                    errInfo = string.Format("上传对齐文件listBlackImsi到FTP服务器出错.");
                                                                    add_log_info(LogInfoType.EROR, errInfo, "Main", LogCategory.I);
                                                                    Logger.Trace(LogInfoType.EROR, errInfo, "Main", LogCategory.I);

                                                                    Fill_IMMS_Info(ref gApLower, Main2ApControllerMsgType.ReportGenParaAck, -1, errInfo, true, null, null);
                                                                    Send_Msg_2_ApCtrl_Lower(gApLower);
                                                                    break;
                                                                }
                                                            }
                                                        }
                                                        catch (Exception e)
                                                        {
                                                            //返回出错处理
                                                            errInfo = string.Format("上传对齐文件listBlackImsi到FTP服务器出错." + e.Message);
                                                            add_log_info(LogInfoType.EROR, errInfo, "Main", LogCategory.I);
                                                            Logger.Trace(LogInfoType.EROR, errInfo, "Main", LogCategory.I);

                                                            Fill_IMMS_Info(ref gApLower, Main2ApControllerMsgType.ReportGenParaAck, -1, errInfo, true, null, null);
                                                            Send_Msg_2_ApCtrl_Lower(gApLower);
                                                            break;
                                                        }

                                                        string ftpUrl = string.Format("ftp://{0}:{1}@{2}:{3}/{4}/{5}",
                                                            DataController.StrFtpUserId,
                                                            DataController.StrFtpUserPsw,
                                                            DataController.StrFtpIpAddr,
                                                            DataController.StrFtpPort,
                                                            DataController.StrFtpUpdateDir,
                                                            fileName);

                                                        gApLower.Body.dic.Add("FtpUrl_Black", ftpUrl);

                                                        if (!gApLower.Body.dic.ContainsKey("FtpUser"))
                                                        {
                                                            gApLower.Body.dic.Add("FtpUser", DataController.StrFtpUserId);
                                                            gApLower.Body.dic.Add("FtpPas", DataController.StrFtpUserPsw);
                                                            gApLower.Body.dic.Add("ftpRootDir", DataController.StrFtpUpdateDir);
                                                            gApLower.Body.dic.Add("ftpServerIp", DataController.StrFtpIpAddr);
                                                            gApLower.Body.dic.Add("ftpPort", DataController.StrFtpPort);
                                                            gApLower.Body.dic.Add("sys", carry.ToString());
                                                        }
                                                    }

                                                    #endregion

                                                    #region 将DB中通用参数传给AP

                                                    if (!gsm_para_match)
                                                    {
                                                        str_Gsm_All_Para allInfo = new str_Gsm_All_Para();

                                                        //(1)
                                                        rtv = gDbHelper.gsm_sys_para_record_get_by_devid(carry, devInfo.id, ref allInfo.gsmSysPara);
                                                        if (rtv != 0)
                                                        {
                                                            errInfo = string.Format("gsm_sys_para_record_get_by_devid出错:") + gDbHelper.get_rtv_str(rtv);
                                                            add_log_info(LogInfoType.EROR, errInfo, "Main", LogCategory.I);
                                                            Logger.Trace(LogInfoType.EROR, errInfo, "Main", LogCategory.I);

                                                            Fill_IMMS_Info(ref gApLower, Main2ApControllerMsgType.ReportGenParaAck, -1, errInfo, true, null, null);
                                                            Send_Msg_2_ApCtrl_Lower(gApLower);
                                                            break;
                                                        }

                                                        //(2)
                                                        rtv = gDbHelper.gsm_sys_option_record_get_by_devid(carry, devInfo.id, ref allInfo.gsmSysOption);
                                                        if (rtv != 0)
                                                        {
                                                            errInfo = string.Format("gsm_sys_option_record_get_by_devid出错:") + gDbHelper.get_rtv_str(rtv);
                                                            add_log_info(LogInfoType.EROR, errInfo, "Main", LogCategory.I);
                                                            Logger.Trace(LogInfoType.EROR, errInfo, "Main", LogCategory.I);

                                                            Fill_IMMS_Info(ref gApLower, Main2ApControllerMsgType.ReportGenParaAck, -1, errInfo, true, null, null);
                                                            Send_Msg_2_ApCtrl_Lower(gApLower);
                                                            break;
                                                        }

                                                        //(3)
                                                        rtv = gDbHelper.gsm_rf_para_record_get_by_devid(carry, devInfo.id, ref allInfo.gsmRfPara);
                                                        if (rtv != 0)
                                                        {
                                                            errInfo = string.Format("gsm_rf_para_record_get_by_devid出错:") + gDbHelper.get_rtv_str(rtv);
                                                            add_log_info(LogInfoType.EROR, errInfo, "Main", LogCategory.I);
                                                            Logger.Trace(LogInfoType.EROR, errInfo, "Main", LogCategory.I);

                                                            Fill_IMMS_Info(ref gApLower, Main2ApControllerMsgType.ReportGenParaAck, -1, errInfo, true, null, null);
                                                            Send_Msg_2_ApCtrl_Lower(gApLower);
                                                            break;
                                                        }

                                                        //       "name":"RECV_SYS_PARA",                 //4.1   系统参数
                                                        //      {
                                                        //					"paraMcc":移动国家码
                                                        //					"paraMnc":移动网号
                                                        //					"paraBsic":基站识别码
                                                        //					"paraLac":位置区号
                                                        //					"paraCellId":小区ID
                                                        //					"paraC2":C2偏移量
                                                        //					"paraPeri":周期性位置更新周期
                                                        //					"paraAccPwr":接入功率
                                                        //					"paraMsPwr":手机发射功率
                                                        //					"paraRejCau":位置更新拒绝原因
                                                        //       }
                                                        Name_DIC_Struct ndic = new Name_DIC_Struct();
                                                        ndic.name = "RECV_SYS_PARA";

                                                        ndic.dic.Add("paraMcc", allInfo.gsmSysPara.paraMcc);
                                                        ndic.dic.Add("paraMnc", allInfo.gsmSysPara.paraMnc);
                                                        ndic.dic.Add("paraBsic", allInfo.gsmSysPara.paraBsic);
                                                        ndic.dic.Add("paraLac", allInfo.gsmSysPara.paraLac);
                                                        ndic.dic.Add("paraCellId", allInfo.gsmSysPara.paraCellId);
                                                        ndic.dic.Add("paraC2", allInfo.gsmSysPara.paraC2);
                                                        ndic.dic.Add("paraPeri", allInfo.gsmSysPara.paraPeri);
                                                        ndic.dic.Add("paraAccPwr", allInfo.gsmSysPara.paraAccPwr);
                                                        ndic.dic.Add("paraMsPwr", allInfo.gsmSysPara.paraMsPwr);
                                                        ndic.dic.Add("paraRejCau", allInfo.gsmSysPara.paraRejCau);
                                                        gApLower.Body.n_dic.Add(ndic);

                                                        //       "name":"RECV_SYS_OPTION",                 //4.2  系统选项
                                                        //      {
                                                        //					"opLuSms":登录时发送短信
                                                        //					"opLuImei":登录时获取IMEI
                                                        //					"opCallEn":允许用户主叫
                                                        //					"opDebug":调试模式，上报信令
                                                        //					"opLuType":登录类型
                                                        //					"opSmsType":短信类型
                                                        //       }
                                                        ndic = new Name_DIC_Struct();
                                                        ndic.name = "RECV_SYS_OPTION";

                                                        ndic.dic.Add("opLuSms", allInfo.gsmSysOption.opLuSms);
                                                        ndic.dic.Add("opLuImei", allInfo.gsmSysOption.opLuImei);
                                                        ndic.dic.Add("opCallEn", allInfo.gsmSysOption.opCallEn);
                                                        ndic.dic.Add("opDebug", allInfo.gsmSysOption.opDebug);
                                                        ndic.dic.Add("opLuType", allInfo.gsmSysOption.opLuType);
                                                        ndic.dic.Add("opSmsType", allInfo.gsmSysOption.opSmsType);
                                                        //ndic.dic.Add("opRegModel", allInfo.gsmSysOption.opRegModel);
                                                        gApLower.Body.n_dic.Add(ndic);


                                                        //       "name":"RECV_RF_PARA",                 //4.4	射频参数
                                                        //      {
                                                        //					"rfEnable":射频使能
                                                        //					"rfFreq":信道号
                                                        //					"rfPwr":发射功率衰减值
                                                        //       }
                                                        ndic = new Name_DIC_Struct();
                                                        ndic.name = "RECV_RF_PARA";
                                                        ndic.dic.Add("rfEnable", allInfo.gsmRfPara.rfEnable);
                                                        ndic.dic.Add("rfFreq", allInfo.gsmRfPara.rfFreq);
                                                        ndic.dic.Add("rfPwr", allInfo.gsmRfPara.rfPwr);
                                                        gApLower.Body.n_dic.Add(ndic);

                                                        //       "name":"RECV_REG_MODE",            //4.33	注册工作模式
                                                        //       {
                                                        //          "regMode":模式0时由设备自行根据系统选项决定是否允许终端入网，是否对终端发送短信；
                                                        //                    模式1时设备将终端标识发送给上位机，由上位机告知设备下一步的动作
                                                        //       }
                                                        ndic = new Name_DIC_Struct();
                                                        ndic.name = "RECV_REG_MODE";

                                                        ndic.dic.Add("regMode", allInfo.gsmSysOption.opRegModel);
                                                        gApLower.Body.n_dic.Add(ndic);
                                                    }

                                                    #endregion

                                                    Send_Msg_2_ApCtrl_Lower(gApLower);

                                                    break;

                                                    #endregion
                                                }
                                                else
                                                {
                                                    #region 以AP为准

                                                    #region 告诉AP发白名单FTP服务器上

                                                    if (!whiteimsi_md5_match)
                                                    {
                                                        Fill_IMMS_Info(ref gApLower, Main2ApControllerMsgType.SetGenParaReq, 0, "成功", true, null, null);
                                                        gApLower.Body.dic.Add("ApIsBase", DataController.DataAlignMode.ToString());

                                                        string fileName = gClsDataAlign.fileNameWhiteList_Ap_Base;

                                                        string ftpUrl = string.Format("ftp://{0}:{1}@{2}:{3}/{4}/{5}",
                                                            DataController.StrFtpUserId,
                                                            DataController.StrFtpUserPsw,
                                                            DataController.StrFtpIpAddr,
                                                            DataController.StrFtpPort,
                                                            DataController.StrFtpUpdateDir,
                                                            fileName);

                                                        gApLower.Body.dic.Add("FtpUrl_White", ftpUrl);

                                                        if (!gApLower.Body.dic.ContainsKey("FtpUser"))
                                                        {
                                                            gApLower.Body.dic.Add("FtpUser", DataController.StrFtpUserId);
                                                            gApLower.Body.dic.Add("FtpPas", DataController.StrFtpUserPsw);
                                                            gAppUpper.Body.dic.Add("ftpRootDir", DataController.StrFtpUpdateDir);
                                                            gAppUpper.Body.dic.Add("ftpServerIp", DataController.StrFtpIpAddr);
                                                            gAppUpper.Body.dic.Add("ftpPort", DataController.StrFtpPort);
                                                        }
                                                    }

                                                    #endregion

                                                    #region 告诉AP发黑名单到FTP服务器上

                                                    if (!blackimsi_md5_match)
                                                    {
                                                        Fill_IMMS_Info(ref gApLower, Main2ApControllerMsgType.SetGenParaReq, 0, "成功", true, null, null);
                                                        gApLower.Body.dic.Add("ApIsBase", DataController.DataAlignMode.ToString());

                                                        string fileName = gClsDataAlign.fileNameBlackList_Ap_Base;

                                                        string ftpUrl = string.Format("ftp://{0}:{1}@{2}:{3}/{4}/{5}",
                                                            DataController.StrFtpUserId,
                                                            DataController.StrFtpUserPsw,
                                                            DataController.StrFtpIpAddr,
                                                            DataController.StrFtpPort,
                                                            DataController.StrFtpUpdateDir,
                                                            fileName);

                                                        gApLower.Body.dic.Add("FtpUrl_Black", ftpUrl);

                                                        if (!gApLower.Body.dic.ContainsKey("FtpUser"))
                                                        {
                                                            gApLower.Body.dic.Add("FtpUser", DataController.StrFtpUserId);
                                                            gApLower.Body.dic.Add("FtpPas", DataController.StrFtpUserPsw);
                                                            gAppUpper.Body.dic.Add("ftpRootDir", DataController.StrFtpUpdateDir);
                                                            gAppUpper.Body.dic.Add("ftpServerIp", DataController.StrFtpIpAddr);
                                                            gAppUpper.Body.dic.Add("ftpPort", DataController.StrFtpPort);
                                                        }
                                                    }

                                                    #endregion

                                                    #region 将通用参数更新到DB

                                                    if (!gsm_para_match)
                                                    {
                                                        rtv = 0;
                                                        if (gsmAllParaReport.gsmSysParaFlag == true)
                                                        {
                                                            rtv += gDbHelper.gsm_sys_para_record_update(carry, devInfo.id, gsmAllParaReport.gsmSysPara);
                                                        }

                                                        if (gsmAllParaReport.gsmSysOptionFlag == true)
                                                        {
                                                            rtv += gDbHelper.gsm_sys_option_record_update(carry, devInfo.id, gsmAllParaReport.gsmSysOption);
                                                        }

                                                        //if (gsmAllParaReport.gsmRfParaFlag == true)
                                                        //{
                                                        //    rtv += gDbHelper.gsm_rf_para_record_update(carry, devInfo.id, gsmAllParaReport.gsmRfPara);
                                                        //}

                                                        if (gsmAllParaReport.gsmMsgOptionFlag == true)
                                                        {
                                                            rtv += gDbHelper.gsm_msg_option_insert(carry, devInfo.id, gsmAllParaReport.gsmMsgOption);
                                                        }

                                                        if ((int)RC.SUCCESS != rtv)
                                                        {
                                                            //返回出错处理
                                                            errInfo = string.Format("对齐时gsm_sys_para_record_update等等出错.");
                                                            add_log_info(LogInfoType.EROR, errInfo, "Main", LogCategory.I);
                                                            Logger.Trace(LogInfoType.EROR, errInfo, "Main", LogCategory.I);

                                                            Fill_IMMS_Info(ref gApLower, Main2ApControllerMsgType.ReportGenParaAck, -1, errInfo, true, null, null);
                                                            Send_Msg_2_ApCtrl_Lower(gApLower);
                                                            break;
                                                        }
                                                        else
                                                        {
                                                            Fill_IMMS_Info(ref gApLower, Main2ApControllerMsgType.DataAlignOver, 0, "成功", true, null, null);
                                                            Send_Msg_2_ApCtrl_Lower(gApLower);
                                                        }
                                                    }
                                                    else
                                                    {
                                                        Fill_IMMS_Info(ref gApLower, Main2ApControllerMsgType.DataAlignOver, 0, "成功", true, null, null);
                                                        Send_Msg_2_ApCtrl_Lower(gApLower);
                                                    }

                                                    #endregion

                                                    #endregion
                                                }
                                            }

                                            #endregion

                                            break;

                                            #endregion                                            
                                        }
                                    case devMode.MODE_WCDMA:
                                    case devMode.MODE_LTE_TDD:
                                    case devMode.MODE_LTE_FDD:
                                        {
                                            #region LTE处理

                                            #region 获取所有通用参数以及串

                                            genParaFullString = "";
                                            if (gApLower.Body.dic.ContainsKey("mode"))
                                            {
                                                if (!string.IsNullOrEmpty(gApLower.Body.dic["mode"].ToString()))
                                                {
                                                    apGP.mode = gApLower.Body.dic["mode"].ToString();
                                                    genParaFullString += string.Format("[{0}]", apGP.mode);
                                                }
                                            }
                                            else
                                            {
                                                //返回出错处理
                                                string errInfo = string.Format("{0}:没包含mode字段.", Main2ApControllerMsgType.ReportGenPara);
                                                add_log_info(LogInfoType.EROR, errInfo, "Main", LogCategory.I);
                                                Logger.Trace(LogInfoType.EROR, errInfo, "Main", LogCategory.I);

                                                Fill_IMMS_Info(ref gApLower, Main2ApControllerMsgType.ReportGenParaAck, -1, errInfo, true, null, null);
                                                Send_Msg_2_ApCtrl_Lower(gApLower);
                                                break;
                                            }

                                            if (gApLower.Body.dic.ContainsKey("primaryplmn"))
                                            {
                                                if (!string.IsNullOrEmpty(gApLower.Body.dic["primaryplmn"].ToString()))
                                                {
                                                    apGP.primaryplmn = gApLower.Body.dic["primaryplmn"].ToString();
                                                    genParaFullString += string.Format("[{0}]", apGP.primaryplmn);
                                                }
                                            }
                                            else
                                            {
                                                //返回出错处理
                                                string errInfo = string.Format("{0}:没包含primaryplmn字段.", Main2ApControllerMsgType.ReportGenPara);
                                                add_log_info(LogInfoType.EROR, errInfo, "Main", LogCategory.I);
                                                Logger.Trace(LogInfoType.EROR, errInfo, "Main", LogCategory.I);

                                                Fill_IMMS_Info(ref gApLower, Main2ApControllerMsgType.ReportGenParaAck, -1, errInfo, true, null, null);
                                                Send_Msg_2_ApCtrl_Lower(gApLower);
                                                break;
                                            }

                                            if (gApLower.Body.dic.ContainsKey("earfcndl"))
                                            {
                                                if (!string.IsNullOrEmpty(gApLower.Body.dic["earfcndl"].ToString()))
                                                {
                                                    apGP.earfcndl = gApLower.Body.dic["earfcndl"].ToString();
                                                    genParaFullString += string.Format("[{0}]", apGP.earfcndl);
                                                }
                                            }
                                            else
                                            {
                                                //返回出错处理
                                                string errInfo = string.Format("{0}:没包含earfcndl字段.", Main2ApControllerMsgType.ReportGenPara);
                                                add_log_info(LogInfoType.EROR, errInfo, "Main", LogCategory.I);
                                                Logger.Trace(LogInfoType.EROR, errInfo, "Main", LogCategory.I);

                                                Fill_IMMS_Info(ref gApLower, Main2ApControllerMsgType.ReportGenParaAck, -1, errInfo, true, null, null);
                                                Send_Msg_2_ApCtrl_Lower(gApLower);
                                                break;
                                            }

                                            if (gApLower.Body.dic.ContainsKey("earfcnul"))
                                            {
                                                if (!string.IsNullOrEmpty(gApLower.Body.dic["earfcnul"].ToString()))
                                                {
                                                    apGP.earfcnul = gApLower.Body.dic["earfcnul"].ToString();
                                                    genParaFullString += string.Format("[{0}]", apGP.earfcnul);
                                                }
                                            }
                                            else
                                            {
                                                //返回出错处理
                                                string errInfo = string.Format("{0}:没包含earfcnul字段.", Main2ApControllerMsgType.ReportGenPara);
                                                add_log_info(LogInfoType.EROR, errInfo, "Main", LogCategory.I);
                                                Logger.Trace(LogInfoType.EROR, errInfo, "Main", LogCategory.I);

                                                Fill_IMMS_Info(ref gApLower, Main2ApControllerMsgType.ReportGenParaAck, -1, errInfo, true, null, null);
                                                Send_Msg_2_ApCtrl_Lower(gApLower);
                                                break;
                                            }

                                            if (gApLower.Body.dic.ContainsKey("cellid"))
                                            {
                                                if (!string.IsNullOrEmpty(gApLower.Body.dic["cellid"].ToString()))
                                                {
                                                    apGP.cellid = gApLower.Body.dic["cellid"].ToString();
                                                    genParaFullString += string.Format("[{0}]", apGP.cellid);
                                                }
                                            }
                                            else
                                            {
                                                //返回出错处理
                                                string errInfo = string.Format("{0}:没包含cellid字段.", Main2ApControllerMsgType.ReportGenPara);
                                                add_log_info(LogInfoType.EROR, errInfo, "Main", LogCategory.I);
                                                Logger.Trace(LogInfoType.EROR, errInfo, "Main", LogCategory.I);

                                                Fill_IMMS_Info(ref gApLower, Main2ApControllerMsgType.ReportGenParaAck, -1, errInfo, true, null, null);
                                                Send_Msg_2_ApCtrl_Lower(gApLower);
                                                break;
                                            }

                                            if (gApLower.Body.dic.ContainsKey("pci"))
                                            {
                                                if (!string.IsNullOrEmpty(gApLower.Body.dic["pci"].ToString()))
                                                {
                                                    apGP.pci = gApLower.Body.dic["pci"].ToString();
                                                    genParaFullString += string.Format("[{0}]", apGP.pci);
                                                }
                                            }
                                            else
                                            {
                                                //返回出错处理
                                                string errInfo = string.Format("{0}:没包含pci字段.", Main2ApControllerMsgType.ReportGenPara);
                                                add_log_info(LogInfoType.EROR, errInfo, "Main", LogCategory.I);
                                                Logger.Trace(LogInfoType.EROR, errInfo, "Main", LogCategory.I);

                                                Fill_IMMS_Info(ref gApLower, Main2ApControllerMsgType.ReportGenParaAck, -1, errInfo, true, null, null);
                                                Send_Msg_2_ApCtrl_Lower(gApLower);
                                                break;
                                            }

                                            if (gApLower.Body.dic.ContainsKey("bandwidth"))
                                            {
                                                if (!string.IsNullOrEmpty(gApLower.Body.dic["bandwidth"].ToString()))
                                                {
                                                    apGP.bandwidth = gApLower.Body.dic["bandwidth"].ToString();
                                                    genParaFullString += string.Format("[{0}]", apGP.bandwidth);
                                                }
                                            }
                                            else
                                            {
                                                //返回出错处理
                                                string errInfo = string.Format("{0}:没包含bandwidth字段.", Main2ApControllerMsgType.ReportGenPara);
                                                add_log_info(LogInfoType.EROR, errInfo, "Main", LogCategory.I);
                                                Logger.Trace(LogInfoType.EROR, errInfo, "Main", LogCategory.I);

                                                Fill_IMMS_Info(ref gApLower, Main2ApControllerMsgType.ReportGenParaAck, -1, errInfo, true, null, null);
                                                Send_Msg_2_ApCtrl_Lower(gApLower);
                                                break;
                                            }

                                            if (gApLower.Body.dic.ContainsKey("tac"))
                                            {
                                                if (!string.IsNullOrEmpty(gApLower.Body.dic["tac"].ToString()))
                                                {
                                                    apGP.tac = gApLower.Body.dic["tac"].ToString();
                                                    genParaFullString += string.Format("[{0}]", apGP.tac);
                                                }
                                            }
                                            else
                                            {
                                                //返回出错处理
                                                string errInfo = string.Format("{0}:没包含tac字段.", Main2ApControllerMsgType.ReportGenPara);
                                                add_log_info(LogInfoType.EROR, errInfo, "Main", LogCategory.I);
                                                Logger.Trace(LogInfoType.EROR, errInfo, "Main", LogCategory.I);

                                                Fill_IMMS_Info(ref gApLower, Main2ApControllerMsgType.ReportGenParaAck, -1, errInfo, true, null, null);
                                                Send_Msg_2_ApCtrl_Lower(gApLower);
                                                break;
                                            }

                                            if (gApLower.Body.dic.ContainsKey("txpower"))
                                            {
                                                if (!string.IsNullOrEmpty(gApLower.Body.dic["txpower"].ToString()))
                                                {
                                                    apGP.txpower = gApLower.Body.dic["txpower"].ToString();
                                                    genParaFullString += string.Format("[{0}]", apGP.txpower);
                                                }
                                            }
                                            else
                                            {
                                                //返回出错处理
                                                string errInfo = string.Format("{0}:没包含txpower字段.", Main2ApControllerMsgType.ReportGenPara);
                                                add_log_info(LogInfoType.EROR, errInfo, "Main", LogCategory.I);
                                                Logger.Trace(LogInfoType.EROR, errInfo, "Main", LogCategory.I);

                                                Fill_IMMS_Info(ref gApLower, Main2ApControllerMsgType.ReportGenParaAck, -1, errInfo, true, null, null);
                                                Send_Msg_2_ApCtrl_Lower(gApLower);
                                                break;
                                            }

                                            if (gApLower.Body.dic.ContainsKey("periodtac"))
                                            {
                                                if (!string.IsNullOrEmpty(gApLower.Body.dic["periodtac"].ToString()))
                                                {
                                                    apGP.periodtac = gApLower.Body.dic["periodtac"].ToString();
                                                    genParaFullString += string.Format("[{0}]", apGP.periodtac);
                                                }
                                            }
                                            else
                                            {
                                                //返回出错处理
                                                string errInfo = string.Format("{0}:没包含periodtac字段.", Main2ApControllerMsgType.ReportGenPara);
                                                add_log_info(LogInfoType.EROR, errInfo, "Main", LogCategory.I);
                                                Logger.Trace(LogInfoType.EROR, errInfo, "Main", LogCategory.I);

                                                Fill_IMMS_Info(ref gApLower, Main2ApControllerMsgType.ReportGenParaAck, -1, errInfo, true, null, null);
                                                Send_Msg_2_ApCtrl_Lower(gApLower);
                                                break;
                                            }

                                            if (gApLower.Body.dic.ContainsKey("manualfreq"))
                                            {
                                                if (!string.IsNullOrEmpty(gApLower.Body.dic["manualfreq"].ToString()))
                                                {
                                                    apGP.manualfreq = gApLower.Body.dic["manualfreq"].ToString();
                                                    genParaFullString += string.Format("[{0}]", apGP.manualfreq);
                                                }
                                            }
                                            else
                                            {
                                                //返回出错处理
                                                string errInfo = string.Format("{0}:没包含manualfreq字段.", Main2ApControllerMsgType.ReportGenPara);
                                                add_log_info(LogInfoType.EROR, errInfo, "Main", LogCategory.I);
                                                Logger.Trace(LogInfoType.EROR, errInfo, "Main", LogCategory.I);

                                                Fill_IMMS_Info(ref gApLower, Main2ApControllerMsgType.ReportGenParaAck, -1, errInfo, true, null, null);
                                                Send_Msg_2_ApCtrl_Lower(gApLower);
                                                break;
                                            }

                                            if (gApLower.Body.dic.ContainsKey("bootMode"))
                                            {
                                                if (!string.IsNullOrEmpty(gApLower.Body.dic["bootMode"].ToString()))
                                                {
                                                    apGP.bootMode = gApLower.Body.dic["bootMode"].ToString();
                                                    genParaFullString += string.Format("[{0}]", apGP.bootMode);
                                                }
                                            }
                                            else
                                            {
                                                //返回出错处理
                                                string errInfo = string.Format("{0}:没包含bootMode字段.", Main2ApControllerMsgType.ReportGenPara);
                                                add_log_info(LogInfoType.EROR, errInfo, "Main", LogCategory.I);
                                                Logger.Trace(LogInfoType.EROR, errInfo, "Main", LogCategory.I);

                                                Fill_IMMS_Info(ref gApLower, Main2ApControllerMsgType.ReportGenParaAck, -1, errInfo, true, null, null);
                                                Send_Msg_2_ApCtrl_Lower(gApLower);
                                                break;
                                            }

                                            if (gApLower.Body.dic.ContainsKey("Earfcnlist"))
                                            {
                                                if (!string.IsNullOrEmpty(gApLower.Body.dic["Earfcnlist"].ToString()))
                                                {
                                                    apGP.Earfcnlist = gApLower.Body.dic["Earfcnlist"].ToString();
                                                    genParaFullString += string.Format("[{0}]", apGP.Earfcnlist);
                                                }
                                            }
                                            else
                                            {
                                                //返回出错处理
                                                string errInfo = string.Format("{0}:没包含Earfcnlist字段.", Main2ApControllerMsgType.ReportGenPara);
                                                add_log_info(LogInfoType.EROR, errInfo, "Main", LogCategory.I);
                                                Logger.Trace(LogInfoType.EROR, errInfo, "Main", LogCategory.I);

                                                Fill_IMMS_Info(ref gApLower, Main2ApControllerMsgType.ReportGenParaAck, -1, errInfo, true, null, null);
                                                Send_Msg_2_ApCtrl_Lower(gApLower);
                                                break;
                                            }

                                            if (gApLower.Body.dic.ContainsKey("Bandoffset"))
                                            {
                                                if (!string.IsNullOrEmpty(gApLower.Body.dic["Bandoffset"].ToString()))
                                                {
                                                    apGP.Bandoffset = gApLower.Body.dic["Bandoffset"].ToString();
                                                    genParaFullString += string.Format("[{0}]", apGP.Bandoffset);
                                                }
                                            }
                                            else
                                            {
                                                //返回出错处理
                                                string errInfo = string.Format("{0}:没包含Bandoffset字段.", Main2ApControllerMsgType.ReportGenPara);
                                                add_log_info(LogInfoType.EROR, errInfo, "Main", LogCategory.I);
                                                Logger.Trace(LogInfoType.EROR, errInfo, "Main", LogCategory.I);

                                                Fill_IMMS_Info(ref gApLower, Main2ApControllerMsgType.ReportGenParaAck, -1, errInfo, true, null, null);
                                                Send_Msg_2_ApCtrl_Lower(gApLower);
                                                break;
                                            }

                                            if (gApLower.Body.dic.ContainsKey("NTP"))
                                            {
                                                if (!string.IsNullOrEmpty(gApLower.Body.dic["NTP"].ToString()))
                                                {
                                                    apGP.NTP = gApLower.Body.dic["NTP"].ToString();
                                                    genParaFullString += string.Format("[{0}]", apGP.NTP);
                                                }
                                            }
                                            else
                                            {
                                                //返回出错处理
                                                string errInfo = string.Format("{0}:没包含NTP字段.", Main2ApControllerMsgType.ReportGenPara);
                                                add_log_info(LogInfoType.EROR, errInfo, "Main", LogCategory.I);
                                                Logger.Trace(LogInfoType.EROR, errInfo, "Main", LogCategory.I);

                                                Fill_IMMS_Info(ref gApLower, Main2ApControllerMsgType.ReportGenParaAck, -1, errInfo, true, null, null);
                                                Send_Msg_2_ApCtrl_Lower(gApLower);
                                                break;
                                            }

                                            if (gApLower.Body.dic.ContainsKey("ntppri"))
                                            {
                                                if (!string.IsNullOrEmpty(gApLower.Body.dic["ntppri"].ToString()))
                                                {
                                                    apGP.ntppri = gApLower.Body.dic["ntppri"].ToString();
                                                    genParaFullString += string.Format("[{0}]", apGP.ntppri);
                                                }
                                            }
                                            else
                                            {
                                                //返回出错处理
                                                string errInfo = string.Format("{0}:没包含ntppri字段.", Main2ApControllerMsgType.ReportGenPara);
                                                add_log_info(LogInfoType.EROR, errInfo, "Main", LogCategory.I);
                                                Logger.Trace(LogInfoType.EROR, errInfo, "Main", LogCategory.I);

                                                Fill_IMMS_Info(ref gApLower, Main2ApControllerMsgType.ReportGenParaAck, -1, errInfo, true, null, null);
                                                Send_Msg_2_ApCtrl_Lower(gApLower);
                                                break;
                                            }

                                            if (gApLower.Body.dic.ContainsKey("source"))
                                            {
                                                if (!string.IsNullOrEmpty(gApLower.Body.dic["source"].ToString()))
                                                {
                                                    apGP.source = gApLower.Body.dic["source"].ToString();
                                                    genParaFullString += string.Format("[{0}]", apGP.source);
                                                }
                                            }
                                            else
                                            {
                                                //返回出错处理
                                                string errInfo = string.Format("{0}:没包含source字段.", Main2ApControllerMsgType.ReportGenPara);
                                                add_log_info(LogInfoType.EROR, errInfo, "Main", LogCategory.I);
                                                Logger.Trace(LogInfoType.EROR, errInfo, "Main", LogCategory.I);

                                                Fill_IMMS_Info(ref gApLower, Main2ApControllerMsgType.ReportGenParaAck, -1, errInfo, true, null, null);
                                                Send_Msg_2_ApCtrl_Lower(gApLower);
                                                break;
                                            }

                                            if (gApLower.Body.dic.ContainsKey("ManualEnable"))
                                            {
                                                if (!string.IsNullOrEmpty(gApLower.Body.dic["ManualEnable"].ToString()))
                                                {
                                                    apGP.ManualEnable = gApLower.Body.dic["ManualEnable"].ToString();
                                                    genParaFullString += string.Format("[{0}]", apGP.ManualEnable);
                                                }
                                            }
                                            else
                                            {
                                                //返回出错处理
                                                string errInfo = string.Format("{0}:没包含ManualEnable字段.", Main2ApControllerMsgType.ReportGenPara);
                                                add_log_info(LogInfoType.EROR, errInfo, "Main", LogCategory.I);
                                                Logger.Trace(LogInfoType.EROR, errInfo, "Main", LogCategory.I);

                                                Fill_IMMS_Info(ref gApLower, Main2ApControllerMsgType.ReportGenParaAck, -1, errInfo, true, null, null);
                                                Send_Msg_2_ApCtrl_Lower(gApLower);
                                                break;
                                            }

                                            if (gApLower.Body.dic.ContainsKey("ManualEarfcn"))
                                            {
                                                if (!string.IsNullOrEmpty(gApLower.Body.dic["ManualEarfcn"].ToString()))
                                                {
                                                    apGP.ManualEarfcn = gApLower.Body.dic["ManualEarfcn"].ToString();
                                                    genParaFullString += string.Format("[{0}]", apGP.ManualEarfcn);
                                                }
                                            }
                                            else
                                            {
                                                //返回出错处理
                                                string errInfo = string.Format("{0}:没包含ManualEarfcn字段.", Main2ApControllerMsgType.ReportGenPara);
                                                add_log_info(LogInfoType.EROR, errInfo, "Main", LogCategory.I);
                                                Logger.Trace(LogInfoType.EROR, errInfo, "Main", LogCategory.I);

                                                Fill_IMMS_Info(ref gApLower, Main2ApControllerMsgType.ReportGenParaAck, -1, errInfo, true, null, null);
                                                Send_Msg_2_ApCtrl_Lower(gApLower);
                                                break;
                                            }

                                            if (gApLower.Body.dic.ContainsKey("ManualPci"))
                                            {
                                                if (!string.IsNullOrEmpty(gApLower.Body.dic["ManualPci"].ToString()))
                                                {
                                                    apGP.ManualPci = gApLower.Body.dic["ManualPci"].ToString();
                                                    genParaFullString += string.Format("[{0}]", apGP.ManualPci);
                                                }
                                            }
                                            else
                                            {
                                                //返回出错处理
                                                string errInfo = string.Format("{0}:没包含ManualPci字段.", Main2ApControllerMsgType.ReportGenPara);
                                                add_log_info(LogInfoType.EROR, errInfo, "Main", LogCategory.I);
                                                Logger.Trace(LogInfoType.EROR, errInfo, "Main", LogCategory.I);

                                                Fill_IMMS_Info(ref gApLower, Main2ApControllerMsgType.ReportGenParaAck, -1, errInfo, true, null, null);
                                                Send_Msg_2_ApCtrl_Lower(gApLower);
                                                break;
                                            }

                                            if (gApLower.Body.dic.ContainsKey("ManualBw"))
                                            {
                                                if (!string.IsNullOrEmpty(gApLower.Body.dic["ManualBw"].ToString()))
                                                {
                                                    apGP.ManualBw = gApLower.Body.dic["ManualBw"].ToString();
                                                    genParaFullString += string.Format("[{0}]", apGP.ManualBw);
                                                }
                                            }
                                            else
                                            {
                                                //返回出错处理
                                                string errInfo = string.Format("{0}:没包含ManualBw字段.", Main2ApControllerMsgType.ReportGenPara);
                                                add_log_info(LogInfoType.EROR, errInfo, "Main", LogCategory.I);
                                                Logger.Trace(LogInfoType.EROR, errInfo, "Main", LogCategory.I);

                                                Fill_IMMS_Info(ref gApLower, Main2ApControllerMsgType.ReportGenParaAck, -1, errInfo, true, null, null);
                                                Send_Msg_2_ApCtrl_Lower(gApLower);
                                                break;
                                            }

                                            if (gApLower.Body.dic.ContainsKey("gpsConfig"))
                                            {
                                                if (!string.IsNullOrEmpty(gApLower.Body.dic["gpsConfig"].ToString()))
                                                {
                                                    apGP.gpsConfig = gApLower.Body.dic["gpsConfig"].ToString();
                                                    genParaFullString += string.Format("[{0}]", apGP.gpsConfig);
                                                }
                                            }
                                            else
                                            {
                                                //返回出错处理
                                                string errInfo = string.Format("{0}:没包含gpsConfig字段.", Main2ApControllerMsgType.ReportGenPara);
                                                add_log_info(LogInfoType.EROR, errInfo, "Main", LogCategory.I);
                                                Logger.Trace(LogInfoType.EROR, errInfo, "Main", LogCategory.I);

                                                Fill_IMMS_Info(ref gApLower, Main2ApControllerMsgType.ReportGenParaAck, -1, errInfo, true, null, null);
                                                Send_Msg_2_ApCtrl_Lower(gApLower);
                                                break;
                                            }


                                            // 2018-07-23
                                            if (gApLower.Body.dic.ContainsKey("otherplmn"))
                                            {
                                                if (!string.IsNullOrEmpty(gApLower.Body.dic["otherplmn"].ToString()))
                                                {
                                                    apGP.otherplmn = gApLower.Body.dic["otherplmn"].ToString();
                                                    genParaFullString += string.Format("[{0}]", apGP.otherplmn);
                                                }
                                            }
                                            else
                                            {
                                                //返回出错处理
                                                string errInfo = string.Format("{0}:没包含otherplmn字段.", Main2ApControllerMsgType.ReportGenPara);
                                                add_log_info(LogInfoType.EROR, errInfo, "Main", LogCategory.I);
                                                Logger.Trace(LogInfoType.EROR, errInfo, "Main", LogCategory.I);

                                                Fill_IMMS_Info(ref gApLower, Main2ApControllerMsgType.ReportGenParaAck, -1, errInfo, true, null, null);
                                                Send_Msg_2_ApCtrl_Lower(gApLower);
                                                break;
                                            }


                                            if (gApLower.Body.dic.ContainsKey("periodFreq"))
                                            {
                                                if (!string.IsNullOrEmpty(gApLower.Body.dic["periodFreq"].ToString()))
                                                {
                                                    apGP.periodFreq = gApLower.Body.dic["periodFreq"].ToString();
                                                    genParaFullString += string.Format("[{0}]", apGP.periodFreq);
                                                }
                                            }
                                            else
                                            {
                                                //返回出错处理
                                                string errInfo = string.Format("{0}:没包含periodFreq字段.", Main2ApControllerMsgType.ReportGenPara);
                                                add_log_info(LogInfoType.EROR, errInfo, "Main", LogCategory.I);
                                                Logger.Trace(LogInfoType.EROR, errInfo, "Main", LogCategory.I);

                                                Fill_IMMS_Info(ref gApLower, Main2ApControllerMsgType.ReportGenParaAck, -1, errInfo, true, null, null);
                                                Send_Msg_2_ApCtrl_Lower(gApLower);
                                                break;
                                            }


                                            #endregion

                                            #region 计算DB中的通用参数串

                                            rtv = gDbHelper.ap_general_para_string_get_by_devid(devInfo.id, ref genParaFullString_db);
                                            if (rtv != 0)
                                            {
                                                //返回出错处理
                                                string errInfo = string.Format("ap_general_para_string_get_by_devid出错.");
                                                add_log_info(LogInfoType.EROR, errInfo, "Main", LogCategory.I);
                                                Logger.Trace(LogInfoType.EROR, errInfo, "Main", LogCategory.I);

                                                Fill_IMMS_Info(ref gApLower, Main2ApControllerMsgType.ReportGenParaAck, -1, errInfo, true, null, null);
                                                Send_Msg_2_ApCtrl_Lower(gApLower);
                                                break;
                                            }

                                            if (genParaFullString_db.Equals(genParaFullString))
                                            {
                                                gen_para_match = true;
                                            }
                                            else
                                            {
                                                string errInfo = string.Format("genParaFullString={0}\ngenParaFullString_db={1}", genParaFullString, genParaFullString_db);
                                                add_log_info(LogInfoType.INFO, errInfo, "Main", LogCategory.I);
                                                Logger.Trace(LogInfoType.INFO, errInfo, "Main", LogCategory.I);
                                            }

                                            #endregion

                                            #region 计算DB中黑白名单的MD5

                                            rtv = gDbHelper.bwlist_record_md5sum_get(bwType.BWTYPE_BLACK, devInfo.id, ref listBlackImsi);
                                            if ((int)RC.SUCCESS != rtv)
                                            {
                                                //返回出错处理
                                                string errInfo = string.Format("bwlist_record_md5sum_get出错：{0}", gDbHelper.get_rtv_str(rtv));
                                                add_log_info(LogInfoType.EROR, errInfo, "Main", LogCategory.I);
                                                Logger.Trace(LogInfoType.EROR, errInfo, "Main", LogCategory.I);

                                                Fill_IMMS_Info(ref gApLower, Main2ApControllerMsgType.ReportGenParaAck, -1, errInfo, true, null, null);
                                                Send_Msg_2_ApCtrl_Lower(gApLower);
                                                break;
                                            }
                                            else
                                            {
                                                listBlackImsi.Sort();
                                            }

                                            rtv = gDbHelper.bwlist_record_md5sum_get(bwType.BWTYPE_WHITE, devInfo.id, ref listWhiteImsi);
                                            if ((int)RC.SUCCESS != rtv)
                                            {
                                                //返回出错处理
                                                string errInfo = string.Format("bwlist_record_md5sum_get出错：{0}", gDbHelper.get_rtv_str(rtv));
                                                add_log_info(LogInfoType.EROR, errInfo, "Main", LogCategory.I);
                                                Logger.Trace(LogInfoType.EROR, errInfo, "Main", LogCategory.I);

                                                Fill_IMMS_Info(ref gApLower, Main2ApControllerMsgType.ReportGenParaAck, -1, errInfo, true, null, null);
                                                Send_Msg_2_ApCtrl_Lower(gApLower);
                                                break;
                                            }
                                            else
                                            {
                                                listWhiteImsi.Sort();
                                            }

                                                                                       
                                            if (0 != Get_Md5_Sum(listBlackImsi, 1, ref blackimsi_md5_db))
                                            {
                                                //返回出错处理
                                                string errInfo = string.Format("Get_Md5_Sum出错");
                                                add_log_info(LogInfoType.EROR, errInfo, "Main", LogCategory.I);
                                                Logger.Trace(LogInfoType.EROR, errInfo, "Main", LogCategory.I);

                                                Fill_IMMS_Info(ref gApLower, Main2ApControllerMsgType.ReportGenParaAck, -1, errInfo, true, null, null);
                                                Send_Msg_2_ApCtrl_Lower(gApLower);
                                                break;
                                            }

                                            if (0 != Get_Md5_Sum(listWhiteImsi, 1, ref whiteimsi_md5_db))
                                            {
                                                //返回出错处理
                                                string errInfo = string.Format("Get_Md5_Sum出错");
                                                add_log_info(LogInfoType.EROR, errInfo, "Main", LogCategory.I);
                                                Logger.Trace(LogInfoType.EROR, errInfo, "Main", LogCategory.I);

                                                Fill_IMMS_Info(ref gApLower, Main2ApControllerMsgType.ReportGenParaAck, -1, errInfo, true, null, null);
                                                Send_Msg_2_ApCtrl_Lower(gApLower);
                                                break;
                                            }

                                            if (whiteimsi_md5.Equals(whiteimsi_md5_db))
                                            {
                                                whiteimsi_md5_match = true;
                                            }
                                            else
                                            {
                                                string errInfo = string.Format("whiteimsi_md5={0}\nwhiteimsi_md5_db={1}", whiteimsi_md5, whiteimsi_md5_db);
                                                add_log_info(LogInfoType.INFO, errInfo, "Main", LogCategory.I);
                                                Logger.Trace(LogInfoType.INFO, errInfo, "Main", LogCategory.I);
                                            }

                                            if (blackimsi_md5.Equals(blackimsi_md5_db))
                                            {
                                                blackimsi_md5_match = true;
                                            }
                                            else
                                            {
                                                string errInfo = string.Format("blackimsi_md5={0}\nblackimsi_md5_db={1}", blackimsi_md5, blackimsi_md5_db);
                                                add_log_info(LogInfoType.INFO, errInfo, "Main", LogCategory.I);
                                                Logger.Trace(LogInfoType.INFO, errInfo, "Main", LogCategory.I);
                                            }

                                            gClsDataAlign.whiteimsi_md5_match = whiteimsi_md5_match;
                                            gClsDataAlign.blackimsi_md5_match = blackimsi_md5_match;

                                            #endregion

                                            #region 回复ReportGenParaAck

                                            Fill_IMMS_Info(ref gApLower, Main2ApControllerMsgType.ReportGenParaAck, 0, "成功", true, null, null);
                                            Send_Msg_2_ApCtrl_Lower(gApLower);

                                            #endregion

                                            #region 流程处理

                                            if (gen_para_match && blackimsi_md5_match && whiteimsi_md5_match)
                                            {
                                                //数据对齐                                        
                                                Fill_IMMS_Info(ref gApLower, Main2ApControllerMsgType.DataAlignOver, 0, "成功", true, null, null);
                                                Send_Msg_2_ApCtrl_Lower(gApLower);                                         
                                            }
                                            else
                                            {
                                                //数据不对齐
                                                if (DataController.DataAlignMode == 0)
                                                {
                                                    #region 以DB为准

                                                    Fill_IMMS_Info(ref gApLower, Main2ApControllerMsgType.SetGenParaReq, 0, "成功", true, null, null);
                                                    gApLower.Body.dic.Add("ApIsBase", DataController.DataAlignMode.ToString());

                                                    #region 上传白名单到FTP

                                                    if (!whiteimsi_md5_match)
                                                    {                                                       
                                                        string fileName = gClsDataAlign.fileNameWhiteList_Db_Base;

                                                        try
                                                        {
                                                            byte[] data = null;
                                                            if (0 == generate_ftp_byte(ref data, listWhiteImsi))
                                                            {
                                                                rtv = gFtpHelperFile.Put(fileName, data);
                                                                if (rtv != 0)
                                                                {
                                                                    //返回出错处理
                                                                    string errInfo = string.Format("上传对齐文件listWhiteImsi到FTP服务器出错.");
                                                                    add_log_info(LogInfoType.EROR, errInfo, "Main", LogCategory.I);
                                                                    Logger.Trace(LogInfoType.EROR, errInfo, "Main", LogCategory.I);

                                                                    Fill_IMMS_Info(ref gApLower, Main2ApControllerMsgType.ReportGenParaAck, -1, errInfo, true, null, null);
                                                                    Send_Msg_2_ApCtrl_Lower(gApLower);
                                                                    break;
                                                                }
                                                            }
                                                        }
                                                        catch (Exception e)
                                                        {
                                                            //返回出错处理
                                                            string errInfo = string.Format("上传对齐文件listWhiteImsi到FTP服务器出错." + e.Message);
                                                            add_log_info(LogInfoType.EROR, errInfo, "Main", LogCategory.I);
                                                            Logger.Trace(LogInfoType.EROR, errInfo, "Main", LogCategory.I);

                                                            Fill_IMMS_Info(ref gApLower, Main2ApControllerMsgType.ReportGenParaAck, -1, errInfo, true, null, null);
                                                            Send_Msg_2_ApCtrl_Lower(gApLower);
                                                            break;
                                                        }                                             

                                                        string ftpUrl = string.Format("ftp://{0}:{1}@{2}:{3}/{4}/{5}",
                                                            DataController.StrFtpUserId,
                                                            DataController.StrFtpUserPsw,
                                                            DataController.StrFtpIpAddr,
                                                            DataController.StrFtpPort,
                                                            DataController.StrFtpUpdateDir,
                                                            fileName);

                                                        gApLower.Body.dic.Add("FtpUrl_White", ftpUrl);

                                                        if (!gApLower.Body.dic.ContainsKey("FtpUser"))
                                                        {
                                                            gApLower.Body.dic.Add("FtpUser", DataController.StrFtpUserId);
                                                            gApLower.Body.dic.Add("FtpPas", DataController.StrFtpUserPsw);
                                                            gApLower.Body.dic.Add("ftpRootDir", DataController.StrFtpUpdateDir);
                                                            gApLower.Body.dic.Add("ftpServerIp", DataController.StrFtpIpAddr);
                                                            gApLower.Body.dic.Add("ftpPort", DataController.StrFtpPort);
                                                        }
                                                    }

                                                    #endregion

                                                    #region 上传黑名单到FTP

                                                    if (!blackimsi_md5_match)
                                                    {                                                    
                                                        string fileName = gClsDataAlign.fileNameBlackList_Db_Base;

                                                        try
                                                        {
                                                            byte[] data = null;
                                                            if (0 == generate_ftp_byte(ref data, listBlackImsi))
                                                            {
                                                                rtv = gFtpHelperFile.Put(fileName, data);
                                                                if (rtv != 0)
                                                                {
                                                                    //返回出错处理
                                                                    string errInfo = string.Format("上传对齐文件listBlackImsi到FTP服务器出错.");
                                                                    add_log_info(LogInfoType.EROR, errInfo, "Main", LogCategory.I);
                                                                    Logger.Trace(LogInfoType.EROR, errInfo, "Main", LogCategory.I);

                                                                    Fill_IMMS_Info(ref gApLower, Main2ApControllerMsgType.ReportGenParaAck, -1, errInfo, true, null, null);
                                                                    Send_Msg_2_ApCtrl_Lower(gApLower);
                                                                    break;
                                                                }
                                                            }
                                                        }
                                                        catch (Exception e)
                                                        {
                                                            //返回出错处理
                                                            string errInfo = string.Format("上传对齐文件listBlackImsi到FTP服务器出错." + e.Message);
                                                            add_log_info(LogInfoType.EROR, errInfo, "Main", LogCategory.I);
                                                            Logger.Trace(LogInfoType.EROR, errInfo, "Main", LogCategory.I);

                                                            Fill_IMMS_Info(ref gApLower, Main2ApControllerMsgType.ReportGenParaAck, -1, errInfo, true, null, null);
                                                            Send_Msg_2_ApCtrl_Lower(gApLower);
                                                            break;
                                                        }
                                                        
                                                        string ftpUrl = string.Format("ftp://{0}:{1}@{2}:{3}/{4}/{5}",
                                                            DataController.StrFtpUserId,
                                                            DataController.StrFtpUserPsw,
                                                            DataController.StrFtpIpAddr,
                                                            DataController.StrFtpPort,
                                                            DataController.StrFtpUpdateDir,
                                                            fileName);

                                                        gApLower.Body.dic.Add("FtpUrl_Black", ftpUrl);

                                                        if (!gApLower.Body.dic.ContainsKey("FtpUser"))
                                                        {
                                                            gApLower.Body.dic.Add("FtpUser", DataController.StrFtpUserId);
                                                            gApLower.Body.dic.Add("FtpPas", DataController.StrFtpUserPsw);
                                                            gApLower.Body.dic.Add("ftpRootDir", DataController.StrFtpUpdateDir);
                                                            gApLower.Body.dic.Add("ftpServerIp", DataController.StrFtpIpAddr);
                                                            gApLower.Body.dic.Add("ftpPort", DataController.StrFtpPort);
                                                        }
                                                    }

                                                    #endregion

                                                    #region 将DB中通用参数传给AP

                                                    if (!gen_para_match)
                                                    {
                                                        rtv = gDbHelper.ap_general_para_record_get_by_devid(devInfo.id, ref apGP);
                                                        if (rtv != 0)
                                                        {
                                                            //返回出错处理
                                                            string errInfo = string.Format("ap_general_para_record_get_by_devid出错:") + gDbHelper.get_rtv_str(rtv);
                                                            add_log_info(LogInfoType.EROR, errInfo, "Main", LogCategory.I);
                                                            Logger.Trace(LogInfoType.EROR, errInfo, "Main", LogCategory.I);

                                                            Fill_IMMS_Info(ref gApLower, Main2ApControllerMsgType.ReportGenParaAck, -1, errInfo, true, null, null);
                                                            Send_Msg_2_ApCtrl_Lower(gApLower);
                                                            break;
                                                        }

                                                        if (!string.IsNullOrEmpty(apGP.mode))
                                                        {
                                                            gApLower.Body.dic.Add("mode", apGP.mode);
                                                        }

                                                        if (!string.IsNullOrEmpty(apGP.primaryplmn))
                                                        {
                                                            gApLower.Body.dic.Add("primaryplmn", apGP.primaryplmn);
                                                        }

                                                        if (!string.IsNullOrEmpty(apGP.earfcndl))
                                                        {
                                                            gApLower.Body.dic.Add("earfcndl", apGP.earfcndl);
                                                        }

                                                        if (!string.IsNullOrEmpty(apGP.earfcnul))
                                                        {
                                                            gApLower.Body.dic.Add("earfcnul", apGP.earfcnul);
                                                        }

                                                        // 2018-06-26
                                                        if (!string.IsNullOrEmpty(apGP.cellid))
                                                        {
                                                            gApLower.Body.dic.Add("cellid", apGP.cellid);
                                                        }

                                                        if (!string.IsNullOrEmpty(apGP.pci))
                                                        {
                                                            gApLower.Body.dic.Add("pci", apGP.pci);
                                                        }

                                                        if (!string.IsNullOrEmpty(apGP.bandwidth))
                                                        {
                                                            gApLower.Body.dic.Add("bandwidth", apGP.bandwidth);
                                                        }

                                                        if (!string.IsNullOrEmpty(apGP.tac))
                                                        {
                                                            gApLower.Body.dic.Add("tac", apGP.tac);
                                                        }

                                                        if (!string.IsNullOrEmpty(apGP.txpower))
                                                        {
                                                            gApLower.Body.dic.Add("txpower", apGP.txpower);
                                                        }

                                                        if (!string.IsNullOrEmpty(apGP.periodtac))
                                                        {
                                                            gApLower.Body.dic.Add("periodtac", apGP.periodtac);
                                                        }

                                                        if (!string.IsNullOrEmpty(apGP.manualfreq))
                                                        {
                                                            gApLower.Body.dic.Add("manualfreq", apGP.manualfreq);
                                                        }

                                                        if (!string.IsNullOrEmpty(apGP.bootMode))
                                                        {
                                                            gApLower.Body.dic.Add("bootMode", apGP.bootMode);
                                                        }

                                                        if (!string.IsNullOrEmpty(apGP.Earfcnlist))
                                                        {
                                                            gApLower.Body.dic.Add("Earfcnlist", apGP.Earfcnlist);
                                                        }

                                                        if (!string.IsNullOrEmpty(apGP.Bandoffset))
                                                        {
                                                            gApLower.Body.dic.Add("Bandoffset", apGP.Bandoffset);
                                                        }

                                                        if (!string.IsNullOrEmpty(apGP.NTP))
                                                        {
                                                            gApLower.Body.dic.Add("NTP", apGP.NTP);
                                                        }

                                                        if (!string.IsNullOrEmpty(apGP.ntppri))
                                                        {
                                                            gApLower.Body.dic.Add("ntppri", apGP.ntppri);
                                                        }

                                                        if (!string.IsNullOrEmpty(apGP.source))
                                                        {
                                                            gApLower.Body.dic.Add("source", apGP.source);
                                                        }

                                                        if (!string.IsNullOrEmpty(apGP.ManualEnable))
                                                        {
                                                            gApLower.Body.dic.Add("ManualEnable", apGP.ManualEnable);
                                                        }

                                                        if (!string.IsNullOrEmpty(apGP.ManualEarfcn))
                                                        {
                                                            gApLower.Body.dic.Add("ManualEarfcn", apGP.ManualEarfcn);
                                                        }

                                                        if (!string.IsNullOrEmpty(apGP.ManualPci))
                                                        {
                                                            gApLower.Body.dic.Add("ManualPci", apGP.ManualPci);
                                                        }

                                                        if (!string.IsNullOrEmpty(apGP.ManualBw))
                                                        {
                                                            gApLower.Body.dic.Add("ManualBw", apGP.ManualBw);
                                                        }

                                                        if (!string.IsNullOrEmpty(apGP.gpsConfig))
                                                        {
                                                            gApLower.Body.dic.Add("gpsConfig", apGP.gpsConfig);
                                                        }
                                                    }

                                                    #endregion

                                                    Send_Msg_2_ApCtrl_Lower(gApLower);
                                                    break;

                                                    #endregion
                                                }
                                                else
                                                {
                                                    #region 以AP为准

                                                    #region 告诉AP发白名单到FTP服务器上

                                                    if (!whiteimsi_md5_match)
                                                    {
                                                        Fill_IMMS_Info(ref gApLower, Main2ApControllerMsgType.SetGenParaReq, 0, "成功", true, null, null);
                                                        gApLower.Body.dic.Add("ApIsBase", DataController.DataAlignMode.ToString());

                                                        //告诉AP发文件到FTP服务器上
                                                        string fileName = gClsDataAlign.fileNameWhiteList_Ap_Base;

                                                        string ftpUrl = string.Format("ftp://{0}:{1}@{2}:{3}/{4}/{5}",
                                                            DataController.StrFtpUserId,
                                                            DataController.StrFtpUserPsw,
                                                            DataController.StrFtpIpAddr,
                                                            DataController.StrFtpPort,
                                                            DataController.StrFtpUpdateDir,
                                                            fileName);

                                                        gApLower.Body.dic.Add("FtpUrl_White", ftpUrl);

                                                        if (!gApLower.Body.dic.ContainsKey("FtpUser"))
                                                        {
                                                            gApLower.Body.dic.Add("FtpUser", DataController.StrFtpUserId);
                                                            gApLower.Body.dic.Add("FtpPas", DataController.StrFtpUserPsw);
                                                            gAppUpper.Body.dic.Add("ftpRootDir", DataController.StrFtpUpdateDir);
                                                            gAppUpper.Body.dic.Add("ftpServerIp", DataController.StrFtpIpAddr);
                                                            gAppUpper.Body.dic.Add("ftpPort", DataController.StrFtpPort);
                                                        }
                                                    }

                                                    #endregion

                                                    #region 告诉AP发黑名单到FTP服务器上

                                                    if (!blackimsi_md5_match)
                                                    {
                                                        Fill_IMMS_Info(ref gApLower, Main2ApControllerMsgType.SetGenParaReq, 0, "成功", true, null, null);
                                                        gApLower.Body.dic.Add("ApIsBase", DataController.DataAlignMode.ToString());

                                                        //告诉AP发文件到FTP服务器上
                                                        string fileName = gClsDataAlign.fileNameBlackList_Ap_Base;

                                                        string ftpUrl = string.Format("ftp://{0}:{1}@{2}:{3}/{4}/{5}",
                                                            DataController.StrFtpUserId,
                                                            DataController.StrFtpUserPsw,
                                                            DataController.StrFtpIpAddr,
                                                            DataController.StrFtpPort,
                                                            DataController.StrFtpUpdateDir,
                                                            fileName);

                                                        gApLower.Body.dic.Add("FtpUrl_Black", ftpUrl);

                                                        if (!gApLower.Body.dic.ContainsKey("FtpUser"))
                                                        {
                                                            gApLower.Body.dic.Add("FtpUser", DataController.StrFtpUserId);
                                                            gApLower.Body.dic.Add("FtpPas", DataController.StrFtpUserPsw);
                                                            gAppUpper.Body.dic.Add("ftpRootDir", DataController.StrFtpUpdateDir);
                                                            gAppUpper.Body.dic.Add("ftpServerIp", DataController.StrFtpIpAddr);
                                                            gAppUpper.Body.dic.Add("ftpPort", DataController.StrFtpPort);
                                                        }
                                                    }

                                                    #endregion

                                                    #region 将AP通用参数更新到DB中

                                                    if (!gen_para_match)
                                                    {
                                                        //更新数据库
                                                        rtv = gDbHelper.ap_general_para_record_update(devInfo.id, apGP);
                                                        if ((int)RC.SUCCESS != rtv)
                                                        {
                                                            //返回出错处理
                                                            string errInfo = string.Format("对齐时ap_general_para_record_update出错.");
                                                            add_log_info(LogInfoType.EROR, errInfo, "Main", LogCategory.I);
                                                            Logger.Trace(LogInfoType.EROR, errInfo, "Main", LogCategory.I);

                                                            Fill_IMMS_Info(ref gApLower, Main2ApControllerMsgType.ReportGenParaAck, -1, errInfo, true, null, null);
                                                            Send_Msg_2_ApCtrl_Lower(gApLower);
                                                            break;
                                                        }
                                                        else
                                                        {
                                                            Fill_IMMS_Info(ref gApLower, Main2ApControllerMsgType.DataAlignOver, 0, "成功", true, null, null);
                                                            Send_Msg_2_ApCtrl_Lower(gApLower);
                                                        }
                                                    }
                                                    else
                                                    {                                                        
                                                        Fill_IMMS_Info(ref gApLower, Main2ApControllerMsgType.DataAlignOver, 0, "成功", true, null, null);
                                                        Send_Msg_2_ApCtrl_Lower(gApLower);
                                                    }

                                                    #endregion

                                                    #endregion
                                                }
                                            }

                                            #endregion

                                            break;

                                            #endregion
                                        }
                                    default:
                                        {
                                            #region 出错处理

                                            //返回出错处理
                                            string errInfo = string.Format("不支持的apInnerType:{0}.", dm);
                                            add_log_info(LogInfoType.EROR, errInfo, "Main", LogCategory.I);
                                            Logger.Trace(LogInfoType.EROR, errInfo, "Main", LogCategory.I);

                                            Fill_IMMS_Info(ref gApLower, Main2ApControllerMsgType.ReportGenParaAck, -1, errInfo, true, null, null);
                                            Send_Msg_2_ApCtrl_Lower(gApLower);
                                            break;

                                            #endregion
                                        }
                                }

                                #endregion
                            }                            
                                  
                            break;

                            #endregion
                        }
                        case Main2ApControllerMsgType.SetGenParaRsp:
                        {
                            #region 获取信息

                            int rtv = -1;
                            int ReturnCode = 0;

                            if (gApLower.Body.dic.ContainsKey("ReturnCode"))
                            {
                                ReturnCode = int.Parse(gApLower.Body.dic["ReturnCode"].ToString());
                            }
                            else
                            {
                                string errInfo = string.Format("SetGenParaRsp没包含ReturnCode字段");                                 
                                Fill_IMMS_Info(ref gApLower, Main2ApControllerMsgType.DataAlignOver, -1, errInfo, true, null, null);
                                Send_Msg_2_ApCtrl_Lower(gApLower);
                            }

                            if (ReturnCode != 0)
                            {
                                string errInfo = string.Format("SetGenParaRsp返回失败.");
                                Fill_IMMS_Info(ref gApLower, Main2ApControllerMsgType.DataAlignOver, -1, errInfo, true, null, null);
                                Send_Msg_2_ApCtrl_Lower(gApLower);
                            }

                            #endregion

                            #region 返回处理

                            //数据不对齐
                            if (DataController.DataAlignMode == 0)
                            {
                                #region 以DB为准

                                Fill_IMMS_Info(ref gApLower, Main2ApControllerMsgType.DataAlignOver, 0, "成功", true, null, null);
                                Send_Msg_2_ApCtrl_Lower(gApLower);

                                #endregion
                            }
                            else
                            {
                                #region 以AP为准

                                #region 白名单处理

                                if (gClsDataAlign.whiteimsi_md5_match == false)
                                {
                                    //从FTP服务器上下载文件
                                    rtv = gFtpHelperFile.Get(gClsDataAlign.fileNameWhiteList_Ap_Base, Application.StartupPath, gClsDataAlign.fileNameWhiteList_Ap_Base);
                                    if (rtv != 0)
                                    {
                                        string errInfo = string.Format("从FTP服务器下载文件:{0}失败.",gClsDataAlign.fileNameWhiteList_Ap_Base);
                                        add_log_info(LogInfoType.EROR, errInfo, "Main", LogCategory.I);
                                        Logger.Trace(LogInfoType.EROR, errInfo, "Main", LogCategory.I);

                                        Fill_IMMS_Info(ref gApLower, Main2ApControllerMsgType.DataAlignOver, -1, errInfo, true, null, null);
                                        Send_Msg_2_ApCtrl_Lower(gApLower);
                                        break;
                                    }

                                    string info = "";
                                    List<strBwList> list = new List<strBwList>();
                                    string fileFullPath = string.Format("{0}\\{1}", Application.StartupPath, gClsDataAlign.fileNameWhiteList_Ap_Base);

                                    //更新到数据库中
                                    rtv = Get_BwList_From_File(fileFullPath,bwType.BWTYPE_WHITE, ref list, ref info);
                                    if (rtv != 0)
                                    {
                                        string errInfo = string.Format("解析文件:{0}失败.", fileFullPath);
                                        add_log_info(LogInfoType.EROR, errInfo, "Main", LogCategory.I);
                                        Logger.Trace(LogInfoType.EROR, errInfo, "Main", LogCategory.I);

                                        Fill_IMMS_Info(ref gApLower, Main2ApControllerMsgType.DataAlignOver, -1, errInfo, true, null, null);
                                        Send_Msg_2_ApCtrl_Lower(gApLower);
                                        break;
                                    }

                                    rtv = gDbHelper.bwlist_record_bwflag_delete(bwType.BWTYPE_WHITE, gClsDataAlign.devId);
                                    if (rtv != 0)
                                    {
                                        string errInfo = string.Format("bwlist_record_bwflag_delete失败.");
                                        add_log_info(LogInfoType.EROR, errInfo, "Main", LogCategory.I);
                                        Logger.Trace(LogInfoType.EROR, errInfo, "Main", LogCategory.I);

                                        Fill_IMMS_Info(ref gApLower, Main2ApControllerMsgType.DataAlignOver, -1, errInfo, true, null, null);
                                        Send_Msg_2_ApCtrl_Lower(gApLower);
                                        break;
                                    }

                                    rtv = gDbHelper.bwlist_record_insert_batch(list, gClsDataAlign.devId);
                                    if (rtv != 0)
                                    {
                                        string errInfo = string.Format("bwlist_record_insert_batch失败.");
                                        add_log_info(LogInfoType.EROR, errInfo, "Main", LogCategory.I);
                                        Logger.Trace(LogInfoType.EROR, errInfo, "Main", LogCategory.I);

                                        Fill_IMMS_Info(ref gApLower, Main2ApControllerMsgType.DataAlignOver, -1, errInfo, true, null, null);
                                        Send_Msg_2_ApCtrl_Lower(gApLower);
                                        break;
                                    }                                   
                                }


                                #endregion

                                #region 黑名单处理

                                if (gClsDataAlign.blackimsi_md5_match == false)
                                {
                                    //从FTP服务器上下载文件
                                    rtv = gFtpHelperFile.Get(gClsDataAlign.fileNameBlackList_Ap_Base, Application.StartupPath, gClsDataAlign.fileNameBlackList_Ap_Base);
                                    if (rtv != 0)
                                    {
                                        string errInfo = string.Format("从FTP服务器下载文件:{0}失败.", gClsDataAlign.fileNameBlackList_Ap_Base);
                                        add_log_info(LogInfoType.EROR, errInfo, "Main", LogCategory.I);
                                        Logger.Trace(LogInfoType.EROR, errInfo, "Main", LogCategory.I);

                                        Fill_IMMS_Info(ref gApLower, Main2ApControllerMsgType.DataAlignOver, -1, errInfo, true, null, null);
                                        Send_Msg_2_ApCtrl_Lower(gApLower);
                                        break;
                                    }

                                    string info = "";
                                    List<strBwList> list = new List<strBwList>();
                                    string fileFullPath = string.Format("{0}\\{1}", Application.StartupPath, gClsDataAlign.fileNameBlackList_Ap_Base);

                                    //更新到数据库中
                                    rtv = Get_BwList_From_File(fileFullPath, bwType.BWTYPE_BLACK, ref list, ref info);
                                    if (rtv != 0)
                                    {
                                        string errInfo = string.Format("解析文件:{0}失败.", fileFullPath);
                                        add_log_info(LogInfoType.EROR, errInfo, "Main", LogCategory.I);
                                        Logger.Trace(LogInfoType.EROR, errInfo, "Main", LogCategory.I);

                                        Fill_IMMS_Info(ref gApLower, Main2ApControllerMsgType.DataAlignOver, -1, errInfo, true, null, null);
                                        Send_Msg_2_ApCtrl_Lower(gApLower);
                                        break;
                                    }

                                    rtv = gDbHelper.bwlist_record_bwflag_delete(bwType.BWTYPE_BLACK, gClsDataAlign.devId);
                                    if (rtv != 0)
                                    {
                                        string errInfo = string.Format("bwlist_record_bwflag_delete失败.");
                                        add_log_info(LogInfoType.EROR, errInfo, "Main", LogCategory.I);
                                        Logger.Trace(LogInfoType.EROR, errInfo, "Main", LogCategory.I);

                                        Fill_IMMS_Info(ref gApLower, Main2ApControllerMsgType.DataAlignOver, -1, errInfo, true, null, null);
                                        Send_Msg_2_ApCtrl_Lower(gApLower);
                                        break;
                                    }

                                    rtv = gDbHelper.bwlist_record_insert_batch(list, gClsDataAlign.devId);
                                    if (rtv != 0)
                                    {
                                        string errInfo = string.Format("bwlist_record_insert_batch失败.");
                                        add_log_info(LogInfoType.EROR, errInfo, "Main", LogCategory.I);
                                        Logger.Trace(LogInfoType.EROR, errInfo, "Main", LogCategory.I);

                                        Fill_IMMS_Info(ref gApLower, Main2ApControllerMsgType.DataAlignOver, -1, errInfo, true, null, null);
                                        Send_Msg_2_ApCtrl_Lower(gApLower);
                                        break;
                                    }                                   
                                }

                                #endregion

                                Fill_IMMS_Info(ref gApLower, Main2ApControllerMsgType.DataAlignOver, rtv,gDbHelper.get_rtv_str(rtv), true, null, null);
                                Send_Msg_2_ApCtrl_Lower(gApLower);

                                #endregion
                            }

                            break;

                            #endregion
                        }
                    case ApMsgType.Update_result:
                        {
                            #region 转发结果

                            //修改消息type
                            gApLower.Body.type = AppMsgType.app_ftp_update_response;

                            //透传消息给AppController
                            Send_Msg_2_AppCtrl_Upper(gApLower);

                            break;

                            #endregion
                        }
                    case AppMsgType.app_add_bwlist_response:
                        {
                            #region 获取信息

                            strDevice devInfo = new strDevice();
                            if (string.IsNullOrEmpty(gApLower.ApInfo.Fullname))
                            {
                                string errInfo = get_debug_info() + "Fullname is NULL.";
                                add_log_info(LogInfoType.EROR, errInfo, "Main", LogCategory.I);
                                Logger.Trace(LogInfoType.EROR, errInfo, "Main", LogCategory.I);

                                Fill_IMMS_Info(ref gApLower, AppMsgType.app_add_bwlist_response, -1, errInfo, true, "1", "2");
                                Send_Msg_2_ApCtrl_Lower(gApLower);
                                break;
                            }
                            else
                            {                           
                                if (!gDicDeviceId.ContainsKey(gApLower.ApInfo.Fullname))
                                {
                                    string errInfo = get_debug_info() + string.Format("{0}:对应的设备ID在gDicDeviceId中找不到", gApLower.ApInfo.Fullname);
                                    add_log_info(LogInfoType.EROR, errInfo, "Main", LogCategory.I);
                                    Logger.Trace(LogInfoType.EROR, errInfo, "Main", LogCategory.I);

                                    Fill_IMMS_Info(ref gApLower, AppMsgType.app_add_bwlist_response, -1, errInfo, true, "1", "2");
                                    Send_Msg_2_ApCtrl_Lower(gApLower);
                                    break;
                                }
                                else
                                {
                                    devInfo = gDicDeviceId[gApLower.ApInfo.Fullname];
                                }
                            }


                            /*
                             *  在自学习的状态下，AP在自己添加白名单后，会持续发
                             *  app_add_bwlist_response给到上层这里，从而会
                             *  造成异常的流程处理，因此，在自学习的状态下，要忽略
                             *  这条消息。                             
                             */
                            if (devInfo.wSelfStudy.Equals("1"))
                            {
                                string errInfo = string.Format("{0}:正在自学习，忽略消息:{1}", gApLower.ApInfo.Fullname, AppMsgType.app_add_bwlist_response);
                                add_log_info(LogInfoType.INFO, errInfo, "Main", LogCategory.I);
                                Logger.Trace(LogInfoType.INFO, errInfo, "Main", LogCategory.I);
                                break;
                            }

                            #endregion

                            #region 透传消息给AppCtrl

                            //转给界面
                            Send_Msg_2_AppCtrl_Upper(gApLower);

                            #endregion

                            #region 根据结果决定是否更新库

                            if (gTimerBlackWhite.TimeOutFlag == false)
                            {
                                //计时器尚未超时，关闭先
                                gTimerBlackWhite.Stop();

                                #region 根据结果决定是否更新库

                                int rtv = 0;                               
                                string result = "";
                                
                                if (gApLower.Body.dic.ContainsKey("result"))
                                {
                                    if (!string.IsNullOrEmpty(gApLower.Body.dic["result"].ToString()))
                                    {
                                        result = gApLower.Body.dic["result"].ToString();
                                    }
                                }

                                if (result.Equals("0"))
                                {                                   
                                    // 保存到库中
                                    for (int i = 0; i < gBwListSetInfo.listBwInfo.Count; i++)
                                    {
                                        rtv = gDbHelper.bwlist_record_insert(gBwListSetInfo.listBwInfo[i], devInfo.id);
                                    }

                                    gBwListSetInfo.listBwInfo = new List<strBwList>();
                                }                                

                                break;

                                #endregion
                            }
                            else
                            {
                                //已经在gTimerBlackWhite超时中反馈给Ap了。
                                //不再处理
                                break;
                            }                     

                            #endregion                             
                        }
                    case AppMsgType.app_del_bwlist_response:
                        {
                            #region 转发给AppController

                            //转给界面
                            Send_Msg_2_AppCtrl_Upper(gApLower);

                            #endregion

                            #region 根据结果决定是否更新库

                            if (gTimerBlackWhite.TimeOutFlag == false)
                            {
                                //计时器尚未超时，关闭先
                                gTimerBlackWhite.Stop();

                                #region 根据结果决定是否更新库

                                int rtv = 0;
                                string Fullname = "";
                                string result = "";

                                if (string.IsNullOrEmpty(gApLower.ApInfo.Fullname))
                                {
                                    string errInfo = string.Format("Fullname is NULL:{0}", AppMsgType.app_del_bwlist_response);
                                    add_log_info(LogInfoType.EROR, errInfo, "Main", LogCategory.I);
                                    Logger.Trace(LogInfoType.EROR, errInfo, "Main", LogCategory.I);

                                    Fill_IMMS_Info(ref gApLower, AppMsgType.app_del_bwlist_response, -1, errInfo, true, "1", "2");
                                    Send_Msg_2_ApCtrl_Lower(gApLower);
                                    break;
                                }
                                else
                                {
                                    Fullname = gApLower.ApInfo.Fullname;
                                }

                                if (gApLower.Body.dic.ContainsKey("result"))
                                {
                                    if (!string.IsNullOrEmpty(gApLower.Body.dic["result"].ToString()))
                                    {
                                        result = gApLower.Body.dic["result"].ToString();
                                    }
                                }

                                if (!gDicDeviceId.ContainsKey(Fullname))
                                {
                                    string errInfo = get_debug_info() + string.Format("{0}:对应的设备ID在gDicDeviceId中找不到", Fullname);
                                    add_log_info(LogInfoType.EROR, errInfo, "Main", LogCategory.I);
                                    Logger.Trace(LogInfoType.EROR, errInfo, "Main", LogCategory.I);

                                    Fill_IMMS_Info(ref gApLower, AppMsgType.app_del_bwlist_response, -1, errInfo, true, "1", "2");
                                    Send_Msg_2_ApCtrl_Lower(gApLower);
                                    break;
                                }
                                else
                                {
                                    strDevice devInfo = gDicDeviceId[Fullname];
                                    if (result.Equals("0"))
                                    {
                                        //保存到库中
                                        for (int i = 0; i < gBwListSetInfo.listBwInfo.Count; i++)
                                        {
                                            if (!string.IsNullOrEmpty(gBwListSetInfo.listBwInfo[i].imsi))
                                            {
                                                rtv = gDbHelper.bwlist_record_imsi_delete(gBwListSetInfo.listBwInfo[i].imsi, gBwListSetInfo.listBwInfo[i].bwFlag, devInfo.id);
                                            }
                                            else
                                            {
                                                rtv = gDbHelper.bwlist_record_imei_delete(gBwListSetInfo.listBwInfo[i].imei, gBwListSetInfo.listBwInfo[i].bwFlag, devInfo.id);
                                            }
                                        }

                                        gBwListSetInfo.listBwInfo = new List<strBwList>();
                                    }
                                }

                                break;

                                #endregion
                            }
                            else
                            {
                                //已经在gTimerBlackWhite超时中反馈给Ap了。
                                //不再处理
                                break;
                            }

                            #endregion                            
                        }
                    case AppMsgType.set_param_response:
                        {
                            #region 获取信息

                            string result = "";
                            string paramName = "";

                            if (gApLower.Body.dic.ContainsKey("paramName"))
                            {
                                if (!string.IsNullOrEmpty(gApLower.Body.dic["paramName"].ToString()))
                                {
                                    paramName = gApLower.Body.dic["paramName"].ToString();                                    
                                }
                            }

                            if (gApLower.Body.dic.ContainsKey("result"))
                            {
                                if (!string.IsNullOrEmpty(gApLower.Body.dic["result"].ToString()))
                                {
                                    result = gApLower.Body.dic["result"].ToString();
                                }
                            }

                            #endregion

                            #region 返回信息

                            if (paramName == "")
                            {
                                //透传消息给AppCtrlUpper
                                Send_Msg_2_AppCtrl_Upper(gApLower);
                                break;
                            }
                            else
                            {
                                if (paramName != "CFG_FULL_NAME")
                                {
                                    //透传消息给AppCtrlUpper
                                    Send_Msg_2_AppCtrl_Upper(gApLower);
                                    break;
                                }
                                else
                                {
                                    #region 处理设置全名                         
                                   
                                    if (gTimerSetFullName.TimeOutFlag == false && !string.IsNullOrEmpty(gTimerSetFullName.parentFullPathName))
                                    {
                                        #region 计时器尚未超时

                                        gTimerSetFullName.Stop();

                                        //修改消息type
                                        gApLower.Body.type = AppMsgType.app_add_device_response;
                                       
                                        if (result.Equals("0"))
                                        {
                                            int affDomainId = -1;
                                            int rtv = gDbHelper.domain_get_id_by_nameFullPath(gTimerSetFullName.parentFullPathName, ref affDomainId);
                                            if (rtv == 0)
                                            {
                                                rtv = gDbHelper.device_record_insert(affDomainId, gTimerSetFullName.devName, gTimerSetFullName.mode);
                                            }
                                            
                                            gApLower.Body.dic = new Dictionary<string, object>();
                                            gApLower.Body.dic.Add("ReturnCode", rtv);
                                            gApLower.Body.dic.Add("ReturnStr", gDbHelper.get_rtv_str(rtv));

                                            //添加设备成功
                                            Send_Msg_2_AppCtrl_Upper(gApLower);

                                            #region 重新获取gDicDeviceId

                                            if (rtv == 0)
                                            {
                                                if (0 == gDbHelper.domain_dictionary_info_join_get(ref gDicDeviceId))
                                                {
                                                    add_log_info(LogInfoType.INFO, "gDicDeviceId -> 获取OK！", "Main", LogCategory.I);
                                                    Logger.Trace(LogInfoType.INFO, "gDicDeviceId -> 获取OK！", "Main", LogCategory.I);
                                                }
                                                else
                                                {
                                                    add_log_info(LogInfoType.INFO, "gDicDeviceId -> 获取FAILED！", "Main", LogCategory.I);
                                                    Logger.Trace(LogInfoType.INFO, "gDicDeviceId -> 获取FAILED！", "Main", LogCategory.I);
                                                }
                                            }

                                            #endregion

                                            if ((int)RC.NO_EXIST == gDbHelper.device_unknown_record_exist(gTimerSetFullName.ipAddr, int.Parse(gTimerSetFullName.port)))
                                            {
                                                //设备(未指派)记录不存在
                                                break;
                                            }

                                            //更新新记录
                                            rtv = gDbHelper.device_unknown_record_delete(gTimerSetFullName.ipAddr, int.Parse(gTimerSetFullName.port));
                                            if (rtv == 0)
                                            {
                                                #region 重新获取未指派设备

                                                //2018-06-26
                                                DataTable dt = new DataTable();
                                                rtv = gDbHelper.device_unknown_record_entity_get(ref dt);

                                                gAppUpper.Body.type = Main2ApControllerMsgType.app_all_device_response;
                                                gAppUpper.Body.dic = new Dictionary<string, object>();
                                                gAppUpper.Body.dic.Add("ReturnCode", rtv);
                                                gAppUpper.Body.dic.Add("ReturnStr", gDbHelper.get_rtv_str(rtv));
                                                gAppUpper.Body.n_dic = new List<Name_DIC_Struct>();

                                                if (rtv == 0)
                                                {
                                                    if (dt.Rows.Count > 0)
                                                    {
                                                        gAppUpper.Body.n_dic = new List<Name_DIC_Struct>();
                                                        set_device_unknown_info_by_datatable(dt, ref gAppUpper);
                                                    }
                                                }

                                                string info = string.Format("发送app_all_device_response给AppCtrl,未指派个数{0}", dt.Rows.Count);
                                                Logger.Trace(LogInfoType.DEBG, info, "Main", LogCategory.S);

                                                //发送给APP去更新未指派设备信息
                                                Send_Msg_2_AppCtrl_Upper(gAppUpper);

                                                #endregion
                                            }
                                        }
                                        else
                                        {
                                            Send_Msg_2_AppCtrl_Upper(gApLower);
                                        }

                                        #endregion 
                                    }
                                    else
                                    {
                                        #region 计时器已经超时

                                        //已经在gTimerSetFullName超时中反馈给Ap了。
                                        //不再处理
                                        break;

                                        #endregion
                                    }

                                    break;

                                    #endregion
                                }
                            }

                            #endregion
                        }
                    case "gsm_para_change":
                        {
                            #region 获取信息

                            int rtv = 0;
                            string Fullname = "";
                            str_Gsm_All_Para gsmAllPara = new str_Gsm_All_Para();

                            if (string.IsNullOrEmpty(gApLower.ApInfo.Fullname))
                            {
                                //返回出错处理
                                gApLower.Body.type = "Main2ApControllerMsgType.gsm_para_change_ack";
                                gApLower.Body.dic = new Dictionary<string, object>();
                                gApLower.Body.dic.Add("ReturnCode", -1);
                                gApLower.Body.dic.Add("ReturnStr", get_debug_info() + "Fullname is NULL.");

                                Send_Msg_2_ApCtrl_Lower(gApLower);
                                break;
                            }
                            else
                            {
                                Fullname = gApLower.ApInfo.Fullname;
                            }

                            if (get_gsm_info(gApLower, ref gsmAllPara) != 0)
                            {
                                //返回出错处理
                                gApLower.Body.type = "Main2ApControllerMsgType.gsm_para_change_ack";
                                gApLower.Body.dic = new Dictionary<string, object>();
                                gApLower.Body.dic.Add("ReturnCode", -1);
                                gApLower.Body.dic.Add("ReturnStr", get_debug_info() + "获取GSM相关的参数出错.");

                                Send_Msg_2_ApCtrl_Lower(gApLower);
                                break;
                            }

                            //string ipAddr = gApLower.ApInfo.IP;
                            //string name = "";               

                            //if (gApLower.ApInfo.Fullname.Contains("."))
                            //{
                            //    int i = gApLower.ApInfo.Fullname.LastIndexOf(".");
                            //    name = gApLower.ApInfo.Fullname.Substring(i + 1);                               
                            //}

                            // Fullname = Fullname + "." + gsmAllPara.sys.ToString();

                            int carry = gsmAllPara.sys;

                            #endregion

                            #region 更新数据库

                            if (!gDicDeviceId.ContainsKey(Fullname))
                            {
                                string info = get_debug_info() + string.Format("{0}:对应的设备ID在gDicDeviceId中找不到", Fullname);

                                add_log_info(LogInfoType.EROR, info, "Main", LogCategory.R);
                                Logger.Trace(LogInfoType.EROR, info, "Main", LogCategory.R);

                                gApLower.Body.type = "Main2ApControllerMsgType.gsm_para_change_ack";
                                gApLower.Body.dic = new Dictionary<string, object>();
                                gApLower.Body.dic.Add("ReturnCode", -1);
                                gApLower.Body.dic.Add("ReturnStr", info);

                                Send_Msg_2_ApCtrl_Lower(gApLower);
                                break;
                            }
                            else
                            {
                                strDevice devInfo = gDicDeviceId[Fullname];
                                rtv = 0;

                                if (gsmAllPara.gsmSysParaFlag == true)
                                {
                                    rtv += gDbHelper.gsm_sys_para_record_update(carry, devInfo.id, gsmAllPara.gsmSysPara);
                                }

                                if (gsmAllPara.gsmSysOptionFlag == true)
                                {
                                    rtv += gDbHelper.gsm_sys_option_record_update(carry, devInfo.id, gsmAllPara.gsmSysOption);
                                }

                                if (gsmAllPara.gsmRfParaFlag == true)
                                {
                                    rtv += gDbHelper.gsm_rf_para_record_update(carry, devInfo.id, gsmAllPara.gsmRfPara);
                                }

                                if (gsmAllPara.gsmMsgOptionFlag == true)
                                {                                 
                                    rtv += gDbHelper.gsm_msg_option_insert(carry, devInfo.id, gsmAllPara.gsmMsgOption);
                                }
                            }                                                                                         

                            gApLower.Body.type = "Main2ApControllerMsgType.gsm_para_change_ack";
                            gApLower.Body.dic = new Dictionary<string, object>();
                            gApLower.Body.dic.Add("ReturnCode", rtv);
                            if (rtv != 0)
                            {
                                gApLower.Body.dic.Add("ReturnStr", get_debug_info() + gDbHelper.get_rtv_str(rtv));
                            }
                            else
                            {
                                gApLower.Body.dic.Add("ReturnStr", gDbHelper.get_rtv_str(rtv));
                            }

                            Send_Msg_2_ApCtrl_Lower(gApLower);                       

                            break;

                            #endregion                       
                        }
                    case AppMsgType.set_redirection_rsp:
                        {
                            #region 转发给AppController
                            
                            if (gTimerSetRedirection.TimeOutFlag == false)
                            {
                                //计时器尚未超时，关闭先
                                gTimerSetRedirection.Stop();

                                //修改消息type
                                gApLower.Body.type = AppMsgType.app_set_redirection_response;

                                //透传消息给AppController
                                Send_Msg_2_AppCtrl_Upper(gApLower);                                
                            }
                            else
                            {
                                //已经在gTimerSetRedirection超时中反馈给Ap了。
                                //不在处理
                                break;
                            }

                            #endregion          

                            #region 根据结果决定是否更新库
                     
                            string Fullname = "";
                            string result = "";

                            if (string.IsNullOrEmpty(gApLower.ApInfo.Fullname))
                            {                                
                                break;
                            }
                            else
                            {
                                Fullname = gApLower.ApInfo.Fullname;
                            }

                            if (gApLower.Body.dic.ContainsKey("result"))
                            {
                                if (!string.IsNullOrEmpty(gApLower.Body.dic["result"].ToString()))
                                {
                                    result = gApLower.Body.dic["result"].ToString();
                                }
                            }

                            if (!gDicDeviceId.ContainsKey(Fullname))
                            {                                
                                break;
                            }
                            else
                            {
                                strDevice devInfo = gDicDeviceId[Fullname];

                                if (result.Equals("0"))
                                {
                                    //保存到库中
                                    if ((int)RC.EXIST == gDbHelper.redirection_record_exist(int.Parse(gRedirectionInfo.category), devInfo.id))
                                    {
                                        //记录存在，只是更新
                                        gDbHelper.redirection_record_update(int.Parse(gRedirectionInfo.category), devInfo.id, gRedirectionInfo);
                                    }
                                    else
                                    {
                                        //记录不存在，先插入，再更新
                                        gDbHelper.redirection_record_insert(int.Parse(gRedirectionInfo.category), devInfo.id);
                                        gDbHelper.redirection_record_update(int.Parse(gRedirectionInfo.category), devInfo.id, gRedirectionInfo);
                                    }                                                                       
                                }
                            }

                            break;

                            #endregion                                                                   
                        }                    
                    default:
                        {
                            #region 透传给AppCtrlUpper

                            string info = string.Format("透传给AppCtrlUpper的消息:{0}", gApLower.Body.type);
                            add_log_info(LogInfoType.INFO, info, "Main", LogCategory.S);
                            Logger.Trace(LogInfoType.INFO, info, "Main", LogCategory.S);

                            //透传消息给AppCtrlUpper
                            Send_Msg_2_AppCtrl_Upper(gApLower);                                                

                            break;
                           
                            #endregion
                        }
                }
            }
            catch (Exception ee)
            {
                add_log_info(LogInfoType.EROR, ee.Message, "Main", LogCategory.I);
                Logger.Trace(LogInfoType.EROR, ee.Message, "Main", LogCategory.I);
                return -1;
            }

            return rv;
        }

        #endregion

        #region 用于接收【界面】的消息线程
    
        /// <summary>
        /// 填充IMMS信息
        /// </summary>
        /// <param name="imms"></param>
        /// <param name="type"></param>
        /// <param name="rtvCode"></param>
        /// <param name="rtvStr"></param>
        /// <param name="ndicClear"></param>
        /// <param name="result"></param>
        /// <param name="rebootflag"></param>
        private void Fill_IMMS_Info(ref InterModuleMsgStruct imms, string type, int rtvCode, string rtvStr, bool ndicClear,string result,string rebootflag)
        {
            imms.Body.type = type;
            imms.Body.dic = new Dictionary<string, object>();
            imms.Body.dic.Add("ReturnCode", rtvCode);
            imms.Body.dic.Add("ReturnStr", rtvStr);

            // result:"0",      // 0:SUCCESS ; 1:GENERAL FAILURE;2:CONFIGURATION FAIURE OR NOT SUPPORTED
            // rebootflag:"1",	// 1—立刻reboot,2—需要reboot
            // timestamp:"xxx"  // Time in seconds when send this message, start from 00:00:00 UTC 1

            if (null != result)
            {
                imms.Body.dic.Add("result", result);
            }

            if (null != rebootflag)
            {
                imms.Body.dic.Add("rebootflag", rebootflag);
                imms.Body.dic.Add("timestamp", DateTime.Now.ToString());
            }            

            if (ndicClear)
            {
                imms.Body.n_dic = new List<Name_DIC_Struct>();
            }
        }

        private string Get_App_Info(InterModuleMsgStruct imms)
        {
            if (!string.IsNullOrEmpty(imms.AppInfo.Ip) && (imms.AppInfo.Port> 0))
            {
                return string.Format("{0}:{1}", imms.AppInfo.Ip, imms.AppInfo.Port);
            }
            else
            {
                return "";
            }
        }

        /// <summary>
        /// 用于从AppController中收消息
        /// </summary>
        /// <param name="mt"></param>
        /// <param name="mb"></param>
        public static void MessageDelegate_For_AppController(MessageType mt, MessageBody mb)
        {
            strMsgInfo msgInfo;
            msgInfo.mt = mt;
            msgInfo.mb = mb;

            //string tmp = string.Format("【收到AppCtrl的消息:{0}】:\n{1}\n",  gMsgFor_App_Controller.Count, mb.bJson);
            //Logger.Trace(LogInfoType.EROR, tmp, "Main", LogCategory.R);

            lock (mutex_App_Controller)
            {
                gMsgFor_App_Controller.Enqueue(msgInfo);
            }
        }

        /// <summary>
        /// 发送消息给AppController--Upper
        /// </summary>
        /// <param name="app"></param>
        private void Send_Msg_2_AppCtrl_Upper(InterModuleMsgStruct app)
        {
            strMsgInfo msgInfo = new strMsgInfo();

            msgInfo.mt = MessageType.MSG_JSON;
            msgInfo.mb.bJson = JsonConvert.SerializeObject(app);

            add_log_info(LogInfoType.DEBG, "Main->AppCtrl:" + msgInfo.mb.bJson, "Main", LogCategory.S);
            Logger.Trace(LogInfoType.DEBG, "Main->AppCtrl:" + msgInfo.mb.bJson, "Main", LogCategory.S);


            #region 检查Type，2018-08-01

            AppInnerType appInnerType;
            if ((!app.AppInfo.Ip.Equals(MsgStruct.AllDevice))
                        && (!Enum.TryParse(app.AppInfo.Type, true, out appInnerType)))
            {
                string errInfo = string.Format("app.ApInfo.Type = {0},错误的类型.", app.ApInfo.Type);
                app.Body.type = AppMsgType.general_error_result;

                app.Body.dic = new Dictionary<string, object>();
                app.Body.dic.Add("RecvType", app.Body.type);
                app.Body.dic.Add("ErrStr", errInfo);

                add_log_info(LogInfoType.EROR, errInfo, "Main", LogCategory.S);
                Logger.Trace(LogInfoType.EROR, errInfo, "Main", LogCategory.S);

                //Send_Msg_2_ApCtrl_Lower(app);              
                return;
            }

            #endregion

            //将消息转发给ApController
            Delegate_SendMsg_2_AppCtrl_Upper(msgInfo.mt, msgInfo.mb);
        }        

        /// <summary>
        /// 用于接收AppController的消息线程
        /// </summary>
        /// <param name="obj"></param>
        private void thread_for_app_controller(object obj)
        {
            bool noMsg = false;
            strMsgInfo msgInfo;

            while (true)
            {
                if (noMsg)
                {
                    Thread.Sleep(100);
                }
                else
                {
                   //Thread.Sleep(1);
                }

                lock (mutex_App_Controller)
                {                    
                    if (gMsgFor_App_Controller.Count <= 0)
                    {
                        noMsg = true;
                        continue;
                    }

                    //string tmp = string.Format("【线程中处理AppCtrl的消息:{0}】\n", gMsgFor_App_Controller.Count);
                    //Logger.Trace(LogInfoType.EROR, tmp, "Main", LogCategory.R);

                    //循环处理从AppController接收到的消息
                    msgInfo = gMsgFor_App_Controller.Dequeue();
                }

                noMsg = false;
                switch (msgInfo.mt)
                {
                    case MessageType.MSG_STRING:
                        {
                            add_log_info(LogInfoType.DEBG, "recv from app controller，MSG_STRING", "Main", LogCategory.R);
                            Logger.Trace(LogInfoType.DEBG, "recv from app controller，MSG_STRING", "Main", LogCategory.R);

                            break;
                        }
                    case MessageType.MSG_INT:
                        {
                            add_log_info(LogInfoType.DEBG, "recv from app controller，MSG_INT", "Main", LogCategory.R);
                            Logger.Trace(LogInfoType.DEBG, "recv from app controller，MSG_INT", "Main", LogCategory.R);

                            break;
                        }
                    case MessageType.MSG_DOUBLE:
                        {
                            add_log_info(LogInfoType.DEBG, "recv from app controller，MSG_DOUBLE", "Main", LogCategory.R);
                            Logger.Trace(LogInfoType.DEBG, "recv from app controller，MSG_DOUBLE", "Main", LogCategory.R);
                            break;
                        }
                    case MessageType.MSG_DATATABLE:
                        {
                            add_log_info(LogInfoType.DEBG, "recv from app controller，MSG_DATATABLE", "Main", LogCategory.R);
                            Logger.Trace(LogInfoType.DEBG, "recv from app controller，MSG_DATATABLE", "Main", LogCategory.R);

                            break;
                        }
                    case MessageType.MSG_XML:
                        {
                            add_log_info(LogInfoType.DEBG, "recv from app controller，MSG_XML", "Main", LogCategory.R);
                            Logger.Trace(LogInfoType.DEBG, "recv from app controller，MSG_XML", "Main", LogCategory.R);

                            break;
                        }
                    case MessageType.MSG_STATUS:
                        {
                            add_log_info(LogInfoType.DEBG, "recv from app controller，MSG_STATUS", "Main", LogCategory.R);
                            Logger.Trace(LogInfoType.DEBG, "recv from app controller，MSG_STATUS", "Main", LogCategory.R);

                            break;
                        }
                    case MessageType.MSG_JSON:
                        {
                            add_log_info(LogInfoType.DEBG, "RecvFromUpper:" + msgInfo.mb.bJson, "Main", LogCategory.R);
                            Logger.Trace(LogInfoType.DEBG, "RecvFromUpper:" + msgInfo.mb.bJson, "Main", LogCategory.R);
                            process_app_controller_msg(msgInfo.mb.bJson);

                            break;
                        }
                    default:
                        {
                            add_log_info(LogInfoType.WARN, "recv from app controller，MSG_ERROR", "Main", LogCategory.R);
                            Logger.Trace(LogInfoType.WARN, "recv from app controller，MSG_ERROR", "Main", LogCategory.R);
                            break;
                        }
                }
            }
        }

        /// <summary>
        /// 检查ID集合的合法性，返回分离后的list
        /// </summary>
        /// <param name="idSet">2:50</param>
        /// <param name="curPageInx">当前页指向</param>
        /// <param name="totalPages">总页数</param>
        /// <returns>
        /// true  ： 合法
        /// false ： 非法
        /// </returns>
        private bool check_and_get_page_info(string idSet,ref int curPageInx,ref int totalPages)
        {
            if (string.IsNullOrEmpty(idSet))
            {
                Logger.Trace(LogInfoType.EROR, "idSet参数为空", "Main", LogCategory.I);
                return false;
            }

            if (idSet.Length > 16)
            {
                Logger.Trace(LogInfoType.EROR, "idSet参数长度有误", "Main", LogCategory.I);
                return false;
            }

            curPageInx = -1;
            totalPages = -1;

            string[] s = idSet.Split(new char[] { ':' });

            if (s.Length <= 0)
            {
                return false;
            }
            else
            {
                if( s.Length < 2)
                {
                    return false;
                }           

                try
                {
                    curPageInx = UInt16.Parse(s[0]);
                    totalPages = UInt16.Parse(s[1]);
                }
                catch (Exception ee)
                {
                    Logger.Trace(LogInfoType.EROR, ee.Message, "Main", LogCategory.I);
                    return false;
                }
            }

            if (curPageInx <= 0)
            {
                return false;
            }

            if(totalPages >= curPageInx)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool IsValidFileName(string fileName)
        {
            System.Diagnostics.Debug.Assert(fileName != null);
            bool isValid = true;
            try
            {
                FileInfo fileInfor = new FileInfo(fileName);
                if (fileInfor.Name != fileName)
                {
                    isValid = false;
                }
            }
            catch (ArgumentException ee)
            {         
                Logger.Trace(LogInfoType.EROR, ee.Message, "FTP", LogCategory.I);
                isValid = false;
            }
            catch (PathTooLongException ee)
            {
                Logger.Trace(LogInfoType.EROR, ee.Message, "FTP", LogCategory.I);
                isValid = false;
            }
            catch (NotSupportedException ee)
            {
                Logger.Trace(LogInfoType.EROR, ee.Message, "FTP", LogCategory.I);
                isValid = false;
            }
            catch (Exception ee)
            {
                Logger.Trace(LogInfoType.EROR, ee.Message, "FTP", LogCategory.I);
                isValid = true;
            }

            return isValid;
        }

        private void history_record_process_delegate_fun(InterModuleMsgStruct imms)
        {
            #region 获取信息

            string appId = Get_App_Info(imms);

            //   "bwListApplyTo":"device",                                    //历史记录搜索适用于那种类型，device，domain或者none
            //   "deviceFullPathName":"设备.深圳.福田.中心广场.西北监控.电信TDD",  //bwListApplyTo为device时起作用
            //   "domainFullPathName":"设备.深圳.福田",                         //bwListApplyTo为domain时起作用
            //   "imsi":"46000xxxxxxxxx",                                     //指定要搜索的IMSI号，不指定是为""
            //   "imei":"46000xxxxxxxxx",                                     //指定要搜索的IMEI号，不指定是为""
            //   "bwFlag":"设备.深圳.福田",                                     //black,white,other,不指定是为""
            //   "timeStart":"2018-05-23 12:34:56",                           //开始时间，不指定是为""
            //   "timeEnded":"2018-05-29 12:34:56",                           //结束时间，不指定是为""
            //   "RmDupFlag":"0",                                             //是否对设备名称和SN去重标志，0:不去重，1:去重

            int rtv = -1;
            strDevice devInfo = new strDevice();

            string bwListApplyTo = "";
            string deviceFullPathName = "";
            string domainFullPathName = "";
            string imsi = "";
            string imei = "";
            string bwFlag = "";
            string timeStart = "";
            string timeEnded = "";
            string RmDupFlag = "";

            strCaptureQuery cq = new strCaptureQuery();

            if (imms.Body.dic.ContainsKey("imsi"))
            {
                if (!string.IsNullOrEmpty(imms.Body.dic["imsi"].ToString()))
                {
                    imsi = imms.Body.dic["imsi"].ToString();
                    cq.imsi = imsi;
                }
            }

            if (imms.Body.dic.ContainsKey("imei"))
            {
                if (!string.IsNullOrEmpty(imms.Body.dic["imei"].ToString()))
                {
                    imei = imms.Body.dic["imei"].ToString();
                    cq.imei = imei;
                }
            }

            if (imms.Body.dic.ContainsKey("bwFlag"))
            {
                if (!string.IsNullOrEmpty(imms.Body.dic["bwFlag"].ToString()))
                {
                    bwFlag = imms.Body.dic["bwFlag"].ToString();
                    if (bwFlag != "black" && bwFlag != "white" && bwFlag != "other")
                    {
                        string errInfo = get_debug_info() + string.Format("bwFlag的类型不对");
                        add_log_info(LogInfoType.EROR, errInfo, "Main", LogCategory.I);
                        Logger.Trace(LogInfoType.EROR, errInfo, "Main", LogCategory.I);

                        Fill_IMMS_Info(ref imms, AppMsgType.app_history_record_response, -1, errInfo, true, null, null);
                        Send_Msg_2_AppCtrl_Upper(imms);
                        return;
                    }

                    if (bwFlag == "black")
                    {
                        cq.bwFlag = bwType.BWTYPE_BLACK;
                    }
                    else if (bwFlag == "white")
                    {
                        cq.bwFlag = bwType.BWTYPE_WHITE;
                    }
                    else
                    {
                        cq.bwFlag = bwType.BWTYPE_OTHER;
                    }
                }
                else
                {
                    cq.bwFlag = bwType.BWTYPE_ALL;
                }
            }

            if (imms.Body.dic.ContainsKey("timeStart"))
            {
                if (!string.IsNullOrEmpty(imms.Body.dic["timeStart"].ToString()))
                {
                    timeStart = imms.Body.dic["timeStart"].ToString();
                    try
                    {
                        DateTime.Parse(timeStart);
                        cq.timeStart = timeStart;
                    }
                    catch
                    {
                        string errInfo = get_debug_info() + string.Format("timeStart的格式不对");
                        add_log_info(LogInfoType.EROR, errInfo, "Main", LogCategory.I);
                        Logger.Trace(LogInfoType.EROR, errInfo, "Main", LogCategory.I);

                        Fill_IMMS_Info(ref imms, AppMsgType.app_history_record_response, -1, errInfo, true, null, null);
                        Send_Msg_2_AppCtrl_Upper(imms);
                        return;
                    }
                }
                else
                {
                    cq.timeStart = "1900-01-01 12:34:56";
                }
            }
            else
            {
                cq.timeStart = "1900-01-01 12:34:56";
            }

            if (imms.Body.dic.ContainsKey("timeEnded"))
            {
                if (!string.IsNullOrEmpty(imms.Body.dic["timeEnded"].ToString()))
                {
                    timeEnded = imms.Body.dic["timeEnded"].ToString();
                    try
                    {
                        DateTime.Parse(timeEnded);
                        cq.timeEnded = timeEnded;

                        if (string.Compare(cq.timeStart, cq.timeEnded) > 0)
                        {
                            string errInfo = get_debug_info() + string.Format("timeStart大于timeEnded.");
                            add_log_info(LogInfoType.EROR, errInfo, "Main", LogCategory.I);
                            Logger.Trace(LogInfoType.EROR, errInfo, "Main", LogCategory.I);

                            Fill_IMMS_Info(ref imms, AppMsgType.app_history_record_response, -1, errInfo, true, null, null);
                            Send_Msg_2_AppCtrl_Upper(imms);
                            return;
                        }
                    }
                    catch
                    {
                        string errInfo = get_debug_info() + string.Format("timeEnded的格式不对");
                        add_log_info(LogInfoType.EROR, errInfo, "Main", LogCategory.I);
                        Logger.Trace(LogInfoType.EROR, errInfo, "Main", LogCategory.I);

                        Fill_IMMS_Info(ref imms, AppMsgType.app_history_record_response, -1, errInfo, true, null, null);
                        Send_Msg_2_AppCtrl_Upper(imms);
                        return;
                    }
                }
                else
                {
                    cq.timeEnded = "2918-06-05 12:34:56";
                }
            }
            else
            {
                cq.timeEnded = "2918-06-05 12:34:56";
            }

            if (imms.Body.dic.ContainsKey("RmDupFlag"))
            {
                if (!string.IsNullOrEmpty(imms.Body.dic["RmDupFlag"].ToString()))
                {
                    RmDupFlag = imms.Body.dic["RmDupFlag"].ToString();
                    if (RmDupFlag != "0" && RmDupFlag != "1")
                    {
                        string errInfo = get_debug_info() + string.Format("RmDupFlag的类型不对");
                        add_log_info(LogInfoType.EROR, errInfo, "Main", LogCategory.I);
                        Logger.Trace(LogInfoType.EROR, errInfo, "Main", LogCategory.I);

                        Fill_IMMS_Info(ref imms, AppMsgType.app_history_record_response, -1, errInfo, true, null, null);
                        Send_Msg_2_AppCtrl_Upper(imms);
                        return;
                    }
                    cq.RmDupFlag = int.Parse(RmDupFlag);
                }
                else
                {
                    RmDupFlag = "0";
                }
            }

            if (imms.Body.dic.ContainsKey("bwListApplyTo"))
            {
                bwListApplyTo = imms.Body.dic["bwListApplyTo"].ToString();
            }

            if (imms.Body.dic.ContainsKey("deviceFullPathName"))
            {
                deviceFullPathName = imms.Body.dic["deviceFullPathName"].ToString();
            }

            if (imms.Body.dic.ContainsKey("domainFullPathName"))
            {
                domainFullPathName = imms.Body.dic["domainFullPathName"].ToString();
            }

            if (bwListApplyTo == "device")
            {
                if (!gDicDeviceId.ContainsKey(deviceFullPathName))
                {
                    string errInfo = get_debug_info() + string.Format("{0}:对应的设备ID在gDicDeviceId中找不到", deviceFullPathName);
                    add_log_info(LogInfoType.EROR, errInfo, "Main", LogCategory.I);
                    Logger.Trace(LogInfoType.EROR, errInfo, "Main", LogCategory.I);

                    Fill_IMMS_Info(ref imms, AppMsgType.app_all_bwlist_response, -1, errInfo, true, null, null);
                    Send_Msg_2_AppCtrl_Upper(imms);
                    return;
                }
                else
                {
                    devInfo = gDicDeviceId[deviceFullPathName];
                    cq.affDeviceId = devInfo.id;
                }
            }
            else if (bwListApplyTo == "domain")
            {
                if ((int)RC.NO_EXIST == gDbHelper.domain_record_exist(domainFullPathName))
                {
                    string errInfo = get_debug_info() + domainFullPathName + ":记录不存在";
                    add_log_info(LogInfoType.EROR, errInfo, "Main", LogCategory.I);
                    Logger.Trace(LogInfoType.EROR, errInfo, "Main", LogCategory.I);

                    Fill_IMMS_Info(ref imms, AppMsgType.app_all_bwlist_response, -1, errInfo, true, null, null);
                    Send_Msg_2_AppCtrl_Upper(imms);
                    return;
                }
            }
            else if (bwListApplyTo == "none")
            {
                //
                cq.affDeviceId = -1;
            }
            else
            {
                string errInfo = get_debug_info() + "bwListApplyTo必须为device,domain或者none.";
                add_log_info(LogInfoType.EROR, errInfo, "Main", LogCategory.I);
                Logger.Trace(LogInfoType.EROR, errInfo, "Main", LogCategory.I);

                Fill_IMMS_Info(ref imms, AppMsgType.app_all_bwlist_response, -1, errInfo, true, null, null);
                Send_Msg_2_AppCtrl_Upper(imms);
                return;
            }


            #endregion

            #region 返回消息

            if (string.IsNullOrEmpty(appId))
            {
                string errInfo = get_debug_info() + "获取AppInfo的IP和Port失败.";
                add_log_info(LogInfoType.EROR, errInfo, "Main", LogCategory.I);
                Logger.Trace(LogInfoType.EROR, errInfo, "Main", LogCategory.I);

                Fill_IMMS_Info(ref imms, AppMsgType.app_history_record_response, -1, errInfo, true, null, null);
                Send_Msg_2_AppCtrl_Upper(imms);
                return;
            }

            if (bwListApplyTo == "device" || bwListApplyTo == "none")
            {
                #region 设备的处理 

                if (!gDicCaptureQueryInfo.ContainsKey(appId))
                {
                    strCaptureQueryInfo qi = new strCaptureQueryInfo();
                    qi.dt = new DataTable();

                    rtv = gDbHelper.capture_record_entity_query(ref qi.dt, cq);
                    if (rtv == 0)
                    {
                        qi.totalRecords = qi.dt.Rows.Count;
                        qi.totalPages = (int)Math.Ceiling((double)qi.dt.Rows.Count / DataController.RecordsOfPageSize);
                        qi.pageSize = DataController.RecordsOfPageSize;
                    }

                    //添加App对应的黑白名单查询条件和结果
                    gDicCaptureQueryInfo.Add(appId, qi);
                }
                else
                {
                    gDicCaptureQueryInfo.Remove(appId);

                    strCaptureQueryInfo qi = new strCaptureQueryInfo();
                    qi.dt = new DataTable();

                    //每次都从库中取吧，因为再次查询时，库已经发生变化了。
                    rtv = gDbHelper.capture_record_entity_query(ref qi.dt, cq);
                    if (rtv == 0)
                    {
                        qi.totalRecords = qi.dt.Rows.Count;
                        qi.totalPages = (int)Math.Ceiling((double)qi.dt.Rows.Count / DataController.RecordsOfPageSize);
                        qi.pageSize = DataController.RecordsOfPageSize;
                    }

                    //添加App对应的黑白名单查询条件和结果
                    gDicCaptureQueryInfo.Add(appId, qi);
                }

                if (rtv != (int)RC.SUCCESS)
                {
                    Fill_IMMS_Info(ref imms, AppMsgType.app_history_record_response, rtv, gDbHelper.get_rtv_str(rtv), true, null, null);
                    Send_Msg_2_AppCtrl_Upper(imms);
                    return;
                }
                else
                {
                    string pageInfo = "";
                    if (gDicCaptureQueryInfo[appId].dt.Rows.Count == 0)
                    {
                        pageInfo = "0:0";
                    }
                    else
                    {
                        pageInfo = string.Format("1:{0}", Math.Ceiling((double)gDicCaptureQueryInfo[appId].dt.Rows.Count / DataController.RecordsOfPageSize));
                    }

                    int firstPageSize = 0;
                    if (gDicCaptureQueryInfo[appId].dt.Rows.Count > DataController.RecordsOfPageSize)
                    {
                        firstPageSize = DataController.RecordsOfPageSize;
                    }
                    else
                    {
                        firstPageSize = gDicCaptureQueryInfo[appId].dt.Rows.Count;
                    }

                    #region 取出各条记录

                    //         "imsi":"46000123456788",       //imsi
                    //         "imei":"46000123456789",       //imei
                    //         "name":"电信TDD",               //名称
                    //         "tmsi":"xxxxxx",               //TMSI
                    //         "time":"2018-05-24 18:58:50",  //时间信息
                    //         "bwFlag":"black",              //黑白名单类型，black,white或者other
                    //         "sn":"EN16110123456789",       //SN号    

                    Fill_IMMS_Info(ref imms, AppMsgType.app_history_record_response, rtv, gDbHelper.get_rtv_str(rtv), true, null, null);
                    imms.Body.dic.Add("TotalRecords", gDicCaptureQueryInfo[appId].totalRecords.ToString());
                    imms.Body.dic.Add("CurPageIndex", pageInfo);
                    imms.Body.dic.Add("PageSize", gDicCaptureQueryInfo[appId].pageSize.ToString());

                    for (int inx = 0; inx < firstPageSize; inx++)
                    {
                        DataRow dr = gDicCaptureQueryInfo[appId].dt.Rows[inx];

                        Name_DIC_Struct ndic = new Name_DIC_Struct();
                        ndic.name = (inx + 1).ToString();

                        if (string.IsNullOrEmpty(dr["imsi"].ToString()))
                        {
                            ndic.dic.Add("imsi", "null");
                        }
                        else
                        {
                            ndic.dic.Add("imsi", dr["imsi"].ToString());
                        }

                        if (string.IsNullOrEmpty(dr["imei"].ToString()))
                        {
                            ndic.dic.Add("imei", "null");
                        }
                        else
                        {
                            ndic.dic.Add("imei", dr["imei"].ToString());
                        }


                        if (string.IsNullOrEmpty(dr["name"].ToString()))
                        {
                            ndic.dic.Add("name", "null");
                        }
                        else
                        {
                            ndic.dic.Add("name", dr["name"].ToString());
                        }

                        if (string.IsNullOrEmpty(dr["tmsi"].ToString()))
                        {
                            ndic.dic.Add("tmsi", "null");
                        }
                        else
                        {
                            ndic.dic.Add("tmsi", dr["tmsi"].ToString());
                        }

                        if (string.IsNullOrEmpty(dr["bsPwr"].ToString()))
                        {
                            ndic.dic.Add("bsPwr", "null");
                        }
                        else
                        {
                            ndic.dic.Add("bsPwr", dr["bsPwr"].ToString());
                        }


                        if (string.IsNullOrEmpty(dr["time"].ToString()))
                        {
                            ndic.dic.Add("time", "null");
                        }
                        else
                        {
                            ndic.dic.Add("time", dr["time"].ToString());
                        }

                        if (string.IsNullOrEmpty(dr["bwFlag"].ToString()))
                        {
                            ndic.dic.Add("bwFlag", "bwFlag");
                        }
                        else
                        {
                            ndic.dic.Add("bwFlag", dr["bwFlag"].ToString());
                        }

                        if (string.IsNullOrEmpty(dr["sn"].ToString()))
                        {
                            ndic.dic.Add("sn", "sn");
                        }
                        else
                        {
                            ndic.dic.Add("sn", dr["sn"].ToString());
                        }

                        imms.Body.n_dic.Add(ndic);
                    }

                    #endregion
                }

                #endregion
            }
            else if (bwListApplyTo == "domain")
            {
                #region 域的处理

                //历史记录关联到域                                

                //获取一个节点下所有站点下所有设备Id的列表
                rtv = gDbHelper.domain_record_device_id_list_get(domainFullPathName, ref cq.listAffDeviceId);
                if (rtv != (int)RC.SUCCESS)
                {
                    Fill_IMMS_Info(ref imms, AppMsgType.app_history_record_response, rtv, gDbHelper.get_rtv_str(rtv), true, null, null);
                    Send_Msg_2_AppCtrl_Upper(imms);
                    return;
                }

                if (!gDicCaptureQueryInfo.ContainsKey(appId))
                {
                    strCaptureQueryInfo qi = new strCaptureQueryInfo();
                    qi.dt = new DataTable();

                    rtv = gDbHelper.capture_record_entity_query(ref qi.dt, cq);
                    if (rtv == 0)
                    {
                        qi.totalRecords = qi.dt.Rows.Count;
                        qi.totalPages = (int)Math.Ceiling((double)qi.dt.Rows.Count / DataController.RecordsOfPageSize);
                        qi.pageSize = DataController.RecordsOfPageSize;
                    }

                    //添加App对应的黑白名单查询条件和结果
                    gDicCaptureQueryInfo.Add(appId, qi);
                }
                else
                {
                    gDicCaptureQueryInfo.Remove(appId);

                    strCaptureQueryInfo qi = new strCaptureQueryInfo();
                    qi.dt = new DataTable();

                    //每次都从库中取吧，因为再次查询时，库可能已经发生变化了。
                    rtv = gDbHelper.capture_record_entity_query(ref qi.dt, cq);
                    if (rtv == 0)
                    {
                        qi.totalRecords = qi.dt.Rows.Count;
                        qi.totalPages = (int)Math.Ceiling((double)qi.dt.Rows.Count / DataController.RecordsOfPageSize);
                        qi.pageSize = DataController.RecordsOfPageSize;
                    }

                    //添加App对应的黑白名单查询条件和结果
                    gDicCaptureQueryInfo.Add(appId, qi);
                }

                if (rtv != (int)RC.SUCCESS)
                {
                    Fill_IMMS_Info(ref imms, AppMsgType.app_history_record_response, rtv, gDbHelper.get_rtv_str(rtv), true, null, null);
                    Send_Msg_2_AppCtrl_Upper(imms);
                    return;
                }
                else
                {
                    string pageInfo = "";
                    if (gDicCaptureQueryInfo[appId].dt.Rows.Count == 0)
                    {
                        pageInfo = "0:0";
                    }
                    else
                    {
                        pageInfo = string.Format("1:{0}", Math.Ceiling((double)gDicCaptureQueryInfo[appId].dt.Rows.Count / DataController.RecordsOfPageSize));
                    }

                    int firstPageSize = 0;
                    if (gDicCaptureQueryInfo[appId].dt.Rows.Count > DataController.RecordsOfPageSize)
                    {
                        firstPageSize = DataController.RecordsOfPageSize;
                    }
                    else
                    {
                        firstPageSize = gDicCaptureQueryInfo[appId].dt.Rows.Count;
                    }

                    #region 取出各条记录

                    //         "imsi":"46000123456788",       //imsi
                    //         "imei":"46000123456789",       //imei
                    //         "name":"电信TDD",               //名称
                    //         "time":"2018-05-24 18:58:50",  //时间信息
                    //         "bwFlag":"black",              //黑白名单类型，black,white或者other
                    //         "sn":"EN16110123456789",       //SN号    

                    Fill_IMMS_Info(ref imms, AppMsgType.app_history_record_response, rtv, gDbHelper.get_rtv_str(rtv), true, null, null);
                    imms.Body.dic.Add("TotalRecords", gDicCaptureQueryInfo[appId].totalRecords.ToString());
                    imms.Body.dic.Add("CurPageIndex", pageInfo);
                    imms.Body.dic.Add("PageSize", gDicCaptureQueryInfo[appId].pageSize.ToString());

                    for (int inx = 0; inx < firstPageSize; inx++)
                    {
                        DataRow dr = gDicCaptureQueryInfo[appId].dt.Rows[inx];

                        Name_DIC_Struct ndic = new Name_DIC_Struct();
                        ndic.name = (inx + 1).ToString();

                        if (string.IsNullOrEmpty(dr["imsi"].ToString()))
                        {
                            ndic.dic.Add("imsi", "null");
                        }
                        else
                        {
                            ndic.dic.Add("imsi", dr["imsi"].ToString());
                        }

                        if (string.IsNullOrEmpty(dr["imei"].ToString()))
                        {
                            ndic.dic.Add("imei", "null");
                        }
                        else
                        {
                            ndic.dic.Add("imei", dr["imei"].ToString());
                        }

                        if (string.IsNullOrEmpty(dr["name"].ToString()))
                        {
                            ndic.dic.Add("name", "null");
                        }
                        else
                        {
                            ndic.dic.Add("name", dr["name"].ToString());
                        }


                        if (string.IsNullOrEmpty(dr["tmsi"].ToString()))
                        {
                            ndic.dic.Add("tmsi", "null");
                        }
                        else
                        {
                            ndic.dic.Add("tmsi", dr["tmsi"].ToString());
                        }

                        if (string.IsNullOrEmpty(dr["bsPwr"].ToString()))
                        {
                            ndic.dic.Add("bsPwr", "null");
                        }
                        else
                        {
                            ndic.dic.Add("bsPwr", dr["bsPwr"].ToString());
                        }

                        if (string.IsNullOrEmpty(dr["time"].ToString()))
                        {
                            ndic.dic.Add("time", "null");
                        }
                        else
                        {
                            ndic.dic.Add("time", dr["time"].ToString());
                        }

                        if (string.IsNullOrEmpty(dr["bwFlag"].ToString()))
                        {
                            ndic.dic.Add("bwFlag", "bwFlag");
                        }
                        else
                        {
                            ndic.dic.Add("bwFlag", dr["bwFlag"].ToString());
                        }

                        if (string.IsNullOrEmpty(dr["sn"].ToString()))
                        {
                            ndic.dic.Add("sn", "sn");
                        }
                        else
                        {
                            ndic.dic.Add("sn", dr["sn"].ToString());
                        }

                        imms.Body.n_dic.Add(ndic);
                    }

                    #endregion
                }

                #endregion
            }

            Send_Msg_2_AppCtrl_Upper(imms);
            return;

            #endregion
        }

        private int get_bwlist_info(InterModuleMsgStruct app, ref List<strBwList> list,string affDeviceId,string affDomainId)
        {
            if (app.Body.n_dic.Count <= 0)
            {
                return -1;
            }

            list = new List<strBwList>();

            for (int i = 0; i < app.Body.n_dic.Count; i++)
            {
                strBwList bw = new strBwList();

                if (app.Body.n_dic[i].dic.ContainsKey("imsi"))
                {
                    if (app.Body.n_dic[i].dic["imsi"].ToString() != "")
                    {
                        bw.imsi = app.Body.n_dic[i].dic["imsi"].ToString();
                    }
                }

                if (app.Body.n_dic[i].dic.ContainsKey("imei"))
                {
                    if (app.Body.n_dic[i].dic["imei"].ToString() != "")
                    {
                        bw.imei = app.Body.n_dic[i].dic["imei"].ToString();
                    }
                }

                if (string.IsNullOrEmpty(bw.imsi) && string.IsNullOrEmpty(bw.imsi))
                {
                    string errInfo = "imsi和imei都为空";
                    add_log_info(LogInfoType.EROR, errInfo, "Main", LogCategory.I);
                    Logger.Trace(LogInfoType.EROR, errInfo, "Main", LogCategory.I);
                    continue;
                }

                if (app.Body.n_dic[i].dic.ContainsKey("rbStart"))
                {
                    if (app.Body.n_dic[i].dic["rbStart"].ToString() != "")
                    {
                        bw.rbStart = app.Body.n_dic[i].dic["rbStart"].ToString();
                    }
                }

                if (app.Body.n_dic[i].dic.ContainsKey("rbEnd"))
                {
                    if (app.Body.n_dic[i].dic["rbEnd"].ToString() != "")
                    {
                        bw.rbEnd = app.Body.n_dic[i].dic["rbEnd"].ToString();
                    }
                }

                if (app.Body.n_dic[i].dic.ContainsKey("bwFlag"))
                {
                    if (app.Body.n_dic[i].dic["bwFlag"].ToString() != "")
                    {
                        string tmp = app.Body.n_dic[i].dic["bwFlag"].ToString();

                        if (tmp == "black")
                        {
                            bw.bwFlag = bwType.BWTYPE_BLACK;
                        }
                        else if (tmp == "white")
                        {
                            bw.bwFlag = bwType.BWTYPE_WHITE;
                        }
                        else if (tmp == "other")
                        {
                            bw.bwFlag = bwType.BWTYPE_OTHER;
                        }
                        else
                        {
                            string errInfo = "不支持的黑白名单类型";
                            add_log_info(LogInfoType.EROR, errInfo, "Main", LogCategory.I);
                            Logger.Trace(LogInfoType.EROR, errInfo, "Main", LogCategory.I);
                            continue;
                        }
                    }
                }

                if (app.Body.n_dic[i].dic.ContainsKey("time"))
                {
                    if (app.Body.n_dic[i].dic["time"].ToString() != "")
                    {
                        bw.time = app.Body.n_dic[i].dic["time"].ToString();
                    }
                }

                if (app.Body.n_dic[i].dic.ContainsKey("des"))
                {
                    if (app.Body.n_dic[i].dic["des"].ToString() != "")
                    {
                        bw.des = app.Body.n_dic[i].dic["des"].ToString();
                    }
                }

                if (!string.IsNullOrEmpty(affDeviceId))
                {
                    bw.linkFlag = "0";
                    bw.affDeviceId = affDeviceId;
                }
                else
                {
                    if (!string.IsNullOrEmpty(affDomainId))
                    {
                        bw.linkFlag = "1";
                        bw.affDomainId = affDomainId;
                    }
                }

                list.Add(bw);
            }

            return 0;
        }

        private int change_domain_id_2_nameFullPath(string idSet,ref string nameFullPathSet)
        {
            if (string.IsNullOrEmpty(idSet))
            {
                return -1;
            }

            List<string> listStr = new List<string>();
            if (false == gDbHelper.check_and_get_id_set(idSet, ref listStr))
            {
                add_log_info(LogInfoType.EROR, "check_and_get_id_set出错", "Main", LogCategory.I);
                Logger.Trace(LogInfoType.EROR, "check_and_get_id_set出错", "Main", LogCategory.I);
                return -1;
            }

            Dictionary<string, string> dic = new Dictionary<string, string>();
            if ((int)RC.SUCCESS != gDbHelper.domain_dictionary_id_nameFullPath_get(ref dic))
            {
                add_log_info(LogInfoType.EROR, "domain_dictionary_id_nameFullPath_get出错", "Main", LogCategory.I);
                Logger.Trace(LogInfoType.EROR, "domain_dictionary_id_nameFullPath_get出错", "Main", LogCategory.I);
                return -1;
            }

            nameFullPathSet = "";
            foreach (string str in listStr)
            {
                if (dic.ContainsKey(str))
                {
                    nameFullPathSet += dic[str] + ",";
                }
            }

            if (nameFullPathSet != "")
            {
                nameFullPathSet = nameFullPathSet.Remove(nameFullPathSet.Length - 1, 1);
            }

            return 0;
        }

        private int change_domain_nameFullPath_2_id(string nameFullPathSet, ref string idSet)
        {
            if (string.IsNullOrEmpty(nameFullPathSet))
            {
                return -1;
            }

            List<string> listStr = new List<string>();
            string[] s = nameFullPathSet.Split(new char[] { ',' });

            if (s.Length <= 0)
            {
                return -1;
            }
            else
            {
                foreach (string str in s)
                {
                    listStr.Add(str);
                }
            }      

            Dictionary<string, string> dic = new Dictionary<string, string>();
            if ((int)RC.SUCCESS != gDbHelper.domain_dictionary_id_nameFullPath_get(ref dic))
            {
                add_log_info(LogInfoType.EROR, "domain_dictionary_id_nameFullPath_get出错", "Main", LogCategory.I);
                Logger.Trace(LogInfoType.EROR, "domain_dictionary_id_nameFullPath_get出错", "Main", LogCategory.I);
                return -1;
            }

            idSet = "";
            foreach (string str in listStr)
            {
                foreach (KeyValuePair<string, string> kv in dic)
                {
                    if (kv.Value == str)
                    {
                        idSet += kv.Key + ",";
                        break;
                    }
                }
            }

            if (idSet != "")
            {
                idSet = idSet.Remove(idSet.Length - 1, 1);
            }

            return 0;
        }

        /// <summary>
        /// 处理收到从AppController收到的消息
        /// </summary>
        /// <param name="strBody">消息体</param>
        /// <returns></returns>
        private int process_app_controller_msg(string strBody)
        {
            int rv = 0;            

            if (string.IsNullOrEmpty(strBody))
            {
                add_log_info(LogInfoType.EROR, "strBody is Error.\n", "Main", LogCategory.I);
                Logger.Trace(LogInfoType.EROR, "strBody is Error.\n", "Main", LogCategory.I);
                return -1;
            }

            try
            {
                //反系列化
                gAppUpper = JsonConvert.DeserializeObject<InterModuleMsgStruct>(strBody);
                switch (gAppUpper.Body.type)
                {
                    case AppMsgType.app_all_domain_request:
                        {
                            #region 老的交互，先注释掉

                            #region 获取所有的叶子节点

                            //DataTable dt = new DataTable();
                            //int rtv = gDbHelper.domain_record_leaf_get(ref dt);

                            //if (rtv != 0)
                            //{
                            //    gAppUpper.Body.type = "app_oper_domain_response";
                            //    gAppUpper.Body.dic = new Dictionary<string, object>();
                            //    gAppUpper.Body.dic.Add("ReturnCode", rtv);
                            //    gAppUpper.Body.dic.Add("ReturnStr", gDbHelper.get_rtv_str(rtv));

                            //    Send_Msg_2_AppCtrl_Upper(gAppUpper);
                            //    break;
                            //}

                            #endregion

                            #region 获取所有的站点下的设备

                            //int leafSuccessCnt = dt.Rows.Count;
                            //int domainId = 0;
                            //int isStation = 0;
                            //string nameFullPath = "";

                            //gAppUpper.Body.type = "app_oper_domain_response";
                            //gAppUpper.Body.dic = new Dictionary<string, object>();
                            //gAppUpper.Body.dic.Add("ReturnCode", rtv);
                            //gAppUpper.Body.dic.Add("ReturnStr", gDbHelper.get_rtv_str(rtv));
                            //gAppUpper.Body.dic.Add("LeafCount", leafSuccessCnt.ToString());
                            //gAppUpper.Body.n_dic = new List<Name_DIC_Struct>();

                            //int inx = 1;
                            //foreach (DataRow dr in dt.Rows)
                            //{
                            //    domainId = int.Parse(dr["id"].ToString());
                            //    isStation = int.Parse(dr["isStation"].ToString());
                            //    nameFullPath = dr["nameFullPath"].ToString();

                            //    if (isStation == 1)
                            //    {
                            //        List<string> listId = new List<string>();
                            //        List<string> listName = new List<string>();

                            //        rtv = gDbHelper.device_id_name_get_by_affdomainid(domainId, ref listId, ref listName);
                            //        if (rtv == 0)
                            //        {
                            //            string tmp = "";
                            //            foreach (string str in listName)
                            //            {
                            //                tmp += str + ",";

                            //                //设置站点下所有设备的信息
                            //                set_device_info_by_name_affdomainid(str, domainId, nameFullPath, ref gAppUpper);
                            //            }

                            //            if (tmp != "")
                            //            {
                            //                tmp = tmp.Remove(tmp.Length - 1, 1);                                     

                            //                gAppUpper.Body.dic.Add("DomainName" + inx.ToString(), nameFullPath);
                            //                gAppUpper.Body.dic.Add("DeviceList" + inx.ToString(), tmp);
                            //                inx++;
                            //            }
                            //            else
                            //            {
                            //                gAppUpper.Body.dic.Add("DomainName" + inx.ToString(), nameFullPath);
                            //                gAppUpper.Body.dic.Add("DeviceList" + inx.ToString(), "null");
                            //                inx++;
                            //            }                                      
                            //        }
                            //    }
                            //    else
                            //    {
                            //        gAppUpper.Body.dic.Add("DomainName" + inx.ToString(), nameFullPath);
                            //        gAppUpper.Body.dic.Add("DeviceList" + inx.ToString(), "null");
                            //        inx++;
                            //    }
                            //}

                            #endregion

                            #endregion

                            #region 获取所有的节点

                            //app获取所有的域信息请求
                            //即把整个设备树发给APP

                            DataTable dt = new DataTable();
                            int rtv = gDbHelper.domain_record_entity_get(ref dt, 0);

                            gAppUpper.Body.type = AppMsgType.app_all_domain_response;
                            gAppUpper.Body.dic = new Dictionary<string, object>();                           
                            gAppUpper.Body.n_dic = new List<Name_DIC_Struct>();
                                                      
                            if (rtv != 0)
                            {
                                gAppUpper.Body.dic.Add("ReturnCode", rtv);
                                gAppUpper.Body.dic.Add("ReturnStr", get_debug_info() + gDbHelper.get_rtv_str(rtv));
                                Send_Msg_2_AppCtrl_Upper(gAppUpper);
                                break;
                            }
                            else
                            {
                                gAppUpper.Body.dic.Add("ReturnCode", rtv);
                                gAppUpper.Body.dic.Add("ReturnStr", gDbHelper.get_rtv_str(rtv));
                                gAppUpper.Body.dic.Add("NodeCount", dt.Rows.Count.ToString());
                                foreach (DataRow dr in dt.Rows)
                                {
                                    Name_DIC_Struct ndic = new Name_DIC_Struct();

                                    if (string.IsNullOrEmpty(dr["nameFullPath"].ToString()))
                                    {
                                        ndic.name = "null";
                                    }
                                    else
                                    {
                                        ndic.name = dr["nameFullPath"].ToString();
                                    }

                                    if (string.IsNullOrEmpty(dr["id"].ToString()))
                                    {
                                        ndic.dic.Add("id", "null");
                                    }
                                    else
                                    {
                                        ndic.dic.Add("id", dr["id"].ToString());
                                    }

                                    if (string.IsNullOrEmpty(dr["name"].ToString()))
                                    {
                                        ndic.dic.Add("name", "null");
                                    }
                                    else
                                    {
                                        ndic.dic.Add("name", dr["name"].ToString());
                                    }

                                    if (string.IsNullOrEmpty(dr["parentId"].ToString()))
                                    {
                                        ndic.dic.Add("parentId", "null");
                                    }
                                    else
                                    {
                                        ndic.dic.Add("parentId", dr["parentId"].ToString());
                                    }

                                    if (string.IsNullOrEmpty(dr["isStation"].ToString()))
                                    {
                                        ndic.dic.Add("isStation", "null");
                                    }
                                    else
                                    {
                                        ndic.dic.Add("isStation", dr["isStation"].ToString());
                                    }

                                    gAppUpper.Body.n_dic.Add(ndic);
                                }

                                Send_Msg_2_AppCtrl_Upper(gAppUpper);
                                break;
                            }

                            #endregion                            
                        }
                    case AppMsgType.app_add_domain_request:
                        {
                            #region 获取信息

                            string name = "";
                            string parentNameFullPath = "";
                            int isStation = -1;
                            string des = "";

                            if (gAppUpper.Body.dic.ContainsKey("name"))
                            {
                                name = gAppUpper.Body.dic["name"].ToString();
                            }

                            if (gAppUpper.Body.dic.ContainsKey("parentNameFullPath"))
                            {
                                parentNameFullPath = gAppUpper.Body.dic["parentNameFullPath"].ToString();
                            }

                            if (gAppUpper.Body.dic.ContainsKey("isStation"))
                            {
                                isStation = int.Parse(gAppUpper.Body.dic["isStation"].ToString());
                            }

                            if (gAppUpper.Body.dic.ContainsKey("des"))
                            {
                                des = gAppUpper.Body.dic["des"].ToString();

                                if (string.IsNullOrEmpty(des))
                                {
                                    des = "null";
                                }
                            }
                            else
                            {
                                des = "null";
                            }

                            if (name == "" || parentNameFullPath == "" || isStation == -1)
                            {
                                add_log_info(LogInfoType.EROR, "app_add_domain_request,参数有误", "Main", LogCategory.I);
                                Logger.Trace(LogInfoType.EROR, "app_add_domain_request,参数有误", "Main", LogCategory.I);

                                //返回出错处理
                                gAppUpper.Body.type = AppMsgType.app_add_domain_response;
                                gAppUpper.Body.dic = new Dictionary<string, object>();
                                gAppUpper.Body.dic.Add("ReturnCode", -1);
                                gAppUpper.Body.dic.Add("ReturnStr", get_debug_info() + "app_add_domain_request,参数有误.");

                                Send_Msg_2_AppCtrl_Upper(gAppUpper);
                                break;
                            }

                            if (isStation != 0 && isStation != 1)
                            {
                                add_log_info(LogInfoType.EROR, "app_add_domain_request,isStation", "Main", LogCategory.I);
                                Logger.Trace(LogInfoType.EROR, "app_add_domain_request,isStation", "Main", LogCategory.I);

                                //返回出错处理
                                gAppUpper.Body.type = AppMsgType.app_add_domain_response;
                                gAppUpper.Body.dic = new Dictionary<string, object>();
                                gAppUpper.Body.dic.Add("ReturnCode", -1);
                                gAppUpper.Body.dic.Add("ReturnStr", get_debug_info() + "app_add_domain_request,isStation.");

                                Send_Msg_2_AppCtrl_Upper(gAppUpper);
                                break;
                            }

                            string str = string.Format("name = {0},parentNameFullPath = {1},isStation = {2},des = {3}", name, parentNameFullPath, isStation, des);
                            add_log_info(LogInfoType.INFO, str, "Main", LogCategory.I);
                            Logger.Trace(LogInfoType.INFO, str, "Main", LogCategory.I);

                            #endregion

                            #region 返回消息

                            int rtv = gDbHelper.domain_record_insert(name, parentNameFullPath, isStation, des);

                            gAppUpper.Body.type = AppMsgType.app_add_domain_response;
                            gAppUpper.Body.dic = new Dictionary<string, object>();
                            gAppUpper.Body.dic.Add("ReturnCode", rtv);

                            if (rtv != 0)
                            {
                                gAppUpper.Body.dic.Add("ReturnStr", get_debug_info() + gDbHelper.get_rtv_str(rtv));
                            }
                            else
                            {
                                gAppUpper.Body.dic.Add("ReturnStr", gDbHelper.get_rtv_str(rtv));                                
                            }

                            Send_Msg_2_AppCtrl_Upper(gAppUpper);
                            break;

                            #endregion
                        }
                    case AppMsgType.app_del_domain_request:
                        {
                            #region 获取信息

                            string nameFullPath = "";

                            if (gAppUpper.Body.dic.ContainsKey("nameFullPath"))
                            {
                                nameFullPath = gAppUpper.Body.dic["nameFullPath"].ToString();
                            }

                            if (nameFullPath == "")
                            {
                                add_log_info(LogInfoType.EROR, "app_del_domain_request,参数有误", "Main", LogCategory.I);
                                Logger.Trace(LogInfoType.EROR, "app_del_domain_request,参数有误", "Main", LogCategory.I);

                                //返回出错处理
                                gAppUpper.Body.type = AppMsgType.app_del_domain_response;
                                gAppUpper.Body.dic = new Dictionary<string, object>();
                                gAppUpper.Body.dic.Add("ReturnCode", -1);
                                gAppUpper.Body.dic.Add("ReturnStr", get_debug_info() + "app_del_domain_request,参数有误.");

                                Send_Msg_2_AppCtrl_Upper(gAppUpper);
                                break;
                            }

                            string str = string.Format("nameFullPath = {0}", nameFullPath);
                            add_log_info(LogInfoType.INFO, str, "Main", LogCategory.I);
                            Logger.Trace(LogInfoType.INFO, str, "Main", LogCategory.I);

                            #endregion

                            #region 返回消息

                            int rtv = gDbHelper.domain_record_delete(nameFullPath);

                            gAppUpper.Body.type = AppMsgType.app_del_domain_response;
                            gAppUpper.Body.dic = new Dictionary<string, object>();
                            gAppUpper.Body.dic.Add("ReturnCode", rtv);

                            if (rtv != 0)
                            {
                                gAppUpper.Body.dic.Add("ReturnStr", get_debug_info() + gDbHelper.get_rtv_str(rtv));
                            }
                            else
                            {
                                gAppUpper.Body.dic.Add("ReturnStr", gDbHelper.get_rtv_str(rtv));                                
                            }

                            Send_Msg_2_AppCtrl_Upper(gAppUpper);

                            #endregion    

                            #region 重新获取gDicDeviceId

                            if (rtv == 0)
                            {
                                if (0 == gDbHelper.domain_dictionary_info_join_get(ref gDicDeviceId))
                                {
                                    add_log_info(LogInfoType.INFO, "gDicDeviceId -> 获取OK！", "Main", LogCategory.I);
                                    Logger.Trace(LogInfoType.INFO, "gDicDeviceId -> 获取OK！", "Main", LogCategory.I);
                                }
                                else
                                {
                                    add_log_info(LogInfoType.INFO, "gDicDeviceId -> 获取FAILED！", "Main", LogCategory.I);
                                    Logger.Trace(LogInfoType.INFO, "gDicDeviceId -> 获取FAILED！", "Main", LogCategory.I);
                                }
                            }

                            #endregion

                            break;
                        }
                    case AppMsgType.app_rename_domain_request:
                        {
                            #region 获取信息

                            //    "oldNameFullPath":"设备.深圳.南山",
                            //    "newNameFullPath":"设备.深圳.宝安",
                            //    "newDes":"描述"   //当该字段为""时，就不修改oldNameFullPath的描述
                            //                     //当该字段不为""时，就修改oldNameFullPath的描述

                            int rtv = -1;
                            string oldNameFullPath = "";
                            string newNameFullPath = "";
                            string newDes = "";

                            if (gAppUpper.Body.dic.ContainsKey("oldNameFullPath"))
                            {
                                oldNameFullPath = gAppUpper.Body.dic["oldNameFullPath"].ToString();
                            }

                            if (gAppUpper.Body.dic.ContainsKey("newNameFullPath"))
                            {
                                newNameFullPath = gAppUpper.Body.dic["newNameFullPath"].ToString();
                            }

                            if (gAppUpper.Body.dic.ContainsKey("newDes"))
                            {
                                newDes = gAppUpper.Body.dic["newDes"].ToString();
                            }

                            if (oldNameFullPath == "" || newNameFullPath == "")
                            {
                                add_log_info(LogInfoType.EROR, "app_rename_domain_request,参数有误", "Main", LogCategory.I);
                                Logger.Trace(LogInfoType.EROR, "app_rename_domain_request,参数有误","Main", LogCategory.I);

                                //返回出错处理
                                gAppUpper.Body.type = AppMsgType.app_rename_domain_response;
                                gAppUpper.Body.dic = new Dictionary<string, object>();
                                gAppUpper.Body.dic.Add("ReturnCode", -1);
                                gAppUpper.Body.dic.Add("ReturnStr", get_debug_info() + "app_del_domain_request,参数有误.");

                                Send_Msg_2_AppCtrl_Upper(gAppUpper);
                                break;
                            }

                            string oldNameFullPathRoot = "";
                            string newNameFullPathRoot = "";

                            if (!oldNameFullPath.Contains(".") || !newNameFullPath.Contains("."))
                            {
                                add_log_info(LogInfoType.EROR, "没包含点分割号,参数有误", "Main", LogCategory.I);
                                Logger.Trace(LogInfoType.EROR, "没包含点分割号,参数有误", "Main", LogCategory.I);

                                //返回出错处理
                                gAppUpper.Body.type = AppMsgType.app_rename_domain_response;
                                gAppUpper.Body.dic = new Dictionary<string, object>();
                                gAppUpper.Body.dic.Add("ReturnCode", -1);
                                gAppUpper.Body.dic.Add("ReturnStr", get_debug_info() + "没包含点分割号,参数有误.");

                                Send_Msg_2_AppCtrl_Upper(gAppUpper);
                                break;
                            }
                            else
                            {
                                int i = oldNameFullPath.LastIndexOf(".");
                                int j = newNameFullPath.LastIndexOf(".");

                                oldNameFullPathRoot = oldNameFullPath.Substring(0, i);
                                newNameFullPathRoot = newNameFullPath.Substring(0, i);

                                if (oldNameFullPathRoot != newNameFullPathRoot)
                                {
                                    add_log_info(LogInfoType.EROR, "根部路径名称不一致,参数有误", "Main", LogCategory.I);
                                    Logger.Trace(LogInfoType.EROR, "根部路径名称不一致,参数有误", "Main", LogCategory.I);

                                    //返回出错处理
                                    gAppUpper.Body.type = AppMsgType.app_rename_domain_response;
                                    gAppUpper.Body.dic = new Dictionary<string, object>();
                                    gAppUpper.Body.dic.Add("ReturnCode", -1);
                                    gAppUpper.Body.dic.Add("ReturnStr", get_debug_info() + "根部路径名称不一致,参数有误.");

                                    Send_Msg_2_AppCtrl_Upper(gAppUpper);
                                    break;
                                }
                            }

                            #region 修改描述处理

                            if (newDes != "")
                            {
                                rtv = gDbHelper.domain_record_update_des(oldNameFullPath, newDes);
                                if (rtv != 0)
                                {
                                    //返回出错处理
                                    gAppUpper.Body.type = AppMsgType.app_rename_domain_response;
                                    gAppUpper.Body.dic = new Dictionary<string, object>();
                                    gAppUpper.Body.dic.Add("ReturnCode", -1);
                                    gAppUpper.Body.dic.Add("ReturnStr", gDbHelper.get_rtv_str(rtv));
                                    Send_Msg_2_AppCtrl_Upper(gAppUpper);

                                    break;
                                }
                            }

                            #endregion

                            string str = string.Format("oldNameFullPath = {0},newNameFullPath = {1}", oldNameFullPath, newNameFullPath);
                            add_log_info(LogInfoType.INFO, str, "Main", LogCategory.I);
                            Logger.Trace(LogInfoType.INFO, str, "Main", LogCategory.I);

                            #endregion

                            #region 返回消息

                            rtv = 0;
                            if (oldNameFullPath != newNameFullPath)
                            {
                                rtv = gDbHelper.domain_record_rename(oldNameFullPath, newNameFullPath);
                            }

                            gAppUpper.Body.type = AppMsgType.app_rename_domain_response;
                            gAppUpper.Body.dic = new Dictionary<string, object>();
                            gAppUpper.Body.dic.Add("ReturnCode", rtv);

                            if (rtv != 0)
                            {
                                gAppUpper.Body.dic.Add("ReturnStr", get_debug_info() + gDbHelper.get_rtv_str(rtv));
                            }
                            else
                            {
                                gAppUpper.Body.dic.Add("ReturnStr", gDbHelper.get_rtv_str(rtv));
                            }

                            Send_Msg_2_AppCtrl_Upper(gAppUpper);                           

                            #endregion                                               

                            #region 重新获取gDicDeviceId

                            if (rtv == 0)
                            {
                                if (0 == gDbHelper.domain_dictionary_info_join_get(ref gDicDeviceId))
                                {
                                    add_log_info(LogInfoType.INFO, "gDicDeviceId -> 获取OK！", "Main", LogCategory.I);
                                    Logger.Trace(LogInfoType.INFO, "gDicDeviceId -> 获取OK！", "Main", LogCategory.I);
                                }
                                else
                                {
                                    add_log_info(LogInfoType.INFO, "gDicDeviceId -> 获取FAILED！", "Main", LogCategory.I);
                                    Logger.Trace(LogInfoType.INFO, "gDicDeviceId -> 获取FAILED！", "Main", LogCategory.I);
                                }
                            }

                            break;

                            #endregion                            
                        }
                    case AppMsgType.app_login_request:
                        {
                            #region 获取信息

                            string usr = "";
                            string psw = "";

                            if (gAppUpper.Body.dic.ContainsKey("UserName"))
                            {
                                usr = gAppUpper.Body.dic["UserName"].ToString();
                            }

                            if (gAppUpper.Body.dic.ContainsKey("PassWord"))
                            {
                                psw = gAppUpper.Body.dic["PassWord"].ToString();
                            }

                            if (usr == "" || psw == "")
                            {
                                add_log_info(LogInfoType.EROR, "usr or psw NULL.", "Main", LogCategory.I);
                                Logger.Trace(LogInfoType.EROR, "usr or psw NULL.", "Main", LogCategory.I);

                                //返回出错处理
                                gAppUpper.Body.type = AppMsgType.app_login_response;
                                gAppUpper.Body.dic = new Dictionary<string, object>();
                                gAppUpper.Body.dic.Add("ReturnCode", -1);
                                gAppUpper.Body.dic.Add("ReturnStr", get_debug_info() + "usr or psw NULL.");

                                Send_Msg_2_AppCtrl_Upper(gAppUpper);
                                break;
                            }

                            string str = string.Format("UserName = {0},PassWord = {1}", usr, psw);
                            add_log_info(LogInfoType.INFO, str, "Main", LogCategory.I);
                            Logger.Trace(LogInfoType.INFO, str, "Main", LogCategory.I);

                            #endregion

                            #region 验证用户合法性

                            int rtv = gDbHelper.user_record_check(usr, psw);
                            if (rtv != 0)
                            {
                                gAppUpper.Body.type = AppMsgType.app_login_response;
                                gAppUpper.Body.dic = new Dictionary<string, object>();
                                gAppUpper.Body.dic.Add("ReturnCode", rtv);
                                gAppUpper.Body.dic.Add("ReturnStr", get_debug_info() + gDbHelper.get_rtv_str(rtv));

                                Send_Msg_2_AppCtrl_Upper(gAppUpper);
                                break;
                            }
                            else
                            {
                                gAppUpper.Body.type = AppMsgType.app_login_response;
                                gAppUpper.Body.dic = new Dictionary<string, object>();
                                gAppUpper.Body.dic.Add("ReturnCode", rtv);
                                gAppUpper.Body.dic.Add("ReturnStr", gDbHelper.get_rtv_str(rtv));
                            }

                            #endregion

                            #region 获取用户所属的组/角色

                            string roleName = "";
                            rtv = gDbHelper.userrole_get_by_user_name(usr, ref roleName);
                            if (rtv != 0)
                            {
                                string errInfo = string.Format("通过用户名称获取所属的用户角色失败:{0}",gDbHelper.get_rtv_str(rtv));
                                add_log_info(LogInfoType.WARN, errInfo, "Main", LogCategory.I);
                                Logger.Trace(LogInfoType.WARN, errInfo, "Main", LogCategory.I);
                            }

                            #endregion

                            #region 获取用户的域权限                           

                            List<string> listDomain = new List<string>();
                            rtv = gDbHelper.userdomain_set_get_by_usrname(usr, ref listDomain);
                            if (rtv != 0)
                            {
                                string errInfo = string.Format("通过用户名获取用户所有的域权限失败:{0}", gDbHelper.get_rtv_str(rtv));

                                add_log_info(LogInfoType.WARN, errInfo, "Main", LogCategory.I);
                                Logger.Trace(LogInfoType.WARN, errInfo, "Main", LogCategory.I);
                            }

                            #endregion

                            #region 获取用户的功能权限                       

                            List<string> listIdSet = new List<string>();

                            // 通过角色名称获取对应的权限ID集合
                            rtv = gDbHelper.roleprivilege_priidset_get_by_rolename(roleName, ref listIdSet);
                            if (rtv != 0)
                            {
                                string errInfo = string.Format("通过角色名称获取对应的权限ID集合失败:{0}", gDbHelper.get_rtv_str(rtv));

                                add_log_info(LogInfoType.WARN, errInfo, "Main", LogCategory.I);
                                Logger.Trace(LogInfoType.WARN, errInfo, "Main", LogCategory.I);
                            }

                            List<string> listFunName = new List<string>();
                            foreach (string id in listIdSet)
                            {
                                string funName = "";

                                //通过id获取对应的功能ID
                                rtv = gDbHelper.privilege_funname_get_by_id(id, ref funName);
                                if (rtv != 0)
                                {
                                    string errInfo = string.Format("privilege_funname_get_by_id,通过id获取对应的功能ID:{0}", gDbHelper.get_rtv_str(rtv));
                                    add_log_info(LogInfoType.WARN, errInfo, "Main", LogCategory.I);
                                    Logger.Trace(LogInfoType.WARN, errInfo, "Main", LogCategory.I);
                                }
                                else
                                {
                                    listFunName.Add(funName);
                                }
                            }

                            #endregion

                            #region 返回信息

                            if (roleName == "")
                            {
                                gAppUpper.Body.dic.Add("GroupName", "");
                            }
                            else
                            {
                                gAppUpper.Body.dic.Add("GroupName", roleName);
                            }

                            if (listDomain.Count == 0)
                            {
                                gAppUpper.Body.dic.Add("DomainList", "");
                            }
                            else
                            {
                                string tmp = "";
                                for (int i = 0; i < listDomain.Count; i++)
                                {
                                    if (i == (listDomain.Count - 1))
                                    {
                                        tmp += listDomain[i];
                                    }
                                    else
                                    {
                                        tmp += listDomain[i] + ",";
                                    }
                                }

                                gAppUpper.Body.dic.Add("DomainList", tmp);
                            }


                            if (listFunName.Count == 0)
                            {
                                gAppUpper.Body.dic.Add("FunList", "");
                            }
                            else
                            {
                                string tmp = "";
                                for (int i = 0; i < listFunName.Count; i++)
                                {
                                    if (i == (listFunName.Count - 1))
                                    {
                                        tmp += listFunName[i];
                                    }
                                    else
                                    {
                                        tmp += listFunName[i] + ",";
                                    }
                                }

                                gAppUpper.Body.dic.Add("FunList", tmp);
                            }

                            Send_Msg_2_AppCtrl_Upper(gAppUpper);

                            #endregion

                            break;
                        }
                    case AppMsgType.app_all_roletype_request:
                        {
                            #region 返回信息

                            DataTable dt = new DataTable();

                            int rtv = gDbHelper.roletype_record_entity_get(ref dt);

                            gAppUpper.Body.type = AppMsgType.app_all_roletype_response;
                            gAppUpper.Body.dic = new Dictionary<string, object>();
                            gAppUpper.Body.dic.Add("ReturnCode", rtv);
                            gAppUpper.Body.dic.Add("ReturnStr", gDbHelper.get_rtv_str(rtv));

                            if (rtv == 0)
                            {
                                gAppUpper.Body.dic.Add("TypeCount", dt.Rows.Count.ToString());

                                int i = 1;
                                foreach (DataRow dr in dt.Rows)
                                {
                                    if (string.IsNullOrEmpty(dr["roleType"].ToString()))
                                    {
                                        gAppUpper.Body.dic.Add("RoleType" + i.ToString(), "null");
                                    }
                                    else
                                    {
                                        gAppUpper.Body.dic.Add("RoleType" + i.ToString(), dr["roleType"].ToString());
                                    }

                                    if (string.IsNullOrEmpty(dr["des"].ToString()))
                                    {
                                        gAppUpper.Body.dic.Add("Des" + i.ToString(), "null");
                                    }
                                    else
                                    {
                                        gAppUpper.Body.dic.Add("Des" + i.ToString(), dr["des"].ToString());
                                    }

                                    i++;
                                }
                            }

                            Send_Msg_2_AppCtrl_Upper(gAppUpper);
                            break;

                            #endregion
                        }
                    case AppMsgType.app_add_roletype_request:
                        {
                            #region 获取信息

                            string RoleType = "";
                            string Des = "";

                            if (gAppUpper.Body.dic.ContainsKey("RoleType"))
                            {
                                RoleType = gAppUpper.Body.dic["RoleType"].ToString();
                            }

                            if (gAppUpper.Body.dic.ContainsKey("Des"))
                            {
                                Des = gAppUpper.Body.dic["Des"].ToString();                               
                            }                           

                            if (RoleType == "")
                            {
                                add_log_info(LogInfoType.EROR, "app_add_roletype_request,参数有误", "Main", LogCategory.I);
                                Logger.Trace(LogInfoType.EROR, "app_add_roletype_request,参数有误", "Main", LogCategory.I);

                                //返回出错处理
                                gAppUpper.Body.type = AppMsgType.app_add_roletype_response;
                                gAppUpper.Body.dic = new Dictionary<string, object>();
                                gAppUpper.Body.dic.Add("ReturnCode", -1);
                                gAppUpper.Body.dic.Add("ReturnStr", get_debug_info() + "app_add_roletype_response,参数有误.");

                                Send_Msg_2_AppCtrl_Upper(gAppUpper);
                                break;
                            }

                            #endregion

                            #region 返回消息

                            int rtv = gDbHelper.roletype_record_insert(RoleType, Des);

                            gAppUpper.Body.type = AppMsgType.app_add_roletype_response;
                            gAppUpper.Body.dic = new Dictionary<string, object>();
                            gAppUpper.Body.dic.Add("ReturnCode", rtv);

                            if (rtv != 0)
                            {
                                gAppUpper.Body.dic.Add("ReturnStr", get_debug_info() + gDbHelper.get_rtv_str(rtv));
                            }
                            else
                            {
                                gAppUpper.Body.dic.Add("ReturnStr", gDbHelper.get_rtv_str(rtv));
                            }

                            Send_Msg_2_AppCtrl_Upper(gAppUpper);
                            break;

                            #endregion                   
                        }
                    case AppMsgType.app_del_roletype_request:
                        {
                            #region 获取信息

                            string RoleType = "";

                            if (gAppUpper.Body.dic.ContainsKey("RoleType"))
                            {
                                RoleType = gAppUpper.Body.dic["RoleType"].ToString();
                            }

                            if (RoleType == "")
                            {
                                add_log_info(LogInfoType.EROR, "app_del_roletype_request,参数有误", "Main", LogCategory.I);
                                Logger.Trace(LogInfoType.EROR, "app_del_roletype_request,参数有误", "Main", LogCategory.I);

                                //返回出错处理
                                gAppUpper.Body.type = AppMsgType.app_del_roletype_response;
                                gAppUpper.Body.dic = new Dictionary<string, object>();
                                gAppUpper.Body.dic.Add("ReturnCode", -1);
                                gAppUpper.Body.dic.Add("ReturnStr", get_debug_info() + "app_del_roletype_request,参数有误.");

                                Send_Msg_2_AppCtrl_Upper(gAppUpper);
                                break;
                            }

                            #endregion

                            #region 返回消息

                            int rtv = gDbHelper.roletype_record_delete(RoleType);

                            gAppUpper.Body.type = AppMsgType.app_del_roletype_response;
                            gAppUpper.Body.dic = new Dictionary<string, object>();
                            gAppUpper.Body.dic.Add("ReturnCode", rtv);

                            if (rtv != 0)
                            {
                                gAppUpper.Body.dic.Add("ReturnStr", get_debug_info() + gDbHelper.get_rtv_str(rtv));
                            }
                            else
                            {
                                gAppUpper.Body.dic.Add("ReturnStr", gDbHelper.get_rtv_str(rtv));
                            }

                            Send_Msg_2_AppCtrl_Upper(gAppUpper);
                            break;

                            #endregion                                             
                        }
                    case AppMsgType.app_all_role_request:
                        {
                            #region 返回信息

                            DataTable dt = new DataTable();

                            //name,roleType,timeStart,timeEnd,des
                            int rtv = gDbHelper.role_record_entity_get(ref dt);

                            gAppUpper.Body.type = AppMsgType.app_all_role_response;
                            gAppUpper.Body.dic = new Dictionary<string, object>();
                            gAppUpper.Body.dic.Add("ReturnCode", rtv);

                            if (rtv != 0)
                            {
                                gAppUpper.Body.dic.Add("ReturnStr", get_debug_info() + gDbHelper.get_rtv_str(rtv));
                            }
                            else
                            {
                                gAppUpper.Body.dic.Add("ReturnStr", gDbHelper.get_rtv_str(rtv));
                            }

                            gAppUpper.Body.n_dic = new List<Name_DIC_Struct>();

                            if (rtv == 0)
                            {
                                gAppUpper.Body.dic.Add("GroupCount", dt.Rows.Count.ToString());
                                gAppUpper.Body.n_dic = new List<Name_DIC_Struct>();


                                foreach (DataRow dr in dt.Rows)
                                {
                                    Name_DIC_Struct ndic = new Name_DIC_Struct();

                                    if (string.IsNullOrEmpty(dr["name"].ToString()))
                                    {
                                        ndic.name = "null";
                                        ndic.dic.Add("name", "null");
                                    }
                                    else
                                    {
                                        ndic.name = dr["name"].ToString();
                                        ndic.dic.Add("name", dr["name"].ToString());
                                    }

                                    if (string.IsNullOrEmpty(dr["roleType"].ToString()))
                                    {
                                        ndic.dic.Add("roleType", "null");
                                    }
                                    else
                                    {
                                        ndic.dic.Add("roleType", dr["roleType"].ToString());
                                    }

                                    if (string.IsNullOrEmpty(dr["timeStart"].ToString()))
                                    {
                                        ndic.dic.Add("timeStart", "null");
                                    }
                                    else
                                    {
                                        ndic.dic.Add("timeStart", dr["timeStart"].ToString());
                                    }

                                    if (string.IsNullOrEmpty(dr["timeEnd"].ToString()))
                                    {
                                        ndic.dic.Add("timeEnd", "null");
                                    }
                                    else
                                    {
                                        ndic.dic.Add("timeEnd", dr["timeEnd"].ToString());
                                    }

                                    if (string.IsNullOrEmpty(dr["des"].ToString()))
                                    {
                                        ndic.dic.Add("des", "null");
                                    }
                                    else
                                    {
                                        ndic.dic.Add("des", dr["des"].ToString());
                                    }

                                    gAppUpper.Body.n_dic.Add(ndic);
                                }
                            }

                            Send_Msg_2_AppCtrl_Upper(gAppUpper);
                            break;

                            #endregion
                        }
                    case AppMsgType.app_add_role_request:
                        {
                            #region 获取信息

                            //name,roleType,timeStart,timeEnd,des
                            string name = "";
                            string roleType = "";
                            string timeStart = "";
                            string timeEnd = "";
                            string des = "";

                            if (gAppUpper.Body.dic.ContainsKey("name"))
                            {
                                name = gAppUpper.Body.dic["name"].ToString();
                            }

                            if (gAppUpper.Body.dic.ContainsKey("roleType"))
                            {
                                roleType = gAppUpper.Body.dic["roleType"].ToString();
                            }

                            if (gAppUpper.Body.dic.ContainsKey("timeStart"))
                            {
                                timeStart = gAppUpper.Body.dic["timeStart"].ToString();
                            }

                            if (gAppUpper.Body.dic.ContainsKey("timeEnd"))
                            {
                                timeEnd = gAppUpper.Body.dic["timeEnd"].ToString();
                            }

                            if (gAppUpper.Body.dic.ContainsKey("des"))
                            {
                                des = gAppUpper.Body.dic["des"].ToString();
                                if (string.IsNullOrEmpty(des))
                                {
                                    des = "null";
                                }
                            }
                            else
                            {
                                des = "null";
                            }

                            if (name == "" || roleType == "" || timeStart == "" || timeEnd == "" || des == "")
                            {
                                add_log_info(LogInfoType.EROR, "app_add_role_request,参数有误", "Main", LogCategory.I);
                                Logger.Trace(LogInfoType.EROR, "app_add_role_request,参数有误", "Main", LogCategory.I);

                                //返回出错处理
                                gAppUpper.Body.type = AppMsgType.app_add_role_response;
                                gAppUpper.Body.dic = new Dictionary<string, object>();
                                gAppUpper.Body.dic.Add("ReturnCode", -1);
                                gAppUpper.Body.dic.Add("ReturnStr", get_debug_info() + "app_add_role_request,参数有误.");

                                Send_Msg_2_AppCtrl_Upper(gAppUpper);
                                break;
                            }

                            #endregion

                            #region 返回消息

                            int rtv = gDbHelper.role_record_insert(name, roleType, timeStart, timeEnd, des);

                            gAppUpper.Body.type = AppMsgType.app_add_role_response;
                            gAppUpper.Body.dic = new Dictionary<string, object>();

                            gAppUpper.Body.dic.Add("ReturnCode", rtv);
                            if (rtv != 0)
                            {
                                gAppUpper.Body.dic.Add("ReturnStr", get_debug_info() + gDbHelper.get_rtv_str(rtv));
                            }
                            else
                            {
                                gAppUpper.Body.dic.Add("ReturnStr", gDbHelper.get_rtv_str(rtv));
                            }

                            Send_Msg_2_AppCtrl_Upper(gAppUpper);
                            break;

                            #endregion                   
                        }
                    case AppMsgType.app_del_role_request:
                        {
                            #region 获取信息

                            string name = "";

                            if (gAppUpper.Body.dic.ContainsKey("name"))
                            {
                                name = gAppUpper.Body.dic["name"].ToString();
                            }

                            if (name == "")
                            {
                                add_log_info(LogInfoType.EROR, "app_del_role_request,参数有误", "Main", LogCategory.I);
                                Logger.Trace(LogInfoType.EROR, "app_del_role_request,参数有误", "Main", LogCategory.I);

                                //返回出错处理
                                gAppUpper.Body.type = AppMsgType.app_del_role_response;
                                gAppUpper.Body.dic = new Dictionary<string, object>();
                                gAppUpper.Body.dic.Add("ReturnCode", -1);
                                gAppUpper.Body.dic.Add("ReturnStr", get_debug_info() + "app_del_roletype_request,参数有误.");

                                Send_Msg_2_AppCtrl_Upper(gAppUpper);
                                break;
                            }

                            #endregion

                            #region 返回消息

                            int rtv = gDbHelper.role_record_delete(name);

                            gAppUpper.Body.type = AppMsgType.app_del_role_response;
                            gAppUpper.Body.dic = new Dictionary<string, object>();
                            gAppUpper.Body.dic.Add("ReturnCode", rtv);

                            if (rtv != 0)
                            {
                                gAppUpper.Body.dic.Add("ReturnStr", get_debug_info() + gDbHelper.get_rtv_str(rtv));
                            }
                            else
                            {
                                gAppUpper.Body.dic.Add("ReturnStr", gDbHelper.get_rtv_str(rtv));
                            }

                            Send_Msg_2_AppCtrl_Upper(gAppUpper);
                            break;

                            #endregion                                         
                        }
                    case AppMsgType.app_all_device_request:
                        {
                            #region 获取信息

                            int affDomainId = -1;
                            string parentFullPathName = "";

                            if (gAppUpper.Body.dic.ContainsKey("parentFullPathName"))
                            {
                                parentFullPathName = gAppUpper.Body.dic["parentFullPathName"].ToString();
                            }

                            if (parentFullPathName == "")
                            {
                                add_log_info(LogInfoType.EROR, "app_all_device_request,参数有误", "Main", LogCategory.I);
                                Logger.Trace(LogInfoType.EROR, "app_all_device_request,参数有误", "Main", LogCategory.I);

                                //返回出错处理
                                gAppUpper.Body.type = AppMsgType.app_all_device_response;
                                gAppUpper.Body.dic = new Dictionary<string, object>();
                                gAppUpper.Body.dic.Add("ReturnCode", -1);
                                gAppUpper.Body.dic.Add("ReturnStr", get_debug_info() +  "app_all_device_request,参数有误.");

                                Send_Msg_2_AppCtrl_Upper(gAppUpper);
                                break;
                            }

                            #endregion

                            #region 返回消息

                            // 通过全路径获取ID
                            int rtv = gDbHelper.domain_get_id_by_nameFullPath(parentFullPathName, ref affDomainId);
                            if (rtv == 0)
                            {
                                //检查域ID是否为站点
                                rtv = gDbHelper.domain_record_is_station(affDomainId);
                                if (rtv == (int)RC.IS_STATION)
                                {
                                    List<string> listId = new List<string>();
                                    List<string> listName = new List<string>();

                                    rtv = gDbHelper.device_id_name_get_by_affdomainid(affDomainId, ref listId, ref listName);
                                    if (rtv == 0)
                                    {
                                        gAppUpper.Body.type = AppMsgType.app_all_device_response;
                                        gAppUpper.Body.dic = new Dictionary<string, object>();
                                        gAppUpper.Body.dic.Add("ReturnCode", rtv);
                                        gAppUpper.Body.dic.Add("ReturnStr", gDbHelper.get_rtv_str(rtv));
                                        gAppUpper.Body.dic.Add("DeviceCount", listName.Count.ToString());
                                        gAppUpper.Body.n_dic = new List<Name_DIC_Struct>();

                                        foreach (string str in listName)
                                        {
                                            //设置站点下所有设备的信息
                                            set_device_info_by_name_affdomainid(str, affDomainId, parentFullPathName, ref gAppUpper);
                                        }
                                    }
                                }
                                else
                                {
                                    gAppUpper.Body.type = AppMsgType.app_all_device_response;
                                    gAppUpper.Body.dic = new Dictionary<string, object>();
                                    gAppUpper.Body.dic.Add("ReturnCode", rtv);
                                    gAppUpper.Body.dic.Add("ReturnStr", get_debug_info() + gDbHelper.get_rtv_str(rtv));
                                }
                            }
                            else
                            {
                                gAppUpper.Body.type = AppMsgType.app_all_device_response;
                                gAppUpper.Body.dic = new Dictionary<string, object>();
                                gAppUpper.Body.dic.Add("ReturnCode", rtv);
                                gAppUpper.Body.dic.Add("ReturnStr", get_debug_info() + gDbHelper.get_rtv_str(rtv));
                            }

                            #region 获取未指派设备

                            //2018-06-26
                            DataTable dt = new DataTable();
                            rtv = gDbHelper.device_unknown_record_entity_get(ref dt);
                            if (rtv == 0)
                            {
                                if (dt.Rows.Count > 0)
                                {
                                    set_device_unknown_info_by_datatable(dt,ref gAppUpper);
                                }
                            }

                            #endregion

                            Send_Msg_2_AppCtrl_Upper(gAppUpper);
                            break;

                            #endregion                                         
                        }
                    case AppMsgType.app_add_device_request:
                        {
                            #region 获取信息

                            //   "parentFullPathName":"设备.深圳.福田.中心广场.西北监控",
                            //   "name":"电信FDD",
                            //   "mode":"LTE-TDD",    //GSM,TD-SCDMA,WCDMA,LTE-TDD,LTE-FDD     
                            //}
                            //   "n_dic":[     //2018-07-04
                            //      {
                            //         "name":"device_unknown",   //device_unknown标识未指派的设备
                            //         "dic":{
                            //         "ipAddr":"172.17.0.123",
                            //         "port":"12345",
                            //      }

                            int affDomainId = -1;
                            string parentFullPathName = "";
                            string name = "";
                            string mode = "";

                            string ipAddr = "";
                            string port = "";
                            bool noAssignedFlag = false;

                            if (gAppUpper.Body.dic.ContainsKey("parentFullPathName"))
                            {
                                parentFullPathName = gAppUpper.Body.dic["parentFullPathName"].ToString();
                            }

                            if (gAppUpper.Body.dic.ContainsKey("name"))
                            {
                                name = gAppUpper.Body.dic["name"].ToString();
                            }

                            if (gAppUpper.Body.dic.ContainsKey("mode"))
                            {
                                mode = gAppUpper.Body.dic["mode"].ToString();
                            }

                            if (parentFullPathName == "" || name == "" || mode == "")
                            {
                                //返回出错处理
                                string errInfo = get_debug_info() + string.Format("app_add_device_request,参数为空.");                            
                                add_log_info(LogInfoType.EROR, errInfo, "Main", LogCategory.I);
                                Logger.Trace(LogInfoType.EROR, errInfo, "Main", LogCategory.I);
                              
                                Fill_IMMS_Info(ref gAppUpper, AppMsgType.app_add_device_response, -1, errInfo, true,null,null);
                                Send_Msg_2_AppCtrl_Upper(gAppUpper);
                                break;                               
                            }

                            #region 未指派设备信息

                            // 2018-07-04
                            if (gAppUpper.Body.n_dic != null && gAppUpper.Body.n_dic.Count > 0)
                            {
                                string device_unknown_name = "";

                                if (!string.IsNullOrEmpty(gAppUpper.Body.n_dic[0].name))
                                {
                                    device_unknown_name = gAppUpper.Body.n_dic[0].name;
                                }

                                if (gAppUpper.Body.n_dic[0].dic.ContainsKey("ipAddr"))
                                {
                                    ipAddr = gAppUpper.Body.n_dic[0].dic["ipAddr"].ToString();
                                }

                                if (gAppUpper.Body.n_dic[0].dic.ContainsKey("port"))
                                {
                                    port = gAppUpper.Body.n_dic[0].dic["port"].ToString();
                                }

                                if (device_unknown_name != "device_unknown" || ipAddr == "" || port == "")
                                {
                                    //返回出错处理
                                    string errInfo = get_debug_info() + string.Format("获取device_unknown或ipAddr或port失败.");
                                    add_log_info(LogInfoType.EROR, errInfo, "Main", LogCategory.I);
                                    Logger.Trace(LogInfoType.EROR, errInfo, "Main", LogCategory.I);

                                    Fill_IMMS_Info(ref gAppUpper, AppMsgType.app_add_device_response, -1, errInfo, true, null, null);
                                    Send_Msg_2_AppCtrl_Upper(gAppUpper);
                                    break;                                   
                                }

                                noAssignedFlag = true;
                            }
                            else
                            {
                                noAssignedFlag = false;
                            }

                            #endregion

                            #endregion

                            #region 返回消息

                            if (noAssignedFlag == false)
                            {
                                //无需处理将未指派设备添加确定的设备中

                                int rtv = gDbHelper.domain_get_id_by_nameFullPath(parentFullPathName, ref affDomainId);
                                if (rtv == 0)
                                {
                                    rtv = gDbHelper.device_record_insert(affDomainId, name, mode);
                                }

                                Fill_IMMS_Info(ref gAppUpper, AppMsgType.app_add_device_response, rtv, gDbHelper.get_rtv_str(rtv), true, null, null);
                                Send_Msg_2_AppCtrl_Upper(gAppUpper);
                               
                                #region 重新获取gDicDeviceId

                                if (rtv == 0)
                                {
                                    if (0 == gDbHelper.domain_dictionary_info_join_get(ref gDicDeviceId))
                                    {
                                        add_log_info(LogInfoType.INFO, "gDicDeviceId -> 获取OK！", "Main", LogCategory.I);
                                        Logger.Trace(LogInfoType.INFO, "gDicDeviceId -> 获取OK！", "Main", LogCategory.I);
                                    }
                                    else
                                    {
                                        add_log_info(LogInfoType.INFO, "gDicDeviceId -> 获取FAILED！", "Main", LogCategory.I);
                                        Logger.Trace(LogInfoType.INFO, "gDicDeviceId -> 获取FAILED！", "Main", LogCategory.I);
                                    }
                                }

                                #endregion

                                break;
                            }
                            else
                            {
                                //处理将未指派设备添加确定的设备中                               
                                if ((int)RC.NO_EXIST == gDbHelper.device_unknown_record_exist(ipAddr, int.Parse(port)))
                                {
                                    string errInfo = string.Format("{0}:{1}对应的未指派设备不存在.", ipAddr, port);

                                    add_log_info(LogInfoType.EROR, errInfo, "Main", LogCategory.I);
                                    Logger.Trace(LogInfoType.EROR, errInfo, "Main", LogCategory.I);

                                    //设置未指派设备的全名失败
                                    Fill_IMMS_Info(ref gAppUpper, AppMsgType.app_add_device_response, -1, errInfo, true, "1", "2");
                                    Send_Msg_2_AppCtrl_Upper(gAppUpper);
                                    break;
                                }

                                #region 发下命令设置fullname           

                                string fullname = string.Format("{0}.{1}", parentFullPathName, name);

                                DataTable dt = new DataTable();
                                int rtv = gDbHelper.device_unknown_record_entity_get_by_ipaddr_port(ipAddr, int.Parse(port), ref dt);

                                if (((int)RC.SUCCESS != rtv) || (dt.Rows.Count == 0))
                                {
                                    string errInfo = get_debug_info() + "device_unknown_record_entity_get_by_ipaddr_port失败.";
                                    add_log_info(LogInfoType.EROR, errInfo, "Main", LogCategory.I);
                                    Logger.Trace(LogInfoType.EROR, errInfo, "Main", LogCategory.I);

                                    Fill_IMMS_Info(ref gAppUpper, AppMsgType.app_add_device_response, -1, errInfo, true, "1", "2");
                                    Send_Msg_2_AppCtrl_Upper(gAppUpper);
                                    break;
                                }

                                gAppUpper.ApInfo.SN = dt.Rows[0]["sn"].ToString();
                                gAppUpper.ApInfo.Fullname = fullname;
                                gAppUpper.ApInfo.IP = ipAddr;
                                gAppUpper.ApInfo.Port = int.Parse(port);
                                gAppUpper.ApInfo.Type = dt.Rows[0]["innerType"].ToString();

                                gAppUpper.Body.type = ApMsgType.set_parameter_request;
                                gAppUpper.MsgType = MsgType.CONFIG.ToString();

                                gAppUpper.Body.dic = new Dictionary<string, object>();
                                gAppUpper.Body.dic.Add("paramName", "CFG_FULL_NAME");            
                                gAppUpper.Body.dic.Add("paramValue", fullname);

                                //发送给ApController
                                Send_Msg_2_ApCtrl_Lower(gAppUpper);

                                #endregion

                                #region 启动超时计时器 

                                gTimerSetFullName = new TaskTimer();
                                gTimerSetFullName.Interval = DataController.TimerTimeOutInterval * 1000;

                                gTimerSetFullName.Id = 0;
                                gTimerSetFullName.Name = string.Format("{0}:{1}:{2}", "gTimerSetFullName", ipAddr, port);
                                gTimerSetFullName.MsgType = AppMsgType.app_add_device_response;
                                gTimerSetFullName.TimeOutFlag = false;
                                gTimerSetFullName.Imms = gAppUpper;

                                //保存信息
                                gTimerSetFullName.parentFullPathName = parentFullPathName;
                                gTimerSetFullName.devName = name;
                                gTimerSetFullName.mode = mode;
                                gTimerSetFullName.ipAddr = ipAddr;
                                gTimerSetFullName.port = port;

                                gTimerSetFullName.Elapsed += new System.Timers.ElapsedEventHandler(TimerFunc);
                                gTimerSetFullName.Start();

                                break;
                                #endregion
                            }

                            #endregion                            
                        }
                    case AppMsgType.app_del_device_request:
                        {
                            #region 获取信息

                            int affDomainId = -1;
                            string parentFullPathName = "";
                            string name = "";

                            if (gAppUpper.Body.dic.ContainsKey("parentFullPathName"))
                            {
                                parentFullPathName = gAppUpper.Body.dic["parentFullPathName"].ToString();
                            }

                            if (gAppUpper.Body.dic.ContainsKey("name"))
                            {
                                name = gAppUpper.Body.dic["name"].ToString();
                            }

                            if (parentFullPathName == "" || name == "")
                            {
                                add_log_info(LogInfoType.EROR, "app_del_device_request,参数有误", "Main", LogCategory.I);
                                Logger.Trace(LogInfoType.EROR, "app_del_device_request,参数有误", "Main", LogCategory.I);

                                //返回出错处理
                                gAppUpper.Body.type = AppMsgType.app_del_device_response;
                                gAppUpper.Body.dic = new Dictionary<string, object>();
                                gAppUpper.Body.dic.Add("ReturnCode", -1);
                                gAppUpper.Body.dic.Add("ReturnStr", get_debug_info() + "app_del_device_request,参数有误.");

                                Send_Msg_2_AppCtrl_Upper(gAppUpper);
                                break;
                            }


                            #endregion

                            #region 返回消息

                            int rtv = gDbHelper.domain_get_id_by_nameFullPath(parentFullPathName, ref affDomainId);
                            if (rtv == 0)
                            {
                                rtv = gDbHelper.device_record_delete(affDomainId, name);
                            }

                            gAppUpper.Body.type = AppMsgType.app_del_device_response;
                            gAppUpper.Body.dic = new Dictionary<string, object>();
                            gAppUpper.Body.dic.Add("ReturnCode", rtv);

                            if (rtv != 0)
                            {
                                gAppUpper.Body.dic.Add("ReturnStr", get_debug_info() + gDbHelper.get_rtv_str(rtv));
                            }
                            else
                            {
                                gAppUpper.Body.dic.Add("ReturnStr", gDbHelper.get_rtv_str(rtv));
                            }

                            Send_Msg_2_AppCtrl_Upper(gAppUpper);

                            #endregion

                            #region 重新获取gDicDeviceId

                            if (rtv == 0)
                            {
                                if (0 == gDbHelper.domain_dictionary_info_join_get(ref gDicDeviceId))
                                {
                                    add_log_info(LogInfoType.INFO, "gDicDeviceId -> 获取OK！", "Main", LogCategory.I);
                                    Logger.Trace(LogInfoType.INFO, "gDicDeviceId -> 获取OK！", "Main", LogCategory.I);
                                }
                                else
                                {
                                    add_log_info(LogInfoType.INFO, "gDicDeviceId -> 获取FAILED！", "Main", LogCategory.I);
                                    Logger.Trace(LogInfoType.INFO, "gDicDeviceId -> 获取FAILED！", "Main", LogCategory.I);
                                }
                            }

                            break;

                            #endregion                            
                        }
                    case AppMsgType.app_update_device_request:
                        {
                            #region 获取信息

                            int affDomainId = -1;
                            string parentFullPathName = "";
                            string name = "";

                            if (gAppUpper.Body.dic.ContainsKey("parentFullPathName"))
                            {
                                parentFullPathName = gAppUpper.Body.dic["parentFullPathName"].ToString();
                            }

                            if (gAppUpper.Body.dic.ContainsKey("name"))
                            {
                                name = gAppUpper.Body.dic["name"].ToString();
                            }

                            if (parentFullPathName == "" || name == "")
                            {
                                add_log_info(LogInfoType.EROR, "app_update_device_request,参数有误", "Main", LogCategory.I);
                                Logger.Trace(LogInfoType.EROR, "app_update_device_request,参数有误", "Main", LogCategory.I);

                                //返回出错处理
                                gAppUpper.Body.type = AppMsgType.app_update_device_response;
                                gAppUpper.Body.dic = new Dictionary<string, object>();
                                gAppUpper.Body.dic.Add("ReturnCode", -1);
                                gAppUpper.Body.dic.Add("ReturnStr", get_debug_info() + "app_update_device_request,参数有误.");

                                Send_Msg_2_AppCtrl_Upper(gAppUpper);
                                break;
                            }

                            if (gAppUpper.Body.n_dic == null || gAppUpper.Body.n_dic.Count == 0)
                            {
                                add_log_info(LogInfoType.EROR, "n_dic中没有信息,参数有误", "Main", LogCategory.I);
                                Logger.Trace(LogInfoType.EROR, "n_dic中没有信息,参数有误", "Main", LogCategory.I);

                                //返回出错处理
                                gAppUpper.Body.type = AppMsgType.app_update_device_response;
                                gAppUpper.Body.dic = new Dictionary<string, object>();
                                gAppUpper.Body.dic.Add("ReturnCode", -1);
                                gAppUpper.Body.dic.Add("ReturnStr", get_debug_info() + "n_dic中没有信息,参数有误.");

                                Send_Msg_2_AppCtrl_Upper(gAppUpper);
                                break;
                            }

                            //         "name":"电信FDD-band3",
                            //         "sn":"EN1800S116340039",
                            //         "ipAddr":"172.17.0.125",
                            //         "port":"12345",
                            //         "netmask":"255.255.255.0",
                            //         "mode":"FDD",
                            strDevice dev = new strDevice();

                            if (gAppUpper.Body.n_dic[0].dic.ContainsKey("name"))
                            {
                                if (gAppUpper.Body.n_dic[0].dic["name"].ToString() != "")
                                {
                                    dev.name = gAppUpper.Body.n_dic[0].dic["name"].ToString();
                                }
                            }

                            if (gAppUpper.Body.n_dic[0].dic.ContainsKey("sn"))
                            {
                                if (gAppUpper.Body.n_dic[0].dic["sn"].ToString() != "")
                                {
                                    dev.sn = gAppUpper.Body.n_dic[0].dic["sn"].ToString();
                                }
                            }                            

                            if (gAppUpper.Body.n_dic[0].dic.ContainsKey("ipAddr"))
                            {
                                if (gAppUpper.Body.n_dic[0].dic["ipAddr"].ToString() != "")
                                {
                                    dev.ipAddr = gAppUpper.Body.n_dic[0].dic["ipAddr"].ToString();
                                }
                            }

                            if (gAppUpper.Body.n_dic[0].dic.ContainsKey("port"))
                            {
                                if (gAppUpper.Body.n_dic[0].dic["port"].ToString() != "")
                                {
                                    dev.port = gAppUpper.Body.n_dic[0].dic["port"].ToString();
                                }
                            }

                            if (gAppUpper.Body.n_dic[0].dic.ContainsKey("netmask"))
                            {
                                if (gAppUpper.Body.n_dic[0].dic["netmask"].ToString() != "")
                                {
                                    dev.netmask = gAppUpper.Body.n_dic[0].dic["netmask"].ToString();
                                }
                            }

                            if (gAppUpper.Body.n_dic[0].dic.ContainsKey("mode"))
                            {
                                if (gAppUpper.Body.n_dic[0].dic["mode"].ToString() != "")
                                {
                                    dev.mode = gAppUpper.Body.n_dic[0].dic["mode"].ToString();
                                }
                            }

                            #endregion

                            #region 返回消息

                            int rtv = gDbHelper.domain_get_id_by_nameFullPath(parentFullPathName, ref affDomainId);
                            if (rtv == 0)
                            {
                                rtv = gDbHelper.device_record_update(affDomainId, name, dev);
                            }

                            gAppUpper.Body.type = AppMsgType.app_update_device_response;
                            gAppUpper.Body.dic = new Dictionary<string, object>();
                            gAppUpper.Body.dic.Add("ReturnCode", rtv);
                            if (rtv != 0)
                            {
                                gAppUpper.Body.dic.Add("ReturnStr", get_debug_info() + gDbHelper.get_rtv_str(rtv));
                            }
                            else
                            {
                                gAppUpper.Body.dic.Add("ReturnStr", gDbHelper.get_rtv_str(rtv));
                            }

                            gAppUpper.Body.n_dic = new List<Name_DIC_Struct>();

                            Send_Msg_2_AppCtrl_Upper(gAppUpper);
                            break;

                            #endregion         
                        }
                    case AppMsgType.app_get_device_detail_request:
                        {
                            #region 获取信息
                            
                            int rtv;
                            strDevice devInfo = new strDevice();
                            strApStatus apSts = new strApStatus();
                            
                            string name = "";
                            string devFullPathName = "";
                            string parentFullPathName = "";

                            if (gAppUpper.Body.dic.ContainsKey("parentFullPathName"))
                            {
                                parentFullPathName = gAppUpper.Body.dic["parentFullPathName"].ToString();
                            }

                            if (gAppUpper.Body.dic.ContainsKey("name"))
                            {
                                name = gAppUpper.Body.dic["name"].ToString();
                            }

                            if (parentFullPathName == "" || name == "")
                            {
                                //返回出错处理
                                string errInfo = get_debug_info() + string.Format("app_get_device_detail_request,参数有误");
                                add_log_info(LogInfoType.EROR, errInfo, "Main", LogCategory.I);
                                Logger.Trace(LogInfoType.EROR, errInfo, "Main", LogCategory.I);

                                Fill_IMMS_Info(ref gAppUpper, AppMsgType.app_get_device_detail_response, -1, errInfo, true, null, null);
                                Send_Msg_2_AppCtrl_Upper(gAppUpper);
                                break;
                            }

                            #endregion

                            #region 返回消息
                          
                            strDomian domian = new strDomian();
                            devFullPathName = string.Format("{0}.{1}", parentFullPathName, name);

                            if (!gDicDeviceId.ContainsKey(devFullPathName))
                            {
                                //返回出错处理    
                                string errInfo = string.Format("{0}:对应的设备ID在gDicDeviceId中找不到", devFullPathName);
                                add_log_info(LogInfoType.EROR, errInfo, "Main", LogCategory.I);
                                Logger.Trace(LogInfoType.EROR, errInfo, "Main", LogCategory.I);
                                
                                Fill_IMMS_Info(ref gAppUpper, AppMsgType.app_get_device_detail_response, -1, errInfo, true, null, null);
                                Send_Msg_2_AppCtrl_Upper(gAppUpper);               
                                break;
                            }
                            else
                            {
                                devInfo = gDicDeviceId[devFullPathName];

                                //通过名称全路径获取域对应记录的信息
                                rtv = gDbHelper.domain_record_get_by_nameFullPath(parentFullPathName, ref domian);
                                if (rtv != 0)
                                {
                                    string errInfo = string.Format("{0}:domain_record_get_by_nameFullPath出错", parentFullPathName);
                                    add_log_info(LogInfoType.EROR, errInfo, "Main", LogCategory.I);
                                    Logger.Trace(LogInfoType.EROR, errInfo, "Main", LogCategory.I);

                                    Fill_IMMS_Info(ref gAppUpper, AppMsgType.app_get_device_detail_response, -1, errInfo, true, null, null);
                                    Send_Msg_2_AppCtrl_Upper(gAppUpper);
                                    break;
                                }
                            }

                            Fill_IMMS_Info(ref gAppUpper, AppMsgType.app_get_device_detail_response, rtv, gDbHelper.get_rtv_str(rtv), true, null, null);

                            gAppUpper.Body.dic.Add("domainId", domian.id.ToString());
                            gAppUpper.Body.dic.Add("domainParentId", domian.parentId.ToString());
                            gAppUpper.Body.dic.Add("parentFullPathName", parentFullPathName);
                            gAppUpper.Body.dic.Add("name", name);

                            switch (devInfo.devMode)
                            {
                                case devMode.MODE_GSM:
                                    {
                                        #region GSM详细信息

                                        strGsmRfPara grp = new strGsmRfPara();
                                        rtv = gDbHelper.gsm_rf_para_record_get_by_devid(0, devInfo.id, ref grp);                                   
                                        if (rtv != 0)
                                        {
                                            string errInfo = string.Format("{0}:ap_status_record_get_by_devid出错", parentFullPathName);
                                            add_log_info(LogInfoType.EROR, errInfo, "Main", LogCategory.I);
                                            Logger.Trace(LogInfoType.EROR, errInfo, "Main", LogCategory.I);

                                            Fill_IMMS_Info(ref gAppUpper, AppMsgType.app_get_device_detail_response, -1, errInfo, true, null, null);
                                            Send_Msg_2_AppCtrl_Upper(gAppUpper);
                                            break;
                                        }

                                        gAppUpper.Body.n_dic = new List<Name_DIC_Struct>();
                                        Name_DIC_Struct ndic = new Name_DIC_Struct();
                                        ndic.name = "carry0";
                                        ndic.dic.Add("RADIO", grp.rfEnable);                                  
                                        gAppUpper.Body.n_dic.Add(ndic);


                                        grp = new strGsmRfPara();
                                        rtv = gDbHelper.gsm_rf_para_record_get_by_devid(1, devInfo.id, ref grp);
                                        if (rtv != 0)
                                        {
                                            string errInfo = string.Format("{0}:ap_status_record_get_by_devid出错", parentFullPathName);
                                            add_log_info(LogInfoType.EROR, errInfo, "Main", LogCategory.I);
                                            Logger.Trace(LogInfoType.EROR, errInfo, "Main", LogCategory.I);

                                            Fill_IMMS_Info(ref gAppUpper, AppMsgType.app_get_device_detail_response, -1, errInfo, true, null, null);
                                            Send_Msg_2_AppCtrl_Upper(gAppUpper);
                                            break;
                                        }
                                        
                                        ndic = new Name_DIC_Struct();
                                        ndic.name = "carry1";
                                        ndic.dic.Add("RADIO", grp.rfEnable);
                                        gAppUpper.Body.n_dic.Add(ndic);

                                        Send_Msg_2_AppCtrl_Upper(gAppUpper);                                     
                                        break;

                                        #endregion
                                    }
                                case devMode.MODE_GSM_V2:
                                    {
                                        #region GSM详细信息
                                   
                                        strGcMisc gm = new strGcMisc();
                                        rtv = gDbHelper.gc_misc_record_get_by_devid(0, devInfo.id, ref gm);
                                        if (rtv != 0)
                                        {
                                            string errInfo = string.Format("{0}:gc_misc_record_get_by_devid出错", parentFullPathName);
                                            add_log_info(LogInfoType.EROR, errInfo, "Main", LogCategory.I);
                                            Logger.Trace(LogInfoType.EROR, errInfo, "Main", LogCategory.I);

                                            Fill_IMMS_Info(ref gAppUpper, AppMsgType.app_get_device_detail_response, -1, errInfo, true, null, null);
                                            Send_Msg_2_AppCtrl_Upper(gAppUpper);
                                            break;
                                        }

                                        gAppUpper.Body.n_dic = new List<Name_DIC_Struct>();
                                        Name_DIC_Struct ndic = new Name_DIC_Struct();

                                        ndic.name = "carry0";
                                        ndic.dic.Add("SCTP", gm.SCTP);
                                        ndic.dic.Add("S1", gm.S1);
                                        ndic.dic.Add("GPS", gm.GPS);
                                        ndic.dic.Add("CELL", gm.CELL);
                                        ndic.dic.Add("SYNC", gm.SYNC);
                                        ndic.dic.Add("LICENSE", gm.LICENSE);
                                        ndic.dic.Add("RADIO", gm.RADIO);
                                        ndic.dic.Add("time", gm.time);
                                        gAppUpper.Body.n_dic.Add(ndic);

                                        gm = new strGcMisc();
                                        rtv = gDbHelper.gc_misc_record_get_by_devid(1, devInfo.id, ref gm);
                                        if (rtv != 0)
                                        {
                                            string errInfo = string.Format("{0}:gc_misc_record_get_by_devid出错", parentFullPathName);
                                            add_log_info(LogInfoType.EROR, errInfo, "Main", LogCategory.I);
                                            Logger.Trace(LogInfoType.EROR, errInfo, "Main", LogCategory.I);

                                            Fill_IMMS_Info(ref gAppUpper, AppMsgType.app_get_device_detail_response, -1, errInfo, true, null, null);
                                            Send_Msg_2_AppCtrl_Upper(gAppUpper);
                                            break;
                                        }

                                        gAppUpper.Body.n_dic = new List<Name_DIC_Struct>();
                                        ndic = new Name_DIC_Struct();

                                        ndic.name = "carry1";
                                        ndic.dic.Add("SCTP", gm.SCTP);
                                        ndic.dic.Add("S1", gm.S1);
                                        ndic.dic.Add("GPS", gm.GPS);
                                        ndic.dic.Add("CELL", gm.CELL);
                                        ndic.dic.Add("SYNC", gm.SYNC);
                                        ndic.dic.Add("LICENSE", gm.LICENSE);
                                        ndic.dic.Add("RADIO", gm.RADIO);
                                        ndic.dic.Add("time", gm.time);
                                        gAppUpper.Body.n_dic.Add(ndic);


                                        Send_Msg_2_AppCtrl_Upper(gAppUpper);
                                        break;

                                        #endregion
                                    }
                                case devMode.MODE_CDMA:
                                    {
                                        #region CDMA详细信息

                                        strGcMisc gm = new strGcMisc();
                                        rtv = gDbHelper.gc_misc_record_get_by_devid(-1, devInfo.id, ref gm);
                                        if (rtv != 0)
                                        {
                                            string errInfo = string.Format("{0}:gc_misc_record_get_by_devid出错", parentFullPathName);
                                            add_log_info(LogInfoType.EROR, errInfo, "Main", LogCategory.I);
                                            Logger.Trace(LogInfoType.EROR, errInfo, "Main", LogCategory.I);

                                            Fill_IMMS_Info(ref gAppUpper, AppMsgType.app_get_device_detail_response, -1, errInfo, true, null, null);
                                            Send_Msg_2_AppCtrl_Upper(gAppUpper);
                                            break;
                                        }

                                        gAppUpper.Body.n_dic = new List<Name_DIC_Struct>();
                                        Name_DIC_Struct ndic = new Name_DIC_Struct();

                                        ndic.name = "carry0";
                                        ndic.dic.Add("SCTP", gm.SCTP);
                                        ndic.dic.Add("S1", gm.S1);
                                        ndic.dic.Add("GPS", gm.GPS);
                                        ndic.dic.Add("CELL", gm.CELL);
                                        ndic.dic.Add("SYNC", gm.SYNC);
                                        ndic.dic.Add("LICENSE", gm.LICENSE);
                                        ndic.dic.Add("RADIO", gm.RADIO);
                                        ndic.dic.Add("time", gm.time);
                                        gAppUpper.Body.n_dic.Add(ndic);
                                        
                                        Send_Msg_2_AppCtrl_Upper(gAppUpper);
                                        break;

                                        #endregion                               
                                    }
                                case devMode.MODE_TD_SCDMA:
                                    {
                                        break;
                                    }
                                case devMode.MODE_WCDMA:
                                case devMode.MODE_LTE_FDD:
                                case devMode.MODE_LTE_TDD:                                
                                    {
                                        #region LTE详细信息

                                        rtv = gDbHelper.ap_status_record_get_by_devid(devInfo.id, ref apSts);
                                        if (rtv != 0)
                                        {
                                            string errInfo = string.Format("{0}:ap_status_record_get_by_devid出错", parentFullPathName);
                                            add_log_info(LogInfoType.EROR, errInfo, "Main", LogCategory.I);
                                            Logger.Trace(LogInfoType.EROR, errInfo, "Main", LogCategory.I);

                                            Fill_IMMS_Info(ref gAppUpper, AppMsgType.app_get_device_detail_response, -1, errInfo, true, null, null);
                                            Send_Msg_2_AppCtrl_Upper(gAppUpper);
                                            break;
                                        }

                                        gAppUpper.Body.n_dic = new List<Name_DIC_Struct>();
                                        Name_DIC_Struct ndic = new Name_DIC_Struct();

                                        ndic.dic.Add("SCTP", apSts.SCTP);
                                        ndic.dic.Add("S1", apSts.S1);
                                        ndic.dic.Add("GPS", apSts.GPS);
                                        ndic.dic.Add("CELL", apSts.CELL);
                                        ndic.dic.Add("SYNC", apSts.SYNC);
                                        ndic.dic.Add("LICENSE", apSts.LICENSE);
                                        ndic.dic.Add("RADIO", apSts.RADIO);
                                        ndic.dic.Add("time", apSts.time);
                                        ndic.dic.Add("wSelfStudy", devInfo.wSelfStudy);
                                        gAppUpper.Body.n_dic.Add(ndic);

                                        Send_Msg_2_AppCtrl_Upper(gAppUpper);
                                        break;

                                        #endregion
                                    }
                                case devMode.MODE_UNKNOWN:
                                    {
                                        #region 未知mode

                                        string errInfo = string.Format("mode有误");
                                        add_log_info(LogInfoType.EROR, errInfo, "Main", LogCategory.I);
                                        Logger.Trace(LogInfoType.EROR, errInfo, "Main", LogCategory.I);

                                        Fill_IMMS_Info(ref gAppUpper, AppMsgType.app_get_device_detail_response, -1, errInfo, true, null, null);
                                        Send_Msg_2_AppCtrl_Upper(gAppUpper);
                                        break;

                                        #endregion
                                    }
                            }

                            break;

                            #endregion                                         
                        }
                    case AppMsgType.app_all_province_request:
                        {
                            #region 获取所有省信息

                            List<Province> provinceList = new List<Province>();

                            int rtv = gDbHelper.db_getProvince_info(ref provinceList);

                            gAppUpper.Body.type = AppMsgType.app_all_province_response;
                            gAppUpper.Body.dic = new Dictionary<string, object>();
                            gAppUpper.Body.dic.Add("ReturnCode", rtv);
                           
                            if (rtv != 0)
                            {
                                gAppUpper.Body.dic.Add("ReturnStr", get_debug_info() + gDbHelper.get_rtv_str(rtv));
                            }
                            else
                            {
                                gAppUpper.Body.dic.Add("ReturnStr", gDbHelper.get_rtv_str(rtv));
                            }

                            gAppUpper.Body.n_dic = new List<Name_DIC_Struct>();

                            if (rtv != 0)
                            {
                                Send_Msg_2_AppCtrl_Upper(gAppUpper);
                                break;
                            }
                            else
                            {
                                gAppUpper.Body.dic.Add("ProvinceCount", provinceList.Count.ToString());
                                for (int i = 0; i < provinceList.Count; i++)
                                {
                                    Name_DIC_Struct ndic = new Name_DIC_Struct();

                                    ndic.name = i.ToString();
                                    ndic.dic.Add("provice_id", provinceList[i].provice_id);
                                    ndic.dic.Add("provice_name", provinceList[i].provice_name);

                                    gAppUpper.Body.n_dic.Add(ndic);
                                }

                                Send_Msg_2_AppCtrl_Upper(gAppUpper);
                                break;
                            }

                            #endregion                            
                        }
                    case AppMsgType.app_get_city_request:
                        {
                            #region 获取信息

                            string provice_id = "";

                            if (gAppUpper.Body.dic.ContainsKey("provice_id"))
                            {
                                provice_id = gAppUpper.Body.dic["provice_id"].ToString();
                            }

                            if (provice_id == "")
                            {
                                add_log_info(LogInfoType.EROR, "app_get_city_request,参数有误", "Main", LogCategory.I);
                                Logger.Trace(LogInfoType.EROR, "app_get_city_request,参数有误", "Main", LogCategory.I);

                                //返回出错处理
                                gAppUpper.Body.type = AppMsgType.app_get_city_response;
                                gAppUpper.Body.dic = new Dictionary<string, object>();
                                gAppUpper.Body.dic.Add("ReturnCode", -1);
                                gAppUpper.Body.dic.Add("ReturnStr", get_debug_info() + "app_get_city_request,参数有误.");

                                Send_Msg_2_AppCtrl_Upper(gAppUpper);
                                break;
                            }

                            #endregion

                            #region 获取对应城市信息

                            List<City> cityList = new List<City>();
                            int rtv = gDbHelper.db_getCity_info(ref cityList, provice_id);

                            gAppUpper.Body.type = AppMsgType.app_get_city_response;
                            gAppUpper.Body.dic = new Dictionary<string, object>();
                            gAppUpper.Body.dic.Add("ReturnCode", rtv);
                            if (rtv != 0)
                            {
                                gAppUpper.Body.dic.Add("ReturnStr", get_debug_info() + gDbHelper.get_rtv_str(rtv));
                            }
                            else
                            {
                                gAppUpper.Body.dic.Add("ReturnStr", gDbHelper.get_rtv_str(rtv));
                            }

                            gAppUpper.Body.n_dic = new List<Name_DIC_Struct>();

                            if (rtv != 0)
                            {
                                Send_Msg_2_AppCtrl_Upper(gAppUpper);
                                break;
                            }
                            else
                            {
                                gAppUpper.Body.dic.Add("CityCount", cityList.Count.ToString());
                                for (int i = 0; i < cityList.Count; i++)
                                {
                                    Name_DIC_Struct ndic = new Name_DIC_Struct();

                                    ndic.name = i.ToString();
                                    ndic.dic.Add("city_id", cityList[i].city_id);
                                    ndic.dic.Add("city_name", cityList[i].city_name);

                                    gAppUpper.Body.n_dic.Add(ndic);
                                }

                                Send_Msg_2_AppCtrl_Upper(gAppUpper);
                                break;
                            }

                            #endregion                            
                        }
                    case AppMsgType.app_get_distract_request:
                        {
                            #region 获取信息

                            string city_id = "";

                            if (gAppUpper.Body.dic.ContainsKey("city_id"))
                            {
                                city_id = gAppUpper.Body.dic["city_id"].ToString();
                            }

                            if (city_id == "")
                            {
                                add_log_info(LogInfoType.EROR, "app_get_distract_request,参数有误", "Main", LogCategory.I);
                                Logger.Trace(LogInfoType.EROR, "app_get_distract_request,参数有误", "Main", LogCategory.I);

                                //返回出错处理
                                gAppUpper.Body.type = AppMsgType.app_get_distract_response;
                                gAppUpper.Body.dic = new Dictionary<string, object>();
                                gAppUpper.Body.dic.Add("ReturnCode", -1);
                                gAppUpper.Body.dic.Add("ReturnStr", get_debug_info() + "app_get_distract_request,参数有误.");

                                Send_Msg_2_AppCtrl_Upper(gAppUpper);
                                break;
                            }

                            #endregion

                            #region 获取对应区信息

                            List<Distract> distractList = new List<Distract>();
                            int rtv = gDbHelper.db_getDistract_info(ref distractList, city_id);

                            gAppUpper.Body.type = AppMsgType.app_get_distract_response;
                            gAppUpper.Body.dic = new Dictionary<string, object>();
                            gAppUpper.Body.dic.Add("ReturnCode", rtv);
                            if (rtv != 0)
                            {
                                gAppUpper.Body.dic.Add("ReturnStr", get_debug_info() + gDbHelper.get_rtv_str(rtv));
                            }
                            else
                            {
                                gAppUpper.Body.dic.Add("ReturnStr", gDbHelper.get_rtv_str(rtv));
                            }

                            gAppUpper.Body.n_dic = new List<Name_DIC_Struct>();

                            if (rtv != 0)
                            {
                                Send_Msg_2_AppCtrl_Upper(gAppUpper);
                                break;
                            }
                            else
                            {
                                gAppUpper.Body.dic.Add("DistractCount", distractList.Count.ToString());
                                for (int i = 0; i < distractList.Count; i++)
                                {
                                    Name_DIC_Struct ndic = new Name_DIC_Struct();

                                    ndic.name = i.ToString();
                                    ndic.dic.Add("county_name", distractList[i].county_name);
                                    gAppUpper.Body.n_dic.Add(ndic);
                                }

                                Send_Msg_2_AppCtrl_Upper(gAppUpper);
                                break;
                            }

                            #endregion                            
                        }
                    case AppMsgType.app_all_user_request:
                        {
                            #region 返回信息

                            DataTable dt = new DataTable();
                            int rtv = gDbHelper.user_record_entity_get(ref dt);

                            gAppUpper.Body.type = AppMsgType.app_all_user_response;
                            gAppUpper.Body.dic = new Dictionary<string, object>();
                            gAppUpper.Body.dic.Add("ReturnCode", rtv);
                            if (rtv != 0)
                            {
                                gAppUpper.Body.dic.Add("ReturnStr", get_debug_info() + gDbHelper.get_rtv_str(rtv));
                            }
                            else
                            {
                                gAppUpper.Body.dic.Add("ReturnStr", gDbHelper.get_rtv_str(rtv));
                            }

                            gAppUpper.Body.n_dic = new List<Name_DIC_Struct>();

                            if (rtv == 0)
                            {
                                gAppUpper.Body.dic.Add("UserCount", dt.Rows.Count.ToString());

                                int i = 1;
                                foreach (DataRow dr in dt.Rows)
                                {
                                    Name_DIC_Struct ndic = new Name_DIC_Struct();
                                    ndic.name = i.ToString();

                                    if (string.IsNullOrEmpty(dr["name"].ToString()))
                                    {
                                        ndic.dic.Add("name", "null");
                                    }
                                    else
                                    {
                                        ndic.dic.Add("name", dr["name"].ToString());
                                    }

                                    if (string.IsNullOrEmpty(dr["des"].ToString()))
                                    {
                                        ndic.dic.Add("des", "null");
                                    }
                                    else
                                    {
                                        ndic.dic.Add("des", dr["des"].ToString());
                                    }

                                    gAppUpper.Body.n_dic.Add(ndic);
                                    i++;
                                }
                            }

                            Send_Msg_2_AppCtrl_Upper(gAppUpper);
                            break;

                            #endregion
                        }
                    case AppMsgType.app_add_user_request:
                        {
                            #region 获取信息

                            string name = "";
                            string psw = "";
                            string des = "";

                            if (gAppUpper.Body.dic.ContainsKey("name"))
                            {
                                name = gAppUpper.Body.dic["name"].ToString();
                            }

                            if (gAppUpper.Body.dic.ContainsKey("psw"))
                            {
                                psw = gAppUpper.Body.dic["psw"].ToString();
                            }

                            if (gAppUpper.Body.dic.ContainsKey("des"))
                            {
                                des = gAppUpper.Body.dic["des"].ToString();

                                if (string.IsNullOrEmpty(des))
                                {
                                    des = "null";
                                }
                            }
                            else
                            {
                                des = "null";
                            }

                            if (name == "" || psw == "" || des == "")
                            {
                                add_log_info(LogInfoType.EROR, "app_add_user_request,参数有误", "Main", LogCategory.I);
                                Logger.Trace(LogInfoType.EROR, "app_add_user_request,参数有误", "Main", LogCategory.I);

                                //返回出错处理
                                gAppUpper.Body.type = AppMsgType.app_add_user_response;
                                gAppUpper.Body.dic = new Dictionary<string, object>();
                                gAppUpper.Body.dic.Add("ReturnCode", -1);
                                gAppUpper.Body.dic.Add("ReturnStr", get_debug_info() + "app_add_user_request,参数有误.");

                                Send_Msg_2_AppCtrl_Upper(gAppUpper);
                                break;
                            }

                            #endregion

                            #region 返回消息

                            int rtv = gDbHelper.user_record_insert(name, psw, des);

                            gAppUpper.Body.type = AppMsgType.app_add_user_response;
                            gAppUpper.Body.dic = new Dictionary<string, object>();
                            gAppUpper.Body.dic.Add("ReturnCode", rtv);
                            if (rtv != 0)
                            {
                                gAppUpper.Body.dic.Add("ReturnStr", get_debug_info() + gDbHelper.get_rtv_str(rtv));
                            }
                            else
                            {
                                gAppUpper.Body.dic.Add("ReturnStr", gDbHelper.get_rtv_str(rtv));
                            }

                            Send_Msg_2_AppCtrl_Upper(gAppUpper);
                            break;

                            #endregion                   
                        }
                    case AppMsgType.app_del_user_request:
                        {
                            #region 获取信息

                            string name = "";

                            if (gAppUpper.Body.dic.ContainsKey("name"))
                            {
                                name = gAppUpper.Body.dic["name"].ToString();
                            }

                            if (name == "")
                            {
                                add_log_info(LogInfoType.EROR, "app_del_user_request,参数有误", "Main", LogCategory.I);
                                Logger.Trace(LogInfoType.EROR, "app_del_user_request,参数有误", "Main", LogCategory.I);

                                //返回出错处理
                                gAppUpper.Body.type = AppMsgType.app_del_user_response;
                                gAppUpper.Body.dic = new Dictionary<string, object>();
                                gAppUpper.Body.dic.Add("ReturnCode", -1);
                                gAppUpper.Body.dic.Add("ReturnStr", get_debug_info() + "app_del_user_request,参数有误.");

                                Send_Msg_2_AppCtrl_Upper(gAppUpper);
                                break;
                            }

                            #endregion

                            #region 返回消息

                            int rtv = gDbHelper.user_record_delete(name);

                            gAppUpper.Body.type = AppMsgType.app_del_user_response;
                            gAppUpper.Body.dic = new Dictionary<string, object>();
                            gAppUpper.Body.dic.Add("ReturnCode", rtv);
                            if (rtv != 0)
                            {
                                gAppUpper.Body.dic.Add("ReturnStr", get_debug_info() + gDbHelper.get_rtv_str(rtv));
                            }
                            else
                            {
                                gAppUpper.Body.dic.Add("ReturnStr", gDbHelper.get_rtv_str(rtv));
                            }

                            Send_Msg_2_AppCtrl_Upper(gAppUpper);
                            break;

                            #endregion                                             
                        }
                    case AppMsgType.app_modify_user_psw_request:
                        {
                            #region 获取信息

                            string name = "";
                            string oldPasswd = "";
                            string newPasswd = "";

                            if (gAppUpper.Body.dic.ContainsKey("name"))
                            {
                                name = gAppUpper.Body.dic["name"].ToString();
                            }

                            if (gAppUpper.Body.dic.ContainsKey("oldPasswd"))
                            {
                                oldPasswd = gAppUpper.Body.dic["oldPasswd"].ToString();
                            }

                            if (gAppUpper.Body.dic.ContainsKey("newPasswd"))
                            {
                                newPasswd = gAppUpper.Body.dic["newPasswd"].ToString();
                            }

                            if (name == "" || oldPasswd == "" || newPasswd == "")
                            {
                                add_log_info(LogInfoType.EROR, "app_modify_user_psw_request,参数有误", "Main", LogCategory.I);
                                Logger.Trace(LogInfoType.EROR, "app_modify_user_psw_request,参数有误", "Main", LogCategory.I);

                                //返回出错处理
                                gAppUpper.Body.type = AppMsgType.app_modify_user_psw_response;
                                gAppUpper.Body.dic = new Dictionary<string, object>();
                                gAppUpper.Body.dic.Add("ReturnCode", -1);
                                gAppUpper.Body.dic.Add("ReturnStr", get_debug_info() + "app_modify_user_psw_request,参数有误.");

                                Send_Msg_2_AppCtrl_Upper(gAppUpper);
                                break;
                            }

                            #endregion

                            #region 返回消息

                            int rtv = gDbHelper.user_record_update(name, oldPasswd, newPasswd);

                            gAppUpper.Body.type = AppMsgType.app_modify_user_psw_response;
                            gAppUpper.Body.dic = new Dictionary<string, object>();
                            gAppUpper.Body.dic.Add("ReturnCode", rtv);
                            if (rtv != 0)
                            {
                                gAppUpper.Body.dic.Add("ReturnStr", get_debug_info() + gDbHelper.get_rtv_str(rtv));
                            }
                            else
                            {
                                gAppUpper.Body.dic.Add("ReturnStr", gDbHelper.get_rtv_str(rtv));
                            }

                            Send_Msg_2_AppCtrl_Upper(gAppUpper);
                            break;

                            #endregion                                             
                        }
                    case AppMsgType.app_all_privilege_request:
                        {
                            #region 返回信息

                            DataTable dt = new DataTable();
                            int rtv = gDbHelper.privilege_record_entity_get(ref dt);

                            gAppUpper.Body.type = AppMsgType.app_all_privilege_response;
                            gAppUpper.Body.dic = new Dictionary<string, object>();
                            gAppUpper.Body.dic.Add("ReturnCode", rtv);
                            if (rtv != 0)
                            {
                                gAppUpper.Body.dic.Add("ReturnStr", get_debug_info() + gDbHelper.get_rtv_str(rtv));
                            }
                            else
                            {
                                gAppUpper.Body.dic.Add("ReturnStr", gDbHelper.get_rtv_str(rtv));
                            }

                            gAppUpper.Body.n_dic = new List<Name_DIC_Struct>();

                            if (rtv == 0)
                            {
                                gAppUpper.Body.dic.Add("PrivilegeCount", dt.Rows.Count.ToString());

                                int i = 1;
                                foreach (DataRow dr in dt.Rows)
                                {
                                    Name_DIC_Struct ndic = new Name_DIC_Struct();
                                    ndic.name = i.ToString();

                                    if (string.IsNullOrEmpty(dr["priId"].ToString()))
                                    {
                                        ndic.dic.Add("priId", "null");
                                    }
                                    else
                                    {
                                        ndic.dic.Add("priId", dr["priId"].ToString());
                                    }

                                    if (string.IsNullOrEmpty(dr["funName"].ToString()))
                                    {
                                        ndic.dic.Add("funName", "null");
                                    }
                                    else
                                    {
                                        ndic.dic.Add("funName", dr["funName"].ToString());
                                    }

                                    if (string.IsNullOrEmpty(dr["aliasName"].ToString()))
                                    {
                                        ndic.dic.Add("aliasName", "null");
                                    }
                                    else
                                    {
                                        ndic.dic.Add("aliasName", dr["aliasName"].ToString());
                                    }

                                    if (string.IsNullOrEmpty(dr["des"].ToString()))
                                    {
                                        ndic.dic.Add("des", "null");
                                    }
                                    else
                                    {
                                        ndic.dic.Add("des", dr["des"].ToString());
                                    }

                                    gAppUpper.Body.n_dic.Add(ndic);
                                    i++;
                                }
                            }

                            Send_Msg_2_AppCtrl_Upper(gAppUpper);
                            break;

                            #endregion
                        }
                    case AppMsgType.app_add_privilege_request:
                        {
                            #region 获取信息     

                            string funName = "";
                            string aliasName = "";
                            string des = "";

                            if (gAppUpper.Body.dic.ContainsKey("funName"))
                            {
                                funName = gAppUpper.Body.dic["funName"].ToString();
                            }

                            if (gAppUpper.Body.dic.ContainsKey("aliasName"))
                            {
                                aliasName = gAppUpper.Body.dic["aliasName"].ToString();
                                if (string.IsNullOrEmpty(aliasName))
                                {
                                    aliasName = "null";
                                }
                            }
                            else
                            {
                                aliasName = "null";
                            }

                            if (gAppUpper.Body.dic.ContainsKey("des"))
                            {
                                des = gAppUpper.Body.dic["des"].ToString();
                                if (string.IsNullOrEmpty(des))
                                {
                                    des = "null";
                                }
                            }
                            else
                            {
                                des = "null";
                            }

                            if (funName == "" || aliasName == "" || des == "")
                            {
                                add_log_info(LogInfoType.EROR, "app_add_privilege_request,参数有误", "Main", LogCategory.I);
                                Logger.Trace(LogInfoType.EROR, "app_add_privilege_request,参数有误", "Main", LogCategory.I);

                                //返回出错处理
                                gAppUpper.Body.type = AppMsgType.app_add_privilege_response;
                                gAppUpper.Body.dic = new Dictionary<string, object>();
                                gAppUpper.Body.dic.Add("ReturnCode", -1);
                                gAppUpper.Body.dic.Add("ReturnStr", get_debug_info() + "app_add_privilege_request,参数有误.");

                                Send_Msg_2_AppCtrl_Upper(gAppUpper);
                                break;
                            }

                            #endregion

                            #region 返回消息

                            int rtv = gDbHelper.privilege_record_insert(funName, aliasName, des);

                            gAppUpper.Body.type = AppMsgType.app_add_privilege_response;
                            gAppUpper.Body.dic = new Dictionary<string, object>();
                            gAppUpper.Body.dic.Add("ReturnCode", rtv);
                            if (rtv != 0)
                            {
                                gAppUpper.Body.dic.Add("ReturnStr", get_debug_info() + gDbHelper.get_rtv_str(rtv));
                            }
                            else
                            {
                                gAppUpper.Body.dic.Add("ReturnStr", gDbHelper.get_rtv_str(rtv));
                            }

                            Send_Msg_2_AppCtrl_Upper(gAppUpper);
                            break;

                            #endregion
                        }
                    case AppMsgType.app_del_privilege_request:
                        {
                            #region 获取信息

                            string funName = "";

                            if (gAppUpper.Body.dic.ContainsKey("funName"))
                            {
                                funName = gAppUpper.Body.dic["funName"].ToString();
                            }

                            if (funName == "")
                            {
                                add_log_info(LogInfoType.EROR, "app_del_privilege_request,参数有误", "Main", LogCategory.I);
                                Logger.Trace(LogInfoType.EROR, "app_del_privilege_request,参数有误", "Main", LogCategory.I);

                                //返回出错处理
                                gAppUpper.Body.type = AppMsgType.app_del_privilege_response;
                                gAppUpper.Body.dic = new Dictionary<string, object>();
                                gAppUpper.Body.dic.Add("ReturnCode", -1);
                                gAppUpper.Body.dic.Add("ReturnStr", get_debug_info() + "app_del_privilege_request,参数有误.");

                                Send_Msg_2_AppCtrl_Upper(gAppUpper);
                                break;
                            }

                            #endregion

                            #region 返回消息

                            int rtv = gDbHelper.privilege_record_delete(funName);

                            gAppUpper.Body.type = AppMsgType.app_del_user_response;
                            gAppUpper.Body.dic = new Dictionary<string, object>();
                            gAppUpper.Body.dic.Add("ReturnCode", rtv);
                            if (rtv != 0)
                            {
                                gAppUpper.Body.dic.Add("ReturnStr", get_debug_info() + gDbHelper.get_rtv_str(rtv));
                            }
                            else
                            {
                                gAppUpper.Body.dic.Add("ReturnStr", gDbHelper.get_rtv_str(rtv));
                            }

                            Send_Msg_2_AppCtrl_Upper(gAppUpper);
                            break;

                            #endregion                                             
                        }
                    case AppMsgType.app_all_usr_group_request:
                        {
                            #region 返回信息

                            DataTable dt = new DataTable();

                            //usrRoleId,usrName,roleName,des
                            int rtv = gDbHelper.userrole_record_entity_get(ref dt);

                            gAppUpper.Body.type = AppMsgType.app_all_usr_group_response;
                            gAppUpper.Body.dic = new Dictionary<string, object>();
                            gAppUpper.Body.dic.Add("ReturnCode", rtv);
                            if (rtv != 0)
                            {
                                gAppUpper.Body.dic.Add("ReturnStr", get_debug_info() + gDbHelper.get_rtv_str(rtv));
                            }
                            else
                            {
                                gAppUpper.Body.dic.Add("ReturnStr", gDbHelper.get_rtv_str(rtv));
                            }

                            gAppUpper.Body.n_dic = new List<Name_DIC_Struct>();

                            if (rtv == 0)
                            {
                                gAppUpper.Body.dic.Add("UsrGroupCount", dt.Rows.Count.ToString());
                                gAppUpper.Body.n_dic = new List<Name_DIC_Struct>();

                                foreach (DataRow dr in dt.Rows)
                                {
                                    Name_DIC_Struct ndic = new Name_DIC_Struct();

                                    if (string.IsNullOrEmpty(dr["usrRoleId"].ToString()))
                                    {
                                        ndic.name = "null";
                                    }
                                    else
                                    {
                                        ndic.name = dr["usrRoleId"].ToString();
                                    }

                                    if (string.IsNullOrEmpty(dr["usrName"].ToString()))
                                    {
                                        ndic.dic.Add("usrName", "null");
                                    }
                                    else
                                    {
                                        ndic.dic.Add("usrName", dr["usrName"].ToString());
                                    }

                                    if (string.IsNullOrEmpty(dr["roleName"].ToString()))
                                    {
                                        ndic.dic.Add("roleName", "null");
                                    }
                                    else
                                    {
                                        ndic.dic.Add("roleName", dr["roleName"].ToString());
                                    }

                                    if (string.IsNullOrEmpty(dr["des"].ToString()))
                                    {
                                        ndic.dic.Add("des", "null");
                                    }
                                    else
                                    {
                                        ndic.dic.Add("des", dr["des"].ToString());
                                    }

                                    gAppUpper.Body.n_dic.Add(ndic);
                                }
                            }

                            Send_Msg_2_AppCtrl_Upper(gAppUpper);
                            break;

                            #endregion
                        }
                    case AppMsgType.app_add_usr_group_request:
                        {
                            #region 获取信息

                            string usrName = "";
                            string roleName = "";
                            string des = "";

                            if (gAppUpper.Body.dic.ContainsKey("usrName"))
                            {
                                usrName = gAppUpper.Body.dic["usrName"].ToString();
                            }

                            if (gAppUpper.Body.dic.ContainsKey("roleName"))
                            {
                                roleName = gAppUpper.Body.dic["roleName"].ToString();
                            }

                            if (gAppUpper.Body.dic.ContainsKey("des"))
                            {
                                des = gAppUpper.Body.dic["des"].ToString();

                                if (string.IsNullOrEmpty(des))
                                {
                                    des = "null";
                                }
                            }
                            else
                            {
                                des = "null";
                            }

                            if (usrName == "" || roleName == "" || des == "")
                            {
                                add_log_info(LogInfoType.EROR, "app_add_usr_group_request,参数有误", "Main", LogCategory.I);
                                Logger.Trace(LogInfoType.EROR, "app_add_usr_group_request,参数有误", "Main", LogCategory.I);

                                //返回出错处理
                                gAppUpper.Body.type = AppMsgType.app_add_usr_group_response;
                                gAppUpper.Body.dic = new Dictionary<string, object>();
                                gAppUpper.Body.dic.Add("ReturnCode", -1);
                                gAppUpper.Body.dic.Add("ReturnStr", get_debug_info() + "app_add_usr_group_request,参数有误.");

                                Send_Msg_2_AppCtrl_Upper(gAppUpper);
                                break;
                            }

                            #endregion

                            #region 返回消息

                            int rtv = gDbHelper.userrole_record_insert(usrName, roleName, des);

                            gAppUpper.Body.type = AppMsgType.app_add_usr_group_response;
                            gAppUpper.Body.dic = new Dictionary<string, object>();
                            gAppUpper.Body.dic.Add("ReturnCode", rtv);
                            if (rtv != 0)
                            {
                                gAppUpper.Body.dic.Add("ReturnStr", get_debug_info() + gDbHelper.get_rtv_str(rtv));
                            }
                            else
                            {
                                gAppUpper.Body.dic.Add("ReturnStr", gDbHelper.get_rtv_str(rtv));
                            }

                            Send_Msg_2_AppCtrl_Upper(gAppUpper);
                            break;

                            #endregion                   
                        }
                    case AppMsgType.app_del_usr_group_request:
                        {
                            #region 获取信息

                            string usrName = "";
                            string roleName = "";

                            if (gAppUpper.Body.dic.ContainsKey("usrName"))
                            {
                                usrName = gAppUpper.Body.dic["usrName"].ToString();
                            }

                            if (gAppUpper.Body.dic.ContainsKey("roleName"))
                            {
                                roleName = gAppUpper.Body.dic["roleName"].ToString();
                            }

                            if (usrName == "" || roleName == "")
                            {
                                add_log_info(LogInfoType.EROR, "app_del_usr_group_request,参数有误", "Main", LogCategory.I);
                                Logger.Trace(LogInfoType.EROR, "app_del_usr_group_request,参数有误", "Main", LogCategory.I);

                                //返回出错处理
                                gAppUpper.Body.type = AppMsgType.app_del_usr_group_response;
                                gAppUpper.Body.dic = new Dictionary<string, object>();
                                gAppUpper.Body.dic.Add("ReturnCode", -1);
                                gAppUpper.Body.dic.Add("ReturnStr", get_debug_info() + "app_del_usr_group_request,参数有误.");

                                Send_Msg_2_AppCtrl_Upper(gAppUpper);
                                break;
                            }

                            #endregion

                            #region 返回消息

                            int rtv = gDbHelper.userrole_record_delete(usrName, roleName);

                            gAppUpper.Body.type = AppMsgType.app_del_usr_group_response;
                            gAppUpper.Body.dic = new Dictionary<string, object>();
                            gAppUpper.Body.dic.Add("ReturnCode", rtv);
                            if (rtv != 0)
                            {
                                gAppUpper.Body.dic.Add("ReturnStr", get_debug_info() + gDbHelper.get_rtv_str(rtv));
                            }
                            else
                            {
                                gAppUpper.Body.dic.Add("ReturnStr", gDbHelper.get_rtv_str(rtv));
                            }

                            Send_Msg_2_AppCtrl_Upper(gAppUpper);
                            break;

                            #endregion                   
                        }
                    case AppMsgType.app_all_group_privilege_request:
                        {
                            #region 返回信息

                            DataTable dt = new DataTable();
                            int rtv = gDbHelper.roleprivilege_record_entity_get(ref dt);

                            gAppUpper.Body.type = AppMsgType.app_all_group_privilege_response;
                            gAppUpper.Body.dic = new Dictionary<string, object>();
                            gAppUpper.Body.dic.Add("ReturnCode", rtv);
                            if (rtv != 0)
                            {
                                gAppUpper.Body.dic.Add("ReturnStr", get_debug_info() + gDbHelper.get_rtv_str(rtv));
                            }
                            else
                            {
                                gAppUpper.Body.dic.Add("ReturnStr", gDbHelper.get_rtv_str(rtv));
                            }

                            gAppUpper.Body.n_dic = new List<Name_DIC_Struct>();

                            if (rtv == 0)
                            {
                                gAppUpper.Body.dic.Add("GroupPriCount", dt.Rows.Count.ToString());

                                //rolePriId,roleName,priIdSet,des
                                foreach (DataRow dr in dt.Rows)
                                {
                                    Name_DIC_Struct ndic = new Name_DIC_Struct();

                                    if (string.IsNullOrEmpty(dr["roleName"].ToString()))
                                    {
                                        ndic.name = "null";
                                    }
                                    else
                                    {
                                        ndic.name = dr["roleName"].ToString();
                                    }

                                    if (string.IsNullOrEmpty(dr["priIdSet"].ToString()))
                                    {
                                        ndic.dic.Add("priIdSet", "null");
                                    }
                                    else
                                    {
                                        ndic.dic.Add("priIdSet", dr["priIdSet"].ToString());
                                    }

                                    if (string.IsNullOrEmpty(dr["des"].ToString()))
                                    {
                                        ndic.dic.Add("des", "null");
                                    }
                                    else
                                    {
                                        ndic.dic.Add("des", dr["des"].ToString());
                                    }

                                    gAppUpper.Body.n_dic.Add(ndic);
                                }
                            }

                            Send_Msg_2_AppCtrl_Upper(gAppUpper);
                            break;

                            #endregion
                        }
                    case AppMsgType.app_add_group_privilege_request:
                        {
                            #region 获取信息

                            //   "roleName":"RoleSA",
                            //   "priIdSet":"1,2,3,4,5,6,7",
                            //   "des":"备注"

                            string roleName = "";
                            string priIdSet = "";
                            string des = "";

                            if (gAppUpper.Body.dic.ContainsKey("roleName"))
                            {
                                roleName = gAppUpper.Body.dic["roleName"].ToString();
                            }

                            if (gAppUpper.Body.dic.ContainsKey("priIdSet"))
                            {
                                priIdSet = gAppUpper.Body.dic["priIdSet"].ToString();
                            }

                            if (gAppUpper.Body.dic.ContainsKey("des"))
                            {
                                des = gAppUpper.Body.dic["des"].ToString();

                                if (string.IsNullOrEmpty(des))
                                {
                                    des = "null";
                                }
                            }
                            else
                            {
                                des = "null";
                            }

                            if (roleName == "" || priIdSet == "" || des == "")
                            {
                                add_log_info(LogInfoType.EROR, "app_add_group_privilege_request,参数有误", "Main", LogCategory.I);
                                Logger.Trace(LogInfoType.EROR, "app_add_group_privilege_request,参数有误", "Main", LogCategory.I);

                                //返回出错处理
                                gAppUpper.Body.type = AppMsgType.app_add_group_privilege_response;
                                gAppUpper.Body.dic = new Dictionary<string, object>();
                                gAppUpper.Body.dic.Add("ReturnCode", -1);
                                gAppUpper.Body.dic.Add("ReturnStr", get_debug_info() + "app_add_group_privilege_request,参数有误.");

                                Send_Msg_2_AppCtrl_Upper(gAppUpper);
                                break;
                            }

                            #endregion

                            #region 返回消息

                            int rtv = gDbHelper.roleprivilege_record_insert(roleName, priIdSet, des);

                            gAppUpper.Body.type = AppMsgType.app_add_group_privilege_response;
                            gAppUpper.Body.dic = new Dictionary<string, object>();
                            gAppUpper.Body.dic.Add("ReturnCode", rtv);
                            if (rtv != 0)
                            {
                                gAppUpper.Body.dic.Add("ReturnStr", get_debug_info() + gDbHelper.get_rtv_str(rtv));
                            }
                            else
                            {
                                gAppUpper.Body.dic.Add("ReturnStr", gDbHelper.get_rtv_str(rtv));
                            }

                            Send_Msg_2_AppCtrl_Upper(gAppUpper);
                            break;

                            #endregion                   
                        }
                    case AppMsgType.app_del_group_privilege_request:
                        {
                            #region 获取信息

                            string roleName = "";

                            if (gAppUpper.Body.dic.ContainsKey("roleName"))
                            {
                                roleName = gAppUpper.Body.dic["roleName"].ToString();
                            }

                            if (roleName == "")
                            {
                                add_log_info(LogInfoType.EROR, "app_del_group_privilege_request,参数有误", "Main", LogCategory.I);
                                Logger.Trace(LogInfoType.EROR, "app_del_group_privilege_request,参数有误", "Main", LogCategory.I);

                                //返回出错处理
                                gAppUpper.Body.type = AppMsgType.app_del_group_privilege_response;
                                gAppUpper.Body.dic = new Dictionary<string, object>();
                                gAppUpper.Body.dic.Add("ReturnCode", -1);
                                gAppUpper.Body.dic.Add("ReturnStr", get_debug_info() + "app_del_group_privilege_request,参数有误.");

                                Send_Msg_2_AppCtrl_Upper(gAppUpper);
                                break;
                            }

                            #endregion

                            #region 返回消息

                            int rtv = gDbHelper.roleprivilege_record_delete(roleName);

                            gAppUpper.Body.type = AppMsgType.app_del_group_privilege_response;
                            gAppUpper.Body.dic = new Dictionary<string, object>();
                            gAppUpper.Body.dic.Add("ReturnCode", rtv);
                            if (rtv != 0)
                            {
                                gAppUpper.Body.dic.Add("ReturnStr", get_debug_info() + gDbHelper.get_rtv_str(rtv));
                            }
                            else
                            {
                                gAppUpper.Body.dic.Add("ReturnStr", gDbHelper.get_rtv_str(rtv));
                            }

                            Send_Msg_2_AppCtrl_Upper(gAppUpper);
                            break;

                            #endregion                   
                        }
                    case AppMsgType.app_update_group_privilege_request:
                        {
                            #region 获取信息

                            //   "roleName":"RoleSA",
                            //   "priIdSet":"1,2,3,4,5,6,7",
                            //   "des":"备注"

                            string roleName = "";
                            string priIdSet = "";
                            string des = "";

                            if (gAppUpper.Body.dic.ContainsKey("roleName"))
                            {
                                roleName = gAppUpper.Body.dic["roleName"].ToString();
                            }

                            if (gAppUpper.Body.dic.ContainsKey("priIdSet"))
                            {
                                priIdSet = gAppUpper.Body.dic["priIdSet"].ToString();
                            }

                            if (gAppUpper.Body.dic.ContainsKey("des"))
                            {
                                des = gAppUpper.Body.dic["des"].ToString();

                                if (string.IsNullOrEmpty(des))
                                {
                                    des = "null";
                                }
                            }
                            else
                            {
                                des = "null";
                            }

                            if (roleName == "" || priIdSet == "" || des == "")
                            {
                                add_log_info(LogInfoType.EROR, "app_update_group_privilege_request,参数有误", "Main", LogCategory.I);
                                Logger.Trace(LogInfoType.EROR, "app_update_group_privilege_request,参数有误", "Main", LogCategory.I);

                                //返回出错处理
                                gAppUpper.Body.type = AppMsgType.app_update_group_privilege_response;
                                gAppUpper.Body.dic = new Dictionary<string, object>();
                                gAppUpper.Body.dic.Add("ReturnCode", -1);
                                gAppUpper.Body.dic.Add("ReturnStr", get_debug_info() + "app_update_group_privilege_request,参数有误.");

                                Send_Msg_2_AppCtrl_Upper(gAppUpper);
                                break;
                            }

                            #endregion

                            #region 返回消息

                            int rtv = gDbHelper.roleprivilege_record_update(roleName, priIdSet, des);

                            gAppUpper.Body.type = AppMsgType.app_update_group_privilege_response;
                            gAppUpper.Body.dic = new Dictionary<string, object>();
                            gAppUpper.Body.dic.Add("ReturnCode", rtv);
                            if (rtv != 0)
                            {
                                gAppUpper.Body.dic.Add("ReturnStr", get_debug_info() + gDbHelper.get_rtv_str(rtv));
                            }
                            else
                            {
                                gAppUpper.Body.dic.Add("ReturnStr", gDbHelper.get_rtv_str(rtv));
                            }

                            Send_Msg_2_AppCtrl_Upper(gAppUpper);
                            break;

                            #endregion                   
                        }
                    case AppMsgType.app_all_usr_domain_request:
                        {
                            #region 获取信息

                            DataTable dt = new DataTable();
                            int rtv = gDbHelper.userdomain_record_entity_get(ref dt);

                            if ((int)RC.SUCCESS != rtv)
                            {
                                string errInfo = get_debug_info() + gDbHelper.get_rtv_str(rtv);
                                add_log_info(LogInfoType.EROR, errInfo, "Main", LogCategory.I);
                                Logger.Trace(LogInfoType.EROR, errInfo, "Main", LogCategory.I);

                                Fill_IMMS_Info(ref gAppUpper, AppMsgType.app_all_usr_domain_response, -1, errInfo, true, null, null);
                                Send_Msg_2_AppCtrl_Upper(gAppUpper);
                                break;
                            }

                            Fill_IMMS_Info(ref gAppUpper, AppMsgType.app_all_usr_domain_response, rtv, gDbHelper.get_rtv_str(rtv), true, null, null);
                            gAppUpper.Body.dic.Add("UsrDomaiCount", dt.Rows.Count.ToString());

                            //usrDomainId,usrName,domainIdSet,des
                            foreach (DataRow dr in dt.Rows)
                            {
                                Name_DIC_Struct ndic = new Name_DIC_Struct();

                                if (string.IsNullOrEmpty(dr["usrName"].ToString()))
                                {
                                    ndic.name = "null";
                                }
                                else
                                {
                                    ndic.name = dr["usrName"].ToString();
                                }

                                if (string.IsNullOrEmpty(dr["domainIdSet"].ToString()))
                                {
                                    ndic.dic.Add("domainIdSet", "null");
                                }
                                else
                                {
                                    string nameFullPathSet = "";
                                    if (0 != change_domain_id_2_nameFullPath(dr["domainIdSet"].ToString(), ref nameFullPathSet))
                                    {
                                        add_log_info(LogInfoType.EROR, "change_domain_id_2_nameFullPath出错", "Main", LogCategory.I);
                                        Logger.Trace(LogInfoType.EROR, "change_domain_id_2_nameFullPath出错", "Main", LogCategory.I);
                                        continue;
                                    }
                                    else
                                    {
                                        ndic.dic.Add("domainIdSet", nameFullPathSet);
                                    }                                    
                                }

                                if (string.IsNullOrEmpty(dr["des"].ToString()))
                                {
                                    ndic.dic.Add("des", "null");
                                }
                                else
                                {
                                    ndic.dic.Add("des", dr["des"].ToString());
                                }

                                gAppUpper.Body.n_dic.Add(ndic);
                            }

                            Send_Msg_2_AppCtrl_Upper(gAppUpper);
                            break;

                            #endregion
                        }
                    case AppMsgType.app_add_usr_domain_request:
                        {
                            #region 获取信息

                            //   "usrName":"root",
                            //   "domainIdSet":"9,18",
                            //   "des":"添加用户root的域集合"

                            string usrName = "";
                            string domainIdSet = "";
                            string des = " ";

                            if (gAppUpper.Body.dic.ContainsKey("usrName"))
                            {
                                usrName = gAppUpper.Body.dic["usrName"].ToString();
                            }

                            if (gAppUpper.Body.dic.ContainsKey("domainIdSet"))
                            {
                                domainIdSet = gAppUpper.Body.dic["domainIdSet"].ToString();
                            }

                            if (gAppUpper.Body.dic.ContainsKey("des"))
                            {                          
                                if (!string.IsNullOrEmpty(gAppUpper.Body.dic["des"].ToString()))
                                {
                                    des = gAppUpper.Body.dic["des"].ToString();
                                }
                            }
                    

                            if (usrName == "" || domainIdSet == "")
                            {               
                                //返回出错处理                        
                                string errInfo = string.Format("app_add_usr_domain_request,参数有误");
                                add_log_info(LogInfoType.EROR, errInfo, "Main", LogCategory.I);
                                Logger.Trace(LogInfoType.EROR, errInfo, "Main", LogCategory.I);

                                Fill_IMMS_Info(ref gAppUpper, AppMsgType.app_add_usr_domain_response, -1, errInfo, true,null,null);                            
                                Send_Msg_2_AppCtrl_Upper(gAppUpper);
                                break;
                            }

                            #endregion

                            #region 返回消息

                            string idSet = "";

                            if (0 != change_domain_nameFullPath_2_id(domainIdSet, ref idSet))
                            {
                                //返回出错处理                        
                                string errInfo = string.Format("change_domain_nameFullPath_2_id,出错");
                                add_log_info(LogInfoType.EROR, errInfo, "Main", LogCategory.I);
                                Logger.Trace(LogInfoType.EROR, errInfo, "Main", LogCategory.I);

                                Fill_IMMS_Info(ref gAppUpper, AppMsgType.app_add_usr_domain_response, -1, errInfo, true, null, null);
                                Send_Msg_2_AppCtrl_Upper(gAppUpper);
                                break;
                            }

                            int rtv = gDbHelper.userdomain_record_insert(usrName, idSet, des);

                            Fill_IMMS_Info(ref gAppUpper, AppMsgType.app_add_usr_domain_response, rtv, gDbHelper.get_rtv_str(rtv), true, null, null);                       
                            Send_Msg_2_AppCtrl_Upper(gAppUpper);
                            break;

                            #endregion                   
                        }
                    case AppMsgType.app_del_usr_domain_request:
                        {
                            #region 获取信息

                            string usrName = "";

                            if (gAppUpper.Body.dic.ContainsKey("usrName"))
                            {
                                usrName = gAppUpper.Body.dic["usrName"].ToString();
                            }

                            if (usrName == "")
                            {
                                //返回出错处理
                                string errInfo = get_debug_info() + string.Format("app_del_usr_domain_request,参数有误");
                                add_log_info(LogInfoType.EROR, errInfo, "Main", LogCategory.I);
                                Logger.Trace(LogInfoType.EROR, errInfo, "Main", LogCategory.I);

                                Fill_IMMS_Info(ref gAppUpper, AppMsgType.app_del_usr_domain_response, -1, errInfo, true, null, null);                            
                                Send_Msg_2_AppCtrl_Upper(gAppUpper);
                                break;
                            }

                            #endregion

                            #region 返回消息

                            int rtv = gDbHelper.userdomain_record_delete(usrName);

                            Fill_IMMS_Info(ref gAppUpper, AppMsgType.app_del_usr_domain_response, rtv, gDbHelper.get_rtv_str(rtv), true, null, null);                            
                            Send_Msg_2_AppCtrl_Upper(gAppUpper);
                            break;

                            #endregion                   
                        }
                    case AppMsgType.app_update_usr_domain_request:
                        {
                            #region 获取信息

                            //   "usrName":"root",
                            //   "domainIdSet":"9,18",
                            //   "des":"添加用户root的域集合"

                            string usrName = "";
                            string domainIdSet = "";
                            string des = " ";

                            if (gAppUpper.Body.dic.ContainsKey("usrName"))
                            {
                                usrName = gAppUpper.Body.dic["usrName"].ToString();
                            }

                            if (gAppUpper.Body.dic.ContainsKey("domainIdSet"))
                            {
                                domainIdSet = gAppUpper.Body.dic["domainIdSet"].ToString();
                            }

                            if (gAppUpper.Body.dic.ContainsKey("des"))
                            {             
                                if (!string.IsNullOrEmpty(gAppUpper.Body.dic["des"].ToString()))
                                {
                                    des = gAppUpper.Body.dic["des"].ToString();
                                }
                            }                  

                            if (usrName == "" || domainIdSet == "")
                            {
                                //返回出错处理
                                string errInfo = get_debug_info() + string.Format("app_update_usr_domain_request,参数有误");
                                add_log_info(LogInfoType.EROR, errInfo, "Main", LogCategory.I);
                                Logger.Trace(LogInfoType.EROR, errInfo, "Main", LogCategory.I);

                                Fill_IMMS_Info(ref gAppUpper, AppMsgType.app_update_usr_domain_response, -1, errInfo, true, null, null);
                                Send_Msg_2_AppCtrl_Upper(gAppUpper);
                                break;
                            }

                            #endregion

                            #region 返回消息

                            string idSet = "";
                            if (0 != change_domain_nameFullPath_2_id(domainIdSet, ref idSet))
                            {
                                //返回出错处理                        
                                string errInfo = string.Format("change_domain_nameFullPath_2_id,出错");
                                add_log_info(LogInfoType.EROR, errInfo, "Main", LogCategory.I);
                                Logger.Trace(LogInfoType.EROR, errInfo, "Main", LogCategory.I);

                                Fill_IMMS_Info(ref gAppUpper, AppMsgType.app_update_usr_domain_response, -1, errInfo, true, null, null);
                                Send_Msg_2_AppCtrl_Upper(gAppUpper);
                                break;
                            }

                            int rtv = gDbHelper.userdomain_record_update(usrName, idSet, des);

                            Fill_IMMS_Info(ref gAppUpper, AppMsgType.app_update_usr_domain_response, rtv, gDbHelper.get_rtv_str(rtv), true, null, null);
                            Send_Msg_2_AppCtrl_Upper(gAppUpper);                            
                            break;

                            #endregion                   
                        }
                    case AppMsgType.app_add_bwlist_request:
                        {
                            #region 获取信息

                            // 
                            // GSW和WCDMA的设备，IMSI和IMEI可同时设置
                            //
                            // "bwListApplyTo":"device",                                    //黑白名单适用于那种类型，device或者domain                          
                            // "deviceFullPathName":"设备.深圳.福田.中心广场.西北监控.电信TDD",  //bwListApplyTo为device时起作用
                            // "domainFullPathName":"设备.深圳.福田",                         //bwListApplyTo为domain时起作用

                            int rtv = -1;
                            strDevice devInfo = new strDevice();                          

                            string bwListApplyTo = "";                          
                            string deviceFullPathName = "";
                            string domainFullPathName = "";

                            if (gAppUpper.Body.dic.ContainsKey("bwListApplyTo"))
                            {
                                bwListApplyTo = gAppUpper.Body.dic["bwListApplyTo"].ToString();
                            }

                            if (gAppUpper.Body.dic.ContainsKey("deviceFullPathName"))
                            {
                                deviceFullPathName = gAppUpper.Body.dic["deviceFullPathName"].ToString();
                            }

                            if (gAppUpper.Body.dic.ContainsKey("domainFullPathName"))
                            {
                                domainFullPathName = gAppUpper.Body.dic["domainFullPathName"].ToString();
                            }
                            
                            if (bwListApplyTo == "device")
                            {
                                if (!gDicDeviceId.ContainsKey(deviceFullPathName))
                                {
                                    string errInfo = string.Format("{0}:对应的设备ID在gDicDeviceId中找不到", get_debug_info() + deviceFullPathName);
                                    add_log_info(LogInfoType.EROR, errInfo, "Main", LogCategory.I);
                                    Logger.Trace(LogInfoType.EROR, errInfo, "Main", LogCategory.I);

                                    Fill_IMMS_Info(ref gAppUpper, AppMsgType.app_add_bwlist_response, -1, errInfo, true, "1", "2");                       
                                    Send_Msg_2_AppCtrl_Upper(gAppUpper);
                                    break;
                                }
                                else
                                {
                                    devInfo = gDicDeviceId[deviceFullPathName];
                                    gBwListSetInfo.devId = devInfo.id.ToString();
                                }
                            }
                            else if (bwListApplyTo == "domain")
                            {
                                if ((int)RC.NO_EXIST == gDbHelper.domain_record_exist(domainFullPathName))
                                {
                                    string errInfo = get_debug_info() + domainFullPathName + ":记录不存在";
                                    add_log_info(LogInfoType.EROR, errInfo, "Main", LogCategory.I);
                                    Logger.Trace(LogInfoType.EROR, errInfo, "Main", LogCategory.I);

                                    Fill_IMMS_Info(ref gAppUpper, AppMsgType.app_add_bwlist_response, -1, errInfo, true, "1", "2");
                                    Send_Msg_2_AppCtrl_Upper(gAppUpper);
                                    break;
                                }
                                else
                                {
                                    int domainId = -1;
                                    if ((int)RC.SUCCESS == gDbHelper.domain_get_id_by_nameFullPath(domainFullPathName, ref domainId))
                                    {
                                        gBwListSetInfo.domainId = domainId.ToString();
                                    }
                                    else
                                    {
                                        string errInfo = get_debug_info() + domainFullPathName + ":获取ID出错.";
                                        add_log_info(LogInfoType.EROR, errInfo, "Main", LogCategory.I);
                                        Logger.Trace(LogInfoType.EROR, errInfo, "Main", LogCategory.I);

                                        Fill_IMMS_Info(ref gAppUpper, AppMsgType.app_add_bwlist_response, -1, errInfo, true, "1", "2");                                        
                                        Send_Msg_2_AppCtrl_Upper(gAppUpper);
                                        break;
                                    }
                                }
                            }
                            else
                            {
                                string errInfo = get_debug_info() + "bwListApplyTo必须为device或domain.";
                                add_log_info(LogInfoType.EROR, errInfo, "Main", LogCategory.I);
                                Logger.Trace(LogInfoType.EROR, errInfo, "Main", LogCategory.I);

                                Fill_IMMS_Info(ref gAppUpper, AppMsgType.app_add_bwlist_response, -1, errInfo, true, "1", "2");
                                Send_Msg_2_AppCtrl_Upper(gAppUpper);
                                break;
                            }

                            if (gAppUpper.Body.n_dic == null || gAppUpper.Body.n_dic.Count == 0)
                            {
                                string errInfo = get_debug_info() + "n_dic中没有信息,参数有误";
                                add_log_info(LogInfoType.EROR, errInfo, "Main", LogCategory.I);
                                Logger.Trace(LogInfoType.EROR, errInfo, "Main", LogCategory.I);

                                Fill_IMMS_Info(ref gAppUpper, AppMsgType.app_add_bwlist_response, -1, errInfo, true, "1", "2");
                                Send_Msg_2_AppCtrl_Upper(gAppUpper);
                                break;
                            }

                            if (-1 == get_bwlist_info(gAppUpper, ref gBwListSetInfo.listBwInfo, gBwListSetInfo.devId, gBwListSetInfo.domainId))
                            {
                                string errInfo = get_debug_info() + "从n_dic中获取黑白名单列表失败.";
                                add_log_info(LogInfoType.EROR, errInfo, "Main", LogCategory.I);
                                Logger.Trace(LogInfoType.EROR, errInfo, "Main", LogCategory.I);

                                Fill_IMMS_Info(ref gAppUpper, AppMsgType.app_add_bwlist_response, -1, errInfo, true, "1", "2");                
                                Send_Msg_2_AppCtrl_Upper(gAppUpper);
                                break;
                            }
                           
                            #endregion

                            #region 返回消息

                            if (bwListApplyTo == "device")
                            {
                                #region 设备的处理

                                gBwListSetInfo.listDevId = new List<int>();
                                gBwListSetInfo.listDevFullName = new List<string>();

                                gBwListSetInfo.listDevId.Add(devInfo.id);
                                gBwListSetInfo.listDevFullName.Add(deviceFullPathName);

                                /*
                                 * 转发给APController处理，然后根据
                                 * APController返回的结果决定是否进行存库.
                                 */
                                strDevice strDev = new strDevice();
                                rtv = gDbHelper.device_record_entity_get_by_devid(devInfo.id, ref strDev);
                                if (rtv == 0)
                                {
                                    if (string.IsNullOrEmpty(strDev.online) || strDev.online == "0")                                   
                                    {
                                        string errInfo = get_debug_info() + "设备离线";
                                        add_log_info(LogInfoType.EROR, errInfo, "Main", LogCategory.I);
                                        Logger.Trace(LogInfoType.EROR, errInfo, "Main", LogCategory.I);

                                        Fill_IMMS_Info(ref gAppUpper, AppMsgType.app_add_bwlist_response, -1, errInfo, true, "1", "2");                                        
                                        Send_Msg_2_AppCtrl_Upper(gAppUpper);
                                        break;
                                    }

                                    //只发给上线的AP
                                    gAppUpper.ApInfo.SN = strDev.sn;
                                    gAppUpper.ApInfo.Fullname = deviceFullPathName;
                                    gAppUpper.ApInfo.IP = strDev.ipAddr;
                                    gAppUpper.ApInfo.Port = int.Parse(strDev.port);
                                    gAppUpper.ApInfo.Type = strDev.innerType;
                                    gAppUpper.MsgType = MsgType.CONFIG.ToString();                                    

                                    //发送给ApController
                                    Send_Msg_2_ApCtrl_Lower(gAppUpper);

                                    #region 启动超时计时器 

                                    gTimerBlackWhite = new TaskTimer();
                                    gTimerBlackWhite.Interval = DataController.TimerTimeOutInterval * 1000;

                                    gTimerBlackWhite.Id = 0;
                                    gTimerBlackWhite.Name = string.Format("{0}:{1}:{2}", "gTimerBlackWhite", strDev.ipAddr, strDev.port);
                                    gTimerBlackWhite.MsgType = AppMsgType.app_add_device_response;
                                    gTimerBlackWhite.TimeOutFlag = false;
                                    gTimerBlackWhite.Imms = gAppUpper;

                                    //保存信息                               
                                    gTimerBlackWhite.ipAddr = strDev.ipAddr;
                                    gTimerBlackWhite.port = strDev.port;

                                    gTimerBlackWhite.Elapsed += new System.Timers.ElapsedEventHandler(TimerFunc);
                                    gTimerBlackWhite.Start();
                                 
                                    #endregion

                                    break;
                                }
                                else
                                {
                                    string errInfo = get_debug_info() + gDbHelper.get_rtv_str(rtv);
                                    add_log_info(LogInfoType.EROR, errInfo, "Main", LogCategory.I);
                                    Logger.Trace(LogInfoType.EROR, errInfo, "Main", LogCategory.I);

                                    Fill_IMMS_Info(ref gAppUpper, AppMsgType.app_add_bwlist_response, -1, errInfo, true, "1", "2");                           
                                    Send_Msg_2_AppCtrl_Upper(gAppUpper);
                                    break;
                                }

                                #endregion
                            }
                            else
                            {
                                #region 域的处理

                                /*
                                 * 2017-07-24 对域的处理暂时不使用超时计时器
                                 */ 

                                gBwListSetInfo.listDevId = new List<int>();
                                gBwListSetInfo.listDevFullName = new List<string>();                           

                                //黑白名单关联到域                             
                                rtv = gDbHelper.domain_record_device_id_list_get(domainFullPathName, ref gBwListSetInfo.listDevId);
                                if (rtv == (int)RC.SUCCESS)
                                {
                                    foreach(int id in gBwListSetInfo.listDevId)
                                    {
                                        foreach (KeyValuePair<string, strDevice> kvp in gDicDeviceId)
                                        {
                                            if (kvp.Value.id == id)
                                            {
                                                gBwListSetInfo.listDevFullName.Add(kvp.Key);
                                                break;
                                            }
                                        }
                                    }

                                    //插入域所属的黑白名单
                                    for (int i = 0; i < gBwListSetInfo.listBwInfo.Count; i++)
                                    {
                                        gDbHelper.bwlist_record_insert_affdomainid(gBwListSetInfo.listBwInfo[i], int.Parse(gBwListSetInfo.domainId));
                                    }

                                    for (int i= 0;i< gBwListSetInfo.listDevId.Count;i++)
                                    {
                                        strDevice strDev = new strDevice();
                                        rtv = gDbHelper.device_record_entity_get_by_devid(gBwListSetInfo.listDevId[i], ref strDev);
                                        if (rtv == 0)
                                        {
                                            if (string.IsNullOrEmpty(strDev.online) || strDev.online == "0")
                                            {
                                                continue;
                                            }

                                            //只发给上线的AP
                                            gAppUpper.ApInfo.SN = strDev.sn;
                                            gAppUpper.ApInfo.Fullname = gBwListSetInfo.listDevFullName[i];
                                            gAppUpper.ApInfo.IP = strDev.ipAddr;
                                            gAppUpper.ApInfo.Port = int.Parse(strDev.port);
                                            gAppUpper.ApInfo.Type = strDev.innerType;
                                            gAppUpper.MsgType = MsgType.CONFIG.ToString();                                            

                                            //发送给ApController
                                            Send_Msg_2_ApCtrl_Lower(gAppUpper);                                            
                                        }                                       
                                    }                                    
                                }
                                else
                                {
                                    string errInfo = string.Format("domain_record_device_id_list_get出错:{0}.", gDbHelper.get_rtv_str(rtv));
                                    add_log_info(LogInfoType.EROR, errInfo, "Main", LogCategory.I);
                                    Logger.Trace(LogInfoType.EROR, errInfo, "Main", LogCategory.I);

                                    Fill_IMMS_Info(ref gAppUpper, AppMsgType.app_add_bwlist_response, -1, errInfo, true, "1", "2");
                                    Send_Msg_2_AppCtrl_Upper(gAppUpper);
                                    break;
                                }

                                #endregion
                            }

                            break;

                            #endregion
                        }
                    case AppMsgType.app_del_bwlist_request:
                        {
                            #region 获取信息

                            //   "bwListApplyTo":"device",                                    //黑白名单适用于那种类型，device或者domain  
                            //   "deviceFullPathName":"设备.深圳.福田.中心广场.西北监控.电信TDD",  //bwListApplyTo为device时起作用
                            //   "domainFullPathName":"设备.深圳.福田",                         //bwListApplyTo为domain时起作用

                            strDevice devInfo = new strDevice();
                            int rtv = -1;                      

                            string bwListApplyTo = "";                
                            string deviceFullPathName = "";
                            string domainFullPathName = "";

                            if (gAppUpper.Body.dic.ContainsKey("bwListApplyTo"))
                            {
                                bwListApplyTo = gAppUpper.Body.dic["bwListApplyTo"].ToString();
                            }

                            if (gAppUpper.Body.dic.ContainsKey("deviceFullPathName"))
                            {
                                deviceFullPathName = gAppUpper.Body.dic["deviceFullPathName"].ToString();
                            }

                            if (gAppUpper.Body.dic.ContainsKey("domainFullPathName"))
                            {
                                domainFullPathName = gAppUpper.Body.dic["domainFullPathName"].ToString();
                            }

                            if (bwListApplyTo == "device")
                            {
                                deviceFullPathName = gAppUpper.Body.dic["deviceFullPathName"].ToString();
                                if (!gDicDeviceId.ContainsKey(deviceFullPathName))
                                {
                                    string errInfo = string.Format("{0}:对应的设备ID在gDicDeviceId中找不到", get_debug_info() + deviceFullPathName);
                                    add_log_info(LogInfoType.EROR, errInfo, "Main", LogCategory.I);
                                    Logger.Trace(LogInfoType.EROR, errInfo, "Main", LogCategory.I);

                                    Fill_IMMS_Info(ref gAppUpper, AppMsgType.app_del_bwlist_response, -1, errInfo, true, "1", "2");
                                    Send_Msg_2_AppCtrl_Upper(gAppUpper);
                                    break;
                                }
                                else
                                {
                                    devInfo = gDicDeviceId[deviceFullPathName];
                                    gBwListSetInfo.devId = devInfo.id.ToString();
                                }

                            }
                            else if (bwListApplyTo == "domain")
                            {
                                domainFullPathName = gAppUpper.Body.dic["domainFullPathName"].ToString();
                                if ((int)RC.NO_EXIST == gDbHelper.domain_record_exist(domainFullPathName))
                                {
                                    string errInfo = get_debug_info() + domainFullPathName + ":记录不存在";
                                    add_log_info(LogInfoType.EROR, errInfo, "Main", LogCategory.I);
                                    Logger.Trace(LogInfoType.EROR, errInfo, "Main", LogCategory.I);

                                    Fill_IMMS_Info(ref gAppUpper, AppMsgType.app_del_bwlist_response, -1, errInfo, true, "1", "2");
                                    Send_Msg_2_AppCtrl_Upper(gAppUpper);
                                    break;
                                }
                                else
                                {
                                    int domainId = -1;
                                    if ((int)RC.SUCCESS == gDbHelper.domain_get_id_by_nameFullPath(domainFullPathName, ref domainId))
                                    {
                                        gBwListSetInfo.domainId = domainId.ToString();
                                    }
                                    else
                                    {
                                        string errInfo = get_debug_info() + domainFullPathName + ":获取ID出错.";
                                        add_log_info(LogInfoType.EROR, errInfo, "Main", LogCategory.I);
                                        Logger.Trace(LogInfoType.EROR, errInfo, "Main", LogCategory.I);

                                        Fill_IMMS_Info(ref gAppUpper, AppMsgType.app_del_bwlist_response, -1, errInfo, true, "1", "2");
                                        Send_Msg_2_AppCtrl_Upper(gAppUpper);
                                        break;
                                    }
                                }
                            }
                            else
                            {
                                string errInfo = get_debug_info() + "bwListApplyTo必须为device或domain.";
                                add_log_info(LogInfoType.EROR, errInfo, "Main", LogCategory.I);
                                Logger.Trace(LogInfoType.EROR, errInfo, "Main", LogCategory.I);

                                Fill_IMMS_Info(ref gAppUpper, AppMsgType.app_del_bwlist_response, -1, errInfo, true, "1", "2");
                                Send_Msg_2_AppCtrl_Upper(gAppUpper);
                                break;
                            }

                            if (gAppUpper.Body.n_dic == null || gAppUpper.Body.n_dic.Count == 0)
                            {
                                string errInfo = get_debug_info() + "n_dic中没有信息,参数有误.";
                                add_log_info(LogInfoType.EROR, errInfo, "Main", LogCategory.I);
                                Logger.Trace(LogInfoType.EROR, errInfo, "Main", LogCategory.I);

                                Fill_IMMS_Info(ref gAppUpper, AppMsgType.app_del_bwlist_response, -1, errInfo, true, "1", "2");
                                Send_Msg_2_AppCtrl_Upper(gAppUpper);
                                break;
                            }

                            if (-1 == get_bwlist_info(gAppUpper, ref gBwListSetInfo.listBwInfo, gBwListSetInfo.devId, gBwListSetInfo.domainId))
                            {
                                string errInfo = get_debug_info() + "从n_dic中获取黑白名单列表失败.";
                                add_log_info(LogInfoType.EROR, errInfo, "Main", LogCategory.I);
                                Logger.Trace(LogInfoType.EROR, errInfo, "Main", LogCategory.I);

                                Fill_IMMS_Info(ref gAppUpper, AppMsgType.app_del_bwlist_response, -1, errInfo, true, "1", "2");
                                Send_Msg_2_AppCtrl_Upper(gAppUpper);
                                break;
                            }

                            #endregion

                            #region 返回消息

                            if (bwListApplyTo == "device")
                            {
                                #region 设备的处理

                                gBwListSetInfo.listDevId = new List<int>();
                                gBwListSetInfo.listDevFullName = new List<string>();

                                gBwListSetInfo.listDevId.Add(devInfo.id);
                                gBwListSetInfo.listDevFullName.Add(deviceFullPathName);

                                /*
                                 * 转发给APController处理，然后根据
                                 * APController返回的结果决定是否进行存库.
                                 */
                                strDevice strDev = new strDevice();
                                rtv = gDbHelper.device_record_entity_get_by_devid(devInfo.id, ref strDev);
                                if (rtv == 0)
                                {
                                    if (string.IsNullOrEmpty(strDev.online) || strDev.online == "0")
                                    {
                                        string errInfo = get_debug_info() + "设备离线";
                                        add_log_info(LogInfoType.EROR, errInfo, "Main", LogCategory.I);
                                        Logger.Trace(LogInfoType.EROR, errInfo, "Main", LogCategory.I);

                                        Fill_IMMS_Info(ref gAppUpper, AppMsgType.app_del_bwlist_response, -1, errInfo, true, "1", "2");
                                        Send_Msg_2_AppCtrl_Upper(gAppUpper);
                                        break;
                                    }

                                    //只发给上线的AP
                                    gAppUpper.ApInfo.SN = strDev.sn;
                                    gAppUpper.ApInfo.Fullname = deviceFullPathName;
                                    gAppUpper.ApInfo.IP = strDev.ipAddr;
                                    gAppUpper.ApInfo.Port = int.Parse(strDev.port);
                                    gAppUpper.ApInfo.Type = strDev.innerType;
                                    gAppUpper.MsgType = MsgType.CONFIG.ToString();

                                    //发送给ApController
                                    Send_Msg_2_ApCtrl_Lower(gAppUpper);

                                    #region 启动超时计时器 

                                    gTimerBlackWhite = new TaskTimer();
                                    gTimerBlackWhite.Interval = DataController.TimerTimeOutInterval * 1000;

                                    gTimerBlackWhite.Id = 0;
                                    gTimerBlackWhite.Name = string.Format("{0}:{1}:{2}", "gTimerBlackWhite", strDev.ipAddr, strDev.port);
                                    gTimerBlackWhite.MsgType = AppMsgType.app_del_bwlist_response;
                                    gTimerBlackWhite.TimeOutFlag = false;
                                    gTimerBlackWhite.Imms = gAppUpper;

                                    //保存信息                               
                                    gTimerBlackWhite.ipAddr = strDev.ipAddr;
                                    gTimerBlackWhite.port = strDev.port;

                                    gTimerBlackWhite.Elapsed += new System.Timers.ElapsedEventHandler(TimerFunc);
                                    gTimerBlackWhite.Start();

                                    #endregion

                                    break;
                                }
                                else
                                {
                                    string errInfo = get_debug_info() + gDbHelper.get_rtv_str(rtv);
                                    add_log_info(LogInfoType.EROR, errInfo, "Main", LogCategory.I);
                                    Logger.Trace(LogInfoType.EROR, errInfo, "Main", LogCategory.I);

                                    Fill_IMMS_Info(ref gAppUpper, AppMsgType.app_del_bwlist_response, -1, errInfo, true, "1", "2");             
                                    Send_Msg_2_AppCtrl_Upper(gAppUpper);                                   
                                    break;
                                }

                                #endregion                                                                                   
                            }
                            else
                            {
                                #region 域的处理

                                /*
                                 * 2017-07-24 对域的处理暂时不使用超时计时器
                                 */

                                gBwListSetInfo.listDevId = new List<int>();
                                gBwListSetInfo.listDevFullName = new List<string>();

                                //黑白名单关联到域                             
                                rtv = gDbHelper.domain_record_device_id_list_get(domainFullPathName, ref gBwListSetInfo.listDevId);
                                if (rtv == (int)RC.SUCCESS)
                                {
                                    foreach (int id in gBwListSetInfo.listDevId)
                                    {
                                        foreach (KeyValuePair<string, strDevice> kvp in gDicDeviceId)
                                        {
                                            if (kvp.Value.id == id)
                                            {
                                                gBwListSetInfo.listDevFullName.Add(kvp.Key);
                                                break;
                                            }
                                        }
                                    }

                                    //删除域所属的黑白名单
                                    for (int i = 0; i < gBwListSetInfo.listBwInfo.Count; i++)
                                    {
                                        gDbHelper.bwlist_record_imsi_delete_affdomainid(gBwListSetInfo.listBwInfo[i].imsi, gBwListSetInfo.listBwInfo[i].bwFlag,int.Parse(gBwListSetInfo.domainId));
                                    }

                                    for (int i = 0; i < gBwListSetInfo.listDevId.Count; i++)
                                    {
                                        strDevice strDev = new strDevice();
                                        rtv = gDbHelper.device_record_entity_get_by_devid(gBwListSetInfo.listDevId[i], ref strDev);
                                        if (rtv == 0)
                                        {
                                            if (strDev.online == "0")
                                            {
                                                continue;
                                            }

                                            //只发给上线的AP
                                            gAppUpper.ApInfo.SN = strDev.sn;
                                            gAppUpper.ApInfo.Fullname = gBwListSetInfo.listDevFullName[i];
                                            gAppUpper.ApInfo.IP = strDev.ipAddr;
                                            gAppUpper.ApInfo.Port = int.Parse(strDev.port);
                                            gAppUpper.ApInfo.Type = strDev.innerType;
                                            gAppUpper.MsgType = MsgType.CONFIG.ToString();

                                            //发送给ApController
                                            Send_Msg_2_ApCtrl_Lower(gAppUpper);
                                        }
                                    }
                                }
                                else
                                {
                                    string errInfo = string.Format("domain_record_device_id_list_get出错:{0}.", gDbHelper.get_rtv_str(rtv));
                                    add_log_info(LogInfoType.EROR, errInfo, "Main", LogCategory.I);
                                    Logger.Trace(LogInfoType.EROR, errInfo, "Main", LogCategory.I);

                                    Fill_IMMS_Info(ref gAppUpper, AppMsgType.app_del_bwlist_response, -1, errInfo, true, "1", "2");                           
                                    Send_Msg_2_AppCtrl_Upper(gAppUpper);
                                    break;
                                }

                                #endregion
                            }

                            break;

                            #endregion
                        }
                    case AppMsgType.app_all_bwlist_request:
                        {
                            #region 获取信息

                            string appId = Get_App_Info(gAppUpper); 

                            // 
                            // GSW和WCDMA的设备，IMSI和IMEI可同时设置
                            //
                            // "bwListApplyTo":"device",                                    //黑白名单适用于那种类型，device或者domain                          
                            // "deviceFullPathName":"设备.深圳.福田.中心广场.西北监控.电信TDD",  //bwListApplyTo为device时起作用
                            // "domainFullPathName":"设备.深圳.福田",                         //bwListApplyTo为domain时起作用

                            int rtv = -1;
                            strDevice devInfo = new strDevice();                     

                            string bwListApplyTo = "";
                            string deviceFullPathName = "";
                            string domainFullPathName = "";                            

                            if (gAppUpper.Body.dic.ContainsKey("bwListApplyTo"))
                            {
                                bwListApplyTo = gAppUpper.Body.dic["bwListApplyTo"].ToString();
                            }

                            if (gAppUpper.Body.dic.ContainsKey("deviceFullPathName"))
                            {
                                deviceFullPathName = gAppUpper.Body.dic["deviceFullPathName"].ToString();
                            }

                            if (gAppUpper.Body.dic.ContainsKey("domainFullPathName"))
                            {
                                domainFullPathName = gAppUpper.Body.dic["domainFullPathName"].ToString();
                            }

                            if (bwListApplyTo == "device")
                            {
                                if (!gDicDeviceId.ContainsKey(deviceFullPathName))
                                {
                                    string errInfo = get_debug_info() + string.Format("{0}:对应的设备ID在gDicDeviceId中找不到", deviceFullPathName);
                                    add_log_info(LogInfoType.EROR, errInfo, "Main", LogCategory.I);
                                    Logger.Trace(LogInfoType.EROR, errInfo, "Main", LogCategory.I);

                                    Fill_IMMS_Info(ref gAppUpper, AppMsgType.app_all_bwlist_response, -1, errInfo, true, null, null);
                                    Send_Msg_2_AppCtrl_Upper(gAppUpper);
                                    break;
                                }
                                else
                                {
                                    devInfo = gDicDeviceId[deviceFullPathName];
                                }
                            }
                            else if (bwListApplyTo == "domain")
                            {
                                if ((int)RC.NO_EXIST == gDbHelper.domain_record_exist(domainFullPathName))
                                {
                                    string errInfo = get_debug_info() + domainFullPathName + ":记录不存在";
                                    add_log_info(LogInfoType.EROR, errInfo, "Main", LogCategory.I);
                                    Logger.Trace(LogInfoType.EROR, errInfo, "Main", LogCategory.I);

                                    Fill_IMMS_Info(ref gAppUpper, AppMsgType.app_all_bwlist_response, -1, errInfo, true, null, null);
                                    Send_Msg_2_AppCtrl_Upper(gAppUpper);
                                    break;
                                }
                            }
                            else
                            {
                                string errInfo = get_debug_info() + "bwListApplyTo必须为device或domain.";
                                add_log_info(LogInfoType.EROR, errInfo, "Main", LogCategory.I);
                                Logger.Trace(LogInfoType.EROR, errInfo, "Main", LogCategory.I);

                                Fill_IMMS_Info(ref gAppUpper, AppMsgType.app_all_bwlist_response, -1, errInfo, true, null, null);
                                Send_Msg_2_AppCtrl_Upper(gAppUpper);
                                break;
                            }

                            strBwQuery bq = new strBwQuery();

                            if (gAppUpper.Body.dic.ContainsKey("imsi"))
                            {
                                if (!string.IsNullOrEmpty(gAppUpper.Body.dic["imsi"].ToString()))
                                {
                                    bq.imsi = gAppUpper.Body.dic["imsi"].ToString();                               
                                }
                            }

                            if (gAppUpper.Body.dic.ContainsKey("imei"))
                            {
                                if (!string.IsNullOrEmpty(gAppUpper.Body.dic["imei"].ToString()))
                                {
                                    bq.imei = gAppUpper.Body.dic["imei"].ToString();                            
                                }
                            }

                            if (gAppUpper.Body.dic.ContainsKey("bwFlag"))
                            {
                                if (!string.IsNullOrEmpty(gAppUpper.Body.dic["bwFlag"].ToString()))
                                {
                                    string bwFlag = gAppUpper.Body.dic["bwFlag"].ToString();

                                    if (bwFlag != "black" && bwFlag != "white" && bwFlag != "other")
                                    {
                                        string errInfo = get_debug_info() + string.Format("bwFlag的类型不对");
                                        add_log_info(LogInfoType.EROR, errInfo, "Main", LogCategory.I);
                                        Logger.Trace(LogInfoType.EROR, errInfo, "Main", LogCategory.I);

                                        Fill_IMMS_Info(ref gAppUpper, AppMsgType.app_all_bwlist_response, -1, errInfo, true, null, null);
                                        Send_Msg_2_AppCtrl_Upper(gAppUpper);
                                        break;
                                    }

                                    if (bwFlag == "black")
                                    {
                                        bq.bwFlag = bwType.BWTYPE_BLACK;
                                    }
                                    else if (bwFlag == "white")
                                    {
                                        bq.bwFlag = bwType.BWTYPE_WHITE;
                                    }
                                    else
                                    {
                                        bq.bwFlag = bwType.BWTYPE_OTHER;
                                    }
                                }
                                else
                                {
                                    bq.bwFlag = bwType.BWTYPE_ALL;
                                }
                            }

                            if (gAppUpper.Body.dic.ContainsKey("timeStart"))
                            {
                                if (!string.IsNullOrEmpty(gAppUpper.Body.dic["timeStart"].ToString()))
                                {
                                    bq.timeStart = gAppUpper.Body.dic["timeStart"].ToString();
                                    try
                                    {
                                        DateTime.Parse(bq.timeStart);
                                    }
                                    catch
                                    {
                                        string errInfo = get_debug_info() + string.Format("timeStart的格式不对");
                                        add_log_info(LogInfoType.EROR, errInfo, "Main", LogCategory.I);
                                        Logger.Trace(LogInfoType.EROR, errInfo, "Main", LogCategory.I);

                                        Fill_IMMS_Info(ref gAppUpper, AppMsgType.app_all_bwlist_response, -1, errInfo, true, null, null);
                                        Send_Msg_2_AppCtrl_Upper(gAppUpper);
                                        break;
                                    }
                                }
                                else
                                {
                                    bq.timeStart = "1900-01-01 12:34:56";
                                }
                            }
                            else
                            {
                                bq.timeStart = "1900-01-01 12:34:56";
                            }

                            if (gAppUpper.Body.dic.ContainsKey("timeEnded"))
                            {
                                if (!string.IsNullOrEmpty(gAppUpper.Body.dic["timeEnded"].ToString()))
                                {
                                    bq.timeEnded = gAppUpper.Body.dic["timeEnded"].ToString();
                                    try
                                    {
                                        DateTime.Parse(bq.timeEnded);
                                        if (string.Compare(bq.timeStart, bq.timeEnded) > 0)
                                        {
                                            string errInfo = get_debug_info() + string.Format("timeStart大于timeEnded.");
                                            add_log_info(LogInfoType.EROR, errInfo, "Main", LogCategory.I);
                                            Logger.Trace(LogInfoType.EROR, errInfo, "Main", LogCategory.I);

                                            Fill_IMMS_Info(ref gAppUpper, AppMsgType.app_all_bwlist_response, -1, errInfo, true, null, null);
                                            Send_Msg_2_AppCtrl_Upper(gAppUpper);
                                            break;
                                        }
                                    }
                                    catch
                                    {
                                        string errInfo = get_debug_info() + string.Format("timeEnded的格式不对");
                                        add_log_info(LogInfoType.EROR, errInfo, "Main", LogCategory.I);
                                        Logger.Trace(LogInfoType.EROR, errInfo, "Main", LogCategory.I);

                                        Fill_IMMS_Info(ref gAppUpper, AppMsgType.app_all_bwlist_response, -1, errInfo, true, null, null);
                                        Send_Msg_2_AppCtrl_Upper(gAppUpper);
                                        break;
                                    }
                                }
                                else
                                {
                                    bq.timeEnded = "2918-06-05 12:34:56";
                                }
                            }
                            else
                            {
                                bq.timeEnded = "2918-06-05 12:34:56";
                            }


                            if (gAppUpper.Body.dic.ContainsKey("des"))
                            {
                                if (!string.IsNullOrEmpty(gAppUpper.Body.dic["des"].ToString()))
                                {
                                    bq.des = gAppUpper.Body.dic["des"].ToString();
                                }
                            }

                            #endregion

                            #region 返回消息

                            if (string.IsNullOrEmpty(appId))
                            {
                                string errInfo = get_debug_info() + "获取AppInfo的IP和Port失败.";
                                add_log_info(LogInfoType.EROR, errInfo, "Main", LogCategory.I);
                                Logger.Trace(LogInfoType.EROR, errInfo, "Main", LogCategory.I);

                                Fill_IMMS_Info(ref gAppUpper, AppMsgType.app_all_bwlist_response, -1, errInfo, true, null, null);
                                Send_Msg_2_AppCtrl_Upper(gAppUpper);
                                break;
                            }

                            if (bwListApplyTo == "device")
                            {
                                #region 设备的处理 

                                if (!gDicBwListQueryInfo.ContainsKey(appId))
                                {
                                    strBwListQueryInfo qi = new strBwListQueryInfo();
                                    qi.bwListApplyTo = bwListApplyTo;
                                    qi.deviceFullPathName = deviceFullPathName;
                                 
                                    qi.dt = new DataTable();
                                    
                                    rtv = gDbHelper.bwlist_record_entity_get(ref qi.dt, devInfo.id, bq);
                                    if (rtv == 0)
                                    {
                                        qi.totalRecords = qi.dt.Rows.Count;
                                        qi.totalPages = (int)Math.Ceiling((double)qi.dt.Rows.Count / DataController.RecordsOfPageSize);
                                        qi.pageSize = DataController.RecordsOfPageSize;
                                    }

                                    //添加App对应的黑白名单查询条件和结果
                                    gDicBwListQueryInfo.Add(appId, qi);
                                }
                                else
                                {
                                    gDicBwListQueryInfo.Remove(appId);

                                    strBwListQueryInfo qi = new strBwListQueryInfo();
                                    qi.bwListApplyTo = bwListApplyTo;
                                    qi.deviceFullPathName = deviceFullPathName;

                                    qi.dt = new DataTable();

                                    //每次都从库中取吧，因为再次查询时，库已经发生变化了。
                                    rtv = gDbHelper.bwlist_record_entity_get(ref qi.dt, devInfo.id, bq);
                                    if (rtv == 0)
                                    {
                                        qi.totalRecords = qi.dt.Rows.Count;
                                        qi.totalPages = (int)Math.Ceiling((double)qi.dt.Rows.Count / DataController.RecordsOfPageSize);
                                        qi.pageSize = DataController.RecordsOfPageSize;
                                    }

                                    //添加App对应的黑白名单查询条件和结果
                                    gDicBwListQueryInfo.Add(appId, qi);
                                }
                                                     
                                if (rtv != (int)RC.SUCCESS)
                                {
                                    Fill_IMMS_Info(ref gAppUpper, AppMsgType.app_all_bwlist_response, rtv, gDbHelper.get_rtv_str(rtv), true, null, null);
                                    Send_Msg_2_AppCtrl_Upper(gAppUpper);
                                    break;
                                }
                                else
                                {
                                    string pageInfo = "";
                                    if (gDicBwListQueryInfo[appId].dt.Rows.Count == 0)
                                    {
                                        pageInfo = "0:0";
                                    }
                                    else
                                    {
                                        pageInfo = string.Format("1:{0}", Math.Ceiling((double)gDicBwListQueryInfo[appId].dt.Rows.Count / DataController.RecordsOfPageSize));
                                    }

                                    int firstPageSize = 0;
                                    if (gDicBwListQueryInfo[appId].dt.Rows.Count > DataController.RecordsOfPageSize)
                                    {
                                        firstPageSize = DataController.RecordsOfPageSize;
                                    }
                                    else
                                    {
                                        firstPageSize = gDicBwListQueryInfo[appId].dt.Rows.Count;
                                    }                                   

                                    #region 取出各条记录

                                    Fill_IMMS_Info(ref gAppUpper, AppMsgType.app_all_bwlist_response, rtv, gDbHelper.get_rtv_str(rtv), true, null, null);
                                    gAppUpper.Body.dic.Add("TotalRecords", gDicBwListQueryInfo[appId].totalRecords.ToString());
                                    gAppUpper.Body.dic.Add("CurPageIndex", pageInfo);
                                    gAppUpper.Body.dic.Add("PageSize", gDicBwListQueryInfo[appId].pageSize.ToString());

                                    for (int inx = 0; inx < firstPageSize; inx++)
                                    {
                                        DataRow dr = gDicBwListQueryInfo[appId].dt.Rows[inx];

                                        Name_DIC_Struct ndic = new Name_DIC_Struct();
                                        ndic.name = (inx+1).ToString();                                       

                                        if (string.IsNullOrEmpty(dr["imsi"].ToString()))
                                        {
                                            ndic.dic.Add("imsi", "");
                                        }
                                        else
                                        {
                                            ndic.dic.Add("imsi", dr["imsi"].ToString());
                                        }

                                        if (string.IsNullOrEmpty(dr["imei"].ToString()))
                                        {
                                            ndic.dic.Add("imei", "");
                                        }
                                        else
                                        {
                                            ndic.dic.Add("imei", dr["imei"].ToString());
                                        }

                                        if (string.IsNullOrEmpty(dr["bwFlag"].ToString()))
                                        {
                                            ndic.dic.Add("bwFlag", "");
                                        }
                                        else
                                        {
                                            ndic.dic.Add("bwFlag", dr["bwFlag"].ToString());
                                        }

                                        if (string.IsNullOrEmpty(dr["rbStart"].ToString()))
                                        {
                                            ndic.dic.Add("rbStart", "");
                                        }
                                        else
                                        {
                                            ndic.dic.Add("rbStart", dr["rbStart"].ToString());
                                        }

                                        if (string.IsNullOrEmpty(dr["rbEnd"].ToString()))
                                        {
                                            ndic.dic.Add("rbEnd", "");
                                        }
                                        else
                                        {
                                            ndic.dic.Add("rbEnd", dr["rbEnd"].ToString());
                                        }

                                        if (string.IsNullOrEmpty(dr["time"].ToString()))
                                        {
                                            ndic.dic.Add("time", "");
                                        }
                                        else
                                        {
                                            ndic.dic.Add("time", dr["time"].ToString());
                                        }

                                        if (string.IsNullOrEmpty(dr["des"].ToString()))
                                        {
                                            ndic.dic.Add("des", "");
                                        }
                                        else
                                        {
                                            ndic.dic.Add("des", dr["des"].ToString());
                                        }

                                        gAppUpper.Body.n_dic.Add(ndic);
                                    }

                                    #endregion
                                }

                                #endregion
                            }
                            else
                            {
                                #region 域的处理

                                //黑白名单关联到域
                                int affDomainId = -1;
                                List<int> listDevId = new List<int>();

                                //获取一个节点下所有站点下所有设备Id的列表
                                rtv = gDbHelper.domain_record_device_id_list_get(domainFullPathName, ref listDevId);
                                if (rtv != (int)RC.SUCCESS)
                                {
                                    Fill_IMMS_Info(ref gAppUpper, AppMsgType.app_all_bwlist_response, rtv, gDbHelper.get_rtv_str(rtv), true, null, null);
                                    Send_Msg_2_AppCtrl_Upper(gAppUpper);
                                    break;
                                }
                                else
                                {
                                    rtv = gDbHelper.domain_get_id_by_nameFullPath(domainFullPathName, ref affDomainId);
                                    if (rtv != (int)RC.SUCCESS)
                                    {
                                        Fill_IMMS_Info(ref gAppUpper, AppMsgType.app_all_bwlist_response, rtv, gDbHelper.get_rtv_str(rtv), true, null, null);
                                        Send_Msg_2_AppCtrl_Upper(gAppUpper);
                                        break;
                                    }                 
                                }

                                if (!gDicBwListQueryInfo.ContainsKey(appId))
                                {
                                    strBwListQueryInfo qi = new strBwListQueryInfo();

                                    qi.bwListApplyTo = bwListApplyTo;                                  
                                    qi.domainFullPathName = domainFullPathName;
                                    qi.dt = new DataTable();

                                    rtv = gDbHelper.bwlist_record_entity_get(ref qi.dt, listDevId,affDomainId,bq);
                                    if (rtv == 0)
                                    {
                                        qi.totalRecords = qi.dt.Rows.Count;
                                        qi.totalPages = (int)Math.Ceiling((double)qi.dt.Rows.Count / DataController.RecordsOfPageSize);
                                        qi.pageSize = DataController.RecordsOfPageSize;
                                    }

                                    //添加App对应的黑白名单查询条件和结果
                                    gDicBwListQueryInfo.Add(appId, qi);
                                }
                                else
                                {
                                    gDicBwListQueryInfo.Remove(appId);

                                    strBwListQueryInfo qi = new strBwListQueryInfo();

                                    qi.bwListApplyTo = bwListApplyTo;
                                    qi.domainFullPathName = domainFullPathName;
                                    qi.dt = new DataTable();

                                    //每次都从库中取吧，因为再次查询时，库可能已经发生变化了。
                                    rtv = gDbHelper.bwlist_record_entity_get(ref qi.dt, listDevId, affDomainId,bq);
                                    if (rtv == 0)
                                    {
                                        qi.totalRecords = qi.dt.Rows.Count;
                                        qi.totalPages = (int)Math.Ceiling((double)qi.dt.Rows.Count / DataController.RecordsOfPageSize);
                                        qi.pageSize = DataController.RecordsOfPageSize;
                                    }

                                    //添加App对应的黑白名单查询条件和结果
                                    gDicBwListQueryInfo.Add(appId, qi);
                                }

                                if (rtv != (int)RC.SUCCESS)
                                {
                                    Fill_IMMS_Info(ref gAppUpper, AppMsgType.app_all_bwlist_response, rtv, gDbHelper.get_rtv_str(rtv), true, null, null);
                                    Send_Msg_2_AppCtrl_Upper(gAppUpper);
                                    break;
                                }
                                else
                                {
                                    string pageInfo = "";
                                    if (gDicBwListQueryInfo[appId].dt.Rows.Count == 0)
                                    {
                                        pageInfo = "0:0";
                                    }
                                    else
                                    {
                                        pageInfo = string.Format("1:{0}", Math.Ceiling((double)gDicBwListQueryInfo[appId].dt.Rows.Count / DataController.RecordsOfPageSize));
                                    }

                                    int firstPageSize = 0;
                                    if (gDicBwListQueryInfo[appId].dt.Rows.Count > DataController.RecordsOfPageSize)
                                    {
                                        firstPageSize = DataController.RecordsOfPageSize;
                                    }
                                    else
                                    {
                                        firstPageSize = gDicBwListQueryInfo[appId].dt.Rows.Count;
                                    }                                                        

                                    #region 取出各条记录

                                    Fill_IMMS_Info(ref gAppUpper, AppMsgType.app_all_bwlist_response, rtv, gDbHelper.get_rtv_str(rtv), true, null, null);
                                    gAppUpper.Body.dic.Add("TotalRecords", gDicBwListQueryInfo[appId].totalRecords.ToString());
                                    gAppUpper.Body.dic.Add("CurPageIndex", pageInfo);
                                    gAppUpper.Body.dic.Add("PageSize", gDicBwListQueryInfo[appId].pageSize.ToString());

                                    for (int inx = 0; inx < firstPageSize; inx++)
                                    {
                                        DataRow dr = gDicBwListQueryInfo[appId].dt.Rows[inx];

                                        Name_DIC_Struct ndic = new Name_DIC_Struct();
                                        ndic.name = (inx+1).ToString();                                       

                                        if (string.IsNullOrEmpty(dr["imsi"].ToString()))
                                        {
                                            ndic.dic.Add("imsi", "");
                                        }
                                        else
                                        {
                                            ndic.dic.Add("imsi", dr["imsi"].ToString());
                                        }

                                        if (string.IsNullOrEmpty(dr["imei"].ToString()))
                                        {
                                            ndic.dic.Add("imei", "");
                                        }
                                        else
                                        {
                                            ndic.dic.Add("imei", dr["imei"].ToString());
                                        }

                                        if (string.IsNullOrEmpty(dr["bwFlag"].ToString()))
                                        {
                                            ndic.dic.Add("bwFlag", "");
                                        }
                                        else
                                        {
                                            ndic.dic.Add("bwFlag", dr["bwFlag"].ToString());
                                        }

                                        if (string.IsNullOrEmpty(dr["rbStart"].ToString()))
                                        {
                                            ndic.dic.Add("rbStart", "");
                                        }
                                        else
                                        {
                                            ndic.dic.Add("rbStart", dr["rbStart"].ToString());
                                        }

                                        if (string.IsNullOrEmpty(dr["rbEnd"].ToString()))
                                        {
                                            ndic.dic.Add("rbEnd", "");
                                        }
                                        else
                                        {
                                            ndic.dic.Add("rbEnd", dr["rbEnd"].ToString());
                                        }

                                        if (string.IsNullOrEmpty(dr["time"].ToString()))
                                        {
                                            ndic.dic.Add("time", "");
                                        }
                                        else
                                        {
                                            ndic.dic.Add("time", dr["time"].ToString());
                                        }

                                        if (string.IsNullOrEmpty(dr["des"].ToString()))
                                        {
                                            ndic.dic.Add("des", "");
                                        }
                                        else
                                        {
                                            ndic.dic.Add("des", dr["des"].ToString());
                                        }

                                        gAppUpper.Body.n_dic.Add(ndic);
                                    }

                                    #endregion
                                }

                                #endregion
                            }

                            Send_Msg_2_AppCtrl_Upper(gAppUpper);
                            break;

                            #endregion                                     
                        }
                    case AppMsgType.app_all_next_page_bwlist_request:
                        {
                            #region 获取信息

                            string appId = Get_App_Info(gAppUpper);

                            // 
                            // GSW和WCDMA的设备，IMSI和IMEI可同时设置
                            //
                            // "CurPageIndex":"1:50",   
                            //                                                 

                            string CurPageIndex = "";
                            int curPageInx = -1;
                            int totalPages = -1;

                            if (gAppUpper.Body.dic.ContainsKey("CurPageIndex"))
                            {
                                CurPageIndex = gAppUpper.Body.dic["CurPageIndex"].ToString();
                                if (check_and_get_page_info(CurPageIndex, ref curPageInx, ref totalPages) == false)
                                {
                                    string errInfo = get_debug_info() + string.Format("{0}:", "CurPageIndex字段解析出错.");
                                    add_log_info(LogInfoType.EROR, errInfo, "Main", LogCategory.I);
                                    Logger.Trace(LogInfoType.EROR, errInfo, "Main", LogCategory.I);

                                    Fill_IMMS_Info(ref gAppUpper, AppMsgType.app_all_bwlist_response, -1, errInfo, true, null, null);
                                    Send_Msg_2_AppCtrl_Upper(gAppUpper);
                                    break;
                                }
                            }
                            else
                            {
                                string errInfo = get_debug_info() + string.Format("{0}:", "没包含字段CurPageIndex");
                                add_log_info(LogInfoType.EROR, errInfo, "Main", LogCategory.I);
                                Logger.Trace(LogInfoType.EROR, errInfo, "Main", LogCategory.I);

                                Fill_IMMS_Info(ref gAppUpper, AppMsgType.app_all_bwlist_response, -1, errInfo, true, null, null);
                                Send_Msg_2_AppCtrl_Upper(gAppUpper);
                                break;
                            }

                            if (string.IsNullOrEmpty(appId))
                            {
                                string errInfo = get_debug_info() + "获取AppInfo的IP和Port失败.";
                                add_log_info(LogInfoType.EROR, errInfo, "Main", LogCategory.I);
                                Logger.Trace(LogInfoType.EROR, errInfo, "Main", LogCategory.I);

                                Fill_IMMS_Info(ref gAppUpper, AppMsgType.app_all_bwlist_response, -1, errInfo, true, null, null);
                                Send_Msg_2_AppCtrl_Upper(gAppUpper);
                                break;
                            }

                            #endregion

                            #region 返回消息

                            if (!gDicBwListQueryInfo.ContainsKey(appId))
                            {
                                string errInfo = get_debug_info() + string.Format("{0}:对应的查询信息不存在.", appId);
                                add_log_info(LogInfoType.EROR, errInfo, "Main", LogCategory.I);
                                Logger.Trace(LogInfoType.EROR, errInfo, "Main", LogCategory.I);

                                Fill_IMMS_Info(ref gAppUpper, AppMsgType.app_all_bwlist_response, -1, errInfo, true, null, null);
                                Send_Msg_2_AppCtrl_Upper(gAppUpper);
                                break;
                            }

                            strBwListQueryInfo qi = gDicBwListQueryInfo[appId];
                            if (qi.totalPages != totalPages)
                            {
                                string errInfo = get_debug_info() + string.Format("{0}:对应的总页数不匹配.", appId);
                                add_log_info(LogInfoType.EROR, errInfo, "Main", LogCategory.I);
                                Logger.Trace(LogInfoType.EROR, errInfo, "Main", LogCategory.I);

                                Fill_IMMS_Info(ref gAppUpper, AppMsgType.app_all_bwlist_response, -1, errInfo, true, null, null);
                                Send_Msg_2_AppCtrl_Upper(gAppUpper);
                                break;
                            }                          

                            int startInx = -1;
                            int endedInx = -1;

                            if (curPageInx == totalPages)
                            {
                                //最后一页
                                startInx = (curPageInx - 1) * qi.pageSize;
                                endedInx = qi.totalRecords;
                            }
                            else
                            {
                                //不是最后一页
                                startInx = (curPageInx - 1) * qi.pageSize;
                                endedInx = startInx + qi.pageSize;
                            }

                            #region 取出各条记录

                            string pageInfo = string.Format("{0}:{1}", curPageInx, qi.totalPages);
                            Fill_IMMS_Info(ref gAppUpper, AppMsgType.app_all_bwlist_response, 0, gDbHelper.get_rtv_str(0), true, null, null);
                            gAppUpper.Body.dic.Add("TotalRecords", qi.totalRecords.ToString());
                            gAppUpper.Body.dic.Add("CurPageIndex", pageInfo);
                            gAppUpper.Body.dic.Add("PageSize", qi.pageSize.ToString());

                            for (int inx = startInx; inx < endedInx; inx++)
                            {
                                DataRow dr = gDicBwListQueryInfo[appId].dt.Rows[inx];

                                Name_DIC_Struct ndic = new Name_DIC_Struct();
                                ndic.name = (inx + 1).ToString();

                                ndic.dic.Add("domainId", dr["id"].ToString());
                                ndic.dic.Add("domainParentId", dr["parentId"].ToString());
                                ndic.dic.Add("parentFullPathName", dr["nameFullPath"].ToString());
                                ndic.dic.Add("name", dr["name"].ToString());

                                if (string.IsNullOrEmpty(dr["imsi"].ToString()))
                                {
                                    ndic.dic.Add("imsi", "null");
                                }
                                else
                                {
                                    ndic.dic.Add("imsi", dr["imsi"].ToString());
                                }

                                if (string.IsNullOrEmpty(dr["imei"].ToString()))
                                {
                                    ndic.dic.Add("imei", "null");
                                }
                                else
                                {
                                    ndic.dic.Add("imei", dr["imei"].ToString());
                                }

                                if (string.IsNullOrEmpty(dr["bwFlag"].ToString()))
                                {
                                    ndic.dic.Add("bwFlag", "null");
                                }
                                else
                                {
                                    ndic.dic.Add("bwFlag", dr["bwFlag"].ToString());
                                }

                                if (string.IsNullOrEmpty(dr["rbStart"].ToString()))
                                {
                                    ndic.dic.Add("rbStart", "null");
                                }
                                else
                                {
                                    ndic.dic.Add("rbStart", dr["rbStart"].ToString());
                                }

                                if (string.IsNullOrEmpty(dr["rbEnd"].ToString()))
                                {
                                    ndic.dic.Add("rbEnd", "null");
                                }
                                else
                                {
                                    ndic.dic.Add("rbEnd", dr["rbEnd"].ToString());
                                }

                                if (string.IsNullOrEmpty(dr["time"].ToString()))
                                {
                                    ndic.dic.Add("time", "null");
                                }
                                else
                                {
                                    ndic.dic.Add("time", dr["time"].ToString());
                                }

                                if (string.IsNullOrEmpty(dr["des"].ToString()))
                                {
                                    ndic.dic.Add("des", "null");
                                }
                                else
                                {
                                    ndic.dic.Add("des", dr["des"].ToString());
                                }

                                gAppUpper.Body.n_dic.Add(ndic);
                            }

                            #endregion
                                             

                            Send_Msg_2_AppCtrl_Upper(gAppUpper);
                            break;

                            #endregion                                     
                        }                        
                    case AppMsgType.app_set_GenPara_ActiveTime_Request:
                        {
                            #region 获取信息

                            //   "parentFullPathName":"设备.深圳.福田.中心广场.西北监控",
                            //   "name":"电信FDD"
                            //   "activeTime1Start":"2018-05-28 09:30:00"  生效时间1的起始时间
                            //   "activeTime1Ended":"2018-05-28 12:30:00"  生效时间1的结束时间
                            //   "activeTime2Start":"2018-05-28 13:30:00"  生效时间2的起始时间
                            //   "activeTime2Ended":"2018-05-28 14:30:00"  生效时间2的结束时间
                            //   "activeTime3Start":"2018-05-28 16:30:00"  生效时间3的起始时间，有的话就添加该项
                            //   "activeTime3Ended":"2018-05-28 18:30:00"  生效时间3的结束时间，有的话就添加该项
                            //   "activeTime4Start":"2018-05-28 20:30:00"  生效时间4的起始时间，有的话就添加该项
                            //   "activeTime4Ended":"2018-05-28 22:30:00"  生效时间4的结束时间，有的话就添加该项

                            int rtv = -1;
                            strDevice devInfo = new strDevice();

                            string carry = "";
                            string name = "";
                            string devFullPathName = "";
                            string parentFullPathName = "";

                            if (gAppUpper.Body.dic.ContainsKey("parentFullPathName"))
                            {
                                parentFullPathName = gAppUpper.Body.dic["parentFullPathName"].ToString();
                            }

                            if (gAppUpper.Body.dic.ContainsKey("name"))
                            {
                                name = gAppUpper.Body.dic["name"].ToString();
                            }

                            if (parentFullPathName == "" || name == "")
                            {
                                //返回出错处理
                                string errInfo = string.Format("app_set_GenPara_ActiveTime_Request,参数有误.");
                                add_log_info(LogInfoType.EROR, errInfo, "Main", LogCategory.I);
                                Logger.Trace(LogInfoType.EROR, errInfo, "Main", LogCategory.I);

                                Fill_IMMS_Info(ref gAppUpper, AppMsgType.app_set_GenPara_ActiveTime_Response, -1, errInfo, true, null, null);
                                Send_Msg_2_AppCtrl_Upper(gAppUpper);
                                break;
                            }

                            if (gAppUpper.Body.dic.ContainsKey("carry"))
                            {
                                carry = gAppUpper.Body.dic["carry"].ToString();
                                if (carry != "0" && carry != "1" && carry != "2")
                                {
                                    //返回出错处理
                                    string errInfo = string.Format("app_set_GenPara_ActiveTime_Request,carry = {0},参数有误.",carry);
                                    add_log_info(LogInfoType.EROR, errInfo, "Main", LogCategory.I);
                                    Logger.Trace(LogInfoType.EROR, errInfo, "Main", LogCategory.I);

                                    Fill_IMMS_Info(ref gAppUpper, AppMsgType.app_set_GenPara_ActiveTime_Response, -1, errInfo, true, null, null);
                                    Send_Msg_2_AppCtrl_Upper(gAppUpper);
                                    break;
                                }
                            }

                            devFullPathName = string.Format("{0}.{1}", parentFullPathName, name);

                            if (!gDicDeviceId.ContainsKey(devFullPathName))
                            {
                                //返回出错处理
                                string errInfo = get_debug_info() + string.Format("{0}:对应的设备ID在gDicDeviceId中找不到", devFullPathName);
                                add_log_info(LogInfoType.EROR, errInfo, "Main", LogCategory.I);
                                Logger.Trace(LogInfoType.EROR, errInfo, "Main", LogCategory.I);

                                Fill_IMMS_Info(ref gAppUpper, AppMsgType.app_set_GenPara_ActiveTime_Response, -1, errInfo, true, null, null);
                                Send_Msg_2_AppCtrl_Upper(gAppUpper);
                                break;                                
                            }
                            else
                            {
                                devInfo = gDicDeviceId[devFullPathName];
                            }

                            #endregion

                            #region 返回消息

                            switch (devInfo.devMode)
                            {
                                case devMode.MODE_GSM:
                                    {
                                        #region GSM处理

                                        strGsmRfPara grp = new strGsmRfPara();

                                        if (gAppUpper.Body.dic.ContainsKey("activeTime1Start"))
                                        {
                                            grp.activeTime1Start = gAppUpper.Body.dic["activeTime1Start"].ToString();
                                        }

                                        if (gAppUpper.Body.dic.ContainsKey("activeTime1Ended"))
                                        {
                                            grp.activeTime1Ended = gAppUpper.Body.dic["activeTime1Ended"].ToString();
                                        }

                                        if (gAppUpper.Body.dic.ContainsKey("activeTime2Start"))
                                        {
                                            grp.activeTime2Start = gAppUpper.Body.dic["activeTime2Start"].ToString();
                                        }

                                        if (gAppUpper.Body.dic.ContainsKey("activeTime2Ended"))
                                        {
                                            grp.activeTime2Ended = gAppUpper.Body.dic["activeTime2Ended"].ToString();
                                        }

                                        if (gAppUpper.Body.dic.ContainsKey("activeTime3Start"))
                                        {
                                            grp.activeTime3Start = gAppUpper.Body.dic["activeTime3Start"].ToString();
                                        }

                                        if (gAppUpper.Body.dic.ContainsKey("activeTime3Ended"))
                                        {
                                            grp.activeTime3Ended = gAppUpper.Body.dic["activeTime3Ended"].ToString();
                                        }

                                        if (gAppUpper.Body.dic.ContainsKey("activeTime4Start"))
                                        {
                                            grp.activeTime4Start = gAppUpper.Body.dic["activeTime4Start"].ToString();
                                        }

                                        if (gAppUpper.Body.dic.ContainsKey("activeTime4Ended"))
                                        {
                                            grp.activeTime4Ended = gAppUpper.Body.dic["activeTime4Ended"].ToString();
                                        }

                                        rtv = 0;
                                        if (carry == "0")
                                        {
                                            rtv += gDbHelper.gsm_rf_para_record_update(0, devInfo.id, grp);                                           
                                        }
                                        else if (carry == "1")
                                        {                                            
                                            rtv += gDbHelper.gsm_rf_para_record_update(1, devInfo.id, grp);
                                        }
                                        else
                                        {
                                            rtv += gDbHelper.gsm_rf_para_record_update(0, devInfo.id, grp);
                                            rtv += gDbHelper.gsm_rf_para_record_update(1, devInfo.id, grp);
                                        }                                        

                                        Fill_IMMS_Info(ref gAppUpper, AppMsgType.app_set_GenPara_ActiveTime_Response, rtv, gDbHelper.get_rtv_str(rtv), true, null, null);
                                        Send_Msg_2_AppCtrl_Upper(gAppUpper);
                                        break;

                                        #endregion
                                    }
                                case devMode.MODE_GSM_V2:
                                    {
                                        #region GSM-V2处理

                                        strGcMisc gm = new strGcMisc();

                                        if (gAppUpper.Body.dic.ContainsKey("activeTime1Start"))
                                        {
                                            gm.activeTime1Start = gAppUpper.Body.dic["activeTime1Start"].ToString();
                                        }

                                        if (gAppUpper.Body.dic.ContainsKey("activeTime1Ended"))
                                        {
                                            gm.activeTime1Ended = gAppUpper.Body.dic["activeTime1Ended"].ToString();
                                        }

                                        if (gAppUpper.Body.dic.ContainsKey("activeTime2Start"))
                                        {
                                            gm.activeTime2Start = gAppUpper.Body.dic["activeTime2Start"].ToString();
                                        }

                                        if (gAppUpper.Body.dic.ContainsKey("activeTime2Ended"))
                                        {
                                            gm.activeTime2Ended = gAppUpper.Body.dic["activeTime2Ended"].ToString();
                                        }

                                        if (gAppUpper.Body.dic.ContainsKey("activeTime3Start"))
                                        {
                                            gm.activeTime3Start = gAppUpper.Body.dic["activeTime3Start"].ToString();
                                        }

                                        if (gAppUpper.Body.dic.ContainsKey("activeTime3Ended"))
                                        {
                                            gm.activeTime3Ended = gAppUpper.Body.dic["activeTime3Ended"].ToString();
                                        }

                                        if (gAppUpper.Body.dic.ContainsKey("activeTime4Start"))
                                        {
                                            gm.activeTime4Start = gAppUpper.Body.dic["activeTime4Start"].ToString();
                                        }

                                        if (gAppUpper.Body.dic.ContainsKey("activeTime4Ended"))
                                        {
                                            gm.activeTime4Ended = gAppUpper.Body.dic["activeTime4Ended"].ToString();
                                        }

                                        rtv = 0;
                                        if (carry == "0")
                                        {
                                            rtv += gDbHelper.gc_misc_record_update(0, devInfo.id, gm);
                                        }
                                        else if (carry == "1")
                                        {
                                            rtv += gDbHelper.gc_misc_record_update(1, devInfo.id, gm);
                                        }
                                        else
                                        {
                                            rtv += gDbHelper.gc_misc_record_update(0, devInfo.id, gm);
                                            rtv += gDbHelper.gc_misc_record_update(1, devInfo.id, gm);
                                        }

                                        Fill_IMMS_Info(ref gAppUpper, AppMsgType.app_set_GenPara_ActiveTime_Response, rtv, gDbHelper.get_rtv_str(rtv), true, null, null);
                                        Send_Msg_2_AppCtrl_Upper(gAppUpper);
                                        break;

                                        #endregion                                      
                                    }
                                case devMode.MODE_CDMA:
                                    {
                                        #region GSM-V2处理

                                        strGcMisc gm = new strGcMisc();

                                        if (gAppUpper.Body.dic.ContainsKey("activeTime1Start"))
                                        {
                                            gm.activeTime1Start = gAppUpper.Body.dic["activeTime1Start"].ToString();
                                        }

                                        if (gAppUpper.Body.dic.ContainsKey("activeTime1Ended"))
                                        {
                                            gm.activeTime1Ended = gAppUpper.Body.dic["activeTime1Ended"].ToString();
                                        }

                                        if (gAppUpper.Body.dic.ContainsKey("activeTime2Start"))
                                        {
                                            gm.activeTime2Start = gAppUpper.Body.dic["activeTime2Start"].ToString();
                                        }

                                        if (gAppUpper.Body.dic.ContainsKey("activeTime2Ended"))
                                        {
                                            gm.activeTime2Ended = gAppUpper.Body.dic["activeTime2Ended"].ToString();
                                        }

                                        if (gAppUpper.Body.dic.ContainsKey("activeTime3Start"))
                                        {
                                            gm.activeTime3Start = gAppUpper.Body.dic["activeTime3Start"].ToString();
                                        }

                                        if (gAppUpper.Body.dic.ContainsKey("activeTime3Ended"))
                                        {
                                            gm.activeTime3Ended = gAppUpper.Body.dic["activeTime3Ended"].ToString();
                                        }

                                        if (gAppUpper.Body.dic.ContainsKey("activeTime4Start"))
                                        {
                                            gm.activeTime4Start = gAppUpper.Body.dic["activeTime4Start"].ToString();
                                        }

                                        if (gAppUpper.Body.dic.ContainsKey("activeTime4Ended"))
                                        {
                                            gm.activeTime4Ended = gAppUpper.Body.dic["activeTime4Ended"].ToString();
                                        }

                                        rtv = 0;                                    
                                        rtv += gDbHelper.gc_misc_record_update(-1, devInfo.id, gm);
                                        
                                        Fill_IMMS_Info(ref gAppUpper, AppMsgType.app_set_GenPara_ActiveTime_Response, rtv, gDbHelper.get_rtv_str(rtv), true, null, null);
                                        Send_Msg_2_AppCtrl_Upper(gAppUpper);
                                        break;

                                        #endregion                                      
                                    }
                                case devMode.MODE_TD_SCDMA:
                                    {
                                        break;
                                    }
                                case devMode.MODE_WCDMA:
                                case devMode.MODE_LTE_FDD:
                                case devMode.MODE_LTE_TDD:                                                         
                                    {
                                        #region LTE

                                        strApGenPara apGP = new strApGenPara();

                                        if (gAppUpper.Body.dic.ContainsKey("activeTime1Start"))
                                        {
                                            apGP.activeTime1Start = gAppUpper.Body.dic["activeTime1Start"].ToString();
                                        }

                                        if (gAppUpper.Body.dic.ContainsKey("activeTime1Ended"))
                                        {
                                            apGP.activeTime1Ended = gAppUpper.Body.dic["activeTime1Ended"].ToString();
                                        }

                                        if (gAppUpper.Body.dic.ContainsKey("activeTime2Start"))
                                        {
                                            apGP.activeTime2Start = gAppUpper.Body.dic["activeTime2Start"].ToString();
                                        }

                                        if (gAppUpper.Body.dic.ContainsKey("activeTime2Ended"))
                                        {
                                            apGP.activeTime2Ended = gAppUpper.Body.dic["activeTime2Ended"].ToString();
                                        }

                                        if (gAppUpper.Body.dic.ContainsKey("activeTime3Start"))
                                        {
                                            apGP.activeTime3Start = gAppUpper.Body.dic["activeTime3Start"].ToString();
                                        }

                                        if (gAppUpper.Body.dic.ContainsKey("activeTime3Ended"))
                                        {
                                            apGP.activeTime3Ended = gAppUpper.Body.dic["activeTime3Ended"].ToString();
                                        }

                                        if (gAppUpper.Body.dic.ContainsKey("activeTime4Start"))
                                        {
                                            apGP.activeTime4Start = gAppUpper.Body.dic["activeTime4Start"].ToString();
                                        }

                                        if (gAppUpper.Body.dic.ContainsKey("activeTime4Ended"))
                                        {
                                            apGP.activeTime4Ended = gAppUpper.Body.dic["activeTime4Ended"].ToString();
                                        }

                                        rtv = gDbHelper.ap_general_para_record_update(devInfo.id, apGP);

                                        Fill_IMMS_Info(ref gAppUpper, AppMsgType.app_set_GenPara_ActiveTime_Response, rtv, gDbHelper.get_rtv_str(rtv), true, null, null);                                        
                                        Send_Msg_2_AppCtrl_Upper(gAppUpper);
                                        break;

                                        #endregion
                                    }
                                case devMode.MODE_UNKNOWN:
                                    {
                                        #region 未知mode

                                        string errInfo = string.Format("mode有误");
                                        add_log_info(LogInfoType.EROR, errInfo, "Main", LogCategory.I);
                                        Logger.Trace(LogInfoType.EROR, errInfo, "Main", LogCategory.I);

                                        Fill_IMMS_Info(ref gAppUpper, AppMsgType.app_set_GenPara_ActiveTime_Response, -1, errInfo, true, null, null);
                                        Send_Msg_2_AppCtrl_Upper(gAppUpper);
                                        break;

                                        #endregion
                                    }
                            }
                                                      
                            break;

                            #endregion                   
                        }
                    case AppMsgType.app_get_GenPara_Request:
                        {
                            #region 获取信息

                            //   "parentFullPathName":"设备.深圳.福田.中心广场.西北监控",
                            //   "name":"电信FDD"

                            int rtv;
                            strDevice devInfo = new strDevice();
                            strApGenPara apGP = new strApGenPara();

                            string name = "";
                            string parentFullPathName = "";                            
                            string devFullPathName = "";

                            if (gAppUpper.Body.dic.ContainsKey("parentFullPathName"))
                            {
                                parentFullPathName = gAppUpper.Body.dic["parentFullPathName"].ToString();
                            }

                            if (gAppUpper.Body.dic.ContainsKey("name"))
                            {
                                name = gAppUpper.Body.dic["name"].ToString();
                            }

                            if (parentFullPathName == "" || name == "")
                            {
                                //返回出错处理
                                string errInfo = string.Format("app_get_GenPara_Request,参数有误.");
                                add_log_info(LogInfoType.EROR, errInfo, "Main", LogCategory.I);
                                Logger.Trace(LogInfoType.EROR, errInfo, "Main", LogCategory.I);

                                Fill_IMMS_Info(ref gAppUpper, AppMsgType.app_get_GenPara_Response, -1, errInfo, true, null, null);
                                Send_Msg_2_AppCtrl_Upper(gAppUpper);
                                break;                              
                            }

                            #endregion

                            #region 返回消息

                            strDomian domian = new strDomian();
                            devFullPathName = string.Format("{0}.{1}", parentFullPathName, name);

                            if (!gDicDeviceId.ContainsKey(devFullPathName))
                            {
                                //返回出错处理
                                string errInfo = get_debug_info() + string.Format("{0}:对应的设备ID在gDicDeviceId中找不到", devFullPathName);
                                add_log_info(LogInfoType.EROR, errInfo, "Main", LogCategory.I);
                                Logger.Trace(LogInfoType.EROR, errInfo, "Main", LogCategory.I);

                                Fill_IMMS_Info(ref gAppUpper, AppMsgType.app_get_GenPara_Response, -1, errInfo, true, null, null);
                                Send_Msg_2_AppCtrl_Upper(gAppUpper);
                                break;
                            }
                            else
                            {
                                devInfo = gDicDeviceId[devFullPathName];

                                // 2018-07-04
                                if (devInfo.devMode == devMode.MODE_GSM || devInfo.devMode == devMode.MODE_UNKNOWN)
                                {
                                    //返回出错处理
                                    string errInfo = get_debug_info() + string.Format("该设备没有没有通用参数.");
                                    add_log_info(LogInfoType.EROR, errInfo, "Main", LogCategory.I);
                                    Logger.Trace(LogInfoType.EROR, errInfo, "Main", LogCategory.I);

                                    Fill_IMMS_Info(ref gAppUpper, AppMsgType.app_get_GenPara_Response, -1, errInfo, true, null, null);
                                    Send_Msg_2_AppCtrl_Upper(gAppUpper);
                                    break;                                   
                                }

                                rtv = gDbHelper.domain_record_get_by_nameFullPath(parentFullPathName, ref domian);
                                if (rtv == 0)
                                {
                                    rtv = gDbHelper.ap_general_para_record_get_by_devid(devInfo.id, ref apGP);
                                }
                                else
                                {
                                    //返回出错处理
                                    string errInfo = get_debug_info() + string.Format("domain_record_get_by_nameFullPath出错.");
                                    add_log_info(LogInfoType.EROR, errInfo, "Main", LogCategory.I);
                                    Logger.Trace(LogInfoType.EROR, errInfo, "Main", LogCategory.I);

                                    Fill_IMMS_Info(ref gAppUpper, AppMsgType.app_get_GenPara_Response, -1, errInfo, true, null, null);
                                    Send_Msg_2_AppCtrl_Upper(gAppUpper);
                                    break;                                   
                                }
                            }

                            Fill_IMMS_Info(ref gAppUpper, AppMsgType.app_get_GenPara_Response, rtv, gDbHelper.get_rtv_str(rtv), true, null, null);
                           
                            gAppUpper.Body.dic.Add("domainId", domian.id.ToString());
                            gAppUpper.Body.dic.Add("domainParentId", domian.parentId.ToString());
                            gAppUpper.Body.dic.Add("parentFullPathName", parentFullPathName);
                            gAppUpper.Body.dic.Add("name", name);

                            //   "mode":"GSM",          设备工作制式：GSM、TDD-LTE、FDD-LTE等
                            //   "primaryplmn:"xxx",    PLMN
                            //   "earfcndl:"xxx",       工作频点（下行）
                            //   "earfcnul:"xxx",       工作频点（上行）
                            //   "cellid:"xxx",         cellid,2018-06-26
                            //   "pci:"xxx",            工作PCI
                            //   "bandwidth:"xxx",      工作Band
                            //   "tac:"xxx",            工作Tac
                            //   "txpower:"xxx",        发射功率
                            //   "periodtac:"xxx",      变换Tac周期
                            //   "manualfreq:"xxx",     手动选择工作频点（0：自动选择；1：手动选择）
                            //   "bootMode:"0",         设备启动方式（0：半自动。此时需要发送Active命令才开始建小区 1：全自动）
                            //   "Earfcnlist:"xxx,xxx", REM扫描频点列表
                            //   "Bandoffse:"xxxx",     Band频偏值。用于GPS同步时的补偿值
                            //   "NTP:"172.17.0.210",   NTP服务器地址
                            //   "ntppri:"5",           NTP获取时间的优先级
                            //   "source:"0，           同步源（0：GPS ； 1：CNM ； 2：no sync）
                            //   "ManualEnable:"1",     是否启动手动选择同步频点功能（0：不启动 ； 1 :启动）
                            //   "ManualEarfcn:"xxx",   手动选择的同步频点
                            //   "ManualPci:"xxx",      手动选择的同步PCI
                            //   "ManualBw:"xxx"        手动选择的同步小区带宽
                            //   "gpsConfig":"0"        GPS配置，0表示NOGPS，1表示GPS
                            //   "activeTime1Start":"2018-05-28 09:30:00"  生效时间1的起始时间
                            //   "activeTime1Ended":"2018-05-28 12:30:00"  生效时间1的结束时间
                            //   "activeTime2Start":"2018-05-28 13:30:00"  生效时间2的起始时间
                            //   "activeTime2Ended":"2018-05-28 14:30:00"  生效时间2的结束时间
                            //   "activeTime3Start":"2018-05-28 16:30:00"  生效时间3的起始时间，有的话就添加该项
                            //   "activeTime3Ended":"2018-05-28 18:30:00"  生效时间3的结束时间，有的话就添加该项
                            //   "activeTime4Start":"2018-05-28 20:30:00"  生效时间4的起始时间，有的话就添加该项
                            //   "activeTime4Ended":"2018-05-28 22:30:00"  生效时间4的结束时间，有的话就添加该项

                            if (rtv == 0)
                            {
                                if (!string.IsNullOrEmpty(apGP.mode))
                                {
                                    gAppUpper.Body.dic.Add("mode", apGP.mode);
                                }

                                if (!string.IsNullOrEmpty(apGP.primaryplmn))
                                {
                                    gAppUpper.Body.dic.Add("primaryplmn", apGP.primaryplmn);
                                }

                                if (!string.IsNullOrEmpty(apGP.earfcndl))
                                {
                                    gAppUpper.Body.dic.Add("earfcndl", apGP.earfcndl);
                                }

                                if (!string.IsNullOrEmpty(apGP.earfcnul))
                                {
                                    gAppUpper.Body.dic.Add("earfcnul", apGP.earfcnul);
                                }

                                // 2018-06-26
                                if (!string.IsNullOrEmpty(apGP.cellid))
                                {
                                    gAppUpper.Body.dic.Add("cellid", apGP.cellid);
                                }

                                if (!string.IsNullOrEmpty(apGP.pci))
                                {
                                    gAppUpper.Body.dic.Add("pci", apGP.pci);
                                }

                                if (!string.IsNullOrEmpty(apGP.bandwidth))
                                {
                                    gAppUpper.Body.dic.Add("bandwidth", apGP.bandwidth);
                                }

                                if (!string.IsNullOrEmpty(apGP.tac))
                                {
                                    gAppUpper.Body.dic.Add("tac", apGP.tac);
                                }

                                if (!string.IsNullOrEmpty(apGP.txpower))
                                {
                                    gAppUpper.Body.dic.Add("txpower", apGP.txpower);
                                }

                                if (!string.IsNullOrEmpty(apGP.periodtac))
                                {
                                    gAppUpper.Body.dic.Add("periodtac", apGP.periodtac);
                                }

                                if (!string.IsNullOrEmpty(apGP.manualfreq))
                                {
                                    gAppUpper.Body.dic.Add("manualfreq", apGP.manualfreq);
                                }

                                if (!string.IsNullOrEmpty(apGP.bootMode))
                                {
                                    gAppUpper.Body.dic.Add("bootMode", apGP.bootMode);
                                }

                                if (!string.IsNullOrEmpty(apGP.Earfcnlist))
                                {
                                    gAppUpper.Body.dic.Add("Earfcnlist", apGP.Earfcnlist);
                                }

                                if (!string.IsNullOrEmpty(apGP.Bandoffset))
                                {
                                    gAppUpper.Body.dic.Add("Bandoffset", apGP.Bandoffset);
                                }

                                if (!string.IsNullOrEmpty(apGP.NTP))
                                {
                                    gAppUpper.Body.dic.Add("NTP", apGP.NTP);
                                }

                                if (!string.IsNullOrEmpty(apGP.ntppri))
                                {
                                    gAppUpper.Body.dic.Add("ntppri", apGP.ntppri);
                                }

                                if (!string.IsNullOrEmpty(apGP.source))
                                {
                                    gAppUpper.Body.dic.Add("source", apGP.source);
                                }

                                if (!string.IsNullOrEmpty(apGP.ManualEnable))
                                {
                                    gAppUpper.Body.dic.Add("ManualEnable", apGP.ManualEnable);
                                }

                                if (!string.IsNullOrEmpty(apGP.ManualEarfcn))
                                {
                                    gAppUpper.Body.dic.Add("ManualEarfcn", apGP.ManualEarfcn);
                                }

                                if (!string.IsNullOrEmpty(apGP.ManualPci))
                                {
                                    gAppUpper.Body.dic.Add("ManualPci", apGP.ManualPci);
                                }

                                if (!string.IsNullOrEmpty(apGP.ManualBw))
                                {
                                    gAppUpper.Body.dic.Add("ManualBw", apGP.ManualBw);
                                }

                                if (!string.IsNullOrEmpty(apGP.gpsConfig))
                                {
                                    gAppUpper.Body.dic.Add("gpsConfig", apGP.gpsConfig);
                                }

                                // 2018-07-23
                                if (!string.IsNullOrEmpty(apGP.otherplmn))
                                {
                                    gAppUpper.Body.dic.Add("otherplmn", apGP.otherplmn);
                                }

                                if (!string.IsNullOrEmpty(apGP.periodFreq))
                                {
                                    gAppUpper.Body.dic.Add("periodFreq", apGP.periodFreq);
                                }

                                if (!string.IsNullOrEmpty(apGP.res1))
                                {
                                    gAppUpper.Body.dic.Add("res1", apGP.res1);
                                }

                                if (!string.IsNullOrEmpty(apGP.res2))
                                {
                                    gAppUpper.Body.dic.Add("res2", apGP.res2);
                                }

                                if (!string.IsNullOrEmpty(apGP.res3))
                                {
                                    gAppUpper.Body.dic.Add("res3", apGP.res3);
                                }

                                if (!string.IsNullOrEmpty(apGP.activeTime1Start))
                                {
                                    gAppUpper.Body.dic.Add("activeTime1Start", apGP.activeTime1Start);
                                }

                                if (!string.IsNullOrEmpty(apGP.activeTime1Ended))
                                {
                                    gAppUpper.Body.dic.Add("activeTime1Ended", apGP.activeTime1Ended);
                                }

                                if (!string.IsNullOrEmpty(apGP.activeTime2Start))
                                {
                                    gAppUpper.Body.dic.Add("activeTime2Start", apGP.activeTime2Start);
                                }

                                if (!string.IsNullOrEmpty(apGP.activeTime2Ended))
                                {
                                    gAppUpper.Body.dic.Add("activeTime2Ended", apGP.activeTime2Ended);
                                }

                                if (!string.IsNullOrEmpty(apGP.activeTime3Start))
                                {
                                    gAppUpper.Body.dic.Add("activeTime3Start", apGP.activeTime3Start);
                                }

                                if (!string.IsNullOrEmpty(apGP.activeTime3Ended))
                                {
                                    gAppUpper.Body.dic.Add("activeTime3Ended", apGP.activeTime3Ended);
                                }

                                if (!string.IsNullOrEmpty(apGP.activeTime4Start))
                                {
                                    gAppUpper.Body.dic.Add("activeTime4Start", apGP.activeTime4Start);
                                }

                                if (!string.IsNullOrEmpty(apGP.activeTime4Ended))
                                {
                                    gAppUpper.Body.dic.Add("activeTime4Ended", apGP.activeTime4Ended);
                                }

                                if (!string.IsNullOrEmpty(apGP.time))
                                {
                                    gAppUpper.Body.dic.Add("time", apGP.time);
                                }
                            }

                            Send_Msg_2_AppCtrl_Upper(gAppUpper);
                            break;

                            #endregion                                         
                        }
                    case AppMsgType.app_ftp_oper_request:
                        {
                            #region 获取信息
                            
                            //   "md5":"asdfadfs4adf3d3adf3",   //fileName的MD5
                            //   "fileName":"xxxxx.tar.gz",     //要上传的文件名
                            //   "version":"x.y.z",             //要上传的版本号                                  
                            
                            string md5 = "";
                            string fileName = "";
                            string version = "";                            

                            if (gAppUpper.Body.dic.ContainsKey("md5"))
                            {
                                md5 = gAppUpper.Body.dic["md5"].ToString();
                            }

                            if (gAppUpper.Body.dic.ContainsKey("fileName"))
                            {
                                fileName = gAppUpper.Body.dic["fileName"].ToString();
                            }

                            if (gAppUpper.Body.dic.ContainsKey("version"))
                            {
                                version = gAppUpper.Body.dic["version"].ToString();
                            }                            

                            if (md5 == "" || fileName == "" || version == "")
                            {
                                add_log_info(LogInfoType.EROR, "app_ftp_oper_request,参数有误", "Main", LogCategory.I);
                                Logger.Trace(LogInfoType.EROR, "app_ftp_oper_request,参数有误", "Main", LogCategory.I);

                                //返回出错处理
                                gAppUpper.Body.type = AppMsgType.app_ftp_oper_response;
                                gAppUpper.Body.dic = new Dictionary<string, object>();
                                gAppUpper.Body.dic.Add("ReturnCode", -1);
                                gAppUpper.Body.dic.Add("ReturnStr", get_debug_info() + "app_ftp_oper_request,参数有误.");

                                Send_Msg_2_AppCtrl_Upper(gAppUpper);
                                break;
                            }
                    
                            gUpdateInfo.md5 = md5;
                            gUpdateInfo.fileName = fileName;
                            gUpdateInfo.version = version;

                            #endregion

                            #region 返回消息

                            //    "ReturnCode": 0，            //返回码：0,成功；其它值为失败
                            //    "ReturnStr": "成功"，         //失败原因值。ReturnCode不为0时有意义
                            //    "ftpUsrName":"root",       
                            //    "ftpPwd":"root",
                            //    "ftpRootDir": "updaeFile",
                            //    "ftpServerIp": "172.17.0.210",
                            //    "ftpPort":"21",
                            //    "needToUpdate":"0",         //是否需求上传，0不需要，1需要

                            gAppUpper.Body.type = AppMsgType.app_ftp_oper_response;
                            gAppUpper.Body.dic = new Dictionary<string, object>();
                            gAppUpper.Body.dic.Add("ReturnCode", 0);
                            gAppUpper.Body.dic.Add("ReturnStr", "成功");

                            gAppUpper.Body.dic.Add("ftpUsrName", DataController.StrFtpUserId);
                            gAppUpper.Body.dic.Add("ftpPwd", DataController.StrFtpUserPsw);
                            gAppUpper.Body.dic.Add("ftpRootDir", DataController.StrFtpUpdateDir);
                            gAppUpper.Body.dic.Add("ftpServerIp", DataController.StrFtpIpAddr);
                            gAppUpper.Body.dic.Add("ftpPort", DataController.StrFtpPort);

                            if ((int)RC.NO_EXIST == gDbHelper.update_info_record_exist(gUpdateInfo.md5))
                            {
                                //FTP服务器上不存在该文件，需要界面上传
                                gAppUpper.Body.dic.Add("needToUpdate", "1");
                                gUpdateInfo.needToUpdate = true;
                            }
                            else
                            {
                                //FTP服务器上已经存在该文件，不需要界面上传
                                gAppUpper.Body.dic.Add("needToUpdate", "0");
                                gUpdateInfo.needToUpdate = false;
                            }

                            Send_Msg_2_AppCtrl_Upper(gAppUpper);
                            break;

                            #endregion                   
                        }
                    case AppMsgType.app_ftp_update_request:
                        {
                            #region 获取信息

                            //    "canUpdateFlag":"1",        //是否可以升级，0不可以，1可以  
                            //    "fileName":"xxxxx.tar.gz",  //要上传的文件名
                            //    "deviceCnt":"2",
                            //    "device1":"设备.深圳.福田.中心广场.西北监控.电信TDD"
                            //    "device2":"设备.深圳.福田.中心广场.西北监控.移动FDD" 

                            int rtv = -1;
                            int deviceRealCnt = -1;

                            string fileName = "";
                            string deviceCnt = "";
                            string canUpdateFlag = "";

                            if (gAppUpper.Body.dic.ContainsKey("canUpdateFlag"))
                            {
                                canUpdateFlag = gAppUpper.Body.dic["canUpdateFlag"].ToString();
                            }

                            if (gAppUpper.Body.dic.ContainsKey("fileName"))
                            {
                                fileName = gAppUpper.Body.dic["fileName"].ToString();
                            }                           

                            if (gAppUpper.Body.dic.ContainsKey("deviceCnt"))
                            {
                                deviceCnt = gAppUpper.Body.dic["deviceCnt"].ToString();
                            }

                            if (canUpdateFlag == "" || fileName == "" || deviceCnt == "")
                            {
                                add_log_info(LogInfoType.EROR, "app_ftp_oper_request,参数有误", "Main", LogCategory.I);
                                Logger.Trace(LogInfoType.EROR, "app_ftp_oper_request,参数有误", "Main", LogCategory.I);

                                //返回出错处理
                                gAppUpper.Body.type = AppMsgType.app_ftp_update_response;
                                gAppUpper.Body.dic = new Dictionary<string, object>();
                                gAppUpper.Body.dic.Add("ReturnCode", -1);
                                gAppUpper.Body.dic.Add("ReturnStr", get_debug_info() + "app_ftp_update_response,参数有误.");

                                Send_Msg_2_AppCtrl_Upper(gAppUpper);
                                break;
                            }

                            try
                            {
                                deviceRealCnt = int.Parse(deviceCnt);
                                if (deviceRealCnt <= 0)
                                {
                                    add_log_info(LogInfoType.EROR, "deviceCnt,参数有误", "Main", LogCategory.I);
                                    Logger.Trace(LogInfoType.EROR, "deviceCnt,参数有误", "Main", LogCategory.I);

                                    //返回出错处理
                                    gAppUpper.Body.type = AppMsgType.app_ftp_update_response;
                                    gAppUpper.Body.dic = new Dictionary<string, object>();
                                    gAppUpper.Body.dic.Add("ReturnCode", -1);
                                    gAppUpper.Body.dic.Add("ReturnStr", get_debug_info() + "deviceCnt,参数有误.");

                                    Send_Msg_2_AppCtrl_Upper(gAppUpper);
                                    break;
                                }
                            }
                            catch (Exception ee)
                            {
                                add_log_info(LogInfoType.EROR, "deviceCnt,参数有误" + ee.Message, "Main", LogCategory.I);
                                Logger.Trace(LogInfoType.EROR, "deviceCnt,参数有误" + ee.Message, "Main", LogCategory.I);

                                //返回出错处理
                                gAppUpper.Body.type = AppMsgType.app_ftp_update_response;
                                gAppUpper.Body.dic = new Dictionary<string, object>();
                                gAppUpper.Body.dic.Add("ReturnCode", -1);
                                gAppUpper.Body.dic.Add("ReturnStr", get_debug_info() + "deviceCnt,参数有误." + ee.Message);

                                Send_Msg_2_AppCtrl_Upper(gAppUpper);
                                break;
                            }

                            string inx = "";
                            string devTmp = "";
                            gUpdateInfo.listDevFullName = new List<string>();
                            gUpdateInfo.listDevId = new List<int>();

                            for (int i = 0; i < deviceRealCnt; i++)
                            {
                                inx = "device" + (i + 1).ToString();
                                if (gAppUpper.Body.dic.ContainsKey(inx))
                                {
                                    devTmp = gAppUpper.Body.dic[inx].ToString();
                                    if (gDicDeviceId.ContainsKey(devTmp))
                                    {
                                        gUpdateInfo.listDevFullName.Add(devTmp);
                                        gUpdateInfo.listDevId.Add(gDicDeviceId[devTmp].id);
                                    }
                                }
                                else
                                {
                                    break;
                                }
                            }

                            if (gUpdateInfo.listDevId.Count != deviceRealCnt)
                            {
                                add_log_info(LogInfoType.EROR, "设备列表解析有误！", "Main", LogCategory.I);
                                Logger.Trace(LogInfoType.EROR, "设备列表解析有误！", "Main", LogCategory.I);

                                //返回出错处理
                                gAppUpper.Body.type = AppMsgType.app_ftp_update_response;
                                gAppUpper.Body.dic = new Dictionary<string, object>();
                                gAppUpper.Body.dic.Add("ReturnCode", -1);
                                gAppUpper.Body.dic.Add("ReturnStr", get_debug_info() + "设备列表解析有误！");

                                Send_Msg_2_AppCtrl_Upper(gAppUpper);
                                break;
                            }

                            if (canUpdateFlag != "0" && canUpdateFlag != "1")
                            {
                                add_log_info(LogInfoType.EROR, "canUpdateFlag,参数有误", "Main", LogCategory.I);
                                Logger.Trace(LogInfoType.EROR, "canUpdateFlag,参数有误", "Main", LogCategory.I);

                                //返回出错处理
                                gAppUpper.Body.type = AppMsgType.app_ftp_update_response;
                                gAppUpper.Body.dic = new Dictionary<string, object>();
                                gAppUpper.Body.dic.Add("ReturnCode", -1);
                                gAppUpper.Body.dic.Add("ReturnStr", get_debug_info() + "canUpdateFlag,参数有误.");

                                Send_Msg_2_AppCtrl_Upper(gAppUpper);
                                break;
                            }

                            if (canUpdateFlag == "0")
                            {
                                add_log_info(LogInfoType.EROR, "canUpdateFlag,不能升级", "Main", LogCategory.I);
                                Logger.Trace(LogInfoType.EROR, "canUpdateFlag,不能升级", "Main", LogCategory.I);

                                //返回出错处理
                                gAppUpper.Body.type = AppMsgType.app_ftp_update_response;
                                gAppUpper.Body.dic = new Dictionary<string, object>();
                                gAppUpper.Body.dic.Add("ReturnCode", -1);
                                gAppUpper.Body.dic.Add("ReturnStr", get_debug_info() + "canUpdateFlag,不能升级");

                                Send_Msg_2_AppCtrl_Upper(gAppUpper);
                                break;
                            }

                            #endregion

                            #region 返回消息

                            if (fileName != gUpdateInfo.fileName)
                            {
                                //fileName和上一条消息不一致

                                add_log_info(LogInfoType.EROR, "fileName != gUpdateInfo.fileName", "Main", LogCategory.I);
                                Logger.Trace(LogInfoType.EROR, "fileName != gUpdateInfo.fileName", "Main", LogCategory.I);

                                //返回出错处理
                                gAppUpper.Body.type = AppMsgType.app_ftp_update_response;
                                gAppUpper.Body.dic = new Dictionary<string, object>();
                                gAppUpper.Body.dic.Add("ReturnCode", -1);
                                gAppUpper.Body.dic.Add("ReturnStr", get_debug_info() + "fileName != gUpdateInfo.fileName");

                                Send_Msg_2_AppCtrl_Upper(gAppUpper);
                                break;
                            }                     

                            if(gUpdateInfo.needToUpdate == true)
                            {
                                //插入记录到升级信息表中
                                rtv = gDbHelper.update_info_record_insert(gUpdateInfo.md5, gUpdateInfo.fileName, gUpdateInfo.version);
                                if (rtv != 0)
                                {
                                    add_log_info(LogInfoType.EROR, gDbHelper.get_rtv_str(rtv), "Main", LogCategory.I);
                                    Logger.Trace(LogInfoType.EROR, gDbHelper.get_rtv_str(rtv), "Main", LogCategory.I);

                                    //返回出错处理
                                    gAppUpper.Body.type = AppMsgType.app_ftp_update_response;
                                    gAppUpper.Body.dic = new Dictionary<string, object>();
                                    gAppUpper.Body.dic.Add("ReturnCode", rtv);
                                    gAppUpper.Body.dic.Add("ReturnStr", get_debug_info() + gDbHelper.get_rtv_str(rtv));

                                    Send_Msg_2_AppCtrl_Upper(gAppUpper);
                                    break;
                                }
                            }
                                                       
                            #region 遍历设备列表发送给AP

                            int devId = -1;
                            strDevice strDev = new strDevice();
                            for (int i = 0;i < gUpdateInfo.listDevId.Count;i++)
                            {
                                devId = gUpdateInfo.listDevId[i];
                                rtv = gDbHelper.device_record_entity_get_by_devid(devId, ref strDev);
                                if (rtv == 0)
                                {
                                    if (strDev.online == "0")
                                    {
                                        continue;
                                    }

                                    //只发给上线的AP
                                    gAppUpper.ApInfo.SN = strDev.sn;
                                    gAppUpper.ApInfo.Fullname = gUpdateInfo.listDevFullName[i];
                                    gAppUpper.ApInfo.IP = strDev.ipAddr;
                                    gAppUpper.ApInfo.Port = int.Parse(strDev.port);
                                    gAppUpper.ApInfo.Type = strDev.innerType;

                                    gAppUpper.Body.type = ApMsgType.Update;
                                    gAppUpper.MsgType = MsgType.CONFIG.ToString(); 

                                    gAppUpper.Body.dic = new Dictionary<string, object>();
                                    gAppUpper.Body.dic.Add("User_name", DataController.StrFtpUserId);
                                    gAppUpper.Body.dic.Add("update_type", 0);
                                    gAppUpper.Body.dic.Add("Password", DataController.StrFtpUserPsw);
                                    gAppUpper.Body.dic.Add("timestamp", DateTime.Now.ToString());
                                    gAppUpper.Body.dic.Add("version", gUpdateInfo.version);
                                    gAppUpper.Body.dic.Add("filename", gUpdateInfo.fileName);
                                    gAppUpper.Body.dic.Add("ftp_type", 1);
                                    gAppUpper.Body.dic.Add("serverAdd", DataController.StrFtpIpAddr + ":" + DataController.StrFtpPort);

                                    //发送给ApController
                                    Send_Msg_2_ApCtrl_Lower(gAppUpper);
                                }
                            }                            

                            #endregion

                            break;

                            #endregion                                        
                        }
                    case AppMsgType.app_history_record_request:
                        {
                            #region 异步处理

                            BeginInvoke(new history_record_process_delegate(history_record_process_delegate_fun), new object[] { gAppUpper });
                            break;

                            #endregion
                        }
                    case AppMsgType.app_history_record_next_page_request:
                        {
                            #region 获取信息

                            string appId = Get_App_Info(gAppUpper);

                            // 
                            // GSW和WCDMA的设备，IMSI和IMEI可同时设置
                            //
                            // "CurPageIndex":"1:50",   
                            //                                                 

                            string CurPageIndex = "";
                            int curPageInx = -1;
                            int totalPages = -1;

                            if (gAppUpper.Body.dic.ContainsKey("CurPageIndex"))
                            {
                                CurPageIndex = gAppUpper.Body.dic["CurPageIndex"].ToString();
                                if (check_and_get_page_info(CurPageIndex, ref curPageInx, ref totalPages) == false)
                                {
                                    string errInfo = get_debug_info() + string.Format("{0}:", "CurPageIndex字段解析出错.");
                                    add_log_info(LogInfoType.EROR, errInfo, "Main", LogCategory.I);
                                    Logger.Trace(LogInfoType.EROR, errInfo, "Main", LogCategory.I);

                                    Fill_IMMS_Info(ref gAppUpper, AppMsgType.app_history_record_response, -1, errInfo, true, null, null);
                                    Send_Msg_2_AppCtrl_Upper(gAppUpper);
                                    break;
                                }
                            }
                            else
                            {
                                string errInfo = get_debug_info() + string.Format("{0}:", "没包含字段CurPageIndex");
                                add_log_info(LogInfoType.EROR, errInfo, "Main", LogCategory.I);
                                Logger.Trace(LogInfoType.EROR, errInfo, "Main", LogCategory.I);

                                Fill_IMMS_Info(ref gAppUpper, AppMsgType.app_history_record_response, -1, errInfo, true, null, null);
                                Send_Msg_2_AppCtrl_Upper(gAppUpper);
                                break;
                            }

                            if (string.IsNullOrEmpty(appId))
                            {
                                string errInfo = get_debug_info() + "获取AppInfo的IP和Port失败.";
                                add_log_info(LogInfoType.EROR, errInfo, "Main", LogCategory.I);
                                Logger.Trace(LogInfoType.EROR, errInfo, "Main", LogCategory.I);

                                Fill_IMMS_Info(ref gAppUpper, AppMsgType.app_history_record_response, -1, errInfo, true, null, null);
                                Send_Msg_2_AppCtrl_Upper(gAppUpper);
                                break;
                            }

                            #endregion

                            #region 返回消息

                            if (!gDicCaptureQueryInfo.ContainsKey(appId))
                            {
                                string errInfo = get_debug_info() + string.Format("{0}:对应的查询信息不存在.", appId);
                                add_log_info(LogInfoType.EROR, errInfo, "Main", LogCategory.I);
                                Logger.Trace(LogInfoType.EROR, errInfo, "Main", LogCategory.I);

                                Fill_IMMS_Info(ref gAppUpper, AppMsgType.app_history_record_response, -1, errInfo, true, null, null);
                                Send_Msg_2_AppCtrl_Upper(gAppUpper);
                                break;
                            }

                            strCaptureQueryInfo qi = gDicCaptureQueryInfo[appId];
                            if (qi.totalPages != totalPages)
                            {
                                string errInfo = get_debug_info() + string.Format("{0}:对应的总页数不匹配.", appId);
                                add_log_info(LogInfoType.EROR, errInfo, "Main", LogCategory.I);
                                Logger.Trace(LogInfoType.EROR, errInfo, "Main", LogCategory.I);

                                Fill_IMMS_Info(ref gAppUpper, AppMsgType.app_history_record_response, -1, errInfo, true, null, null);
                                Send_Msg_2_AppCtrl_Upper(gAppUpper);
                                break;
                            }

                            int startInx = -1;
                            int endedInx = -1;

                            if (curPageInx == totalPages)
                            {
                                //最后一页
                                startInx = (curPageInx - 1) * qi.pageSize;
                                endedInx = qi.totalRecords;
                            }
                            else
                            {
                                //不是最后一页
                                startInx = (curPageInx - 1) * qi.pageSize;
                                endedInx = startInx + qi.pageSize;
                            }

                            #region 取出各条记录

                            string pageInfo = string.Format("{0}:{1}", curPageInx, qi.totalPages);
                            Fill_IMMS_Info(ref gAppUpper, AppMsgType.app_history_record_response, 0, gDbHelper.get_rtv_str(0), true, null, null);

                            gAppUpper.Body.dic.Add("TotalRecords", qi.totalRecords.ToString());
                            gAppUpper.Body.dic.Add("CurPageIndex", pageInfo);
                            gAppUpper.Body.dic.Add("PageSize", qi.pageSize.ToString());

                            for (int inx = startInx; inx < endedInx; inx++)
                            {
                                DataRow dr = gDicCaptureQueryInfo[appId].dt.Rows[inx];

                                Name_DIC_Struct ndic = new Name_DIC_Struct();
                                ndic.name = (inx + 1).ToString();

                                if (string.IsNullOrEmpty(dr["imsi"].ToString()))
                                {
                                    ndic.dic.Add("imsi", "null");
                                }
                                else
                                {
                                    ndic.dic.Add("imsi", dr["imsi"].ToString());
                                }

                                if (string.IsNullOrEmpty(dr["imei"].ToString()))
                                {
                                    ndic.dic.Add("imei", "null");
                                }
                                else
                                {
                                    ndic.dic.Add("imei", dr["imei"].ToString());
                                }

                                if (string.IsNullOrEmpty(dr["name"].ToString()))
                                {
                                    ndic.dic.Add("name", "null");
                                }
                                else
                                {
                                    ndic.dic.Add("name", dr["name"].ToString());
                                }

                                if (string.IsNullOrEmpty(dr["time"].ToString()))
                                {
                                    ndic.dic.Add("time", "null");
                                }
                                else
                                {
                                    ndic.dic.Add("time", dr["time"].ToString());
                                }

                                if (string.IsNullOrEmpty(dr["bwFlag"].ToString()))
                                {
                                    ndic.dic.Add("bwFlag", "bwFlag");
                                }
                                else
                                {
                                    ndic.dic.Add("bwFlag", dr["bwFlag"].ToString());
                                }

                                if (string.IsNullOrEmpty(dr["sn"].ToString()))
                                {
                                    ndic.dic.Add("sn", "sn");
                                }
                                else
                                {
                                    ndic.dic.Add("sn", dr["sn"].ToString());
                                }

                                gAppUpper.Body.n_dic.Add(ndic);
                            }

                            #endregion


                            Send_Msg_2_AppCtrl_Upper(gAppUpper);
                            break;

                            #endregion                                     
                        }
                    case AppMsgType.app_history_record_export_csv_request:
                        {
                            #region 获取信息

                            string appId = Get_App_Info(gAppUpper);

                            //                         
                            //
                            // "fileName":"abc.csv"
                            //                                                 
                          
                            string fileName = "";                       

                            if (gAppUpper.Body.dic.ContainsKey("fileName"))
                            {
                                fileName = gAppUpper.Body.dic["fileName"].ToString();

                                if (false == IsValidFileName(fileName))
                                {
                                    string errInfo = get_debug_info() + string.Format("{0}:", "文件名非法.");
                                    add_log_info(LogInfoType.EROR, errInfo, "Main", LogCategory.I);
                                    Logger.Trace(LogInfoType.EROR, errInfo, "Main", LogCategory.I);

                                    Fill_IMMS_Info(ref gAppUpper, AppMsgType.app_history_record_export_csv_response, -1, errInfo, true, null, null);
                                    Send_Msg_2_AppCtrl_Upper(gAppUpper);
                                    break;
                                }
                                else
                                {
                                    string extension = System.IO.Path.GetExtension(fileName);
                                    if (extension != ".csv")
                                    {
                                        string errInfo = get_debug_info() + string.Format("{0}:", "后缀名非法.");
                                        add_log_info(LogInfoType.EROR, errInfo, "Main", LogCategory.I);
                                        Logger.Trace(LogInfoType.EROR, errInfo, "Main", LogCategory.I);

                                        Fill_IMMS_Info(ref gAppUpper, AppMsgType.app_history_record_export_csv_response, -1, errInfo, true, null, null);
                                        Send_Msg_2_AppCtrl_Upper(gAppUpper);
                                        break;
                                    }
                                }                                
                            }
                            else
                            {
                                string errInfo = get_debug_info() + string.Format("{0}:", "没包含字段fileName");
                                add_log_info(LogInfoType.EROR, errInfo, "Main", LogCategory.I);
                                Logger.Trace(LogInfoType.EROR, errInfo, "Main", LogCategory.I);

                                Fill_IMMS_Info(ref gAppUpper, AppMsgType.app_history_record_export_csv_response, -1, errInfo, true, null, null);
                                Send_Msg_2_AppCtrl_Upper(gAppUpper);
                                break;
                            }


                            if (string.IsNullOrEmpty(appId))
                            {
                                string errInfo = get_debug_info() + "获取AppInfo的IP和Port失败.";
                                add_log_info(LogInfoType.EROR, errInfo, "Main", LogCategory.I);
                                Logger.Trace(LogInfoType.EROR, errInfo, "Main", LogCategory.I);

                                Fill_IMMS_Info(ref gAppUpper, AppMsgType.app_history_record_export_csv_response, -1, errInfo, true, null, null);
                                Send_Msg_2_AppCtrl_Upper(gAppUpper);
                                break;
                            }

                            #endregion

                            #region 返回消息

                            if (!gDicCaptureQueryInfo.ContainsKey(appId))
                            {
                                string errInfo = get_debug_info() + string.Format("{0}:对应的查询信息不存在.", appId);
                                add_log_info(LogInfoType.EROR, errInfo, "Main", LogCategory.I);
                                Logger.Trace(LogInfoType.EROR, errInfo, "Main", LogCategory.I);

                                Fill_IMMS_Info(ref gAppUpper, AppMsgType.app_history_record_export_csv_response, -1, errInfo, true, null, null);
                                Send_Msg_2_AppCtrl_Upper(gAppUpper);
                                break;
                            }

                            strCaptureQueryInfo qi = gDicCaptureQueryInfo[appId];

                            int rtv = -1;
                            byte[] outData = null;
                            if (0 == generate_ftp_byte_csv(ref outData, qi.dt))
                            {
                                try
                                {
                                    //gFtpHelperFile.RemotePath = DataController.StrFtpUpdateDir;
                                    //gFtpHelperFile.Connect();

                                    //if (gFtpHelperFile.RemotePath != DataController.StrFtpUpdateDir)
                                    //{
                                    //    gFtpHelperFile.RemotePath = DataController.StrFtpUpdateDir;
                                    //    gFtpHelperFile.Connect();
                                    //}


                                    rtv = gFtpHelperFile.Put(fileName, outData);
                                    if (rtv == -1)
                                    {
                                        string errInfo = get_debug_info() + string.Format("上传文件失败.");
                                        add_log_info(LogInfoType.EROR, errInfo, "Main", LogCategory.I);
                                        Logger.Trace(LogInfoType.EROR, errInfo, "Main", LogCategory.I);

                                        Fill_IMMS_Info(ref gAppUpper, AppMsgType.app_history_record_export_csv_response, -1, errInfo, true, null, null);
                                        Send_Msg_2_AppCtrl_Upper(gAppUpper);
                                        break;
                                    }
                                }
                                catch (Exception ee)
                                {
                                    add_log_info(LogInfoType.EROR, ee.Message, "Main", LogCategory.I);
                                    Logger.Trace(LogInfoType.EROR, ee.Message, "Main", LogCategory.I);

                                    gAppUpper.Body.type = AppMsgType.app_history_record_export_csv_response;
                                    gAppUpper.Body.dic = new Dictionary<string, object>();
                                    gAppUpper.Body.dic.Add("ReturnCode", -1);
                                    gAppUpper.Body.dic.Add("ReturnStr", "失败");                               
                                    gAppUpper.Body.dic.Add("ftpUsrName", DataController.StrFtpUserId);
                                    gAppUpper.Body.dic.Add("ftpPwd", DataController.StrFtpUserPsw);
                                    gAppUpper.Body.dic.Add("ftpRootDir", DataController.StrFtpUpdateDir);
                                    gAppUpper.Body.dic.Add("ftpServerIp", DataController.StrFtpIpAddr);
                                    gAppUpper.Body.dic.Add("ftpPort", DataController.StrFtpPort);
                                    gAppUpper.Body.dic.Add("fileName", fileName);

                                    Send_Msg_2_AppCtrl_Upper(gAppUpper);
                                    break;
                                }

                                //else
                                //{
                                //    gFtpHelper.RemotePath = DataController.StrFtpImsiDir;
                                //    gFtpHelper.Connect();
                                //}
                            }

                            gAppUpper.Body.type = AppMsgType.app_history_record_export_csv_response;
                            gAppUpper.Body.dic = new Dictionary<string, object>();

                            if (rtv == 0)
                            {
                                gAppUpper.Body.dic.Add("ReturnCode", 0);
                                gAppUpper.Body.dic.Add("ReturnStr", "成功");
                            }
                            else
                            {
                                gAppUpper.Body.dic.Add("ReturnCode", -1);
                                gAppUpper.Body.dic.Add("ReturnStr", "失败");
                            }

                            gAppUpper.Body.dic.Add("ftpUsrName", DataController.StrFtpUserId);
                            gAppUpper.Body.dic.Add("ftpPwd", DataController.StrFtpUserPsw);
                            gAppUpper.Body.dic.Add("ftpRootDir", DataController.StrFtpUpdateDir);
                            gAppUpper.Body.dic.Add("ftpServerIp", DataController.StrFtpIpAddr);
                            gAppUpper.Body.dic.Add("ftpPort", DataController.StrFtpPort);
                            gAppUpper.Body.dic.Add("fileName", fileName);

                            Send_Msg_2_AppCtrl_Upper(gAppUpper);
                            break;

                            #endregion                                     
                        }
                    case AppMsgType.app_del_device_unknown_request:
                        {
                            #region 获取信息

                            //   "ipAddr":"172.17.0.210",
                            //   "port":"12345"
                            //   "fullname":"设备.深圳.福田.中心广场.西北监控.电信FDD" 

                            string ipAddr = "";
                            string port = "";
                            string fullname = "";

                            if (gAppUpper.Body.dic.ContainsKey("ipAddr"))
                            {
                                ipAddr = gAppUpper.Body.dic["ipAddr"].ToString();
                            }

                            if (gAppUpper.Body.dic.ContainsKey("port"))
                            {
                                port = gAppUpper.Body.dic["port"].ToString();
                            }

                            if (gAppUpper.Body.dic.ContainsKey("fullname"))
                            {
                                fullname = gAppUpper.Body.dic["fullname"].ToString();
                            }

                            if (ipAddr == "" || port == "")
                            {
                                string errInfo = get_debug_info() + "获取ipAddr或port失败.";
                                add_log_info(LogInfoType.EROR, errInfo, "Main", LogCategory.I);
                                Logger.Trace(LogInfoType.EROR, errInfo, "Main", LogCategory.I);

                                Fill_IMMS_Info(ref gAppUpper, AppMsgType.app_del_device_unknown_response, -1, errInfo, true, null, null);
                                Send_Msg_2_AppCtrl_Upper(gAppUpper);
                                break;
                            }

                            if (fullname == "" )
                            {
                                string errInfo = get_debug_info() + "获取fullname失败.";
                                add_log_info(LogInfoType.EROR, errInfo, "Main", LogCategory.I);
                                Logger.Trace(LogInfoType.EROR, errInfo, "Main", LogCategory.I);

                                Fill_IMMS_Info(ref gAppUpper, AppMsgType.app_del_device_unknown_response, -1, errInfo, true, null, null);
                                Send_Msg_2_AppCtrl_Upper(gAppUpper);
                                break;
                            }

                            if ((int)RC.NO_EXIST == gDbHelper.device_unknown_record_exist(ipAddr, int.Parse(port)))
                            {
                                string errInfo = string.Format("{0}:{1}对应的未指派设备不存在.", ipAddr, port);

                                add_log_info(LogInfoType.EROR, errInfo, "Main", LogCategory.I);
                                Logger.Trace(LogInfoType.EROR, errInfo, "Main", LogCategory.I);

                                Fill_IMMS_Info(ref gAppUpper, AppMsgType.app_del_device_unknown_response, -1, errInfo, true, null, null);
                                Send_Msg_2_AppCtrl_Upper(gAppUpper);
                                break;
                            }

                            #endregion

                            #region 发下命令设置fullname           

                            DataTable dt = new DataTable();
                            if (0 != gDbHelper.device_unknown_record_entity_get_by_ipaddr_port(ipAddr, int.Parse(port), ref dt))
                            {
                                string errInfo = get_debug_info() + "device_unknown_record_entity_get_by_ipaddr_port失败.";
                                add_log_info(LogInfoType.EROR, errInfo, "Main", LogCategory.I);
                                Logger.Trace(LogInfoType.EROR, errInfo, "Main", LogCategory.I);

                                Fill_IMMS_Info(ref gAppUpper, AppMsgType.app_del_device_unknown_response, -1, errInfo, true, null, null);
                                Send_Msg_2_AppCtrl_Upper(gAppUpper);
                                break;
                            }

                            if (dt.Rows.Count == 0)
                            {
                                string errInfo = get_debug_info() + "dt为空.";
                                add_log_info(LogInfoType.EROR, errInfo, "Main", LogCategory.I);
                                Logger.Trace(LogInfoType.EROR, errInfo, "Main", LogCategory.I);

                                Fill_IMMS_Info(ref gAppUpper, AppMsgType.app_del_device_unknown_response, -1, errInfo, true, null, null);
                                Send_Msg_2_AppCtrl_Upper(gAppUpper);
                                break;
                            }


                            gAppUpper.ApInfo.SN = dt.Rows[0]["sn"].ToString();
                            gAppUpper.ApInfo.Fullname = fullname;
                            gAppUpper.ApInfo.IP = ipAddr;
                            gAppUpper.ApInfo.Port = int.Parse(port);
                            gAppUpper.ApInfo.Type = dt.Rows[0]["innerType"].ToString();

                            gAppUpper.Body.type = ApMsgType.set_parameter_request;
                            gAppUpper.MsgType = MsgType.CONFIG.ToString();

                            gAppUpper.Body.dic = new Dictionary<string, object>();
                            gAppUpper.Body.dic.Add("paramName", "CFG_FULL_NAME");


                            //byte[] gbk = Encoding.GetEncoding("GBK").GetBytes(fullname);

                           // byte[] gbk = Encoding.Default.GetBytes(fullname);


                            //string code = "";
                            //foreach (byte b in gbk)
                            //{
                            //    code += string.Format("{0:X2}", b);
                            //}

                            //code = Encoding.GetEncoding("GBK").GetString(Encoding.GetEncoding("GBK").GetBytes(fullname));

                            // string code = Encoding.GetEncoding("GBK").GetString(gbk);

                           // string code = Encoding.GetEncoding("GBK").GetString(gbk);                           

                            //string temp = string.Empty;
                            //UTF8Encoding utf8 = new UTF8Encoding();

                            //byte[] encodedBytes = utf8.GetBytes(fullname);

                            //foreach (byte b in encodedBytes)
                            //{
                            //    temp += "%" + b.ToString("X");
                            //}

                            //fullname = Encoding.UTF8.GetString(Encoding.UTF8.GetBytes(fullname));

                            gAppUpper.Body.dic.Add("paramValue", fullname);

                            //发送给ApController
                            Send_Msg_2_ApCtrl_Lower(gAppUpper);

                            #endregion

                            #region 启动超时计时器 

                            gTimerSetFullName = new TaskTimer();
                            gTimerSetFullName.Interval = DataController.TimerTimeOutInterval * 1000;

                            gTimerSetFullName.Id = 0;
                            gTimerSetFullName.Name = "gTimerSetFullName";
                            gTimerSetFullName.MsgType = AppMsgType.app_del_device_unknown_response;
                            gTimerSetFullName.TimeOutFlag = false;
                            gTimerSetFullName.Imms = gAppUpper;

                            gTimerSetFullName.Elapsed += new System.Timers.ElapsedEventHandler(TimerFunc);
                            gTimerSetFullName.Start();                            

                            #endregion

                            break;
                        }
                    case AppMsgType.app_get_GsmInfo_Request:
                        {
                            #region 获取信息

                            //   "parentFullPathName":"设备.深圳.福田.中心广场.西北监控",
                            //   "name":"GSM-Name"     //GSM的名称
                            //   "carry":"0"           //GSM的载波标识，"0"或者"1"

                            int rtv = -1;
                            strDevice devInfo = new strDevice();                       
                            string mode = "";

                            string parentFullPathName = "";
                            string name = "";
                            string carry = "";

                            if (gAppUpper.Body.dic.ContainsKey("parentFullPathName"))
                            {
                                parentFullPathName = gAppUpper.Body.dic["parentFullPathName"].ToString();
                            }

                            if (gAppUpper.Body.dic.ContainsKey("name"))
                            {
                                name = gAppUpper.Body.dic["name"].ToString();
                            }

                            if (gAppUpper.Body.dic.ContainsKey("carry"))
                            {
                                carry = gAppUpper.Body.dic["carry"].ToString();
                            }

                            if (parentFullPathName == "" || name == "" || carry == "")
                            {
                                string errInfo = get_debug_info() + string.Format("app_get_GsmInfo_Request,参数有误");
                                add_log_info(LogInfoType.EROR, errInfo, "Main", LogCategory.I);
                                Logger.Trace(LogInfoType.EROR, errInfo, "Main", LogCategory.I);

                                Fill_IMMS_Info(ref gAppUpper, AppMsgType.app_get_GsmInfo_Response, -1, errInfo, true, null, null);
                                Send_Msg_2_AppCtrl_Upper(gAppUpper);
                                break;
                            }

                            #endregion

                            #region 返回消息
                           
                            strDomian domian = new strDomian();
                            string devFullPathName = string.Format("{0}.{1}", parentFullPathName, name);

                            if (!gDicDeviceId.ContainsKey(devFullPathName))
                            {                  
                                string errInfo = string.Format("{0}:对应的设备ID在gDicDeviceId中找不到", devFullPathName);
                                add_log_info(LogInfoType.EROR, errInfo, "Main", LogCategory.I);
                                Logger.Trace(LogInfoType.EROR, errInfo, "Main", LogCategory.I);

                                Fill_IMMS_Info(ref gAppUpper, AppMsgType.app_get_GsmInfo_Response, -1, get_debug_info() + errInfo, true, null, null);
                                Send_Msg_2_AppCtrl_Upper(gAppUpper);
                                break;
                            }
                            else
                            {
                                devInfo = gDicDeviceId[devFullPathName];
                                rtv = gDbHelper.domain_record_get_by_nameFullPath(parentFullPathName, ref domian);
                                if (rtv == 0)
                                {
                                    int id = -1;
                                    rtv = gDbHelper.device_record_id_get_by_affdomainid_and_name(domian.id, name, ref id, ref mode);
                                }
                            }


                            str_Gsm_All_Para allInfo = new str_Gsm_All_Para();

                            //(1)
                            rtv = gDbHelper.gsm_sys_para_record_get_by_devid(int.Parse(carry), devInfo.id, ref allInfo.gsmSysPara);
                            if (rtv != 0)
                            {
                                string errInfo = string.Format("gsm_sys_para_record_get_by_devid出错：") + gDbHelper.get_rtv_str(rtv);
                                add_log_info(LogInfoType.EROR, errInfo, "Main", LogCategory.I);
                                Logger.Trace(LogInfoType.EROR, errInfo, "Main", LogCategory.I);

                                Fill_IMMS_Info(ref gAppUpper, AppMsgType.app_get_GsmInfo_Response, -1, errInfo, true, null, null);
                                Send_Msg_2_AppCtrl_Upper(gAppUpper);
                                break;
                            }

                            //(2)
                            rtv = gDbHelper.gsm_sys_option_record_get_by_devid(int.Parse(carry), devInfo.id, ref allInfo.gsmSysOption);
                            if (rtv != 0)
                            {
                                string errInfo = string.Format("gsm_sys_option_record_get_by_devid出错:")  +gDbHelper.get_rtv_str(rtv); ;
                                add_log_info(LogInfoType.EROR, errInfo, "Main", LogCategory.I);
                                Logger.Trace(LogInfoType.EROR, errInfo, "Main", LogCategory.I);

                                Fill_IMMS_Info(ref gAppUpper, AppMsgType.app_get_GsmInfo_Response, -1, errInfo, true, null, null);
                                Send_Msg_2_AppCtrl_Upper(gAppUpper);
                                break;
                            }

                            //(3)
                            rtv = gDbHelper.gsm_rf_para_record_get_by_devid(int.Parse(carry), devInfo.id, ref allInfo.gsmRfPara);
                            if (rtv != 0)
                            {
                                string errInfo = string.Format("gsm_rf_para_record_get_by_devid出错") + gDbHelper.get_rtv_str(rtv);
                                add_log_info(LogInfoType.EROR, errInfo, "Main", LogCategory.I);
                                Logger.Trace(LogInfoType.EROR, errInfo, "Main", LogCategory.I);

                                Fill_IMMS_Info(ref gAppUpper, AppMsgType.app_get_GsmInfo_Response, -1, errInfo, true, null, null);
                                Send_Msg_2_AppCtrl_Upper(gAppUpper);
                                break;
                            }

                            //(4)
                            rtv = gDbHelper.gsm_msg_option_get_by_devid(int.Parse(carry), devInfo.id, ref allInfo.listGMO);
                            if (rtv != 0)
                            {
                                string errInfo = string.Format("gsm_msg_option_get_by_devid出错:") + gDbHelper.get_rtv_str(rtv); ;
                                add_log_info(LogInfoType.EROR, errInfo, "Main", LogCategory.I);
                                Logger.Trace(LogInfoType.EROR, errInfo, "Main", LogCategory.I);

                                Fill_IMMS_Info(ref gAppUpper, AppMsgType.app_get_GsmInfo_Response, -1, errInfo, true, null, null);
                                Send_Msg_2_AppCtrl_Upper(gAppUpper);
                                break;
                            }

                            Fill_IMMS_Info(ref gAppUpper, AppMsgType.app_get_GsmInfo_Response, rtv, gDbHelper.get_rtv_str(rtv), true, null, null);

                            gAppUpper.Body.dic.Add("domainId", domian.id.ToString());
                            gAppUpper.Body.dic.Add("domainParentId", domian.parentId.ToString());
                            gAppUpper.Body.dic.Add("parentFullPathName", parentFullPathName);
                            gAppUpper.Body.dic.Add("name", name);
                            gAppUpper.Body.dic.Add("carry", carry);


                            //					"paraMcc":移动国家码
                            //					"paraMnc":移动网号
                            //					"paraBsic":基站识别码
                            //					"paraLac":位置区号
                            //					"paraCellId":小区ID
                            //					"paraC2":C2偏移量
                            //					"paraPeri":周期性位置更新周期
                            //					"paraAccPwr":接入功率
                            //					"paraMsPwr":手机发射功率
                            //					"paraRejCau":位置更新拒绝原因
                            gAppUpper.Body.dic.Add("paraMcc", allInfo.gsmSysPara.paraMcc);
                            gAppUpper.Body.dic.Add("paraMnc", allInfo.gsmSysPara.paraMnc);
                            gAppUpper.Body.dic.Add("paraBsic", allInfo.gsmSysPara.paraBsic);
                            gAppUpper.Body.dic.Add("paraLac", allInfo.gsmSysPara.paraLac);
                            gAppUpper.Body.dic.Add("paraCellId", allInfo.gsmSysPara.paraCellId);
                            gAppUpper.Body.dic.Add("paraC2", allInfo.gsmSysPara.paraC2);
                            gAppUpper.Body.dic.Add("paraPeri", allInfo.gsmSysPara.paraPeri);
                            gAppUpper.Body.dic.Add("paraAccPwr", allInfo.gsmSysPara.paraAccPwr);
                            gAppUpper.Body.dic.Add("paraMsPwr", allInfo.gsmSysPara.paraMsPwr);
                            gAppUpper.Body.dic.Add("paraRejCau", allInfo.gsmSysPara.paraRejCau);

                            //					"opLuSms":登录时发送短信
                            //					"opLuImei":登录时获取IMEI
                            //					"opCallEn":允许用户主叫
                            //					"opDebug":调试模式，上报信令
                            //					"opLuType":登录类型
                            //					"opSmsType":短信类型
                            //                  "opRegModel":登录模式
                            gAppUpper.Body.dic.Add("opLuSms", allInfo.gsmSysOption.opLuSms);
                            gAppUpper.Body.dic.Add("opLuImei", allInfo.gsmSysOption.opLuImei);
                            gAppUpper.Body.dic.Add("opCallEn", allInfo.gsmSysOption.opCallEn);
                            gAppUpper.Body.dic.Add("opDebug", allInfo.gsmSysOption.opDebug);
                            gAppUpper.Body.dic.Add("opLuType", allInfo.gsmSysOption.opLuType);
                            gAppUpper.Body.dic.Add("opSmsType", allInfo.gsmSysOption.opSmsType);
                            gAppUpper.Body.dic.Add("opRegModel", allInfo.gsmSysOption.opRegModel);


                            //					"rfEnable":射频使能
                            //					"rfFreq":信道号
                            //					"rfPwr":发射功率衰减值
                            gAppUpper.Body.dic.Add("rfEnable", allInfo.gsmRfPara.rfEnable);
                            gAppUpper.Body.dic.Add("rfFreq", allInfo.gsmRfPara.rfFreq);
                            gAppUpper.Body.dic.Add("rfPwr", allInfo.gsmRfPara.rfPwr);

                            if (allInfo.listGMO.Count > 0)
                            {
                                //          "smsRPOA":短消息中心号码
                                //          "smsTPOA":短消息原叫号码
                                //          "smsSCTS":短消息发送时间  
                                //          "smsDATA":短消息内容 （编码格式为Unicode编码）
                                //          "autoSend":是否自动发送
                                //          "autoFilterSMS":是否自动过滤短信
                                //          "delayTime":发送延时时间
                                //          "smsCoding":短信的编码格式

                                gAppUpper.Body.n_dic = new List<Name_DIC_Struct>();
                                for (int i = 0; i < allInfo.listGMO.Count; i++)
                                {
                                    Name_DIC_Struct ndic = new Name_DIC_Struct();

                                    ndic.name = (i + 1).ToString();
                                    ndic.dic.Add("smsRPOA", allInfo.listGMO[i].smsRPOA);
                                    ndic.dic.Add("smsTPOA", allInfo.listGMO[i].smsTPOA);
                                    ndic.dic.Add("smsSCTS", allInfo.listGMO[i].smsSCTS);
                                    ndic.dic.Add("smsDATA", allInfo.listGMO[i].smsDATA);
                                    ndic.dic.Add("autoSend", allInfo.listGMO[i].autoSend);
                                    ndic.dic.Add("autoFilterSMS", allInfo.listGMO[i].autoFilterSMS);
                                    ndic.dic.Add("delayTime", allInfo.listGMO[i].delayTime);
                                    ndic.dic.Add("smsCoding", allInfo.listGMO[i].smsCoding);

                                    gAppUpper.Body.n_dic.Add(ndic);
                                }
                            }                          

                            Send_Msg_2_AppCtrl_Upper(gAppUpper);
                            break;

                            #endregion                                         
                        }
                    case AppMsgType.app_get_GCInfo_Request:
                        {
                            #region 获取信息

                            //   "parentFullPathName":"设备.深圳.福田.中心广场.西北监控",
                            //   "name":"GCname"     //GSM-V2/CDMA的名称
                            //   "carry":"x"         //GSM-V2的载波标识，"0"或者"1" ; CDMA时为"-1"

                            int rtv = -1;
                            string mode = "";
                            strDevice devInfo = new strDevice();

                            string name = "";
                            string carry = "";
                            string parentFullPathName = "";

                            if (gAppUpper.Body.dic.ContainsKey("parentFullPathName"))
                            {
                                parentFullPathName = gAppUpper.Body.dic["parentFullPathName"].ToString();
                            }

                            if (gAppUpper.Body.dic.ContainsKey("name"))
                            {
                                name = gAppUpper.Body.dic["name"].ToString();
                            }

                            if (gAppUpper.Body.dic.ContainsKey("carry"))
                            {
                                carry = gAppUpper.Body.dic["carry"].ToString();
                            }

                            if (parentFullPathName == "" || name == "")
                            {
                                string errInfo = get_debug_info() + string.Format("app_get_GCInfo_Request,参数有误");
                                add_log_info(LogInfoType.EROR, errInfo, "Main", LogCategory.I);
                                Logger.Trace(LogInfoType.EROR, errInfo, "Main", LogCategory.I);

                                Fill_IMMS_Info(ref gAppUpper, AppMsgType.app_get_GCInfo_Response, -1, errInfo, true, null, null);
                                Send_Msg_2_AppCtrl_Upper(gAppUpper);
                                break;
                            }

                            if (carry != "-1" && carry != "0" && carry != "1")
                            {
                                string errInfo = get_debug_info() + string.Format("app_get_GsmInfo_Request,carry = {0},参数有误", carry);
                                add_log_info(LogInfoType.EROR, errInfo, "Main", LogCategory.I);
                                Logger.Trace(LogInfoType.EROR, errInfo, "Main", LogCategory.I);

                                Fill_IMMS_Info(ref gAppUpper, AppMsgType.app_get_GCInfo_Response, -1, errInfo, true, null, null);
                                Send_Msg_2_AppCtrl_Upper(gAppUpper);
                                break;
                            }

                            #endregion

                            #region 返回消息

                            strDomian domian = new strDomian();
                            string devFullPathName = string.Format("{0}.{1}", parentFullPathName, name);

                            if (!gDicDeviceId.ContainsKey(devFullPathName))
                            {
                                string errInfo = string.Format("{0}:对应的设备ID在gDicDeviceId中找不到", devFullPathName);
                                add_log_info(LogInfoType.EROR, errInfo, "Main", LogCategory.I);
                                Logger.Trace(LogInfoType.EROR, errInfo, "Main", LogCategory.I);

                                Fill_IMMS_Info(ref gAppUpper, AppMsgType.app_get_GCInfo_Response, -1, get_debug_info() + errInfo, true, null, null);
                                Send_Msg_2_AppCtrl_Upper(gAppUpper);
                                break;
                            }
                            else
                            {
                                devInfo = gDicDeviceId[devFullPathName];
                                rtv = gDbHelper.domain_record_get_by_nameFullPath(parentFullPathName, ref domian);
                                if (rtv == 0)
                                {
                                    int id = -1;
                                    rtv = gDbHelper.device_record_id_get_by_affdomainid_and_name(domian.id, name, ref id, ref mode);
                                }
                            }

                            str_GC_All_Para allInfo = new str_GC_All_Para();

                            //(1)
                            rtv = gDbHelper.gc_param_config_record_get_by_devid(int.Parse(carry), devInfo.id, ref allInfo.gcParamConfig);
                            if (rtv != 0)
                            {
                                string errInfo = string.Format("gc_param_config_record_get_by_devid出错:") + gDbHelper.get_rtv_str(rtv); ;
                                add_log_info(LogInfoType.EROR, errInfo, "Main", LogCategory.I);
                                Logger.Trace(LogInfoType.EROR, errInfo, "Main", LogCategory.I);

                                Fill_IMMS_Info(ref gAppUpper, AppMsgType.app_get_GCInfo_Response, -1, errInfo, true, null, null);
                                Send_Msg_2_AppCtrl_Upper(gAppUpper);
                                break;
                            }

                            //(2)
                            rtv = gDbHelper.gc_misc_record_get_by_devid(int.Parse(carry), devInfo.id, ref allInfo.gcMisc);
                            if (rtv != 0)
                            {
                                string errInfo = string.Format("gc_misc_record_get_by_devid出错") + gDbHelper.get_rtv_str(rtv);
                                add_log_info(LogInfoType.EROR, errInfo, "Main", LogCategory.I);
                                Logger.Trace(LogInfoType.EROR, errInfo, "Main", LogCategory.I);

                                Fill_IMMS_Info(ref gAppUpper, AppMsgType.app_get_GCInfo_Response, -1, errInfo, true, null, null);
                                Send_Msg_2_AppCtrl_Upper(gAppUpper);
                                break;
                            }

                            //(3)
                            rtv = gDbHelper.gc_imsi_action_record_get_by_devid(int.Parse(carry), devInfo.id, ref allInfo.listGcImsiAction);
                            if (rtv != 0)
                            {
                                string errInfo = string.Format("gc_imsi_action_record_get_by_devid出错:") + gDbHelper.get_rtv_str(rtv); ;
                                add_log_info(LogInfoType.EROR, errInfo, "Main", LogCategory.I);
                                Logger.Trace(LogInfoType.EROR, errInfo, "Main", LogCategory.I);

                                Fill_IMMS_Info(ref gAppUpper, AppMsgType.app_get_GCInfo_Response, -1, errInfo, true, null, null);
                                Send_Msg_2_AppCtrl_Upper(gAppUpper);
                                break;
                            }

                            //(4)
                            rtv = gDbHelper.gc_carrier_msg_record_get_by_devid(int.Parse(carry), devInfo.id, ref allInfo.gcCarrierMsg);
                            if (rtv != 0)
                            {
                                string errInfo = string.Format("gc_carrier_msg_record_get_by_devid出错:") + gDbHelper.get_rtv_str(rtv); ;
                                add_log_info(LogInfoType.EROR, errInfo, "Main", LogCategory.I);
                                Logger.Trace(LogInfoType.EROR, errInfo, "Main", LogCategory.I);

                                Fill_IMMS_Info(ref gAppUpper, AppMsgType.app_get_GCInfo_Response, -1, errInfo, true, null, null);
                                Send_Msg_2_AppCtrl_Upper(gAppUpper);
                                break;
                            }


                            Fill_IMMS_Info(ref gAppUpper, AppMsgType.app_get_GCInfo_Response, rtv, gDbHelper.get_rtv_str(rtv), true, null, null);

                            gAppUpper.Body.dic.Add("domainId", domian.id.ToString());
                            gAppUpper.Body.dic.Add("domainParentId", domian.parentId.ToString());
                            gAppUpper.Body.dic.Add("parentFullPathName", parentFullPathName);
                            gAppUpper.Body.dic.Add("name", name);
                            gAppUpper.Body.dic.Add("carry", carry);

                            //"n_dic":
                            //   [
                            //       "name":"CONFIG_FAP_MSG",            //4.4  GUI配置FAP的启动参数
                            //       {
                            //					"bWorkingMode":XXX		    工作模式:1 为侦码模式 ;3驻留模式.
                            //					"bC":XXX		            是否自动切换模式。保留
                            //					"wRedirectCellUarfcn":XXX	CDMA黑名单频点
                            //					"dwDateTime":XXX			当前时间	
                            //					"bPLMNId":XXX		    PLMN标志
                            //					"bTxPower":XXX			实际发射功率.设置发射功率衰减寄存器, 0输出最大功率, 每增加1, 衰减1DB
                            //					"bRxGain":XXX			接收信号衰减寄存器. 每增加1增加1DB的增益
                            //					"wPhyCellId":XXX		物理小区ID.
                            //					"wLAC":XXX			    追踪区域码。GSM：LAC;CDMA：REG_ZONE
                            //					"wUARFCN":XXX			小区频点. CDMA 制式为BSID
                            //					"dwCellId":XXX			小区ID。注意在CDMA制式没有小区ID，高位WORD 是SID ， 低位WORD 是NID
                            //       }
                            //    ]; 

                            Name_DIC_Struct ndic = new Name_DIC_Struct();
                            if (!string.IsNullOrEmpty(allInfo.gcParamConfig.bWorkingMode))
                            {
                                ndic = new Name_DIC_Struct();
                                ndic.name = "CONFIG_FAP_MSG";

                                ndic.dic.Add("bWorkingMode", allInfo.gcParamConfig.bWorkingMode);
                                ndic.dic.Add("bC", allInfo.gcParamConfig.bC);
                                ndic.dic.Add("wRedirectCellUarfcn", allInfo.gcParamConfig.wRedirectCellUarfcn);
                                ndic.dic.Add("dwDateTime", allInfo.gcParamConfig.dwDateTime);
                                ndic.dic.Add("bPLMNId", allInfo.gcParamConfig.bPLMNId);
                                ndic.dic.Add("bTxPower", allInfo.gcParamConfig.bTxPower);
                                ndic.dic.Add("bRxGain", allInfo.gcParamConfig.bRxGain);
                                ndic.dic.Add("wPhyCellId", allInfo.gcParamConfig.wPhyCellId);
                                ndic.dic.Add("wLAC", allInfo.gcParamConfig.wLAC);
                                ndic.dic.Add("wUARFCN", allInfo.gcParamConfig.wUARFCN);
                                ndic.dic.Add("dwCellId", allInfo.gcParamConfig.dwCellId);
                                gAppUpper.Body.n_dic.Add(ndic);
                            }


                            //       "name":"FAP_TRACE_MSG",     //4.7  FAP上报一些事件和状态给GUI，GUI程序需要显示给操作者看。
                            //      {
                            //					"wTraceLen":XXX	      Trace长度
                            //                  "cTrace":XXX          Trace内容
                            //       }
                            if (!string.IsNullOrEmpty(allInfo.gcMisc.wTraceLen))
                            {
                                ndic = new Name_DIC_Struct();
                                ndic.name = "FAP_TRACE_MSG";

                                ndic.dic.Add("wTraceLen", allInfo.gcMisc.wTraceLen);
                                ndic.dic.Add("cTrace", allInfo.gcMisc.cTrace);
                                gAppUpper.Body.n_dic.Add(ndic);
                            }


                            //       "name":"UE_ORM_REPORT_MSG",                 //4.9  FAP上报UE主叫信息，只用于GSM和CDMA
                            //      {
                            //					"bOrmType":XXX	    	主叫类型。1=呼叫号码, 2=短消息PDU,3=寻呼测量
                            //					"bUeId":XXX	     	    IMSI
                            //					"cRSRP":XXX	    	    接收信号强度。寻呼测量时，-128表示寻呼失败
                            //					"bUeContentLen":XXX	    Ue主叫内容长度
                            //					"bUeContent":XXX	    Ue主叫内容。最大249字节。
                            //       }
                            if (!string.IsNullOrEmpty(allInfo.gcMisc.bOrmType))
                            {
                                ndic = new Name_DIC_Struct();
                                ndic.name = "UE_ORM_REPORT_MSG";

                                ndic.dic.Add("bOrmType", allInfo.gcMisc.bOrmType);
                                ndic.dic.Add("bUeId", allInfo.gcMisc.bUeId);
                                ndic.dic.Add("cRSRP", allInfo.gcMisc.cRSRP);
                                ndic.dic.Add("bUeContentLen", allInfo.gcMisc.bUeContentLen);
                                ndic.dic.Add("bUeContent", allInfo.gcMisc.bUeContent);
                                gAppUpper.Body.n_dic.Add(ndic);
                            }


                            //       "name":"CONFIG_SMS_CONTENT_MSG",                 //4.10  FAP 配置下发短信号码和内容
                            //      {
                            //					"bSMSOriginalNumLen":XXX	    主叫号码长度
                            //					"bSMSOriginalNum":XXX	    	主叫号码
                            //					"bSMSContentLen":XXX	    	短信内容字数
                            //					"bSMSContent":XXX	            短信内容.unicode编码，每个字符占2字节
                            //       }
                            if (!string.IsNullOrEmpty(allInfo.gcMisc.bSMSOriginalNumLen))
                            {
                                ndic = new Name_DIC_Struct();
                                ndic.name = "CONFIG_SMS_CONTENT_MSG";

                                ndic.dic.Add("bSMSOriginalNumLen", allInfo.gcMisc.bSMSOriginalNumLen);
                                ndic.dic.Add("bSMSOriginalNum", allInfo.gcMisc.bSMSOriginalNum);
                                ndic.dic.Add("bSMSContentLen", allInfo.gcMisc.bSMSContentLen);
                                ndic.dic.Add("bSMSContent", allInfo.gcMisc.bSMSContent);
                                gAppUpper.Body.n_dic.Add(ndic);
                            }


                            //       "name":"CONFIG_CDMA_CARRIER_MSG",            //4.14  GUI 配置CDMA多载波参数
                            //       {
                            //					"wARFCN1":XXX	        工作频点1	
                            //					"bARFCN1Mode":XXX	    工作频点1模式。0表示扫描，1表示常开,2表示关闭。
                            //					"wARFCN1Duration":XXX	工作频点1扫描时长
                            //					"wARFCN1Period":XXX	    工作频点1扫描间隔
                            //					"wARFCN2":XXX	        工作频点2
                            //					"bARFCN2Mode":XXX	    工作频点2模式。 0表示扫描，1表示常开,2表示关闭。
                            //					"wARFCN2Duration":XXX	工作频点2扫描时长
                            //					"wARFCN2Period":XXX	    工作频点2扫描间隔
                            //					"wARFCN3":XXX	        工作频点3	
                            //					"bARFCN3Mode":XXX	    工作频点3模式。 0表示扫描，1表示常开,2表示关闭。
                            //					"wARFCN3Duration":XXX	工作频点3扫描时长	
                            //					"wARFCN3Period":XXX	    工作频点3扫描间隔
                            //					"wARFCN4":XXX	        工作频点4	
                            //					"bARFCN4Mode":XXX	    工作频点4模式。	0表示扫描，1表示常开,2表示关闭。
                            //					"wARFCN4Duration":XXX	工作频点4扫描时长
                            //					"wARFCN4Period":XXX	    工作频点4扫描间隔
                            //       }
                            if (!string.IsNullOrEmpty(allInfo.gcCarrierMsg.wARFCN1))
                            {
                                ndic = new Name_DIC_Struct();
                                ndic.name = "CONFIG_CDMA_CARRIER_MSG";

                                ndic.dic.Add("wARFCN1", allInfo.gcCarrierMsg.wARFCN1);
                                ndic.dic.Add("bARFCN1Mode", allInfo.gcCarrierMsg.bARFCN1Mode);
                                ndic.dic.Add("wARFCN1Duration", allInfo.gcCarrierMsg.wARFCN1Duration);
                                ndic.dic.Add("wARFCN1Period", allInfo.gcCarrierMsg.wARFCN1Period);

                                ndic.dic.Add("wARFCN2", allInfo.gcCarrierMsg.wARFCN2);
                                ndic.dic.Add("bARFCN2Mode", allInfo.gcCarrierMsg.bARFCN2Mode);
                                ndic.dic.Add("wARFCN2Duration", allInfo.gcCarrierMsg.wARFCN2Duration);
                                ndic.dic.Add("wARFCN2Period", allInfo.gcCarrierMsg.wARFCN2Period);

                                ndic.dic.Add("wARFCN3", allInfo.gcCarrierMsg.wARFCN3);
                                ndic.dic.Add("bARFCN3Mode", allInfo.gcCarrierMsg.bARFCN3Mode);
                                ndic.dic.Add("wARFCN3Duration", allInfo.gcCarrierMsg.wARFCN3Duration);
                                ndic.dic.Add("wARFCN3Period", allInfo.gcCarrierMsg.wARFCN3Period);

                                ndic.dic.Add("wARFCN4", allInfo.gcCarrierMsg.wARFCN4);
                                ndic.dic.Add("bARFCN4Mode", allInfo.gcCarrierMsg.bARFCN4Mode);
                                ndic.dic.Add("wARFCN4Duration", allInfo.gcCarrierMsg.wARFCN4Period);
                                ndic.dic.Add("wARFCN4Period", allInfo.gcCarrierMsg.wARFCN4Period);

                                gAppUpper.Body.n_dic.Add(ndic);
                            }


                            //       "name":"CONFIG_IMSI_MSG_V3_ID",    //4.17  大数量imsi名单，用于配置不同的目标IMSI不同的行为
                            //      {
                            //					"wTotalImsi":XXX		总的IMSI数（此版本忽略）				
                            //					"bIMSI_#n#":XXX	        IMSI数组。0~9	配置/删除/查询的IMSI
                            //					"bUeActionFlag_#n#":XXX 目标IMSI对应的动作。1 = Reject；5 = Hold ON	
                            //       }
                            if (allInfo.listGcImsiAction.Count > 0)
                            {
                                int i = 1;
                                string field = "";
                                ndic = new Name_DIC_Struct();
                                ndic.name = "CONFIG_IMSI_MSG_V3_ID";

                                ndic.dic.Add("wTotalImsi", allInfo.listGcImsiAction.Count.ToString());
                                foreach (strGcImsiAction str in allInfo.listGcImsiAction)
                                {
                                    field = string.Format("bIMSI_#{0}#", i);
                                    ndic.dic.Add(field, str.bIMSI);

                                    field = string.Format("bUeActionFlag_#{0}#", i);
                                    ndic.dic.Add(field, str.bUeActionFlag);
                                    i++;
                                }

                                gAppUpper.Body.n_dic.Add(ndic);
                            }

                            Send_Msg_2_AppCtrl_Upper(gAppUpper);
                            break;

                            #endregion                                         
                        }
                    case AppMsgType.app_set_redirection_request:
                        {
                            #region 获取信息

                            //   "parentFullPathName":"设备.深圳.福田.中心广场.西北监控",
                            //   "name":"电信FDD"
                            //   "category":"0"                       //0:white,1:black,2:other
                            //   "priority":"2"                       //2:2G,3:3G,4:4G,Others:noredirect
                            //   "GeranRedirect":"0"                  //0:disable;1:enable
                            //   "arfcn":"2G frequency"               //2G frequency
                            //   "UtranRedirect":"0"                  //0:disable;1:enable
                            //   "uarfcn":"3G frequency"              //3G frequency
                            //   "EutranRedirect":"0"                 //0:disable;1:enable
                            //   "earfcn":"4G frequency"              //4G frequency
                            //   "RejectMethod":"1"                   //1,2,0xFF,0x10-0xFE
                            //   "additionalFreq":"uarfcn1,uarfcn2"   //不超过7个freq，超过7个freq的默认丢弃

                            int rtv = -1;
                            strDevice devInfo = new strDevice();
                            string devFullPathName = "";

                            string parentFullPathName = "";
                            string name = "";
                            string category = "";

                            if (gAppUpper.Body.dic.ContainsKey("parentFullPathName"))
                            {
                                parentFullPathName = gAppUpper.Body.dic["parentFullPathName"].ToString();
                            }

                            if (gAppUpper.Body.dic.ContainsKey("name"))
                            {
                                name = gAppUpper.Body.dic["name"].ToString();
                            }

                            if (gAppUpper.Body.dic.ContainsKey("category"))
                            {
                                category = gAppUpper.Body.dic["category"].ToString();
                            }

                            if (parentFullPathName == "" || name == "" || category == "")
                            {
                                string errInfo = get_debug_info() + string.Format("app_set_redirection_request,参数有误");
                                add_log_info(LogInfoType.EROR, errInfo, "Main", LogCategory.I);
                                Logger.Trace(LogInfoType.EROR, errInfo, "Main", LogCategory.I);

                                Fill_IMMS_Info(ref gAppUpper, AppMsgType.app_set_redirection_response, -1, errInfo, true, null, null);
                                Send_Msg_2_AppCtrl_Upper(gAppUpper);
                                break;
                            }

                            if (category != "0" && category != "1" && category != "2")
                            {
                                string errInfo = get_debug_info() + string.Format("category,参数有误");
                                add_log_info(LogInfoType.EROR, errInfo, "Main", LogCategory.I);
                                Logger.Trace(LogInfoType.EROR, errInfo, "Main", LogCategory.I);

                                Fill_IMMS_Info(ref gAppUpper, AppMsgType.app_set_redirection_response, -1, errInfo, true, null, null);
                                Send_Msg_2_AppCtrl_Upper(gAppUpper);
                                break;
                            }

                            devFullPathName = string.Format("{0}.{1}", parentFullPathName, name);

                            if (!gDicDeviceId.ContainsKey(devFullPathName))
                            {
                                string info = get_debug_info() + string.Format("{0}:对应的设备ID在gDicDeviceId中找不到", devFullPathName);

                                add_log_info(LogInfoType.EROR, info, "Main", LogCategory.I);
                                Logger.Trace(LogInfoType.EROR, info, "Main", LogCategory.I);

                                //返回出错处理    
                                gAppUpper.Body.type = AppMsgType.app_set_redirection_response;
                                gAppUpper.Body.dic = new Dictionary<string, object>();
                                gAppUpper.Body.dic.Add("ReturnCode", -1);
                                gAppUpper.Body.dic.Add("ReturnStr", info);

                                Send_Msg_2_AppCtrl_Upper(gAppUpper);
                                break;
                            }
                            else
                            {
                                devInfo = gDicDeviceId[devFullPathName];
                            }

                            gRedirectionInfo = new strRedirection();
                            gRedirectionInfo.category = category;

                            if (gAppUpper.Body.dic.ContainsKey("priority"))
                            {
                                gRedirectionInfo.priority = gAppUpper.Body.dic["priority"].ToString();
                            }

                            if (gAppUpper.Body.dic.ContainsKey("GeranRedirect"))
                            {
                                gRedirectionInfo.GeranRedirect = gAppUpper.Body.dic["GeranRedirect"].ToString();

                                if (gRedirectionInfo.GeranRedirect != "0" && gRedirectionInfo.GeranRedirect != "1")
                                {
                                    string errInfo = get_debug_info() + string.Format("GeranRedirect,参数有误");
                                    add_log_info(LogInfoType.EROR, errInfo, "Main", LogCategory.I);
                                    Logger.Trace(LogInfoType.EROR, errInfo, "Main", LogCategory.I);

                                    Fill_IMMS_Info(ref gAppUpper, AppMsgType.app_set_redirection_response, -1, errInfo, true, null, null);
                                    Send_Msg_2_AppCtrl_Upper(gAppUpper);
                                    break;
                                }
                            }

                            if (gAppUpper.Body.dic.ContainsKey("arfcn"))
                            {
                                gRedirectionInfo.arfcn = gAppUpper.Body.dic["arfcn"].ToString();
                            }

                            if (gAppUpper.Body.dic.ContainsKey("UtranRedirect"))
                            {
                                gRedirectionInfo.UtranRedirect = gAppUpper.Body.dic["UtranRedirect"].ToString();

                                if (gRedirectionInfo.UtranRedirect != "0" && gRedirectionInfo.UtranRedirect != "1")
                                {
                                    string errInfo = get_debug_info() + string.Format("UtranRedirect,参数有误");
                                    add_log_info(LogInfoType.EROR, errInfo, "Main", LogCategory.I);
                                    Logger.Trace(LogInfoType.EROR, errInfo, "Main", LogCategory.I);

                                    Fill_IMMS_Info(ref gAppUpper, AppMsgType.app_set_redirection_response, -1, errInfo, true, null, null);
                                    Send_Msg_2_AppCtrl_Upper(gAppUpper);
                                    break;
                                }
                            }

                            if (gAppUpper.Body.dic.ContainsKey("uarfcn"))
                            {
                                gRedirectionInfo.uarfcn = gAppUpper.Body.dic["uarfcn"].ToString();
                            }

                            if (gAppUpper.Body.dic.ContainsKey("EutranRedirect"))
                            {
                                gRedirectionInfo.EutranRedirect = gAppUpper.Body.dic["EutranRedirect"].ToString();

                                if (gRedirectionInfo.EutranRedirect != "0" && gRedirectionInfo.EutranRedirect != "1")
                                {
                                    string errInfo = get_debug_info() + string.Format("EutranRedirect,参数有误");
                                    add_log_info(LogInfoType.EROR, errInfo, "Main", LogCategory.I);
                                    Logger.Trace(LogInfoType.EROR, errInfo, "Main", LogCategory.I);

                                    Fill_IMMS_Info(ref gAppUpper, AppMsgType.app_set_redirection_response, -1, errInfo, true, null, null);
                                    Send_Msg_2_AppCtrl_Upper(gAppUpper);
                                    break;
                                }
                            }

                            if (gAppUpper.Body.dic.ContainsKey("earfcn"))
                            {
                                gRedirectionInfo.earfcn = gAppUpper.Body.dic["earfcn"].ToString();
                            }

                            if (gAppUpper.Body.dic.ContainsKey("RejectMethod"))
                            {
                                gRedirectionInfo.RejectMethod = gAppUpper.Body.dic["RejectMethod"].ToString();
                            }

                            if (gAppUpper.Body.dic.ContainsKey("additionalFreq"))
                            {
                                gRedirectionInfo.additionalFreq = gAppUpper.Body.dic["additionalFreq"].ToString();
                            }

                            #endregion

                            #region 给AP下发命令

                            strDevice strDev = new strDevice();

                            //通过设备ID获取对应的记录（获取最新的上下线信息）
                            rtv = gDbHelper.device_record_entity_get_by_devid(devInfo.id, ref strDev);
                            if (rtv == 0)
                            {
                                if (strDev.online == "0")
                                {
                                    string errInfo = get_debug_info() + "设备下线";
                                    add_log_info(LogInfoType.EROR, errInfo, "Main", LogCategory.I);
                                    Logger.Trace(LogInfoType.EROR, errInfo, "Main", LogCategory.I);

                                    Fill_IMMS_Info(ref gAppUpper, AppMsgType.app_set_redirection_response, rtv, errInfo, true, "1", "2");
                                    Send_Msg_2_AppCtrl_Upper(gAppUpper);
                                    break;
                                }
                             
                                gAppUpper.ApInfo.SN = strDev.sn;
                                gAppUpper.ApInfo.Fullname = devFullPathName;
                                gAppUpper.ApInfo.IP = strDev.ipAddr;
                                gAppUpper.ApInfo.Port = int.Parse(strDev.port);
                                gAppUpper.ApInfo.Type = strDev.innerType;

                                gAppUpper.Body.type = ApMsgType.set_redirection_req;
                                gAppUpper.MsgType = MsgType.CONFIG.ToString();

                                gAppUpper.Body.dic = new Dictionary<string, object>();
                                gAppUpper.Body.dic.Add("category", gRedirectionInfo.category);
                                gAppUpper.Body.dic.Add("priority", gRedirectionInfo.priority);
                                gAppUpper.Body.dic.Add("GeranRedirect", gRedirectionInfo.GeranRedirect);
                                gAppUpper.Body.dic.Add("arfcn", gRedirectionInfo.arfcn);
                                gAppUpper.Body.dic.Add("UtranRedirect", gRedirectionInfo.UtranRedirect);
                                gAppUpper.Body.dic.Add("uarfcn", gRedirectionInfo.uarfcn);
                                gAppUpper.Body.dic.Add("EutranRedirect", gRedirectionInfo.EutranRedirect);
                                gAppUpper.Body.dic.Add("earfcn", gRedirectionInfo.earfcn);
                                gAppUpper.Body.dic.Add("RejectMethod", gRedirectionInfo.RejectMethod);
                                gAppUpper.Body.dic.Add("additionalFreq", gRedirectionInfo.additionalFreq);

                                //发送给ApController
                                Send_Msg_2_ApCtrl_Lower(gAppUpper);              

                                #region 启动超时计时器 

                                gTimerSetRedirection = new TaskTimer();
                                gTimerSetRedirection.Interval = DataController.TimerTimeOutInterval * 1000;

                                gTimerSetRedirection.Id = 0;
                                gTimerSetRedirection.Name = string.Format("{0}:{1}:{2}", "gTimerSetRedirection", strDev.ipAddr, strDev.port);
                                gTimerSetRedirection.MsgType = AppMsgType.app_set_redirection_response;
                                gTimerSetRedirection.TimeOutFlag = false;
                                gTimerSetRedirection.Imms = gAppUpper;

                                gTimerSetRedirection.Elapsed += new System.Timers.ElapsedEventHandler(TimerFunc);
                                gTimerSetRedirection.Start();

                                // 临时测试
                                ////保存到库中
                                //if ((int)RC.EXIST == gDbHelper.redirection_record_exist(int.Parse(gRedirectionInfo.category), devInfo.id))
                                //{
                                //    //记录存在，只是更新
                                //    gDbHelper.redirection_record_update(int.Parse(gRedirectionInfo.category), devInfo.id, gRedirectionInfo);
                                //}
                                //else
                                //{
                                //    //记录不存在，先插入，再更新
                                //    gDbHelper.redirection_record_insert(int.Parse(gRedirectionInfo.category), devInfo.id);
                                //    gDbHelper.redirection_record_update(int.Parse(gRedirectionInfo.category), devInfo.id, gRedirectionInfo);
                                //}

                                #endregion

                                break;
                            }
                            else
                            {
                                string errInfo = get_debug_info() + gDbHelper.get_rtv_str(rtv);
                                add_log_info(LogInfoType.EROR, errInfo, "Main", LogCategory.I);
                                Logger.Trace(LogInfoType.EROR, errInfo, "Main", LogCategory.I);

                                Fill_IMMS_Info(ref gAppUpper, AppMsgType.app_set_redirection_response, rtv, errInfo, true, null, null);
                                Send_Msg_2_AppCtrl_Upper(gAppUpper);
                                break;                                                       
                            }
                            
                            #endregion                                         
                        }
                    case AppMsgType.app_get_redirection_request:
                        {
                            #region 获取信息

                            //   "parentFullPathName":"设备.深圳.福田.中心广场.西北监控",
                            //   "name":"电信FDD"
                            //   "category":"0"     //0:white,1:black,2:other       

                            int rtv;
                            strDevice devInfo = new strDevice();
                            strRedirection strRd = new strRedirection();

                            string parentFullPathName = "";
                            string name = "";
                            string devFullPathName = "";
                            string category = "";

                            if (gAppUpper.Body.dic.ContainsKey("parentFullPathName"))
                            {
                                parentFullPathName = gAppUpper.Body.dic["parentFullPathName"].ToString();
                            }

                            if (gAppUpper.Body.dic.ContainsKey("name"))
                            {
                                name = gAppUpper.Body.dic["name"].ToString();
                            }

                            if (parentFullPathName == "" || name == "")
                            {
                                add_log_info(LogInfoType.EROR, "app_get_redirection_request,参数有误", "Main", LogCategory.I);
                                Logger.Trace(LogInfoType.EROR, "app_get_redirection_request,参数有误", "Main", LogCategory.I);

                                //返回出错处理
                                gAppUpper.Body.type = AppMsgType.app_get_redirection_response;
                                gAppUpper.Body.dic = new Dictionary<string, object>();
                                gAppUpper.Body.dic.Add("ReturnCode", -1);
                                gAppUpper.Body.dic.Add("ReturnStr", get_debug_info() + "app_get_redirection_request,参数有误.");

                                Send_Msg_2_AppCtrl_Upper(gAppUpper);
                                break;
                            }

                            if (gAppUpper.Body.dic.ContainsKey("category"))
                            {
                                category = gAppUpper.Body.dic["category"].ToString();
                            }

                            if (category != "0" && category != "1" && category != "2")
                            {
                                string errInfo = get_debug_info() + string.Format("category,参数有误");
                                add_log_info(LogInfoType.EROR, errInfo, "Main", LogCategory.I);
                                Logger.Trace(LogInfoType.EROR, errInfo, "Main", LogCategory.I);

                                Fill_IMMS_Info(ref gAppUpper, AppMsgType.app_get_redirection_response, -1, errInfo, true, null, null);
                                Send_Msg_2_AppCtrl_Upper(gAppUpper);
                                break;
                            }

                            #endregion

                            #region 返回消息
                       
                            strDomian domian = new strDomian();
                            devFullPathName = string.Format("{0}.{1}", parentFullPathName, name);

                            if (!gDicDeviceId.ContainsKey(devFullPathName))
                            {
                                string info = string.Format("{0}:对应的设备ID在gDicDeviceId中找不到", devFullPathName);

                                add_log_info(LogInfoType.EROR, info, "Main", LogCategory.I);
                                Logger.Trace(LogInfoType.EROR, info, "Main", LogCategory.I);

                                //返回出错处理    
                                gAppUpper.Body.type = AppMsgType.app_get_redirection_response;
                                gAppUpper.Body.dic = new Dictionary<string, object>();
                                gAppUpper.Body.dic.Add("ReturnCode", -1);
                                gAppUpper.Body.dic.Add("ReturnStr", get_debug_info() + info);

                                Send_Msg_2_AppCtrl_Upper(gAppUpper);
                                break;
                            }
                            else
                            {
                                devInfo = gDicDeviceId[devFullPathName];
                                rtv = gDbHelper.domain_record_get_by_nameFullPath(parentFullPathName, ref domian);
                                if ((int)RC.SUCCESS != rtv)
                                {
                                    string info = "domain_record_get_by_nameFullPath,出错" + gDbHelper.get_rtv_str(rtv);

                                    add_log_info(LogInfoType.EROR, info, "Main", LogCategory.I);
                                    Logger.Trace(LogInfoType.EROR, info, "Main", LogCategory.I);

                                    //返回出错处理    
                                    gAppUpper.Body.type = AppMsgType.app_get_redirection_response;
                                    gAppUpper.Body.dic = new Dictionary<string, object>();
                                    gAppUpper.Body.dic.Add("ReturnCode", rtv);
                                    gAppUpper.Body.dic.Add("ReturnStr", get_debug_info() + info);

                                    Send_Msg_2_AppCtrl_Upper(gAppUpper);
                                    break;
                                }
                            }

                            rtv = gDbHelper.redirection_record_get_by_devid(int.Parse(category), devInfo.id, ref strRd);
                            if ((int)RC.SUCCESS != rtv)
                            {
                                string info = "redirection_record_get_by_devid,出错" + gDbHelper.get_rtv_str(rtv);

                                add_log_info(LogInfoType.EROR, info, "Main", LogCategory.I);
                                Logger.Trace(LogInfoType.EROR, info, "Main", LogCategory.I);

                                //返回出错处理    
                                gAppUpper.Body.type = AppMsgType.app_get_redirection_response;
                                gAppUpper.Body.dic = new Dictionary<string, object>();
                                gAppUpper.Body.dic.Add("ReturnCode", rtv);
                                gAppUpper.Body.dic.Add("ReturnStr", get_debug_info() + info);

                                Send_Msg_2_AppCtrl_Upper(gAppUpper);
                                break;
                            }


                            gAppUpper.Body.type = AppMsgType.app_get_redirection_response;
                            gAppUpper.Body.dic = new Dictionary<string, object>();
                            gAppUpper.Body.dic.Add("ReturnCode", rtv);
                            gAppUpper.Body.dic.Add("ReturnStr", gDbHelper.get_rtv_str(rtv));

                            gAppUpper.Body.dic.Add("domainId", domian.id.ToString());
                            gAppUpper.Body.dic.Add("domainParentId", domian.parentId.ToString());
                            gAppUpper.Body.dic.Add("parentFullPathName", parentFullPathName);
                            gAppUpper.Body.dic.Add("name", name);

                            //   "category":"0"                       //0:white,1:black,2:other  
                            //   "priority":"2"                       //2:2G,3:3G,4:4G,Others:noredirect
                            //   "GeranRedirect":"0"                  //0:disable;1:enable
                            //   "arfcn":"2G frequency"               //2G frequency
                            //   "UtranRedirect":"0"                  //0:disable;1:enable
                            //   "uarfcn":"3G frequency"              //3G frequency
                            //   "EutranRedirect":"0"                 //0:disable;1:enable
                            //   "earfcn":"4G frequency"              //4G frequency
                            //   "RejectMethod":"1"                   //1,2,0xFF,0x10-0xFE
                            //   "additionalFreq":"uarfcn1,uarfcn2"   //不超过7个freq，超过7个freq的默认丢弃

                            gAppUpper.Body.dic.Add("category", category);
                            gAppUpper.Body.dic.Add("priority", strRd.priority);
                            gAppUpper.Body.dic.Add("GeranRedirect", strRd.GeranRedirect);
                            gAppUpper.Body.dic.Add("arfcn", strRd.arfcn);
                            gAppUpper.Body.dic.Add("UtranRedirect", strRd.UtranRedirect);
                            gAppUpper.Body.dic.Add("uarfcn", strRd.uarfcn);
                            gAppUpper.Body.dic.Add("EutranRedirect", strRd.EutranRedirect);
                            gAppUpper.Body.dic.Add("earfcn", strRd.earfcn);
                            gAppUpper.Body.dic.Add("RejectMethod", strRd.RejectMethod);
                            gAppUpper.Body.dic.Add("additionalFreq", strRd.additionalFreq);

                            Send_Msg_2_AppCtrl_Upper(gAppUpper);
                            break;

                            #endregion                                         
                        }
                    case AppMsgType.set_whitelist_study_request:
                        {
                            #region 获取信息

                            //    "command": int              //0:停止 1:开机执行 2:立即执行
                            //    "txpower": float(-128:0)    //Relative to maximum output power
                            //    "duration":int              //白名单自学习时长,单位秒
                            //    "clearWhiteList": int       //清除手配白名单 0：否 1：是


                            string Fullname = "";
                            strDevice devInfo = new strDevice();

                            if (string.IsNullOrEmpty(gAppUpper.ApInfo.Fullname))
                            {
                                //返回出错处理
                                string errInfo = string.Format("{0}:Fullname is NULL.", AppMsgType.set_whitelist_study_request);
                                add_log_info(LogInfoType.EROR, errInfo, "Main", LogCategory.I);
                                Logger.Trace(LogInfoType.EROR, errInfo, "Main", LogCategory.I);

                                Fill_IMMS_Info(ref gAppUpper, AppMsgType.set_whitelist_study_request, -1, errInfo, true, null, null);
                                Send_Msg_2_AppCtrl_Upper(gAppUpper);
                                break;
                            }
                            else
                            {
                                Fullname = gAppUpper.ApInfo.Fullname;
                            }

                            if (!gDicDeviceId.ContainsKey(Fullname))
                            {
                                string errInfo = get_debug_info() + string.Format("{0}:对应的设备ID在gDicDeviceId中找不到", Fullname);
                                add_log_info(LogInfoType.EROR, errInfo, "Main", LogCategory.I);
                                Logger.Trace(LogInfoType.EROR, errInfo, "Main", LogCategory.I);

                                Fill_IMMS_Info(ref gAppUpper, AppMsgType.set_whitelist_study_request, -1, errInfo, true, null, null);
                                Send_Msg_2_AppCtrl_Upper(gAppUpper);
                                break;
                            }
                            else
                            {
                                devInfo = gDicDeviceId[Fullname];
                            }

                            if (gAppUpper.Body.dic.ContainsKey("command"))
                            {
                                devInfo.command = gAppUpper.Body.dic["command"].ToString();
                            }

                            if (gAppUpper.Body.dic.ContainsKey("txpower"))
                            {
                                devInfo.txpower = gAppUpper.Body.dic["txpower"].ToString();
                            }

                            if (gAppUpper.Body.dic.ContainsKey("duration"))
                            {
                                devInfo.duration = gAppUpper.Body.dic["duration"].ToString();
                            }

                            if (gAppUpper.Body.dic.ContainsKey("clearWhiteList"))
                            {
                                devInfo.clearWhiteList = gAppUpper.Body.dic["clearWhiteList"].ToString();
                            }

                            #endregion

                            #region 保存信息

                            //保存信息
                            gDicDeviceId[Fullname] = devInfo;

                            #endregion

                            #region 发给设备

                            //发给设备
                            Send_Msg_2_ApCtrl_Lower(gAppUpper);

                            break;

                            #endregion
                        }
                    default:
                        {
                            #region 出错和透传处理

                            string info = "";
                            if (string.IsNullOrEmpty(gAppUpper.ApInfo.Fullname) &&
                                (string.IsNullOrEmpty(gAppUpper.ApInfo.IP) || gAppUpper.ApInfo.Port == 0))
                            {
                                info = string.Format("{0}->{1}", gAppUpper.Body.type, "消息中ApInfo信息错误，FullName和Ip都为空！");
                                Logger.Trace(LogInfoType.EROR, info, "Main", LogCategory.S);

                                //返回出错处理    
                                gAppUpper.Body.type = AppMsgType.general_error_result;
                                gAppUpper.Body.dic = new Dictionary<string, object>();
                                gAppUpper.Body.dic.Add("ErrStr", info);
                                gAppUpper.Body.dic.Add("RecvType", gAppUpper.Body.type);

                                //发给界面
                                Send_Msg_2_AppCtrl_Upper(gAppUpper);
                                break;
                            }

                            if (string.IsNullOrEmpty(gAppUpper.ApInfo.Type))
                            {
                                info = string.Format("{0}->{1}", gAppUpper.Body.type, "消息中ApInfo信息错误，Type为空！");
                                Logger.Trace(LogInfoType.EROR, info, "Main", LogCategory.S);

                                //返回出错处理    
                                gAppUpper.Body.type = AppMsgType.general_error_result;
                                gAppUpper.Body.dic = new Dictionary<string, object>();
                                gAppUpper.Body.dic.Add("ErrStr", info);
                                gAppUpper.Body.dic.Add("RecvType", gAppUpper.Body.type);

                                //发给界面
                                Send_Msg_2_AppCtrl_Upper(gAppUpper);
                                break;
                            }

                            info = string.Format("透传给ApCtrlLower的消息:{0}", gAppUpper.Body.type);
                            add_log_info(LogInfoType.INFO, info, "Main", LogCategory.S);
                            Logger.Trace(LogInfoType.INFO, info, "Main", LogCategory.S);

                            //发给设备
                            Send_Msg_2_ApCtrl_Lower(gAppUpper);
                            
                            rv = -1;
                            break;

                            #endregion
                        }
                }
            }
            catch (Exception ee)
            {
                add_log_info(LogInfoType.EROR, ee.Message, "Main", LogCategory.I);
                Logger.Trace(LogInfoType.EROR, ee.Message, "Main", LogCategory.I);
                return -1;
            }

            return rv;
        }

        #endregion

        #region 用于处理DbHelper的线程

        private int get_level_list(ref List<int> list, int level)
        {
            if (level <= 0)
            {
                return -1;
            }

            list = new List<int>();

            for (int i = 1; i <= level; i++)
            {
                list.Add(i * 50);
            }

            return 0;
        }

        private int get_level_list_value(List<int> list, int count)
        {
            if (count <= 0)
            {
                return 50;
            }

            if (list == null || list.Count == 0)
            {
                return 50;
            }           

            for (int i = (list.Count-1);i>=0 ; i--)
            {
                if (count >= list[i])
                {
                    return list[i];
                }
            }

            return list[0];
        }

        private int db_batch_process_delegate_fun(List<strCapture> listSC)
        {
            if(gDbHelper.MyDbConnFlag == false)
            {
                Logger.Trace(LogInfoType.EROR, "尚未连接到数据库!", "Main", LogCategory.I);
                return -1;
            }

            if (listSC == null || listSC.Count == 0)
            {
                Logger.Trace(LogInfoType.INFO,"DB listSC is empty.", "Main", LogCategory.I);
                return -1;
            }

            Stopwatch sw = new Stopwatch();
            sw.Start();

            try
            {
                int rv = gDbHelper.capture_record_insert_batch(listSC);
            }
            catch (Exception e)
            {
                Logger.Trace(LogInfoType.EROR, e.Message, "Main", LogCategory.I);
            }

            sw.Stop();
            TimeSpan ts2 = sw.Elapsed;

            //string ss = "";
            //for (int i = 0; i < listSC.Count; i++)
            //{
            //    ss += listSC[i].time + " ";
            //}

            //Logger.Trace(LogInfoType.EROR, ss + " DB批量插入：" + listSC.Count.ToString() + "  ms:" + ts2.TotalMilliseconds.ToString());

            Logger.Trace(LogInfoType.INFO, " DB批量插入：" + listSC.Count.ToString() + "  ms:" + ts2.TotalMilliseconds.ToString(), "Main", LogCategory.I);

            return 0;
        }

        /// <summary>
        /// 用于处理DbHelper的线程
        /// </summary>
        /// <param name="obj"></param>
        private void thread_for_db_helper(object obj)
        {
            #region 初始化gDbHelper

            int DbMaxIdleSeconds = 60;
            int DbBatchUpdateRecordsLevel = 6;
            int DbBatchUpdateRecords = -1;
            List<int> DbBatchUpdateRecordsLevelList = new List<int>();

            gDbHelper = new DbHelper(DataController.StrDbIpAddr,
                                     DataController.StrDbName,
                                     DataController.StrDbUserId,
                                     DataController.StrDbUserPsw,
                                     DataController.StrDbPort);

            string tmp = string.Format("{0},{1},{2},{3},{4}", DataController.StrDbIpAddr,
                                      DataController.StrDbName,
                                      DataController.StrDbUserId,
                                      DataController.StrDbUserPsw,
                                      DataController.StrDbPort);

            if (gDbHelper.MyDbConnFlag == true)
            {
                add_log_info(LogInfoType.INFO, "【" + tmp + " -> 连接数据库OK！】", "Main", LogCategory.I);
                Logger.Trace(LogInfoType.INFO, "【" + tmp + " -> 连接数据库OK！】", "Main", LogCategory.I);


                //将所有设备设置为下线           
                if (0 == gDbHelper.device_record_clear_online())
                {
                    add_log_info(LogInfoType.INFO, "device_record_clear_online -> OK！", "Main", LogCategory.I);
                }
                else
                {
                    add_log_info(LogInfoType.INFO, "device_record_clear_online -> FAILED！", "Main", LogCategory.I);
                }

                //在设备(未指派)表中清空所有的记录               
                if (0 == gDbHelper.device_unknown_record_clear())
                {
                    add_log_info(LogInfoType.INFO, "device_unknown_record_clear -> OK！", "Main", LogCategory.I);
                }
                else
                {
                    add_log_info(LogInfoType.INFO, "device_unknown_record_clear -> FAILED！", "Main", LogCategory.I);
                }

                Stopwatch sw = new Stopwatch();
                sw.Start();

                //用于快速通过设备的全名早点设备对应的ID
                if (0 == gDbHelper.domain_dictionary_info_join_get(ref gDicDeviceId))
                {
                    add_log_info(LogInfoType.INFO, "gDicDeviceId -> 获取OK！", "Main", LogCategory.I);
                    Logger.Trace(LogInfoType.INFO, "gDicDeviceId -> 获取OK！", "Main", LogCategory.I);
                }
                else
                {
                    add_log_info(LogInfoType.INFO, "gDicDeviceId -> 获取FAILED！", "Main", LogCategory.I);
                    Logger.Trace(LogInfoType.INFO, "gDicDeviceId -> 获取FAILED！", "Main", LogCategory.I);
                }

                sw.Stop();
                TimeSpan ts2 = sw.Elapsed;

                string info = string.Format("domain_dictionary_info_join_get->Stopwatch总共花费:{0}ms", Math.Ceiling(ts2.TotalMilliseconds));
                add_log_info(LogInfoType.WARN, info, "Main", LogCategory.I);
                Logger.Trace(LogInfoType.WARN, info, "Main", LogCategory.I);


                //if (0 == gDbHelper.domain_dictionary_info_get(ref gDicDeviceId))
                //{
                //    add_log_info(LogInfoType.INFO, "gDicDeviceId -> 获取OK！");
                //}
                //else
                //{
                //    add_log_info(LogInfoType.INFO, "gDicDeviceId -> 获取FAILED！");
                //}

                //sw.Stop();
                //TimeSpan ts2 = sw.Elapsed;
                //add_log_info(LogInfoType.INFO, "domain_dictionary_info_get->Stopwatch总共花费ms : " + ts2.TotalMilliseconds.ToString());
            }
            else
            {
                add_log_info(LogInfoType.EROR, "【" + tmp + " -> 连接数据库FAILED！】", "Main", LogCategory.I);
                Logger.Trace(LogInfoType.EROR, "【" + tmp + " -> 连接数据库FAILED！】", "Main", LogCategory.I);

                //MessageBox.Show("数据库连接失败，请确认配置信息!", "出错", MessageBoxButtons.OK, MessageBoxIcon.Error);

                MessageBox.Show("【" + tmp + " -> 连接数据库FAILED！】", "出错", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                Process.GetCurrentProcess().Kill();
            }


            try
            {
                DbMaxIdleSeconds = int.Parse(DataController.StrDbMaxIdleSeconds);
                DbBatchUpdateRecordsLevel = int.Parse(DataController.StrDbBatchUpdateRecordsLevel);
                get_level_list(ref DbBatchUpdateRecordsLevelList, DbBatchUpdateRecordsLevel);

            }
            catch (Exception ee)
            {
                add_log_info(LogInfoType.EROR, ee.Message + "StrDbMaxIdleSeconds解析出错", "Main", LogCategory.I);
                DbMaxIdleSeconds = 60;
                DbBatchUpdateRecordsLevel = 6;
                get_level_list(ref DbBatchUpdateRecordsLevelList, DbBatchUpdateRecordsLevel);
            }

            #endregion

            bool noMsg = false;            

            DateTime startTime = System.DateTime.Now;
            DateTime endTime = System.DateTime.Now;
            TimeSpan ts = endTime.Subtract(startTime);

            long pingPangIndex = 0;
            List<strCapture> batchData0 = new List<strCapture>();
            List<strCapture> batchData1 = new List<strCapture>();
            List<strCapture> batchData2 = new List<strCapture>();

            /*
             * 用于修复mysql连接的空闲时间超过8小时后，MySQL自动断开该连接的问题
             * wait_timeout = 8*3600
             * 即每隔fix_for_wait_timeout的时间（秒数）就访问一下数据库
             */
            int fix_for_wait_timeout = 60*60;

            DateTime startTimeConn = System.DateTime.Now;
            DateTime endTimeConn = System.DateTime.Now;
            TimeSpan tsConn = endTime.Subtract(startTimeConn);

            try
            {
                while (true)
                {
                    if (noMsg)
                    {
                        Thread.Sleep(100);
                    }
                    else
                    {
                        //Thread.Sleep(1);
                    }

                    #region 防止自动断开该连接的问题

                    endTimeConn = System.DateTime.Now;
                    tsConn = endTime.Subtract(startTimeConn);

                    if (tsConn.TotalSeconds >= fix_for_wait_timeout)
                    {
                        List<string> listAllTbl = gDbHelper.Get_All_TableName();

                        Logger.Trace(LogInfoType.INFO, "fix_for_wait_timeout:" + listAllTbl.Count.ToString(), "Main", LogCategory.I);
                        startTimeConn = System.DateTime.Now;
                    }

                    #endregion

                    #region 上传处理

                    lock (mutex_DbHelper)
                    {
                        //动态计算要更新的数量
                        DbBatchUpdateRecords = get_level_list_value(DbBatchUpdateRecordsLevelList, gCaptureInfoDb.Count);

                        if (gCaptureInfoDb.Count < DbBatchUpdateRecords)
                        {
                            #region 数量不足

                            endTime = System.DateTime.Now;
                            ts = endTime.Subtract(startTime);

                            if (ts.TotalSeconds < DbMaxIdleSeconds)
                            {
                                noMsg = true;
                                continue;
                            }
                            else
                            {
                                //清空数据
                                batchData0 = new List<strCapture>();

                                //拷贝数据
                                while (gCaptureInfoDb.Count > 0)
                                {
                                    batchData0.Add(gCaptureInfoDb.Dequeue());
                                }

                                //处理数据
                                BeginInvoke(new db_batch_process_delegate(db_batch_process_delegate_fun), new object[] { batchData0 });

                                //复位计时
                                startTime = System.DateTime.Now;
                            }

                            #endregion
                        }
                        else
                        {
                            #region 数量充足

                            #region 清空数据

                            if (0 == (pingPangIndex % 3))
                            {
                                batchData0 = new List<strCapture>();
                            }
                            else if (1 == (pingPangIndex % 3))
                            {
                                batchData1 = new List<strCapture>();
                            }
                            else
                            {
                                batchData2 = new List<strCapture>();
                            }

                            #endregion

                            #region 从队列中获取设定的批量数据

                            for (int i = 0; i < DbBatchUpdateRecords; i++)
                            {
                                if (0 == (pingPangIndex % 3))
                                {
                                    batchData0.Add(gCaptureInfoDb.Dequeue());
                                }
                                else if (1 == (pingPangIndex % 3))
                                {
                                    batchData1.Add(gCaptureInfoDb.Dequeue());
                                }
                                else
                                {
                                    batchData2.Add(gCaptureInfoDb.Dequeue());
                                }
                            }

                            #endregion

                            #region 处理数据

                            //复位起始时间
                            startTime = System.DateTime.Now;

                            //处理批量的数据
                            if (0 == (pingPangIndex % 3))
                            {
                                BeginInvoke(new db_batch_process_delegate(db_batch_process_delegate_fun), new object[] { batchData0 });
                            }
                            else if (1 == (pingPangIndex % 3))
                            {
                                BeginInvoke(new db_batch_process_delegate(db_batch_process_delegate_fun), new object[] { batchData1 });
                            }
                            else
                            {
                                BeginInvoke(new db_batch_process_delegate(db_batch_process_delegate_fun), new object[] { batchData2 });
                            }

                            #endregion

                            #endregion
                        }
                    }

                    pingPangIndex++;
                    noMsg = false;

                    #endregion
                }
            }
            catch (Exception ee)
            {
                add_log_info(LogInfoType.EROR, ee.Message, "Main", LogCategory.I);
                Logger.Trace(LogInfoType.EROR, ee.Message, "Main", LogCategory.I);
            }
        }

        #endregion

        #region 用于处理FtpHelper的线程

        private int ftp_batch_process_delegate_fun(List<strCapture> listSC)
        {
            if (gFtpHelperImsi.Connected == false)
            {
                Logger.Trace(LogInfoType.EROR, "尚未连接到FTP Server!", "Main", LogCategory.I);
                return -1;
            }

            if (listSC == null || listSC.Count == 0)
            {
                add_log_info(LogInfoType.INFO, "FTP listSC is empty", "Main", LogCategory.I);
                Logger.Trace(LogInfoType.INFO, "FTP listSC is empty", "Main", LogCategory.I);
                return -1;
            }

            //string fullPathFile = string.Format("F:\\1234567-{0}.txt", aaa++);

            string fullPathFile = DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss");
            fullPathFile += ".csv";

            Stopwatch sw = new Stopwatch();
            sw.Start();           

            try
            {
                byte[] data = null;
                if (0 == generate_ftp_byte(ref data, listSC))
                {
                    gFtpHelperImsi.Put(fullPathFile, data);
                }
            }
            catch (Exception e)
            {
                Logger.Trace(LogInfoType.EROR, e.Message, "Main", LogCategory.I);
                if (e.Message.Contains("中止"))
                {
                    gFtpHelperImsi.Connected = false;
                    gFtpHelperImsi.Connect();

                    byte[] data = null;
                    if (0 == generate_ftp_byte(ref data, listSC))
                    {
                        gFtpHelperImsi.Put(fullPathFile, data);
                    }
                }
            }

            sw.Stop();
            TimeSpan ts2 = sw.Elapsed;

            //add_log_info(LogInfoType.INFO, "FTP批量插入：" + listSC.Count.ToString() + "  ms:" + ts2.TotalMilliseconds.ToString());
            Logger.Trace(LogInfoType.INFO, "FTP批量插入：" + listSC.Count.ToString() + "  ms:" + ts2.TotalMilliseconds.ToString(), "Main", LogCategory.I);

            return 0;
        }

        private int generate_ftp_file(string fileFullPath, List<strCapture> capList)
        {
            if (string.IsNullOrEmpty(fileFullPath))
            {
                add_log_info(LogInfoType.EROR, "fileFullPath is NULL", "Main", LogCategory.I);
                Logger.Trace(LogInfoType.EROR, "fileFullPath is NULL", "Main", LogCategory.I);
                return -1;
            }

            if (File.Exists(fileFullPath))
            {
                File.Delete(fileFullPath);
            }

            if (capList == null || capList.Count == 0)
            {
                add_log_info(LogInfoType.EROR, "capList is NULL", "Main", LogCategory.I);
                Logger.Trace(LogInfoType.EROR, "capList is NULL", "Main", LogCategory.I);
                return -1;
            }


            //public string imsi;          //IMSI号
            //public string imei;          //IMEI号
            //public bwType bwFlag;        //名单标识
            //public string isdn;          //手机号码号段
            //public string bsPwr;         //手机上报的基站功率
            //public string tmsi;          //手机TMSI号
            //public string time;          //感知时间
            //public string sn;            //SN号

            try
            {
                byte[] data = null;
                FileStream fs = new FileStream(fileFullPath, FileMode.Create);

                string title = string.Format("{0,4}{1,18}{2,18}{3,8}{4,18}{5,4}{6,12}{6,20}{6,20}\n",
                 "序号", "IMSI", "IMEI", "用户类型", "ISDN", "功率", "TMSI","时间","SN");

                data = System.Text.Encoding.Default.GetBytes(title);
                fs.Write(data, 0, data.Length);

                title = string.Format("---------------------------------------------------------------------\n" );
                data = System.Text.Encoding.Default.GetBytes(title);
                fs.Write(data, 0, data.Length);


                int index = 1;                
                foreach (strCapture cap in capList)
                {
                    string sqlSub = "";
               
                    #region 构造字符串

                    sqlSub += string.Format("{0:D4} -- ", index++);

                    //(1)
                    if (!string.IsNullOrEmpty(cap.imsi))
                    {
                        if (cap.imsi.Length > 15)
                        {
                            Logger.Trace(LogInfoType.EROR, "Length error.", "Main", LogCategory.I);
                            continue;
                        }
                        else
                        {
                            sqlSub += string.Format("{0,18}", cap.imsi);
                        }
                    }


                    //(2)
                    if (!string.IsNullOrEmpty(cap.imei))
                    {
                        if (cap.imei.Length > 15)
                        {
                            Logger.Trace(LogInfoType.EROR, "Length error.", "Main", LogCategory.I);
                            continue;
                        }
                        else
                        {
                            sqlSub += string.Format("{0} -- ", cap.imei);
                        }
                    }

                    //(3)
                    if (!string.IsNullOrEmpty(cap.isdn))
                    {
                        if (cap.isdn.Length > 10)
                        {
                            Logger.Trace(LogInfoType.EROR, "Length error.", "Main", LogCategory.I);
                            continue;
                        }
                        else
                        {
                            sqlSub += string.Format("{0} -- ", cap.isdn);
                        }
                    }

                    //(4)
                    if (!string.IsNullOrEmpty(cap.bsPwr))
                    {
                        if (cap.bsPwr.Length > 4)
                        {
                            Logger.Trace(LogInfoType.EROR, "Length error.", "Main", LogCategory.I);
                            continue;
                        }
                        else
                        {
                            sqlSub += string.Format("{0} -- ", cap.bsPwr);
                        }
                    }

                    //(5)
                    if (!string.IsNullOrEmpty(cap.tmsi))
                    {
                        if (cap.tmsi.Length > 10)
                        {
                            Logger.Trace(LogInfoType.EROR, "Length error.", "Main", LogCategory.I);
                            continue;
                        }
                        else
                        {
                            sqlSub += string.Format("{0} -- ", cap.tmsi);
                        }
                    }

                    //(6)
                    if (!string.IsNullOrEmpty(cap.time))
                    {
                        if (cap.time.Length > 19)
                        {
                            Logger.Trace(LogInfoType.EROR, "Length error.", "Main", LogCategory.I);
                            continue;
                        }
                        else
                        {
                            sqlSub += string.Format("{0} -- ", cap.time);
                        }
                    }

                    //(7)
                    if (!string.IsNullOrEmpty(cap.affDeviceId))
                    {
                        if (cap.affDeviceId.Length > 10)
                        {
                            Logger.Trace(LogInfoType.EROR, "Length error.", "Main", LogCategory.I);
                            continue;
                        }
                        else
                        {
                            sqlSub += string.Format("{0} -- ", cap.affDeviceId);
                        }
                    }

                    if (sqlSub != "")
                    {
                        //去掉最后4个字符
                        sqlSub = sqlSub.Remove(sqlSub.Length - 4, 4);
                    }

                    #endregion

                    data = System.Text.Encoding.Default.GetBytes(sqlSub + "\n");
                    fs.Write(data, 0, data.Length);
                }

                //清空缓冲区、关闭流
                fs.Flush();
                fs.Close();
            }
            catch (Exception e)
            {
                add_log_info(LogInfoType.EROR, e.Message, "Main", LogCategory.I);
                Logger.Trace(LogInfoType.EROR, e.Message, "Main", LogCategory.I);
                return -1;
            }

            return 0;
        }       

        private int generate_ftp_byte(ref byte[] outData, List<strCapture> capList)
        {
            if (capList == null || capList.Count == 0)
            {
                add_log_info(LogInfoType.INFO, "capList is empty", "Main", LogCategory.I);
                Logger.Trace(LogInfoType.INFO, "capList is empty", "Main", LogCategory.I);
                return -1;
            }

            try
            {
                int index = 1;

                StringBuilder bigStr = new StringBuilder(1024 * 10);

                string title = string.Format("序号,IMSI,时间,用户类型,TMSI,设备名称,信号,运营商,号码归属地,IMEI,SN\n");
                bigStr.Append(title); 
                      
                foreach (strCapture cap in capList)
                {
                    string sqlSub = "";

                    #region 构造字符串

                    //public string imsi;          //IMSI号
                    //public string imei;          //IMEI号
                    //public bwType bwFlag;        //名单标识
                    //public string isdn;          //手机号码号段
                    //public string bsPwr;         //手机上报的基站功率
                    //public string tmsi;          //手机TMSI号
                    //public string time;          //感知时间
                    //public string sn;            //SN号

                    //(1) 序号
                    sqlSub += string.Format("{0},", index++);

                    //(2) IMSI
                    if (!string.IsNullOrEmpty(cap.imsi))
                    {
                        if (cap.imsi.Length > 15)
                        {
                            Logger.Trace(LogInfoType.EROR, "Length error.", "Main", LogCategory.I);
                            continue;
                        }
                        else
                        {
                            sqlSub += string.Format("'{0},", cap.imsi);
                        }
                    }

                    //(3) 时间
                    if (!string.IsNullOrEmpty(cap.time))
                    {
                        if (cap.time.Length > 19)
                        {
                            Logger.Trace(LogInfoType.EROR, "Length error.", "Main", LogCategory.I);
                            continue;
                        }
                        else
                        {
                            sqlSub += string.Format("{0},", cap.time);
                        }
                    }

                    //(4) 用户类型
                    if (!string.IsNullOrEmpty(cap.bwFlag.ToString()))
                    {
                        if (cap.bwFlag == bwType.BWTYPE_BLACK)
                        {
                            sqlSub += string.Format("{0},", "黑名单");
                        }
                        else if (cap.bwFlag == bwType.BWTYPE_WHITE)
                        {
                            sqlSub += string.Format("{0},", "白名单");
                        }
                        else if (cap.bwFlag == bwType.BWTYPE_OTHER)
                        {
                            sqlSub += string.Format("{0},", "其它");
                        }
                        else
                        {
                            sqlSub += string.Format("{0},", "未知");
                        }
                    }


                    //(5) TMSI
                    if (!string.IsNullOrEmpty(cap.tmsi))
                    {
                        if (cap.tmsi.Length > 10)
                        {
                            Logger.Trace(LogInfoType.EROR, "Length error.", "Main", LogCategory.I);
                            continue;
                        }
                        else
                        {
                            sqlSub += string.Format("{0},", cap.tmsi);
                        }
                    }
                    else
                    {
                        sqlSub += string.Format(",");
                    }

                    //(6) 设备名称
                    if (!string.IsNullOrEmpty(cap.name))
                    {
                        if (cap.name.Length > 64)
                        {
                            Logger.Trace(LogInfoType.EROR, "Length error.", "Main", LogCategory.I);
                            continue;
                        }
                        else
                        {
                            sqlSub += string.Format("{0},", cap.name);
                        }
                    }
                    else
                    {
                        sqlSub += string.Format(",");
                    }                   

                    //(7) 信号
                    if (!string.IsNullOrEmpty(cap.bsPwr))
                    {
                        if (cap.bsPwr.Length > 4)
                        {
                            Logger.Trace(LogInfoType.EROR, "Length error.", "Main", LogCategory.I);
                            continue;
                        }
                        else
                        {
                            sqlSub += string.Format("{0},", cap.bsPwr);
                        }
                    }
                    else
                    {
                        sqlSub += string.Format(",");
                    }

                    strImsiParse sip = new strImsiParse();
                    if (0 == Location_And_Operator_Get(cap.imsi, ref sip))
                    {
                        // (8) 运营商
                        sqlSub += string.Format("{0},", sip.operators);

                        // (9) 号码归属地
                        sqlSub += string.Format("{0},", sip.country + sip.location);
                    }
                    else
                    {
                        // (8) 运营商
                        sqlSub += string.Format("{0},", "Null");

                        // (9) 号码归属地
                        sqlSub += string.Format("{0},", "Null");
                    }

                    //(10) IMEI
                    if (!string.IsNullOrEmpty(cap.imei))
                    {
                        if (cap.imei.Length > 15)
                        {
                            Logger.Trace(LogInfoType.EROR, "Length error.", "Main", LogCategory.I);
                            continue;
                        }
                        else
                        {
                            sqlSub += string.Format("{0},", cap.imei);
                        }
                    }
                    else
                    {
                        sqlSub += string.Format(",");
                    }

                    //(11) SN
                    if (!string.IsNullOrEmpty(cap.sn))
                    {
                        if (cap.affDeviceId.Length > 16)
                        {
                            Logger.Trace(LogInfoType.EROR, "Length error.", "Main", LogCategory.I);
                            continue;
                        }
                        else
                        {
                            sqlSub += string.Format("{0},", cap.sn);
                        }
                    }

                    if (sqlSub != "")
                    {
                        //去掉最后4个字符
                        sqlSub = sqlSub.Remove(sqlSub.Length - 4, 4);
                    }

                    #endregion

                    bigStr.Append(sqlSub + "\n");
                }

                outData = System.Text.Encoding.Default.GetBytes(bigStr.ToString());
            }
            catch (Exception e)
            {
                add_log_info(LogInfoType.EROR, e.Message, "Main", LogCategory.I);
                Logger.Trace(LogInfoType.EROR, e.Message, "Main", LogCategory.I);
                return -1;
            }

            return 0;
        }

        private int generate_ftp_byte(ref byte[] outData, List<string> strList)
        {
            if (strList == null)
            {
                add_log_info(LogInfoType.INFO, "strList is empty", "Main", LogCategory.I);
                Logger.Trace(LogInfoType.INFO, "strList is empty", "Main", LogCategory.I);
                return -1;
            }

            try
            {            
                string bigStr = "";                
                foreach (string str in strList)
                {
                    bigStr += string.Format("{0}\n",str);               
                }

                outData = System.Text.Encoding.Default.GetBytes(bigStr);
            }
            catch (Exception e)
            {
                add_log_info(LogInfoType.EROR, e.Message, "Main", LogCategory.I);
                Logger.Trace(LogInfoType.EROR, e.Message, "Main", LogCategory.I);
                return -1;
            }

            return 0;
        }

        private int generate_ftp_memory(ref MemoryStream ms, List<strCapture> capList)
        {
            if (ms == null)
            {
                add_log_info(LogInfoType.EROR, "ms is NULL", "Main", LogCategory.I);
                Logger.Trace(LogInfoType.EROR, "ms is NULL", "Main", LogCategory.I);
                return -1;
            }

            if (capList == null || capList.Count == 0)
            {
                add_log_info(LogInfoType.EROR, "capList is NULL", "Main", LogCategory.I);
                Logger.Trace(LogInfoType.EROR, "capList is NULL", "Main", LogCategory.I);
                return -1;
            }

            try
            {
                int index = 1;
                int offset = 0;
                ms = new MemoryStream();
                string str = "";

                foreach (strCapture cap in capList)
                {
                    string sqlSub = "";

                    #region 构造字符串

                    sqlSub += string.Format("{0:D4} -- ", index++);

                    //(1)
                    if (!string.IsNullOrEmpty(cap.imsi))
                    {
                        if (cap.imsi.Length > 15)
                        {
                            Logger.Trace(LogInfoType.EROR, "Length error.", "Main", LogCategory.I);
                            continue;
                        }
                        else
                        {
                            sqlSub += string.Format("{0} -- ", cap.imsi);
                        }
                    }


                    //(2)
                    if (!string.IsNullOrEmpty(cap.imei))
                    {
                        if (cap.imei.Length > 15)
                        {
                            Logger.Trace(LogInfoType.EROR, "Length error.", "Main", LogCategory.I);
                            continue;
                        }
                        else
                        {
                            sqlSub += string.Format("{0} -- ", cap.imei);
                        }
                    }

                    //(3)
                    if (!string.IsNullOrEmpty(cap.isdn))
                    {
                        if (cap.isdn.Length > 10)
                        {
                            Logger.Trace(LogInfoType.EROR, "Length error.", "Main", LogCategory.I);
                            continue;
                        }
                        else
                        {
                            sqlSub += string.Format("{0} -- ", cap.isdn);
                        }
                    }

                    //(4)
                    if (!string.IsNullOrEmpty(cap.bsPwr))
                    {
                        if (cap.bsPwr.Length > 4)
                        {
                            Logger.Trace(LogInfoType.EROR, "Length error.", "Main", LogCategory.I);
                            continue;
                        }
                        else
                        {
                            sqlSub += string.Format("{0} -- ", cap.bsPwr);
                        }
                    }

                    //(5)
                    if (!string.IsNullOrEmpty(cap.tmsi))
                    {
                        if (cap.tmsi.Length > 10)
                        {
                            Logger.Trace(LogInfoType.EROR, "Length error.", "Main", LogCategory.I);
                            continue;
                        }
                        else
                        {
                            sqlSub += string.Format("{0} -- ", cap.tmsi);
                        }
                    }

                    //(6)
                    if (!string.IsNullOrEmpty(cap.time))
                    {
                        if (cap.time.Length > 19)
                        {
                            Logger.Trace(LogInfoType.EROR, "Length error.", "Main", LogCategory.I);
                            continue;
                        }
                        else
                        {
                            sqlSub += string.Format("{0} -- ", cap.time);
                        }
                    }

                    //(7)
                    if (!string.IsNullOrEmpty(cap.affDeviceId))
                    {
                        if (cap.affDeviceId.Length > 10)
                        {
                            Logger.Trace(LogInfoType.EROR, "Length error.", "Main", LogCategory.I);
                            continue;
                        }
                        else
                        {
                            sqlSub += string.Format("{0} -- ", cap.affDeviceId);
                        }
                    }

                    if (sqlSub != "")
                    {
                        //去掉最后4个字符
                        sqlSub = sqlSub.Remove(sqlSub.Length - 4, 4);
                    }

                    str += sqlSub + "\n";
                    #endregion

                    //data = new byte[1000];
                    //data = System.Text.Encoding.Default.GetBytes(sqlSub + "\n");                               
                }

                byte[] data = System.Text.Encoding.Default.GetBytes(str);
                ms.Write(data, offset, data.Length);
            }
            catch (Exception e)
            {
                add_log_info(LogInfoType.EROR, e.Message, "Main", LogCategory.I);
                Logger.Trace(LogInfoType.EROR, e.Message, "Main", LogCategory.I);
                return -1;
            }

            return 0;
        }

        private int generate_ftp_byte_csv(ref byte[] outData, DataTable dt)
        {
            if (dt == null || dt.Rows.Count == 0)
            {
                add_log_info(LogInfoType.EROR, "generate_ftp_byte_csv,dt is NULL", "Main", LogCategory.I);
                Logger.Trace(LogInfoType.EROR, "generate_ftp_byte_csv,dt is NULL", "Main", LogCategory.I);
                return -1;
            }

            string str = "";
            StringBuilder bigStr = new StringBuilder(1024 * 10);
            
            string title = string.Format("序号,IMSI,时间,用户类型,TMSI,设备名称,信号,运营商,号码归属地,IMEI,SN\n");
            bigStr.Append(title);

            for (int inx = 0; inx < dt.Rows.Count; inx++)
            {
                DataRow dr = dt.Rows[inx];

                str = "";

                //(1)序号
                str += string.Format("{0},",inx + 1);

                //(2)IMSI
                if (string.IsNullOrEmpty(dr["imsi"].ToString()))
                {
                    str += string.Format(",");
                }
                else
                {
                    str += string.Format("'{0},", dr["imsi"].ToString());
                }

                //(3) 时间
                if (string.IsNullOrEmpty(dr["time"].ToString()))
                {
                    str += string.Format(",");
                }
                else
                {
                    str += string.Format("{0},", dr["time"].ToString());
                }


                //(4) 用户类型
                if (string.IsNullOrEmpty(dr["bwFlag"].ToString()))
                {
                    str += string.Format(",");
                }
                else
                {
                    if (dr["bwFlag"].ToString() == "other")
                    {
                        str += string.Format("其他,");
                    }
                    else if (dr["bwFlag"].ToString() == "white")
                    {
                        str += string.Format("白名单,");
                    }
                    else if (dr["bwFlag"].ToString() == "black")
                    {
                        str += string.Format("黑名单,");
                    }
                    else
                    {
                        str += string.Format("未知,");
                    }
                }


                // (5) TMSI 2018-07-24
                if (string.IsNullOrEmpty(dr["tmsi"].ToString()))
                {
                    str += string.Format(",");
                }
                else
                {
                    str += string.Format("{0},", dr["tmsi"].ToString());
                }

                //(6) 设备名称
                if (string.IsNullOrEmpty(dr["name"].ToString()))
                {
                    str += string.Format(",");
                }
                else
                {
                    str += string.Format("{0},", dr["name"].ToString());
                }

                // (7)信号 2018-07-25
                if (string.IsNullOrEmpty(dr["bsPwr"].ToString()))
                {
                    str += string.Format(",");
                }
                else
                {
                    str += string.Format("{0},", dr["bsPwr"].ToString());
                }

                strImsiParse sip = new strImsiParse();
                if (0 == Location_And_Operator_Get(dr["imsi"].ToString(), ref sip))
                {
                    // (8) 运营商
                    str += string.Format("{0},", sip.operators);

                    // (9) 号码归属地
                    str += string.Format("{0},", sip.country + sip.location);
                }
                else
                {
                    // (8) 运营商
                    str += string.Format("{0},", "Null");

                    // (9) 号码归属地
                    str += string.Format("{0},", "Null");
                }               

                //(10) IMEI
                if (string.IsNullOrEmpty(dr["imei"].ToString()))
                {
                    str += string.Format(",");
                }
                else
                {
                    str += string.Format("'{0},", dr["imei"].ToString());
                }

                //(11) SN
                if (string.IsNullOrEmpty(dr["sn"].ToString()))
                {
                    str += string.Format("\n");
                }
                else
                {
                    str += string.Format("{0}\n", dr["sn"].ToString());
                }

                bigStr.Append(str);
            }

            outData = System.Text.Encoding.Default.GetBytes(bigStr.ToString());
            return 0;
        }

        /// <summary>
        /// 用于处理FtpHelper的线程
        /// </summary>
        /// <param name="obj"></param>
        private void thread_for_ftp_helper(object obj)
        {       
            #region 初始化FtpHelper

            int FtpMaxIdleSeconds = 60;
            int FtpBatchUpdateRecordsLevel = 6;
            int FtpBatchUpdateRecords = -1;
            List<int> FtpBatchUpdateRecordsLevelList = new List<int>();

            /// <summary>
            /// 和FTP Server保持KeepAlive的频率(秒数)
            /// </summary>
            int CHECK_FTP_CONN_STAT = DataController.CheckFtpConnStatTime;

            try
            {
                FtpMaxIdleSeconds = int.Parse(DataController.StrFtpMaxIdleSeconds);
                FtpBatchUpdateRecordsLevel = int.Parse(DataController.StrFtpBatchUpdateRecordsLevel);
                get_level_list(ref FtpBatchUpdateRecordsLevelList, FtpBatchUpdateRecordsLevel);
            }
            catch (Exception ee)
            {
                add_log_info(LogInfoType.EROR, ee.Message + "StrDbMaxIdleSeconds解析出错", "Main", LogCategory.I);

                FtpMaxIdleSeconds = 60;
                FtpBatchUpdateRecordsLevel = 6;
                get_level_list(ref FtpBatchUpdateRecordsLevelList, FtpBatchUpdateRecordsLevel);
            }

            try
            {
                gFtpHelperImsi = new FtpHelper(DataController.StrFtpIpAddr,
                                           DataController.StrFtpImsiDir,
                                           DataController.StrFtpUserId,
                                           DataController.StrFtpUserPsw,
                                           int.Parse(DataController.StrFtpPort));

                if (gFtpHelperImsi.Connected == false)
                {
                    gFtpHelperImsi.Connect();
                }
               
                if (gFtpHelperImsi.Connected == true)
                {
                    string info = string.Format("【{0} {1} {2} {3} {4} -> 连接FTP服务器OK！】",
                                          DataController.StrFtpIpAddr,
                                          DataController.StrFtpImsiDir,
                                          DataController.StrFtpUserId,
                                          DataController.StrFtpUserPsw,
                                          int.Parse(DataController.StrFtpPort));

                    add_log_info(LogInfoType.INFO, info, "Main", LogCategory.I);
                    Logger.Trace(LogInfoType.INFO, info, "Main", LogCategory.I);
                }
                else
                {
                    string info = string.Format("【{0} {1} {2} {3} {4} -> 连接FTP服务器FAILED！】",
                                         DataController.StrFtpIpAddr,
                                         DataController.StrFtpImsiDir,
                                         DataController.StrFtpUserId,
                                         DataController.StrFtpUserPsw,
                                         int.Parse(DataController.StrFtpPort));

                    add_log_info(LogInfoType.INFO, info, "Main", LogCategory.I);
                    Logger.Trace(LogInfoType.INFO, info, "Main", LogCategory.I);

                    MessageBox.Show(info, "出错", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                    Process.GetCurrentProcess().Kill();
                }
            }
            catch (Exception ee)
            {
                add_log_info(LogInfoType.EROR, ee.Message, "Main", LogCategory.I);
                Logger.Trace(LogInfoType.EROR, ee.Message, "Main", LogCategory.I);
            }


            try
            {
                gFtpHelperFile = new FtpHelper(DataController.StrFtpIpAddr,
                                           DataController.StrFtpUpdateDir,
                                           DataController.StrFtpUserId,
                                           DataController.StrFtpUserPsw,
                                           int.Parse(DataController.StrFtpPort));

                if (gFtpHelperFile.Connected == false)
                {
                    gFtpHelperFile.Connect();
                }

                if (gFtpHelperFile.Connected == true)
                {
                    string info = string.Format("【{0} {1} {2} {3} {4} -> 连接FTP服务器OK！】",
                                          DataController.StrFtpIpAddr,
                                          DataController.StrFtpUpdateDir,
                                          DataController.StrFtpUserId,
                                          DataController.StrFtpUserPsw,
                                          int.Parse(DataController.StrFtpPort));

                    add_log_info(LogInfoType.INFO, info, "Main", LogCategory.I);
                    Logger.Trace(LogInfoType.INFO, info, "Main", LogCategory.I);
                }
                else
                {
                    string info = string.Format("【{0} {1} {2} {3} {4} -> 连接FTP服务器FAILED！】",
                                         DataController.StrFtpIpAddr,
                                         DataController.StrFtpUpdateDir,
                                         DataController.StrFtpUserId,
                                         DataController.StrFtpUserPsw,
                                         int.Parse(DataController.StrFtpPort));

                    add_log_info(LogInfoType.INFO, info, "Main", LogCategory.I);
                    Logger.Trace(LogInfoType.INFO, info, "Main", LogCategory.I);

                    MessageBox.Show(info, "出错", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                    Process.GetCurrentProcess().Kill();
                }
            }
            catch (Exception ee)
            {
                add_log_info(LogInfoType.EROR, ee.Message, "Main", LogCategory.I);
                Logger.Trace(LogInfoType.EROR, ee.Message, "Main", LogCategory.I);
            }

            #endregion

            #region 变量定义

            bool noMsg = false;
            long pingPangIndex = 1;

            DateTime startTime = System.DateTime.Now;
            DateTime endTime = System.DateTime.Now;
            TimeSpan ts = endTime.Subtract(startTime);

            DateTime startTimeConn = System.DateTime.Now;
            DateTime endTimeConn = System.DateTime.Now;
            TimeSpan tsConn = endTime.Subtract(startTimeConn);            

            List<strCapture> batchData0 = new List<strCapture>();
            List<strCapture> batchData1 = new List<strCapture>();
            List<strCapture> batchData2 = new List<strCapture>();

            int keepAliveMode = 0;

            #endregion

            try
            {
                while (true)
                {
                    #region Sleep处理

                    if (noMsg)
                    {
                        Thread.Sleep(100);
                    }
                    else
                    {
                        //Thread.Sleep(1);
                    }

                    #endregion

                    #region keepAlive处理

                    endTimeConn = System.DateTime.Now;
                    tsConn = endTime.Subtract(startTimeConn);

                    if (tsConn.TotalSeconds >= CHECK_FTP_CONN_STAT)
                    {
                        string str = string.Format("[{0}]--keepAlive\r\n", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff"));
                        byte[] data = System.Text.Encoding.Default.GetBytes(str);

                        #region gFtpHelperImsi

                        try
                        {                                                       
                            if (keepAliveMode == 0)
                            {
                                if (gFtpHelperImsi.Connected == false)
                                {
                                    gFtpHelperImsi.Connect();
                                }
                                else
                                {
                                    if (0 == gFtpHelperImsi.Put("keepAlive", data))
                                    {
                                        add_log_info(LogInfoType.INFO, "gFtpHelperImsi -> keepAlive OK.", "Main", LogCategory.I);
                                        Logger.Trace(LogInfoType.INFO, "gFtpHelperImsi -> keepAlive OK.", "Main", LogCategory.I);
                                    }
                                    else
                                    {
                                        add_log_info(LogInfoType.INFO, "gFtpHelperImsi -> keepAlive NO.", "Main", LogCategory.I);
                                        Logger.Trace(LogInfoType.INFO, "gFtpHelperImsi -> keepAlive NO.", "Main", LogCategory.I);
                                    }
                                }
                            }
                            else
                            {
                                gFtpHelperImsi.Connected = false;
                                gFtpHelperImsi.Connect();
                            }                                                 
                        }
                        catch (Exception e)
                        {
                            add_log_info(LogInfoType.EROR, e.Message, "Main", LogCategory.I);
                            Logger.Trace(LogInfoType.EROR, e.Message, "Main", LogCategory.I);

                            if (keepAliveMode == 0)
                            {
                                string errInfo = string.Format("异常：{0}，gFtpHelperImsi重连FTP服务器...", e.Message);
                                add_log_info(LogInfoType.EROR, errInfo, "Main", LogCategory.I);
                                Logger.Trace(LogInfoType.EROR, errInfo, "Main", LogCategory.I);

                                gFtpHelperImsi.Connected = false;
                                gFtpHelperImsi.Connect();
                            }
                        }

                        #endregion       

                        #region gFtpHelperFile

                        try
                        {                                                                                                              
                            if (keepAliveMode == 0)
                            {
                                if (gFtpHelperFile.Connected == false)
                                {
                                    gFtpHelperFile.Connect();
                                }
                                else
                                {
                                    if (0 == gFtpHelperFile.Put("keepAlive", data))
                                    {
                                        add_log_info(LogInfoType.INFO, "gFtpHelperFile -> keepAlive OK.", "Main", LogCategory.I);
                                        Logger.Trace(LogInfoType.INFO, "gFtpHelperFile -> keepAlive OK.", "Main", LogCategory.I);
                                    }
                                    else
                                    {
                                        add_log_info(LogInfoType.INFO, "gFtpHelperFile -> keepAlive NO.", "Main", LogCategory.I);
                                        Logger.Trace(LogInfoType.INFO, "gFtpHelperFile -> keepAlive NO.", "Main", LogCategory.I);
                                    }
                                }
                            }
                            else
                            {
                                gFtpHelperFile.Connected = false;
                                gFtpHelperFile.Connect();
                            }                                             
                        }
                        catch (Exception e)
                        {
                            add_log_info(LogInfoType.EROR, e.Message, "Main", LogCategory.I);
                            Logger.Trace(LogInfoType.EROR, e.Message, "Main", LogCategory.I);

                            if (keepAliveMode == 0)
                            {
                                string errInfo = string.Format("异常：{0}，gFtpHelperFile重连FTP服务器...", e.Message);
                                add_log_info(LogInfoType.EROR, errInfo, "Main", LogCategory.I);
                                Logger.Trace(LogInfoType.EROR, errInfo, "Main", LogCategory.I);

                                gFtpHelperFile.Connected = false;
                                gFtpHelperFile.Connect();
                            }
                        }

                        #endregion           

                        #region 计时复位

                        startTimeConn = System.DateTime.Now;

                        #endregion
                    }

                    #endregion

                    #region 上传处理

                    lock (mutex_FtpHelper)
                    {
                        //动态计算要更新的数量
                        FtpBatchUpdateRecords = get_level_list_value(FtpBatchUpdateRecordsLevelList, gCaptureInfoFtp.Count);

                        if (gCaptureInfoFtp.Count < FtpBatchUpdateRecords)
                        {
                            #region 数量不足

                            endTime = System.DateTime.Now;
                            ts = endTime.Subtract(startTime);

                            if (ts.TotalSeconds < FtpMaxIdleSeconds)
                            {
                                noMsg = true;
                                continue;
                            }
                            else
                            {
                                //清空数据
                                batchData0 = new List<strCapture>();

                                //拷贝数据
                                while (gCaptureInfoFtp.Count > 0)
                                {
                                    batchData0.Add(gCaptureInfoFtp.Dequeue());
                                }

                                //处理数据
                                BeginInvoke(new db_batch_process_delegate(ftp_batch_process_delegate_fun), new object[] { batchData0 });

                                //复位计时  
                                startTime = System.DateTime.Now;
                            }

                            #endregion
                        }
                        else
                        {
                            #region 数量充足

                            #region 清空数据

                            if (0 == (pingPangIndex % 3))
                            {
                                batchData0 = new List<strCapture>();
                            }
                            else if (1 == (pingPangIndex % 3))
                            {
                                batchData1 = new List<strCapture>();
                            }
                            else
                            {
                                batchData2 = new List<strCapture>();
                            }

                            #endregion

                            #region 从队列中获取设定的批量数据

                            for (int i = 0; i < FtpBatchUpdateRecords; i++)
                            {
                                if (0 == (pingPangIndex % 3))
                                {
                                    batchData0.Add(gCaptureInfoFtp.Dequeue());
                                }
                                else if (1 == (pingPangIndex % 3))
                                {
                                    batchData1.Add(gCaptureInfoFtp.Dequeue());
                                }
                                else
                                {
                                    batchData2.Add(gCaptureInfoFtp.Dequeue());
                                }
                            }

                            #endregion

                            #region 处理批量的数据

                            if (0 == (pingPangIndex % 3))
                            {
                                BeginInvoke(new db_batch_process_delegate(ftp_batch_process_delegate_fun), new object[] { batchData0 });
                            }
                            else if (1 == (pingPangIndex % 3))
                            {
                                BeginInvoke(new db_batch_process_delegate(ftp_batch_process_delegate_fun), new object[] { batchData1 });
                            }
                            else
                            {
                                BeginInvoke(new db_batch_process_delegate(ftp_batch_process_delegate_fun), new object[] { batchData2 });
                            }

                            #endregion

                            #region 复位计时

                            //复位起始时间
                            startTime = System.DateTime.Now;

                            #endregion

                            #endregion
                        }
                    }

                    noMsg = false;
                    pingPangIndex++;

                    #endregion
                }
            }
            catch (Exception ee)
            {
                add_log_info(LogInfoType.EROR, ee.Message, "Main", LogCategory.I);
                Logger.Trace(LogInfoType.EROR, ee.Message, "Main", LogCategory.I);
            }
        }

        #endregion

        #region 窗体相关处理

        public FrmMainController()
        {
            #region 初始化DataController
            
            add_log_info(LogInfoType.INFO, "今天是个好日子！", "Main", LogCategory.I);
            add_log_info(LogInfoType.INFO, "欢迎进入ScannerBackgrdServer！", "Main", LogCategory.I);
                        
            Logger.Trace(LogInfoType.INFO, "今天是个好日子！", "Main", LogCategory.I);
            Logger.Trace(LogInfoType.INFO, "欢迎进入ScannerBackgrdServer！", "Main", LogCategory.I);

            //add_log_info(LogInfoType.INFO, "StrAppDebugMode = " + DataController.StrAppDebugMode);
            //add_log_info(LogInfoType.INFO, "StrDbSwitch = " + DataController.StrDbSwitch);
            //add_log_info(LogInfoType.INFO, "StrDbIpAddr = " + DataController.StrDbIpAddr);
            //add_log_info(LogInfoType.INFO, "StrDbName = " + DataController.StrDbName);
            //add_log_info(LogInfoType.INFO, "StrDbUserId = " + DataController.StrDbUserId);
            //add_log_info(LogInfoType.INFO, "StrDbUserPsw = " + DataController.StrDbUserPsw);
            //add_log_info(LogInfoType.INFO, "StrDbPort = " + DataController.StrDbPort);
            //add_log_info(LogInfoType.INFO, "StrFtpSwitch = " + DataController.StrFtpSwitch);
            //add_log_info(LogInfoType.INFO, "StrFtpIpAddr = " + DataController.StrFtpIpAddr);
            //add_log_info(LogInfoType.INFO, "StrFtpUserId = " + DataController.StrFtpUserId);
            //add_log_info(LogInfoType.INFO, "StrFtpUserPsw = " + DataController.StrFtpUserPsw);
            //add_log_info(LogInfoType.INFO, "StrFtpPort = " + DataController.StrFtpPort);
            //add_log_info(LogInfoType.INFO, "StrFtpUserDir = " + DataController.StrFtpUserDir);            

            #endregion

            #region 初始化ApCtrl

            try
            {
                new Ap_LTE().Start(int.Parse(DataController.StrStartPortLTE));
                new Ap_WCDMA().Start(int.Parse(DataController.StrStartPortWCDMA));

                // 2018-07-25
                new Ap_GSM_ZYF().Start(int.Parse(DataController.StrStartPortGSM_ZYF));
                new Ap_CDMA_ZYF().Start(int.Parse(DataController.StrStartPortCDMA_ZYF));
                new Ap_GSM_HJT().Start(int.Parse(DataController.StrStartPortGSM_HJT));
            }
            catch (Exception ee)
            {
                add_log_info(LogInfoType.EROR, ee.Message, "Main", LogCategory.I);
                Logger.Trace(LogInfoType.EROR, ee.Message, "Main", LogCategory.I);
            }

            #endregion

            #region 初始化AppCtrl

            try
            {
                new App_Windows().Start(int.Parse(DataController.StrStartPortAppWindows));
            }
            catch (Exception ee)
            {
                add_log_info(LogInfoType.EROR, ee.Message, "Main", LogCategory.I);
                Logger.Trace(LogInfoType.EROR, ee.Message, "Main", LogCategory.I);
            }

            #endregion                     

            InitializeComponent();

            if (DataController.StrAppDebugMode.Equals("1"))
            {
                //调试模式

                button1.Visible = false;
                button2.Visible = false;
                button3.Visible = false;
                button4.Visible = false;
                button5.Visible = false;
                button6.Visible = false;
                button8.Visible = false;
                button9.Visible = false;
                button10.Visible = false;
                button12.Visible = false;
                button13.Visible = false;

                textBox1.Visible = false;
                panel1.Height = 85;
             
                label3.Text = string.Format("当前调试级别:{0}", DataController.LogOutputLevel.ToString());                

                pictureBox1.Visible = false;
                label1.Visible = false;
                label2.Visible = false;
                //timer1.Enabled = false;


                button16.Visible = false;
                label5.Visible = false;

                richTextBoxLog.Dock = DockStyle.Fill;

                this.Height = 600;
                this.Width = 750;

                label3.Text = string.Format("当前级别:{0}", DataController.LogOutputLevel.ToString());
            }
            else
            {
                //非调试模式

                panel1.Visible = false;
                richTextBoxLog.Visible = false;

                this.Height = 600;
                this.Width = 450;

                label5.Text = string.Format("当前级别:{0}", DataController.LogOutputLevel.ToString());
            }

            gCurLogInfoTypeIndex = (int)DataController.LogOutputLevel;
            this.StartPosition = FormStartPosition.CenterScreen;
        }
 
        private void FrmMainController_Load(object sender, EventArgs e)
        {                                             
            #region 启动Log线程

            //通过ParameterizedThreadStart创建线程
            Thread thread1 = new Thread(new ParameterizedThreadStart(thread_for_logger));

            thread1.Priority = ThreadPriority.Lowest;

            //给方法传值
            thread1.Start("this is elephant speaking!\n");
            thread1.IsBackground = true;

            #endregion

            #region 启动用于接收ApController消息的线程

            //通过ParameterizedThreadStart创建线程
            Thread thread2 = new Thread(new ParameterizedThreadStart(thread_for_ap_controller));

            //给方法传值
            thread2.Start("thread_for_ap_controller!\n");
            thread2.IsBackground = true;

            #endregion

            #region 启动用于接收AppController消息的线程

            //通过ParameterizedThreadStart创建线程
            Thread thread3 = new Thread(new ParameterizedThreadStart(thread_for_app_controller));

            //给方法传值
            thread3.Start("thread_for_app_controller!\n");
            thread3.IsBackground = true;

            #endregion

            #region 启动用于处理DbHelper的线程

            //通过ParameterizedThreadStart创建线程
            Thread thread4 = new Thread(new ParameterizedThreadStart(thread_for_db_helper));

            //给方法传值
            thread4.Start("thread_for_db_helper!\n");
            thread4.IsBackground = true;

            #endregion

            #region 启动用于处理FtpHelper的线程

            //通过ParameterizedThreadStart创建线程
            Thread thread5 = new Thread(new ParameterizedThreadStart(thread_for_ftp_helper));

            //给方法传值
            thread5.Start("thread_for_ftp_helper!\n");
            thread5.IsBackground = true;

            #endregion

            #region 初始化号码归属地

            Stopwatch sw = new Stopwatch();
            sw.Start();

            //用于快速通过设备的全名早点设备对应的ID
            if (0 == Location_And_Operator_Init())
            {
                add_log_info(LogInfoType.INFO, "Location_And_Operator_Init -> OK！", "Main", LogCategory.I);
                Logger.Trace(LogInfoType.INFO, "Location_And_Operator_Init -> OK！", "Main", LogCategory.I);
            }
            else
            {
                add_log_info(LogInfoType.INFO, "Location_And_Operator_Init -> NO！", "Main", LogCategory.I);
                Logger.Trace(LogInfoType.INFO, "Location_And_Operator_Init -> NO！", "Main", LogCategory.I);
            }

            sw.Stop();
            TimeSpan ts2 = sw.Elapsed;
            string info = string.Format("Location_And_Operator_Init->Stopwatch总共花费:{0}ms",Math.Ceiling(ts2.TotalMilliseconds));
            add_log_info(LogInfoType.WARN, info, "Main", LogCategory.I);

            #endregion

            #region 启动Logger消息线程

            Logger.Start();

            #endregion
        }

        private void FrmMainController_FormClosing(object sender, FormClosingEventArgs e)
        {
            DialogResult dr = MessageBox.Show("    是否退出应用程序?", "提示:", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);

            if (dr == DialogResult.OK)   //如果单击"是"按钮
            {
                //关闭窗体
                e.Cancel = false;
            }
            else if (dr == DialogResult.Cancel)
            {
                //不执行操作
                e.Cancel = true;
            }
        }

        #endregion

        #region 调试按钮处理

        /// <summary>
        /// 进程的线程详细信息
        /// </summary>
        public static void GetProcessThreadInfomation()
        {
            Process pro = Process.GetProcessesByName("ScannerBackgrdServer")[0];

            Console.WriteLine("ScannerBackgrdServer程序进程的线程详细信息如下：");
            int length = pro.Threads.Count;
            for (int i = 0; i < length; i++)
            {
                var thread = pro.Threads[i];

                Console.WriteLine("标识符：" + thread.Id.ToString());
                Console.WriteLine("基本优先级：" + thread.BasePriority.ToString());
                Console.WriteLine("当前优先级：" + thread.CurrentPriority.ToString());
                Console.WriteLine("内存地址：" + thread.StartAddress.ToInt32());
                Console.WriteLine("启动时间：" + thread.StartTime.ToString());
                Console.WriteLine("使用时间：" + thread.UserProcessorTime.ToString());
                Console.Write("当前状态：");


                switch (thread.ThreadState)
                {
                    case System.Diagnostics.ThreadState.Initialized:
                        Console.WriteLine("线程已经初始化但尚未启动");
                        break;
                    case System.Diagnostics.ThreadState.Ready:
                        Console.WriteLine("线程准备在下一个可用的处理器上运行");
                        break;
                    case System.Diagnostics.ThreadState.Running:
                        Console.WriteLine("当前正在使用处理器");
                        break;
                    case System.Diagnostics.ThreadState.Standby:
                        Console.WriteLine("线程将要使用处理器");
                        break;
                    case System.Diagnostics.ThreadState.Terminated:
                        Console.WriteLine("线程已完成执行并退出");
                        break;
                    case System.Diagnostics.ThreadState.Transition:
                        Console.WriteLine("线程在可以执行钱等待处理器之外的资源");
                        break;
                    case System.Diagnostics.ThreadState.Unknown:
                        Console.WriteLine("状态未知");
                        break;
                    case System.Diagnostics.ThreadState.Wait:
                        Console.WriteLine("正在等待外围操作完成或者资源释放");
                        break;
                    default:
                        break;
                }

                /*
                if (thread.ThreadState == System.Diagnostics.ThreadState.Wait)
                {

                    Console.Write("等待原因：");
                    switch (thread.WaitReason)
                    {
                        case ThreadWaitReason.EventPairHigh:
                            Console.WriteLine("线程正在等待事件对高");
                            break;
                        case ThreadWaitReason.EventPairLow:
                            Console.WriteLine("线程正在等待事件对低");
                            break;
                        case ThreadWaitReason.ExecutionDelay:
                            Console.WriteLine("线程执行延迟");
                            break;
                        case ThreadWaitReason.Executive:
                            Console.WriteLine("线程正在等待计划程序");
                            break;
                        case ThreadWaitReason.FreePage:
                            Console.WriteLine("线程正在等待可用的虚拟内存页");
                            break;
                        case ThreadWaitReason.LpcReceive:
                            Console.WriteLine("线程正在等待本地过程调用到达");
                            break;
                        case ThreadWaitReason.LpcReply:
                            Console.WriteLine("线程正在等待对本地过程调用的回复到达");
                            break;
                        case ThreadWaitReason.PageIn:
                            Console.WriteLine("线程正在等待虚拟内存页到达内存");
                            break;
                        case ThreadWaitReason.PageOut:
                            Console.WriteLine("线程正在等待虚拟内存页写入磁盘");
                            break;
                        case ThreadWaitReason.Suspended:
                            Console.WriteLine("线程执行暂停");
                            break;
                        case ThreadWaitReason.SystemAllocation:
                            Console.WriteLine("线程正在等待系统分配");
                            break;
                        case ThreadWaitReason.Unknown:
                            Console.WriteLine("线程因位置原因而等待");
                            break;
                        case ThreadWaitReason.UserRequest:
                            Console.WriteLine("线程正在等待用户请求");
                            break;
                        case ThreadWaitReason.VirtualMemory:
                            Console.WriteLine("线程正在等待系统分配虚拟内存");
                            break;
                        default:
                            break;
                    }
                }
                */
                Console.WriteLine();
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string dd = Guid.NewGuid().ToString(); // 9af7f46a-ea52-4aa3-b8c3-9fd484c2af12


            MessageBox.Show("this is elephant speaking! " + dd);

            add_log_info(LogInfoType.INFO, "this is elephant speaking!", "Main", LogCategory.I);

            dd = "root" + dd.Substring(0, 4);
            string str1 = Common.Common.Encode(dd);

            string ddddd = Common.Common.Decode(str1);

            string str2 = Common.Common.Encode("ftpuser");

            string str3 = Common.Common.Decode("Af8SZ2BneUw=");
            string str4 = Common.Common.Decode("S4N5N1nIj1Y=");

            string strxx = "1,2,3,t";
            string[] s = strxx.Split(new char[] { ',' });

            foreach (string str in s)
            {
                try
                {
                    UInt16.Parse(str);
                }
                catch (Exception ee)
                {
                    Logger.Trace(LogInfoType.EROR, ee.Message, "Main", LogCategory.I);
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            //add_log_info(LogInfoType.WARN, "this is elephant speaking!");

            add_log_info(LogInfoType.WARN, "接收心跳数：" + Ap_LTE.heartbeatMsgNum, "Main", LogCategory.I);
            add_log_info(LogInfoType.WARN, "接收IMSI数：" + Ap_LTE.imsiMsgNum, "Main", LogCategory.I);

            //add_log_info(LogInfoType.WARN, "接收消息数：" + DeviceManager.recvDeciveMsgNum);
            //add_log_info(LogInfoType.WARN, "处理消息数：" + DeviceManager.handleDeciveMsgNum);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            //GetProcessThreadInfomation();
            strGcMisc gm = new strGcMisc();
            gDbHelper.gc_misc_record_get_by_devid(-1, 1, ref gm);
            Logger.Trace(LogInfoType.EROR, "this is elephant speaking!", "Main", LogCategory.I);

            add_log_info(LogInfoType.EROR, null, "Main", LogCategory.I);

            byte[] aa = new byte[3];

            try
            {
                byte b = aa[5];
            }
            catch (Exception ee)
            {
                Logger.Trace(LogInfoType.EROR, ee.Message, "Main", LogCategory.I);
            }           
        }

        private void button4_Click(object sender, EventArgs e)
        {
            //gFtpHelper = new FtpHelper(gDataController.StrFtpIpAddr,"123",gDataController.StrFtpUserId,gDataController.StrFtpUserPsw,int.Parse(gDataController.StrFtpPort));            

            int code = gFtpHelperImsi.SendNoop();
            MessageBox.Show(code.ToString());
        }

        private void button6_Click(object sender, EventArgs e)
        {
            GC.Collect();
        }

        private void button7_Click(object sender, EventArgs e)
        {        
            MessageBox.Show("长度：" + richTextBoxLog.Text.Length);
            richTextBoxLog.Text = "";
            Ap_LTE.heartbeatMsgNum = 0;
            Ap_LTE.imsiMsgNum = 0;
            DeviceManager.recvDeciveMsgNum = 0;
            DeviceManager.handleDeciveMsgNum = 0;
        }

        private void button5_Click(object sender, EventArgs e)
        {
           // gFtpHelperImsi.Connect();
            gFtpHelperImsi.Put(@textBox1.Text);

            //gFtpHelper.Put(@"D:\\ucl.chm");           

            //gFtpHelper.Put(@textBox1.Text);

            //byte[] firstString = System.Text.Encoding.Default.GetBytes("123456789ABCDEF\n");

            //using (MemoryStream ms = new MemoryStream())
            //{
            //    // Write the first string to the stream.  
            //    ms.Write(firstString, 0, firstString.Length);

            //    // Set the position to the beginning of the stream.  
            //    ms.Seek(0, SeekOrigin.Begin);

            //    gFtpHelperImsi.Put(@"123456789.txt",ms);
            //}
        }

        private void button8_Click(object sender, EventArgs e)
        {
            //gFtpHelper.Get(@"123.jpg", @"F:\\333", @"123.jpg");
            gFtpHelperImsi.Get("12*", @"F:\\333");
        }
     
        private void button9_Click(object sender, EventArgs e)
        {
            DataTable dt = new DataTable();
            strCaptureQuery cq = new strCaptureQuery();

            cq.affDeviceId = 2;
            //cq.imsi = "1";
            cq.RmDupFlag = 0;
            cq.topCount = 5;

            //cq.bwFlag = bwType.BWTYPE_BLACK;
            cq.timeStart = "2018-05-29 12:34:56";
            cq.timeEnded = "2018-06-29 12:34:56";


            //int ret  = gDbHelper.update_info_record_insert("f1d63db8b273b92b51339c78f8d04227", "1.tar.gz","x.y.z");

            int ret = gDbHelper.capture_record_entity_query(ref dt,cq);

            string str;
            foreach (DataRow dr in dt.Rows)
            {
                str = "";
                for (int i = 0; i < dt.Columns.Count; i++)
                {
                    if (i == (dt.Columns.Count - 1))
                    {
                        str += dr[i].ToString();
                    }
                    else
                    {
                        str += dr[i].ToString() + " -- ";
                    }
                }

                MessageBox.Show(str); 
            }


            //DataTable dt = new DataTable();
            //int ret = gDbHelper.user_record_entity_get(ref dt);

            //string str;
            //foreach (DataRow dr in dt.Rows)
            //{
            //    str = "";
            //    for (int i = 0; i < dt.Columns.Count; i++)
            //    {
            //        if (i == (dt.Columns.Count - 1))
            //        {
            //            str += dr[i].ToString();
            //        }
            //        else
            //        {
            //            str += dr[i].ToString() + " -- ";
            //        }
            //    }

            //    //MessageBox.Show(str); 
            //}

            ret = gDbHelper.user_record_insert("root1", "1234", "asdadfadsfdaf");

            ret = gDbHelper.user_record_check("root1", "1234");

            ret = gDbHelper.user_record_update("root1", "1234", "123456");

            ret = gDbHelper.roletype_record_entity_get(ref dt);
            foreach (DataRow dr in dt.Rows)
            {
                str = "";
                for (int i = 0; i < dt.Columns.Count; i++)
                {
                    if (i == (dt.Columns.Count - 1))
                    {
                        str += dr[i].ToString();
                    }
                    else
                    {
                        str += dr[i].ToString() + " -- ";
                    }
                }

                //MessageBox.Show(str);
            }

            //ret = gDbHelper.roletype_record_insert("elephant", "12345");
            //ret = gDbHelper.roletype_record_insert("SuperAdmin", "12345");

            //ret = gDbHelper.roletype_record_delete("elephant");

            ret = gDbHelper.role_record_entity_get(ref dt);
            foreach (DataRow dr in dt.Rows)
            {
                str = "";
                for (int i = 0; i < dt.Columns.Count; i++)
                {
                    if (i == (dt.Columns.Count - 1))
                    {
                        str += dr[i].ToString();
                    }
                    else
                    {
                        str += dr[i].ToString() + " -- ";
                    }
                }

                //MessageBox.Show(str);
            }

            //ret = gDbHelper.role_record_insert("aaaaa", "Administrator", "2015-04-22", "2015-04-26", "adfad");

            ret = ret = gDbHelper.role_record_delete("aaaaa");


            ret = gDbHelper.privilege_record_entity_get(ref dt);
            foreach (DataRow dr in dt.Rows)
            {
                str = "";
                for (int i = 0; i < dt.Columns.Count; i++)
                {
                    if (i == (dt.Columns.Count - 1))
                    {
                        str += dr[i].ToString();
                    }
                    else
                    {
                        str += dr[i].ToString() + " -- ";
                    }
                }

                //MessageBox.Show(str);
            }


            //ret = gDbHelper.privilege_record_insert("111", "111", "111");

            ret = gDbHelper.privilege_record_delete("111");


            ret = gDbHelper.userrole_record_entity_get(ref dt);
            foreach (DataRow dr in dt.Rows)
            {
                str = "";
                for (int i = 0; i < dt.Columns.Count; i++)
                {
                    if (i == (dt.Columns.Count - 1))
                    {
                        str += dr[i].ToString();
                    }
                    else
                    {
                        str += dr[i].ToString() + " -- ";
                    }
                }

                //MessageBox.Show(str);
            }

            ret = gDbHelper.userrole_record_insert("root1", "RoleAdmin", "ssss");

            ret = gDbHelper.userrole_record_delete("root12", "RoleAdmin");


            ret = gDbHelper.roleprivilege_record_entity_get(ref dt);
            foreach (DataRow dr in dt.Rows)
            {
                str = "";
                for (int i = 0; i < dt.Columns.Count; i++)
                {
                    if (i == (dt.Columns.Count - 1))
                    {
                        str += dr[i].ToString();
                    }
                    else
                    {
                        str += dr[i].ToString() + " -- ";
                    }
                }

                MessageBox.Show(str);
            }

            ret = gDbHelper.roleprivilege_record_insert("RoleAdmin", "1,2,3,4,5,6,7,8,9", "adsfadfd");


            //ret = gDbHelper.roleprivilege_record_insert("RoleSO", "1,2,3,4,5,6,7,8,9", "adsfadfd");

            //ret = gDbHelper.roleprivilege_record_update("RoleSO", "1,2,3,4,5,6,7,8,9,10,11", "adsfadfd");

            ret = gDbHelper.roleprivilege_record_delete("RoleSO");


            ret = gDbHelper.domain_record_insert("深圳", "设备", 0, "this is SZ");
            ret = gDbHelper.domain_record_insert("东莞", "设备", 0, "this is DG");

            ret = gDbHelper.domain_record_insert("福田", "设备.深圳", 0, "this is FT");
            ret = gDbHelper.domain_record_insert("南山", "设备.深圳", 0, "this is NS");
            ret = gDbHelper.domain_record_insert("城区", "设备.东莞", 0, "this is NS");

            ret = gDbHelper.domain_record_insert("中心广场", "设备.深圳.福田", 0, "this is NS");
            ret = gDbHelper.domain_record_insert("莲花山", "设备.深圳.福田", 0, "this is NS");

            ret = gDbHelper.domain_record_insert("西北监控", "设备.深圳.福田.中心广场", 1, "this is elephant speaking");

            //ret = gDbHelper.domain_record_delete("设备.广州");
            //ret = gDbHelper.domain_record_delete("设备");


            //ret = gDbHelper.domain_record_entity_get(ref dt,1);
            //foreach (DataRow dr in dt.Rows)
            //{
            //    str = "";
            //    for (int i = 0; i < dt.Columns.Count; i++)
            //    {
            //        if (i == (dt.Columns.Count - 1))
            //        {
            //            str += dr[i].ToString();
            //        }
            //        else
            //        {
            //            str += dr[i].ToString() + " -- ";
            //        }
            //    }

            //    //MessageBox.Show(str);
            //}

            //ret = gDbHelper.domain_record_rename("设备.深圳", "设备.广州");


            ret = gDbHelper.userdomain_record_insert("root", "1,2,3,4", "aaaaaaaaaa");
            ret = gDbHelper.userdomain_record_insert("engi", "1,2,3,4", "aaaaaaaaaa");

            //ret = gDbHelper.userdomain_record_delete("root");


            ret = gDbHelper.userdomain_record_entity_get(ref dt);
            foreach (DataRow dr in dt.Rows)
            {
                str = "";
                for (int i = 0; i < dt.Columns.Count; i++)
                {
                    if (i == (dt.Columns.Count - 1))
                    {
                        str += dr[i].ToString();
                    }
                    else
                    {
                        str += dr[i].ToString() + " -- ";
                    }
                }

                //MessageBox.Show(str);
            }



            // ret = gDbHelper.device_record_insert(9, "LTE-FDD1");
            // ret = gDbHelper.device_record_insert(9, "LTE-FDD2");
            // ret = gDbHelper.device_record_insert(9, "LTE-FDD3");

            //ret = gDbHelper.device_record_delete(9, "LTE-FDD3");


            ret = gDbHelper.device_record_insert(9, "LTE-FDD3-dd", "LTE-FDD");


            strDevice dev = new strDevice();
            dev.sn = "EN1234567890";
            dev.netmask = "255.255.255.0";
            dev.mode = "中国人民解放军";
            dev.online = "1";
            dev.lastOnline = DateTime.Now.ToString();
            dev.isActive = "1";
            dev.ipAddr = "172.17.0.210";
            dev.port = "56789";
            ret = gDbHelper.device_record_update(9, "LTE-FDD2", dev);



            ret = gDbHelper.device_record_entity_get(ref dt);
            foreach (DataRow dr in dt.Rows)
            {
                str = "";
                for (int i = 0; i < dt.Columns.Count; i++)
                {
                    if (i == (dt.Columns.Count - 1))
                    {
                        str += dr[i].ToString();
                    }
                    else
                    {
                        str += dr[i].ToString() + " -- ";
                    }
                }

                MessageBox.Show(str);
            }

        }

        private void button10_Click(object sender, EventArgs e)
        {            
            //Stopwatch sw = new Stopwatch();
            //sw.Start();


            //for (int i = 0; i < 10*10000; i++)
            //{
            //    gDicRemoveDup.Add("12345" + i.ToString(), DateTime.Now);
            //}

            //sw.Stop();
            //TimeSpan ts2 = sw.Elapsed;
            //MessageBox.Show("Stopwatch总共花费ms : " + ts2.TotalMilliseconds.ToString());

            ////gDicRemoveDup.Add("123450", DateTime.Now);

            //sw.Restart();

            //string str = "";
            //if (gDicRemoveDup.ContainsKey("123450"))
            //{
            //    str = "ContainsKey";
            //}
            //else
            //{
            //    str = "NOContainsKey";
            //}

            //sw.Stop();
            //TimeSpan ts22 = sw.Elapsed;
            //MessageBox.Show(str + "Stopwatch总共花费ms : " + ts22.TotalMilliseconds.ToString());

            //int ret = 0;
            //ret = gDbHelper.domain_record_insert("深圳", "设备", 0, "this is SZ");
            //ret = gDbHelper.domain_record_insert("东莞", "设备", 0, "this is DG");

            //ret = gDbHelper.domain_record_insert("福田", "设备.深圳", 0, "this is FT");
            //ret = gDbHelper.domain_record_insert("南山", "设备.深圳", 0, "this is NS");
            //ret = gDbHelper.domain_record_insert("城区", "设备.东莞", 0, "this is NS");

            //ret = gDbHelper.domain_record_insert("中心广场", "设备.深圳.福田", 0, "this is NS");
            //ret = gDbHelper.domain_record_insert("莲花山", "设备.深圳.福田", 0, "this is NS");

            //ret = gDbHelper.domain_record_insert("西北监控", "设备.深圳.福田.中心广场", 1, "this is elephant speaking");

            //string nameFullPath = "{guangdong.shenzhen.nanshan.abc.def.123456789}";

            //int i = nameFullPath.LastIndexOf(".");
            //int j = nameFullPath.LastIndexOf("}");

            //Name = nameFullPath.Substring(i + 1, j - i - 1);
            //nameFullPath = nameFullPath.Substring(1, i - 1);  

            //string nameFullPath = "guangdong.123456789";
            //int i = nameFullPath.LastIndexOf(".");

            //string name1 = nameFullPath.Substring(i + 1);
            //string name2 = nameFullPath.Substring(0, i - 1);

            gDbHelper.device_record_if_rename(9, "电信FDD", "联通W");

            //strBwList list = new strBwList();
            //list.imei = "0123456789ABCDE";
            //list.bwFlag = bwType.BWTYPE_WHITE;
            //list.time = DateTime.Now.ToString();

            //list.imsi = "460000123456789";
            //list.bwFlag = bwType.BWTYPE_WHITE;
            //list.rbStart = "0";
            //list.rbEnd = "123";
            //list.time = DateTime.Now.ToString();

            //int rtv = gDbHelper.bwlist_record_insert(list, 4);

            //int rtv = gDbHelper.bwlist_record_imei_delete("0123456789ABCDE", bwType.BWTYPE_BLACK, 4);

            //int rtv = gDbHelper.bwlist_record_imsi_delete("460000123456789", bwType.BWTYPE_BLACK, 4);


            //int rtv = gDbHelper.roleprivilege_record_insert("RoleSO", "1,2,3,4,5,6,7,8,9", "afafadf");


            int rtv = gDbHelper.userdomain_record_insert("ABC", "1,3,9,12", "adfadfa");

            List<int> listDevId = new List<int>();
            //rtv = gDbHelper.domain_record_device_id_list_get("设备.深圳.福田.中心广场.西北监控",ref listDevId);

            rtv = gDbHelper.domain_record_device_id_list_get("设备", ref listDevId);

            //rtv = gDbHelper.ap_general_para_record_insert(1);

            strApGenPara apGP = new strApGenPara();
            apGP.mode = "GSM";
            apGP.NTP = "172.17.0.183";
            apGP.periodtac = "123";
            apGP.tac = "46000";
            apGP.earfcnul = "38520";

            //rtv = gDbHelper.ap_general_para_record_update(1, apGP);

            rtv = gDbHelper.ap_general_para_record_get_by_devid(1, ref apGP);


            DataTable dt = new DataTable();
            int ret = gDbHelper.bwlist_record_entity_get(ref dt);
            foreach (DataRow dr in dt.Rows)
            {
                string str = "";
                for (int i = 0; i < dt.Columns.Count; i++)
                {
                    if (i == (dt.Columns.Count - 1))
                    {
                        str += dr[i].ToString();
                    }
                    else
                    {
                        str += dr[i].ToString() + " -- ";
                    }
                }

                MessageBox.Show(str);
            }
        }                       
       
        private void button11_Click(object sender, EventArgs e)
        {
            if (button11.Text.Contains("停住"))
            {
                stopFlag = true;
                button11.Text = "开始显示";
            }
            else if (button11.Text.Contains("开始"))
            {
                stopFlag = false;
                button11.Text = "停住显示";
            }
            else
            {

            }

        }

        private static void OnTimedEvent(object source, ElapsedEventArgs e, int a)
        {
            MessageBox.Show("The Elapsed event was raised" + a.ToString());
        }

        private void button12_Click(object sender, EventArgs e)
        {

            string mm = string.Format("123{0,-10}--asdfasdf","00000");


            MessageBox.Show(mm.ToString());

            string temp = string.Empty;
            UTF8Encoding utf8 = new UTF8Encoding();
            byte[] encodedBytes = utf8.GetBytes("中国");
            foreach (byte b in encodedBytes)
            {
                temp += "%" + b.ToString("X");
            }
            MessageBox.Show(temp);


            List<int> list = new List<int>();

            get_level_list(ref list, int.Parse(DataController.StrDbBatchUpdateRecordsLevel));

            try
            {
                int.Parse(textBox1.Text);
            }
            catch(Exception ee)
            {
                MessageBox.Show(ee.Message.ToString());
                return;
            }

            int dd = get_level_list_value(list, int.Parse(textBox1.Text));

            // MessageBox.Show(dd.ToString());


            gDbHelper.domain_record_update_des("设备.深圳.福田.中心广场", "adfadf");


            gDbHelper.domain_record_rename("设备.深圳.福田.中心广场", "设备.深圳.福田.中心广场");
        }

        private void button13_Click(object sender, EventArgs e)
        {
            strGcMisc gm = new strGcMisc();
            // gm.bOrmType = "1";
            // gm.bSMSContentLen = "23";
            //gm.bUeContent= "adsfadfadfa";

            //gDbHelper.gc_misc_record_update(0, 1, gm);

            gDbHelper.gc_misc_record_get_by_devid(0, 1,ref  gm);

            gDbHelper.gc_misc_record_insert(0, 1);
            gDbHelper.gc_misc_record_insert(1, 1);


            List<strGcImsiAction> ll = new List<strGcImsiAction>();
            gDbHelper.gc_imsi_action_record_get_by_devid(0, 1, ref ll);

            strGcImsiAction gia = new strGcImsiAction();
            gia.bIMSI = "46001123456788";
            gia.bUeActionFlag = "1";

            gDbHelper.gc_imsi_action_record_insert(0, 1, gia);

            gia.bIMSI = "46001123456787";
            gia.bUeActionFlag = "1";

            gDbHelper.gc_imsi_action_record_insert(0, 1, gia);


            gDbHelper.gc_param_config_record_delete(0, 1);
            strGcParamConfig gpr1 = new strGcParamConfig();
            gDbHelper.gc_param_config_record_get_by_devid(0, 1, ref gpr1);

            strGcParamConfig gpr = new strGcParamConfig();

            gpr.bWorkingMode = "1";         //工作模式。1：侦码模式；3：驻留模式(GSM/CDMA支持)            
            gpr.bPLMNId = "331";              //PLMN标志。ASCII字符           
            gpr.bRxGain = "12";              //保留字段。Unit: dB

           
            gDbHelper.gc_param_config_record_update(0, 1,gpr);

            // gDbHelper.gc_param_report_record_insert(0, 1);
            //gDbHelper.gc_param_report_record_insert(1, 1);

            gDbHelper.gc_nb_cell_record_delete(0, 1);

            strGcNbCell str = new strGcNbCell();
            strGcNbCellItem item = new strGcNbCellItem();

            str.bC2 = "122";
            str.bNbCellNum = "33";
            str.wTac = "345";
            str.wPhyCellId = "3667";

            item.bC2 = "1";
            item.cRSRP = "-9";
            item.wPhyCellId = "12";

            str.listItem = new List<strGcNbCellItem>();
            str.listItem.Add(item);
            str.listItem.Add(item);
            str.listItem.Add(item);
            str.listItem.Add(item);

            List<strGcNbCell> listBig = new List<strGcNbCell>();

            listBig.Add(str);
            listBig.Add(str);
            listBig.Add(str);
            listBig.Add(str);
            listBig.Add(str);

            gDbHelper.gc_nb_cell_record_insert_batch(0, 1, listBig);

            List<strGcNbCell> listTest = new List<strGcNbCell>();
            gDbHelper.gc_nb_cell_record_get_by_devid(0, 1, ref listTest);

            //int rtv;
            //string tmp = "144430757";
            //Int32 i = Convert.ToInt32(tmp);

            //StringBuilder ss = new StringBuilder(10);
            //ss.Append("asdfadf\n");
            //ss.Append("asdfadf\n");
            //ss.Append("asdfadf\n");

            //byte[] outData = System.Text.Encoding.Default.GetBytes(ss.ToString());

            //Stopwatch sw = new Stopwatch();
            //sw.Start();

            //strImsiParse ip = new strImsiParse();

            //Location_And_Operator_Get("460003472179561", ref ip);


            ////Location_And_Operator_Get("460003472179562", ref ip);
            ////Location_And_Operator_Get("460003472179563", ref ip);
            ////Location_And_Operator_Get("460003472179564", ref ip);
            ////Location_And_Operator_Get("460003472179565", ref ip);
            ////Location_And_Operator_Get("460003472179566", ref ip);
            ////Location_And_Operator_Get("460003472179567", ref ip);
            ////Location_And_Operator_Get("460003472179568", ref ip);
            ////Location_And_Operator_Get("460003472179569", ref ip);
            ////Location_And_Operator_Get("460003472179570", ref ip);
            ////Location_And_Operator_Get("460003472179571", ref ip);
            ////Location_And_Operator_Get("460003472179572", ref ip);
            ////Location_And_Operator_Get("460003472179573", ref ip);
            ////Location_And_Operator_Get("460003472179574", ref ip);
            ////Location_And_Operator_Get("460003472179575", ref ip);
            ////Location_And_Operator_Get("460003472179576", ref ip);
            ////Location_And_Operator_Get("460003472179577", ref ip);
            ////Location_And_Operator_Get("460003472179578", ref ip);
            ////Location_And_Operator_Get("460003472179579", ref ip);
            ////Location_And_Operator_Get("460003472179580", ref ip);
            ////Location_And_Operator_Get("460003472179581", ref ip);
            ////Location_And_Operator_Get("460003472179582", ref ip);
            ////Location_And_Operator_Get("460003472179583", ref ip);
            ////Location_And_Operator_Get("460003472179584", ref ip);
            ////Location_And_Operator_Get("460003472179585", ref ip);
            ////Location_And_Operator_Get("460003472179586", ref ip);
            ////Location_And_Operator_Get("460003472179587", ref ip);

            //sw.Stop();
            //TimeSpan ts2 = sw.Elapsed;

            //string info1 = string.Format("Location_And_Operator_Get->Stopwatch总共花费:{0}ms,{1}{2}", ts2.TotalMilliseconds,ip.country,ip.location);
            //add_log_info(LogInfoType.INFO, info1, "Main", LogCategory.I);





            //gDbHelper.ap_general_para_string_get_by_devid(1, ref tmp);

            //List<string> lst = new List<string>();
            //gDbHelper.bwlist_record_md5sum_get(bwType.BWTYPE_WHITE, 1, ref lst);

            //lst.Sort();

            //Get_Md5_Sum(lst, 1, ref tmp);

            //byte[] data = null;
            //rtv = generate_ftp_byte(ref data, lst);
            //rtv = gFtpHelperFile.Put("dddddd.txt", data);


            ////从FTP服务器上下载文件
            //rtv = gFtpHelperFile.Get("1234.txt", Application.StartupPath, "1234.txt");


            //string info = "";
            //List<strBwList> list = new List<strBwList>();
            //string fileFullPath = string.Format("{0}\\{1}", Application.StartupPath, "1234.txt");

            ////更新到数据库中
            //rtv = Get_BwList_From_File(fileFullPath, bwType.BWTYPE_WHITE, ref list, ref info);

            //rtv = gDbHelper.bwlist_record_bwflag_delete(bwType.BWTYPE_WHITE, 1);


            //rtv = gDbHelper.bwlist_record_insert_batch(list, 1);

            //从FTP服务器上下载文件
            // rtv = gFtpHelperFile.Get("5678.txt", Application.StartupPath, "5678.txt");


            //string info = "";
            // List<strBwList> list = new List<strBwList>();
            //string fileFullPath = string.Format("{0}\\{1}", Application.StartupPath, "5678.txt");

            //更新到数据库中
            // rtv = Get_BwList_From_File(fileFullPath, bwType.BWTYPE_BLACK, ref list, ref info);

            // rtv = gDbHelper.bwlist_record_bwflag_delete(bwType.BWTYPE_BLACK, 1);


            //  rtv = gDbHelper.bwlist_record_insert_batch(list, 1);



            //StringWriter sw = new StringWriter();
            //XmlTextWriter xw = new XmlTextWriter(sw);

            //// Save Xml Document to Text Writter.
            //myXmlDoc.WriteTo(xw);

            //System.Text.ASCIIEncoding encoding = new System.Text.ASCIIEncoding();

            //byte[] data = Encoding.UTF8.GetBytes(sw.ToString());

            //byte[] data;
            //using (MemoryStream ms = new MemoryStream())
            //{
            //    System.Xml.XmlDocument xmlDoc = new System.Xml.XmlDocument();
            //    xmlDoc.Load(@"F:\\test.xml");

            //    xmlDoc.Save(ms);
            //    data = ms.ToArray();
            //    int len = data.Length;

            //    StringWriter sw = new StringWriter();
            //    System.Xml.XmlTextWriter xw = new System.Xml.XmlTextWriter(sw);

            //    // Save Xml Document to Text Writter.
            //    xmlDoc.WriteTo(xw);
            //    byte[] data1 = Encoding.GetEncoding("GBK").GetBytes(sw.ToString()); //KO

            //    //byte[] data1 = Encoding.UTF8.GetBytes(sw.ToString());  //NO
            //    int len1 = data1.Length;
            //}
        }

        private void button14_Click(object sender, EventArgs e)
        {
            gCurLogInfoTypeIndex = (gCurLogInfoTypeIndex + 1) % 4;

            DataController.LogOutputLevel = (LogInfoType)gCurLogInfoTypeIndex;
            label3.Text = string.Format("当前级别:{0}", DataController.LogOutputLevel.ToString());
        }

        private void button16_Click(object sender, EventArgs e)
        {
            gCurLogInfoTypeIndex = (gCurLogInfoTypeIndex + 1) % 4;

            DataController.LogOutputLevel = (LogInfoType)gCurLogInfoTypeIndex;
            label5.Text = string.Format("当前级别:{0}", DataController.LogOutputLevel.ToString());
        }

        private void button16_MouseEnter(object sender, EventArgs e)
        {
            toolTip1.ShowAlways = true;
            toolTip1.SetToolTip(this.button16, "切换设置调试Level.");
        }

        #endregion

        #region 显示运行时间

        ///<summary>
        ///由秒数得到日期几天几小时。。。
        ///</summary
        ///<param name="t">秒数</param>
        ///<returns>几天几小时几分几秒</returns>
        public static string parseTimeSeconds(long t)
        {
            string r = "";
            int day = 0;
            int hour = 0;
            int minute = 0;
            int second = 0;

            if (t >= 86400) //天,
            {
                day = Convert.ToInt16(t / 86400);
                hour = Convert.ToInt16((t % 86400) / 3600);
                minute = Convert.ToInt16((t % 86400 % 3600) / 60);
                second = Convert.ToInt16(t % 86400 % 3600 % 60);                                         
            }
            else if (t >= 3600)//时,
            {
                hour = Convert.ToInt16(t / 3600);
                minute = Convert.ToInt16((t % 3600) / 60);
                second = Convert.ToInt16(t % 3600 % 60);                
            }
            else if (t >= 60)//分
            {
                minute = Convert.ToInt16(t / 60);
                second = Convert.ToInt16(t % 60);               
            }
            else
            {
                second = Convert.ToInt16(t);               
            }

            if (day > 0)
            {
                r = string.Format("{0} {1:D2}:{2:D2}:{3:D2}", day, hour, minute, second);
            }
            else
            {
                r = string.Format("{0:D2}:{1:D2}:{2:D2}",hour, minute, second);
            }

            return r;
        }

        //private long runTimeCnt = 86400 - 10;
        private long runTimeCnt = 0;
        private void timer1_Tick(object sender, EventArgs e)
        {
            runTimeCnt++;
            
            if (DataController.StrAppDebugMode.Equals("1"))
            {
                //this.Text = string.Format("Scanner后台服务器:{0}", label1.Text);
                label6.Text = parseTimeSeconds(runTimeCnt);
            }
            else
            {
                label1.Text = parseTimeSeconds(runTimeCnt);
            }

            ////for (int i = 0; i < 10; i++)
            ////{
            ////    Logger.Trace(LogInfoType.EROR, "sdfadsffadf", "Main", LogCategory.I);
            ////    add_log_info(LogInfoType.EROR, "sdfadsffadf", "Main", LogCategory.I);

            ////    Thread.Sleep(50);
            ////}

        }

        #endregion       
    }
}
