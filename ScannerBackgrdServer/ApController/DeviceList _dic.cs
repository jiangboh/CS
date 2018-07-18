using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using static ScannerBackgrdServer.Common.MsgStruct;

namespace ScannerBackgrdServer.ApController
{
    class DeviceList
    {
        private struct DeviceDicKey
        {
            public string ip;
            public int port;

            public DeviceDicKey(string ip,int port)
            {
                this.ip = ip;
                this.port = port;
            }
        }

        private static readonly object locker1 = new object();
        private Dictionary<DeviceDicKey,AsyncUserToken> connList;

        public DeviceList()
        {
            connList = new Dictionary<DeviceDicKey, AsyncUserToken>();
        }

        public HashSet<AsyncUserToken> GetConnList()
        {
            HashSet<AsyncUserToken> toKenList = new HashSet<AsyncUserToken>();
            lock (locker1)
            {
                foreach (KeyValuePair<DeviceDicKey, AsyncUserToken> kvp in connList)
                {
                    toKenList.Add(kvp.Value);
                }
            }
            return toKenList;
        }

        public void CopyConnList(ref HashSet<AsyncUserToken> toKenList)
        {
            lock (locker1)
            {
                foreach(KeyValuePair<DeviceDicKey, AsyncUserToken> kvp in connList)
                {
                    toKenList.Add(kvp.Value);
                }
            }
            return;
        }

        public int GetCount()
        {
            lock (locker1)
            {
                return connList.Count;
            }
        }

        /// <summary>
        /// 更改最后收到消息的时间为当前时间
        /// </summary>
        /// <param name="toKen"></param>
        /// <returns>成功:true ；失败:false</returns>
        public bool ChangeEndMsgTime(AsyncUserToken toKen)
        {
            lock (locker1)
            {
                DeviceDicKey dicKey = new DeviceDicKey(toKen.IPAddress.ToString(), toKen.Port);

                if (connList.ContainsKey(dicKey))
                {
                    connList[dicKey].EndMsgTime = DateTime.Now;
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 增加一条消息Id与App信息对应关系
        /// </summary>
        /// <param name="toKen">Ap信息</param>
        /// <param name="msgId2App">id与App对应关系</param>
        /// <returns>成功:true ；失败:false</returns>
        public bool AddMsgId2App(AsyncUserToken toKen,MsgId2App msgId2App)
        {
            lock (locker1)
            {
                DeviceDicKey dicKey = new DeviceDicKey(toKen.IPAddress.ToString(), toKen.Port);

                if (connList.ContainsKey(dicKey))
                {
                    msgId2App.AddTime = DateTime.Now;
                    connList[dicKey].msgId2App.Add(msgId2App);
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 删除一条id与App对应关系
        /// </summary>
        /// <param name="toKen">Ap信息</param>
        /// <param name="msgId">消息Id</param>
        /// <returns>成功:true ；失败:false</returns>
        public bool RemoveMsgId2App(AsyncUserToken toKen, UInt16 msgId)
        {
            bool re = false;
            lock (locker1)
            {
                DeviceDicKey dicKey = new DeviceDicKey(toKen.IPAddress.ToString(), toKen.Port);

                if (connList.ContainsKey(dicKey))
                {
                    HashSet<MsgId2App> RemoveList = new HashSet<MsgId2App>();
                    foreach (MsgId2App y in connList[dicKey].msgId2App)
                    {
                        if (y.id == msgId)
                        {
                            RemoveList.Add(y);
                            re = true;
                        }
                        else
                        {
                            TimeSpan timeSpan = DateTime.Now - y.AddTime;
                            if (timeSpan.TotalSeconds > 30) //大于30秒认为设备不会再回消息了
                            {
                                RemoveList.Add(y);
                            }
                        }
                    }

                    foreach (MsgId2App y in RemoveList)
                    {
                        connList[dicKey].msgId2App.Remove(y);
                    }
                    connList[dicKey].msgId2App.TrimExcess();

                    RemoveList.Clear();
                    RemoveList.TrimExcess();
                }
            }
            return re;
        }

        /// <summary>
        /// 获取AP在MainController模块的在线状态
        /// </summary>
        /// <param name="toKen">AP信息</param>
        /// <returns>状态</returns>
        public string GetMainControllerStatus(AsyncUserToken toKen)
        {
            string str = "unknown";
            lock (locker1)
            {
                DeviceDicKey dicKey = new DeviceDicKey(toKen.IPAddress.ToString(), toKen.Port);

                if (connList.ContainsKey(dicKey))
                {
                    str = connList[dicKey].MainControllerStatus;
                }
            }
            return str;
        }

        /// <summary>
        /// 设置AP在MainController模块的在线状态
        /// </summary>
        /// <param name="status">状态</param>
        /// <param name="toKen">AP信息</param>
        /// <returns>修改成功返回true,否则返回false</returns>
        public bool SetMainControllerStatus(string status,AsyncUserToken toKen)
        {
            lock (locker1)
            {
                DeviceDicKey dicKey = new DeviceDicKey(toKen.IPAddress.ToString(), toKen.Port);
                if (connList.ContainsKey(dicKey))
                {
                    connList[dicKey].MainControllerStatus = status;
                    return true;
                }
            }
            return false;
        }
        public bool SetMainControllerStatus(string status, string ip,int port)
        {
            lock (locker1)
            {
                DeviceDicKey dicKey = new DeviceDicKey(ip, port);

                if (connList.ContainsKey(dicKey))
                {
                    connList[dicKey].MainControllerStatus = status;
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 根据设备全名，查找设备信息(不区分大小写)
        /// </summary>
        /// <param name="fullname">设备全名</param>
        /// <returns>设备信息，未找到返回null</returns>
        //public AsyncUserToken FindByFullname(string fullname)
        //{
        //    AsyncUserToken toKen = null;
        //    lock (locker1)
        //    {
        //        foreach (AsyncUserToken x in connList)
        //        {
        //            if (String.Compare(x.FullName, fullname, true) == 0)
        //            {
        //                return x;
        //            }
        //        }
        //    }
        //    return toKen;
        //}

        /// <summary>
        /// 根据设备SN，查找设备信息(不区分大小写)
        /// </summary>
        /// <param name="sn">设备sn</param>
        /// <returns>设备信息，未找到返回null</returns>
        //public AsyncUserToken FindBySN(string sn)
        //{
        //    AsyncUserToken toKen = null;
        //    lock (locker1)
        //    {
        //        foreach (AsyncUserToken x in connList)
        //        {
        //            if (String.Compare(x.Sn, sn, true) == 0)
        //            {
        //                return x;
        //            }
        //        }
        //    }
        //    return toKen;
        //}

        /// <summary>
        /// 根据ip和端口，查找设备信息
        /// </summary>
        /// <param name="ip">ip</param>
        /// <param name="port">端口</param>
        /// <returns>设备信息，未找到返回null</returns>
        public AsyncUserToken FindByIpPort(string ip,int port)
        {
            AsyncUserToken toKen = null;
            lock (locker1)
            {
                DeviceDicKey dicKey = new DeviceDicKey(ip, port);
                if (connList.ContainsKey(dicKey))
                {
                    return connList[dicKey];
                }
            }
            return toKen;
        }

        /// <summary>
        /// 向设备列表中添加收到的消息
        /// </summary>
        /// <param name="toKen">设备信息</param>
        /// <param name="buff">收到的消息</param>
        /// <returns></returns>
        public bool AddMsgBuff(AsyncUserToken toKen, byte[] buff)
        {
            lock (locker1)
            {
                DeviceDicKey dicKey = new DeviceDicKey(toKen.IPAddress.ToString(), toKen.Port);
                if (connList.ContainsKey(dicKey))
                {
                    connList[dicKey].Buffer.AddRange(buff);
                    connList[dicKey].EndMsgTime = DateTime.Now;
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 获取设备当前收到的消息
        /// </summary>
        /// <param name="toKen">设备信息</param>
        /// <returns>收到的消息</returns>
        public string GetMsgBuff_Str(AsyncUserToken toKen)
        {
            lock (locker1)
            {
                DeviceDicKey dicKey = new DeviceDicKey(toKen.IPAddress.ToString(), toKen.Port);
                if (connList.ContainsKey(dicKey))
                {
                    byte[] rev = connList[dicKey].Buffer.GetRange(0,connList[dicKey].Buffer.Count).ToArray();
                    return System.Text.Encoding.Default.GetString(rev);
                }
            }
            return string.Empty;
        }

        public byte[] GetMsgBuff_Byte(AsyncUserToken toKen)
        {
            lock (locker1)
            {
                DeviceDicKey dicKey = new DeviceDicKey(toKen.IPAddress.ToString(), toKen.Port);
                if (connList.ContainsKey(dicKey))
                {
                    byte[] rev = connList[dicKey].Buffer.GetRange(0, connList[dicKey].Buffer.Count).ToArray();
                    return rev;
                }
            }
            return null;
        }
        /// <summary>
        /// 删除设备收到的消息
        /// </summary>
        /// <param name="toKen">设置信息</param>
        /// <param name="startIndex">起始标识</param>
        /// <param name="endIndex">结束标志</param>
        /// <returns></returns>
        public bool DelMsgBuff(AsyncUserToken toKen,int startIndex,int endIndex)
        {
            lock (locker1)
            {
                DeviceDicKey dicKey = new DeviceDicKey(toKen.IPAddress.ToString(), toKen.Port);
                if (connList.ContainsKey(dicKey))
                {
                    if (startIndex < 0) startIndex = 0;
                    if (endIndex > connList[dicKey].Buffer.Count) endIndex = connList[dicKey].Buffer.Count;

                    connList[dicKey].Buffer.RemoveRange(startIndex,endIndex);
                    connList[dicKey].Buffer.TrimExcess();
                    return true;
                }
            }
            return false;
        }

        //public void add(List<String> snList)
        //{
        //    lock (locker1)
        //    {
        //        foreach (String sn in snList)
        //        {
        //            foreach (AsyncUserToken x in ConnList)
        //            {
        //                if (String.Compare(x.Sn, sn, true) == 0)
        //                {
        //                    ConnList.Remove(x);
        //                    break;
        //                }
        //            }
        //            AsyncUserToken connInfo = new AsyncUserToken(sn);
        //            ConnList.Add(connInfo);
        //        }
        //    }
        //}

        //public void add(String sn)
        //{
        //    lock (locker1)
        //    {
        //        foreach (AsyncUserToken x in ConnList)
        //        {
        //            if (String.Compare(x.Sn, sn, true) == 0)
        //            {
        //                ConnList.Remove(x);
        //                break;
        //            }
        //        }
        //        AsyncUserToken connInfo = new AsyncUserToken(sn);
        //        ConnList.Add(connInfo);
        //    }
        //}

        public int add(AsyncUserToken toKen)
        {
            lock (locker1)
            {
                DeviceDicKey dicKey = new DeviceDicKey(toKen.IPAddress.ToString(), toKen.Port);
                if (connList.ContainsKey(dicKey))
                {
                    connList[dicKey].EndMsgTime = DateTime.Now;
                }
                else
                {
                    toKen.EndMsgTime = DateTime.Now;
                    toKen.MainControllerStatus = "un";
                    toKen.ConnectTime = DateTime.Now;
   
                    connList.Add(dicKey,toKen);
                }

                return connList.Count;
            }
        }

        public int remov(String ip,int port)
        {
            lock (locker1)
            {
                DeviceDicKey dicKey = new DeviceDicKey(ip,port);
                if (connList.ContainsKey(dicKey))
                { 
                    connList.Remove(dicKey);
                    return connList.Count;
                }
            }
            return -1;
        }

        //public int remov (EndPoint Remote)
        //{
        //    lock (locker1)
        //    {
        //        foreach (AsyncUserToken x in connList)
        //        {
        //            if (Remote == x.Remote)
        //            {
        //                connList.Remove(x);
        //                connList.TrimExcess();
        //                return connList.Count;
        //            }
        //        }
        //    }
        //    return -1;
        //}

        public int remov(AsyncUserToken connInfo)
        {
             return this.remov(connInfo.IPAddress.ToString(),connInfo.Port);
        }


    }
}
