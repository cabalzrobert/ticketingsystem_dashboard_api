using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using webapi.App.Aggregates.Common;
using webapi.App.Model.User;

using webapi.App.Aggregates.AppRecruiterAggregate.Common;
using webapi.App.Aggregates.AppRecruiterAggregate.Features;
using webapi.App.RequestModel.Common;

namespace webapi.Controllers.AppRecruiterControllers.Features
{
    [Route("app/v1/recruiter")]
    [ServiceFilter(typeof(RecruiterAuthenticationAttribute))]
    [ApiController]
    public class RequestDataController : ControllerBase
    {
        private readonly ISubscriberRepository _repo;
        public RequestDataController(IRecruiter identity, ISubscriberRepository repo){
            _repo = repo;
        }

        [HttpPost]
        [Route("subscribers")]
        public async Task<IActionResult> Task0d([FromBody] FilterRequest filter){
            if(!FilterRequest.validity0a(filter, false)) return NotFound();
            var repoResult = await _repo.SubscribersAsync(filter);
            if(repoResult.result == Results.Success)
                return Ok(repoResult.items);
            return NotFound();
        }
    }
}