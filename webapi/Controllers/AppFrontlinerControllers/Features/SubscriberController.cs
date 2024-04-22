using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Comm.Commons.Extensions;
using Comm.Commons.Advance;
using webapi.App.Aggregates.Common;
using webapi.App.Model.User;
using System.Collections.Generic;

using webapi.App.Aggregates.AppFrontlinerAggregate.Features;
using webapi.App.RequestModel.Common;
using webapi.App.RequestModel.AppRecruiter;
using webapi.App.Features.UserFeature;
using System.IO;
using Newtonsoft.Json;

namespace webapi.Controllers.AppFrontlinerControllers.Features
{
    [Route("app/v1/frontliner")]
    [ApiController]
    public class SubscriberController : ControllerBase
    {
        private readonly ISubscriberRepository _repo;
        public SubscriberController(ISubscriberRepository repo){
            _repo = repo;
        }

        [HttpPost]
        [Route("subscriber/{AccountID}")]
        public async Task<IActionResult> Task0a(String AccountID){
            if(AccountID.IsEmpty()) return NotFound();
            var repoResult = await _repo.SearchAccountAsync(AccountID);
            if(repoResult.result == Results.Success)
                return Ok(repoResult.items);
            return NotFound();
        }
    }
}