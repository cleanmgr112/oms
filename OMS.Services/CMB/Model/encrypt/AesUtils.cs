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
using System.Security.Cryptography;
using System.Text;

namespace CmblifeOpenSDK
{
    public class AesUtils
    {
        /// <summary>
        /// 生成AES密钥,支持128，192，256长度密钥
        /// </summary>
        /// <param name="keySize">密钥长度，128位，192位，或者256位</param>
        /// <returns>AES对称密钥</returns>
        public static string GenAesKey(int keySize)
        {
            AesCryptoServiceProvider aes = new AesCryptoServiceProvider();
            aes.KeySize = keySize;                
            return Convert.ToBase64String(aes.Key);
        }
        /// <summary>
        /// 生成密钥，默认长度为128位
        /// </summary>
        /// <returns>密钥</returns>
        public static string GenAesKey()
        {
            return GenAesKey(Constants.CMBLIFE_DEFAULT_AES_KEY_SIZE);
        }

        /// <summary>
        ///  AES 加密，默认为AES/ECB/PKCS7Padding
        /// 其中PKCS7Padding对应JAVA中PKCS5Padding
        /// </summary>
        /// <param name="str">待加密内容</param>
        /// <param name="key">AES密钥</param>
        /// <returns></returns>
        public static string Encrypt(string str, string key)
        {
            return Encrypt(str,key,CipherMode.ECB,PaddingMode.PKCS7,null);
        }
        /// <summary>
        /// AES加密
        /// </summary>
        /// <param name="str">待加密字符串</param>
        /// <param name="key">AES密钥</param>
        /// <param name="cipherMode">工作模式，支持ECB/CBC/CFB/OFB</param>
        /// <param name="paddingMode">填充方式，支持NoPadding/ISO10126/PKCS7Padding/ANSIX923/Zeros模式</param>
        /// <param name="iv">初始向量</param>
        /// <returns>加密后的内容</returns>
        public static string Encrypt(string str, string key, CipherMode cipherMode, PaddingMode paddingMode, byte[] iv)
        {
            if (string.IsNullOrEmpty(str)) return null;
            RijndaelManaged rm = null;
            Byte[] toEncryptArray = Encoding.UTF8.GetBytes(str);
            if (CipherMode.ECB.Equals(cipherMode))
            {
                rm = new RijndaelManaged
                {
                    Key = Convert.FromBase64String(key),
                    Mode = cipherMode,
                    Padding = paddingMode
                };
            }
            else
            {
                rm = new RijndaelManaged
                {
                    Key = Convert.FromBase64String(key),
                    Mode = cipherMode,
                    Padding = paddingMode,
                    IV = iv
                };
            }
            ICryptoTransform cTransform = rm.CreateEncryptor();
            Byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);

            return Convert.ToBase64String(resultArray, 0, resultArray.Length);
        }

        /// <summary>
        /// AES解密，默认使用为AES/ECB/PKCS7Padding，其中PKCS7Padding对应JAVA中PKCS5Padding
        /// </summary>
        /// <param name="str">待解密字符串</param>
        /// <param name="key">AES密钥</param>
        /// <returns>解密后明文</returns>
        public static string Decrypt(string str, string key) {
            return Decrypt(str,key,CipherMode.ECB,PaddingMode.PKCS7,null);
        }


        /// <summary>
        ///  AES 解密
        /// </summary>
        /// <param name="str">待解密内容</param>
        /// <param name="key">AES密钥</param>
        /// <param name="cipherMode">工作模式，支持ECB/CBC/CFB/OFB</param>
        /// <param name="paddingMode">填充方式，支持NoPadding/ISO10126/PKCS7Padding/ANSIX923/Zeros模式</param>
        /// <param name="iv">初始向量</param>
        /// <returns>解密后的明文</returns>
        public static string Decrypt(string str, string key, CipherMode cipherMode, PaddingMode paddingMode, byte[] iv)
        {
            if (string.IsNullOrEmpty(str)) return null;
            Byte[] toEncryptArray = Convert.FromBase64String(str);
            RijndaelManaged rm = null;
            if (CipherMode.ECB.Equals(cipherMode))
            {
                 rm = new RijndaelManaged
                {
                    Key = Convert.FromBase64String(key),
                    Mode = cipherMode,
                    Padding = paddingMode
                };
            }
            else
            {
                rm = new RijndaelManaged
                {
                    Key = Convert.FromBase64String(key),
                    Mode = cipherMode,
                    Padding = paddingMode,
                    IV = iv
                };
            }

            ICryptoTransform cTransform = rm.CreateDecryptor();
            Byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);
            return Encoding.UTF8.GetString(resultArray);
        }
    }
}
