using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using webapi.Commons.AutoRegister;
using webapi.App.Aggregates.SubscriberAppAggregate.Common;
using webapi.App.Aggregates.Common;
using Infrastructure.Repositories;
using webapi.App.Model.User;
using Comm.Commons.Extensions;
using webapi.App.RequestModel.Common;
using webapi.App.Aggregates.Common.Dto;
using webapi.App.Features.UserFeature;

namespace webapi.App.Aggregates.TicketingSystemDashboard.Features.Overview
{
    [Service.ITransient(typeof(OverviewRepository))]
    public interface IOverviewRepository
    {
        Task<(Results result, object obj)> TaskOverviewAsync();
    }
    public class OverviewRepository:IOverviewRepository
    {
        private readonly ISubscriber _identity;
        private readonly IRepository _repo;
        public TicketingUser account { get { return _identity.AccountIdentity(); } }
        public OverviewRepository(ISubscriber identity, IRepository repo)
        {
            _identity = identity;
            _repo = repo;
        }

        public async Task<(Results result, object obj)> TaskOverviewAsync()
        {
            var results = _repo.DSpQuery<dynamic>($"dbo.spfn_AEARP0I", new Dictionary<string, object>()
            {
                {"parmplid", account.PL_ID},
                {"parmpgrpid", account.PGRP_ID},
                {"parmuserid", account.USR_ID},
            });
            if (results != null)
                return (Results.Success, results);
            return (Results.Null, null);
        }
    }
}
