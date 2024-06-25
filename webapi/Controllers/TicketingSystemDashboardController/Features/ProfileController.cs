using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using webapi.App.Aggregates.Common;
using webapi.App.Aggregates.SubscriberAppAggregate.Common;
using webapi.App.Aggregates.TicketingSystemDashboard.Features;
using webapi.App.Aggregates.TicketingSystemDashboard.HeadOfficeRepository;
using webapi.App.Model.User;
using webapi.App.RequestModel.Common;
using webapi.App.TSDashboardModel;

namespace webapi.Controllers.TicketingSystemDashboardController.TicketingSystemController.Features
{
    [Route("app/v1/ticketingdashboard/profile")]
    [ApiController]
    [ServiceFilter(typeof(SubscriberAuthenticationAttribute))]
    public class ProfileController: ControllerBase
    {
        private readonly IProfileRepository _repo;
        private readonly IConfiguration _config;
        public ProfileController(IConfiguration config, IProfileRepository repo)
        {
            _config = config;
            _repo = repo;
        }

        [HttpPost]
        [Route("update")]
        public async Task<IActionResult> ResolveTicket([FromBody] Profile profile)
        {
            var result = await _repo.UpdateProfile(profile);
            if (result.result == Results.Success)
                return Ok(new { Status = "ok", message = result.message });
            return BadRequest();

        }
    }
}
