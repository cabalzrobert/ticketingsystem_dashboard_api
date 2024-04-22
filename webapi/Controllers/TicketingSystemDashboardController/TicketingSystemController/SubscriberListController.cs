using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using webapi.App.Aggregates.Common;
using webapi.App.Aggregates.TicketingSystemDashboard.SubscriberListRepository;

namespace webapi.Controllers.TicketingSystemDashboardController.TicketingSystemController
{
    [Route("app/v1/ticketingdashboard")]
    [ApiController]
    public class SubscriberListController:ControllerBase
    {
        private readonly ISubscriberListRepository _repo;
        private readonly IConfiguration _config;
        public SubscriberListController(IConfiguration config, ISubscriberListRepository repo)
        {
            _repo = repo;
            _config = config;
        }

        [HttpPost]
        [Route("dashboardticketing")]
        public async Task<IActionResult> Task0a()
        {
            var result = await _repo.LoadSubscriberListAsyn();
            if (result.result == Results.Success)
                return Ok(new { Result = "ok", Message = result.message, Content = result.subscriberlst });
            else if (result.result == Results.Failed)
                return Ok(new { Result = "error", Message = result.message, Content = result.subscriberlst });
            return NotFound();
        }
    }
}
