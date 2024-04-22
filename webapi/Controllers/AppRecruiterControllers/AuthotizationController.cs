using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text;

using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Configuration;

using Comm.Commons.Advance;
using webapi.App.Aggregates.AppRecruiterAggregate;
using webapi.App.Aggregates.Common;
using webapi.App.Model.User;
using webapi.App.RequestModel.AppRecruiter;

using Newtonsoft.Json;

namespace webapi.Controllers.AppRecruiterControllers
{
    [Route("app/v1/recruiter")]
    [ApiController]
    //[ServiceFilter(typeof(OperatorAuthenticationAttribute))]
    public class AuthotizationController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly IAccountRepository _accountRepo;
        public AuthotizationController(IConfiguration config, IAccountRepository accountRepo){
            _config = config;
            _accountRepo = accountRepo;
        }

        [HttpPost]
        [Route("signin")]
        public async Task<IActionResult> Task0a([FromBody] SignInRequest request){
            var repoResult = await _accountRepo.SignInAsync(request);
            if(repoResult.result == Results.Success){
                var token = CreateToken(repoResult.user);
                return Ok(new { Status = "ok", Auth = token });
            }else if(repoResult.result == Results.Failed)
                return Ok(new { Status = "error", Message = repoResult.message });
            return NotFound();
        }
        /*
        [HttpPost]
        [Route("auth")] ///{SessionID} signin
        public async Task<IActionResult> Task0a([FromBody] DeviceRequest request){
            var repoResult = await _accountRepo.SignInAsync(request);
            if(repoResult.result == Results.Success){
                var token = CreateToken(repoResult.user);
                return Ok(new { Status = "ok", Auth = token });
            }else if(repoResult.result == Results.Failed)
                return Ok(new { Status = "error", Message = repoResult.message });
            return NotFound();
        }
        */

        private object CreateToken(Recruiter user){
            var guid = Guid.NewGuid().ToString();
            var claims = new List<Claim>{
                new Claim("token", Cipher.Encrypt(JsonConvert.SerializeObject(new{
                    GUID = Cipher.MD5Hash(guid),
                    CompanyID = user.CompanyID,
                    BranchID = user.BranchID,
                    UserID = user.UserID,
                }), guid)),
                new Claim(ClaimTypes.Name, user.Fullname),
                new Claim(JwtRegisteredClaimNames.Jti, guid),
            };

            DateTime now = DateTime.Now;
            string Issuer = _config["TokenSettings:Issuer"]
                ,Audience = _config["TokenSettings:Audience"]
                ,Key = _config["TokenSettings:Key"];
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Key));
            var signInCred = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature);
            var token = new JwtSecurityToken(
                issuer: Issuer,
                audience: Audience,
                notBefore: now,
                expires: now.Add(TimeSpan.FromSeconds(30)),
                claims: claims,
                signingCredentials: signInCred
            );
            return new { Token = new JwtSecurityTokenHandler().WriteToken(token), ExpirationDate = token.ValidTo,};
        }
    }
}