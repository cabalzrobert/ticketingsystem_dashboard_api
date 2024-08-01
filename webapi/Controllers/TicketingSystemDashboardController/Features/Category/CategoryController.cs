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
using webapi.App.Aggregates.TicketingSystemDashboard.Features.Category;
using webapi.App.Model.User;
using webapi.App.RequestModel.Common;
using Comm.Commons.Extensions;

namespace webapi.Controllers.TicketingSystemDashboardController.Features.Category
{
    [Route("app/v1/ticketingdashboard/category")]
    [ApiController]
    [ServiceFilter(typeof(SubscriberAuthenticationAttribute))]
    public class CategoryController:ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly ICategoryRepository _repo;
        public CategoryController(IConfiguration config, ICategoryRepository repo)
        {
            _config = config;
            _repo = repo;
        }
        [HttpPost]
        [Route("save")]
        public async Task<IActionResult> TaskNew([FromBody] CategoryModel request)
        {
            var result = (request.CategoryID.IsEmpty()) ? await _repo.SaveCategoryAsyn(request) : await _repo.UpdateCategoryAsyn(request);
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
            var result = await _repo.LoadCategoryAsync(request);
            if (result.result == Results.Success)
                return Ok(new { Status = "ok", category = result.cat });
            else if (result.result == Results.Failed)
                return Ok(new { Status = "error", category = result.cat });
            return NotFound();
        }
        [HttpPost]
        [Route("listbydepartment")]
        public async Task<IActionResult> TaskListbyDepartment([FromBody] FilterRequest request)
        {
            var result = await _repo.LoadCategorybyDepartmentAsync(request);
            if (result.result == Results.Success)
                return Ok(new { Status = "ok", category = result.cat });
            else if (result.result == Results.Failed)
                return Ok(new { Status = "error", category = result.cat });
            return NotFound();
        }
    }
}
