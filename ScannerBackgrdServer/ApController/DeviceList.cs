﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static ScannerBackgrdServer.Common.MsgStruct;

namespace ScannerBackgrdServer.ApController
{
    class DeviceList
    {
        private static readonly object locker1 = new object();
        private HashSet<AsyncUserToken> connList;

        public DeviceList()
        {
            connList = new HashSet<AsyncUserToken>();
            //启动AP状态检测线程
            Thread t = new Thread(new ThreadStart(CheckMsgId));
            t.Start();
            t.IsBackground = true;
        }

        private void CheckMsgId()
        {
            while (true)
            {
                lock (locker1)
                {
                    foreach (AsyncUserToken x in connList)
                    {
                        HashSet<MsgId2App> RemoveList = new HashSet<MsgId2App>();
                        foreach (MsgId2App y in x.msgId2App)
                        {
                            TimeSpan timeSpan = DateTime.Now - y.AddTime;
                            if (timeSpan.TotalSeconds > 20) //大于120秒认为设备不会再回消息了
                            {
                                RemoveList.Add(y);
                            }
                        }

                        foreach (MsgId2App y in RemoveList)
                        {
                            x.msgId2App.Remove(y);
                        }
                        x.msgId2App.TrimExcess();

                        RemoveList.Clear();
                        RemoveList.TrimExcess();
                    }
                }
                Thread.Sleep(10000);
            }
        }

        public HashSet<AsyncUserToken> GetConnList()
        {
            lock (locker1)
            {
                return connList;
            }
        }

        public void CopyConnList(ref HashSet<AsyncUserToken> toKenList)
        {
            lock (locker1)
            {
                foreach(AsyncUserToken x in connList)
                {
                    toKenList.Add(x);
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
                foreach (AsyncUserToken x in connList)
                {
                    if ((String.Compare(x.IPAddress.ToString(), toKen.IPAddress.ToString(), true) == 0) &&
                        (String.Compare(x.Port.ToString(), toKen.Port.ToString(), true) == 0))
                    {
                        x.EndMsgTime = DateTime.Now;
                        return true;
                    }
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
                foreach (AsyncUserToken x in connList)
                {
                    if ((String.Compare(x.IPAddress.ToString(), toKen.IPAddress.ToString(), true) == 0) &&
                        (String.Compare(x.Port.ToString(), toKen.Port.ToString(), true) == 0))
                    {
                        msgId2App.AddTime = DateTime.Now;
                        x.msgId2App.Add(msgId2App);
                        return true;
                    }
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
                foreach (AsyncUserToken x in connList)
                {
                    if ((String.Compare(x.IPAddress.ToString(), toKen.IPAddress.ToString(), true) == 0) &&
                        (String.Compare(x.Port.ToString(), toKen.Port.ToString(), true) == 0))
                    {
                        HashSet<MsgId2App> RemoveList = new HashSet<MsgId2App>();
                        foreach(MsgId2App y in x.msgId2App)
                        {
                            if (y.id == msgId)
                            {
                                RemoveList.Add(y);
                                re = true;
                            }
                            else
                            {
                                TimeSpan timeSpan = DateTime.Now - y.AddTime;
                                if (timeSpan.TotalSeconds > 120) //大于120秒认为设备不会再回消息了
                                {
                                    RemoveList.Add(y);
                                }
                            }
                        }

                        foreach (MsgId2App y in RemoveList)
                        {
                            x.msgId2App.Remove(y);
                        }
                        x.msgId2App.TrimExcess();

                        RemoveList.Clear();
                        RemoveList.TrimExcess();

                        break;
                    }
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
            string str = "";
            lock (locker1)
            {
                foreach (AsyncUserToken x in connList)
                {
                    if ((String.Compare(x.IPAddress.ToString(), toKen.IPAddress.ToString(), true) == 0) &&
                        (String.Compare(x.Port.ToString(), toKen.Port.ToString(), true) == 0))
                    {
                        str = x.MainControllerStatus;
                        break;
                    }
                }
            }
            if (string.IsNullOrEmpty(str)) str = "unknown";
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
                foreach (AsyncUserToken x in connList)
                {
                    if ((String.Compare(x.IPAddress.ToString(), toKen.IPAddress.ToString(), true) == 0) &&
                        (String.Compare(x.Port.ToString(), toKen.Port.ToString(), true) == 0))
                    {
                        x.MainControllerStatus = status;
                        return true;
                    }
                }
            }
            return false;
        }
        public bool SetMainControllerStatus(string status, string ip,int port)
        {
            lock (locker1)
            {
                foreach (AsyncUserToken x in connList)
                {
                    if ((String.Compare(x.IPAddress.ToString(),ip.ToString(), true) == 0) &&
                        (String.Compare(x.Port.ToString(),port.ToString(), true) == 0))
                    {
                        x.MainControllerStatus = status;
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// 根据设备全名，查找设备信息(不区分大小写)
        /// </summary>
        /// <param name="fullname">设备全名</param>
        /// <returns>设备信息，未找到返回null</returns>
        public AsyncUserToken FindByFullname(string fullname)
        {
            AsyncUserToken toKen = null;
            lock (locker1)
            {
                foreach (AsyncUserToken x in connList)
                {
                    if (String.Compare(x.FullName, fullname, true) == 0)
                    {
                        return x;
                    }
                }
            }
            return toKen;
        }

        /// <summary>
        /// 根据设备SN，查找设备信息(不区分大小写)
        /// </summary>
        /// <param name="sn">设备sn</param>
        /// <returns>设备信息，未找到返回null</returns>
        public AsyncUserToken FindBySN(string sn)
        {
            AsyncUserToken toKen = null;
            lock (locker1)
            {
                foreach (AsyncUserToken x in connList)
                {
                    if (String.Compare(x.Sn, sn, true) == 0)
                    {
                        return x;
                    }
                }
            }
            return toKen;
        }

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
                foreach (AsyncUserToken x in connList)
                {
                    if ((String.Compare(x.IPAddress.ToString(), ip, true) == 0) &&
                        (String.Compare(x.Port.ToString(), port.ToString(), true) == 0))
                    {
                        return x;
                    }
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
                foreach (AsyncUserToken x in connList)
                {
                    if ((String.Compare(x.IPAddress.ToString(), toKen.IPAddress.ToString(), true) == 0) &&
                        (String.Compare(x.Port.ToString(), toKen.Port.ToString(), true) == 0))
                    {
                        x.Buffer.AddRange(buff);
                        x.EndMsgTime = DateTime.Now;
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// 获取设备当前收到的消息
        /// </summary>
        /// <param name="toKen">设备信息</param>
        /// <param name="startIndex">起始位置</param>
        /// <param name="len">长度</param>
        /// <returns>收到的消息</returns>
        public string GetMsgBuff(AsyncUserToken toKen,int startIndex, int len)
        {
            lock (locker1)
            {
                foreach (AsyncUserToken x in connList)
                {
                    if ((String.Compare(x.IPAddress.ToString(), toKen.IPAddress.ToString(), true) == 0) &&
                        (String.Compare(x.Port.ToString(), toKen.Port.ToString(), true) == 0))
                    {
                        if (startIndex < 0) startIndex = 0;
                        if ((startIndex + len ) > x.Buffer.Count) len = x.Buffer.Count - startIndex;

                        byte[] rev = x.Buffer.GetRange(startIndex, len).ToArray();
                        return System.Text.Encoding.Default.GetString(rev);
                    }
                }
            }
            return string.Empty;
        }

        public byte[] GetAllMsgBuff(AsyncUserToken toKen)
        {
            lock (locker1)
            {
                foreach (AsyncUserToken x in connList)
                {
                    if ((String.Compare(x.IPAddress.ToString(), toKen.IPAddress.ToString(), true) == 0) &&
                        (String.Compare(x.Port.ToString(), toKen.Port.ToString(), true) == 0))
                    {
                        byte[] rev = x.Buffer.GetRange(0, x.Buffer.Count).ToArray();
                        return rev;
                    }
                }
            }
            return null;
        }
        /// <summary>
        /// 删除设备收到的消息
        /// </summary>
        /// <param name="toKen">设置信息</param>
        /// <param name="startIndex">起始标识</param>
        /// <param name="len">删除长度</param>
        /// <returns></returns>
        public bool DelMsgBuff(AsyncUserToken toKen,int startIndex,int len)
        {
            lock (locker1)
            {
                foreach (AsyncUserToken x in connList)
                {
                    if ((String.Compare(x.IPAddress.ToString(), toKen.IPAddress.ToString(), true) == 0) &&
                        (String.Compare(x.Port.ToString(), toKen.Port.ToString(), true) == 0))
                    {
                        if (startIndex < 0) startIndex = 0;
                        if ((startIndex + len) > x.Buffer.Count) len = x.Buffer.Count - startIndex;

                        x.Buffer.RemoveRange(startIndex,len);
                        x.Buffer.TrimExcess();
                        return true;
                    }
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
            bool isExist = false; //标识是否存在该Ap
            lock (locker1)
            {
                foreach (AsyncUserToken x in connList)
                {
                    if ((String.Compare(x.IPAddress.ToString(), toKen.IPAddress.ToString(), true) == 0) &&
                        (String.Compare(x.Port.ToString(), toKen.Port.ToString(), true) == 0))
                    {
                        isExist = true;
                        toKen.ConnectTime = x.ConnectTime;
                        toKen.MainControllerStatus = x.MainControllerStatus;
                        connList.Remove(x);
                        connList.TrimExcess();
                        break;
                    }
                }
                toKen.EndMsgTime = DateTime.Now;
                if (!isExist)
                {
                    toKen.MainControllerStatus = "";
                    toKen.ConnectTime = DateTime.Now;
                }

                connList.Add(toKen);

                return connList.Count;
            }
        }

        public int remov(String ip,int port)
        {
            lock (locker1)
            {
                foreach (AsyncUserToken x in connList)
                {
                    if ((String.Compare(x.IPAddress.ToString(), ip, true) == 0) &&
                        (String.Compare(x.Port.ToString(), port.ToString(), true) == 0))
                    { 
                        connList.Remove(x);
                        connList.TrimExcess();
                        return connList.Count;
                    }
                }
            }
            return -1;
        }

        public int remov (EndPoint Remote)
        {
            lock (locker1)
            {
                foreach (AsyncUserToken x in connList)
                {
                    if (Remote == x.Remote)
                    {
                        connList.Remove(x);
                        connList.TrimExcess();
                        return connList.Count;
                    }
                }
            }
            return -1;
        }

        public int remov(AsyncUserToken connInfo)
        {
             return this.remov(connInfo.IPAddress.ToString(),connInfo.Port);
        }


    }
}
