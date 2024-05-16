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
using webapi.App.Aggregates.TicketingSystemDashboard.Features.Roles;
using webapi.App.Model.User;
using webapi.App.RequestModel.Common;
using Comm.Commons.Extensions;

namespace webapi.Controllers.TicketingSystemDashboardController.Features.Roles
{
    [Route("app/v1/ticketingdashboard/roles")]
    [ApiController]
    [ServiceFilter(typeof(SubscriberAuthenticationAttribute))]
    public class RolesController:ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly IRolesRepository _repo;
        public RolesController(IConfiguration config, IRolesRepository repo)
        {
            _config = config;
            _repo = repo;
        }
        [HttpPost]
        [Route("save")]
        public async Task<IActionResult> TaskNew([FromBody] RolesModel request)
        {
            var result = (request.RolesID.IsEmpty()) ? await _repo.SaveRolesAsyn(request) : await _repo.UpdateRolesAsyn(request);
            if (result.result == Results.Success)
                return Ok(new { Status = "ok", Message = result.message, Content = request });
            else if (result.result == Results.Failed)
                return Ok(new { Status = "error", Message = result.message });
            return NotFound();
        }
        [HttpPost]
        [Route("list")]
        public async Task<IActionResult> TaskList([FromBody] FilterRequest request)
        {
            var result = await _repo.LoadRolesAsync(request);
            if (result.result == Results.Success)
                return Ok(new { Status = "ok", roles = result.roles });
            else if (result.result == Results.Failed)
                return Ok(new { Status = "error", roles = result.roles });
            return NotFound();
        }
    }
}
