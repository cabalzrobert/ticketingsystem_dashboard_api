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

namespace webapi.Controllers.TicketingSystemDashboardController.Features.Ticket
{
    [Route("app/v1/ticketingdashboard/ticket")]
    [ApiController]
    [ServiceFilter(typeof(SubscriberAuthenticationAttribute))]
    public class TicketController:ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly ITicketRepository _repo;
        public TicketController(IConfiguration config, ITicketRepository repo)
        {
            _config = config;
            _repo = repo;
        }
        [HttpPost]
        [Route("save")]
        public async Task<IActionResult> TaskNew([FromBody] TicketModel request)
        {
            var result = (request.TransactionNo.IsEmpty()) ? await _repo.SaveTicketAsync(request) : await _repo.SaveTicketAsync(request);
            if(result.result == Results.Success)
            {
                return Ok(new { Status = "ok", Message = result.message, Content = request });
            }
            if(result.result == Results.Failed)
            {
                return Ok(new { Status = "error", Message = result.message });
            }
            return NotFound();
        }
        [HttpPost]
        [Route("list")]
        public async Task<IActionResult> TaskList([FromBody] FilterRequest request)
        {
            var result = await _repo.LoadPendingTicketAsync(request);
            if (result.result == Results.Success)
                return Ok(new { Status = "ok", ticket = result.ticket });
            else if (result.result == Results.Failed)
                return Ok(new { Status = "error", ticket = result.ticket });
            return NotFound();
        }

        [HttpPost]
        [Route("commentlist")]
        public async Task<IActionResult> TaskCommentList(string TransactionNo)
        {
            var result = await _repo.LoadTicketComment(TransactionNo);
            if (result.result == Results.Success)
                return Ok(new { Status = "ok", Comment = result.comment });
            else if (result.result == Results.Failed)
                return Ok(new { Status = "error", Comment = result.comment });
            return NotFound();
        }

        [HttpPost]
        [Route("msg/send")]
        public async Task<IActionResult> TaskMessageSend([FromBody] TicketCommentModel request)
        {
            //request.FileAttachment = "";
            var result = await _repo.SendCommentAsyn(request);
            if (result.result == Results.Success)
            {
                return Ok(new { Status = "ok", Message = result.message, Content = request });
            }
            if (result.result == Results.Failed)
            {
                return Ok(new { Status = "error", Message = result.message });
            }
            return NotFound();
        }


        [HttpPost]
        [Route("count")]
        public async Task<IActionResult> TaskTicketCount()
        {
            //request.FileAttachment = "";
            var result = await _repo.LoadCntTicketAsync();
            if (result.result == Results.Success)
            {
                return Ok(new { Status = "ok", TicketCount = result.cntticket });
            }
            if (result.result == Results.Failed)
            {
                return Ok(new { Status = "error", TicketCount = result.cntticket });
            }
            return NotFound();
        }

        [HttpPost]
        [Route("test/notify")]
        public async Task<IActionResult> TestNotify()
        {
            //request.FileAttachment = "";
            var result = await _repo.TestNotificationAsyn();
            if (result.result == Results.Success)
            {
                return Ok(new { Status = "ok", Message = result.message });
            }
            if (result.result == Results.Failed)
            {
                return Ok(new { Status = "error", Message = result.message });
            }
            return NotFound();
        }
        [HttpPost]
        [Route("communicator/{transactionNo}/seen")]
        public async Task<IActionResult> CommunicatorSeenTicket(String transactionNo)
        {
            if (transactionNo.IsEmpty()) return NotFound();
            var result = await _repo.SeenAsync(transactionNo);
            if (result.result == Results.Success)
                return Ok();
            return NotFound();
        }
    }
}
