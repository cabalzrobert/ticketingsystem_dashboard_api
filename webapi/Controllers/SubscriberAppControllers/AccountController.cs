using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text;

using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Configuration;

using Newtonsoft.Json;

using Comm.Commons.Extensions;
using Comm.Commons.Advance;
using webapi.App.Aggregates.Common;
using webapi.App.Model.User;
using webapi.App.RequestModel.SubscriberApp;
using webapi.App.RequestModel.SubscriberApp.Common;
using webapi.App.Aggregates.SubscriberAppAggregate;

namespace webapi.Controllers.SubscriberAppControllers
{
    [Route("app/v1/subscriber")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly IAccountRepository _accountRepo;
        public AccountController(IConfiguration config, IAccountRepository accountRepo){
            _config = config;
            _accountRepo = accountRepo;
        }

        [HttpPost]
        [Route("signin")]
        public async Task<IActionResult> SignIn([FromBody] SignInRequest request){
            if(request==null) return NotFound();
            if(!(request.DeviceID.Str().Equals("web") || request.DeviceName.Str().Equals("web"))){
                var chkResult = await _accountRepo.ApkUpdateCheckerAsync(request);
                if(chkResult.result == SignInResults.ApkUpdate){
                    return Ok(new { Status = "ok", Mode = "apkupdate", Message = chkResult.message, ApkVersion = chkResult.apkVersion, ApkUrl = chkResult.apkUrl, });
                }else if(chkResult.result != SignInResults.Success)
                    return NotFound();
            }

            var repoResult = await _accountRepo.SignInAsync(request);
            if(repoResult.result == SignInResults.Success){
                var user = repoResult.user;
                var token = CreateToken(user);
                if(!user.ImageUrl.Str().StartsWith("http"))
                    user.ImageUrl = $@"{Request.Scheme}://{Request.Host.Value}{user.ImageUrl}";
                return Ok(new { Status = "ok", Account = UserDto.Subscriber(repoResult.user), Auth = token, Data = repoResult.data });
            }else if(repoResult.result == SignInResults.PreRegister)
                return Ok(new { Status = "ok", Mode = "verified", Message = repoResult.message, PreRegister = UserDto.PreSubscriber(repoResult.user),  });
            else if(repoResult.result == SignInResults.ChangePassword)
                return Ok(new { Status = "ok", Mode = "change-password", Message = repoResult.message,  });
            else if(repoResult.result == SignInResults.Failed || repoResult.result == SignInResults.Blocked)
                return Ok(new { Status = "error", Message = repoResult.message });
            return NotFound();
        }

        [HttpPost]
        [Route("signup")]
        public async Task<IActionResult> SignUp([FromBody] SignUpRequest request){
            if(!validity(request)) return NotFound();
            var repoResult = await _accountRepo.SignUpAsync(request);
            if(repoResult.result == SignUpResults.Success){
                var token = CreateToken(repoResult.user);
                return Ok(new { Status = "ok", Account = UserDto.Subscriber(repoResult.user), Auth = token, Message = repoResult.message  });
            }else if(repoResult.result == SignUpResults.Failed)
                return Ok(new { Status = "error", Message = repoResult.message });
            return NotFound();
        }


        [HttpPost]
        [Route("signup/w/mobile")]
        public async Task<IActionResult> SignUpWithMobile([FromBody] SignUpWithMobileRequest request){
            var validityResult = await validity(request);
            if(validityResult.result == Results.Null) return NotFound();
            else if(validityResult.result == Results.Failed)  
                return Ok(new { Status = "error", Message = validityResult.message });
            
            var repoResult = await _accountRepo.SignUpWithMobileAsync(request);
            if(repoResult.result == SignUpResults.Success){
                return Ok(new { Status = "ok", Message = repoResult.message  });
            }else if(repoResult.result == SignUpResults.Failed)
               return Ok(new { Status = "error", Message = repoResult.message });
            return NotFound();
        }

        [HttpPost]
        [Route("verify/otp")]
        public async Task<IActionResult> VerifyOtp([FromBody] AuthFormRequest request){
            if(!validity(request)) return NotFound();
            var repoResult = await _accountRepo.VerifyOtpAsync(request);
            if(repoResult.result == VerifyOtpResults.VerifiedUser){
                return Ok(new { Status = "ok", PreRegister = UserDto.PreSubscriber(repoResult.user) });
            }else if(repoResult.result == VerifyOtpResults.ChangePassword){
                return Ok(new { Status = "ok", Message = repoResult.message });
            }else if(repoResult.result == VerifyOtpResults.Failed)
               return Ok(new { Status = "error", Message = repoResult.message });
            return NotFound();
        }

        //////////////////////////////
        private bool validity(SignUpRequest request){
            if(request==null || request.RequestForm==null)
                return false;
            var form = request.RequestForm;
            if(!(request.IsPin && request.OTPCode==form.OTPCode))
                return false;

            string mobilenumber = form.MobileNumber.Str().Trim();
            if(mobilenumber.IsEmpty())
                return false;
            if(mobilenumber.Length==10 && mobilenumber.IndexOf("9")==0)
                mobilenumber = $"+63{ mobilenumber }";
            else if(mobilenumber.Length==11 && mobilenumber.IndexOf("09")==0)
                mobilenumber = $"+63{ mobilenumber.Substring(1) }";

            if(mobilenumber.Length!=13) 
                return false;
    
            form.MobileNumber = mobilenumber;
            return true;
        } 
        
        private async Task<(Results result, String message)> validity(SignUpWithMobileRequest request){
            if(request==null || request.IsChangePassword)
                return (Results.Null, "");
            if(request.IsPin&&!request.OTPCode.Str().IsEmpty())
                return (Results.Null, "");

            string mobilenumber = request.MobileNumber.Str().Trim();
            if(mobilenumber.IsEmpty())
                return (Results.Null, "");
            if(mobilenumber.Length==10 && mobilenumber.IndexOf("9")==0)
                mobilenumber = $"+63{ mobilenumber }";
            else if(mobilenumber.Length==11 && mobilenumber.IndexOf("09")==0)
                mobilenumber = $"+63{ mobilenumber.Substring(1) }";

            if(mobilenumber.Length!=13) 
                return (Results.Failed, "Please enter valid mobile number");

            request.MobileNumber = mobilenumber;
            return (Results.Success, "");
        } 

        private bool validity(AuthFormRequest request){
            if(request==null)
                return false;
            return (request.IsPin&&!request.OTPCode.Str().IsEmpty());
        } 
        //
        private object CreateToken(Subscriber user){
            var guid = Guid.NewGuid().ToString();
            var claims = new List<Claim>{
                new Claim("token", Cipher.Encrypt(JsonConvert.SerializeObject(new{
                    GUID = Cipher.MD5Hash(guid),
                    CompanyID = user.CompanyID,
                    BranchID = user.BranchID,
                    SubscriberID = user.SubscriberID,
                    AccountID = user.AccountID,
                    //
                    BranchZipCode = user.BranchZipCode,
                    IsTerminal = user.IsTerminal,
                    IsGeneralCoordinator = user.IsGeneralCoordinator,
                    IsCoordinator = user.IsCoordinator,
                    IsPlayer = user.IsPlayer,
                    GeneralCoordinatorID = user.GeneralCoordinatorID,
                    CoordinatorID = user.CoordinatorID,
                    //
                    DeviceID = $"{ user.DeviceID }:{ user.SignatureID }",
                    DeviceName = user.DeviceName,
                    LastLogIn = user.LastLogIn,
                    SessionID = user.SessionID,
                }), guid)),
                new Claim(ClaimTypes.Name, user.Fullname),
                new Claim(JwtRegisteredClaimNames.Sub, user.MobileNumber),
                new Claim(JwtRegisteredClaimNames.Jti, guid),
            };
            DateTime now = DateTime.Now;
            string Issuer = _config["TokenSettings:Issuer"].Str()
                ,Audience = _config["TokenSettings:Audience"].Str()
                ,Key = _config["TokenSettings:Key"].Str();
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Key));
            var signInCred = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature);
            var token = new JwtSecurityToken(
                issuer: Issuer, audience: Audience,
                notBefore: now,
                expires: now.Add(TimeSpan.FromSeconds(30)),  //now.Add(TimeSpan.FromDays(15))  //TimeSpan.FromMinutes(0.1) TimeSpan.FromDays(15) .FromMinutes(2)  TimeSpan.FromMinutes(0.1)
                claims: claims,
                signingCredentials: signInCred
            );
            return new { Token = new JwtSecurityTokenHandler().WriteToken(token), ExpirationDate = token.ValidTo,};
        }   
    }
}