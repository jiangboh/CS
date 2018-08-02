using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using static ScannerBackgrdServer.Common.MsgStruct;

namespace ScannerBackgrdServer.ApController
{
    class Ap_GSM_ZYF : ApBase
    { 
        #region 类参数定义及构造函数

        public static uint heartbeatMsgNum = 0;
        public static uint imsiMsgNum = 0;

        private string MODE_NAME = ApInnerType.GSM_V2.ToString();

        public Ap_GSM_ZYF()
        {
            DeviceType = MODE_NAME;
            //ApBase.ReceiveMainData += OnReceiveMainMsg;
        }

        #endregion
    }
}