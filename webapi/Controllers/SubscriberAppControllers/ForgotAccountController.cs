using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Comm.Commons.Extensions;
using webapi.App.Aggregates.Common;
using webapi.App.RequestModel.SubscriberApp;
using webapi.App.Aggregates.SubscriberAppAggregate;

namespace webapi.Controllers.SubscriberAppControllers 
{
    [Route("app/v1/subscriber")]
    [ApiController]
    public class ForgotAccountController : ControllerBase
    {
        private readonly IForgotAccountRepository _forgotRepo;
        public ForgotAccountController(IForgotAccountRepository forgotRepo){
            _forgotRepo = forgotRepo;
        }

        [HttpPost]
        [Route("r/forgot/password")] 
        public async Task<IActionResult> RequestForgotPassword([FromBody] ForgotPasswordRequest request){
            if(!validity(request)) return NotFound();
            var repoResult = await _forgotRepo.RequestForgotPasswordAsync(request);
            if(repoResult.result == Results.Success){
                return Ok(new { Status = "ok", Message = repoResult.message  });
            }else if(repoResult.result == Results.Failed)
               return Ok(new { Status = "error", Message = repoResult.message });
            return NotFound();
        }

        [HttpPost]
        [Route("forgot/password")]
        public async Task<IActionResult> ForgotPasswordConfirm([FromBody] ForgotPasswordRequest request){
            if(!validity(request, true)) return NotFound();
            var repoResult = await (!request.Required?_forgotRepo.ForgotPasswordAsync(request):_forgotRepo.RequiredChangePasswordAsync(request));
            if(repoResult.result == Results.Success){
                return Ok(new { Status = "ok", Message = repoResult.message  });
            }else if(repoResult.result == Results.Failed)
               return Ok(new { Status = "error", Message = repoResult.message });
            return NotFound();
        }
        //////////////////////////////
        private bool validity(ForgotPasswordRequest request, bool IsConfirm = false){
            if(request==null || !request.IsChangePassword)
                return false;
            if(!request.MobileNumber.IsEmpty()){
                if(!IsConfirm)
                    return (!(request.IsPin && !request.OTPCode.Str().IsEmpty()) && request.RequestForm==null);
                return (request.IsPin && !request.OTPCode.Str().IsEmpty() && request.RequestForm!=null);
            }else if(request.Required){
                return (request.RequestForm!=null);
            }
            return false;
        } 
    }
}