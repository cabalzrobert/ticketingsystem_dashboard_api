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
using webapi.App.TSDashboardModel;

namespace webapi.Controllers.TicketingSystemDashboardController.TicketingSystemController.Features
{
    [Route("app/v1/ticketingdashboard/communicator")]
    [ApiController]
    [ServiceFilter(typeof(SubscriberAuthenticationAttribute))]
    public class CommunicatorController:ControllerBase
    {
        private readonly ICommunicatorRepository _repo;
        private readonly IConfiguration _config;
        public CommunicatorController(IConfiguration config, ICommunicatorRepository repo)
        {
            _config = config;
            _repo = repo;
        }

        [HttpPost]
        [Route("ticket/add")]
        public async Task<IActionResult> CreateTicket(TicketInfo ticket)
        {
            var result = await _repo.CreateTicket(ticket);
            if(result.result == Results.Success)
                return Ok(new { Status = "ok", message = result.message });
            return BadRequest();

        }

        [HttpPost]
        [Route("tickets")]
        public async Task<IActionResult> GetTickets(int tab)
        {
            var result = await _repo.GetTickets(tab);
            if (result.result == Results.Success)
                return Ok(result.tickets);
            return BadRequest();
        }

        [HttpPost]
        [Route("ticket/forward")]
        public async Task<IActionResult> ForwardTicket(TicketInfo ticket)
        {
            var result = await _repo.ForwardTicket(ticket);
            if (result.result == Results.Success)
                return Ok(new { Status = "ok", message = result.message });
            return BadRequest();

        }

        [HttpPost]
        [Route("ticket/forward/permission")]
        public async Task<IActionResult> ConfirmationForwardTicket(TicketInfo ticket)
        {
            var result = await _repo.ConfirmationForwardTicket(ticket);
            if (result.result == Results.Success)
                return Ok(new { Status = "ok", message = result.message });
            return BadRequest();

        }

        [HttpPost]
        [Route("ticket/comments")]
        public async Task<IActionResult> GetComments(string transactionNo)
        {
            var result = await _repo.GetComments(transactionNo);
            if (result.result == Results.Success)
                return Ok(result.comments);
            return BadRequest();
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
    }
}
