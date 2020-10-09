using System;
using System.Collections.Generic;
using CmblifeOpenSDK;
using MerchantPortalOpenSDK.Model.Request;
using MerchantPortalOpenSDK.Model.Response;
using MerchantPortalOpenSDK.util;
using Newtonsoft.Json;

namespace MerchantPortalOpenSDK
{
    public class DefaultMerchantPortalApiClient : IMerchantPortalApiClient
    {
        //API地址
        private String ApiUrl;

        private static readonly  String default_apiUrl = "https://open.cmbchina.com";

        // 商户号
        public String Mid { get; set; }

        // 应用号
        public String Aid { get; set; }

        // 商户私钥
        public String MerchantPrivateKey { get; set; }

        // 掌上生活公钥
        public String CmbLifePublicKey { get; set; }

        private DefaultMerchantPortalApiClient() { }

        public DefaultMerchantPortalApiClient(String mid, String aid, String cmbLifePublicKey, String merchantPrivateKey) : this(default_apiUrl, mid, aid, cmbLifePublicKey, merchantPrivateKey) { }

        public DefaultMerchantPortalApiClient(String apiUrl, String mid, String aid, String cmbLifePublicKey, String merchantPrivateKey)
        {
            this.ApiUrl = apiUrl;
            this.Mid = mid;
            this.Aid = aid;
            this.CmbLifePublicKey = cmbLifePublicKey;
            this.MerchantPrivateKey = merchantPrivateKey;
        }

        public MerchantPortalResponse<T> Execute<T>(MerchantPortalRequest request)
        {
            try
            {
                //拼接请求地址
                string url = string.Format("{0}/AccessGateway/transIn/{1}.json", this.ApiUrl, request.FuncName);
                //获取参数
                var parameters = GetParameters(request);
                //进行网络请求
                var agResponse = Httputil.Post<AGResponse>(url, parameters);
                // 解密
                if (!string.IsNullOrEmpty(agResponse.EncryptBody))
                {
                    string body = CmblifeUtils.Decrypt(agResponse.EncryptBody, this.MerchantPrivateKey);
                    var response = JsonConvert.DeserializeObject<MerchantPortalResponse<T>>(body);
                    return response;
                }
                else
                {
                    return MerchantPortalResponse<T>.Custom(agResponse.respCode, agResponse.respMsg);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 获取请求参数
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        private Dictionary<string, object> GetParameters(MerchantPortalRequest request)
        {
            Dictionary<string, object> dic = new Dictionary<string, object>();
            // 1. 增加公共参数
            dic.Add("mid", this.Mid);
            dic.Add("aid", this.Aid);
            dic.Add("date", CmblifeUtils.GenDate());
            dic.Add("random", CmblifeUtils.GenRandom());
            //2.增加业务参数
            //2.1 组装业务参数

            string businessParamsStr = request.BusinessParams is string
                ? request.BusinessParams.ToString() : JsonConvert.SerializeObject(request.BusinessParams);
            Dictionary<string, object> businessParamDic = new Dictionary<string, object>() {
                { "bizData" , businessParamsStr }
            };
            //2.2 加密业务参数
            String encryptBody = CmblifeUtils.Encrypt(JsonConvert.SerializeObject(businessParamDic), this.CmbLifePublicKey);
            //2.3 参数添加加密后的参数
            dic.Add("encryptBody", encryptBody);
            //3.增加签名
            dic.Add("sign", GetSign(request.FuncName, dic));
            return dic;
        }

        /// <summary>
        /// 获取签名
        /// </summary>
        /// <param name="funcName"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        private String GetSign(String funcName, Dictionary<string, object> request)
        {
            return CmblifeUtils.SignForRequest(funcName + ".json", request, this.MerchantPrivateKey);
        }
    }
}
