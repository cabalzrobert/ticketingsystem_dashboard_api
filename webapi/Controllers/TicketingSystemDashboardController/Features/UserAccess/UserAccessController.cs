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
using webapi.App.Aggregates.TicketingSystemDashboard.Features.UserAccess;
using webapi.App.Model.User;
using webapi.App.RequestModel.Common;
using Comm.Commons.Extensions;

namespace webapi.Controllers.TicketingSystemDashboardController.Features.UserAccess
{
    [Route("app/v1/ticketingdashboard/useraccess")]
    [ApiController]
    [ServiceFilter(typeof(SubscriberAuthenticationAttribute))]
    public class UserAccessController:ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly IUserAccessRepository _repo;
        public UserAccessController(IConfiguration config, IUserAccessRepository repo)
        {
            _config = config;
            _repo = repo;
        }
        [HttpPost]
        [Route("save")]
        public async Task<IActionResult> TaskNew([FromBody] UserAccessModel request)
        {
            var result = await _repo.SaveUerAccessAsyn(request);
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
            var result = await _repo.LoadUserAccessAsync(request);
            if (result.result == Results.Success)
                return Ok(new { Status = "ok", useraccess = result.useraccess });
            else if (result.result == Results.Failed)
                return Ok(new { Status = "error", useraccess = result.useraccess });
            return NotFound();
        }
        [HttpPost]
        [Route("getuseraccess")]
        public async Task<IActionResult> TaskGetUserAccess()
        {
            var result = await _repo.GetUserAccessbyUserIDAsync();
            if (result.result == Results.Success)
                return Ok(new { Status = "ok", useraccess = result.useraccess });
            else if (result.result == Results.Failed)
                return Ok(new { Status = "error", useraccess = result.useraccess });
            return NotFound();
        }
    }
}
