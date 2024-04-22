using Comm.Commons.Extensions;
using Infrastructure.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using webapi.App.Aggregates.Common;
using webapi.App.Aggregates.Common.Dto;
using webapi.Commons.AutoRegister;

namespace webapi.App.Aggregates.TicketingSystemDashboard.SubscriberListRepository
{
    [Service.ITransient(typeof(SubscriberListRepository))]
    public interface ISubscriberListRepository
    {
        Task<(Results result, String message, object subscriberlst)> LoadSubscriberListAsyn();
    }

    public class SubscriberListRepository : ISubscriberListRepository
    {
        public readonly IRepository _repo;
        public SubscriberListRepository(IRepository repo)
        {
            _repo = repo;
        }

        public async Task<(Results result, string message, object subscriberlst)> LoadSubscriberListAsyn()
        {
            var results = _repo.DQuery<dynamic>($"dbo.spfn_CAA02 '8888'");
            if (results != null)
            {
                var row = ((IDictionary<string, object>)results.FirstOrDefault());
                string ResultCode = row["RESULT"].Str();
                if (ResultCode == "1")
                {
                    var stl = TickectingSubscriberDto.GetSubscriberList(results.FirstOrDefault());
                    return (Results.Success, "Exist", stl);
                }
                else if (ResultCode == "0")
                {
                    return (Results.Failed, "Not Exist", null);
                }
            }
            return (Results.Null, "Error", null);
        }
    }

}
