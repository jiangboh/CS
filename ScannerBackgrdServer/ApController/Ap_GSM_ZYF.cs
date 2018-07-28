using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ScannerBackgrdServer.ApController
{
    class Ap_GSM_ZYF : ApBase
    { 
        #region 类参数定义及构造函数

        public static uint heartbeatMsgNum = 0;
        public static uint imsiMsgNum = 0;

        private const string MODE_NAME = "GSM_ZYF";

        public Ap_GSM_ZYF()
        {
            DeviceType = MODE_NAME;
            //ApBase.ReceiveMainData += OnReceiveMainMsg;
        }

        #endregion
    }
}