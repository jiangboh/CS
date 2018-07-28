using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ScannerBackgrdServer.Common;

namespace ScannerBackgrdServer
{
    public class DataController
    {
        #region 变量定义
        
        private static string strAppDebugMode = "1";
        private static string strDbSwitch = "1";
        private static string strDbIpAddr = "172.17.0.104";
        private static string strDbName = "ScannerBackgrdServer";
        private static string strDbUserId = "root";
        private static string strDbUserPsw = "root";
        private static string strDbPort = "3306";

        private static string strFtpSwitch = "1";
        private static string strFtpIpAddr = "172.17.0.104";
        private static string strFtpUserId = "root";
        private static string strFtpUserPsw = "root";
        private static string strFtpPort = "21";
        private static string strFtpImsiDir = "IMSI";
        private static string strFtpUpdateDir = "Update";

        private static string strDbBatchUpdateRecordsLevel = "6";
        private static string strDbMaxIdleSeconds = "60";

        private static string strFtpBatchUpdateRecordsLevel = "6";
        private static string strFtpMaxIdleSeconds = "60";

        // 2018-07-25
        private static string strStartPortCDMA_ZYF = "14783";
        private static string strStartPortGSM_ZYF = "14784";
        private static string strStartPortGSM_HJT = "14785";

        private static string strStartPortLTE = "14786";
        private static string strStartPortTDS = "14787";
        private static string strStartPortWCDMA = "14788";

        private static string strStartPortAppWindows = "14789";
        private static string strStartPortAppLinux = "14790";
        private static string strStartPortAppAndroid = "14791";

        private static int _apOnLineTime = 180;
        private static int _appOnLineTime = 180;

        private static int recordsOfPageSize = 200;

        private static int _timerTimeOutInterval = 8;

        /// <summary>
        /// 配置Log输出级别，大于等于该级别的都输出，否则不输出
        /// </summary>
        private static LogInfoType logOutputLevel = LogInfoType.DEBG;

        /// <summary>
        /// 检查FTP Server的连接状态的频率，秒数
        /// </summary>
        private static int checkFtpConnStatTime = 60;

        /// <summary>
        /// 对捕到的号是否进行起重复处理
        /// 1：进行去重
        /// 0：不进行去重
        /// </summary>
        private static int removeDupMode = 1;

        /// <summary>
        /// 去重的时间长度，分钟
        /// 即xx分钟之内不进行重复保存
        /// </summary>
        private static int removeDupTimeLength = 120;

        private static int simuTest = 0;

        /// <summary>
        /// 数据对齐基准：
        /// 0表示数据库为基准
        /// 1表示以Ap为基准
        /// </summary>
        private static int dataAlignMode = 0;

        /// <summary>
        /// 号码归属地计算方式：
        /// 0直接从接口，1接口加哈希
        /// </summary>
        private static int imsiParseMode = 1;
        
        #endregion

        #region 属性定义

        public static string StrAppDebugMode { get => strAppDebugMode; set => strAppDebugMode = value; }
        public static string StrDbSwitch { get => strDbSwitch; set => strDbSwitch = value; }
        public static string StrDbIpAddr { get => strDbIpAddr; set => strDbIpAddr = value; }
        public static string StrDbName { get => strDbName; set => strDbName = value; }
        public static string StrDbUserId { get => strDbUserId; set => strDbUserId = value; }
        public static string StrDbUserPsw { get => strDbUserPsw; set => strDbUserPsw = value; }
        public static string StrDbPort { get => strDbPort; set => strDbPort = value; }

        public static string StrFtpSwitch { get => strFtpSwitch; set => strFtpSwitch = value; }
        public static string StrFtpIpAddr { get => strFtpIpAddr; set => strFtpIpAddr = value; }
        public static string StrFtpUserId { get => strFtpUserId; set => strFtpUserId = value; }
        public static string StrFtpUserPsw { get => strFtpUserPsw; set => strFtpUserPsw = value; }
        public static string StrFtpPort { get => strFtpPort; set => strFtpPort = value; }
        public static string StrFtpImsiDir { get => strFtpImsiDir; set => strFtpImsiDir = value; }
        public static string StrFtpUpdateDir { get => strFtpUpdateDir; set => strFtpUpdateDir = value; }

       
        public static string StrDbMaxIdleSeconds { get => strDbMaxIdleSeconds; set => strDbMaxIdleSeconds = value; }
        public static string StrFtpMaxIdleSeconds { get => strFtpMaxIdleSeconds; set => strFtpMaxIdleSeconds = value; }

        public static string StrStartPortCDMA_ZYF { get => strStartPortCDMA_ZYF; set => strStartPortCDMA_ZYF = value; }
        public static string StrStartPortGSM_ZYF { get => strStartPortGSM_ZYF; set => strStartPortGSM_ZYF = value; }
        public static string StrStartPortGSM_HJT { get => strStartPortGSM_HJT; set => strStartPortGSM_HJT = value; }

        public static string StrStartPortLTE { get => strStartPortLTE; set => strStartPortLTE = value; }
        public static string StrStartPortTDS { get => strStartPortTDS; set => strStartPortTDS = value; }
        public static string StrStartPortWCDMA { get => strStartPortWCDMA; set => strStartPortWCDMA = value; }

        public static string StrStartPortAppWindows { get => strStartPortAppWindows; set => strStartPortAppWindows = value; }
        public static string StrStartPortAppLinux { get => strStartPortAppLinux; set => strStartPortAppLinux = value; }
        public static string StrStartPortAppAndroid { get => strStartPortAppAndroid; set => strStartPortAppAndroid = value; }

        public static int ApOnLineTime { get => _apOnLineTime; set => _apOnLineTime = value; }
        public static int AppOnLineTime { get => _appOnLineTime; set => _appOnLineTime = value; }

        public static LogInfoType LogOutputLevel { get => logOutputLevel; set => logOutputLevel = value; }
        public static int CheckFtpConnStatTime { get => checkFtpConnStatTime; set => checkFtpConnStatTime = value; }

        public static int RemoveDupMode { get => removeDupMode; set => removeDupMode = value; }
        public static int RemoveDupTimeLength { get => removeDupTimeLength; set => removeDupTimeLength = value; }        
        public static int RecordsOfPageSize { get => recordsOfPageSize; set => recordsOfPageSize = value; }

        public static string StrDbBatchUpdateRecordsLevel { get => strDbBatchUpdateRecordsLevel; set => strDbBatchUpdateRecordsLevel = value; }       
        public static string StrFtpBatchUpdateRecordsLevel { get => strFtpBatchUpdateRecordsLevel; set => strFtpBatchUpdateRecordsLevel = value; }
        public static int TimerTimeOutInterval { get => _timerTimeOutInterval; set => _timerTimeOutInterval = value; }

        public static int SimuTest { get => simuTest; set => simuTest = value; }
        public static int DataAlignMode { get => dataAlignMode; set => dataAlignMode = value; }
        public static int ImsiParseMode { get => imsiParseMode; set => imsiParseMode = value; }

        #endregion

        static DataController()
        {
            try
            {
                strAppDebugMode = ConfigurationManager.AppSettings["strAppDebugMode"].ToString();

                strDbSwitch = ConfigurationManager.AppSettings["strDbSwitch"].ToString();
                strDbIpAddr = ConfigurationManager.AppSettings["strDbIpAddr"].ToString();
                strDbName = ConfigurationManager.AppSettings["strDbName"].ToString();
                strDbUserId = ConfigurationManager.AppSettings["strDbUserId"].ToString();
                strDbUserPsw = ConfigurationManager.AppSettings["strDbUserPsw"].ToString();
                strDbUserPsw = Common.Common.Decode(strDbUserPsw);
                strDbPort = ConfigurationManager.AppSettings["strDbPort"].ToString();

                strFtpSwitch = ConfigurationManager.AppSettings["strFtpSwitch"].ToString();
                StrFtpIpAddr = ConfigurationManager.AppSettings["strFtpIpAddr"].ToString();
                strFtpUserId = ConfigurationManager.AppSettings["strFtpUserId"].ToString();
                strFtpUserPsw = ConfigurationManager.AppSettings["strFtpUserPsw"].ToString();
                strFtpUserPsw = Common.Common.Decode(StrFtpUserPsw);
                strFtpPort = ConfigurationManager.AppSettings["strFtpPort"].ToString();
                strFtpImsiDir = ConfigurationManager.AppSettings["strFtpImsiDir"].ToString();
                strFtpUpdateDir = ConfigurationManager.AppSettings["strFtpUpdateDir"].ToString();

                strDbBatchUpdateRecordsLevel = ConfigurationManager.AppSettings["strDbBatchUpdateRecordsLevel"].ToString();
                strDbMaxIdleSeconds = ConfigurationManager.AppSettings["strDbMaxIdleSeconds"].ToString();

                strFtpBatchUpdateRecordsLevel = ConfigurationManager.AppSettings["strFtpBatchUpdateRecordsLevel"].ToString();
                strFtpMaxIdleSeconds = ConfigurationManager.AppSettings["strFtpMaxIdleSeconds"].ToString();

                strStartPortCDMA_ZYF = ConfigurationManager.AppSettings["strStartPortCDMA_ZYF"].ToString();
                strStartPortGSM_ZYF = ConfigurationManager.AppSettings["strStartPortGSM_ZYF"].ToString();
                strStartPortGSM_HJT = ConfigurationManager.AppSettings["strStartPortGSM_HJT"].ToString();

                strStartPortLTE = ConfigurationManager.AppSettings["strStartPortLTE"].ToString();
                strStartPortTDS = ConfigurationManager.AppSettings["strStartPortTDS"].ToString();
                strStartPortWCDMA = ConfigurationManager.AppSettings["strStartPortWCDMA"].ToString();

                strStartPortAppWindows = ConfigurationManager.AppSettings["strStartPortAppWindows"].ToString();
                strStartPortAppLinux = ConfigurationManager.AppSettings["strStartPortAppLinux"].ToString();
                strStartPortAppAndroid = ConfigurationManager.AppSettings["strStartPortAppAndroid"].ToString();

                _apOnLineTime = int.Parse(ConfigurationManager.AppSettings["apOnLineTime"].ToString());
                _appOnLineTime = int.Parse(ConfigurationManager.AppSettings["appOnLineTime"].ToString());
                recordsOfPageSize = int.Parse(ConfigurationManager.AppSettings["recordsOfPageSize"].ToString());

                logOutputLevel = (LogInfoType)int.Parse(ConfigurationManager.AppSettings["logOutputLevel"].ToString());

                if (logOutputLevel > LogInfoType.EROR)
                {
                    logOutputLevel = LogInfoType.EROR;
                }

                if (logOutputLevel < LogInfoType.DEBG)
                {
                    logOutputLevel = LogInfoType.DEBG;
                }

                checkFtpConnStatTime = int.Parse(ConfigurationManager.AppSettings["checkFtpConnStat"].ToString());

                removeDupMode = int.Parse(ConfigurationManager.AppSettings["removeDupMode"].ToString());
                removeDupTimeLength = int.Parse(ConfigurationManager.AppSettings["removeDupTimeLength"].ToString());

                _timerTimeOutInterval = int.Parse(ConfigurationManager.AppSettings["timerTimeOutInterval"].ToString());

                simuTest = int.Parse(ConfigurationManager.AppSettings["simuTest"].ToString());
                dataAlignMode = int.Parse(ConfigurationManager.AppSettings["dataAlignMode"].ToString());
                imsiParseMode = int.Parse(ConfigurationManager.AppSettings["imsiParseMode"].ToString());
            }
            catch (Exception ee)
            {
                Logger.Trace(ee);
            }
        }
    }
}
