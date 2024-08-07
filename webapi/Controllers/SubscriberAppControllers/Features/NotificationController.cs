using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using webapi.App.Aggregates.Common;
using webapi.App.Aggregates.SubscriberAppAggregate.Common;
using webapi.App.Aggregates.SubscriberAppAggregate.Features;
using webapi.App.RequestModel.Common;
using Comm.Commons.Extensions;

namespace webapi.Controllers.SubscriberAppControllers.Features
{
    [Route("app/v1/ticketingdashboard")]
    [ApiController]
    [ServiceFilter(typeof(SubscriberAuthenticationAttribute))]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationRepository _notifyRepo;
        public NotificationController(INotificationRepository notifyRepo){
            _notifyRepo = notifyRepo;
        }

        [HttpPost]
        [Route("notification")]
        public async Task<IActionResult> Task([FromBody] FilterRequest filter){
            if(!FilterRequest.validity0g(filter)) return NotFound();
            var repoResult = await _notifyRepo.NotificationAsync(filter);
            if(repoResult.result == Results.Success)
                return Ok(repoResult.items);
            return NotFound();
        }

        [HttpPost]
        [Route("notification/{NotificationID}/seen")]
        public async Task<IActionResult> Task(String NotificationID){
            if(NotificationID.IsEmpty()) return NotFound();
            try{ NotificationID = Convert.ToInt32(NotificationID).Str(); }
            catch{ return NotFound(); }
            var repoResult = await _notifyRepo.SeenAsync(NotificationID);
            if(repoResult.result == Results.Success)
                return Ok();
            return NotFound();
        }
        [HttpPost]
        [Route("notification/seen/all")]
        public async Task<IActionResult> ReadAll([FromBody] NotificationList request)
        {
            var result = await _notifyRepo.ReadAllAsync(request);
            if (result.result == Results.Success)
                return Ok();
            return NotFound();
        }

        [HttpPost]
        [Route("notification/unseen")]
        public async Task<IActionResult> Task(){
            var repoResult = await _notifyRepo.UnseenCountAsync();
            if(repoResult.result == Results.Success)
                return Ok(repoResult.count);
            return NotFound();
        }

        [HttpPost]
        [Route("ticketnotification/unseen")]
        public async Task<IActionResult> RequestUnseenCountAsync([FromBody] FilterRequest filter)
        {
            var repoResult = await _notifyRepo.RequestUnseenCountAsync(filter);
            if (repoResult.result == Results.Success)
                return Ok(repoResult.count);
            return NotFound();
        }

        [HttpPost]
        [Route("lasttransactionno")]
        public async Task<IActionResult> LastTransactionNo()
        {
            var repoResult = await _notifyRepo.LastTransactionNo();
            if (repoResult.result == Results.Success)
                return Ok(repoResult.count);
            return NotFound();
        }
    }
}