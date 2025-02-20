﻿using System;
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

namespace webapi.App.Aggregates.TicketingSystemDashboard.Features.UserAccess
{
    [Service.ITransient(typeof(UserAccessRepository))]
    public interface IUserAccessRepository
    { 
        Task<(Results result, String message)> SaveUerAccessAsyn(UserAccessModel request);
        Task<(Results result, object useraccess)> LoadUserAccessAsync(FilterRequest req);
        Task<(Results result, object useraccess)> GetUserAccessbyUserIDAsync();
    }
    public class UserAccessRepository:IUserAccessRepository
    {
        private readonly ISubscriber _identity;
        private readonly IRepository _repo;
        public TicketingUser account { get { return _identity.AccountIdentity(); } }
        public UserAccessRepository(ISubscriber identity, IRepository repo)
        {
            _identity = identity;
            _repo = repo;
        }

        public async Task<(Results result, object useraccess)> LoadUserAccessAsync(FilterRequest req)
        {
            var results = _repo.DSpQuery<dynamic>($"dbo.spfn_AAD002", new Dictionary<string, object>()
            {
                {"parmsearch", req.Search }

            });
            if (results != null)
                return (Results.Success, TickectingSubscriberDto.GetUserAccessList(results));
            return (Results.Null, null);
        }

        public async Task<(Results result, string message)> SaveUerAccessAsyn(UserAccessModel request)
        {
            var result = _repo.DSpQuery<dynamic>($"dbo.spfn_AAD001", new Dictionary<string, object>()
            {
                {"parmuseraccess", request.MenuTab},
                {"parmuserid", request.USR_ID}
            }).FirstOrDefault();
            if (result != null)
            {
                var row = ((IDictionary<string, object>)result);
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

        public async Task<(Results result, object useraccess)> GetUserAccessbyUserIDAsync()
        {
            var results = _repo.DSpQuery<dynamic>($"dbo.spfn_AAD003", new Dictionary<string, object>()
            {
                {"parmuserid", account.USR_ID }

            });
            if (results != null)
                return (Results.Success, TickectingSubscriberDto.GetUserAccessList(results));
            return (Results.Null, null);
        }
    }
}
