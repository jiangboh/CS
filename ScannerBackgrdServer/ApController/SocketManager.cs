﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ScannerBackgrdServer.Common;

namespace ScannerBackgrdServer.ApController
{
    class SocketManager
    {

        private int m_maxConnectNum;    //最大连接数  
        private int m_revBufferSize;    //最大接收字节数  
        BufferManager m_bufferManager;
        const int opsToAlloc = 2;
        Socket listenSocket;            //监听Socket  
        SocketEventPool m_pool;
        int m_clientCount;              //连接的客户端数量  
        Semaphore m_maxNumberAcceptedClients;

        List<AsyncUserToken> m_clients; //客户端列表  

        #region 定义委托  

        /// <summary>  
        /// 客户端连接数量变化时触发  
        /// </summary>  
        /// <param name="num">当前增加客户的个数(用户退出时为负数,增加时为正数,一般为1)</param>  
        /// <param name="token">增加用户的信息</param>  
        public delegate void OnClientNumberChange(int num, AsyncUserToken token);

        /// <summary>  
        /// 接收到客户端的数据  
        /// </summary>  
        /// <param name="token">客户端</param>  
        /// <param name="buff">客户端数据</param>  
        public delegate void OnReceiveData(AsyncUserToken token, byte[] buff);

        /// <summary>
        /// 输出LOG
        /// </summary>
        /// <param name="str">要输出的内容</param>
        public delegate void OnOutputLog(LogInfoType type, string str,
                                LogCategory category = LogCategory.I,
                                [CallerMemberName] string memberName = "",
                                [CallerFilePath] string filePath = "",
                                [CallerLineNumber] int lineNumber = 0);

        #endregion

        #region 定义事件  
        /// <summary>  
        /// 客户端连接数量变化事件  
        /// </summary>  
        public event OnClientNumberChange ClientNumberChange;

        /// <summary>  
        /// 接收到客户端的数据事件  
        /// </summary>  
        public event OnReceiveData ReceiveClientData;

        /// <summary>
        /// 输出LOG信息事件
        /// </summary>
        public event OnOutputLog OutputLog;

        #endregion

        #region 定义属性  

        /// <summary>  
        /// 获取客户端列表  
        /// </summary>  
        public List<AsyncUserToken> ClientList { get { return m_clients; } }

        #endregion

        private void log(LogInfoType type, string str,
                                LogCategory category = LogCategory.I,
                                [CallerMemberName] string memberName = "",
                                [CallerFilePath] string filePath = "",
                                [CallerLineNumber] int lineNumber = 0)
        {
            if (OutputLog != null)
                OutputLog(type,str, category,memberName, filePath, lineNumber);
        }

        /// <summary>  
        /// 构造函数  
        /// </summary>  
        /// <param name="numConnections">最大连接数</param>  
        /// <param name="receiveBufferSize">缓存区大小</param>  
        public SocketManager(int numConnections, int receiveBufferSize)
        {
            m_clientCount = 0;
            m_maxConnectNum = numConnections;
            m_revBufferSize = receiveBufferSize;
            // allocate buffers such that the maximum number of sockets can have one outstanding read and   
            //write posted to the socket simultaneously    
            m_bufferManager = new BufferManager(receiveBufferSize * numConnections * opsToAlloc, receiveBufferSize);

            m_pool = new SocketEventPool(numConnections);
            m_maxNumberAcceptedClients = new Semaphore(numConnections, numConnections);
        }

        /// <summary>  
        /// 初始化  
        /// </summary>  
        public void Init()
        {
            // Allocates one large byte buffer which all I/O operations use a piece of.  This gaurds   
            // against memory fragmentation  
            m_bufferManager.InitBuffer();
            m_clients = new List<AsyncUserToken>();
            // preallocate pool of SocketAsyncEventArgs objects  
            SocketAsyncEventArgs readWriteEventArg;

            for (int i = 0; i < m_maxConnectNum; i++)
            {
                readWriteEventArg = new SocketAsyncEventArgs();
                readWriteEventArg.Completed += new EventHandler<SocketAsyncEventArgs>(IO_Completed);
                readWriteEventArg.UserToken = new AsyncUserToken();

                // assign a byte buffer from the buffer pool to the SocketAsyncEventArg object  
                m_bufferManager.SetBuffer(readWriteEventArg);
                // add SocketAsyncEventArg to the pool  
                m_pool.Push(readWriteEventArg);
            }
            log(LogInfoType.INFO , "初始化Socket完成！最大连接数为:" + m_maxConnectNum + ".");
        }


        /// <summary>  
        /// 启动服务  
        /// </summary>  
        /// <param name="localEndPoint"></param>  
        public bool Start(IPEndPoint localEndPoint)
        {
            try
            {
                m_clients.Clear();
                listenSocket = new Socket(localEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                listenSocket.Bind(localEndPoint);
                // start the server with a listen backlog of 100 connections  
                listenSocket.Listen(m_maxConnectNum);
                // post accepts on the listening socket  
                StartAccept(null);

                string str = string.Format("服务端({0}:{1})启动成功,进入监听状态 ！", localEndPoint.Address.ToString(), 
                    localEndPoint.Port.ToString());
                log(LogInfoType.INFO,str);

                return true;
            }
            catch (Exception e)
            {
                log(LogInfoType.EROR,"启动服务器失败。" + e.Message.ToString());
                return false;
            }
        }

        /// <summary>  
        /// 停止服务  
        /// </summary>  
        public void Stop()
        {
            foreach (AsyncUserToken token in m_clients)
            {
                try
                {
                    token.Socket.Shutdown(SocketShutdown.Both);
                }
                catch (Exception) { }
            }
            try
            {
                listenSocket.Shutdown(SocketShutdown.Both);
            }
            catch (Exception) { }

            listenSocket.Close();
            int c_count = m_clients.Count;
            lock (m_clients) { m_clients.Clear(); }

            if (ClientNumberChange != null)
                ClientNumberChange(-c_count, null);
        }


        public void CloseClient(AsyncUserToken token)
        {
            try
            {
                token.Socket.Shutdown(SocketShutdown.Both);
            }
            catch (Exception) { }
        }


        // Begins an operation to accept a connection request from the client   
        //  
        // <param name="acceptEventArg">The context object to use when issuing   
        // the accept operation on the server's listening socket</param>  
        public void StartAccept(SocketAsyncEventArgs acceptEventArg)
        {
            if (acceptEventArg == null)
            {
                acceptEventArg = new SocketAsyncEventArgs();
                acceptEventArg.Completed += new EventHandler<SocketAsyncEventArgs>(AcceptEventArg_Completed);
            }
            else
            {
                // socket must be cleared since the context object is being reused  
                acceptEventArg.AcceptSocket = null;
            }

            m_maxNumberAcceptedClients.WaitOne();
            if (!listenSocket.AcceptAsync(acceptEventArg))
            {
                ProcessAccept(acceptEventArg);
            }
        }

        // This method is the callback method associated with Socket.AcceptAsync   
        // operations and is invoked when an accept operation is complete  
        //  
        void AcceptEventArg_Completed(object sender, SocketAsyncEventArgs e)
        {
            ProcessAccept(e);
        }

        private void ProcessAccept(SocketAsyncEventArgs e)
        {
            try
            {
                Interlocked.Increment(ref m_clientCount);
                // Get the socket for the accepted client connection and put it into the   
                //ReadEventArg object user token  
                SocketAsyncEventArgs readEventArgs = m_pool.Pop();
                AsyncUserToken userToken = (AsyncUserToken)readEventArgs.UserToken;
                userToken.Socket = e.AcceptSocket;
                userToken.ConnectTime = DateTime.Now;
                userToken.Remote = e.AcceptSocket.RemoteEndPoint;
                userToken.IPAddress = ((IPEndPoint)(e.AcceptSocket.RemoteEndPoint)).Address;
                userToken.Port = ((IPEndPoint)(e.AcceptSocket.RemoteEndPoint)).Port;

                lock (m_clients) { m_clients.Add(userToken); }

                //log(LogInfoType.EROR, string.Format("设备[{0}:{1}]建立TCP连接。当前连接数：{2}！", 
                //    userToken.IPAddress.ToString(), userToken.Port,m_clients.Count));

                if (ClientNumberChange != null)
                    ClientNumberChange(1, userToken);

                if (!e.AcceptSocket.ReceiveAsync(readEventArgs))
                {
                    ProcessReceive(readEventArgs);
                }
            }
            catch (Exception me)
            {
                log(LogInfoType.EROR,me.Message + "\r\n" + me.StackTrace);
            }

            // Accept the next connection request  
            if (e.SocketError == SocketError.OperationAborted) return;
            StartAccept(e);
        }


        void IO_Completed(object sender, SocketAsyncEventArgs e)
        {
            // determine which type of operation just completed and call the associated handler  
            switch (e.LastOperation)
            {
                case SocketAsyncOperation.Receive:
                    ProcessReceive(e);
                    break;
                case SocketAsyncOperation.Send:
                    ProcessSend(e);
                    break;
                default:
                    throw new ArgumentException("The last operation completed on the socket was not a receive or send");
            }

        }


        // This method is invoked when an asynchronous receive operation completes.   
        // If the remote host closed the connection, then the socket is closed.    
        // If data was received then the data is echoed back to the client.  
        //  
        private void ProcessReceive(SocketAsyncEventArgs e)
        {
            try
            {
                // check if the remote host closed the connection  
                AsyncUserToken token = (AsyncUserToken)e.UserToken;
                if (e.BytesTransferred > 0 && e.SocketError == SocketError.Success)
                {
                    //读取数据  
                    byte[] data = new byte[e.BytesTransferred];
                    Array.Copy(e.Buffer, e.Offset, data, 0, e.BytesTransferred);
                    //lock (token.Buffer)
                    //{
                    //    token.Buffer.AddRange(data);
                    //}
                    //注意:这里为什么要用do-while循环?   
                    //如果当客户发送大数据流的时候,e.BytesTransferred的大小就会比客户端发送过来的要小,  
                    //需要分多次接收.所以收到包的时候,先判断包头的大小.够一个完整的包再处理.  
                    //如果客户短时间内发送多个小数据包时, 服务器可能会一次性把他们全收了.  
                    //这样如果没有一个循环来控制,那么只会处理第一个包,  
                    //剩下的包全部留在token.Buffer中了,只有等下一个数据包过来后,才会放出一个来.  
                    //do
                    //{
                    //    //判断包的长度  
                    //    byte[] lenBytes = token.Buffer.GetRange(0, 4).ToArray();
                    //    int packageLen = BitConverter.ToInt32(lenBytes, 0);
                    //    if (packageLen > token.Buffer.Count - 4)
                    //    {   //长度不够时,退出循环,让程序继续接收  
                    //        break;
                    //    }

                    //    //包够长时,则提取出来,交给后面的程序去处理  
                    //    byte[] rev = token.Buffer.GetRange(4, packageLen).ToArray();
                    //    //从数据池中移除这组数据  
                    //    lock (token.Buffer)
                    //    {
                    //        token.Buffer.RemoveRange(0, packageLen + 4);
                    //    }
                    //    //将数据包交给后台处理,这里你也可以新开个线程来处理.加快速度.  
                    //    if (ReceiveClientData != null)
                    //        ReceiveClientData(token, rev);
                    //    //这里API处理完后,并没有返回结果,当然结果是要返回的,却不是在这里, 这里的代码只管接收.  
                    //    //若要返回结果,可在API处理中调用此类对象的SendMessage方法,统一打包发送.不要被微软的示例给迷惑了.  
                    //} while (token.Buffer.Count > 4);

                    //log(LogInfoType.INFO, string.Format("时间[{0}] 客户端({1}:{2}),接收到设备消息进行处理！",
                    //    token.ConnectTime.ToString(), token.IPAddress, token.Port.ToString()));

                    //log(LogInfoType.DEBG, string.Format("消息内容:\n{0}", System.Text.Encoding.Default.GetString(data)));

                    //将数据包交给后台处理,这里你也可以新开个线程来处理.加快速度.  
                    if (ReceiveClientData != null)
                    {
                        string gbk_str = System.Text.Encoding.UTF8.GetString(data);
                        ReceiveClientData(token, System.Text.Encoding.Default.GetBytes(gbk_str));
                    }
                    data = null;
                    //继续接收. 为什么要这么写,请看Socket.ReceiveAsync方法的说明  
                    if (!token.Socket.ReceiveAsync(e))
                        this.ProcessReceive(e);
                }
                else
                {
                    CloseClientSocket(e);
                }
            }
            catch (Exception xe)
            {
                log(LogInfoType.EROR,xe.Message + "\r\n" + xe.StackTrace);
            }
        }

        // This method is invoked when an asynchronous send operation completes.    
        // The method issues another receive on the socket to read any additional   
        // data sent from the client  
        //  
        // <param name="e"></param>  
        private void ProcessSend(SocketAsyncEventArgs e)
        {
            if (e.SocketError == SocketError.Success)
            {
                // done echoing data back to the client  
                AsyncUserToken token = (AsyncUserToken)e.UserToken;
                // read the next block of data send from the client  
                bool willRaiseEvent = token.Socket.ReceiveAsync(e);
                if (!willRaiseEvent)
                {
                    ProcessReceive(e);
                }
            }
            else
            {
                CloseClientSocket(e);
            }
        }

        //关闭客户端  
        private void CloseClientSocket(SocketAsyncEventArgs e)
        {
            AsyncUserToken token = e.UserToken as AsyncUserToken;

            lock (m_clients) { m_clients.Remove(token); }
            //如果有事件,则调用事件,发送客户端数量变化通知  
            //log(LogInfoType.EROR, string.Format("设备[{0}:{1}]TCP连接断开！当前连接数：{2}！",
            //        token.IPAddress.ToString(), token.Port, m_clients.Count));
            if (ClientNumberChange != null)
                ClientNumberChange(-1, token);
            // close the socket associated with the client  
            try
            {
                token.Socket.Shutdown(SocketShutdown.Send);
            }
            catch (Exception) { }
            token.Socket.Close();
            // decrement the counter keeping track of the total number of clients connected to the server  
            Interlocked.Decrement(ref m_clientCount);
            m_maxNumberAcceptedClients.Release();
            // Free the SocketAsyncEventArg so they can be reused by another client  
            e.UserToken = new AsyncUserToken();
            m_pool.Push(e);
        }

        public void CloseClientSocket(AsyncUserToken token)
        {
            lock (m_clients) { m_clients.Remove(token); }
            //如果有事件,则调用事件,发送客户端数量变化通知  
            //log(LogInfoType.EROR, string.Format("设备[{0}:{1}]TCP连接断开！当前连接数：{2}！",
            //        token.IPAddress.ToString(), token.Port, m_clients.Count));

            try
            {
                token.Socket.Shutdown(SocketShutdown.Send);
            }
            catch (Exception) { }
            token.Socket.Close();
            // decrement the counter keeping track of the total number of clients connected to the server  
            Interlocked.Decrement(ref m_clientCount);
        }


        private struct SendStruct
        {
            public AsyncUserToken token;
            public byte[] message;
        }

        private void BeginInvoke_SendMsg(object msg)
        {
            try
            {
                SendStruct sendStruct = (SendStruct)msg;
                sendStruct.token.Socket.Send(sendStruct.message);
            }
            catch (Exception e)
            {
                log(LogInfoType.EROR, "BeginInvoke_SendMsg:" + e.Message);
            }

            return;
        }
        /// <summary>  
        /// 对数据进行打包,然后再发送  
        /// </summary>  
        /// <param name="token"></param>  
        /// <param name="message"></param>  
        /// <returns></returns>  
        public void SendMessage(AsyncUserToken token, byte[] message)
        {
            if (token == null || token.Socket == null || !token.Socket.Connected)
            {
                log(LogInfoType.WARN,"未找到要发送的AP信息！");
                return;
            }
            if (message == null)
            {
                log(LogInfoType.EROR,"要发送的消息为NULL！");
                return;
            }

            try
            {
                //新建异步发送对象, 发送消息  
                //SocketAsyncEventArgs sendArg = new SocketAsyncEventArgs();
                //sendArg.SetBuffer(message, 0, message.Length);  //将数据放置进去.  
                //token.Socket.SendAsync(sendArg);
                string gbk_str = System.Text.Encoding.Default.GetString(message);
                byte[] utf8_byt = System.Text.Encoding.UTF8.GetBytes(gbk_str);
                //string gbk_str1 = System.Text.Encoding.UTF8.GetString(utf8_byt);
                SendStruct sendStruct;
                sendStruct.token = token;
                sendStruct.message = utf8_byt;
                ThreadPool.QueueUserWorkItem(new WaitCallback(BeginInvoke_SendMsg), sendStruct);

                //string str = string.Format("时间:[{0}] 客户端({1}:{2}),发送消息给设备成功！",
                //        token.ConnectTime.ToString(), token.IPAddress,token.Port.ToString(),
                //        System.Text.Encoding.Default.GetString(buff));
                //log(LogInfoType.INFO,str);

                //log(LogInfoType.DEBG, string.Format("消息内容:\n({0})",System.Text.Encoding.Default.GetString(buff)));
            }
            catch (Exception e)
            {
                log(LogInfoType.EROR,"SendMessage - Error:" + e.Message);
            }
        }
    }
}
