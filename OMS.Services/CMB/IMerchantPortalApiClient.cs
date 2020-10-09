using MerchantPortalOpenSDK.Model.Request;
using MerchantPortalOpenSDK.Model.Response;

namespace MerchantPortalOpenSDK
{
    public interface IMerchantPortalApiClient
    {
        MerchantPortalResponse<T> Execute<T>(MerchantPortalRequest request);
    }
}
