//------------------------------------------------------------------------------------- 
// CMB Confidential 
// 
// Copyright (C) 2015 China Merchants Bank Co., Ltd. All rights reserved. 
// 
// No part of this file may be reproduced or transmitted in any form or by any means,  
// electronic, mechanical, photocopying, recording, or otherwise, without prior   
// written permission of China Merchants Bank Co., Ltd. 
// 
//-------------------------------------------------------------------------------------
using System;
using System.Text;
using System.Web;

namespace CmblifeOpenSDK
{
    public class StringUtils
    {
       
        /// <summary>
        /// urlEncode
        /// </summary>
        /// <param name="str">encode前字符串</param>
        /// <returns>URLEncode后字符串</returns>
        // TODO =会被Encode成为%3D 
        public static string UrlEncode(string str)
        {
            try
            {
                string[] temp = str.Split(' ');
                StringBuilder sb = new StringBuilder();
                for (int t = 0; t < temp.Length; t++)
                {
                    sb.Append(HttpUtility.UrlEncode(temp[t].ToString(), Encoding.UTF8));
                    if (t < temp.Length - 1)
                    {
                        sb.Append("%20");
                    }
                }
                return sb.ToString();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
        }


        /// <summary>
        /// urlDecode
        /// </summary>
        /// <param name="str">urlEncode的字符串</param>
        /// <returns>URLDecode后的字符串</returns>
        public static string UrlDecode(string str)
        {
            try
            {
                return HttpUtility.UrlDecode(str, Encoding.UTF8);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
        }

        /// <summary>
        /// base64Encode(string-->string)
        /// </summary>
        /// <param name="str">待Encode内容</param>
        /// <returns></returns>
        public static string Base64Encode(string str)
        {
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(str));
        }

        /// <summary>
        /// base64Encode(byte[]-->byte[])
        /// </summary>
        /// <param name="bytes">待Encode内容</param>
        /// <returns></returns>
        public static byte[] Base64Encode(byte[] bytes)
        {
            return Encoding.UTF8.GetBytes(Convert.ToBase64String(bytes));
        }

        /// <summary>
        /// base64Deocde(byte[]-->byte[])
        /// </summary>
        /// <param name="bytes">base64内容</param>
        /// <returns></returns>
        public static byte[] Base64Decode(byte[] bytes)
        {
            return Encoding.UTF8.GetBytes(Convert.ToBase64String(bytes));
        }

        /// <summary>
        /// base64Deocde(string-->string)
        /// </summary>
        /// <param name="str">base64内容</param>
        /// <returns></returns>
        public static string Base64Decode(string str)
        {
            return Encoding.UTF8.GetString(Convert.FromBase64String(str));
        }


        /// <summary>
        /// byte转String
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static string NewStringUtf8(byte[] bytes)
        {
            if (bytes == null)
            {
                return null;
            }
            return Encoding.UTF8.GetString(bytes);
        }


        /// <summary>
        ///  String转byte
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static byte[] GetBytesUtf8(String str)
        {
            if (str == null)
            {
                return null;
            }
            return Encoding.UTF8.GetBytes(str);
        }

    }
}
