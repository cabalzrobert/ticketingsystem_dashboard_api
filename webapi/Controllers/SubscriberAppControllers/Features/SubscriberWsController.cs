using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using webapi.App.Aggregates.Common;
using webapi.App.Model.User;

using webapi.App.Aggregates.SubscriberAppAggregate.Features;
using webapi.App.Aggregates.SubscriberAppAggregate.Common;

namespace webapi.Controllers.SubscriberAppControllers.Features
{
    [Route("app/v1/stl")]
    [ApiController]
    [ServiceFilter(typeof(SubscriberAuthenticationGETAttribute))]
    public class SubscriberWsController : ControllerBase
    {
        private readonly ISubscriber _identity;
        private readonly ISubscriberWs _ws;
        public TicketingUser account { get{ return _identity.AccountIdentity(); } }
        public SubscriberWsController(ISubscriber identity, ISubscriberWs ws){
            _identity = identity; 
            _ws = ws;
        }

        [HttpGet]
        [Route("ws")]
        public async Task<IActionResult> Task(){
            if(account==null) return NotFound();
            if (HttpContext.WebSockets.IsWebSocketRequest){
                var ws = await HttpContext.WebSockets.AcceptWebSocketAsync();
                await _ws.Echo(this, account, ws);
                return Ok();
            };
            return NotFound();
        }
        
    }
}