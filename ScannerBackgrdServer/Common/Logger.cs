using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ScannerBackgrdServer.Common
{
    //日志信息类型
    public enum LogInfoType       
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

    public enum LogOutType       //日志输出类型
    {
        OT_File = 0,    //仅输出到文件
        OT_Net = 1,     //仅输出到网络
        OT_Both = 2,    //输出到文件和网络
    }

    public class Logger
    {
        #region 定义

        private const string LOG_DEBG  = "\r\n【DEBG】[{0}][{1}] {2}({3})\r\n";
        private const string LOG_INFO  = "\r\n【INFO】[{0}][{1}] {2}({3})\r\n";
        private const string LOG_WARN  = "\r\n【WARN】[{0}][{1}] {2}({3})\r\n";
        private const string LOG_EROR  = "\r\n【EROR】[{0}][{1}] {2}({3})\r\n";
       
        private static TextWriterTraceListener twTraceListener;

        private static DateTime currentLogFileDate = DateTime.Now;

        private static string logSubDirectory;

        private static Object mutex_Logger = new Object();
        private static Queue<string> gQueueLogger = new Queue<string>();

        private static long gLogIndex = 0;

        private static long gLines = 0;
        private static long logLinesPerFils = 50000;

        private static string logRootDirectory = Application.StartupPath + @"\strLogInfo";        

        private enum FileFlushType    //日志文件刷新类型
        {
            Standard = 0,             //标准刷新
            RightNow = 1,             //立即刷新
        }

        /// <summary>
        /// 配置记录输出类型
        /// </summary>
        private static  LogOutType logOutType = LogOutType.OT_File;

        /// <summary>
        /// 配置记录文件的刷新类型
        /// </summary>
        private static readonly FileFlushType fileFlushType = FileFlushType.RightNow;

        private static UdpClient udpSender = new UdpClient(0);

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
                    case LogOutType.OT_File:
                        {
                            System.Diagnostics.Trace.AutoFlush = true;
                            System.Diagnostics.Trace.Listeners.Clear();
                            System.Diagnostics.Trace.Listeners.Add(TWTraceListener);
                            break;
                        }
                    case LogOutType.OT_Net:
                        {                            
                            break;
                        }
                    case LogOutType.OT_Both:
                        {
                            System.Diagnostics.Trace.AutoFlush = true;
                            System.Diagnostics.Trace.Listeners.Clear();
                            System.Diagnostics.Trace.Listeners.Add(TWTraceListener);
                            break;
                        }
                }
            }
        }


        /// <summary>
        /// 获取Log的全路径
        /// </summary>
        private static string GetLogFullPath
        {
            get
            {
                string tmp = string.Concat(logRootDirectory, '\\', string.Concat(logSubDirectory, @"\log", currentLogFileDate.ToString("yyyy-MM-dd")));
                tmp = string.Format("{0}-{1}.txt",tmp,gLines);

                return tmp;
            }
        }

        /// <summary>
        /// 跟踪输出日志文件
        /// </summary>
        private static TextWriterTraceListener TWTraceListener
        {
            get
            {         
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
        
                twTraceListener = new TextWriterTraceListener(GetLogFullPath);              
                return twTraceListener;
            }
        }


        #endregion

        #region 构造

        static Logger()
        {
            logLinesPerFils = DataController.LogLinesPerFils;
            logOutType = DataController.LogOutputType;

            try
            {
                switch (logOutType)
                {
                    case LogOutType.OT_File:
                        {
                            System.Diagnostics.Trace.AutoFlush = true;
                            System.Diagnostics.Trace.Listeners.Clear();
                            System.Diagnostics.Trace.Listeners.Add(TWTraceListener);
                            break;
                        }
                    case LogOutType.OT_Net:
                        {
                            udpSender.Connect(DataController.StrLogIpAddr, int.Parse(DataController.StrLogPort));
                            break;
                        }
                    case LogOutType.OT_Both:
                        {
                            udpSender.Connect(DataController.StrLogIpAddr, int.Parse(DataController.StrLogPort));
                            System.Diagnostics.Trace.AutoFlush = true;
                            System.Diagnostics.Trace.Listeners.Clear();
                            System.Diagnostics.Trace.Listeners.Add(TWTraceListener);
                            break;
                        }
                }
            }
            catch (Exception ee)
            {
                Logger.Trace(LogInfoType.EROR, ee.Message, "Logger", LogCategory.I);
                MessageBox.Show(ee.Message, "Logger出错", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
            }
        }

        #endregion

        #region 方法

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

            lock (mutex_Logger)
            {
                gQueueLogger.Enqueue(tmp);
            }

            //new AsyncLogString(BeginTraceError).BeginInvoke(logInfoType, tmp, null, null);
        }


        /// <summary>
        /// 用于处理Logger的线程
        /// </summary>
        /// <param name="obj"></param>
        private static void thread_for_logger(object obj)
        {                  
            bool noMsg = false;
            string logInfo = "";
            
            try
            {
                while (true)
                {
                    #region Sleep处理

                    if (noMsg)
                    {
                        //没消息时Sleep一大点
                        Thread.Sleep(100);
                    }
                    else
                    {
                        //有消息时Sleep一小点
                        Thread.Sleep(2);
                    }

                    #endregion

                    #region 保存Logger

                    lock (mutex_Logger)
                    {
                        if (gQueueLogger.Count <= 0)
                        {
                            noMsg = true;
                            continue;
                        }

                        logInfo = gQueueLogger.Dequeue();
                    }

                    if (string.IsNullOrEmpty(logInfo))
                    {   
                        continue;
                    }

                    noMsg = false;
                    switch (logOutType)
                    {
                        case LogOutType.OT_File:
                            {
                                gLines++;

                                //检测日志日期
                                StrategyLog();

                                System.Diagnostics.Trace.WriteLine(logInfo);
                                if (fileFlushType == FileFlushType.RightNow)
                                {
                                    System.Diagnostics.Trace.Close();
                                }
                                
                                break;
                            }
                        case LogOutType.OT_Net:
                            {
                                byte[] sendBytes = Encoding.Default.GetBytes(logInfo);
                                udpSender.Send(sendBytes, sendBytes.Length);
                                break;
                            }
                        case LogOutType.OT_Both:
                            {                               
                                //检测日志日期
                                StrategyLog();
                                System.Diagnostics.Trace.WriteLine(logInfo);

                                if (fileFlushType == FileFlushType.RightNow)
                                {
                                    System.Diagnostics.Trace.Close();
                                }

                                byte[] sendBytes = Encoding.Default.GetBytes(logInfo);
                                udpSender.Send(sendBytes, sendBytes.Length);

                                break;
                            }
                    }                   

                    #endregion
                }
            }
            catch (Exception ee)
            {                
                Logger.Trace(LogInfoType.EROR, ee.Message, "Logger", LogCategory.I);
            }
        }

        private delegate void AsyncLogString(LogInfoType logInfoType, string logInfo);

        private static void BeginTraceError(LogInfoType logInfoType, string logInfo)
        {
            if (string.IsNullOrEmpty(logInfo))
            {
                MessageBox.Show("参数非法！");
            }

            lock (mutex_Logger)
            {

                switch (logOutType)
                {
                    case LogOutType.OT_File:
                        {
                            //检测日志日期
                            StrategyLog();
                            System.Diagnostics.Trace.WriteLine(logInfo);

                            if (fileFlushType == FileFlushType.RightNow)
                            {
                                System.Diagnostics.Trace.Close();
                            }

                            break;
                        }
                    case LogOutType.OT_Net:
                        {                            
                            break;
                        }
                    case LogOutType.OT_Both:
                        {                            
                            //检测日志日期
                            StrategyLog();
                            System.Diagnostics.Trace.WriteLine(logInfo);

                            if (fileFlushType == FileFlushType.RightNow)
                            {
                                System.Diagnostics.Trace.Close();
                            }

                            break;
                        }
                }

            }
        }

        private static void StrategyLog()
        {
            if (((gLines % logLinesPerFils) == 0) || (DateTime.Compare(DateTime.Now.Date, currentLogFileDate.Date) != 0))
            {
                DateTime currentDate = DateTime.Now.Date;

                //生成子目录
                BuiderDir(currentDate);

                //更新当前日志日期
                currentLogFileDate = currentDate;
                System.Diagnostics.Trace.Flush();

                //更改输出
                if (twTraceListener != null)
                {
                    System.Diagnostics.Trace.Listeners.Remove(twTraceListener);
                }

                System.Diagnostics.Trace.Listeners.Add(TWTraceListener);
            }

            ////判断日志日期
            //if (DateTime.Compare(DateTime.Now.Date, currentLogFileDate.Date) != 0)
            //{
            //    DateTime currentDate = DateTime.Now.Date;

            //    //生成子目录
            //    BuiderDir(currentDate);

            //    //更新当前日志日期
            //    currentLogFileDate = currentDate;
            //    System.Diagnostics.Trace.Flush();

            //    //更改输出
            //    if (twTraceListener != null)
            //    {
            //        System.Diagnostics.Trace.Listeners.Remove(twTraceListener);
            //    }

            //    System.Diagnostics.Trace.Listeners.Add(TWTraceListener);
            //}
        }

        /// <summary>
        /// 创建路径
        /// </summary>
        /// <param name="currentDate"></param>
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

            //2018年\07月
            logSubDirectory = subdir;
        }

        ///// <summary>
        ///// 压缩文件
        ///// </summary>
        ///// <param name="fileNames">要打包的文件列表</param>
        ///// <param name="GzipFileName">目标文件名称</param>
        ///// <param name="CompressionLevel">压缩品质级别（0~9）</param>   
        //public static int Compress(FileInfo fileNames, string GzipFileName, int CompressionLevel)
        //{
        //    if (!File.Exists(fileNames.FullName))
        //    {
        //        return -1;
        //    }

        //    ZipOutputStream s = new ZipOutputStream(File.Create(GzipFileName));

        //    try
        //    {
        //        /*
        //         * 0 - store only 
        //         * 9 - means best compression
        //         */
        //        s.SetLevel(CompressionLevel);
        //        FileStream fs = fileNames.Open(FileMode.Open, FileAccess.ReadWrite);

        //        // 将文件分批读入缓冲区
        //        byte[] data = new byte[2048];
        //        int size = 2048;

        //        ZipEntry entry = new ZipEntry(Path.GetFileName(fileNames.Name));
        //        entry.DateTime = (fileNames.CreationTime > fileNames.LastWriteTime ? fileNames.LastWriteTime : fileNames.CreationTime);

        //        s.PutNextEntry(entry);

        //        while (true)
        //        {
        //            size = fs.Read(data, 0, size);

        //            if (size <= 0)
        //            {
        //                break;
        //            }

        //            s.Write(data, 0, size);
        //        }

        //        fs.Close();

        //    }
        //    catch (Exception ee)
        //    {
        //        Logger.Trace(LogInfoType.EROR,ee.Message,"Logger",LogCategory.I);
        //        return -1;
        //    }
        //    finally
        //    {
        //        s.Finish();
        //        s.Close();
        //    }

        //    return 0;
        //}

        public static void Start()
        {          
            //通过ParameterizedThreadStart创建线程
            Thread threadLogger = new Thread(new ParameterizedThreadStart(thread_for_logger));

            threadLogger.Priority = ThreadPriority.Lowest;

            //给方法传值
            threadLogger.Start("thread_for_ftp_helper!\n");
            threadLogger.IsBackground = true;           
        }

        #endregion       
    }
}
