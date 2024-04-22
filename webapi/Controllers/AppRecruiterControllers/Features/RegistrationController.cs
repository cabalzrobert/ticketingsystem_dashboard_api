using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Comm.Commons.Extensions;
using Comm.Commons.Advance;
using webapi.App.Aggregates.Common;
using webapi.App.Model.User;
using System.Collections.Generic;

using webapi.App.Aggregates.AppRecruiterAggregate.Common;
using webapi.App.Aggregates.AppRecruiterAggregate.Features;
using webapi.App.RequestModel.Common;
using webapi.App.RequestModel.AppRecruiter;
using webapi.App.Features.UserFeature;
using System.IO;
using Newtonsoft.Json;

namespace webapi.Controllers.AppRecruiterControllers.Features
{
    [Route("app/v1/recruiter")]
    [ServiceFilter(typeof(RecruiterAuthenticationAttribute))]
    [ApiController]
    public class RegistrationController : ControllerBase
    {
        private readonly IRegistrationRepository _repo;
        public RegistrationController(IRecruiter identity, IRegistrationRepository repo){
            _repo = repo;
        }

        [HttpPost]
        [Route("customers")]
        public async Task<IActionResult> Task0a([FromBody] FilterRequest filter){
            if(!FilterRequest.validity0a(filter, false)) return NotFound();
            var repoResult = await _repo.PendingCustomersAsync(filter);
            if(repoResult.result == Results.Success)
                return Ok(repoResult.items);
            return NotFound();
        }
        [HttpPost]
        [Route("customer/new")]
        public async Task<IActionResult> Task0b([FromBody] RegistrationRequest request){
            var valResult = await validity(request);
            if(valResult.result == Results.Failed)
                return Ok(new { Status = "error", Message = valResult.message });
            if(valResult.result != Results.Success)
                return NotFound();

            var repoResult = await _repo.RegistrationAsync(request);
            if(repoResult.result == Results.Success)
                return Ok(new { Status = "ok", Message = repoResult.message, Content = request }); //, Data = repoResult.data  , IsCancelled = (!request.IsApproved)
            else if(repoResult.result == Results.Failed)
                return Ok(new { Status = "error", Message = repoResult.message });
            return NotFound();
        }
        [HttpPost]
        [Route("customer/edit")]
        public async Task<IActionResult> Task0c([FromBody] RegistrationRequest request){
            var valResult = await validity(request);
            if(valResult.result == Results.Failed)
                return Ok(new { Status = "error", Message = valResult.message });
            if(valResult.result != Results.Success)
                return NotFound();

            var repoResult = await _repo.RegistrationAsync(request, true);
            if(repoResult.result == Results.Success)
                return Ok(new { Status = "ok", Message = repoResult.message, Content = request }); //, Data = repoResult.data  , IsCancelled = (!request.IsApproved)
            else if(repoResult.result == Results.Failed)
                return Ok(new { Status = "error", Message = repoResult.message });
            return NotFound();
        }
        private async Task<(Results result, string message)> validity(RegistrationRequest request){
            if(request==null)
                return (Results.Null, null);
            if(!request.ImageUrl.IsEmpty())
                return (Results.Success, null);

            if(request.Img.IsEmpty())
                return (Results.Failed, "Please select an image.");

            byte[] bytes = Convert.FromBase64String(request.Img.Str());
            if(bytes.Length == 0)
                return (Results.Failed, "Make sure selected image is invalid.");
            
            var res = await ImgService.SendAsync(bytes);
            bytes.Clear();
            if(res==null)
                return (Results.Failed, "Please contact to admin.");

            var json = JsonConvert.DeserializeObject<Dictionary<string,object>>(res);
            if(json["status"].Str()!="error"){
                request.ImageUrl = json["url"].Str();
                return (Results.Success, null);
            }
            return (Results.Failed, "Make sure selected image is invalid.");
            /*byte[] bytes = Convert.FromBase64String(request.Img.Str());
            if(bytes.Length == 0)
                return false;
            try{
                var filename = ($"{ Cipher.MD5Hash(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")) }.png");
                var fullpath = ($"./wwwroot/files/img/{filename}");
                var url = ($"/files/img/{filename}"); //
                (new System.IO.FileInfo(fullpath).Directory).Create();
                using (BinaryWriter binWriter = new BinaryWriter(System.IO.File.Open(fullpath, FileMode.Create)))  
                    binWriter.Write(bytes, 0, bytes.Length);
                //
                request.ImageUrl = url;
                return true;
            }catch{}*/
            //return (Results.Null, null);
        }
    }
}