using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
namespace ScannerBackgrdServer.Common
{
    class CodeConver
    {
        #region 公共转换及编码函数
        /// <summary>
        /// 字符转为Ascii码值
        /// </summary>
        /// <param name="character">字符</param>
        /// <returns>Ascii值</returns>
        public static int Str2Asc(string character)
        {
            if (character.Length == 1)
            {
                System.Text.ASCIIEncoding asciiEncoding = new System.Text.ASCIIEncoding();
                int intAsciiCode = (int)asciiEncoding.GetBytes(character)[0];
                return (intAsciiCode);
            }
            else
            {
                throw new Exception("Character is not valid.");
            }
        }
        /// <summary>
        /// 将Ascii字符串转为正常字符串
        /// </summary>
        /// <param name="Astr">Ascii字符串</param>
        /// <returns>正常字符串</returns>
        public static string AscStr2str(string AStr)
        {
            string str=string.Empty;
            for (int i =0;i<AStr.Length;i+=2)
            {
                int intAsciiCode = Convert.ToInt32(AStr.Substring(i,2),16);
                str = string.Format("{0}{1}",str, intAsciiCode-48);
            }
            return str;
        }
        /// <summary>
        /// 初始化字节数组
        /// </summary>
        /// <param name="Bytes"></param>
        public static void IniteByteArray(byte[] Bytes)
        {
            for (int i = 0; i < Bytes.Length; i++)
            {
                Bytes[i] = 0;
            }
        }
        /// <summary>
        /// 字符串转Unicode
        /// </summary>
        /// <param name="source">源字符串</param>
        /// <param name="isLittleEndian">小端字节序</param>
        /// <returns>Unicode编码后的字符串</returns>
        public static string String2Unicode(string source,bool isLittleEndian)
        {
            var bytes = Encoding.Unicode.GetBytes(source);
            var stringBuilder = new StringBuilder();
            for (var i = 0; i < bytes.Length; i += 2)
            {
                if (isLittleEndian)
                    stringBuilder.AppendFormat("{0:x2}{1:x2}", bytes[i], bytes[i + 1]);
                else
                    stringBuilder.AppendFormat("{0:x2}{1:x2}", bytes[i+1], bytes[i]);
            }
            return stringBuilder.ToString();
        }
        /// <summary>
        /// 字符串转Unicode（小端字节序）
        /// </summary>
        /// <param name="source">源字符串</param>
        /// <returns>Unicode编码后的字符串</returns>
        public static string String2Unicode(string source)
        {
            return String2Unicode(source,true);
        }
        /// <summary>  
        /// Unicode字符串转为正常字符串  
        /// </summary>  
        /// <param name="srcText"></param>  
        /// <param name="isLittleEndian">小端字节序</param>
        /// <returns></returns>  
        public static string Unicode2String(string srcText,bool isLittleEndian)
        {
            string dst = "";
            string src = srcText;
            int len = srcText.Length / 4;
            for (int i = 0; i <= len - 1; i++)
            {
                string str = "";
                str = src.Substring(0, 4);
                src = src.Substring(4);
                byte[] bytes = new byte[2];
                if (isLittleEndian)
                {
                    bytes[1] = byte.Parse(int.Parse(str.Substring(2, 2), System.Globalization.NumberStyles.HexNumber).ToString());
                    bytes[0] = byte.Parse(int.Parse(str.Substring(0, 2), System.Globalization.NumberStyles.HexNumber).ToString());
                }
                else
                {
                    bytes[1] = byte.Parse(int.Parse(str.Substring(0, 2), System.Globalization.NumberStyles.HexNumber).ToString());
                    bytes[0] = byte.Parse(int.Parse(str.Substring(2, 2), System.Globalization.NumberStyles.HexNumber).ToString());
                }
                dst += Encoding.Unicode.GetString(bytes);
            }
            return dst;
        }
        public static string Unicode2String(string srcText)
        {
            return Unicode2String(srcText, true);
        }
        public static string String2HexString(string str)
        {
            string hexOutput = string.Empty;
            char[] values = str.ToCharArray();
            foreach (char letter in values)
            {
                // Get the integral value of the character.
                int value = Convert.ToInt32(letter);
                // Convert the decimal value to a hexadecimal value in string form.
                hexOutput = String.Format("{0}{1:X}", hexOutput, value);
            }
            return hexOutput;
        }
        /// <summary>
        /// 将byte[] 转为16进制字符串
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static string byteToHexStr(byte[] bytes)
        {
            string returnStr = "";
            if (bytes != null)
            {
                for (int i = 0; i < bytes.Length; i++)
                {
                    returnStr += bytes[i].ToString("X").PadLeft(2, '0');
                }
            }
            return returnStr;
        }
        /// <summary>
        /// 将16进制的字符串转为byte[]
        /// </summary>
        /// <param name="hexString"></param>
        /// <returns></returns>
        public static byte[] strToToHexByte(string hexString)
        {
            hexString = hexString.Replace(" ", "");
            if ((hexString.Length % 2) != 0)
                hexString += " ";
            byte[] returnBytes = new byte[hexString.Length / 2];
            for (int i = 0; i < returnBytes.Length; i++)
                returnBytes[i] = Convert.ToByte(hexString.Substring(i * 2, 2), 16);
            return returnBytes;
        }
        /// <summary>
        /// 将16进制的字符串转为byte[](奇偶位均可)
        /// </summary>
        /// <param name="hexString"></param>
        /// <returns></returns>
        public static byte[] strToToHexByte(string hexString, int hexStringLengh)
        {
            hexString = hexString.Replace(" ", "");
            byte[] ByteData = null;
            if (hexStringLengh % 2 != 0)
            {
                hexString += "0";
                ByteData = new byte[hexStringLengh];
            }
            byte[] returnBytes = new byte[hexString.Length / 2];
            for (int i = 0; i < returnBytes.Length; i++)
                returnBytes[i] = Convert.ToByte(hexString.Substring(i * 2, 2), 16);
            Array.Copy(returnBytes, ByteData, ByteData.Length);
            return ByteData;
        }
        /// <summary>
        /// GSM 7-Bit编码压缩
        /// </summary>
        /// <param name="Bit7Array">7-Bit编码字节序列</param>
        /// <param name="UDHL">用户数据头字节数</param>
        /// <returns>编码后的字符串</returns>
        public static string Encode7Bit(Byte[] Bit7Array, Int32 UDHL)
        {
            byte[] code128 = {  0x1b, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0x0a, 0xff, 0xff, 0x0d, 0xff, 0xff,
                            0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
                            0x20, 0x21, 0x22, 0x23, 0x02, 0x25, 0x26, 0x27, 0x28, 0x29, 0x2a, 0x2b, 0x2c, 0x2d, 0x2e, 0x2f,
                            0x30, 0x31, 0x32, 0x33, 0x34, 0x35, 0x36, 0x37, 0x38, 0x39, 0x3a, 0x3b, 0x3c, 0x3d, 0x3e, 0x3f,
                            0x00, 0x41, 0x42, 0x43, 0x44, 0x45, 0x46, 0x47, 0x48, 0x49, 0x4a, 0x4b, 0x4c, 0x4d, 0x4e, 0x4f,
                            0x50, 0x51, 0x52, 0x53, 0x54, 0x55, 0x56, 0x57, 0x58, 0x59, 0x5a, 0xff, 0xff, 0xff, 0xff, 0x11,
                            0xff, 0x61, 0x62, 0x63, 0x64, 0x65, 0x66, 0x67, 0x68, 0x69, 0x6a, 0x6b, 0x6c, 0x6d, 0x6e, 0x6f,
                            0x70, 0x71, 0x72, 0x73, 0x74, 0x75, 0x76, 0x77, 0x78, 0x79, 0x7a, 0xff, 0xff, 0xff, 0xff, 0xff};
            byte[] code256 = {  0x1b, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0x0a, 0xff, 0xff, 0x0d, 0xff, 0xff,
                                0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
                                0x20, 0x21, 0x22, 0x23, 0x02, 0x25, 0x26, 0x27, 0x28, 0x29, 0x2a, 0x2b, 0x2c, 0x2d, 0x2e, 0x2f,
                                0x30, 0x31, 0x32, 0x33, 0x34, 0x35, 0x36, 0x37, 0x38, 0x39, 0x3a, 0x3b, 0x3c, 0x3d, 0x3e, 0x3f,
                                0x00, 0x41, 0x42, 0x43, 0x44, 0x45, 0x46, 0x47, 0x48, 0x49, 0x4a, 0x4b, 0x4c, 0x4d, 0x4e, 0x4f,
                                0x50, 0x51, 0x52, 0x53, 0x54, 0x55, 0x56, 0x57, 0x58, 0x59, 0x5a, 0xff, 0xff, 0xff, 0xff, 0x11,
                                0xff, 0x61, 0x62, 0x63, 0x64, 0x65, 0x66, 0x67, 0x68, 0x69, 0x6a, 0x6b, 0x6c, 0x6d, 0x6e, 0x6f,
                                0x70, 0x71, 0x72, 0x73, 0x74, 0x75, 0x76, 0x77, 0x78, 0x79, 0x7a, 0xff, 0xff, 0xff, 0xff, 0xff,
                                0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
                                0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
                                0xff, 0x40, 0xff, 0x01, 0x24, 0x03, 0xff, 0x5f, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
                                0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0x60,
                                0xff, 0xff, 0xff, 0xff, 0x5b, 0x0e, 0x1c, 0x09, 0xff, 0x1f, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
                                0xff, 0x5d, 0xff, 0xff, 0xff, 0xff, 0x5c, 0xff, 0x0b, 0xff, 0xff, 0xff, 0x5e, 0xff, 0xff, 0x1e,
                                0x7f, 0xff, 0xff, 0xff, 0x7b, 0x0f, 0x1d, 0xff, 0x04, 0x05, 0xff, 0xff, 0x07, 0xff, 0xff, 0xff,
                                0xff, 0x7d, 0x08, 0xff, 0xff, 0xff, 0x7c, 0xff, 0x0c, 0x06, 0xff, 0xff, 0x7e, 0xff, 0xff, 0xff};
            // 7Bit对齐需要的填充位
            Int32 FillBits = (UDHL * 8 + 6) / 7 * 7 - (UDHL * 8);
            // 压缩字节数
            Int32 Len = Bit7Array.Length;
            Int32 PackLen = (Len * 7 + FillBits + 7) / 8;
            StringBuilder code = new StringBuilder(PackLen << 1);
            Int32 Remainder = 0;
            // 每8个字节压缩成7个字节
            for (Int32 i = 0; i < Len; i++)
            {
                Int32 CharValue = code256[Bit7Array[i]];
                Int32 Index = (i + 8 - FillBits) % 8;
                if (Index == 0)
                {
                    Remainder = CharValue;
                }
                else
                {
                    Int32 n = ((CharValue << (8 - Index)) | Remainder) & 0xFF;
                    code.Append(n.ToString("X2"));
                    Remainder = CharValue >> Index;
                }
            }
            // 写入剩余数据
            if (((Len * 7 + FillBits) % 8) != 0)
            {
                code.Append(Remainder.ToString("X2"));
            }
            return code.ToString();
        }
        /// <summary>
        /// UCS2编码
        /// </summary>
        /// <param name="src"> UTF-16BE编码的源串</param>
        /// <returns>编码后的UCS2串 </returns>
        public static string EncodeUCS2(string src)
        {
            StringBuilder builer = new StringBuilder();
            builer.Append("000800");
            byte[] tmpSmsText = Encoding.Unicode.GetBytes(src);
            builer.Append(tmpSmsText.Length.ToString("X2"));    //正文内容长度
            for (int i = 0; i < tmpSmsText.Length; i += 2)      //高低字节对调 
            {
                builer.Append(tmpSmsText[i + 1].ToString("X2"));//("X2")转为16进制
                builer.Append(tmpSmsText[i].ToString("X2"));
            }
            builer = builer.Remove(0, 8);
            return builer.ToString();
        }
        public static string Decode7Bit(string userData)
        {
            string result = string.Empty;
            byte[] b = new byte[100];
            string temp = string.Empty;
            for (int i = 0; i < userData.Length; i += 2)
            {
                b[i / 2] = (byte)Convert.ToByte((userData[i].ToString() + userData[i + 1].ToString()), 16);
                //b[i / 2] = (byte)Convert.ToByte((userData[i + 1].ToString() + userData[i].ToString()), 16);
            }
            int j = 0;            //while计数
            int tmp = 1;            //temp中二进制字符字符个数
            while (j < userData.Length / 2 - 1)
            {
                string s = string.Empty;
                s = Convert.ToString(b[j], 2);
                while (s.Length < 8)            //s补满8位 byte转化来的 有的不足8位，直接解码将导致错误
                {
                    s = "0" + s;
                }
                result += (char)Convert.ToInt32(s.Substring(tmp) + temp, 2);        //加入一个字符 结果集 temp 上一位组剩余
                temp = s.Substring(0, tmp);             //前一位组多的部分
                if (tmp > 6)                            //多余的部分满7位，加入一个字符
                {
                    result += (char)Convert.ToInt32(temp, 2);
                    temp = string.Empty;
                    tmp = 0;
                }
                tmp++;
                j++;
                if (j == userData.Length / 2 - 1)           //最后一个字符
                {
                    result += (char)Convert.ToInt32(Convert.ToString(b[j], 2) + temp, 2);
                }
            }
            return result;
        }
        #endregion
    }
    
        /// <summary>
        /// PDU格式短信解码部分
        /// 接口函数：PDUDecoding
        /// 注意：不支持文本压缩短信
        /// </summary>
        public partial class SMS
        {
            /// <summary>
            /// 信息元素结构体，包含信息元素标识和信息元素数据
            /// </summary>
            public struct PDUUDH
            {
                public Byte IEI;    // 信息元素标识（Information Element Identifier）
                public Byte[] IED;  // 信息元素数据（Information Element Data）
            }
            /// <summary>
            /// 编码方案对应的最大用户数据长度
            /// </summary>
            private enum EnumUDL
            {
                BIT7UDL = 160,  // 7Bit编码允许的最大字符数
                BIT8UDL = 140,  // 8Bit编码允许的最大字节数
                UCS2UDL = 70    // UCS2编码允许的最大字符数
            }
            /// <summary>
            /// 短信结构体
            /// </summary>
            public struct SMSPARTS
                {
                    public String SCA;          // 服务中心地址（Service Center Address）
                    public String OA;           // 发送方地址（Originator Adress）            
                    public DateTime SCTS;       // 服务中心的时间戳（Service Center Time Stamp）
                    public PDUUDH[] UDH;        // 用户数据头（User Data Header）
                    public Object UD;           // 用户数据（User Data）
                    // PDU Type 协议数据单元类型
                    public Boolean RP;          // 应答路径（Reply Path）
                    public Boolean UDHI;        // 用户数据头标识（User Data Header Indicator）
                    public Boolean SRI;         // 状态报告指示（Status Report Indication）
                    public Boolean MMS;         // 更多信息发送（More Messages to Send）
                    public Int32 MTI;           // 信息类型指示（Message Type Indicator）
                    // PID协议标识
                    public Byte PID;            // PID协议标识（Protocol Identifier）
                    // DCS数据编码方案
                    public EnumDCS DCS;  // 数据编码方案（Data Coding Scheme）
                    public Boolean TC;  // 文本压缩指示 0-文本未压缩 1-文本用GSM标准压缩算法压缩
                    public Int32 MC;    // 消息类型（Message Class）-1：无 0：立即显示 1：移动设备特定类型 2：SIM特定类型 3：终端设备特定类型
                }
            /// <summary>
            /// 短信解码
            /// </summary>
            /// <param name="data">数据报文</param>
            /// <returns>短信信息</returns>
            /// <remarks>
            /// 接收方PDU格式（SMS-DELIVER-PDU）
            /// SCA（Service Center Adress）：短信中心，长度1-12
            /// PDU-Type（Protocol Data Unit Type）：协议数据单元类型，长度1
            /// OA（Originator Adress）：发送方SME的地址
            /// PID（Protocol Identifier）：协议标识，长度1
            /// DCS（Data Coding Scheme）：编码方案，长度1
            /// SCTS（Service Center Time Stamp）：服务中心时间戳，长度7
            /// UDL（User Data Length）：用户数据段长度，长度1
            /// UD（User Data）：用户数据，长度0-140
            /// </remarks>
            public static SMSPARTS PDUDecoding(String data)
            {
                SMSPARTS Parts;
                Int32 EndIndex;
                // 短信中心
                Parts.SCA = SCADecoding(data, out EndIndex);
                //TPDU Len
                EndIndex += 2; 
                // 协议数据单元类型
                Int32 PDUType = Convert.ToInt32(data.Substring(EndIndex, 2), 16);
                Parts.RP = PDUType.BitTest(7);      // 应答路径
                Parts.UDHI = PDUType.BitTest(6);    // 用户数据头标识
                Parts.SRI = PDUType.BitTest(5);     // 状态报告指示
                Parts.MMS = !PDUType.BitTest(2);    // 更多信息发送  
                Parts.MTI = PDUType & 3;            // 信息类型指示
                EndIndex += 2;
                //MR
                EndIndex += 2;
                // 发送方SME的地址
                Parts.OA = OADecoding(data, EndIndex, out EndIndex);
                // 协议标识
                Parts.PID = Convert.ToByte(data.Substring(EndIndex, 2), 16);
                EndIndex += 2;
                // 数据编码方案
                Int32 DCSType = Convert.ToInt32(data.Substring(EndIndex, 2), 16);
                EndIndex += 2;
                Parts.TC = DCSType.BitTest(5);  // 文本压缩指示
                Parts.DCS = (EnumDCS)((DCSType >> 2) & 3);  // 编码字符集
                if (DCSType.BitTest(4))
                {   // 信息类型信息 0：立即显示 1：移动设备特定类型 2：SIM特定类型 3：终端设备特定类型
                    Parts.MC = DCSType & 3;
                }
                else
                {   // 不含信息类型信息
                    Parts.MC = -1;
                }
                // 服务中心时间戳（BCD编码）
                //Parts.SCTS = SCTSDecoding(data, EndIndex);
                //EndIndex += 14;
                Parts.SCTS = System.DateTime.Now;
                // 用户数据头
                if (Parts.UDHI)
                {
                    Parts.UDH = UDHDecoding(data, EndIndex + 2);
                }
                else
                {
                    Parts.UDH = null;
                }
                // 用户数据
                Parts.UD = UserDataDecoding(data, EndIndex, Parts.UDHI, Parts.DCS);
                return Parts;
            }
            /// <summary>
            /// 用户数据解码
            /// </summary>
            /// <param name="data">编码字符串</param>
            /// <param name="Index">起始索引号</param>
            /// <param name="UDHI">用户数据头标识</param>
            /// <param name="DCS">编码方案</param>
            /// <returns>
            /// String类型：文本内容
            /// Byte[]类型：二进制内容
            /// </returns>
            private static Object UserDataDecoding(String data, Int32 Index, Boolean UDHI = false, EnumDCS DCS = EnumDCS.UCS2)
            {
                // 用户数据区长度
                Int32 UDL = Convert.ToInt32(data.Substring(Index, 2), 16);
                Index += 2;
                // 跳过用户数据头
                Int32 UDHL = 0;
                if (UDHI)
                {   // 用户数据头长度
                    UDHL = Convert.ToInt32(data.Substring(Index, 2), 16);
                    UDHL++;
                    Index += UDHL << 1;
                }
                // 获取用户数据
                if (DCS == EnumDCS.UCS2)
                {   // 获取字符个数
                    Int32 CharNumber = (UDL - UDHL) >> 1;
                    StringBuilder sb = new StringBuilder(CharNumber);
                    for (Int32 i = 0; i < CharNumber; i++)
                    {
                        sb.Append(Convert.ToChar(Convert.ToInt32(data.Substring((i << 2) + Index, 4), 16)));
                    }
                    return sb;
                }
                else if (DCS == EnumDCS.BIT7)
                {
                    Int32 Septets = UDL - (UDHL * 8 + 6) / 7;   // 7-Bit编码字符数
                    Int32 FillBits = (UDHL * 8 + 6) / 7 * 7 - UDHL * 8; // 填充位数
                    return BIT7Decoding(BIT7Unpack(data, Index, Septets, FillBits));
                }
                else
                {   // 8Bit编码
                    // 获取数据长度
                    UDL -= UDHL;
                    Byte[] Binary = new Byte[UDL];
                    for (Int32 i = 0; i < UDL; i++)
                    {
                        Binary[i] = Convert.ToByte(data.Substring((i << 1) + Index, 2), 16);
                    }
                    return Binary;
                }
            }
            /// <summary>
            /// 服务中心地址解码
            /// </summary>
            /// <param name="data">编码字符串</param>
            /// <param name="EndIndex">输出：结束索引位置</param>
            /// <returns>服务中心地址</returns>
            private static String SCADecoding(String data, out Int32 EndIndex)
            {
                // 获取地址长度
                Int32 Len = Convert.ToInt32(data.Substring(0, 2), 16);
                if (Len == 0)
                {
                    EndIndex = 2;
                    return String.Empty;
                }
                StringBuilder sb = new StringBuilder(Len << 1);
                // 服务中心地址类型
                if (data.Substring(2, 2) == "91")
                {   // 国际号码
                    sb.Append("+");
                }
                // 服务中心地址
                EndIndex = (Len + 1) << 1;
                for (Int32 i = 4; i < EndIndex; i += 2)
                {
                    sb.Append(data[i + 1]);
                    sb.Append(data[i]);
                }
                // 去掉填充字符
                if (sb[sb.Length - 1] == 'F')
                {
                    sb.Remove(sb.Length - 1, 1);
                }
                return sb.ToString();
            }
            /// <summary>
            /// 发送方地址解码
            /// </summary>
            /// <param name="data">编码字符串</param>
            /// <param name="Index">起始索引位置</param>
            /// <param name="EndIndex">输出：结束索引位置</param>
            /// <returns>发送方地址</returns>
            private static String OADecoding(String data, Int32 Index, out Int32 EndIndex)
            {
                // 获取号码长度
                Int32 Len = Convert.ToInt32(data.Substring(Index, 2), 16);
                if (Len == 0)
                {
                    EndIndex = Index + 2;
                    return String.Empty;
                }
                StringBuilder sb = new StringBuilder(Len + 1);
                if (data.Substring(Index + 2, 2) == "91")
                {   // 国际号码
                    sb.Append("+");
                }
                // 电话号码
                for (Int32 i = 0; i < Len; i += 2)
                {
                    sb.Append(data[Index + i + 5]);
                    sb.Append(data[Index + i + 4]);
                }
                EndIndex = Index + 4 + Len;
                if (Len % 2 != 0)
                {   // 去掉填充字符
                    sb.Remove(sb.Length - 1, 1);
                    EndIndex++;
                }
                return sb.ToString();
            }
            /// <summary>
            /// 7-Bit编码解压缩
            /// </summary>
            /// <param name="data">短信数据</param>
            /// <param name="Index">起始索引号</param>
            /// <param name="Septets">7-Bit编码字符数</param>
            /// <param name="FillBits">填充Bit位数</param>
            /// <returns>7-Bit字节序列</returns>
            public static Byte[] BIT7Unpack(String data, Int32 Index, Int32 Septets, Int32 FillBits)
            {
                Byte[] Bit7Array = new Byte[Septets];
                // 每8个7-Bit编码字符存放到7个字节
                Int32 PackLen = (Septets * 7 + FillBits + 7) / 8;
                Int32 n = 0;
                Int32 Remainder = 0;
                for (Int32 i = 0; i < PackLen; i++)
                {
                    Int32 Order = (i + (7 - FillBits)) % 7;
                    Int32 Value = Convert.ToInt32(data.Substring((i << 1) + Index, 2), 16);
                    if ((i != 0) || (FillBits == 0))
                    {
                        Bit7Array[n++] = (Byte)(((Value << Order) + Remainder) & 0x7F);
                    }
                    Remainder = Value >> (7 - Order);
                    if (Order == 6)
                    {
                        if (n == Septets) break;    // 避免写入填充数据
                        Bit7Array[n++] = (Byte)Remainder;
                        Remainder = 0;
                    }
                }
                return Bit7Array;
            }
            
            /// <summary>
            /// 转换GSM字符编码到Unicode编码
            /// </summary>
            /// <param name="Bit7Array">7-Bit编码字节序列</param>
            /// <returns>Unicode字符串</returns>
            public static String BIT7Decoding(Byte[] Bit7Array)
            {
                StringBuilder sb = new StringBuilder(Bit7Array.Length);
                for (Int32 i = 0; i < Bit7Array.Length; i++)
                {
                    UInt16 Key = Bit7Array[i];
                    if (isBIT7Same(Key))
                    {
                        sb.Append(Char.ConvertFromUtf32(Key));
                    }
                    else if (BIT7ToUCS2.ContainsKey(Key))
                    {
                        if (Key == 0x1B)    // 转义字符
                        {
                            if (i < Bit7Array.Length - 1 && BIT7EToUCS2.ContainsKey(Bit7Array[i + 1]))
                            {
                                sb.Append(Char.ConvertFromUtf32(BIT7EToUCS2[Bit7Array[i + 1]]));
                                i++;
                            }
                            else
                            {
                                sb.Append(Char.ConvertFromUtf32(BIT7ToUCS2[Key]));
                            }
                        }
                        else
                        {
                            sb.Append(Char.ConvertFromUtf32(BIT7ToUCS2[Key]));
                        }
                    }
                    else
                    {   // 异常数据
                        sb.Append('?');
                    }
                }
                return sb.ToString();
            }
            /// <summary>
            /// BCD解码
            /// </summary>
            /// <param name="data">数据字符串</param>
            /// <param name="Index">起始索引号</param>
            /// <param name="isEnableMSB">最高位是否为符号位</param>
            /// <returns>转化后的十进制数</returns>
            private static Int32 BCDDecoding(String data, Int32 Index, Boolean isEnableMSB = false)
            {
                Int32 n = Convert.ToInt32(data.Substring(Index, 1));    // 个位
                Int32 m = Convert.ToInt32(data.Substring(Index + 1, 1), 16);    // 十位
                if (isEnableMSB)
                {   // 最高位为符号位，值的范围为-79～+79
                    if (m >= 8)
                        return -((m - 8) * 10 + n); // 负值
                    else
                        return m * 10 + n;
                }
                else
                {   // 值的范围为0～99
                    return m * 10 + n;
                }
            }
            /// <summary>
            /// 服务中心时间戳解码
            /// </summary>
            /// <param name="data">数据报文</param>
            /// <param name="Index">起始索引号</param>
            /// <returns>服务中心时间戳对应的本地时间</returns>
            private static DateTime SCTSDecoding(String data, Int32 Index)
            {   // 时区信息，其值为15分钟的倍数
                return new DateTimeOffset(
                    (DateTime.Today.Year / 100 * 100) + BCDDecoding(data, Index),   // 年
                    BCDDecoding(data, Index + 2),   // 月
                    BCDDecoding(data, Index + 4),   // 日
                    BCDDecoding(data, Index + 6),   // 时
                    BCDDecoding(data, Index + 8),   // 分
                    BCDDecoding(data, Index + 10),  // 秒
                    new TimeSpan(0, BCDDecoding(data, Index + 12, true) * 15, 0)).LocalDateTime;
            }
            /// <summary>
            /// 用户数据头解码
            /// </summary>
            /// <param name="data">数据报文</param>
            /// <param name="Index">起始索引号</param>
            /// <returns>解码后的用户数据头</returns>  
            /// <remarks>
            /// 信息元素标识
            /// 00  Concatenated short messages, 8-bit reference number
            /// 01  Special SMS Message Indication
            /// 02  Reserved
            /// 03  Value not used to avoid misinterpretation as <LF> character
            /// 04  Application port addressing scheme, 8 bit address
            /// 05  Application port addressing scheme, 16 bit address
            /// 06  SMSC Control Parameters
            /// 07  UDH Source Indicator
            /// 08  Concatenated short message, 16-bit reference number
            /// 09  Wireless Control Message Protocol
            /// 0A-6F   Reserved for future use
            /// 70-7F   SIM Toolkit Security Headers
            /// 80-9F   SME to SME specific use
            /// A0-BF   Reserved for future use
            /// C0-DF   SC specific use
            /// E0-FF   Reserved for future use
            /// </remarks>
            private static PDUUDH[] UDHDecoding(String data, Int32 Index)
            {
                List<PDUUDH> UDH = new List<PDUUDH>();
                // 用户数据头长度
                Int32 UDHL = Convert.ToInt32(data.Substring(Index, 2), 16);
                Index += 2;
                Int32 i = 0;
                while (i < UDHL)
                {   // 信息元素标识（Information Element Identifier）
                    Byte IEI = Convert.ToByte(data.Substring(Index, 2), 16);
                    Index += 2;
                    // 信息元素数据长度（Length of Information Element）
                    Int32 IEDL = Convert.ToInt32(data.Substring(Index, 2), 16);
                    Index += 2;
                    // 信息元素数据（Information Element Data）
                    Byte[] IED = new Byte[IEDL];
                    for (Int32 j = 0; j < IEDL; j++)
                    {
                        IED[j] = Convert.ToByte(data.Substring(Index, 2), 16);
                        Index += 2;
                    }
                    UDH.Add(new PDUUDH { IEI = IEI, IED = IED });
                    i += IEDL + 2;
                }
                return UDH.ToArray();
            }
            /// <summary>
            /// 用户数据内容拆分
            /// </summary>
            /// <param name="UDC">用户数据内容</param>
            /// <param name="UDH">用户数据头</param>
            /// <param name="DCS">编码方案</param>
            /// <returns>拆分内容列表</returns>
            private List<String> UDCSplit(String UDC, PDUUDH[] UDH = null, EnumDCS DCS = EnumDCS.UCS2)
            {   
                // 统计用户数据头长度
                Int32 UDHL = GetUDHL(UDH);
                if (DCS == EnumDCS.BIT7)
                {   // 7-Bit编码
                    // 计算剩余房间数
                    Int32 Room = (Int32)EnumUDL.BIT7UDL - (UDHL * 8 + 6) / 7;
                    if (Room < 1)
                    {
                        if (String.IsNullOrEmpty(UDC))
                            return new List<String>() { UDC };
                        else
                            return null;    // 超出范围
                    }
                    if (SeptetsLength(UDC) <= Room)
                    {
                        return new List<String>() { UDC };
                    }
                    else
                    {   // 需要拆分成多条短信
                        if (UDHL == 0) UDHL++;
                        if (mCSMIEI == EnumCSMIEI.BIT8)
                            UDHL += 5;  // 1字节消息参考号
                        else
                            UDHL += 6;  // 2字节消息参考号
                        // 更新剩余房间数
                        Room = (Int32)EnumUDL.BIT7UDL - (UDHL * 8 + 6) / 7;
                        if (Room < 1) return null;   // 超出范围
                        List<String> CSM = new List<String>();
                        Int32 i = 0;
                        while (i < UDC.Length)
                        {
                            Int32 Step = SeptetsToChars(UDC, i, Room);
                            if (i + Step < UDC.Length)
                                CSM.Add(UDC.Substring(i, Step));
                            else
                                CSM.Add(UDC.Substring(i));
                            i += Step;
                        }
                        return CSM;
                    }
                }
                else
                {   // UCS2编码
                    // 计算剩余房间数
                    Int32 Room = ((Int32)EnumUDL.BIT8UDL - UDHL) >> 1;
                    if (Room < 1)
                    {
                        if (String.IsNullOrEmpty(UDC))
                            return new List<String>() { UDC };
                        else
                            return null;    // 超出范围
                    }
                    if (UDC == null || UDC.Length <= Room)
                    {
                        return new List<String>() { UDC };
                    }
                    else
                    {   // 需要拆分成多条短信
                        if (UDHL == 0) UDHL++;
                        if (mCSMIEI == EnumCSMIEI.BIT8)
                            UDHL += 5;  // 1字节消息参考号
                        else
                            UDHL += 6;  // 2字节消息参考号
                        // 更新剩余房间数
                        Room = ((Int32)EnumUDL.BIT8UDL - UDHL) >> 1;
                        if (Room < 1) return null;  // 超出范围
                        List<String> CSM = new List<String>();
                        for (Int32 i = 0; i < UDC.Length; i += Room)
                        {
                            if (i + Room < UDC.Length)
                                CSM.Add(UDC.Substring(i, Room));
                            else
                                CSM.Add(UDC.Substring(i));
                        }
                        return CSM;
                    }
                }
            }
            /// <summary>
            /// 用户数据内容拆分
            /// </summary>
            /// <param name="UDC">用户数据内容</param>
            /// <param name="UDH">用户数据头</param>
            /// <returns>拆分内容列表</returns>
            private List<Byte[]> UDCSplit(Byte[] UDC, PDUUDH[] UDH = null)
            {   // 统计用户数据头长度
                Int32 UDHL = GetUDHL(UDH);
                // 8-Bit编码
                if (UDC == null || UDC.Length <= (Int32)EnumUDL.BIT8UDL - UDHL)
                {   // 不需要拆分
                    return new List<Byte[]>() { UDC };
                }
                else
                {   // 需要拆分成多条短信
                    if (UDHL == 0) UDHL++;
                    if (mCSMIEI == EnumCSMIEI.BIT8)
                        UDHL += 5;  // 1字节消息参考号
                    else
                        UDHL += 6;  // 2字节消息参考号
                    // 短信内容拆分
                    List<Byte[]> CSM = new List<Byte[]>();
                    Int32 Step = (Int32)EnumUDL.BIT8UDL - UDHL;
                    for (Int32 i = 0; i < UDC.Length; i += Step)
                    {
                        CSM.Add((Byte[])UDC.SubArray(i, Step));
                    }
                    return CSM;
                }
            }
            /// <summary>
            /// 用户数据头长度
            /// </summary>
            /// <param name="UDH">用户数据头</param>
            /// <returns>用户数据头编码字节数</returns>
            private static Int32 GetUDHL(PDUUDH[] UDH)
            {
                if (UDH == null || UDH.Length == 0) return 0;
                Int32 UDHL = 1;     // 加上1字节的用户数据头长度
                foreach (PDUUDH IE in UDH)
                {
                    UDHL += IE.IED.Length + 2;  // 信息元素标识+信息元素长度+信息元素数据
                }
                return UDHL;
            }
            /// <summary>
            /// 计算字符串需要的7-Bit编码字节数
            /// </summary>
            /// <param name="source">字符串</param>
            /// <returns>7-Bit编码字节数</returns>
            private static Int32 SeptetsLength(String source)
            {
                if (String.IsNullOrEmpty(source)) return 0;
                Int32 Length = source.Length;
                foreach (Char Letter in source)
                {
                    UInt16 Code = Convert.ToUInt16(Letter);
                    if (UCS2ToBIT7.ContainsKey(Code))
                    {
                        if (UCS2ToBIT7[Code] > 0xFF) Length++;
                    }
                }
                return Length;
            }
            /// <summary>
            /// 判断字符串是否在GSM缺省字符集内
            /// </summary>
            /// <param name="source">要评估的字符串</param>
            /// <returns>
            ///     true：在GSM缺省字符集内，可以使用7-Bit编码
            ///     false：不在GSM缺省字符集内，只能使用UCS2编码
            /// </returns>
            private static Boolean isGSMString(String source)
            {
                if (String.IsNullOrEmpty(source)) return true;
                foreach (Char Letter in source)
                {
                    UInt16 Code = Convert.ToUInt16(Letter);
                    if (!(isBIT7Same(Code) || UCS2ToBIT7.ContainsKey(Code)))
                    {
                        return false;
                    }
                }
                return true;
            }
            /// <summary>
            /// 将7-Bit编码字节数换算成UCS2编码字符数
            /// </summary>
            /// <param name="source">字符串</param>
            /// <param name="index">起始索引号</param>
            /// <param name="septets">要换算的7-Bit编码字节数</param>
            /// <returns>UCS2编码字符数</returns>
            private static Int32 SeptetsToChars(String source, Int32 index, Int32 septets)
            {
                if (String.IsNullOrEmpty(source)) return 0;
                Int32 Count = 0;
                Int32 i = index;
                for (; i < source.Length; i++)
                {
                    UInt16 Code = Convert.ToUInt16(source[i]);
                    if (UCS2ToBIT7.ContainsKey(Code) && UCS2ToBIT7[Code] > 0xFF)
                    {
                        Count++;
                    }
                    if (++Count >= septets)
                    {
                        if (Count == septets) i++;
                        break;
                    }
                }
                return i - index;
            }
            /// <summary>
            /// 在用户数据头中增加长短信信息元素
            /// </summary>
            /// <param name="UDH">原始用户数据头</param>
            /// <param name="CSMMR">消息参考号</param>
            /// <param name="Total">短消息总数</param>
            /// <param name="Index">短消息序号</param>
            /// <returns>更新后的用户数据头</returns>
            private PDUUDH[] UpdateUDH(PDUUDH[] UDH, Int32 CSMMR, Int32 Total, Int32 Index)
            {
                List<PDUUDH> CSMUDH;
                if (UDH == null || UDH.Length == 0)
                    CSMUDH = new List<PDUUDH>(1);
                else
                    CSMUDH = new List<PDUUDH>(UDH);
                if (mCSMIEI == EnumCSMIEI.BIT8)
                {
                    Byte[] IED = new Byte[3] { (Byte)(CSMMR & 0xFF), (Byte)Total, (Byte)(Index + 1) };
                    CSMUDH.Insert(0, new PDUUDH { IEI = 0, IED = IED });
                }
                else
                {
                    Byte[] IED = new Byte[4] { (Byte)((CSMMR >> 8) & 0xFF), (Byte)(CSMMR & 0xFF), (Byte)Total, (Byte)(Index + 1) };
                    CSMUDH.Insert(0, new PDUUDH { IEI = 8, IED = IED });
                }
                return CSMUDH.ToArray();
            }
   
            /// <summary>
            /// 数据编码方案（Data Coding Scheme）
            /// </summary>        
            public enum EnumDCS
            {
                BIT7 = 0,   // 采用GSM字符集
                BIT8 = 1,   // 采用ASCII字符集
                UCS2 = 2    // 采用Unicode字符集
            }
            /// <summary>
            /// 长短信编码
            /// </summary>
            /// <param name="DA">接收方地址</param>
            /// <param name="UDC">用户数据内容</param>
            /// <param name="UDH">用户数据头</param>
            /// <returns>长短信编码序列</returns>
            /// <remarks>
            ///     长短信自动拆分
            ///     自动确定最佳编码
            /// </remarks>
            public String[] PDUEncoding(String DA, Object UDC, PDUUDH[] UDH = null)
            {   // 确定编码方案
                EnumDCS DCS;
                if (UDC is String)
                {
                    if (isGSMString(UDC as String))
                        DCS = EnumDCS.BIT7;
                    else
                        DCS = EnumDCS.UCS2;
                }
                else
                {
                    DCS = EnumDCS.BIT8;
                }
                return PDUEncoding(null, DA, UDC, UDH, DCS);
            }
            /// <summary>
            /// 长短信编码
            /// </summary>
            /// <param name="SCA">服务中心地址</param>
            /// <param name="DA">接收方地址</param>
            /// <param name="UDC">用户数据内容</param>
            /// <param name="UDH">用户数据头</param>
            /// <param name="DCS">编码方案</param>
            /// <returns>长短信编码序列</returns>
            /// <remarks>长短信自动拆分</remarks>
            public String[] PDUEncoding(String SCA, String DA, Object UDC, PDUUDH[] UDH = null, EnumDCS DCS = EnumDCS.UCS2)
            {
                // 短信拆分
                if (UDC is String)
                {
                    List<String> CSMUDC = UDCSplit(UDC as String, UDH, DCS);
                    if (CSMUDC == null) return null;
                    if (CSMUDC.Count > 1)
                    {   // 长短信
                        Int32 CSMMR = _mCSMMR;
                        if (++_mCSMMR > 0xFFFF) _mCSMMR = 0;
                        // 生成短信编码序列
                        String[] CSMSeries = new String[CSMUDC.Count];
                        for (Int32 i = 0; i < CSMUDC.Count; i++)
                        {   // 更新用户数据头
                            PDUUDH[] CSMUDH = UpdateUDH(UDH, CSMMR, CSMUDC.Count, i);
                            CSMSeries[i] = SoloPDUEncoding(SCA, DA, CSMUDC[i], CSMUDH, DCS);
                        }
                        return CSMSeries;
                    }
                    else
                    {   // 单条短信
                        return new String[1] { SoloPDUEncoding(SCA, DA, UDC, UDH, DCS) };
                    }
                }
                else if (UDC is Byte[])
                {
                    List<Byte[]> CSMUDC = UDCSplit(UDC as Byte[], UDH);
                    if (CSMUDC == null) return null;
                    if (CSMUDC.Count > 1)
                    {   // 长短信
                        Int32 CSMMR = _mCSMMR;
                        if (++_mCSMMR > 0xFFFF) _mCSMMR = 0;
                        // 生成短信编码序列
                        String[] CSMSeries = new String[CSMUDC.Count];
                        for (Int32 i = 0; i < CSMUDC.Count; i++)
                        {   // 更新用户数据头
                            PDUUDH[] CSMUDH = UpdateUDH(UDH, CSMMR, CSMUDC.Count, i);
                            CSMSeries[i] = SoloPDUEncoding(SCA, DA, CSMUDC[i], CSMUDH, DCS);
                        }
                        return CSMSeries;
                    }
                    else
                    {   // 单条短信
                        return new String[1] { SoloPDUEncoding(SCA, DA, UDC, UDH, DCS) };
                    }
                }
                else
                {
                    return null;
                }
            }
            /// <summary>
            /// 用户数据编码
            /// </summary>
            /// <param name="UDC">短信内容</param>
            /// <param name="UDH">用户数据头</param>
            /// <param name="DCS">编码方案</param>
            /// <returns>编码后的字符串</returns>
            /// <remarks>
            /// L：用户数据长度，长度1
            /// M：用户数据，长度0～140
            /// </remarks>
            private static String UDEncoding(Object UDC, PDUUDH[] UDH = null, EnumDCS DCS = EnumDCS.UCS2)
            {
                // 用户数据头编码
                Int32 UDHL;
                String Header = UDHEncoding(UDH, out UDHL);
                // 用户数据内容编码
                Int32 UDCL;
                String Body;
                if (UDC is String)
                {   // 7-Bit编码或UCS2编码
                    Body = UDCEncoding(UDC as String, out UDCL, UDHL, DCS);
                }
                else
                {   // 8-Bit编码
                    Body = UDCEncoding(UDC as Byte[], out UDCL);
                }
                // 用户数据区长度
                Int32 UDL;
                if (DCS == EnumDCS.BIT7)
                {   // 7-Bit编码
                    UDL = (UDHL * 8 + 6) / 7 + UDCL;    // 字符数
                }
                else
                {   // UCS2编码或者8-Bit编码
                    UDL = UDHL + UDCL;                  // 字节数
                }
                return UDL.ToString("X2") + Header + Body;
            }
            /// <summary>
            /// 用户数据头编码
            /// </summary>
            /// <param name="UDH">用户数据头</param>
            /// <param name="UDHL">输出：用户数据头字节数</param>
            /// <returns>用户数据头编码字符串</returns>
            private static String UDHEncoding(PDUUDH[] UDH, out Int32 UDHL)
            {
                UDHL = 0;
                if (UDH == null || UDH.Length == 0) return String.Empty;
                foreach (PDUUDH IE in UDH)
                {
                    UDHL += IE.IED.Length + 2;  // 信息元素标识+信息元素长度+信息元素数据
                }
                StringBuilder sb = new StringBuilder((UDHL + 1) << 1);
                sb.Append(UDHL.ToString("X2"));
                foreach (PDUUDH IE in UDH)
                {
                    sb.Append(IE.IEI.ToString("X2"));           // 信息元素标识1字节
                    sb.Append(IE.IED.Length.ToString("X2"));    // 信息元素长度1字节
                    foreach (Byte b in IE.IED)
                    {
                        sb.Append(b.ToString("X2"));            // 信息元素数据
                    }
                }
                UDHL++; // 加上1字节的用户数据头长度
                return sb.ToString();
            }
            /// <summary>
            /// 用户数据内容编码
            /// </summary>
            /// <param name="UDC">用户数据内容</param>
            /// <param name="UDCL">输出：UCS2编码字节数或7-Bit编码字符数</param>
            /// <param name="UDHL">用户数据头长度，7-Bit编码时需要参考</param>
            /// <param name="DCS">编码方案</param>
            /// <returns>编码字符串</returns>
            private static String UDCEncoding(String UDC, out Int32 UDCL, Int32 UDHL = 0, EnumDCS DCS = EnumDCS.UCS2)
            {
                if (String.IsNullOrEmpty(UDC))
                {
                    UDCL = 0;
                    return String.Empty;
                }
                if (DCS == EnumDCS.BIT7)
                {   // 7-Bit编码，需要参考用户数据头长度，已保证7-Bit边界对齐
                    return BIT7Pack(BIT7Encoding(UDC, out UDCL), UDHL);
                }
                else
                {   // UCS2编码
                    UDCL = UDC.Length << 1;     // 字节数
                    StringBuilder sb = new StringBuilder(UDCL << 1);
                    foreach (Char Letter in UDC)
                    {
                        sb.Append(Convert.ToInt32(Letter).ToString("X4"));
                    }
                    return sb.ToString();
                }
            }
            /// <summary>
            /// 用户数据内容编码
            /// </summary>
            /// <param name="UDC">用户数据内容</param>
            /// <param name="UDCL">输出：编码字节数</param>
            /// <returns>编码字符串</returns>
            private static String UDCEncoding(Byte[] UDC, out Int32 UDCL)
            {   // 8-Bit编码
                if (UDC == null || UDC.Length == 0)
                {
                    UDCL = 0;
                    return String.Empty;
                }
                UDCL = UDC.Length;
                StringBuilder sb = new StringBuilder(UDCL << 1);
                foreach (Byte b in UDC)
                {
                    sb.Append(b.ToString("X2"));
                }
                return sb.ToString();
            }
            /// <summary>
            /// 7-Bit序列和Unicode编码是否相同
            /// </summary>
            /// <param name="UCS2">要检测的Unicode编码</param>
            /// <returns>
            /// 返回值：
            ///     true：编码一致
            ///     false：编码不一致
            /// </returns>
            private static Boolean isBIT7Same(UInt16 UCS2)
            {
                if (UCS2 >= 0x61 && UCS2 <= 0x7A ||
                    UCS2 >= 0x41 && UCS2 <= 0x5A ||
                    UCS2 >= 0x25 && UCS2 <= 0x3F ||
                    UCS2 >= 0x20 && UCS2 <= 0x23 ||
                    UCS2 == 0x0A ||
                    UCS2 == 0x0D)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            /// <summary>
            /// 将UCS2编码字符串转换成7-Bit编码字节 序列
            /// </summary>
            /// <param name="UDC">用户数据内容</param>
            /// <param name="Septets">7-Bit编码字符数</param>
            /// <returns>7-Bit编码字节序列</returns>
            private static Byte[] BIT7Encoding(String UDC, out Int32 Septets)
            {
                Byte[] Bit7Array = new Byte[UDC.Length << 1];
                Septets = 0;
                foreach (Char Letter in UDC)
                {
                    UInt16 Code = Convert.ToUInt16(Letter);
                    if (isBIT7Same(Code))
                    {   // 编码不变
                        Bit7Array[Septets++] = Convert.ToByte(Code);
                    }
                    else
                    {
                        if (UCS2ToBIT7.ContainsKey(Code))
                        {
                            UInt16 Transcode = UCS2ToBIT7[Code];    // 转换码
                            if (Transcode > 0xFF)
                            {   // 转义序列
                                Bit7Array[Septets++] = Convert.ToByte(Transcode >> 8);
                                Bit7Array[Septets++] = Convert.ToByte(Transcode & 0xFF);
                            }
                            else
                            {
                                Bit7Array[Septets++] = Convert.ToByte(Transcode);
                            }
                        }
                        else
                        {   // 未知字符
                            Bit7Array[Septets++] = Convert.ToByte('?');
                        }
                    }
                }
                // 重新调整大小
                Array.Resize(ref Bit7Array, Septets);
                return Bit7Array;
            }
            /// <summary>
            /// 7-Bit编码压缩
            /// </summary>
            /// <param name="Bit7Array">7-Bit编码字节序列</param>
            /// <param name="UDHL">用户数据头字节数</param>
            /// <returns>编码后的字符串</returns>
            private static String BIT7Pack(Byte[] Bit7Array, Int32 UDHL)
            {
                // 7Bit对齐需要的填充位
                Int32 FillBits = (UDHL * 8 + 6) / 7 * 7 - (UDHL * 8);
                // 压缩字节数
                Int32 Len = Bit7Array.Length;
                Int32 PackLen = (Len * 7 + FillBits + 7) / 8;
                StringBuilder sb = new StringBuilder(PackLen << 1);
                Int32 Remainder = 0;
                for (Int32 i = 0; i < Len; i++)
                {   // 每8个字节压缩成7个字节
                    Int32 CharValue = Bit7Array[i];
                    Int32 Index = (i + 8 - FillBits) % 8;
                    if (Index == 0)
                    {
                        Remainder = CharValue;
                    }
                    else
                    {
                        Int32 n = ((CharValue << (8 - Index)) | Remainder) & 0xFF;
                        sb.Append(n.ToString("X2"));
                        Remainder = CharValue >> Index;
                    }
                }
                if (((Len * 7 + FillBits) % 8) != 0)
                {   // 写入剩余数据
                    sb.Append(Remainder.ToString("X2"));
                }
                return sb.ToString();
            }
            /// <summary>
            /// 服务中心地址编码（SCA = Service Center Adress）
            /// </summary>
            /// <param name="SCA">服务中心地址</param>
            /// <returns>编码后的字符串</returns>
            /// <remarks>
            /// SCA组成：1～12个八位位组
            /// A：服务中心地址长度，长度1，其值为B+C字节数
            /// B：服务中心地址类型，长度0～1
            /// C：服务中心地址，长度0～10。
            /// </remarks>
            private static String SCAEncoding(String SCA)
            {
                if (String.IsNullOrEmpty(SCA))
                {   // 表示使用SIM卡内部的设置值，该值通过AT+CSCA指令设置
                    return "00";
                }
                StringBuilder sb = new StringBuilder(SCA.Length + 5);
                Int32 Index = 0;
                if (SCA.StartsWith("+"))
                {   // 国际号码
                    sb.Append((SCA.Length / 2 + 1).ToString("X2"));         // SCA长度编码
                    sb.Append("91");    // SCA类型编码
                    Index = 1;
                }
                else
                {   // 国内号码
                    sb.Append(((SCA.Length + 1) / 2 + 1).ToString("X2"));   // SCA长度编码
                    sb.Append("81");    // SCA类型编码
                }
                // SCA地址编码
                for (; Index < SCA.Length; Index += 2)
                {   // 号码部分奇偶位对调
                    if (Index == SCA.Length - 1)
                    {
                        sb.Append("F");     // 补“F”凑成偶数个
                        sb.Append(SCA[Index]);
                    }
                    else
                    {
                        sb.Append(SCA[Index + 1]);
                        sb.Append(SCA[Index]);
                    }
                }
                return sb.ToString();
            }
            /// <summary>
            /// 接收方地址编码
            /// </summary>
            /// <param name="DA">接收方地址</param>
            /// <returns>编码后的字符串</returns>
            /// <remarks>
            /// DA组成：2～12个八位位组
            /// F：地址长度，长度1。注意：其值是接收方地址长度，而非字节数
            /// G：地址类型，长度1，取值同B。
            /// H：地址，长度0～10。
            /// </remarks>
            private static String DAEncoding(String DA)
            {
                if (String.IsNullOrEmpty(DA))
                {   // 地址长度0，地址类型未知
                    return "0080";
                }
                StringBuilder sb = new StringBuilder(DA.Length + 5);
                Int32 Index = 0;
                if (DA.StartsWith("+"))
                {   // 国际号码
                    sb.Append((DA.Length - 1).ToString("X2"));  // 地址长度编码
                    sb.Append("91");    // 地址类型
                    Index = 1;
                }
                else
                {   // 国内号码
                    sb.Append(DA.Length.ToString("X2"));        // 地址长度编码
                    sb.Append("81");    // 地址类型
                }
                for (; Index < DA.Length; Index += 2)
                {   // 号码部分奇偶位对调
                    if (Index == DA.Length - 1)
                    {
                        sb.Append("F");     // 补“F”凑成偶数个
                        sb.Append(DA[Index]);
                    }
                    else
                    {
                        sb.Append(DA[Index + 1]);
                        sb.Append(DA[Index]);
                    }
                }
                return sb.ToString();
            }
            /// <summary>
            /// 协议数据单元类型编码
            /// </summary>
            /// <param name="UDHI">用户数据头标识</param>
            /// <returns>编码字符串</returns>
            private String PDUTypeEncoding(Boolean UDHI)
            {   // 信息类型指示（Message Type Indicator）
                Int32 PDUType = 0x01;   // 01 SMS-SUBMIT（MS -> SMSC）
                // 用户数据头标识（User Data Header Indicator）
                if (UDHI)
                {
                    PDUType |= 0x40;
                }
                // 有效期格式（Validity Period Format）
                if (_mVP.Length == 2)
                {   // VP段以整型形式提供（相对的）
                    PDUType |= 0x10;
                }
                else if (_mVP.Length == 14)
                {   // VP段以8位组的一半(semi-octet)形式提供（绝对的）
                    PDUType |= 0x18;
                }
                // 请求状态报告（Status Report Request）
                if (mSRR)
                {
                    PDUType |= 0x20;    // 请求状态报告
                }
                // 拒绝复本（Reject Duplicate）
                if (mRD)
                {
                    PDUType |= 0x04;    // 拒绝复本
                }
                return PDUType.ToString("X2");
            }
            /// <summary>
            /// 消息参考编码（Message Reference）
            /// </summary>
            /// <returns>编码字符串</returns>
            private static String MREncoding()
            {   // 由手机设置
                return "00";
            }
            /// <summary>
            /// 协议标识（Protocol Identifier）
            /// </summary>
            /// <returns>编码字符串</returns>
            private static String PIDEncoding()
            {
                return "00";
            }
            /// <summary>
            /// 数据编码方案
            /// </summary>
            /// <param name="UDC">用户数据</param>
            /// <param name="DCS">编码字符集</param>
            /// <returns>编码字符串</returns>
            private static String DCSEncoding(Object UDC, EnumDCS DCS = EnumDCS.UCS2)
            {
                if (UDC is String)
                {
                    if (DCS == EnumDCS.BIT7)
                    {   // 7-Bit编码
                        return "00";
                    }
                    else
                    {   // UCS2编码
                        return "08";
                    }
                }
                else
                {   // 8-Bit编码
                    return "04";
                }
            }
            /// <summary>
            /// 交换的BCD编码
            /// </summary>
            /// <param name="n">取值范围为-79～+79（MSB）或者0～99</param>
            /// <returns>编码后的字符串</returns>
            private static String BCDEncoding(Int32 n)
            {   // n的取值范围为-79～+79（MSB）或者0～99
                if (n < 0) n = Math.Abs(n) + 80;
                return (n % 10).ToString("X") + (n / 10).ToString("X");
            }
            /// <summary>
            /// 单条短信编码
            /// </summary>
            /// <param name="SCA">服务中心地址，如果为null，则表示使用SIM卡设置</param>
            /// <param name="DA">接收方地址</param>
            /// <param name="UDC">用户数据内容</param>
            /// <param name="UDH">用户数据头</param>
            /// <param name="DCS">编码方案</param>
            /// <returns>编码后的字符串</returns>
            /// <remarks>
            /// 发送方PDU格式（SMS-SUBMIT-PDU）
            /// SCA（Service Center Adress）：短信中心，长度1-12
            /// PDU-Type（Protocol Data Unit Type）：协议数据单元类型，长度1
            /// MR（Message Reference）：消息参考值，为0～255。长度1
            /// DA（Destination Adress）：接收方SME的地址，长度2-12
            /// PID（Protocol Identifier）：协议标识，长度1
            /// DCS（Data Coding Scheme）：编码方案，长度1
            /// VP（Validity Period）：有效期，长度为1（相对）或者7（绝对或增强）
            /// UDL（User Data Length）：用户数据段长度，长度1
            /// UD（User Data）：用户数据，长度0-140
            /// </remarks>
            private String SoloPDUEncoding(String SCA, String DA, Object UDC, PDUUDH[] UDH = null, EnumDCS DCS = EnumDCS.UCS2)
            {
                StringBuilder sb = new StringBuilder(400);
                // 短信中心
                sb.Append(SCAEncoding(SCA));
                // 协议数据单元类型
                if (UDH == null || UDH.Length == 0)
                {
                    sb.Append(PDUTypeEncoding(false));
                }
                else
                {
                    sb.Append(PDUTypeEncoding(true));
                }
                // 消息参考值
                sb.Append(MREncoding());
                // 接收方SME地址
                sb.Append(DAEncoding(DA));
                // 协议标识
                sb.Append(PIDEncoding());
                // 编码方案
                sb.Append(DCSEncoding(UDC, DCS));
                // 有效期
                sb.Append(_mVP);
                // 用户数据长度及内容
                sb.Append(UDEncoding(UDC as String, UDH, DCS));
                return sb.ToString();
            }
   
 
    
        /// <summary>
        /// 短信编码参数设置部分
        ///     mCSMIEI：长短信信息元素参考号（类型：enum）
        ///     mSCA：服务中心地址（类型：String）
        ///     mSRR：请求状态报告（类型：Boolean）
        ///     mRD：拒绝复本（类型：Boolean）
        ///     mVP：短信有效期（类型：Object，接受TimeSpan或者DateTime类型）
        /// </summary>
     
            /// <summary>
            /// 长短信信息元素参考号枚举类型
            ///     BIT8：8-Bit参考号
            ///     BIT16：16-Bit参考号
            /// </summary>
            public enum EnumCSMIEI { BIT8 = 0, BIT16 = 8 };
            /// <summary>
            /// 长短信信息元素参考号，默认为8-Bit编码
            /// </summary>
            public EnumCSMIEI mCSMIEI = EnumCSMIEI.BIT8;
            /// <summary>
            /// 服务中心地址（Service Center Address）
            /// </summary>
            private String _mSCA = null;
            public String mSCA
            {
                get
                {
                    return _mSCA;
                }
                set
                {   // 国际号码、国内号码、固定电话、小灵通
                    _mSCA = value;
                }
            }
            /// <summary>
            /// 请求状态报告（Status Report Request）
            /// </summary>
            public Boolean mSRR = false;
            /// <summary>
            /// 拒绝复本（Reject Duplicate）
            /// </summary>
            public Boolean mRD = false;
            /// <summary>
            /// 短信有效期
            /// </summary>
            private String _mVP = "A7";   // 默认24小时
            public Object mVP
            {
                get
                {
                    if (_mVP.Length == 2)
                    {   // 相对有效期
                        Int32 n = Convert.ToInt32(_mVP, 16);
                        if (n <= 0x8F)
                        {   // 00～8F (VP+1)*5分钟 从5分钟间隔到12个小时
                            return new TimeSpan(0, (n + 1) * 5, 0); // 时 分 秒
                        }
                        else if (n <= 0xA7)
                        {   // 90～A7 12小时+(VP-143)*30分钟
                            return new TimeSpan(12, (n - 143) * 30, 0); // 时 分 秒
                        }
                        else if (n <= 0xC4)
                        {   // A8～C4 (VP-166)*1天
                            return new TimeSpan(n - 166, 0, 0, 0);  // 天 时 分 秒
                        }
                        else
                        {   // C5～FF (VP-192)*1周
                            return new TimeSpan((n - 192) * 7, 0, 0, 0);    // 天 时 分 秒
                        }
                    }
                    else if (_mVP.Length == 14)
                    {   // 绝对有效期
                        return SCTSDecoding(_mVP, 0);
                    }
                    else
                    {
                        return null;
                    }
                }
                set
                {
                    if (value == null)
                    {   // 不提供VP段
                        _mVP = String.Empty;
                    }
                    else if (value is TimeSpan)
                    {   // 相对有效期
                        Int32 Days = ((TimeSpan)value).Days;
                        if (Days >= 2)
                        {
                            if (Days >= 35)
                            {   // C5～FF 5周～63周
                                _mVP = Math.Min(Days / 7 + 192, 255).ToString("X2");
                            }
                            else
                            {   // A8～C4 2天～30天
                                _mVP = Math.Min(Days + 166, 196).ToString("X2");
                            }
                        }
                        else
                        {
                            Int32 TotalMinutes = Math.Max(5, (Int32)((TimeSpan)value).TotalMinutes);
                            if (TotalMinutes >= 750)
                            {   // 90～A7 12小时30分钟～24小时
                                _mVP = Math.Min((TotalMinutes - 720) / 30 + 143, 167).ToString("X2");
                            }
                            else
                            {   // 00～8F 5分钟～12小时
                                _mVP = Math.Min(TotalMinutes / 5 - 1, 143).ToString("X2");
                            }
                        }
                    }
                    else if (value is DateTime)
                    {   // 绝对有效期                    
                        // 调整为本地时间
                        DateTime dt;
                        if (((DateTime)value).Kind == DateTimeKind.Utc)
                        {
                            dt = ((DateTime)value).ToLocalTime();
                        }
                        else if (((DateTime)value).Kind == DateTimeKind.Unspecified)
                        {
                            dt = DateTime.SpecifyKind((DateTime)value, DateTimeKind.Local);
                        }
                        else
                        {
                            dt = (DateTime)value;
                        }
                        StringBuilder sb = new StringBuilder(14);
                        sb.Append(BCDEncoding(dt.Year % 100));  // 年
                        sb.Append(BCDEncoding(dt.Month));       // 月
                        sb.Append(BCDEncoding(dt.Day));         // 日
                        sb.Append(BCDEncoding(dt.Hour));        // 时
                        sb.Append(BCDEncoding(dt.Minute));      // 分
                        sb.Append(BCDEncoding(dt.Second));      // 秒
                        // 时区（-14小时～+14小时），度量范围为-56～+56
                        sb.Append(BCDEncoding((Int32)TimeZoneInfo.Local.BaseUtcOffset.TotalMinutes / 15));
                        _mVP = sb.ToString();
                    }
                }
            }
            /// <summary>
            /// 长短信信息元素消息参考号
            /// </summary>
            private static Int32 _mCSMMR = 0;
    }
        /// <summary>
        /// GSM编码和Unicode编码对照字典
        /// </summary>
        public partial class SMS
        {
            /// <summary>
            /// Unicode编码到GSM编码转换
            /// </summary>
            private static readonly SortedDictionary<UInt16, UInt16> UCS2ToBIT7 = new SortedDictionary<UInt16, UInt16>(){
            {0x000C, 0x1B0A},
            {0x0024, 0x0002},
            {0x0040, 0x0000},
            {0x005B, 0x1B3C},
            {0x005C, 0x1B2F},
            {0x005D, 0x1B3E},
            {0x005E, 0x1B14},
            {0x005F, 0x0011},
            {0x007B, 0x1B28},
            {0x007C, 0x1B40},
            {0x007D, 0x1B29},
            {0x007E, 0x1B3D},
            {0x00A0, 0x001B},
            {0x00A1, 0x0040},
            {0x00A3, 0x0001},
            {0x00A4, 0x0024},
            {0x00A5, 0x0003},
            {0x00A7, 0x005F},
            {0x00BF, 0x0060},
            {0x00C4, 0x005B},
            {0x00C5, 0x000E},
            {0x00C6, 0x001C},
            {0x00C9, 0x001F},
            {0x00D1, 0x005D},
            {0x00D6, 0x005C},
            {0x00D8, 0x000B},
            {0x00DC, 0x005E},
            {0x00DF, 0x001E},
            {0x00E0, 0x007F},
            {0x00E4, 0x007B},
            {0x00E5, 0x000F},
            {0x00E6, 0x001D},
            {0x00E7, 0x0009},
            {0x00E8, 0x0004},
            {0x00E9, 0x0005},
            {0x00EC, 0x0007},
            {0x00F1, 0x007D},
            {0x00F2, 0x0008},
            {0x00F6, 0x007C},
            {0x00F8, 0x000C},
            {0x00F9, 0x0006},
            {0x00FC, 0x007E},
            {0x0393, 0x0013},
            {0x0394, 0x0010},
            {0x0398, 0x0019},
            {0x039B, 0x0014},
            {0x039E, 0x001A},
            {0x03A0, 0x0016},
            {0x03A3, 0x0018},
            {0x03A6, 0x0012},
            {0x03A8, 0x0017},
            {0x03A9, 0x0015},
            {0x20AC, 0x1B65}
        };
            /// <summary>
            /// GSM编码到Unicode编码转换
            /// </summary>
            private static readonly SortedDictionary<UInt16, UInt16> BIT7ToUCS2 = new SortedDictionary<UInt16, UInt16>(){
            {0x0000, 0x0040},
            {0x0001, 0x00A3},
            {0x0002, 0x0024},
            {0x0003, 0x00A5},
            {0x0004, 0x00E8},
            {0x0005, 0x00E9},
            {0x0006, 0x00F9},
            {0x0007, 0x00EC},
            {0x0008, 0x00F2},
            {0x0009, 0x00E7},
            {0x000B, 0x00D8},
            {0x000C, 0x00F8},
            {0x000E, 0x00C5},
            {0x000F, 0x00E5},
            {0x0010, 0x0394},
            {0x0011, 0x005F},
            {0x0012, 0x03A6},
            {0x0013, 0x0393},
            {0x0014, 0x039B},
            {0x0015, 0x03A9},
            {0x0016, 0x03A0},
            {0x0017, 0x03A8},
            {0x0018, 0x03A3},
            {0x0019, 0x0398},
            {0x001A, 0x039E},
            {0x001B, 0x00A0},
            {0x001C, 0x00C6},
            {0x001D, 0x00E6},
            {0x001E, 0x00DF},
            {0x001F, 0x00C9},
            {0x0024, 0x00A4},
            {0x0040, 0x00A1},
            {0x005B, 0x00C4},
            {0x005C, 0x00D6},
            {0x005D, 0x00D1},
            {0x005E, 0x00DC},
            {0x005F, 0x00A7},
            {0x0060, 0x00BF},
            {0x007B, 0x00E4},
            {0x007C, 0x00F6},
            {0x007D, 0x00F1},
            {0x007E, 0x00FC},
            {0x007F, 0x00E0}
        };
            /// <summary>
            /// GSM编码转义序列到Unicode编码转换
            /// </summary>
            private static readonly SortedDictionary<UInt16, UInt16> BIT7EToUCS2 = new SortedDictionary<UInt16, UInt16>(){
            {0x000A, 0x000C},
            {0x0014, 0x005E},
            {0x0028, 0x007B},
            {0x0029, 0x007D},
            {0x002F, 0x005C},
            {0x003C, 0x005B},
            {0x003D, 0x007E},
            {0x003E, 0x005D},
            {0x0040, 0x007C},
            {0x0065, 0x20AC}
        };
        }
    /// <summary>
    /// 扩展方法：
    /// 1、Int32类型的Bit位测试和Bit位设置
    /// 2、Array类型的子数组检索
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Bit位测试
        /// </summary>
        /// <param name="n">要测试的整数</param>
        /// <param name="bit">要测试的Bit位序号</param>
        /// <returns>
        ///     true：该Bit位为1
        ///     false：该Bit为0
        /// </returns>
        public static Boolean BitTest(this Int32 n, Int32 bit)
        {
            if ((n & (1 << bit)) != 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        /// <summary>
        /// Bit位设置
        /// </summary>
        /// <param name="n">要设置的整数</param>
        /// <param name="bit">要设置的Bit位序号</param>
        public static Int32 BitSet(this Int32 n, Int32 bit)
        {
            return n | (1 << bit);
        }
        /// <summary>
        /// 从此实例检索子数组
        /// </summary>
        /// <param name="source">要检索的数组</param>
        /// <param name="startIndex">起始索引号</param>
        /// <param name="length">检索最大长度</param>
        /// <returns>与此实例中在 startIndex 处开头、长度为 length 的子数组等效的一个数组</returns>
        public static Array SubArray(this Array source, Int32 startIndex, Int32 length)
        {
            if (startIndex < 0 || startIndex > source.Length || length < 0)
            {
                throw new ArgumentOutOfRangeException();
            }
            Array Destination;
            if (startIndex + length <= source.Length)
            {
                Destination = Array.CreateInstance(source.GetType(), length);
                Array.Copy(source, startIndex, Destination, 0, length);
            }
            else
            {
                Destination = Array.CreateInstance(source.GetType(), source.Length - startIndex);
                Array.Copy(source, startIndex, Destination, 0, source.Length - startIndex);
            }
            return Destination;
        }
    }
}