using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

using Comm.Commons.Extensions;
using Infrastructure.Repositories;
using webapi.Commons.AutoRegister;
using webapi.App.Aggregates.Common;
using webapi.App.Model.User;
using webapi.App.Globalize.Company;
using webapi.App.Aggregates.SubscriberAppAggregate.Common;
using webapi.App.RequestModel.Common;

namespace webapi.App.Aggregates.SubscriberAppAggregate.ActiveAccount
{
    [Service.ITransient(typeof(ActivityLogRepository))] 
    public interface IActivityLogRepository
    {
        Task<(Results result, object log)> LoginLogsAsync(FilterRequest filter);
    }

    public class ActivityLogRepository : IActivityLogRepository
    {
        private readonly ISubscriber _identity;
        private readonly IRepository _repo;
        public TicketingUser account { get{ return _identity.AccountIdentity(); } } 
        public ActivityLogRepository(ISubscriber identity, IRepository repo){
            _identity = identity; 
            _repo = repo; 
        }

        public async Task<(Results result, object log)> LoginLogsAsync(FilterRequest filter){
            var results = _repo.DSpQueryMultiple("dbo.spfn_BCB0A", new Dictionary<string, object>(){
                { "parmcompid", account.PL_ID },
                { "parmbrcd", account.PGRP_ID },
                { "parmuserid", account.USR_ID },
                { "parmftrns", filter.BaseFilter },
            });
            if(results != null)
                return (Results.Success, ActivityLogDto.FilterLoginLogs(results.Read<dynamic>(), 50)); 
            return (Results.Null, null); 
        }
    }
}