using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ScannerBackgrdServer.ApController
{
    class ImsiRemoveDup
    {
        public struct IMSI
        {
            public string imsi;
            public DateTime time;

            public IMSI(string imsi, DateTime time)
            {
                this.imsi = imsi;
                this.time = time;
            }
        }

        private static HashSet<IMSI> dic;
        private static readonly object locker1 = new object();

        static ImsiRemoveDup()
        {
            //Console.WriteLine("\n\n ImsiRemoveDup构造 \n\n");
            dic = new HashSet<IMSI>();

            Thread t = new Thread(new ThreadStart(CheckImsiList));
            t.Start();
            t.IsBackground = true;
        }

        static private void CheckImsiList()
        {
            HashSet<string> RemovList = new HashSet<string>();

            while (true)
            {
                //去重功能关闭
                if (DataController.RemoveDupMode != 1)
                {
                    Thread.Sleep(3000);
                    continue;
                }

                lock (locker1)
                {
                    foreach (IMSI kvp in dic)
                    {
                        //Console.WriteLine("Key = {0}, Value = {1}", kvp.Key, kvp.Value);
                        TimeSpan timeSpan = DateTime.Now - kvp.time;
                        if (timeSpan.TotalMinutes > DataController.RemoveDupTimeLength)
                        {
                            RemovList.Add(kvp.imsi);
                        }
                    }
                }

                //删除超时的imsi
                foreach (string x in RemovList)
                {
                    del(x);
                }

                //Console.WriteLine("\n\n当前缓存Imsi数量: {0}\n\n", GetCount());
                RemovList.Clear();
                RemovList.TrimExcess();

                Thread.Sleep(3000);
            }
        }

        static public HashSet<IMSI> GetDicList()
        {
            lock (locker1)
            {
                return dic;
            }
        }

        static public void GetDicList(ref HashSet<IMSI> rDic)
        {
            lock (locker1)
            {
                foreach(IMSI x in dic)
                {
                    rDic.Add(x);
                }
            }
            return;
        }

        static public int GetCount()
        {
            lock (locker1)
            {
                return dic.Count;
            }
        }

        static public DateTime GetTime(string imsi)
        {
            lock (locker1)
            {
                foreach (IMSI x in dic)
                {
                    if (String.Compare(x.imsi, imsi, true) == 0)
                    {
                        return x.time;
                    }
                }
            }
            return DateTime.Now;
        }

        static public bool add(string imsi)
        {
            if (isExist(imsi))
                return true;

            lock (locker1)
            {
                IMSI x = new IMSI(imsi, DateTime.Now);
                return dic.Add(x);             
            }
        }

        static public bool del(string imsi)
        {

            lock (locker1)
            {
                foreach (IMSI x in dic)
                {
                    if (String.Compare(x.imsi, imsi, true) == 0)
                    {
                        dic.Remove(x);
                        dic.TrimExcess();

                        return true;
                    }
                }
            }
            return false;
        }

        static public bool isExist(string imsi)
        {
                lock (locker1)
                {
                    foreach (IMSI x in dic)
                    {
                        if (String.Compare(x.imsi, imsi, true) == 0)
                        {
                            return true;
                        }
                    }
                }
                return false;
        }
    }
}
