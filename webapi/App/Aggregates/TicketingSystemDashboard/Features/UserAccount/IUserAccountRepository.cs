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

namespace webapi.App.Aggregates.TicketingSystemDashboard.Features.UserAccount
{
    [Service.ITransient(typeof(UserAccountRepository))]
    public interface IUserAccountRepository
    {
        Task<(Results result, String message)> SaveUserAccountAsyn(UserAccountModel request);
        Task<(Results result, String message)> UpdateUserAccountAsyn(UserAccountModel request);
        Task<(Results result, object useraccount)> LoadUserAccountAsync(FilterRequest req);
    }
    public class UserAccountRepository:IUserAccountRepository
    {
        private readonly ISubscriber _identity;
        private readonly IRepository _repo;
        public TicketingUser account { get { return _identity.AccountIdentity(); } }
        public UserAccountRepository(ISubscriber identity, IRepository repo)
        {
            _identity = identity;
            _repo = repo;
        }

        public async Task<(Results result, string message)> SaveUserAccountAsyn(UserAccountModel request)
        {
            var result = _repo.DSpQuery<dynamic>($"dbo.spfn_BDADBD0001", new Dictionary<string, object>()
            {
                {"parmplid", account.PL_ID},
                {"parmpgrpid", account.PGRP_ID},
                {"parmdepartmentid", request.DepartmentID},
                {"parmpositionid", request.PositionID},
                {"parmrolesid", request.RolesID},
                {"parmfirstname", request.Firstname},
                {"parmmidlename", request.Middlename},
                {"parmlastname", request.Lastname},
                {"parmgender", request.Gender},
                {"parmbirthdate", request.Birthdate},
                {"parmmobilenumber", request.MobileNumber},
                {"parmaddress", request.Address},
                {"parmprofilepictureURL", request.IMGURL},
                {"@parmusername", ""},
                {"parmcommunication", request.isCommunicator},
                {"parmDepartmenthead", request.isDeptartmentHead},
                {"parmuserid", account.USR_ID}
            }).FirstOrDefault();
            if (result != null)
            {
                var row = ((IDictionary<string, object>)result);
                request.UserAccountID = row["USR_ID"].Str();
                request.AccountID = row["ACT_ID"].Str();
                request.Name = row["FLL_NM"].Str();
                request.MobileNumber = row["MOB_NO"].Str();
                string ResultCode = row["RESULT"].Str();
                if (ResultCode == "1")
                {
                    return (Results.Success, "Successfully save.");
                }
                else if (ResultCode == "2")
                    return (Results.Failed, "Invalid Mobile Number, Try again");
                else if (ResultCode == "3")
                    return (Results.Failed, "Username already exist. Try again");
                else if (ResultCode == "4")
                    return (Results.Failed, "Already Exist. Try again");
                else if (ResultCode == "5")
                    return (Results.Failed, "Mobile Number already Exist. Try again");
                else if (ResultCode == "0")
                    return (Results.Failed, "Please check data. Try again");
            }
            return (Results.Null, null);
        }

        public async Task<(Results result, string message)> UpdateUserAccountAsyn(UserAccountModel request)
        {
            var result = _repo.DSpQuery<dynamic>($"dbo.spfn_BDADBD0003", new Dictionary<string, object>()
            {
                {"parmplid", account.PL_ID},
                {"parmpgrpid", account.PGRP_ID},
                {"parmaccountid", request.UserAccountID},
                {"parmdepartmentid", request.DepartmentID},
                {"parmpositionid", request.PositionID},
                {"parmrolesid", request.RolesID},
                {"parmfirstname", request.Firstname},
                {"parmmidlename", request.Middlename},
                {"parmlastname", request.Lastname},
                {"parmgender", request.Gender},
                {"parmbirthdate", request.Birthdate},
                {"parmmobilenumber", request.MobileNumber},
                {"parmaddress", request.Address},
                {"parmprofilepictureURL", request.ImageUrl},
                {"parmcommunication", request.isCommunicator},
                {"parmDepartmenthead", request.isDeptartmentHead},
                {"parmuserid", account.USR_ID}
            }).FirstOrDefault();
            if (result != null)
            {
                var row = ((IDictionary<string, object>)result);
                string ResultCode = row["RESULT"].Str();
                if (ResultCode == "1")
                {
                    request.UserAccountID = row["USR_ID"].Str();
                    request.MobileNumber = row["MOB_NO"].Str();
                    request.Name = row["FLL_NM"].Str();
                    return (Results.Success, "Successfully save.");
                }
                else if (ResultCode == "2")
                    return (Results.Success, "Invalid Mobile Number, Try again");
                else if (ResultCode == "4")
                    return (Results.Failed, "Already Exist. Try again");
                else if (ResultCode == "0")
                    return (Results.Failed, "Please check data. Try again");
            }
            return (Results.Null, null);
        }

        public async Task<(Results result, object useraccount)> LoadUserAccountAsync(FilterRequest req)
        {
            var results = _repo.DSpQuery<dynamic>($"dbo.spfn_BDADBD0002", new Dictionary<string, object>()
            {
                //{"parmplid",account.PL_ID },
                {"parmplid", account.PL_ID},
                {"parmpgrpid", account.PGRP_ID},
                {"parmrownum", req.num_row },
                {"parmsearch", req.Search }

            });
            if (results != null)
                return (Results.Success, TickectingSubscriberDto.GetUsserAccountList(results));
            return (Results.Null, null);
        }
    }
}
