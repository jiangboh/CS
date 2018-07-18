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
        private static Dictionary<string, DateTime> dic;
        private static readonly object locker1 = new object();

        static ImsiRemoveDup()
        {
            //Console.WriteLine("\n\n ImsiRemoveDup构造 \n\n");
            dic = new Dictionary<string, DateTime>();

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
                    foreach (KeyValuePair<string, DateTime> kvp in dic)
                    {
                        //Console.WriteLine("Key = {0}, Value = {1}", kvp.Key, kvp.Value);
                        TimeSpan timeSpan = DateTime.Now - kvp.Value;
                        if (timeSpan.TotalMinutes > DataController.RemoveDupTimeLength)
                        {
                            RemovList.Add(kvp.Key);
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

        static public Dictionary<string, DateTime> GetDicList()
        {
            lock (locker1)
            {
                return dic;
            }
        }

        static public void GetDicList(ref Dictionary<string, DateTime> rDic)
        {
            lock (locker1)
            {
                foreach(KeyValuePair<string, DateTime> x in dic)
                {
                    rDic.Add(x.Key,x.Value);
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
                if (dic.ContainsKey(imsi))
                    return dic[imsi];
            }
            return DateTime.Now;
        }

        static public bool add(string imsi)
        {
            lock (locker1)
            {
                if (!dic.ContainsKey(imsi))
                {
                    dic.Add(imsi, DateTime.Now);
                    return true;
                }
            }
            return false;
        }

        static public bool del(string imsi)
        {
            lock (locker1)
            {
                if (dic.ContainsKey(imsi))
                {
                    dic.Remove(imsi);
                    return true;
                }
            }
            return false;
        }
        static public bool isExist(string imsi)
        {
            lock (locker1)
            {
                return dic.ContainsKey(imsi); ;
            }
        }
    }
}
