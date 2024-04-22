using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Configuration;

using Newtonsoft.Json;

using Comm.Commons.Extensions;
using Comm.Commons.Advance;
using webapi.App.Aggregates.Common;
using webapi.App.Model.User;
using webapi.App.Aggregates.SubscriberAppAggregate.ActiveAccount;
using webapi.App.Aggregates.SubscriberAppAggregate.Common;

using System.IO;

namespace webapi.Controllers.SubscriberAppControllers.ActiveAccount
{
    [Route("app/v1/stl/")]
    [ApiController]
    [ServiceFilter(typeof(SubscriberAuthenticationAttribute))]
    public class AccountController : ControllerBase
    {
        private readonly IAccountRepository _accountRepo;
        public AccountController(IAccountRepository accountRepo){
            _accountRepo = accountRepo;
        }
        [HttpPost]
        [Route("balance")]
        public async Task<IActionResult> Task0a(){
            var repoResult = await _accountRepo.AccountBalanceAsync();
            if(repoResult.result == Results.Success)
                return Ok(repoResult.balance);
            return NotFound();
        }

        /*[HttpPost]
        [Route("firebase/{firebaseToken}")]
        public async Task<IActionResult> Task0b(String firebaseToken){
            var repoResult = await _accountRepo.FirebaseTokenUpdateAsync(firebaseToken);
            return Ok();
        }*/
        
        [HttpPost]
        [Route("info/profile")]
        public async Task<IActionResult> Task0c([FromBody] Subscriber request){
            if(request==null) return NotFound();
            //var repoResult = await _profileRepo.ChangeProfileDetailsAsync(Account, request);
            //if(repoResult.result == Results.Success)
            //    return Ok(new { Status = "ok", Message = repoResult.message });
            //else if(repoResult.result == Results.Failed)
            //    return Ok(new { Status = "error", Message = repoResult.message });
            //await Task.Run(async()=> await Task.Delay(1000));
            await Task.Delay(1000);
            return NotFound();
        }
    }
}