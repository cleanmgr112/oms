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
using RSACryptoServiceProviderExtensions;
using System;
using System.Security.Cryptography;
using System.Text;

namespace CmblifeOpenSDK
{
    public class RsaUtils
    {
        #region RSA 的密钥产生 
        /// <summary>
        /// XML格式的RSA公钥和私钥生成
        /// </summary>
        /// <param name="xmlPrivateKey">xml格式的私钥</param>
        /// <param name="xmlPublicKey">xml格式的公钥</param>
        /// <param name="keySize">密钥长度，1024或2048</param>
        public static void RsaXmlKey(out string xmlPrivateKey, out string xmlPublicKey, int keySize)
        {
            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider(keySize);
            xmlPrivateKey = rsa.ToXmlString(true);
            xmlPublicKey = rsa.ToXmlString(false);
        }

        /// <summary>
        /// XML格式的RSA公钥和私钥生成，默认长度2048
        /// </summary>
        /// <param name="xmlPrivateKey">xml格式的私钥</param>
        /// <param name="xmlPublicKey">xml格式的公钥</param>
        public static void RsaXmlKey(out string xmlPrivateKey, out string xmlPublicKey)
        {
            RsaXmlKey(out xmlPrivateKey, out xmlPublicKey, Constants.CMBLIFE_DEFAULT_RSA_KEY_SIZE);
        }
        #endregion

        #region 签名
        /// <summary>    
        /// 签名    
        /// </summary>    
        /// <param name="content">待签名字符串</param>    
        /// <param name="xml格式的privateKey">xml格式的私钥</param>  
        /// <param name="signAlgorithm">签名算法，SHA256/SHA1</param>  
        /// <returns>签名后字符串</returns>    
        public static string Sign(string content, string xmlPrivateKey, string signAlgorithm)
        {
            byte[] data = Encoding.GetEncoding(Constants.CMBLIFE_DEFAULT_CHARSET_ENCODING).GetBytes(content);
            return Convert.ToBase64String(Sign(data, xmlPrivateKey, signAlgorithm));
        }

        /// <summary>    
        /// 签名    
        /// </summary>    
        /// <param name="content">byte[]类型的待签名字符串</param>    
        /// <param name="xml格式的privateKey">xml格式的私钥</param>   
        /// <param name="signAlgorithm">签名算法，SHA256/SHA1</param> 
        /// <returns>签名后字符串</returns>    
        public static byte[] Sign(byte[] content, string xmlPrivateKey, string signAlgorithm)
        {
            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
            RSACryptoExtensions.FromXmlString(rsa, xmlPrivateKey);
            //rsa.FromXmlString(xmlPrivateKey);
            if (Constants.CMBLIFE_SIGN_ALGORITHM_SHA256.Equals(signAlgorithm))
            {
                return rsa.SignData(content, new SHA256CryptoServiceProvider());
            }
            else if (Constants.CMBLIFE_SIGN_ALGORITHM_SHA1.Equals(signAlgorithm))
            {
                return rsa.SignData(content, new SHA1CryptoServiceProvider());
            }
            else
            {
                throw new ArgumentException("签名算法不合法!");
            }
        }
        #endregion

        #region 验签
        /// <summary>    
        /// 验签    
        /// </summary>    
        /// <param name="content">待验签字符串</param>    
        /// <param name="signedString">签名</param>    
        /// <param name="xml格式的publicKey">xml格式的公钥</param>  
        /// <param name="signAlgorithm">签名算法，SHA256/SHA1</param>  
        /// <returns>true(通过)，false(不通过)</returns>    
        public static bool Verify(string content, string signedString, string xmlPublicKey, string signAlgorithm)
        {
            byte[] data = Encoding.GetEncoding(Constants.CMBLIFE_DEFAULT_CHARSET_ENCODING).GetBytes(content);
            byte[] signature = Convert.FromBase64String(signedString);
            return Verify(data, signature, xmlPublicKey, signAlgorithm);
        }

        /// <summary>    
        /// 验签    
        /// </summary>    
        /// <param name="content">byte[]类型的待验签字符串</param>    
        /// <param name="signedString">签名</param>    
        /// <param name="xml格式的publicKey">xml格式的公钥</param>    
        /// <param name="signAlgorithm">签名算法，SHA256/SHA1</param>
        /// <returns>true(通过)，false(不通过)</returns>    
        public static bool Verify(byte[] content, byte[] signedString, string xmlPublicKey, string signAlgorithm)
        {
            RSACryptoServiceProvider rsaPub = new RSACryptoServiceProvider();
            rsaPub.FromXmlString(xmlPublicKey);
            if (Constants.CMBLIFE_SIGN_ALGORITHM_SHA256.Equals(signAlgorithm))
            {
                return rsaPub.VerifyData(content, new SHA256CryptoServiceProvider(), signedString);
            }
            else if (Constants.CMBLIFE_SIGN_ALGORITHM_SHA1.Equals(signAlgorithm))
            {
                return rsaPub.VerifyData(content, new SHA1CryptoServiceProvider(), signedString);
            }
            else
            {
                throw new ArgumentException("签名算法不合法!");
            }
        }
        #endregion

        #region 加密

        /// <summary>    
        /// 加密,默认使用Pkcs1Padding填充方式    
        /// </summary>    
        /// <param name="resData">需要加密的字符串</param>    
        /// <param name="publicKey">xml格式的公钥</param>    
        /// <returns>加密后的数据</returns> 
        public static string Encrypt(string resData, string xmlPublicKey)
        {
            return Encrypt(resData, xmlPublicKey, false);
        }

        /// <summary>    
        /// 加密    
        /// </summary>    
        /// <param name="resData">需要加密的字符串</param>    
        /// <param name="publicKey">xml格式的公钥</param>    
        /// <param name="isOaep">是否使用oaep填充方式和，ture使用oaep，false使用Pkcs1方式</param>
        /// <returns>加密后的数据</returns>    
        public static string Encrypt(string resData, string xmlPublicKey, bool isOaep)
        {
            byte[] dataToEncrypt = Encoding.UTF8.GetBytes(resData);

            return Encrypt(dataToEncrypt, xmlPublicKey, isOaep);
        }

        /// <summary>    
        /// 加密，默认使用Pkcs1Padding填充方式    
        /// </summary>    
        /// <param name="resData">需要加密的字符串</param>    
        /// <param name="publicKey">xml格式的公钥</param>    
        /// <returns>加密后的数据</returns>  
        public static string Encrypt(byte[] resData, string xmlPublicKey)
        {
            return Encrypt(resData, xmlPublicKey, false);
        }

        /// <summary>    
        /// 加密    
        /// </summary>    
        /// <param name="resData">byte[]类型的需要加密的字符串</param>    
        /// <param name="publicKey">xml格式的公钥</param>    
        /// <param name="isOaep">是否使用oaep填充方式和，ture使用oaep，false使用Pkcs1方式</param>
        /// <returns>加密后的数据</returns>    
        public static string Encrypt(byte[] resData, string xmlPublicKey, bool isOaep)
        {
            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
            RSACryptoExtensions.FromXmlString(rsa, xmlPublicKey);
            //rsa.FromXmlString(xmlPublicKey);
            return Convert.ToBase64String(rsa.Encrypt(resData, isOaep));
        }
        // 自定义padding的rsa加密方式
        //public static string encryptData(byte[] resData, string xmlPublicKey, string padding)
        //{
        //    byte[] s = Convert.FromBase64String(xmlPublicKey);
        //    AsymmetricKeyParameter publicKey = PublicKeyFactory.CreateKey(s);
        //    RsaKeyPairGenerator rsaKeyPairGenerator = new RsaKeyPairGenerator();
        //    IBufferedCipher c = CipherUtilities.GetCipher(padding);// 参数与JAVA中解密的参数一致"RSA/ECB/PKCS1Padding"
        //    c.Init(true, publicKey);
        //    byte[] outBytes = c.DoFinal(resData);
        //    return Convert.ToBase64String(outBytes);
        //}
        #endregion

        #region 解密

        /// <summary>    
        /// 解密, 默认使用Pkcs1Padding填充方式 
        /// </summary>    
        /// <param name="resData">加密字符串</param>    
        /// <param name="privateKey">xml格式的私钥</param>    
        /// <returns>明文</returns>    
        public static byte[] Decrypt(string resData, string xmlPrivateKey)
        {
            return Decrypt(resData, xmlPrivateKey, false);
        }

        /// <summary>    
        /// 解密    
        /// </summary>    
        /// <param name="resData">加密字符串</param>    
        /// <param name="privateKey">xml格式的私钥</param>    
        /// <param name="isOaep">是否使用oaep填充方式和，ture使用oaep，false使用Pkcs1方式</param> 
        /// <returns>明文</returns>    
        public static byte[] Decrypt(string resData, string xmlPrivateKey, bool isOaep)
        {
            byte[] dataToEncrypt = Convert.FromBase64String(resData);
            return Decrypt(dataToEncrypt, xmlPrivateKey, isOaep);
        }

        /// <summary>    
        /// 解密, 默认使用Pkcs1Padding填充方式 
        /// </summary>    
        /// <param name="resData">加密字符串</param>    
        /// <param name="privateKey">xml格式的私钥</param>    
        /// <returns>明文</returns>    
        public static byte[] Decrypt(byte[] resData, string xmlPrivateKey)
        {
            return Decrypt(resData, xmlPrivateKey, false);
        }

        /// <summary>    
        /// 解密    
        /// </summary>    
        /// <param name="resData">byte[]类型的加密字符串</param>    
        /// <param name="privateKey">xml格式的私钥</param>  
        /// <param name="isOaep">是否使用oaep填充方式和，ture使用oaep，false使用Pkcs1方式</param>  
        /// <returns>明文</returns>    
        public static byte[] Decrypt(byte[] resData, string xmlPrivateKey, bool isOaep)
        {
            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
            RSACryptoExtensions.FromXmlString(rsa, xmlPrivateKey);
            //rsa.FromXmlString(xmlPrivateKey);
            return rsa.Decrypt(resData, isOaep);
        }
        #endregion

    }
}
