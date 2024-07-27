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
using webapi.App.RequestModel.Common;
using webapi.App.TSDashboardModel;

namespace webapi.Controllers.TicketingSystemDashboardController.TicketingSystemController.Features
{
    [Route("app/v1/ticketingdashboard/head")]
    [ApiController]
    [ServiceFilter(typeof(SubscriberAuthenticationAttribute))]
    public class DepartmentHeadController: ControllerBase
    {
        private readonly IDepartmentHeadRepository _repo;
        private readonly IConfiguration _config;
        public DepartmentHeadController(IConfiguration config, IDepartmentHeadRepository repo)
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
        public async Task<IActionResult> GetTickets([FromBody] FilterTickets param)
        {
            var result = await _repo.GetTickets(param);
            if (result.result == Results.Success)
                return Ok(result.tickets);
            return BadRequest();
        }

        [HttpPost]
        [Route("tickets/sample")]
        public async Task<IActionResult> GetTickets(int row)
        {
            var result = await _repo.GetTickets(row);
            if (result.result == Results.Success)
                return Ok(result.tickets);
            return BadRequest();
        }

        [HttpPost]
        [Route("ticket/assign")]
        public async Task<IActionResult> AssignedTicket(TicketInfo ticket)
        {
            var result = await _repo.AssignedTicket(ticket);
            if (result.result == Results.Success)
                return Ok(new { Status = "ok", message = result.message });
            return BadRequest();

        }

        [HttpPost]
        [Route("ticket/return")]
        public async Task<IActionResult> ReturnTicket(TicketInfo ticket)
        {
            var result = await _repo.ReturnTicket(ticket);
            if (result.result == Results.Success)
                return Ok(new { Status = "ok", message = result.message });
            return BadRequest();

        }

        [HttpPost]
        [Route("personnels")]
        public async Task<IActionResult> LoadPersonnels(string departmentId)
        {
            var result = await _repo.LoadPersonnels(departmentId);
            if (result.result == Results.Success)
                return Ok(new { Status = "ok", personnels = result.personnels });
            else if (result.result == Results.Failed)
                return Ok(new { Status = "error", personnels = result.personnels });
            return NotFound();
        }

        [HttpPost]
        [Route("ticket/forward")]
        public async Task<IActionResult> ForwardTicket([FromBody] TicketInfo ticketInfo)
        {
            var result = await _repo.ForwardTicket(ticketInfo);
            if (result.result == Results.Success)
                return Ok(new { Status = "ok", message = result.message });
            return BadRequest();
            //Need WebSocket
        }

        [HttpPost]
        [Route("ticket/resolve")]
        public async Task<IActionResult> ResolveTicket(string ticketNo)
        {
            var result = await _repo.ResolveTicket(ticketNo);
            if (result.result == Results.Success)
                return Ok(new { Status = "ok", message = result.message });
            return BadRequest();
            //Need WebSocket
        }

        [HttpPost]
        [Route("ticket/hdresolve")]
        public async Task<IActionResult> HDResolveTicket(string ticketNo)
        {
            var result = await _repo.HDResolveTicket(ticketNo);
            if (result.result == Results.Success)
                return Ok(new { Status = "ok", message = result.message });
            return BadRequest();

        }

        [HttpPost]
        [Route("ticket/cancel")]
        public async Task<IActionResult> DeclineTicket(string ticketNo)
        {
            var result = await _repo.DeclineTicket(ticketNo);
            if (result.result == Results.Success)
                return Ok(new { Status = "ok", message = result.message });
            return BadRequest();
            //Need WebSocket
        }

        [HttpPost]
        [Route("ticket/dismiss")]
        public async Task<IActionResult> CancelTicket(string ticketNo)
        {
            var result = await _repo.CancelTicket(ticketNo);
            if (result.result == Results.Success)
                return Ok(new { Status = "ok", message = result.message });
            return BadRequest();
            //Need WebSocket
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
        [Route("ticket/count")]
        public async Task<IActionResult> TaskTicketCount(string departmentID)
        {
            //request.FileAttachment = "";
            var result = await _repo.LoadCntTicketAsync(departmentID);
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
        [Route("notification/unseen")]
        public async Task<IActionResult> DepartmentUnseenTicket(string departmentID)
        {
            var repoResult = await _repo.UnseenCountAssync(departmentID);
            if (repoResult.result == Results.Success)
                return Ok(repoResult.count);
            return NotFound();
        }
    }
}
