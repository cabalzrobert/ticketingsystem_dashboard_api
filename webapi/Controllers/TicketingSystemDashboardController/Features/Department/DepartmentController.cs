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
using webapi.App.Aggregates.TicketingSystemDashboard.Features.Department;
using webapi.App.Model.User;
using webapi.App.RequestModel.Common;
using Comm.Commons.Extensions;

namespace webapi.Controllers.TicketingSystemDashboardController.Features.Department
{
    [Route("app/v1/ticketingdashboard/department")]
    [ApiController]
    [ServiceFilter(typeof(SubscriberAuthenticationAttribute))]
    public class DepartmentController:ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly IDepartmentRepository _repo;
        public DepartmentController(IConfiguration config, IDepartmentRepository repo)
        {
            _config = config;
            _repo = repo;
        }
        [HttpPost]
        [Route("save")]
        public async Task<IActionResult> TaskNew([FromBody] DepartmentModel request)
        {
            var result = (request.DepartmentID.IsEmpty()) ? await _repo.SaveDepartmentAsyn(request) : await _repo.UpdateDepartmentAsyn(request);
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
            var result = await _repo.LoadDepartmentAsync(request);
            if (result.result == Results.Success)
                return Ok(new { Status = "ok", department = result.dept });
            else if (result.result == Results.Failed)
                return Ok(new { Status = "error", department = result.dept });
            return NotFound();
        }
    }
}
