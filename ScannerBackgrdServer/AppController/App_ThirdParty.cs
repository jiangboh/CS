using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using ScannerBackgrdServer.ApController;
using ScannerBackgrdServer.Common;
using static ScannerBackgrdServer.Common.MsgStruct;

namespace ScannerBackgrdServer.AppController
{
    class App_ThirdParty : AppBase
    {
        private string MODE_NAME = AppInnerType.APP_ThirdParty.ToString();

        public App_ThirdParty()
        {
            DeviceType = MODE_NAME;
            AppBase.ReceiveMainData += OnReceiveMainMsg;
        }
        /// <summary>
        /// 重载LOG函数
        /// </summary>
        /// <param name="str"></param>
        //public override void OnOutputLog(LogInfoType type, string str)
        //{
        //    string outStr = "[" + MODE_NAME + "] " + str;
        //    Console.WriteLine(outStr);
        //    FrmMainController.add_log_info(LogInfoType.EROR, outStr);
        //    Logger.Trace(LogInfoType.EROR, outStr);
        //}

        /// <summary>
        /// 重载收到App侧消息处理
        /// </summary>
        /// <param name="appToKen">Ap信息</param>
        /// <param name="buff">消息内容</param>
        public override void OnReceiveDeviceMsg(AsyncUserToken appToKen, byte[] buff)
        {
            while (true)
            {
                string msg = GetDeviceMsg_JSON(appToKen);
                if (string.IsNullOrEmpty(msg))
                {
                    //OnOutputLog(LogInfoType.INFO, "消息已解析完。缓存里没有完整消息了");
                    return;
                }

                HandleAppMsg(appToKen, msg);
            }
        }

        /// <summary>
        /// 处理收到的App侧消息
        /// </summary>
        /// <param name="appToKen"></param>
        /// <param name="AppMsg"></param>
        private void HandleAppMsg(AsyncUserToken appToKen, string msg)
        {
            ////解析收到的消息
            DeviceServerMsgStruct AppMsg = null;
            try
            {
                AppMsg = JsonConvert.DeserializeObject<DeviceServerMsgStruct>(msg);
            }
            catch (Exception)
            {
                OnOutputLog(LogInfoType.EROR, "解析收到的App消息出错。JSON格式错误！");
                SendErrorMsg2App(appToKen, null, "解析收到的App消息出错。JSON格式错误！");
                return;
            }

            if (AppMsg == null)
            {
                OnOutputLog(LogInfoType.EROR, "收到消息格式错误！");
                SendErrorMsg2App(appToKen, null, "收到消息格式错误！");
                OnOutputLog(LogInfoType.DEBG, "出错消息内容：" + msg);
                return;
            }

            Msg_Body_Struct msgBody = AppMsg.Body;
            if (msgBody == null)
            {
                OnOutputLog(LogInfoType.EROR, "消息内容为NULL！");
                SendErrorMsg2App(appToKen, null, "消息内容为NULL！");
                return;
            }

            if (msgBody.type == ApMsgType.device_test_request)
            {
                Send2App_device_test_response(appToKen);
                return;
            }

            if (msgBody.type != ApMsgType.status_response)
            {
                OnOutputLog(LogInfoType.INFO, string.Format("处理APP[{0}:{1}]消息({2})！",
                    appToKen.IPAddress.ToString(), appToKen.Port, msgBody.type));
            }

            //心跳消息
            if (AppMsg.Body.type == AppMsgType.app_heartbeat_request)
            {
                //OnOutputLog(LogInfoType.INFO, "收到心跳消息");
                //if (GetMsgIntValueInList(MsgStruct.AllNum, msgBody) != msgBody.dic.Count())
                //{
                //    OnOutputLog(LogInfoType.EROR, string.Format("收到消息里键值对数[{0}]与校验值[{1}]不一至。",
                //       msgBody.dic.Count(), GetMsgIntValueInList(MsgStruct.AllNum, msgBody)));
                //    return;
                //}
                appToKen.User = (string)GetMsgStringValueInList("User", msgBody);
                appToKen.Group = (string)GetMsgStringValueInList("Group", msgBody);
                appToKen.Domain = (string)GetMsgStringValueInList("Domain", msgBody);

                MyDeviceList.add(appToKen);

                Msg_Body_Struct TypeKeyValue =
                    new Msg_Body_Struct(AppMsgType.app_heartbeat_response,
                    "timestamp", DateTime.Now.ToLocalTime().ToString());
                SendMsg2App(appToKen, TypeKeyValue);
            }
            else if (AppMsg.Body.type == AppMsgType.transparent_msg_request)
            {
                //透传消息
                OnSendTransparentMsg2Main(appToKen, AppMsg.ApInfo, AppMsg.Body);
            }
            else
            {
                //将消息转发给Main模块
                OnSendMsg2Main(appToKen, AppMsg.ApInfo, AppMsg.Body);
            }
        }

        /// <summary>
        /// 重载收到Main模块消息处理
        /// </summary>
        /// <param name="str">消息内容</param>
        public void OnReceiveMainMsg(MsgStruct.InterModuleMsgStruct MainMsg)
        {
            //在此处理MainController发送过来的信息
            if (MainMsg == null || MainMsg.Body == null)
            {
                OnOutputLog(LogInfoType.EROR, "收到MainController模块消息内容为空!");
                return;
            }

            if (MainMsg.AppInfo.Ip == null || MainMsg.AppInfo.Type == null)
            {
                OnOutputLog(LogInfoType.EROR, "收到MainController模块消息AppInfo内容错误!");
                return;
            }

            if ((!MainMsg.AppInfo.Ip.Equals(MsgStruct.AllDevice)) && (!MainMsg.AppInfo.Type.Equals(MODE_NAME)))
            {
                //OnOutputLog(LogInfoType.DEBG, "收到MainController模块消息,不是本模块消息!");
                return;
            }

            OnOutputLog(LogInfoType.INFO, string.Format("处理MainController消息。消息类型:{0}。", MainMsg.Body.type));

            DeviceServerMsgStruct deviceServerMsgStruct = new DeviceServerMsgStruct();
            deviceServerMsgStruct.Version = MainMsg.Version;
            deviceServerMsgStruct.ApInfo = MainMsg.ApInfo;
            deviceServerMsgStruct.Body = MainMsg.Body;

            SendMsg2App(MainMsg.AppInfo, deviceServerMsgStruct);

            return;
        }
    }
}
