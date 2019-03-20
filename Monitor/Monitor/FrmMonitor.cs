using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Monitor
{
    public partial class FrmMonitor : Form
    {
        #region 声明和定义

        /// <summary>
        /// 各个线程的报告信息
        /// </summary>
        public struct strReportInfo
        {
            public long curValue;
            public long totalCnt;
            public List<string> lstThrName;
            public List<long> lstThrStatus;
        };

        private const int INVALID_HANDLE_VALUE = -1;
        private const int PAGE_READWRITE = 0x04;

        /// <summary>
        /// 错误数达到多少就重启
        /// </summary>
        private const int RebootAtErrCnt = 3;

        /// <summary>
        /// 多少分钟失联就重启
        /// </summary>
        private const int RebootAtDisconn = 5;

        [DllImport("User32.dll")]
        private static extern bool ShowWindowAsync(IntPtr hWnd, int cmdShow);

        [DllImport("User32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        //共享内存
        [DllImport("Kernel32.dll", EntryPoint = "CreateFileMapping")]
        private static extern IntPtr CreateFileMapping(IntPtr hFile, //HANDLE hFile,
         UInt32 lpAttributes,//LPSECURITY_ATTRIBUTES lpAttributes,  //0
         UInt32 flProtect,//DWORD flProtect
         UInt32 dwMaximumSizeHigh,//DWORD dwMaximumSizeHigh,
         UInt32 dwMaximumSizeLow,//DWORD dwMaximumSizeLow,
         string lpName//LPCTSTR lpName
         );

        [DllImport("Kernel32.dll", EntryPoint = "OpenFileMapping")]
        private static extern IntPtr OpenFileMapping(
         UInt32 dwDesiredAccess,//DWORD dwDesiredAccess,
         int bInheritHandle,//BOOL bInheritHandle,
         string lpName//LPCTSTR lpName
         );

        private const int FILE_MAP_ALL_ACCESS = 0x0002;
        private const int FILE_MAP_WRITE = 0x0002;

        [DllImport("Kernel32.dll", EntryPoint = "MapViewOfFile")]
        private static extern IntPtr MapViewOfFile(
         IntPtr hFileMappingObject,//HANDLE hFileMappingObject,
         UInt32 dwDesiredAccess,//DWORD dwDesiredAccess
         UInt32 dwFileOffsetHight,//DWORD dwFileOffsetHigh,
         UInt32 dwFileOffsetLow,//DWORD dwFileOffsetLow,
         UInt32 dwNumberOfBytesToMap//SIZE_T dwNumberOfBytesToMap
         );

        [DllImport("Kernel32.dll", EntryPoint = "UnmapViewOfFile")]
        private static extern int UnmapViewOfFile(IntPtr lpBaseAddress);

        [DllImport("Kernel32.dll", EntryPoint = "CloseHandle")]
        private static extern int CloseHandle(IntPtr hObject);

        private Semaphore m_Write;      //可写的信号
        private Semaphore m_Read;       //可读的信号
        private IntPtr handle;          //文件句柄
        private IntPtr addr;            //共享内存地址
        private uint mapLength = 1024;  //共享内存长

        //Scanner服务器重启的次数
        private int scannerServerRebootCnt = 0;

        //当前报告状态出错的次数
        private int statusErrorCnt = 0;

        //当前报告状态的总次数
        private int statusTotalCnt = 0;

        //声明委托类型
        private delegate void InfoDelegate(string info);

        //声明委托类型
        private delegate void UpdateDelegate();

        private static strReportInfo gReportInfo = new strReportInfo();


        private static string ServerDirectory = Application.StartupPath;

        //private static string ServerDirectory = @"E:\各种资料\C#-Project\ScannerBackgrdServer\ScannerBackgrdServer\ScannerBackgrdServer\bin\Release";
        private static string ServerName = "ScannerBackgrdServer";

        private static DateTime startTimeConn = System.DateTime.Now;
        private static DateTime endTimeConn = System.DateTime.Now;
        private static TimeSpan tsConn = endTimeConn.Subtract(startTimeConn);

        private static CheckSocket gCheckSocket;
        private static DateTime startTimeConnPC = System.DateTime.Now;
        private static DateTime endTimeConnPC = System.DateTime.Now;
        private static TimeSpan tsConnPC = endTimeConnPC.Subtract(startTimeConnPC);

        #endregion

        /// <summary>
        /// 初始化共享内存数据
        /// </summary>
        private void init()
        {
            m_Write = new Semaphore(1, 1, "WriteMap"); //开始的时候有一个可以写
            m_Read = new Semaphore(0, 1, "ReadMap");   //没有数据可读

            mapLength = 1024;
            IntPtr hFile = new IntPtr(INVALID_HANDLE_VALUE);
            handle = CreateFileMapping(hFile, 0, PAGE_READWRITE, 0, mapLength, "shareMemory");
            addr = MapViewOfFile(handle, FILE_MAP_ALL_ACCESS, 0, 0, 0);

            //handle = OpenFileMapping(0x0002, 0, "shareMemory");
            //addr = MapViewOfFile(handle, FILE_MAP_ALL_ACCESS, 0, 0, 0);
        }

        public FrmMonitor()
        {
            InitializeComponent();
            init();
        }

        private void FrmMonitor_Load(object sender, EventArgs e)
        {
            #region 启动用于接收Server消息的线程

            //通过ParameterizedThreadStart创建线程
            Thread threadRecvMsg = new Thread(new ParameterizedThreadStart(threadRecvMsg_fun));

            //给方法传值
            threadRecvMsg.Start("threadRecvMsg_fun!\n");
            threadRecvMsg.IsBackground = true;

            #endregion

            #region 启动用于处理逻辑的线程

            //通过ParameterizedThreadStart创建线程
            Thread threadLogicProcess = new Thread(new ParameterizedThreadStart(threadLogicProcess_fun));

            //给方法传值
            threadLogicProcess.Start("threadLogicProcess_fun!\n");
            threadLogicProcess.IsBackground = true;

            #endregion

            #region 初始化Log

            Logger.Start();
            Logger.Trace(LogInfoType.INFO, "今天是个好日子!", "Main", LogCategory.I);

            #endregion

            #region 窗体初始化

            label1LastUpTime.Text = string.Format("{0}/{1}", DateTime.Now.ToShortTimeString(),statusTotalCnt);
            labelErrRebootCnt.Text = string.Format("{0}/{1}", statusErrorCnt,scannerServerRebootCnt);            

            //不显示出dataGridView1的最后一行空白   
            dataGridViewStatus.AllowUserToAddRows = false;
            dataGridViewStatus.AllowUserToResizeColumns = false;

            #endregion

            #region 启动服务器

            if (!process_is_exit(ServerName))
            {
                string fullPath = string.Format("{0}\\{1}.exe", ServerDirectory, ServerName);
                if (File.Exists(fullPath))
                {
                    //存在文件 
                    open_process(fullPath);
                }
                else
                {
                    //不存在文件    
                    string info = string.Format("{0},文件不存在，请确认！", fullPath);
                    MessageBox.Show(info, this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                }                
            }
            else
            {
                MessageBox.Show(ServerName+"已经在运行！", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            #endregion

            int[] devicePortList = new int[5];
            devicePortList[0] = 14783;
            devicePortList[1] = 14784;
            devicePortList[2] = 14785;
            devicePortList[3] = 14786;
            devicePortList[4] = 14788;

            int[] windowsPortList = new int[1];
            windowsPortList[0] = 14789;

            gCheckSocket = new CheckSocket(devicePortList, windowsPortList);
        }

        private void add_row_2_dgv(string name,long curValue,long stsValue, ref bool failFlag)
        {
            string errInfo = "";
            int index = this.dataGridViewStatus.Rows.Add();
            this.dataGridViewStatus.Rows[index].Cells[0].Value = name;                        

            if (curValue == stsValue)
            {
                dataGridViewStatus.Rows[index].Cells[1].Value = "OK";
                dataGridViewStatus[1, index].Style.BackColor = Color.LimeGreen;
            }
            else
            {
                dataGridViewStatus.Rows[index].Cells[1].Value = "NO";
                dataGridViewStatus[1, index].Style.BackColor = Color.Red;
                failFlag = true;

                errInfo = string.Format("name:{0} curValue:{1} stsValue{2},出错！", name, curValue, stsValue);
                Logger.Trace(LogInfoType.EROR, errInfo, "Main", LogCategory.I);
            }
        }

        private void modify_row_2_dgv(string name, long curValue, long stsValue,ref bool failFlag)
        {
            string errInfo = "";
            int index = get_row_number(name);

            if (index == -1)
            {
                return;
            }            

            if (curValue == stsValue)
            {
                dataGridViewStatus.Rows[index].Cells[1].Value = "OK";
                dataGridViewStatus[1, index].Style.BackColor = Color.LimeGreen;
            }
            else
            {
                dataGridViewStatus.Rows[index].Cells[1].Value = "NO";
                dataGridViewStatus[1, index].Style.BackColor = Color.Red;
                failFlag = true;

                errInfo = string.Format("name:{0} curValue:{1} stsValue{2},出错！", name, curValue, stsValue);
                Logger.Trace(LogInfoType.EROR, errInfo, "Main", LogCategory.I);
            }
        }

        private int get_row_number(string name)
        {
            string tmp = "";

            if (string.IsNullOrEmpty(name))
            {
                return -1;
            }

            for (int i = 0; i < dataGridViewStatus.RowCount; i++)
            {                
                tmp = dataGridViewStatus.Rows[i].Cells[0].Value.ToString();
                if (tmp == name)
                {
                    return i;
                }
            }

            return -1;
        }        
        
        private void FrmMonitor_FormClosing(object sender, FormClosingEventArgs e)
        {
            DialogResult dr = MessageBox.Show("    是否退出Server监控器?", "提示:", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);

            if (dr == DialogResult.OK)   //如果单击"是"按钮
            {
                //关闭窗体
                e.Cancel = false;
                //System.Environment.Exit(0);
            }
            else if (dr == DialogResult.Cancel)
            {
                //不执行操作
                e.Cancel = true;
            }
        }

        /// <summary>
        /// 检查ID集合的合法性，返回分离后的list
        /// </summary>
        /// <param name="idSet">集合以逗号分隔，如下1;2;3;4;5
        /// [0] = 当前的报告值
        /// [1] = 报告的线程总个数n
        /// [2] = 第1个线程的名称和报告的值，如下  AppCtrl_Status:4
        /// [3] = 第1个线程的名称和报告的值，如下  Ftp_Status:4
        /// .......
        /// </param>
        /// <param name=")">返回的ri</param>
        /// <returns>
        /// true  ： 合法
        /// false ： 非法
        /// </returns>
        public bool check_and_get_id_set(string idSet, ref strReportInfo ri)
        {
            long tmp;
            strReportInfo str = new strReportInfo();

            if (string.IsNullOrEmpty(idSet))
            {
                Logger.Trace(LogInfoType.EROR, "idSet参数为空", "Main", LogCategory.I);
                return false;
            }

            if (idSet.Length > 1024)
            {
                Logger.Trace(LogInfoType.EROR, "idSet参数长度有误", "Main", LogCategory.I);
                return false;
            }

            try
            {
                string[] s = idSet.Split(new char[] { ';' });

                if (s.Length <= 0)
                {
                    Logger.Trace(LogInfoType.EROR, "s.Length <= 0", "Main", LogCategory.I);
                    return false;
                }
                else
                {
                    try
                    {
                        tmp = long.Parse(s[0]);
                        str.curValue = tmp;
                    }
                    catch (Exception ee)
                    {
                        Logger.Trace(LogInfoType.EROR, ee.Message + ee.StackTrace, "Main", LogCategory.I);
                        return false;
                    }

                    try
                    {
                        tmp = long.Parse(s[1]);
                        str.totalCnt = tmp;
                    }
                    catch (Exception ee)
                    {
                        Logger.Trace(LogInfoType.EROR, ee.Message + ee.StackTrace, "Main", LogCategory.I);
                        return false;
                    }

                    int inx = 0;
                    str.lstThrName = new List<string>();
                    str.lstThrStatus = new List<long>();

                    for (int i = 2; i < s.Length; i++)
                    {
                        inx = s[i].IndexOf(":");
                        if (inx <= 0)
                        {
                            return false;
                        }

                        str.lstThrName.Add(s[i].Substring(0, inx));

                        try
                        {
                            tmp = long.Parse(s[i].Substring(inx + 1));
                            str.lstThrStatus.Add(tmp);
                        }
                        catch (Exception ee)
                        {
                            Logger.Trace(LogInfoType.EROR, ee.Message + ee.StackTrace, "Main", LogCategory.I);
                            return false;
                        }
                    }
                }

                if (str.lstThrStatus.Count > 0)
                {
                    ri = new strReportInfo();
                    ri = str;
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ee)
            {
                startTimeConn = System.DateTime.Now;
                Logger.Trace(LogInfoType.EROR, ee.Message + ee.StackTrace, "Main", LogCategory.I);
                return false;
            }
        }

        /// <summary>
        /// 检查ID集合的合法性，返回分离后的list
        /// </summary>
        /// <param name="idSet">ApPortList:{0}:{1}:{2}:{3}:{4};UiPortList:{5}
        /// </param>
        /// <param name=")">返回的ri</param>
        /// <returns>
        /// true  ： 合法
        /// false ： 非法
        /// </returns>
        public bool check_and_get_id_set(string idSet, ref List<int> ApPortList,ref List<int> UiPortList)
        {            
            if (string.IsNullOrEmpty(idSet))
            {
                Logger.Trace(LogInfoType.EROR, "idSet参数为空", "Main", LogCategory.I);
                return false;
            }

            if (idSet.Length > 1024)
            {
                Logger.Trace(LogInfoType.EROR, "idSet参数长度有误", "Main", LogCategory.I);
                return false;
            }

            try
            {
                string[] s = idSet.Split(new char[] { ';' });

                if (s.Length <= 0)
                {
                    Logger.Trace(LogInfoType.EROR, "s.Length <= 0", "Main", LogCategory.I);
                    return false;
                }
                else if (s.Length != 2)
                {
                    Logger.Trace(LogInfoType.EROR, "s.Length != 2", "Main", LogCategory.I);
                    return false;
                }
                else
                {                 
                    string[] t = s[0].Split(new char[] { ':' });
                    if (t.Length > 1)
                    {                        
                        for (int i = 1; i < t.Length; i++)
                        {
                            try
                            {
                                int tmp = int.Parse(t[i]);
                                ApPortList.Add(tmp);
                            }
                            catch (Exception ee)
                            {
                                Logger.Trace(LogInfoType.EROR, ee.Message + ee.StackTrace, "Main", LogCategory.I);
                                return false;
                            }
                        }                       
                    }
                    
                    t = s[1].Split(new char[] { ':' });
                    if (t.Length > 1)
                    {
                        for (int i = 1; i < t.Length; i++)
                        {
                            try
                            {
                                int tmp = int.Parse(t[i]);
                                UiPortList.Add(tmp);
                            }
                            catch (Exception ee)
                            {
                                Logger.Trace(LogInfoType.EROR, ee.Message + ee.StackTrace, "Main", LogCategory.I);
                                return false;
                            }
                        }                      
                    }

                }

                if (ApPortList.Count > 0 && UiPortList.Count > 0)
                {                   
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ee)
            {
                startTimeConn = System.DateTime.Now;
                Logger.Trace(LogInfoType.EROR, ee.Message + ee.StackTrace, "Main", LogCategory.I);
                return false;
            }
        }

        /// <summary>
        /// 判断进程是否已经存在
        /// </summary>
        /// <param name="processName"></param>
        /// <returns></returns>
        private bool process_is_exit(string processName)
        {
            if (string.IsNullOrEmpty(processName))
            {
                return false;
            }

            System.Diagnostics.Process[] myProcesses = System.Diagnostics.Process.GetProcesses();
            foreach (System.Diagnostics.Process myProcess in myProcesses)
            {
                if (processName == myProcess.ProcessName)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 打开进程
        /// </summary>
        /// <param name="processName"></param>
        /// <returns></returns>
        private bool open_process(string processName)
        {
            //if (string.IsNullOrEmpty(processName))
            //{
            //    return false;
            //}

            //System.Diagnostics.Process[] myProcesses = System.Diagnostics.Process.GetProcesses();
            //foreach (System.Diagnostics.Process myProcess in myProcesses)
            //{
            //    if (processName == myProcess.ProcessName)
            //    {
            //        return true;
            //    }
            //}
            //
            //return false;

            try
            {
                System.Diagnostics.Process proc = System.Diagnostics.Process.Start(processName);
                if (proc == null)
                {
                    //proc.WaitForExit(3000);
                    //if (proc.HasExited)
                    //{
                    //    MessageBox.Show(String.Format("外部程序 {0} 已经退出！", processName), this.Text,MessageBoxButtons.OK, MessageBoxIcon.Information);
                    //}
                    //else
                    //{
                    //    // 如果外部程序没有结束运行则强行终止之。
                    //    proc.Kill();
                    //    MessageBox.Show(String.Format("外部程序 {0} 被强行终止！", processName), this.Text, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    //}

                    return false;
                }
            }
            catch (ArgumentException ex)
            {
                MessageBox.Show(ex.Message, this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return true;
        }

        /// <summary>
        /// 关闭进程
        /// </summary>
        /// <param name="processName"></param>
        /// <returns></returns>
        private bool close_process(string processName)
        {
            bool flag = false;
            System.Diagnostics.Process[] myProcesses = System.Diagnostics.Process.GetProcesses();

            foreach (System.Diagnostics.Process myProcess in myProcesses)
            {
                if (processName == myProcess.ProcessName)
                {
                    myProcess.Kill();//强制关闭该程序
                    flag = true;
                    break;
                }
            }

            if (flag == false)
            {
                string info = string.Format("找不到程序:{0}", processName);
                Logger.Trace(LogInfoType.EROR, info, "Main", LogCategory.I);
            }

            return flag;
        }

        /// <summary>
        /// 不安全的代码在项目生成的选项中选中允许不安全代码
        /// </summary>
        /// <param name="dst"></param>
        /// <param name="src"></param>
        private static unsafe void byteCopy(byte[] dst, IntPtr src,ref int rtvLen)
        {
            rtvLen = 0;
            fixed (byte* pDst = dst)
            {
                byte* pdst = pDst;
                byte* psrc = (byte*)src;

                while ((*pdst++ = *psrc++) != '\0')
                {
                    rtvLen++;
                }                
            }
        }

        private void FrmMonitor_SizeChanged(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                this.Hide();                //隐藏窗体
                notifyIcon1.Visible = true; //使托盘图标可见
            }
        }     

        private void notifyIcon1_DoubleClick(object sender, EventArgs e)
        {
            this.Visible = true;
            this.WindowState = FormWindowState.Normal;
        }

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
                r = string.Format("{0:D2}:{1:D2}:{2:D2}", hour, minute, second);
            }

            return r;
        }

        private long runTimeCnt = 0;
        //private long runTimeCnt = 86400 - 10;
        private void timerRunTime_Tick(object sender, EventArgs e)
        {
            runTimeCnt++;    
            label1RunTime.Text = parseTimeSeconds(runTimeCnt);

            //if (runTimeCnt % 5 == 0)
            //{
            //    textBox2.BackColor = Color.Red;

            //    textBox3.BackColor = Color.LimeGreen;
            //}
        }

        private void dataGridViewStatus_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            //if (e.RowIndex < 0)
            //{
            //    return;
            //}

            //if (e.RowIndex >= this.dataGridViewStatus.RowCount - 1)
            //{
            //    //this.dataGridViewStatus.Rows[e.RowIndex].Visible = e.RowIndex == 0 ? true : false;
            //    return;
            //}
        }

        private void button1_Click(object sender, EventArgs e)
        {
            int a = 0;
            int b = 10 / a;
        }

        //为DataGridView 添加行号列
        public static void ShowRows_DataGridView_RowPostPaint(DataGridView dgv, object sender, DataGridViewRowPostPaintEventArgs e)
        {
            Rectangle rectangle = new Rectangle(e.RowBounds.Location.X, e.RowBounds.Location.Y, dgv.RowHeadersWidth - 4, e.RowBounds.Height);
            TextRenderer.DrawText(e.Graphics, (e.RowIndex + 1).ToString(), dgv.RowHeadersDefaultCellStyle.Font, rectangle, dgv.RowHeadersDefaultCellStyle.ForeColor, TextFormatFlags.VerticalCenter | TextFormatFlags.Right);
        }

        private void dataGridViewStatus_RowPostPaint(object sender, DataGridViewRowPostPaintEventArgs e)
        {
            ShowRows_DataGridView_RowPostPaint(this.dataGridViewStatus, sender, e);
        }

        #region 消息和逻辑处理线程

        private void UpdateDelegate_Func()
        {            
            label1LastUpTime.Text = string.Format("{0}/{1}", DateTime.Now.ToShortTimeString(), statusTotalCnt);
            labelErrRebootCnt.Text = string.Format("{0}/{1}", statusErrorCnt, scannerServerRebootCnt);
        }

        private void InfoDelegate_Func(string info)
        {
            try
            {
                if (info.Equals("RESTART_ME_RIGHTNOW"))
                {
                    #region 紧急事件处理

                    #region 关闭并重启Server

                    string logInfo = "";
                    logInfo = string.Format("紧急事件处理->InfoDelegate_Func：关闭并重启Server,{0}/{1}", statusErrorCnt, scannerServerRebootCnt);
                    Logger.Trace(LogInfoType.EROR, logInfo, "Main", LogCategory.I);

                    if (process_is_exit(ServerName))
                    {
                        // 2018-11-27,防止Server还没完全退出
                        Thread.Sleep(1234);

                        if (close_process(ServerName))
                        {
                            logInfo = string.Format("{0},进程关闭成功!", ServerName);
                            Logger.Trace(LogInfoType.EROR, logInfo, "Main", LogCategory.I);
                        }
                        else
                        {
                            logInfo = string.Format("{0},进程关闭失败,再次关闭！", ServerName);
                            Logger.Trace(LogInfoType.EROR, logInfo, "Main", LogCategory.I);
                            close_process(ServerName);
                        }

                        string fullPath = string.Format("{0}\\{1}.exe", ServerDirectory, ServerName);
                        if (File.Exists(fullPath))
                        {
                            logInfo = string.Format("{0},重启Server!", fullPath);
                            Logger.Trace(LogInfoType.EROR, logInfo, "Main", LogCategory.I);

                            //存在文件 
                            open_process(fullPath);
                            scannerServerRebootCnt++;
                        }
                        else
                        {
                            //不存在文件    
                            logInfo = string.Format("{0},文件不存在，请确认！", fullPath);
                            Logger.Trace(LogInfoType.EROR, logInfo, "Main", LogCategory.I);
                        }
                    }
                    else
                    {
                        logInfo = string.Format("{0},进程不存在!", ServerName);
                        Logger.Trace(LogInfoType.EROR, logInfo, "Main", LogCategory.I);

                        string fullPath = string.Format("{0}\\{1}.exe", ServerDirectory, ServerName);
                        if (File.Exists(fullPath))
                        {
                            logInfo = string.Format("{0},重启Server!", fullPath);
                            Logger.Trace(LogInfoType.EROR, logInfo, "Main", LogCategory.I);

                            //存在文件 
                            open_process(fullPath);
                            scannerServerRebootCnt++;
                        }
                        else
                        {
                            //不存在文件    
                            logInfo = string.Format("{0},文件不存在，请确认！", fullPath);
                            Logger.Trace(LogInfoType.EROR, logInfo, "Main", LogCategory.I);
                        }
                    }

                    #endregion

                    #endregion
                }
                else if (info.Contains("ApPortList") && info.Contains("UiPortList"))
                {
                    #region 传递各个端口列表

                    string logInfo = "";
                    List<int> ApPortList = new List<int>();
                    List<int> UiPortList = new List<int>();
                    if (false == check_and_get_id_set(info, ref ApPortList,ref UiPortList))
                    {
                        Logger.Trace(LogInfoType.EROR, "传递各个端口列表check_and_get_id_set失败", "Main", LogCategory.I);
                        return;
                    }

                    int[] lst1 = new int[ApPortList.Count];
                    for (int i = 0; i < ApPortList.Count; i++)
                    {
                        lst1[i] = ApPortList[i];
                    }

                    int[] lst2 = new int[UiPortList.Count];
                    for (int i = 0; i < UiPortList.Count; i++)
                    {
                        lst2[i] = UiPortList[i];
                    }

                    gCheckSocket.SetPortList(lst1, lst2);

                    logInfo = string.Format("收到服务器发过来的端口列表:{0},并调用 gCheckSocket.SetPortList", info);
                    Logger.Trace(LogInfoType.INFO, logInfo, "Main", LogCategory.I);

                    #endregion
                }
                else
                {
                    #region 正常报告处理

                    string logInfo = "";
                    if (false == check_and_get_id_set(info, ref gReportInfo))
                    {
                        Logger.Trace(LogInfoType.EROR, "正常报告处理check_and_get_id_set失败", "Main", LogCategory.I);
                        return;
                    }

                    statusTotalCnt++;

                    bool flag = false;
                    int inx = 0;
                    for (int i = 0; i < gReportInfo.lstThrName.Count; i++)
                    {
                        inx = get_row_number(gReportInfo.lstThrName[i]);
                        if (inx == -1)
                        {
                            //新增
                            add_row_2_dgv(gReportInfo.lstThrName[i], gReportInfo.curValue, gReportInfo.lstThrStatus[i], ref flag);
                        }
                        else
                        {
                            //修改 
                            modify_row_2_dgv(gReportInfo.lstThrName[i], gReportInfo.curValue, gReportInfo.lstThrStatus[i], ref flag);
                        }
                    }

                    if (true == flag)
                    {
                        statusErrorCnt++;
                        if (statusErrorCnt % RebootAtErrCnt == 0)
                        {
                            #region 关闭并重启Server

                            // 2018-11-27,防止Server还没完全退出
                            Thread.Sleep(1234);

                            logInfo = string.Format("正常报告处理->InfoDelegate_Func：关闭并重启Server,{0}/{1}", statusErrorCnt, scannerServerRebootCnt);
                            Logger.Trace(LogInfoType.EROR, logInfo, "Main", LogCategory.I);

                            if (process_is_exit(ServerName))
                            {
                                if (close_process(ServerName))
                                {
                                    logInfo = string.Format("{0},进程关闭成功!", ServerName);
                                    Logger.Trace(LogInfoType.EROR, logInfo, "Main", LogCategory.I);
                                }
                                else
                                {
                                    logInfo = string.Format("{0},进程关闭失败,再次关闭！", ServerName);
                                    Logger.Trace(LogInfoType.EROR, logInfo, "Main", LogCategory.I);
                                    close_process(ServerName);
                                }

                                string fullPath = string.Format("{0}\\{1}.exe", ServerDirectory, ServerName);
                                if (File.Exists(fullPath))
                                {
                                    logInfo = string.Format("{0},重启Server!", fullPath);
                                    Logger.Trace(LogInfoType.EROR, logInfo, "Main", LogCategory.I);

                                    //存在文件 
                                    open_process(fullPath);
                                    scannerServerRebootCnt++;
                                }
                                else
                                {
                                    //不存在文件    
                                    logInfo = string.Format("{0},文件不存在，请确认！", fullPath);
                                    Logger.Trace(LogInfoType.EROR, logInfo, "Main", LogCategory.I);
                                }
                            }
                            else
                            {
                                logInfo = string.Format("{0},进程不存在!", ServerName);
                                Logger.Trace(LogInfoType.EROR, logInfo, "Main", LogCategory.I);

                                string fullPath = string.Format("{0}\\{1}.exe", ServerDirectory, ServerName);
                                if (File.Exists(fullPath))
                                {
                                    logInfo = string.Format("{0},重启Server!", fullPath);
                                    Logger.Trace(LogInfoType.EROR, logInfo, "Main", LogCategory.I);

                                    //存在文件 
                                    open_process(fullPath);
                                    scannerServerRebootCnt++;
                                }
                                else
                                {
                                    //不存在文件    
                                    logInfo = string.Format("{0},文件不存在，请确认！", fullPath);
                                    Logger.Trace(LogInfoType.EROR, logInfo, "Main", LogCategory.I);
                                }
                            }

                            #endregion
                        }
                    }

                    #endregion
                }

                label1LastUpTime.Text = string.Format("{0}/{1}", DateTime.Now.ToShortTimeString(), statusTotalCnt);
                labelErrRebootCnt.Text = string.Format("{0}/{1}", statusErrorCnt, scannerServerRebootCnt);
            }
            catch (Exception ee)
            {
                startTimeConn = System.DateTime.Now;
                Logger.Trace(LogInfoType.EROR, ee.Message + ee.StackTrace, "Main", LogCategory.I);           
            }
        }

        /// <summary>
        /// 线程启动从共享内存中获取数据信息 
        /// </summary>
        private void threadRecvMsg_fun(object obj)
        {
            int rtvLen = 0;
            while (true)
            {
                Thread.Sleep(100);

                try
                {
                    //m_Write = Semaphore.OpenExisting("WriteMap");
                    //m_Read = Semaphore.OpenExisting("ReadMap");
                    //handle = OpenFileMapping(FILE_MAP_WRITE, 0, "shareMemory");

                    //读取共享内存中的数据：
                    //是否有数据写过来
                    m_Read.WaitOne();

                    //IntPtr m_Sender = MapViewOfFile(handle, FILE_MAP_ALL_ACCESS, 0, 0, 0);
                    byte[] byteStr = new byte[1024];
                    byteCopy(byteStr, addr,ref rtvLen);

                    //string str = Encoding.Default.GetString(byteStr, 0, byteStr.Length);
                    string str = Encoding.Default.GetString(byteStr, 0, rtvLen);

                    //线程通过方法的委托执行InfoDelegate_Func
                    Invoke(new InfoDelegate(InfoDelegate_Func), new object[] { str });

                    str = string.Format("收到服务器的消息:{0}", str.TrimEnd());
                    Logger.Trace(LogInfoType.INFO, str, "Main", LogCategory.I);

                    //计时器复位
                    startTimeConn = System.DateTime.Now;

                    byteStr = null;
                    /////调用数据处理方法 处理读取到的数据
                    m_Write.Release();
                }
                catch (WaitHandleCannotBeOpenedException ee)
                {
                    Logger.Trace(LogInfoType.EROR, ee.Message + ee.StackTrace, "Main", LogCategory.I);
                    continue;
                }
            }
        }

        /// <summary>
        /// 处理逻辑的线程
        /// </summary>
        /// <param name="obj"></param>
        private void threadLogicProcess_fun(object obj)
        {
            /*
             *  Monitor的逻辑处理如下：
             *  =========================================================
             * 
             * （1） threadRecvMsg_fun线程中阻塞等Server发报告消息，
             *      收到后处理并更新界面状态，每3次出错就关闭并重启Server。
             *      Server中各个线程每一分钟报告一次状态，而Server每3分钟报告
             *      一次状态给我们的Monitor。
             *      
             * （2） 在本线程中，如果超过5分钟Server还没有报告消息给我们Monitor，
             *      那就关闭并重启Server
             * 
             */

            #region 变量定义


            string logInfo = "";
            int timeout = RebootAtDisconn * 60;

            int timeoutPortCheck = 3 * 60;

            #endregion

            while (true)
            {
                Thread.Sleep(50);

                try
                {                   
                    endTimeConn = System.DateTime.Now;
                    tsConn = endTimeConn.Subtract(startTimeConn);
                    if (tsConn.TotalSeconds >= timeout)
                    {
                        #region 关闭并重启Server

                        logInfo = string.Format("threadLogicProcess_fun：关闭并重启Server,{0}/{1}", statusErrorCnt, scannerServerRebootCnt);
                        Logger.Trace(LogInfoType.EROR, logInfo, "Main", LogCategory.I);

                        if (process_is_exit(ServerName))
                        {
                            if (close_process(ServerName))
                            {
                                logInfo = string.Format("{0},进程关闭成功!", ServerName);
                                Logger.Trace(LogInfoType.EROR, logInfo, "Main", LogCategory.I);
                            }
                            else
                            {
                                logInfo = string.Format("{0},进程关闭失败,再次关闭！", ServerName);
                                Logger.Trace(LogInfoType.EROR, logInfo, "Main", LogCategory.I);
                                close_process(ServerName);
                            }

                            string fullPath = string.Format("{0}\\{1}.exe", ServerDirectory, ServerName);
                            if (File.Exists(fullPath))
                            {
                                logInfo = string.Format("{0},重启Server!", fullPath);
                                Logger.Trace(LogInfoType.EROR, logInfo, "Main", LogCategory.I);

                                //存在文件 
                                open_process(fullPath);
                                scannerServerRebootCnt++;
                            }
                            else
                            {
                                //不存在文件    
                                logInfo = string.Format("{0},文件不存在，请确认！", fullPath);
                                Logger.Trace(LogInfoType.EROR, logInfo, "Main", LogCategory.I);
                            }
                        }
                        else
                        {
                            logInfo = string.Format("{0},进程不存在!", ServerName);
                            Logger.Trace(LogInfoType.EROR, logInfo, "Main", LogCategory.I);

                            string fullPath = string.Format("{0}\\{1}.exe", ServerDirectory, ServerName);
                            if (File.Exists(fullPath))
                            {
                                logInfo = string.Format("{0},重启Server!", fullPath);
                                Logger.Trace(LogInfoType.EROR, logInfo, "Main", LogCategory.I);

                                //存在文件 
                                open_process(fullPath);
                                scannerServerRebootCnt++;
                            }
                            else
                            {
                                //不存在文件    
                                logInfo = string.Format("{0},文件不存在，请确认！", fullPath);
                                Logger.Trace(LogInfoType.EROR, logInfo, "Main", LogCategory.I);
                            }
                        }

                        #endregion

                        startTimeConn = System.DateTime.Now;

                        //线程通过方法的委托执行
                        Invoke(new UpdateDelegate(UpdateDelegate_Func), new object[] {});
                    }       
                }
                catch (Exception ee)
                {
                    startTimeConn = System.DateTime.Now;
                    Logger.Trace(LogInfoType.EROR, ee.Message + ee.StackTrace, "Main", LogCategory.I);
                    continue;
                }


                try
                {
                    endTimeConnPC = System.DateTime.Now;
                    tsConnPC = endTimeConnPC.Subtract(startTimeConnPC);
                    if (tsConnPC.TotalSeconds >= timeoutPortCheck)
                    {
                        if (0 != gCheckSocket.start())
                        {
                            #region 关闭并重启Server

                            logInfo = string.Format("端口检查gCheckSocket.start失败：关闭并重启Server,{0}/{1}", statusErrorCnt, scannerServerRebootCnt);
                            Logger.Trace(LogInfoType.EROR, logInfo, "Main", LogCategory.I);

                            if (process_is_exit(ServerName))
                            {
                                if (close_process(ServerName))
                                {
                                    logInfo = string.Format("{0},进程关闭成功!", ServerName);
                                    Logger.Trace(LogInfoType.EROR, logInfo, "Main", LogCategory.I);
                                }
                                else
                                {
                                    logInfo = string.Format("{0},进程关闭失败,再次关闭！", ServerName);
                                    Logger.Trace(LogInfoType.EROR, logInfo, "Main", LogCategory.I);
                                    close_process(ServerName);
                                }

                                string fullPath = string.Format("{0}\\{1}.exe", ServerDirectory, ServerName);
                                if (File.Exists(fullPath))
                                {
                                    logInfo = string.Format("{0},重启Server!", fullPath);
                                    Logger.Trace(LogInfoType.EROR, logInfo, "Main", LogCategory.I);

                                    //存在文件 
                                    open_process(fullPath);
                                    scannerServerRebootCnt++;
                                }
                                else
                                {
                                    //不存在文件    
                                    logInfo = string.Format("{0},文件不存在，请确认！", fullPath);
                                    Logger.Trace(LogInfoType.EROR, logInfo, "Main", LogCategory.I);
                                }
                            }
                            else
                            {
                                logInfo = string.Format("{0},进程不存在!", ServerName);
                                Logger.Trace(LogInfoType.EROR, logInfo, "Main", LogCategory.I);

                                string fullPath = string.Format("{0}\\{1}.exe", ServerDirectory, ServerName);
                                if (File.Exists(fullPath))
                                {
                                    logInfo = string.Format("{0},重启Server!", fullPath);
                                    Logger.Trace(LogInfoType.EROR, logInfo, "Main", LogCategory.I);

                                    //存在文件 
                                    open_process(fullPath);
                                    scannerServerRebootCnt++;
                                }
                                else
                                {
                                    //不存在文件    
                                    logInfo = string.Format("{0},文件不存在，请确认！", fullPath);
                                    Logger.Trace(LogInfoType.EROR, logInfo, "Main", LogCategory.I);
                                }
                            }

                            #endregion                            

                            //线程通过方法的委托执行
                            Invoke(new UpdateDelegate(UpdateDelegate_Func), new object[] { });
                        }

                        startTimeConnPC = System.DateTime.Now;
                    }
                }
                catch (Exception ee)
                {
                    startTimeConnPC = System.DateTime.Now;
                    Logger.Trace(LogInfoType.EROR, ee.Message + ee.StackTrace, "Main", LogCategory.I);
                    continue;
                }
            }
        }

        #endregion
    }
}
