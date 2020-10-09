using OMS.Core;

namespace OMS.Services.Authentication
{
    public interface IAuthenticationService
    {
        void SignIn(string userName);

        void SignOut();

    }
}
