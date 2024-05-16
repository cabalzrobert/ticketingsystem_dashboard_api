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
using webapi.App.Aggregates.TicketingSystemDashboard.Features.Position;
using webapi.App.Model.User;
using webapi.App.RequestModel.Common;
using Comm.Commons.Extensions;

namespace webapi.Controllers.TicketingSystemDashboardController.Features.Position
{
    [Route("app/v1/ticketingdashboard/position")]
    [ApiController]
    [ServiceFilter(typeof(SubscriberAuthenticationAttribute))]
    public class PositionController:ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly IPositionRepository _repo;
        public PositionController(IConfiguration config, IPositionRepository repo)
        {
            _config = config;
            _repo = repo;
        }

        [HttpPost]
        [Route("save")]
        public async Task<IActionResult> TaskNew([FromBody] PositionModel request)
        {
            var result = (request.PositionID.IsEmpty()) ? await _repo.SavePositionAsyn(request) : await _repo.UpdatePositionAsyn(request);
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
            var result = await _repo.LoadPositionAsync(request);
            if (result.result == Results.Success)
                return Ok(new { Status = "ok", position = result.pos });
            else if (result.result == Results.Failed)
                return Ok(new { Status = "error", position = result.pos });
            return NotFound();
        }
    }
}
