using System;
using System.Collections.Generic;
using System.Text;

namespace OMS.Model.Authentication
{
    public class TokenResult
    {
        public TokenResult()
        {
            this.profile = new Profile();
        }
        public bool res_state { get; set; }
        public string access_token { get; set; }
        public string token_type { get; set; }
        public Profile profile { get; set; }
    }
    public class Profile {
        public string name { get; set; }
        public long auth_time { get; set; }
        public long expires_at { get; set; }
    }
}
