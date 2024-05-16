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

namespace webapi.App.Aggregates.TicketingSystemDashboard.Features.Position
{
    [Service.ITransient(typeof(PositionRepository))]
    public interface IPositionRepository
    {
        Task<(Results result, String message)> SavePositionAsyn(PositionModel request);
        Task<(Results result, String message)> UpdatePositionAsyn(PositionModel request);
        Task<(Results result, object pos)> LoadPositionAsync(FilterRequest req);
    }
    public class PositionRepository : IPositionRepository
    {
        private readonly ISubscriber _identity;
        private readonly IRepository _repo;
        public TicketingUser account { get { return _identity.AccountIdentity(); } }
        public PositionRepository(ISubscriber identiry, IRepository repo)
        {
            _identity = identiry;
            _repo = repo;
        }

        public async Task<(Results result, string message)> SavePositionAsyn(PositionModel request)
        {
            var result = _repo.DSpQuery<dynamic>($"dbo.spfn_LAF0001", new Dictionary<string, object>()
            {
                {"parmplid", account.PL_ID},
                {"parmpgrpid", account.PGRP_ID},
                {"parmpositionname", request.Positionname},
                {"parmuserid", account.USR_ID}
            }).FirstOrDefault();
            if (result != null)
            {
                var row = ((IDictionary<string, object>)result);
                request.PositionID = row["POSID"].Str();
                string ResultCode = row["RESULT"].Str();
                if (ResultCode == "1")
                {
                    return (Results.Success, "Successfully save.");
                }
                else if (ResultCode == "2")
                    return (Results.Success, "Already Exist");
                else if (ResultCode == "0")
                    return (Results.Failed, "Please check data. Try again");
            }
            return (Results.Null, null);
        }

        public async Task<(Results result, string message)> UpdatePositionAsyn(PositionModel request)
        {
            var result = _repo.DSpQuery<dynamic>($"dbo.spfn_LAF0003", new Dictionary<string, object>()
            {
                {"parmplid", account.PL_ID},
                {"parmpgrpid", account.PGRP_ID},
                {"parmpositionid", request.PositionID},
                {"parmpositionname", request.Positionname},
                {"parmuserid", account.USR_ID}
            }).FirstOrDefault();
            if (result != null)
            {
                var row = ((IDictionary<string, object>)result);
                //request.DepartmentID = row["DEPT_ID"].Str();
                string ResultCode = row["RESULT"].Str();
                if (ResultCode == "1")
                {
                    return (Results.Success, "Successfully save.");
                }
                else if (ResultCode == "2")
                    return (Results.Success, "Already Exist");
                else if (ResultCode == "0")
                    return (Results.Failed, "Please check data. Try again");
            }
            return (Results.Null, null);
        }

        public async Task<(Results result, object pos)> LoadPositionAsync(FilterRequest req)
        {
            var results = _repo.DSpQuery<dynamic>($"dbo.spfn_LAF0002", new Dictionary<string, object>()
            {
                //{"parmplid",account.PL_ID },
                {"parmplid", account.PL_ID},
                {"parmpgrpid", account.PGRP_ID},
                {"parmrownum", req.num_row },
                {"parmsearch", req.Search }

            });
            if (results != null)
                return (Results.Success, TickectingSubscriberDto.GetPositionList(results));
            return (Results.Null, null);
        }
    }
}
