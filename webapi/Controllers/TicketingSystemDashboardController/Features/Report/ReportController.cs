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
using webapi.App.Aggregates.TicketingSystemDashboard.Features.Report;
using webapi.App.Model.User;
using webapi.App.RequestModel.Common;
using Comm.Commons.Extensions;
using System.Text;
using webapi.App.Features.UserFeature;
using Newtonsoft.Json;

namespace webapi.Controllers.TicketingSystemDashboardController.Features.Report
{
    [Route("app/v1/ticketingdashboard/report")]
    [ApiController]
    [ServiceFilter(typeof(SubscriberAuthenticationAttribute))]
    public class ReportController:ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly IReportRepository _repo;
        public ReportController(IConfiguration config, IReportRepository repo)
        {
            _config = config;
            _repo = repo;
        }
        [HttpPost]
        [Route("ticketrequestperdepartment")]
        public async Task<IActionResult> TicketRequestPerDepartment()
        {
            var result = await _repo.LoadReportRequestperDepartmentAsync();
            if (result.result == Results.Success)
                return Ok(new { Status = "ok", report = result.rpt });
            else
            if (result.result == Results.Failed)
                return Ok(new { Status = "error", report = result.rpt });
            return NotFound();
        }
        [HttpPost]
        [Route("ticketrequestelapsedtime")]
        public async Task<IActionResult> TicketRequestElapsedTime([FromBody] FilterRequest param)
        {
            var result = await _repo.LoadReportTicketRequestperElapsedTimeAsync(param);
            if (result.result == Results.Success)
                return Ok(new { Status = "ok", report = result.rpt });
            else
            if (result.result == Results.Failed)
                return Ok(new { Status = "error", report = result.rpt });
            return NotFound();
        }
    }
}
