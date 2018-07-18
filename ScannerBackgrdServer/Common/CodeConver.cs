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
        /// <returns>Unicode编码后的字符串</returns>
        public static string String2Unicode(string source)
        {
            var bytes = Encoding.Unicode.GetBytes(source);
            var stringBuilder = new StringBuilder();
            for (var i = 0; i < bytes.Length; i += 2)
            {
                stringBuilder.AppendFormat("{0:x2}{1:x2}", bytes[i], bytes[i + 1]);
            }

            return stringBuilder.ToString();
        }


        /// <summary>  
        /// Unicode字符串转为正常字符串  
        /// </summary>  
        /// <param name="srcText"></param>  
        /// <returns></returns>  
        public static string Unicode2String(string srcText)
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
                bytes[1] = byte.Parse(int.Parse(str.Substring(2, 2), System.Globalization.NumberStyles.HexNumber).ToString());
                bytes[0] = byte.Parse(int.Parse(str.Substring(0, 2), System.Globalization.NumberStyles.HexNumber).ToString());
                dst += Encoding.Unicode.GetString(bytes);
            }

            return dst;
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
}