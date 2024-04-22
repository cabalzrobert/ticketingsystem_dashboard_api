using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Comm.Commons.Advance;
using webapi.App.Aggregates.Common;
using webapi.App.Model.User;

using webapi.App.Aggregates.AppRecruiterAggregate.Common;
using webapi.App.Aggregates.AppRecruiterAggregate.Features;
using webapi.App.Aggregates.AppRecruiterAggregate.ActiveAccount;
using webapi.App.RequestModel.Common;
using webapi.App.RequestModel.AppRecruiter;
using System.IO;
using Comm.Commons.Extensions;

namespace webapi.Controllers.AppRecruiterControllers.ActiveAccount
{
    [Route("app/v1/recruiter")]
    [ApiController]
    [ServiceFilter(typeof(RecruiterAuthenticationAttribute))]
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
    }
}