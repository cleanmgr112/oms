using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MerchantPortalOpenSDK.Model.Response
{
    public class MerchantPortalResponse<T>
    {
        public int ResultType { get; set; }
        public string Message { get; set; }
        public T Data { get; set; }

        public static MerchantPortalResponse<T> Custom(int resultType, String message, T data = default(T))
        {
            MerchantPortalResponse<T> response = new MerchantPortalResponse<T>();
            response.ResultType = resultType;
            response.Message = message;
            response.Data = data;
            return response;
        }
    }
}
