﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using static ScannerBackgrdServer.Common.MsgStruct;

namespace ScannerBackgrdServer.Common
{
    class Xml_codec
    {
        /// <summary>
        /// 日志打印。
        /// </summary>
        /// <param name="str"></param>
        public static void StaticOutputLog(LogInfoType type, string str, string modeName,
                                LogCategory category = LogCategory.I,
                                [CallerMemberName] string memberName = "",
                                [CallerFilePath] string filePath = "",
                                [CallerLineNumber] int lineNumber = 0)
        {
            if (type < DataController.LogOutputLevel) return;

            string outStr = string.Format("{0}", str);
            //Console.WriteLine(outStr);

            FrmMainController.add_log_info(type, outStr, modeName, category, filePath, memberName,lineNumber);
            Logger.Trace(type, outStr, modeName, category, memberName, filePath, lineNumber);

            outStr = null;
        }

        static private string[] ErrorCode = {
            "XML解析/封装成功！",
            "收到的数据不是XML格式！",
            "XML中未获取到消息ID或消息类型！",
            "XML中未获取到任何参数！",
            "键值对列表为空！",
            "键值对列表中没有元整！",
            "键值对列表中元素格式不正确！",
            "创建Xml消息出错！",
        };
        
        static UInt32 index = 0;

        /// <summary>
        /// 通过错误编号返回错误描述
        /// </summary>
        /// <param name="errorno">错误编号</param>
        /// <returns></returns>
        static public string getErrorMsg(int errorno)
        {
            return ErrorCode[System.Math.Abs(errorno)].ToString();
        }

        static private XmlNode Get_Node_By_NodeName(XmlNodeList listNodes, String NodeName)
        {
            if (listNodes == null) return null;
            if (string.IsNullOrEmpty(NodeName)) return null;

            foreach (XmlNode node in listNodes)
            {
                if (String.Compare(node.Name, NodeName, true) == 0)
                {
                    return node;
                }
            }
            return null;
        }


        static private XmlNode getCheildNode(XmlElement xmlElement,String str)
        {
            XmlNodeList nodes = xmlElement.ChildNodes;
            if (nodes == null) return null;

            foreach (XmlNode node in nodes)
            {
                if (str.Equals(node.Name))
                    return node;
            }
            return null;
        }

        static private void addDicNodes(XmlDocument myXmlDoc, Dictionary<string, object> dic,XmlElement levelType)
        {
            foreach (KeyValuePair<string, object> kvp in dic)
            {
                if (Convert.ToString(kvp.Value) == MsgStruct.AllNum) continue;

                string[] nameList = kvp.Key.Split('/');
                if (nameList.Count() < 1)
                {
                    return;
                }
                XmlElement levelParent = levelType;
                Boolean isFindNode = true;
                for (int i = 0; i < nameList.Count() - 1; i++)  //添加子节点
                {
                    if (isFindNode)
                    {
                        XmlNode selectNode = getCheildNode(levelParent, nameList[i].ToString());
                        if (null == selectNode)
                        {
                            isFindNode = false; //上层节点没找到，不用再找下层节点了。
                        }
                        else
                        {
                            levelParent = (XmlElement)selectNode;
                            continue;
                        }
                    }
                    XmlElement levelAdd = myXmlDoc.CreateElement(nameList[i].ToString());
                    levelParent.AppendChild(levelAdd);
                    levelParent = levelAdd;

                }
                XmlElement level = myXmlDoc.CreateElement(nameList[nameList.Count() - 1].ToString());
                level.InnerText = Convert.ToString(kvp.Value);
                levelParent.AppendChild(level);
            }
        }
        /// <summary>
        /// 将键值对封装成发给AP的xml消息
        /// </summary>
        /// <param name="KeyValueList">键值对列表</param>
        /// <returns>封装后的xml消息</returns>
        static public string EncodeApXmlMessage(UInt16 id, Msg_Body_Struct TypeKeyValue)
        {
            //初始化一个xml实例
            XmlDocument myXmlDoc = new XmlDocument();

            try
            {
                ////加入XML的声明段落,<?xml version="1.0" encoding="UTF-8"?>
                XmlDeclaration rootElement = myXmlDoc.CreateXmlDeclaration("1.0", "UTF-8", null);

                //创建xml的根节点
                myXmlDoc.AppendChild(rootElement);

                //初始化第一层节点:message_content
                XmlElement levelElement1 = myXmlDoc.CreateElement("message_content");
                myXmlDoc.AppendChild(levelElement1);

                //添加消息id
                XmlElement levelId = myXmlDoc.CreateElement("id");
                levelId.InnerText = id.ToString();
                levelElement1.AppendChild(levelId);

                //初始化第二层节点（消息类型）
                XmlElement levelType = myXmlDoc.CreateElement(TypeKeyValue.type);
                levelElement1.AppendChild(levelType);

                //添加属性字段
                addDicNodes(myXmlDoc, TypeKeyValue.dic, levelType);
       
                if (TypeKeyValue.n_dic != null)
                {
                    foreach (Name_DIC_Struct x in TypeKeyValue.n_dic)
                    {
                        addDicNodes(myXmlDoc,x.dic, levelType);
                    }
                }
                //将xml文件保存到指定的路径下
                //myXmlDoc.Save("d://data2.xml");

                //MemoryStream ms = new MemoryStream();
                //myXmlDoc.Save(ms);
                //byte[] data = ms.ToArray();
                //return data;

                return ConvertXmlToString(myXmlDoc);                
            }
            catch (Exception ex)
            {
                Logger.Trace(LogInfoType.EROR, "封装XML出错。出错原因：" + ex.ToString(), "XML", LogCategory.I);
                return null;
            }
        }

        #region 将XmlDocument转化为string
        /// <summary>
        /// 将XmlDocument转化为string
        /// </summary>
        /// <param name="xmlDoc"></param>
        /// <returns></returns>
        public static string ConvertXmlToString(XmlDocument xmlDoc)
        {
            MemoryStream stream = new MemoryStream();
            XmlTextWriter writer = new XmlTextWriter(stream, null);
            writer.Formatting = Formatting.Indented;
            xmlDoc.Save(writer);
            StreamReader sr = new StreamReader(stream, System.Text.Encoding.UTF8);
            stream.Position = 0;
            string xmlString = sr.ReadToEnd();
            sr.Close();
            stream.Close();
            return xmlString;
        }
        #endregion

        static private void GetAllKeyNodes (string keyName,XmlNode node, ref Dictionary<string, object> KeyValueList)
        {
            string name;
            
            if (node.HasChildNodes && node.ChildNodes[0].Name != "#text")
            {
                foreach (XmlNode x in node.ChildNodes)
                {
                    if (string.IsNullOrEmpty(keyName))
                    {
                        name = x.Name;
                    }
                    else
                    {
                        name = string.Format("{0}/{1}", keyName, x.Name);
                    }
                    GetAllKeyNodes(name,x,ref KeyValueList);
                }
            }
            else
            {
                //Dictionary<string, object> KeyValue = new Dictionary<string, object>();
                //KeyValue.Add(keyName, node.InnerText);
                if (KeyValueList.ContainsKey(keyName))
                {
                    if (index == UInt32.MaxValue)
                    {
                        index = 0;
                    }
                    index++;
                    keyName = string.Format("{0}_#{1}#", keyName, index);
                }
                KeyValueList.Add(keyName, node.InnerText);
            }

            return ;
        }

        /// <summary>
        /// 解析收到的xml消息
        /// </summary>
        /// <param name="msg">xml消息</param>
        /// <returns>解析后的结构体</returns>
        static public Msg_Body_Struct DecodeApXmlMessage(String msg,ref UInt16 msgId)
        {
            Msg_Body_Struct TypeKeyValueList = new Msg_Body_Struct();
            //List<Key_Value_List_Struct> dic = new List<Key_Value_List_Struct>();
            //Key_Value_Struct KeyValue = new Key_Value_Struct();

            XmlDocument xmlDoc = new XmlDocument();
            XmlElement root = null;
            try
            {
                xmlDoc.LoadXml(msg);
                root = xmlDoc.DocumentElement;//取到根结点
            }
            catch (Exception)
            {
                StaticOutputLog(LogInfoType.EROR,"加载Xml消息结构出错。","XML");
                return null;
            }

            XmlNode FirstNode = null;
            FirstNode = root.FirstChild;
            if (FirstNode == null)
            {
                StaticOutputLog(LogInfoType.EROR, "获取Xml消息中第一个子节点出错。", "XML");
                return null;
            }

            XmlNode LastNode = null;
            LastNode = FirstNode.NextSibling;
            if (LastNode == null)
            {
                StaticOutputLog(LogInfoType.EROR, "获取Xml消息中第二个子节点出错。", "XML");
                return null;
            }

            XmlNode idNode;
            XmlNode MsgTypeNode;
            if (FirstNode.Name.ToLower() == "id")
            {
                idNode = FirstNode;
                MsgTypeNode = LastNode;
            }
            else
            {
                idNode = LastNode;
                MsgTypeNode = FirstNode;
            }
                  
            msgId = Convert.ToUInt16(idNode.InnerText,10);

            Msg_Body_Struct TypeKeyValue = new Msg_Body_Struct();

            TypeKeyValue.type = MsgTypeNode.Name;

            Dictionary<string, object> KeyValueList = new Dictionary<string, object>();
            GetAllKeyNodes(null, MsgTypeNode, ref KeyValueList);

            //KeyValueList.Add(MsgStruct.AllNum, KeyValueList.Count + 1);
            TypeKeyValue.dic = KeyValueList;

            TypeKeyValueList= TypeKeyValue;
            
            return TypeKeyValueList;
        }

    }
}
