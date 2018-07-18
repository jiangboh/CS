using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using static ScannerBackgrdServer.Common.MsgStruct;

namespace ScannerBackgrdServer.ApController
{
    class AsyncUserToken
    {
        /// <summary>  
        /// 客户端IP地址  
        /// </summary>  
        public IPAddress IPAddress { get; set; }

        /// <summary>  
        /// 客户端Port  
        /// </summary>  
        public int Port { get; set; }

        /// <summary>  
        /// 客户端Sn  
        /// </summary>  
        public string Sn { get; set; }

        /// <summary>  
        /// 客户端类型GSM/TD_SCDMA/TDD_LTE/FDD_LTE/CDMA等  
        /// </summary>  
        public string Mode { get; set; }

        /// <summary>  
        /// 客户端Id 
        /// </summary>  
        public string Id { get; set; }

        /// <summary>  
        /// 客户端各种状态
        /// </summary>  
        public UInt32 Detail { get; set; }

        /// <summary>  
        /// 客户端全名
        /// </summary>  
        public string FullName { get; set; }

        /// <summary>  
        /// 远程地址  
        /// </summary>  
        public EndPoint Remote { get; set; }

        /// <summary>  
        /// 通信SOKET  
        /// </summary>  
        public Socket Socket { get; set; }

        /// <summary>  
        /// 连接时间  
        /// </summary>  
        public DateTime ConnectTime { get; set; }

        /// <summary>  
        /// 最后一次收到消息时间  
        /// </summary>  
        public DateTime EndMsgTime { get; set; }

        /// <summary>
        /// MainController模块保存的设备状态
        /// </summary>
        public string MainControllerStatus { get; set; }

        /// <summary>
        /// App登录用户名
        /// </summary>
        public string User { get; set; }

        /// <summary>
        /// App登录用户所属组
        /// </summary>
        public string Group { get; set; }

        /// <summary>
        /// App登录用户所属域
        /// </summary>
        public string Domain { get; set; }

        /// <summary>
        /// 设备消息和App信息对应关系
        /// </summary>
        public HashSet<MsgId2App> msgId2App { get; set; }

        /// <summary>  
        /// 数据缓存区  
        /// </summary>  
        public List<byte> Buffer { get; set; }


        public AsyncUserToken()
        {
            this.msgId2App = new HashSet<MsgId2App>();
            this.Buffer = new List<byte>();
            this.MainControllerStatus = "unknown";
        }
    }
}
