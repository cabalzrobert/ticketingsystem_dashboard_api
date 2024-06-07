using Comm.Commons.Advance;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

using Microsoft.IdentityModel.Tokens;

using webapi.App.Aggregates.Common;
using webapi.App.Aggregates.TicketingSystemDashboard;
using webapi.App.Model.User;
using webapi.App.RequestModel.AppRecruiter;
using System.Text;

namespace webapi.Controllers.TicketingSystemDashboardController
{
    [Route("app/v1/ticketingdashboard")]
    [ApiController]
    public class TicketingSystemAuthrorizationController:ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly IAccountRepository _repo;
        public TicketingSystemAuthrorizationController(IConfiguration config, IAccountRepository repo)
        {
            _config = config;
            _repo = repo;
        }
        [HttpPost]
        [Route("dashboardsignin")]
        public async Task<IActionResult> signin([FromBody] TicketingSignInRequest request)
        {
            var result = await _repo.DashboardSignInAsync(request);
            if (result.result == Results.Success)
            {
                var token = CreateToken(result.account);
                return Ok(new { Status = "ok", Message = result.message, account = result.account, auth = token });
            }
            else if (result.result == Results.Failed)
                return Ok(new { Status = "error", Message = result.message });
            return NotFound();
        }

        [HttpPost]
        [Route("newUserLogin")]
        public async Task<IActionResult> NewUserLogin(string username)
        {
            Console.WriteLine(username);
            var result = await _repo.NewUserLogin(username);
            if (result.result == Results.Success)
                return Ok(new { Status = "ok", Message = result.message, UserId = result.userId });
            else if (result.result == Results.Failed)
                return Ok(new { Status = "error", Message = result.message });
            return NotFound();
        }

        [HttpPost]
        [Route("otp")]
        public async Task<IActionResult> SendOTP([FromBody] SendOtp sendOtp)
        {
            var result = await _repo.SendOTP(sendOtp);
            if (result.result == Results.Success)
                return Ok(new { Status = "ok", Message = result.message, Otp = result.otp });
            else if (result.result == Results.Failed)
                return Ok(new { Status = "error", Message = result.message });
            return BadRequest();
        }

        [HttpPost]
        [Route("setPassword")]
        public async Task<IActionResult> SetPassword([FromBody] SetPassword setPassword)
        {
            var result = await _repo.SetPassword(setPassword);
            if (result.result == Results.Success)
                return Ok(new { Status = "ok", Message = result.message });
            else if (result.result == Results.Failed)
                return Ok(new { Status = "error", Message = result.message });
            return BadRequest();
        }

        private object CreateToken(TicketingUser user)
        {
            var guid = Guid.NewGuid().ToString();
            var claims = new List<Claim>
            {
                new Claim("token", Cipher.Encrypt(JsonConvert.SerializeObject(new{
                    GUID = Cipher.MD5Hash(guid),
                    PL_ID = user.PL_ID,
                    PGRP_ID = user.PGRP_ID,
                    USR_ID = user.USR_ID,
                    ACT_ID = user.ACT_ID,
                    MOB_NO = user.MOB_NO,
                    ACT_TYP = user.ACT_TYP,
                    SUBSCRIBER_ID = user.SUBSCRIBER_ID,
                    isCommunicator = user.isCommunicator,
                    isDeptartmentHead = user.isDeptartmentHead

                }), guid)),
                new Claim(ClaimTypes.Name, user.FLL_NM),
                new Claim(JwtRegisteredClaimNames.Jti, guid),
            };

            DateTime now = DateTime.Now;
            string Issuer = _config["TokenSettings:Issuer"]
                , Audience = _config["TokenSettings:Audience"]
                , Key = _config["TokenSettings:Key"];
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
            return new { Token = new JwtSecurityTokenHandler().WriteToken(token), ExpirationDate = token.ValidTo, };
        }
    }
}
