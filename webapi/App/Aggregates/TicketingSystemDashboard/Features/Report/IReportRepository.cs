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

namespace webapi.App.Aggregates.TicketingSystemDashboard.Features.Report
{
    [Service.ITransient(typeof(ReportRepository))]
    public interface IReportRepository
    {
        Task<(Results result, object rpt)> LoadReportRequestperDepartmentAsync();
        Task<(Results result, object rpt)> LoadReportTicketRequestperElapsedTimeAsync(FilterRequest req);

    }
    public class ReportRepository : IReportRepository
    {
        private readonly ISubscriber _identity;
        private readonly IRepository _repo;
        public TicketingUser account { get { return _identity.AccountIdentity(); } }
        public ReportRepository(ISubscriber identity, IRepository repo)
        {
            _identity = identity;
            _repo = repo;
        }
        public async Task<(Results result, object rpt)> LoadReportRequestperDepartmentAsync()
        {
            var results = _repo.DSpQuery<dynamic>($"dbo.spfn_RPT0001", new Dictionary<string, object>()
            {
                //{"parmplid",account.PL_ID },
                {"parmplid", account.PL_ID},
                {"parmpgrpid", account.PGRP_ID},

            });
            if (results != null)
                return (Results.Success, TickectingSubscriberDto.GetReportList(results));
            return (Results.Null, null);
        }

        public async Task<(Results result, object rpt)> LoadReportTicketRequestperElapsedTimeAsync(FilterRequest req)
        {
            var results = _repo.DSpQuery<dynamic>($"dbo.spfn_RPT0002", new Dictionary<string, object>()
            {
                //{"parmplid",account.PL_ID },
                {"parmplid", account.PL_ID},
                {"parmpgrpid", account.PGRP_ID},
                {"parmdatefrom", req.From},
                {"parmdateto", req.To},

            });
            if (results != null)
                return (Results.Success, TickectingSubscriberDto.GetTicketElapsedReportList(results));
            return (Results.Null, null);
        }
    }
}
