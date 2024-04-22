using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using webapi.App.Aggregates.Common;
using webapi.App.Aggregates.SubscriberAppAggregate.Common;
using webapi.App.Aggregates.SubscriberAppAggregate.ActiveAccount;
using webapi.App.RequestModel.Common;

namespace webapi.Controllers.SubscriberAppControllers.ActiveAccount
{
    [Route("app/v1/stl/")]
    [ApiController]
    [ServiceFilter(typeof(SubscriberAuthenticationAttribute))]
    public class ActivityLogController : ControllerBase
    {
        private readonly IActivityLogRepository _logRepo;
        public ActivityLogController(IActivityLogRepository logRepo){
            _logRepo = logRepo;
        }


        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Task0d([FromBody] FilterRequest filter){
            //if(!FilterRequest.validity0a(filter)) return NotFound();
            //if(!FilterRequest.validity0g(filter)) return NotFound();
            //var repoResult = await _logRepo.LoginLogsAsync(filter);
            //if(repoResult.result == Results.Success)
            //    return Ok(repoResult.log);
            return NotFound();
        }
    }
}