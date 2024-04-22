using System;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Comm.Commons.Advance;
using webapi.App.Aggregates.Common;
using webapi.App.Model.User;
using webapi.App.Aggregates.SubscriberAppAggregate.Common;

using webapi.App.RequestModel.SubscriberApp.ActiveAccount;
using webapi.App.Aggregates.SubscriberAppAggregate.ActiveAccount;
using webapi.App.Features.UserFeature;
using Comm.Commons.Extensions;
using Newtonsoft.Json;

namespace webapi.Controllers.SubscriberAppControllers.ActiveAccount
{
    [Route("app/v1/stl/")]
    [ApiController]
    [ServiceFilter(typeof(SubscriberAuthenticationAttribute))]
    public class AccountProfileController : ControllerBase
    {
        private readonly IAccountProfileRepository _profileRepo;
        public AccountProfileController(IAccountProfileRepository profileRepo){
            _profileRepo = profileRepo;
        }

        [HttpPost]
        [Route("password")]
        public async Task<IActionResult> Task([FromBody] ChangePasswordRequest request){
            var repoResult = await _profileRepo.ChangePasswordAsync(request);
            if(repoResult.result == Results.Success)
                return Ok(new { Status = "ok", Message = repoResult.message });
            else if(repoResult.result == Results.Failed)
                return Ok(new { Status = "error", Message = repoResult.message });
            return NotFound();
        }
        
        [HttpPost]
        [Route("profile")]
        public async Task<IActionResult> Task([FromBody] Subscriber request){
            if(!validity(request)) return NotFound();
            var repoResult = await _profileRepo.ChangeProfileDetailsAsync(request);
            if(repoResult.result == Results.Success)
                return Ok(new { Status = "ok", Message = repoResult.message });
            else if(repoResult.result == Results.Failed)
                return Ok(new { Status = "error", Message = repoResult.message });
            return NotFound();
        }

        [HttpPost]
        [Route("profile/image")]
        public async Task<IActionResult> Task([FromBody] ChangeProfileImageRequest request){
            //if(!validity(request)) return NotFound();
            var valResult = await validity(request);
            if(valResult.result == Results.Failed)
                return Ok(new { Status = "error", Message = valResult.message });
            if(valResult.result != Results.Success)
                return NotFound();

            var repoResult = await _profileRepo.ChangeProfileImageAsync(request.ImageUrl);
            if(repoResult.result == Results.Success){
                //var imageUrl = String.Format("{0}://{1}{2}", Request.Scheme, Request.Host.Value, request.ImageUrl);
                return Ok(new { Status = "ok", Message = repoResult.message, ImageUrl = request.ImageUrl });
            }else if(repoResult.result == Results.Failed)
                return Ok(new { Status = "error", Message = repoResult.message });
            return NotFound();
        }

        private bool validity(Subscriber request){
            if(request==null)
                return false;
            request.EmailAddress = request.EmailAddress.Trim();

            return true;
        }

        private async Task<(Results result, string message)> validity(ChangeProfileImageRequest request){
            if(request==null)
                return (Results.Null, null);
            //if(!request.ImageUrl.IsEmpty())
            //    return (Results.Success, null);

            if(request.Img.IsEmpty())
                return (Results.Failed, "Please select an image.");

            byte[] bytes = Convert.FromBase64String(request.Img.Str());
            if(bytes.Length == 0)
                return (Results.Failed, "Make sure selected image is valid.");
            
            var res = await ImgService.SendAsync(bytes);
            bytes.Clear();
            if(res==null)
                return (Results.Failed, "Please contact to admin.");

            var json = JsonConvert.DeserializeObject<Dictionary<string,object>>(res);
            if(json["status"].Str()!="error"){
                request.ImageUrl = json["url"].Str();
                return (Results.Success, null);
            }
            return (Results.Failed, "Make sure selected image is valid.");
        }
    }
}