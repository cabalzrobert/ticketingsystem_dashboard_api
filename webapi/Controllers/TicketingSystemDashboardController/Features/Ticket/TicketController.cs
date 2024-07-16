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
using System.Text;
using webapi.App.Features.UserFeature;
using Newtonsoft.Json;

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
            var valresult = await validity(request);
            if (valresult.result == Results.Failed)
                return Ok(new { Status = "error", Message = valresult.message });
            if (valresult.result != Results.Success)
                return NotFound();
            var result = (request.TransactionNo.IsEmpty()) ? await _repo.SaveTicketAsync(request) : await _repo.UpdateTicketAsync(request);
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

        private async Task<(Results result, string message)> validity(TicketModel request)
        {
            if (request == null)
                return (Results.Null, null);

            if (request.TicketAttachment == null || request.TicketAttachment.Count < 1)
                return (Results.Success, null);
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < request.TicketAttachment.Count; i++)
            {
                var attachment = request.TicketAttachment[i].Str();
                if (attachment.IsEmpty()) continue;
                if (attachment.StartsWith("http"))
                {
                    request.ImageAttachment = attachment;
                    sb.Append($"<item LNK_URL=\"{attachment}\" />");
                }
                else
                {
                    var base64arr = attachment.Split(',');
                    //byte[] bytes = Convert.FromBase64String(attachment);
                    byte[] bytes = Convert.FromBase64String(base64arr[1]);
                    if (bytes.Length == 0)
                        return (Results.Failed, "Make sure selected image is valid.");

                    var res = await ImgService.SendAsync(bytes);
                    bytes.Clear();
                    if (res == null)
                        return (Results.Failed, "Please contact to admin.");

                    var json = JsonConvert.DeserializeObject<Dictionary<string, object>>(res);
                    if (json["status"].Str() != "error")
                    {
                        
                        string url = json["url"].Str();
                        var imageurl = url.Replace("https://119.93.89.82", "http://119.93.89.82:5000");
                        //string url = (json["url"].Str()).Replace(_config["Portforwarding:LOCAL"].Str(), _config["Portforwarding:URL"].Str());
                        sb.Append($"<item LNK_URL=\"{ imageurl }\" />");
                        request.TicketAttachment[i] = imageurl;
                    }
                    else return (Results.Failed, "Make sure selected image is valid.");
                }

            }
            if (sb.Length > 0)
            {
                request.iTicketAttachment = sb.ToString();
                return (Results.Success, null);
            }
            return (Results.Failed, "Make sure selected image is valid.");
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
        [Route("resolve")]
        public async Task<IActionResult> TaskResolve([FromBody] TicketRessolve request)
        {
            var result = await _repo.RessolvedAsync(request);
            if (result.result == Results.Success)
                return Ok(new { Status = "ok", Message = result.message });
            else if (result.result == Results.Failed)
                return Ok(new { Status = "error", Message = result.message });
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
            if(request.isImage == true)
            {
                var valresult = await attachmentvalidity(request);
                if (valresult.result == Results.Failed)
                    return Ok(new { Status = "error", Message = valresult.message });
                if (valresult.result != Results.Success)
                    return NotFound();
            }
            var result = await _repo.SendCommentAsyn(request);
            if (result.result == Results.Success)
            {
                //return Ok(new { Status = "ok", Message = result.message, Content = request });
                return Ok(new { Status = "ok", Message = result.message, Content = new { 
                      Branch_ID = request.Branch_ID
                    , CommentDate = request.CommentDate
                    , CommentID = request.CommentID
                    , Company_ID = request.Company_ID
                    , Department = ""
                    , DisplayName = request.DisplayName
                    , FileAttachment = ""
                    , ImageAttachment = request.ImageAttachment
                    , IsYou = true
                    , Message = request.Message
                    , ProfilePicture = request.ProfilePicture
                    , SenderID = request.SenderID
                    , TransactionNo = request.TransactionNo
                    , isFile = request.isFile
                    , isImage = request.isImage
                    , isMessage = true
                    , isRead = request.isRead } });
            }
            if (result.result == Results.Failed)
            {
                return Ok(new { Status = "error", Message = result.message });
            }
            return NotFound();
        }


        [HttpPost]
        [Route("isassigned")]
        public async Task<IActionResult> IsAssignedAsync(String transactionNo)
        {
            var repoResult = await _repo.IsAssignedAsync(transactionNo);
            if (repoResult.result == Results.Success)
                return Ok(repoResult.isassigned);
            return NotFound();
        }

        private async Task<(Results result, string message)> attachmentvalidity(TicketCommentModel request)
        {
            if (request == null)
                return (Results.Null, null);

            if (request.FileAttachment == null || request.FileAttachment.Count < 1)
                return (Results.Success, null);
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < request.FileAttachment.Count; i++)
            {
                var attachment = request.FileAttachment[i].Str();
                if (attachment.IsEmpty()) continue;
                if (attachment.StartsWith("http"))
                {
                    request.ImageAttachment = attachment;
                    sb.Append($"<item LNK_URL=\"{attachment}\" />");
                }
                else
                {
                    var base64arr = attachment.Split(',');
                    //byte[] bytes = Convert.FromBase64String(attachment);
                    byte[] bytes = Convert.FromBase64String(base64arr[1]);
                    if (bytes.Length == 0)
                        return (Results.Failed, "Make sure selected image is valid.");

                    var res = await ImgService.SendAsync(bytes);
                    bytes.Clear();
                    if (res == null)
                        return (Results.Failed, "Please contact to admin.");

                    var json = JsonConvert.DeserializeObject<Dictionary<string, object>>(res);
                    if (json["status"].Str() != "error")
                    {
                        string url = json["url"].Str().Replace("https://119.93.89.82", "http://119.93.89.82:5000");
                        request.ImageAttachment = url;
                        //string url = (json["url"].Str()).Replace(_config["Portforwarding:LOCAL"].Str(), _config["Portforwarding:URL"].Str());
                        sb.Append($"<item LNK_URL=\"{ url }\" />");
                        request.FileAttachment[i] = url;
                    }
                    else return (Results.Failed, "Make sure selected image is valid.");
                }

            }
            if (sb.Length > 0)
            {
                request.iFileAttachment = sb.ToString();
                return (Results.Success, null);
            }
            return (Results.Failed, "Make sure selected image is valid.");
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
