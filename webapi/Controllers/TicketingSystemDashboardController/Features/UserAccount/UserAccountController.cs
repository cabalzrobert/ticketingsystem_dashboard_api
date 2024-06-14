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
using webapi.App.Features.UserFeature;
using Newtonsoft.Json;

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
            var valresult = await validity(request);
            if (valresult.result == Results.Failed)
                return Ok(new { Status = "error", Message = valresult.message });
            if (valresult.result != Results.Success)
                return NotFound();

            var result = (request.UserAccountID.IsEmpty()) ? await _repo.SaveUserAccountAsyn(request) : await _repo.UpdateUserAccountAsyn(request);
            if (result.result == Results.Success)
                return Ok(new { Status = "ok", Message = result.message, Content = request });
            else if (result.result == Results.Failed)
                return Ok(new { Status = "error", Message = result.message });
            return NotFound();
        }

        private async Task<(Results result, string message)> validity(UserAccountModel request)
        {
            if (request == null)
                return (Results.Null, null);
            if (!request.ImageUrl.IsEmpty())
                return (Results.Success, null);

            if (request.ProfilePicture.IsEmpty())
                return (Results.Failed, "Please select an image.");
            if (request.ProfilePicture.StartsWith("http"))
            {
                request.ImageUrl = request.ProfilePicture;
                return (Results.Success, null);
            }
            else
            {
                var base64arr = request.ProfilePicture.Str().Split(',');
                //byte[] bytes = Convert.FromBase64String(request.ProfilePicture.Str().Trim());
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
                    request.ImageUrl = json["url"].Str();
                    //request.ImageUrl = (json["url"].Str()).Replace(_config["Portforwarding:LOCAL"].Str(), _config["Portforwarding:URL"].Str());
                    return (Results.Success, null);
                }
                return (Results.Null, "Make sure selected image is invalid");
            }
            
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
