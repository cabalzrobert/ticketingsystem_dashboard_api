using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using webapi.App.Aggregates.SubscriberAppAggregate.Common;
using webapi.App.Aggregates.SubscriberAppAggregate.ActiveAccount;
using webapi.App.RequestModel.Common;

namespace webapi.Controllers.SubscriberAppControllers.ActiveAccount
{
    [Route("app/v1/stl/")]
    [ApiController]
    [ServiceFilter(typeof(SubscriberAuthenticationAttribute))]
    public class PingController : ControllerBase
    {
        private readonly IPingRepository _pingRepo;
        public PingController(IPingRepository pingRepo){
            _pingRepo = pingRepo;
        }

        [HttpPost]
        [Route("ping")]
        public async Task<IActionResult> Task(){
            //var repoResult = await _pingRepo.SelfCheckAsync();
            return Ok(new { Status = "ok" });
        }
    }
}