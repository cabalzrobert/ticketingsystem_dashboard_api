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
using webapi.App.Aggregates.TicketingSystemDashboard.Features.UserAccount;
using webapi.App.Model.User;
using webapi.App.RequestModel.Common;
using Comm.Commons.Extensions;

namespace webapi.Controllers.TicketingSystemDashboardController.Features.UserAccount
{
    [Route("app/v1/ticketingdashboard/useraccount")]
    [ApiController]
    [ServiceFilter(typeof(SubscriberAuthenticationAttribute))]
    public class UserAccountController:ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly IUserAccountRepository _repo;
        public UserAccountController(IConfiguration config, IUserAccountRepository repo)
        {
            _config = config;
            _repo = repo;
        }
        [HttpPost]
        [Route("save")]
        public async Task<IActionResult> TaskNew([FromBody] UserAccountModel request)
        {
            var result = (request.UserAccountID.IsEmpty()) ? await _repo.SaveUserAccountAsyn(request) : await _repo.UpdateUserAccountAsyn(request);
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
            var result = await _repo.LoadUserAccountAsync(request);
            if (result.result == Results.Success)
                return Ok(new { Status = "ok", useraccount = result.useraccount });
            else if (result.result == Results.Failed)
                return Ok(new { Status = "error", useraccount = result.useraccount });
            return NotFound();
        }
    }
}
