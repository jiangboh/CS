using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ScannerBackgrdServer
{
    class InstanceManager
    {
        [DllImport("user32.dll")]  //使用user32.dll中提供的两个函数实现显示和激活
        private static extern bool ShowWindowAsync(IntPtr hWnd, int cmdShow);

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        private const int WS_SHOWNORMAL = 1;

        public static Process RunningInstance()
        {  
            //查找是否有同名的进程并比对信息
            Process current = Process.GetCurrentProcess();
            Process[] processes = Process.GetProcessesByName(current.ProcessName);

            foreach (Process process in processes)
            {
                if (current.Id != process.Id &&
                    current.MainModule.FileName == process.MainModule.FileName)
                {
                    return process;
                }
            }

            return null;
        }

        public static void HandleRunningProcess(Process instance)
        {
            //确保窗口没有被最小化和最大化
            ShowWindowAsync(instance.MainWindowHandle, WS_SHOWNORMAL);

            //将窗体显示在前面
            SetForegroundWindow(instance.MainWindowHandle);
        }
    }

    static class Program
    {
        /// <summary>
        /// 是否退出应用程序
        /// </summary>
        static bool glExitApp = true ;

        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            Process instance = InstanceManager.RunningInstance();

            //处理未捕获的异常
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            
            //处理UI线程异常
            Application.ThreadException += new System.Threading.ThreadExceptionEventHandler(Application_ThreadException);

            //处理非线程异常
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);

            if (instance == null)
            {   
                //下面的三行代码是之前Main函数中的
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new FrmMainController());               
            }
            else
            {
                MessageBox.Show("只能运行一个服务器！", "出错", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                InstanceManager.HandleRunningProcess(instance);
            }           

            //Application.EnableVisualStyles();
            //Application.SetCompatibleTextRenderingDefault(false);
            //Application.Run(new FrmMainController());
        }


        /// <summary>
        /// 处理未捕获异常
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {      
            SaveLog("\r\n\r\n");
            SaveLog("-----------------------begin--------------------------");
            SaveLog("CurrentDomain_UnhandledException " + DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss"));
            SaveLog("IsTerminating : " + e.IsTerminating.ToString());
            SaveLog(e.ExceptionObject.ToString());
            SaveLog("-----------------------ended--------------------------");
            SaveLog("\r\n\r\n");

            while (true)
            {
                //循环处理，否则应用程序将会退出
                if (glExitApp)
                {
                    //标志应用程序可以退出，否则程序退出后，进程仍然在运行
                    SaveLog("CurrentDomain_UnhandledException_ExitApp " + DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss"));

                    if (FrmMainController.process_is_exit("Monitor"))
                    {
                        SaveLog("send RESTART_ME_RIGHTNOW to Monitor. " + DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss"));
                        FrmMainController.send_data_2_monitor("RESTART_ME_RIGHTNOW");
                    }

                    System.Threading.Thread.Sleep(100);
                    System.Environment.Exit(System.Environment.ExitCode);                   
                }

                SaveLog("CurrentDomain_UnhandledException_ExitApp... " + DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss"));
                System.Threading.Thread.Sleep(2 * 1000);
            };
        }

        /// <summary>
        /// 处理UI主线程异常
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void Application_ThreadException(object sender, System.Threading.ThreadExceptionEventArgs e)
        {
            SaveLog("\r\n\r\n");
            SaveLog("-----------------------begin--------------------------");
            SaveLog("Application_ThreadException " + DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss"));
            SaveLog("Application_ThreadException:" + e.Exception.Message);
            SaveLog(e.Exception.StackTrace);
            SaveLog("-----------------------ended--------------------------");
            SaveLog("\r\n\r\n");

            while (true)
            {
                //循环处理，否则应用程序将会退出
                if (glExitApp)
                {
                    //标志应用程序可以退出，否则程序退出后，进程仍然在运行
                    SaveLog("Application_ThreadException_ExitApp " + DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss"));
                    if (FrmMainController.process_is_exit("Monitor"))
                    {            
                        SaveLog("send RESTART_ME_RIGHTNOW to Monitor. " + DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss"));
                        FrmMainController.send_data_2_monitor("RESTART_ME_RIGHTNOW");
                    }

                    System.Threading.Thread.Sleep(100);

                    System.Environment.Exit(System.Environment.ExitCode);
                }

                SaveLog("Application_ThreadException_ExitApp... " + DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss"));
                System.Threading.Thread.Sleep(2 * 1000);
            };
        }

        public static void SaveLog(string log)
        {
            string filePath = AppDomain.CurrentDomain.BaseDirectory + @"\LogSer\Exception.txt";

            //采用using关键字，会自动释放
            using (FileStream fs = new FileStream(filePath, FileMode.Append))
            {
                using (StreamWriter sw = new StreamWriter(fs, Encoding.Default))
                {
                    sw.WriteLine(log);
                }
            }
        }
    }
}
