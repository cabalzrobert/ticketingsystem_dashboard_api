using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using webapi.App.Aggregates.Common;
using webapi.App.Aggregates.TicketingSystemDashboard.Features;
using webapi.App.Aggregates.SubscriberAppAggregate.Common;
using webapi.App.TSDashboardModel;
using webapi.App.Aggregates.TicketingSystemDashboard.Features.Ticket;
using webapi.App.Model.User;
using webapi.App.RequestModel.Common;
using Comm.Commons.Extensions;
using System.Text;
using webapi.App.Features.UserFeature;
using Newtonsoft.Json;
using webapi.App.Aggregates.TicketingSystemDashboard.Features.Overview;

namespace webapi.Controllers.TicketingSystemDashboardController.Features.Overview
{
    [Route("app/v1/ticketingdashboard/overview")]
    [ApiController]
    [ServiceFilter(typeof(SubscriberAuthenticationAttribute))]
    public class OverviewController:ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly IOverviewRepository _repo;
        public OverviewController(IConfiguration config, IOverviewRepository repo)
        {
            _config = config;
            _repo = repo;
        }
        [HttpPost]
        [Route("count")]
        public async Task<IActionResult> OverviewCount()
        {
            var result = await _repo.TaskOverviewAsync();
            if (result.result == Results.Success)
                return Ok(new { Status = "ok", count = result.obj });
            else if (result.result == Results.Failed)
                return Ok(new { Status = "error", count = result.obj });
            return NotFound();
        }
    }
}
