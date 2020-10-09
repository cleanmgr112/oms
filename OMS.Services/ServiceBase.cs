using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using OMS.Core;
using OMS.Core.Json;
using OMS.Data.Interface;
using OMS.Model.Authentication;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace OMS.Services
{
    public abstract class ServiceBase
    {
        protected readonly IDbAccessor _omsAccessor;
        protected readonly IWorkContext _workContext;
        protected readonly IConfiguration _configuration;


        protected ServiceBase(IDbAccessor omsAccessor, IWorkContext workContext)
        {
            _omsAccessor = omsAccessor;
            _workContext = workContext;
        }

        protected ServiceBase(IDbAccessor omsAccessor, IWorkContext workContext, IConfiguration configuration)
        {
            _omsAccessor = omsAccessor;
            _workContext = workContext;
            _configuration = configuration;
        }

        /// <summary>
        /// 获取wms的Token,并存储到cookie中
        /// </summary>
        /// <returns></returns>
        protected async Task<string> GetWMSOauthToken()
        {
            //处理SSL问题
            var httpClientHandler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, certificate2, arg3, arg4) => true
            };
            using (var http = new HttpClient(httpClientHandler))
            {
                var clinetName = AppConfigurtaionServices.Configuration["OauthAccount:UserName"].ToString();
                var pwd = AppConfigurtaionServices.Configuration["OauthAccount:Password"].ToString();
                var account = new TokenRequest { UserName = clinetName, Password = pwd };
                var content = new StringContent(account.ToJson(), Encoding.UTF8, "application/json");
                var requestUrl = AppConfigurtaionServices.Configuration["WMSApi:domain"].ToString() + "/api/Oauth/authenticate";
                var response = http.PostAsync(requestUrl, content).Result.Content.ReadAsStringAsync();
                string resultState = await response;
                var tokenResult = resultState.ToObj<TokenResult>();
                if (tokenResult.res_state)
                {
                    _workContext.CurrentHttpContext.Response.Cookies.Delete("wms_token");
                    _workContext.CurrentHttpContext.Response.Cookies.Append("wms_token", tokenResult.access_token,
                        new CookieOptions
                        {
                            HttpOnly = true,
                            IsEssential = true,
                            Expires = DateTime.Now.AddDays(1)
                        });
                    return tokenResult.access_token;
                }
                else
                {
                    return "";
                }
            }
        }
    }
}
