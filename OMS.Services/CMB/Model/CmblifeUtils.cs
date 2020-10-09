//------------------------------------------------------------------------------------- 
// CMB Confidential 

// Copyright (C) 2015 China Merchants Bank Co., Ltd. All rights reserved. 

// No part of this file may be reproduced or transmitted in any form or by any means,  
// electronic, mechanical, photocopying, recording, or otherwise, without prior   
// written permission of China Merchants Bank Co., Ltd. 
//-------------------------------------------------------------------------------------
using System;
using System.Collections.Generic;

namespace CmblifeOpenSDK
{
    public class CmblifeUtils
    {
       
        /// <summary>
        /// dictionary转为querystring
        /// </summary>
        /// <param name="dic">参数dictionary</param>
        /// <param name="isSort">是否排序</param>
        /// <param name="isUrlEncode">是否需要URLEncode</param>
        /// <returns>querystring</returns>
        public static string DicToQueryString(Dictionary<string, object> dic, bool isSort, bool isUrlEncode)
        {
            return URLUtils.DicToQueryString(dic, isSort, isUrlEncode);
        }


        /// <summary>
        /// 对dictionary中的value做urlEncode
        /// </summary>
        /// <param name="dic">参数</param>
        /// <returns>URLEncode过的dictionary参数</returns>
        public static Dictionary<string, string> DicUrlEncode(Dictionary<string, string> dic)
        {
            foreach (KeyValuePair<string, string> k in dic)
            {
                dic.Add(k.Key, StringUtils.UrlEncode(k.Value));
            }
            return dic;
        }

        /// <summary>
        /// 生成掌上生活协议，带有签名
        /// </summary>
        /// <param name="funcName">功能名</param>
        /// <param name="paramsMap">参数</param>
        /// <param name="xmlPrivateKey">签名所使用的Key，为商户私钥</param>
        /// <param name="signAlgorithm">签名算法（SHA1WithRSA 或 SHA256WithRSA）</param>
        /// <returns>掌上生活协议</returns>
        public static string GenProtocol(string funcName, Dictionary<string, object> paramsDic, string xmlPrivateKey, string signAlgorithm)
        {
            if (string.IsNullOrEmpty(xmlPrivateKey))
            {
                throw new ArgumentException("signKey不能为空");
            }
            if (string.IsNullOrEmpty(funcName))
            {
                throw new ArgumentException("funcName不能为空");
            }
            if (string.IsNullOrEmpty(signAlgorithm))
            {
                throw new ArgumentException("signAlgorithm不能为空");
            }

            // 拼接需要签名的串
            string signProtocol = URLUtils.AssembleProtocol(funcName, paramsDic, false);
            // 签名
            string sign = Sign(signProtocol, xmlPrivateKey, signAlgorithm);

            paramsDic.Add("sign", sign);
            // 拼接完整的串
            return URLUtils.AssembleProtocol(funcName, paramsDic, true);
        }

        /// <summary>
        /// 生成掌上生活协议,默认使用SHA256WITHRSA签名算法
        /// </summary>
        /// <param name="funcName">功能名</param>
        /// <param name="paramsMap">参数</param>
        /// <param name="xmlPrivateKey">签名所使用的Key，为商户私钥</param>
        /// <returns>掌上生活协议</returns>
        public static string GenProtocol(string funcName, Dictionary<string, object> paramsDic, string xmlPrivateKey)
        {
            return GenProtocol(funcName, paramsDic, xmlPrivateKey, Constants.CMBLIFE_SIGN_ALGORITHM_SHA256);
        }


        /// <summary>
        /// 签名
        /// </summary>
        /// <param name="signBody">待签名数据</param>
        /// <param name="signKey"> 签名使用的Key，为商户私钥</param>
        /// <param name="signAlgorithm">签名算法（SHA1WithRSA 或 SHA256WithRSA）</param>
        /// <returns>签名</returns>
        public static string Sign(string signBody, string xmlPrivateKey, string signAlgorithm)
        {
            if (string.IsNullOrEmpty(signBody))
            {
                throw new ArgumentException("待签名数据不能为空!");
            }
            if (string.IsNullOrEmpty(xmlPrivateKey))
            {
                throw new ArgumentException("私钥不能为空!");
            }
            return RsaUtils.Sign(signBody, xmlPrivateKey, signAlgorithm);
        }

        /// <summary>
        /// 签名，默认使用SHA256WithRSA签名算法
        /// </summary>
        /// <param name="signBody">待签名数据</param>
        /// <param name="xmlPrivateKey">签名使用的Key，为商户私钥</param>
        /// <returns>签名</returns>
        public static string Sign(string signBody, string xmlPrivateKey)
        {
            return Sign(signBody, xmlPrivateKey, Constants.CMBLIFE_SIGN_ALGORITHM_SHA256);
        }

        /// <summary>
        /// 对响应签名
        /// 调用方向：商户 --> 掌上生活
        /// </summary>
        /// <param name="paramsDic">待签名数据</param>
        /// <param name="xmlPrivateKey">签名使用的key，为商户私钥</param>
        /// <param name="signAlgorithm">签名算法（SHA1WithRSA 或 SHA256WithRSA）</param>
        /// <returns>签名</returns>
        public static string SignForResponse(Dictionary<string, object> paramsDic, string xmlPrivateKey, string signAlgorithm)
        {
            return Sign(DicToQueryString(paramsDic, true, false), xmlPrivateKey, signAlgorithm);
        }

        /// <summary>
        /// 对响应签名(默认使用SHA256WithRSA签名算法)
        /// 调用方向：商户 --> 掌上生活
        /// </summary>
        /// <param name="paramsDic">待签名数据</param>
        /// <param name="xmlPrivateKey">签名使用的key，为商户私钥</param>
        /// <returns>签名</returns>
        public static string SignForResponse(Dictionary<string, object> paramsDic, string xmlPrivateKey)
        {
            return SignForResponse(paramsDic, xmlPrivateKey, Constants.CMBLIFE_SIGN_ALGORITHM_SHA256);
        }

        /// <summary>
        /// 对请求签名 
        /// 调用方向：商户 --> 掌上生活
        /// </summary>
        /// <param name="prefix">前缀，如interface.json</param>
        /// <param name="paramsDic">待签名数据</param>
        /// <param name="xmlPrivateKey">签名使用的key，为商户私钥</param>
        /// <param name="signAlgorithm">签名算法（SHA1WithRSA 或 SHA256WithRSA）</param>
        /// <returns>签名</returns>
        public static string SignForRequest(string prefix, Dictionary<string, object> paramsDic, string xmlPrivateKey, string signAlgorithm)
        {
            string url = URLUtils.AssembleUrl(prefix, paramsDic, false);
            return Sign(url, xmlPrivateKey, signAlgorithm);
        }

        /// <summary>
        /// 对请求签名(默认使用SHA256WithRSA签名算法)
        /// 调用方向：商户 --> 掌上生活
        /// </summary>
        /// <param name="prefix">前缀，如interface.json</param>
        /// <param name="paramsDic">待签名数据</param>
        /// <param name="xmlPrivateKey">签名使用的key，为商户私钥</param>
        /// <returns>签名</returns>
        public static string SignForRequest(string prefix, Dictionary<string, object> paramsDic, string xmlPrivateKey)
        {
            return SignForRequest(prefix, paramsDic, xmlPrivateKey, Constants.CMBLIFE_SIGN_ALGORITHM_SHA256);
        }

        /// <summary>
        /// 验签
        /// </summary>
        /// <param name="verifyBody">待验签的数据</param>
        /// <param name="sign">签名</param>
        /// <param name="xmlPublicKey">验签所使用的Key，为掌上生活公钥</param>
        /// <param name="signAlgorithm">签名算法（SHA1WithRSA 或 SHA256WithRSA）</param>
        /// <returns>true为验签成功，false为验签失败</returns>
        public static bool Verify(string verifyBody, string sign, string xmlPublicKey, string signAlgorithm)
        {
            if (string.IsNullOrEmpty(verifyBody))
            {
                throw new ArgumentException("验签数据不能为空!");
            }
            if (string.IsNullOrEmpty(sign))
            {
                throw new ArgumentException("签名不能为空!");
            }
            if (string.IsNullOrEmpty(xmlPublicKey))
            {
                throw new ArgumentException("公钥不能为空!");
            }
            if (string.IsNullOrEmpty(signAlgorithm))
            {
                throw new ArgumentException("验签算法不能为空!");
            }
            return RsaUtils.Verify(verifyBody, sign, xmlPublicKey, signAlgorithm);
        }

        /// <summary>
        /// 验签(默认使用SHA256WithRSA签名算法)
        /// </summary>
        /// <param name="verifyBody">待验签的数据</param>
        /// <param name="sign">签名</param>
        /// <param name="xmlPublicKey">验签所使用的Key，为掌上生活公钥</param>
        /// <returns>true为验签成功，false为验签失败</returns>
        public static bool Verify(string verifyBody, string sign, string xmlPublicKey)
        {
            return Verify(verifyBody, sign, xmlPublicKey, Constants.CMBLIFE_SIGN_ALGORITHM_SHA256);
        }

        /// <summary>
        /// 对请求验签
        /// 调用方向：掌上生活 --> 商户
        /// </summary>
        /// <param name="paramsDic">掌上生活返回报文</param>
        /// <param name="xmlPublicKey">验签所使用的Key，为掌上生活公钥</param>
        /// <param name="signAlgorithm">签名算法（SHA1WithRSA 或 SHA256WithRSA）</param>
        /// <returns>true为验签成功，false为验签失败</returns>
        public static bool VerifyForRequest(Dictionary<string, object> paramsDic, string xmlPublicKey, string signAlgorithm)
        {
            string sign = paramsDic["sign"].ToString();
            Dictionary<string, object> verifyParamsDic = new Dictionary<string, object>(paramsDic);
            if (string.IsNullOrEmpty(sign))
            {
                throw new ArgumentException("返回报文参数中sign字段为空！");
            }
            verifyParamsDic.Remove("sign");
            return Verify(DicToQueryString(verifyParamsDic, true, false), sign, xmlPublicKey, signAlgorithm);
        }

        /// <summary>
        /// 对请求验签(默认使用SHA256WithRSA签名算法)
        /// 调用方向：掌上生活 --> 商户
        /// </summary>
        /// <param name="paramsDic">掌上生活返回报文</param>
        /// <param name="xmlPublicKey">验签所使用的Key，为掌上生活公钥</param>
        /// <returns>true为验签成功，false为验签失败</returns>
        public static bool VerifyForRequest(Dictionary<string, object> paramsDic, string xmlPublicKey)
        {
            return VerifyForRequest(paramsDic, xmlPublicKey, Constants.CMBLIFE_SIGN_ALGORITHM_SHA256);
        }

        /// <summary>
        /// 对响应验签
        /// 调用方向：掌上生活 --> 商户
        /// </summary>
        /// <param name="response">响应报文</param>
        /// <param name="verifyKey">验签所使用的Key，为掌上生活公钥</param>
        /// <param name="verifyAlgorithm">验签算法（SHA1WithRSA 或 SHA256WithRSA）</param>
        /// <returns>true为验签成功，false为验签失败</returns>
        public static bool VerifyForResponse(String response, String verifyKey, String verifyAlgorithm)
        {
            Dictionary<string, object> verifySignParams = JsonUtils.JsonStrToDic(response);
            string sign = verifySignParams["sign"].ToString();
            if (string.IsNullOrEmpty(sign))
            {
                throw new ArgumentException("返回报文参数中sign字段为空！");
            }
            verifySignParams.Remove("sign");
            Console.WriteLine(URLUtils.AssembleUrl("", verifySignParams, false));
            return Verify(URLUtils.AssembleUrl("", verifySignParams, false), sign, verifyKey, verifyAlgorithm);
        }

        /// <summary>
        /// 对响应验签(默认使用SHA256WithRSA签名算法)
        /// 调用方向：掌上生活 --> 商户
        /// </summary>
        /// <param name="response">响应报文</param>
        /// <param name="verifyKey">验签所使用的Key，为掌上生活公钥</param>
        /// <param name="verifyAlgorithm">验签算法（SHA1WithRSA 或 SHA256WithRSA）</param>
        /// <returns>true为验签成功，false为验签失败</returns
        public static bool VerifyForResponse(String response, String verifyKey)
        {
            return VerifyForResponse(response, verifyKey, Constants.CMBLIFE_SIGN_ALGORITHM_SHA256);
        }

        /// <summary>
        /// 文加密
        /// 使用场景：1.商户请求掌上生活
        ///           2.商户响应掌上生活
        /// </summary>
        /// <param name="encryptBody">需要加密的字符串</param>
        /// <param name="xmlPublicKey">加密使用的Key，为掌上生活RSA公钥</param>
        /// <returns>密文</returns>
        public static string Encrypt(string encryptBody, string xmlPublicKey)
        {
            if (string.IsNullOrEmpty(encryptBody))
            {
                throw new ArgumentException("报文不能为空!");
            }

            if (string.IsNullOrEmpty(xmlPublicKey))
            {
                throw new ArgumentException("公钥不能为空!");
            }

            string aesKey = AesUtils.GenAesKey();
            string aesEncryptedBody = AesUtils.Encrypt(encryptBody,aesKey);
            // TODO 需要先base64Decode
            
            string encryptedAesKey =RsaUtils.Encrypt(Convert.FromBase64String(aesKey), xmlPublicKey);
            return encryptedAesKey + "|" + aesEncryptedBody;
        }

        /// <summary>
        /// 报文解密
        /// 使用场景：1.掌上生活请求商户
        ///           2.掌上生活响应商户
        /// </summary>
        /// <param name="decryptBody">加密后内容(需要解密的字符串)</param>
        /// <param name="xmlPrivateKey">解密使用的Key，为商户RSA私钥</param>
        /// <returns>明文</returns>
        public static string Decrypt(string decryptBody, string xmlPrivateKey)
        {
            string[] data = decryptBody.Split('|');
            if (2 != data.Length)
            {
                throw new ArgumentException("加密报文格式错误!");
            }
            byte[] aesKey = RsaUtils.Decrypt(data[0], xmlPrivateKey);

            //需要先base64Encode
            return AesUtils.Decrypt(data[1], Convert.ToBase64String(aesKey));
        }

        /// <summary>
        /// 生成请求报文体
        /// </summary>
        /// <param name="dicParams">参数</param>
        /// <returns>请求报文体，如： key1=value1&key2=value2...</returns>
        public static string GenRequestBody(Dictionary<string, object> dicParams)
        {
            return URLUtils.AssembleUrl(null, dicParams, true);
        }

        /// <summary>
        /// 生成日期
        /// </summary>
        /// <returns>日期，格式为yyyyMMddHHmmss</returns>
        public static string GenDate()
        {
            return DateTime.Now.ToString(Constants.CMBLIFE_DATE_FORMAT);
        }

        /// <summary>
        /// 生成随机数
        /// </summary>
        /// <returns>随机数，如480f5bb71c8940e18d3ebe1dea0f73c5</returns>
        public static string GenRandom()
        {
            return Guid.NewGuid().ToString("N");
        }

        /// <summary>
        /// 将json字符串反序列化为map
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        public static Dictionary<string, object> JsonToDic(string json)
        {
            return JsonUtils.JsonStrToDic(json);
        }

        /**
         * 将map序列化为json字符串
         *
         * @param params 参数
         * @return json字符串
         */
        public static string DicToJson(Dictionary<string, object> dicParams)
        {
            return JsonUtils.DicToJson(dicParams);
        }
    }
}
