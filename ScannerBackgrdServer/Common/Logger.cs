using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ScannerBackgrdServer.Common
{
    public enum LogInfoType       //日志信息类型
    {
        DEBG = 0,
        INFO = 1,
        WARN = 2,
        EROR = 3,
    }

    public enum LogCategory      
    {
        R = 0,   //接收
        S = 1,   //发送
        I = 2    //信息
    }

    public class Logger
    {
        #region 定义

        private const string LOG_DEBG  = "\r\n【DEBG】[{0}][{1}] {2}({3})\r\n";
        private const string LOG_INFO  = "\r\n【INFO】[{0}][{1}] {2}({3})\r\n";
        private const string LOG_WARN  = "\r\n【WARN】[{0}][{1}] {2}({3})\r\n";
        private const string LOG_EROR  = "\r\n【EROR】[{0}][{1}] {2}({3})\r\n";

        private static DateTime currentLogFileDate = DateTime.Now;

        private static TextWriterTraceListener twtl;

        //private const string logRootDirectory = @"D:\log";

        private static string logRootDirectory = Application.StartupPath + @"\strLogInfo";

        //private static string logRootDirectory = @"C:\Apache24\htdocs\server";

        private static string logSubDirectory;
        //private static string outString;

        private static Mutex gMutexLog = new Mutex();

        private static long gLogIndex = 0;

        private enum LogOutType       //日志输出类型
        {
            MessageBoxOnly = 0,       //仅MessageBox输出
            FileOnly = 1,             //仅日志输出
            MessageBoxAndFile = 2,    //MessageBox输出+日志输出
        }

        private enum FileFlushType    //日志文件刷新类型
        {
            Standard = 0,             //标准刷新
            RightNow = 1,             //立即刷新
        }


        /// <summary>
        /// 配置记录输出类型
        /// 可以修改成从配置文件读取
        /// </summary>
        private static readonly LogOutType logOutType = LogOutType.FileOnly;

        /// <summary>
        /// 配置记录文件的刷新类型
        /// 可以修改成从配置文件读取
        /// </summary>
        private static readonly FileFlushType fileFlushType = FileFlushType.RightNow;


        #endregion

        #region 属性


        /// <summary>
        /// 获取或设置Log的根路径
        /// </summary>
        public static string LogRootDirectory
        {
            get
            {
                return logRootDirectory;
            }

            set
            {
                logRootDirectory = value;

                switch (logOutType)
                {
                    case LogOutType.MessageBoxOnly:
                        {
                            break;
                        }
                    case LogOutType.FileOnly:
                        {
                            System.Diagnostics.Trace.AutoFlush = true;
                            System.Diagnostics.Trace.Listeners.Clear();
                            System.Diagnostics.Trace.Listeners.Add(TWTL);
                            break;
                        }
                    case LogOutType.MessageBoxAndFile:
                        {
                            System.Diagnostics.Trace.AutoFlush = true;
                            System.Diagnostics.Trace.Listeners.Clear();
                            System.Diagnostics.Trace.Listeners.Add(TWTL);
                            break;
                        }
                }
            }
        }

        private static string GetLogFullPath
        {
            get
            {
                return string.Concat(logRootDirectory, '\\', string.Concat(logSubDirectory, @"\log", currentLogFileDate.ToString("yyyy-MM-dd"), ".txt"));
            }
        }

        /// <summary>
        /// 跟踪输出日志文件
        /// </summary>
        private static TextWriterTraceListener TWTL
        {
            get
            {
                //if (twtl == null)
                //{
                if (string.IsNullOrEmpty(logSubDirectory))
                {
                    BuiderDir(DateTime.Now);
                }
                else
                {
                    string logPath = GetLogFullPath;
                    if (!Directory.Exists(Path.GetDirectoryName(logPath)))
                    {
                        BuiderDir(DateTime.Now);
                    }
                }

                //MessageBox.Show(GetLogFullPath);
                twtl = new TextWriterTraceListener(GetLogFullPath);
                //}

                return twtl;
            }
        }


        #endregion

        #region 构造

        static Logger()
        {
            switch (logOutType)
            {
                case LogOutType.MessageBoxOnly:
                    {
                        break;
                    }
                case LogOutType.FileOnly:
                    {
                        System.Diagnostics.Trace.AutoFlush = true;
                        System.Diagnostics.Trace.Listeners.Clear();
                        System.Diagnostics.Trace.Listeners.Add(TWTL);
                        break;
                    }
                case LogOutType.MessageBoxAndFile:
                    {
                        System.Diagnostics.Trace.AutoFlush = true;
                        System.Diagnostics.Trace.Listeners.Clear();
                        System.Diagnostics.Trace.Listeners.Add(TWTL);
                        break;
                    }
            }
        }

        #endregion

        #region 方法

        #region trace

        public static void Trace(Exception ex)
        {
            new AsyncLogException(BeginTraceError).BeginInvoke(ex, null, null);
        }

        //public static void Trace(LogInfoType logInfoType,string logInfo,
        //                        [CallerMemberName] string memberName = "",
        //                        [CallerFilePath] string filePath = "",
        //                        [CallerLineNumber] int lineNumber = 0)
        //{
        //    if (logInfoType < DataController.LogOutputLevel)
        //    {
        //        return;
        //    }

        //    logInfo = string.Format("Line->{0}\r\nFunc->{1}\r\nFile->{2}\r\nInfo->{3}\r\n", lineNumber, memberName, Path.GetFileName(filePath), logInfo);           
        //    new AsyncLogString(BeginTraceError).BeginInvoke(logInfoType, logInfo, null, null);
        //}

        public static void Trace(LogInfoType logInfoType,
                                 string logInfo,
                                 string moduleName,
                                 LogCategory cat,
                                [CallerMemberName] string memberName = "",
                                [CallerFilePath] string filePath = "",
                                [CallerLineNumber] int lineNumber = 0)
        {
            if (logInfoType < DataController.LogOutputLevel)
            {
                return;
            }

            if (string.IsNullOrEmpty(logInfo) || string.IsNullOrEmpty(moduleName))
            {
                return;
            }

            string tmp = "";

            if (logInfoType == LogInfoType.INFO)
            {
                tmp = string.Format(LOG_INFO, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff"), ++gLogIndex, Path.GetFileName(filePath), lineNumber);
            }
            else if (logInfoType == LogInfoType.WARN)
            {
                tmp = string.Format(LOG_WARN, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff"), ++gLogIndex, Path.GetFileName(filePath), lineNumber);
            }
            else if (logInfoType == LogInfoType.EROR)
            {
                tmp = string.Format(LOG_EROR, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff"), ++gLogIndex, Path.GetFileName(filePath), lineNumber);
            }
            else
            {
                tmp = string.Format(LOG_INFO, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff"), ++gLogIndex, Path.GetFileName(filePath), lineNumber);
            }

            /*
             * 2018-07-16，区分Log的开始
             */
            if (logInfo.Contains("今天是个好日子"))
            {
                tmp = "\r\n\r\n\r\n\r\n" + tmp;
            }

            if (cat == LogCategory.R)
            {
                tmp += string.Format("【{0},R】{1}", moduleName, logInfo);
            }
            else if (cat == LogCategory.S)
            {
                tmp += string.Format("【{0},S】{1}", moduleName, logInfo);
            }
            else
            {
                tmp += string.Format("【{0},I】{1}", moduleName, logInfo);
            }

            new AsyncLogString(BeginTraceError).BeginInvoke(logInfoType, tmp, null, null);
        }

        #endregion

        #region delegate

        private delegate void AsyncLogException(Exception ex);

        private delegate void AsyncLogString(LogInfoType logInfoType, string logInfo);

        private static int get_info_from_error(string ori,ref string fileName,ref string line)
        {
            int i;
            if (string.IsNullOrEmpty(ori))
            {
                return -1;
            }

            //int i = gApLower.ApInfo.Fullname.LastIndexOf(".");
            //name = gApLower.ApInfo.Fullname.Substring(i + 1);
            //nameFullPath = gApLower.ApInfo.Fullname.Substring(0, i);
            //E:\各种资料\C#-Project\ScannerBackgrdServer\ScannerB
            //ackgrdServer\ScannerBackgrdServer\FrmMainController.cs:line 11836

            fileName = "";
            line = "";

            i = ori.LastIndexOf("\\");
            if (i > 0)
            {
                ori = ori.Substring(i + 1);
            }

            i = ori.LastIndexOf(" ");
            if (i > 0)
            {
                line = ori.Substring(i + 1);
            }

            i = ori.LastIndexOf(":");            
            if (i > 0 )
            {
                fileName = ori.Substring(0, i);
            }

            if (fileName != "" && line != "")
            {
                return 0;
            }
            else
            {
                return -1;
            }

        }

        private static void BeginTraceError(Exception ex)
        {
            string tmp = "";
            string line = "";
            string fileName = "";

            get_info_from_error(ex.StackTrace.Trim(), ref fileName, ref line);

            tmp = string.Format(LOG_EROR, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff"), ++gLogIndex, fileName, line);
            tmp += string.Format("【ERR,I】{0},{1}", ex.GetType().Name, ex.Message);
           

            //outString = string.Format("1 -> {0} {1}\r\n2 -> {2}\r\n3 -> Source:{3}",
            //ex.GetType().Name,ex.Message, ex.StackTrace.Trim(), ex.Source);

            //申请
            gMutexLog.WaitOne();           

            switch (logOutType)
            {
                case LogOutType.MessageBoxOnly:
                    {
                        MessageBox.Show(tmp);
                        break;
                    }
                case LogOutType.FileOnly:
                    {                      
                        if (null != ex)
                        {
                            StrategyLog();                           
                            //System.Diagnostics.Trace.WriteLine(string.Format(LOG_EROR, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff"), gLogIndex));

                            while (null != ex)
                            {
                                //System.Diagnostics.Trace.WriteLine(outString);
                                System.Diagnostics.Trace.WriteLine(tmp);
                                ex = ex.InnerException;
                            }

                            if (fileFlushType == FileFlushType.RightNow)
                            {
                                System.Diagnostics.Trace.Close();
                            }
                        }

                        break;
                    }
                case LogOutType.MessageBoxAndFile:
                    {
                        MessageBox.Show(string.Format(tmp));

                        if (null != ex)
                        {
                            StrategyLog();
                            //System.Diagnostics.Trace.WriteLine(string.Format(LOG_EROR, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff"),gLogIndex));

                            while (null != ex)
                            {
                                System.Diagnostics.Trace.WriteLine(tmp);
                                ex = ex.InnerException;
                            }

                            if (fileFlushType == FileFlushType.RightNow)
                            {
                                System.Diagnostics.Trace.Close();
                            }
                        }

                        break;
                    }
            }

            //释放
            gMutexLog.ReleaseMutex();
        }

        private static void BeginTraceError(LogInfoType logInfoType, string logInfo)
        {
            if (string.IsNullOrEmpty(logInfo))
            {
                MessageBox.Show("参数非法！");
            }

            //申请
            gMutexLog.WaitOne();

            switch (logOutType)
            {
                case LogOutType.MessageBoxOnly:
                    {
                        MessageBox.Show(logInfo);
                        break;
                    }
                case LogOutType.FileOnly:
                    {
                        //检测日志日期
                        StrategyLog();

                        ////输出日志头
                        //if (logInfoType == LogInfoType.INFO)
                        //{
                        //    System.Diagnostics.Trace.WriteLine(string.Format(LOG_INFO, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff"), gLogIndex));
                        //}
                        //else if (logInfoType == LogInfoType.WARN)
                        //{
                        //    System.Diagnostics.Trace.WriteLine(string.Format(LOG_WARN, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff"), gLogIndex));
                        //}
                        //else if (logInfoType == LogInfoType.EROR)
                        //{
                        //    System.Diagnostics.Trace.WriteLine(string.Format(LOG_EROR, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff"), gLogIndex));
                        //}
                        //else
                        //{
                        //    System.Diagnostics.Trace.WriteLine(string.Format(LOG_DEBG, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff"), gLogIndex));
                        //}
                        
                        System.Diagnostics.Trace.WriteLine(logInfo);

                        if (fileFlushType == FileFlushType.RightNow)
                        {
                            System.Diagnostics.Trace.Close();
                        }

                        break;
                    }
                case LogOutType.MessageBoxAndFile:
                    {
                        MessageBox.Show(logInfo);

                        //检测日志日期
                        StrategyLog();

                        ////输出日志头
                        //if (logInfoType == LogInfoType.INFO)
                        //{
                        //    System.Diagnostics.Trace.WriteLine(string.Format(LOG_INFO, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff"), gLogIndex));
                        //}
                        //else if (logInfoType == LogInfoType.WARN)
                        //{
                        //    System.Diagnostics.Trace.WriteLine(string.Format(LOG_WARN, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff"), gLogIndex));
                        //}
                        //else if (logInfoType == LogInfoType.EROR)
                        //{
                        //    System.Diagnostics.Trace.WriteLine(string.Format(LOG_EROR, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff"), gLogIndex));
                        //}
                        //else
                        //{
                        //    System.Diagnostics.Trace.WriteLine(string.Format(LOG_DEBG, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff"), gLogIndex));
                        //}

                        System.Diagnostics.Trace.WriteLine(logInfo);

                        if (fileFlushType == FileFlushType.RightNow)
                        {
                            System.Diagnostics.Trace.Close();
                        }

                        break;
                    }
            }

            //释放
            gMutexLog.ReleaseMutex();
        }


        #endregion

        #region helper

        private static void StrategyLog()
        {
            //判断日志日期
            if (DateTime.Compare(DateTime.Now.Date, currentLogFileDate.Date) != 0)
            {
                DateTime currentDate = DateTime.Now.Date;

                //生成子目录
                BuiderDir(currentDate);

                //更新当前日志日期
                currentLogFileDate = currentDate;

                System.Diagnostics.Trace.Flush();

                //更改输出
                if (twtl != null)
                {
                    System.Diagnostics.Trace.Listeners.Remove(twtl);
                }

                System.Diagnostics.Trace.Listeners.Add(TWTL);
            }
        }

        private static void BuiderDir(DateTime currentDate)
        {
            int year = currentDate.Year;
            int month = currentDate.Month;

            //年/月
            string subdir = string.Concat(string.Format("{0:D4}年", year), '\\', string.Format("{0:D2}月", month));
            string path = Path.Combine(logRootDirectory, subdir);

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            logSubDirectory = subdir;
        }

        #endregion

        #endregion
    }
}
