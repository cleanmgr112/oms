using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using OMS.Core.Tools;
using OMS.Model.Authentication;
using OMS.Services.Account;

namespace OMS.API.Controllers
{
    [AllowAnonymous]
    [Route("api/[controller]")]
    public class OauthController : Controller
    {
        public IConfiguration _configuration { get; }
        public IUserService _userService { get; set; }

        public OauthController(IConfiguration configuration,
            IUserService userService)
        {
            _configuration = configuration;
            _userService = userService;
        }

        [HttpPost("authenticate")]
        public IActionResult RequestToken([FromBody]TokenRequest request)
        {
            if (request != null)
            {
                var account = _userService.GetByUserName(request.UserName);
                if (account == null)
                    return Ok(new TokenResult { res_state = false });

                if (EncryptTools.AESEncrypt(request.Password, account.Salt) == account.UserPwd)
                {
                    var claims = new[] {
                        //加入用户的名称
                       new Claim(ClaimTypes.Name,request.UserName)
                    };

                    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:SecurityKey"]));
                    var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                    var authTime = DateTime.UtcNow;
                    var expiresAt = authTime.AddDays(1);

                    var token = new JwtSecurityToken(
                        issuer: "wine-world.com",
                        audience: "wine-world.com",
                        claims: claims,
                        expires: expiresAt, // 过期时间
                        signingCredentials: creds);

                    return Ok(new TokenResult
                    {
                        res_state = true,
                        access_token = new JwtSecurityTokenHandler().WriteToken(token),
                        token_type = "Bearer",
                        profile = new Profile
                        {
                            name = request.UserName,
                            auth_time = new DateTimeOffset(authTime).ToUnixTimeSeconds(),
                            expires_at = new DateTimeOffset(expiresAt).ToUnixTimeSeconds()
                        }
                    });
                }
            }

            return Ok(new TokenResult { res_state = false });

        }

    }
}