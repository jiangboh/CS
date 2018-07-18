//using Microsoft.Analytics.Interfaces;
//using Microsoft.Analytics.Types.Sql;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace ScannerBackgrdServer
{
    class test
    {
        private MessageDelegate sendMsgFunc1 = new MessageDelegate(FrmMainController.MessageDelegate_For_ApController);

        private MessageDelegate sendMsgFunc2 = new MessageDelegate(FrmMainController.MessageDelegate_For_AppController);

        public test()
        {
            //通过ParameterizedThreadStart创建线程
            Thread thread1 = new Thread(new ParameterizedThreadStart(thread_for_test));

            //给方法传值
            thread1.Start("this is elephant speaking!\n");
            thread1.IsBackground = true;
        }

        private void thread_for_test(object obj)
        {
            MessageType mt;
            MessageBody mb = new MessageBody ();

            while (true)
            {
                for (int i = 0; i < 50; i++)
                {
                    if ((i % 6) == 0)
                    {
                        mt = MessageType.MSG_STRING;
                        mb.bString = "MessageType.MSG_STRING";

                        sendMsgFunc1(mt, mb);
                        Thread.Sleep(10000);

                        sendMsgFunc2(mt, mb);
                        Thread.Sleep(10000);
                    }
                    else if ((i % 6) == 1)
                    {
                        mt = MessageType.MSG_INT;
                        mb.bInt = 5;

                        sendMsgFunc1(mt, mb);
                        Thread.Sleep(10000);

                        sendMsgFunc2(mt, mb);
                        Thread.Sleep(10000);
                    }
                    else if ((i % 6) == 2)
                    {
                        mt = MessageType.MSG_DOUBLE;
                        mb.bDouble = 3.14;

                        sendMsgFunc1(mt, mb);
                        Thread.Sleep(10000);

                        sendMsgFunc2(mt, mb);
                        Thread.Sleep(10000);
                    }
                    else if ((i % 6) == 3)
                    {
                        mt = MessageType.MSG_DATATABLE;
                        mb.bDataTable = new System.Data.DataTable();

                        sendMsgFunc1(mt, mb);
                        Thread.Sleep(10000);

                        sendMsgFunc2(mt, mb);
                        Thread.Sleep(10000);
                    }
                    else if ((i % 6) == 4)
                    {
                        mt = MessageType.MSG_XML;
                        mb.bXml = "MessageType.MSG_XML";

                        sendMsgFunc1(mt, mb);
                        Thread.Sleep(10000);

                        sendMsgFunc2(mt, mb);
                        Thread.Sleep(10000);
                    }
                    else if ((i % 6) == 5)
                    {
                        mt = MessageType.MSG_STATUS;
                        mb.bStatus = -123456;

                        sendMsgFunc1(mt, mb);
                        Thread.Sleep(10000);

                        sendMsgFunc2(mt, mb);
                        Thread.Sleep(10000);
                    }                    
                }                                
            }
        }
    }
}